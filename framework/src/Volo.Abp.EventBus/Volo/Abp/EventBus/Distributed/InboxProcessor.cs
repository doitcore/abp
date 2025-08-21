using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Threading;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace Volo.Abp.EventBus.Distributed;

public class InboxProcessor : IInboxProcessor, ITransientDependency
{
    protected IServiceProvider ServiceProvider { get; }
    protected AbpAsyncTimer Timer { get; }
    protected IDistributedEventBus DistributedEventBus { get; }
    protected IAbpDistributedLock DistributedLock { get; }
    protected IUnitOfWorkManager UnitOfWorkManager { get; }
    protected IClock Clock { get; }
    protected IEventInbox Inbox { get; private set; } = default!;
    protected InboxConfig InboxConfig { get; private set; } = default!;
    protected AbpEventBusBoxesOptions EventBusBoxesOptions { get; }
    protected InboxProcessorOptions InboxProcessorOptions { get; set; }
    protected DateTime? LastCleanTime { get; set; }

    protected string DistributedLockName { get; set; } = default!;
    public ILogger<InboxProcessor> Logger { get; set; }
    protected CancellationTokenSource StoppingTokenSource { get; }
    protected CancellationToken StoppingToken { get; }

    public InboxProcessor(
        IServiceProvider serviceProvider,
        AbpAsyncTimer timer,
        IDistributedEventBus distributedEventBus,
        IAbpDistributedLock distributedLock,
        IUnitOfWorkManager unitOfWorkManager,
        IClock clock,
        IOptions<AbpEventBusBoxesOptions> eventBusBoxesOptions,
        IOptions<InboxProcessorOptions> inboxProcessorOptions)
    {
        ServiceProvider = serviceProvider;
        Timer = timer;
        DistributedEventBus = distributedEventBus;
        DistributedLock = distributedLock;
        UnitOfWorkManager = unitOfWorkManager;
        Clock = clock;
        EventBusBoxesOptions = eventBusBoxesOptions.Value;
        InboxProcessorOptions = inboxProcessorOptions.Value;
        Timer.Period = Convert.ToInt32(EventBusBoxesOptions.PeriodTimeSpan.TotalMilliseconds);
        Timer.Elapsed += TimerOnElapsed;
        Logger = NullLogger<InboxProcessor>.Instance;
        StoppingTokenSource = new CancellationTokenSource();
        StoppingToken = StoppingTokenSource.Token;
    }

    private async Task TimerOnElapsed(AbpAsyncTimer arg)
    {
        await RunAsync();
    }

    public virtual Task StartAsync(InboxConfig inboxConfig, CancellationToken cancellationToken = default)
    {
        InboxConfig = inboxConfig;
        Inbox = (IEventInbox)ServiceProvider.GetRequiredService(inboxConfig.ImplementationType);
        DistributedLockName = $"AbpInbox_{InboxConfig.DatabaseName}";
        Timer.Start(cancellationToken);
        return Task.CompletedTask;
    }

    public virtual Task StopAsync(CancellationToken cancellationToken = default)
    {
        StoppingTokenSource.Cancel();
        Timer.Stop(cancellationToken);
        StoppingTokenSource.Dispose();
        return Task.CompletedTask;
    }

    protected virtual async Task RunAsync()
    {
        if (StoppingToken.IsCancellationRequested)
        {
            return;
        }

        await using (var handle = await DistributedLock.TryAcquireAsync(DistributedLockName, cancellationToken: StoppingToken))
        {
            if (handle != null)
            {
                await DeleteOldEventsAsync();

                while (true)
                {
                    var waitingEvents = await GetWaitingEventsAsync();
                    if (waitingEvents.Count <= 0)
                    {
                        break;
                    }

                    Logger.LogInformation($"Found {waitingEvents.Count} events in the inbox.");

                    foreach (var waitingEvent in waitingEvents)
                    {
                        if (waitingEvent.NextRetryTime.HasValue && waitingEvent.NextRetryTime.Value > Clock.Now)
                        {
                            Logger.LogInformation($"Event with id = {waitingEvent.Id:N} is not ready to be processed yet. Next retry time: {waitingEvent.NextRetryTime.Value}");
                            continue;
                        }

                        try
                        {
                            using (var uow = UnitOfWorkManager.Begin(isTransactional: true, requiresNew: true))
                            {
                                await DistributedEventBus
                                    .AsSupportsEventBoxes()
                                    .ProcessFromInboxAsync(waitingEvent, InboxConfig);

                                await Inbox.MarkAsProcessedAsync(waitingEvent.Id);

                                await uow.CompleteAsync(StoppingToken);
                            }

                            Logger.LogInformation($"Processed the incoming event with id = {waitingEvent.Id:N}");
                        }
                        catch (Exception e)
                        {
                            Logger.LogError(e, $"An error occurred while processing the incoming event with id = {waitingEvent.Id:N}");

                            if (InboxProcessorOptions.FailurePolicy == InboxProcessorFailurePolicy.Retry)
                            {
                                throw;
                            }

                            if (InboxProcessorOptions.FailurePolicy == InboxProcessorFailurePolicy.RetryLater)
                            {
                                using (var uow = UnitOfWorkManager.Begin(isTransactional: true, requiresNew: true))
                                {
                                    if (waitingEvent.RetryCount >= InboxProcessorOptions.MaxRetryCount)
                                    {
                                        Logger.LogWarning($"Max retry count reached for event with id = {waitingEvent.Id:N}. Discarding the event.");

                                        await Inbox.MarkAsDiscardAsync(waitingEvent.Id);
                                        await uow.CompleteAsync(StoppingToken);
                                        continue;
                                    }

                                    Logger.LogInformation($"Retrying event with id = {waitingEvent.Id:N}. " +
                                                          $"Retry count: {waitingEvent.RetryCount}, " +
                                                          $"Next retry time: {GetNextRetryTime(waitingEvent.RetryCount, InboxProcessorOptions.MaxRetryCount)}");

                                    await Inbox.RetryLaterAsync(waitingEvent.Id, waitingEvent.RetryCount++, GetNextRetryTime(waitingEvent.RetryCount, InboxProcessorOptions.MaxRetryCount));
                                    await uow.CompleteAsync(StoppingToken);
                                }
                                continue;
                            }

                            if (InboxProcessorOptions.FailurePolicy == InboxProcessorFailurePolicy.Discard)
                            {
                                using (var uow = UnitOfWorkManager.Begin(isTransactional: true, requiresNew: true))
                                {
                                    Logger.LogInformation($"Discarding event with id = {waitingEvent.Id:N} due to an error.");
                                    await Inbox.MarkAsDiscardAsync(waitingEvent.Id);
                                    await uow.CompleteAsync(StoppingToken);
                                }
                                continue;
                            }
                        }
                    }
                }
            }
            else
            {
                Logger.LogDebug("Could not obtain the distributed lock: " + DistributedLockName);
                try
                {
                    await Task.Delay(EventBusBoxesOptions.DistributedLockWaitDuration, StoppingToken);
                }
                catch (TaskCanceledException) { }
            }
        }
    }

    protected virtual DateTime? GetNextRetryTime(int retryCount, int maxRetryCount)
    {
        if (retryCount > maxRetryCount)
        {
            return null;
        }

        var delaySeconds = 1 * (int)Math.Pow(2, retryCount - 1);

        return DateTime.Now.AddSeconds(delaySeconds);
    }

    protected virtual async Task<List<IncomingEventInfo>> GetWaitingEventsAsync()
    {
        return await Inbox.GetWaitingEventsAsync(EventBusBoxesOptions.InboxWaitingEventMaxCount, EventBusBoxesOptions.InboxProcessorFilter, StoppingToken);
    }

    protected virtual async Task DeleteOldEventsAsync()
    {
        if (LastCleanTime != null && LastCleanTime + EventBusBoxesOptions.CleanOldEventTimeIntervalSpan > Clock.Now)
        {
            return;
        }

        await Inbox.DeleteOldEventsAsync();

        LastCleanTime = Clock.Now;
    }
}

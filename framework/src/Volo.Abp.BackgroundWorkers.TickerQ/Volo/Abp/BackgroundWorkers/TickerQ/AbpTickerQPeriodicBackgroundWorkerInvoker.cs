using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using TickerQ.Utilities.Models;

namespace Volo.Abp.BackgroundWorkers.TickerQ;

//TODO: Use lambda expression to improve performance.
public class AbpTickerQPeriodicBackgroundWorkerInvoker
{
    private readonly MethodInfo _doWorkAsyncMethod;
    private readonly MethodInfo _doWorkMethod;

    protected IBackgroundWorker Worker { get; }
    protected IServiceProvider ServiceProvider { get; }

    public AbpTickerQPeriodicBackgroundWorkerInvoker(IBackgroundWorker worker, IServiceProvider serviceProvider)
    {
        _doWorkAsyncMethod = worker.GetType().GetMethod("DoWorkAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;
        _doWorkMethod = worker.GetType().GetMethod("DoWork", BindingFlags.Instance | BindingFlags.NonPublic)!;

        Worker = worker;
        ServiceProvider = serviceProvider;
    }

    public virtual async Task DoWorkAsync(TickerFunctionContext context, CancellationToken cancellationToken = default)
    {
        var workerContext = new PeriodicBackgroundWorkerContext(ServiceProvider);
        switch (Worker)
        {
            case AsyncPeriodicBackgroundWorkerBase asyncPeriodicBackgroundWorker:
                await (Task)(_doWorkAsyncMethod.Invoke(asyncPeriodicBackgroundWorker, new object[] { workerContext })!);
                break;
            case PeriodicBackgroundWorkerBase periodicBackgroundWorker:
                _doWorkMethod.Invoke(periodicBackgroundWorker, new object[] { workerContext });
                break;
        }
    }
}

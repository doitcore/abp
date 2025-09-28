using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Volo.Abp.BackgroundWorkers.TickerQ.Samples;

/// <summary>
/// Sample TickerQ background worker that demonstrates basic usage.
/// This worker runs every 5 minutes and performs a simple logging operation.
/// </summary>
public class SampleTickerQWorker : TickerQBackgroundWorkerBase
{
    public SampleTickerQWorker()
    {
        // Configure the worker
        JobId = nameof(SampleTickerQWorker);
        CronExpression = "0 */5 * ? * *"; // Every 5 minutes
        Priority = 1; // Higher than default priority
        MaxRetryAttempts = 5; // Retry up to 5 times on failure
    }

    public override Task DoWorkAsync(CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Sample TickerQ worker executed at {Time}", DateTime.Now);
        
        // Simulate some work
        Logger.LogDebug("Processing sample work...");
        
        // Your background task logic goes here
        // For example:
        // - Process pending orders
        // - Send scheduled notifications
        // - Cleanup old data
        // - Generate reports
        
        Logger.LogInformation("Sample TickerQ worker completed successfully");
        
        return Task.CompletedTask;
    }
}

/// <summary>
/// Sample TickerQ background worker that demonstrates error handling and retry logic.
/// </summary>
public class SampleErrorHandlingTickerQWorker : TickerQBackgroundWorkerBase
{
    private static int _executionCount = 0;

    public SampleErrorHandlingTickerQWorker()
    {
        JobId = nameof(SampleErrorHandlingTickerQWorker);
        CronExpression = "0 */10 * ? * *"; // Every 10 minutes
        MaxRetryAttempts = 3;
    }

    public override async Task DoWorkAsync(CancellationToken cancellationToken = default)
    {
        var currentCount = Interlocked.Increment(ref _executionCount);
        
        Logger.LogInformation("Error handling worker executed {Count} times", currentCount);
        
        // Simulate intermittent failures for demonstration
        if (currentCount % 3 == 0)
        {
            Logger.LogWarning("Simulating a temporary failure (will retry)");
            throw new InvalidOperationException("Simulated failure for demonstration");
        }
        
        // Simulate successful work
        await Task.Delay(100, cancellationToken); // Simulate some async work
        
        Logger.LogInformation("Error handling worker completed successfully");
    }
}

/// <summary>
/// Sample TickerQ background worker that demonstrates working with dependency injection.
/// </summary>
public class SampleDependencyInjectionTickerQWorker : TickerQBackgroundWorkerBase
{
    // In a real application, you would inject your repositories, services, etc.
    // private readonly IMyRepository _myRepository;
    // private readonly IMyService _myService;

    public SampleDependencyInjectionTickerQWorker(
        // IMyRepository myRepository,
        // IMyService myService
        )
    {
        // _myRepository = myRepository;
        // _myService = myService;
        
        JobId = nameof(SampleDependencyInjectionTickerQWorker);
        CronExpression = "0 0 */6 ? * *"; // Every 6 hours
    }

    public override async Task DoWorkAsync(CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Dependency injection worker started");
        
        try
        {
            // Example of using injected services
            // var entities = await _myRepository.GetListAsync(cancellationToken: cancellationToken);
            // await _myService.ProcessEntitiesAsync(entities, cancellationToken);
            
            // For demonstration, just simulate the work
            await Task.Delay(50, cancellationToken);
            
            Logger.LogInformation("Dependency injection worker processed data successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error occurred in dependency injection worker");
            throw; // Re-throw to trigger retry logic
        }
    }
}
using System.Threading;
using System.Threading.Tasks;

namespace Volo.Abp.BackgroundWorkers.TickerQ;

/// <summary>
/// Base class for TickerQ background workers.
/// TickerQ is a fast, reflection-free background task scheduler for .NET.
/// </summary>
public abstract class TickerQBackgroundWorkerBase : BackgroundWorkerBase, ITickerQBackgroundWorker
{
    /// <summary>
    /// Gets or sets the cron expression for the job scheduling.
    /// </summary>
    public string? CronExpression { get; set; }

    /// <summary>
    /// Gets or sets the job identifier.
    /// </summary>
    public string? JobId { get; set; }

    /// <summary>
    /// Gets or sets whether to automatically register this worker.
    /// Default is true.
    /// </summary>
    public bool AutoRegister { get; set; } = true;

    /// <summary>
    /// Gets or sets the job priority.
    /// Default is 0 (normal priority).
    /// </summary>
    public int Priority { get; set; } = 0;

    /// <summary>
    /// Gets or sets the maximum retry attempts for failed jobs.
    /// Default is 3.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// The main work execution method that must be implemented by derived classes.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the work execution.</returns>
    public abstract Task DoWorkAsync(CancellationToken cancellationToken = default);
}
using System.Threading;
using System.Threading.Tasks;

namespace Volo.Abp.BackgroundWorkers.TickerQ;

/// <summary>
/// Interface for TickerQ background workers.
/// TickerQ is a fast, reflection-free background task scheduler for .NET.
/// </summary>
public interface ITickerQBackgroundWorker : IBackgroundWorker
{
    /// <summary>
    /// Gets or sets the cron expression for the job scheduling.
    /// </summary>
    string? CronExpression { get; set; }

    /// <summary>
    /// Gets or sets the job identifier.
    /// </summary>
    string? JobId { get; set; }

    /// <summary>
    /// Gets or sets whether to automatically register this worker.
    /// Default is true.
    /// </summary>
    bool AutoRegister { get; set; }

    /// <summary>
    /// Gets or sets the job priority.
    /// </summary>
    int Priority { get; set; }

    /// <summary>
    /// Gets or sets the maximum retry attempts for failed jobs.
    /// </summary>
    int MaxRetryAttempts { get; set; }

    /// <summary>
    /// The main work execution method.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the work execution.</returns>
    Task DoWorkAsync(CancellationToken cancellationToken = default);
}
namespace Volo.Abp.BackgroundWorkers.TickerQ;

/// <summary>
/// Options for TickerQ background workers.
/// </summary>
public class AbpBackgroundWorkerTickerQOptions
{
    /// <summary>
    /// Gets or sets whether automatic registration is enabled for TickerQ workers.
    /// Default is true.
    /// </summary>
    public bool IsAutoRegisterEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the default cron expression for workers that don't specify one.
    /// Default is every minute: "0 * * ? * *"
    /// </summary>
    public string DefaultCronExpression { get; set; } = "0 * * ? * *";

    /// <summary>
    /// Gets or sets the default maximum retry attempts.
    /// Default is 3.
    /// </summary>
    public int DefaultMaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets the default priority for workers.
    /// Default is 0 (normal priority).
    /// </summary>
    public int DefaultPriority { get; set; } = 0;
}
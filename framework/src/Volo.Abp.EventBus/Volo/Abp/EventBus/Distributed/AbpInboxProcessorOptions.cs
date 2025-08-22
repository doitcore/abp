namespace Volo.Abp.EventBus.Distributed;

public class AbpInboxProcessorOptions
{
    public InboxProcessorFailurePolicy FailurePolicy { get; set; } = InboxProcessorFailurePolicy.Retry;

    /// <summary>
    /// Retry intervals follow an exponential backoff strategy:
    /// The delay for each retry is twice the previous one,
    /// starting with an initial delay of 1 second.
    /// For example: 1s, 2s, 4s, 8s, 16s, 32s ...
    /// Maximum of 10 retries.
    /// </summary>
    public int MaxRetryCount { get; set; } = 10;
}

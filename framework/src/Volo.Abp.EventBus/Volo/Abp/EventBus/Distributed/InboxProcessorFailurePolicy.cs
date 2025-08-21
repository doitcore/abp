namespace Volo.Abp.EventBus.Distributed;

public enum InboxProcessorFailurePolicy
{
    /// <summary>
    /// Default behavior, retry the following event in next period time.
    /// </summary>
    Retry,

    /// <summary>
    /// Skip and retry the event in next period time, but with a delay.
    /// The delay increases in every fail, and it is discarded after a specified amount of time
    /// (e.g. 1 second, 2 seconds, 4 seconds, 8 seconds, etc.),
    /// </summary>
    RetryLater,

    /// <summary>
    /// Skip the event and do not retry it.
    /// </summary>
    Discard,
}

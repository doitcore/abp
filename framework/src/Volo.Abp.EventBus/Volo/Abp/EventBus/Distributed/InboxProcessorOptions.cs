namespace Volo.Abp.EventBus.Distributed;

public class InboxProcessorOptions
{
    public InboxProcessorFailurePolicy FailurePolicy { get; set; } = InboxProcessorFailurePolicy.Retry;

    public int MaxRetryCount { get; set; } = 10;
}

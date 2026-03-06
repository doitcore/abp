using System;
using Volo.Abp.ExceptionHandling;

namespace Volo.Abp.OperationRateLimit;

public class AbpOperationRateLimitException : BusinessException, IHasHttpStatusCode
{
    public string PolicyName { get; }

    public OperationRateLimitResult Result { get; }

    public int HttpStatusCode => 429;

    public AbpOperationRateLimitException(
        string policyName,
        OperationRateLimitResult result,
        string? errorCode = null)
        : base(code: errorCode ?? AbpOperationRateLimitErrorCodes.ExceedLimit)
    {
        PolicyName = policyName;
        Result = result;

        WithData("PolicyName", policyName);
        WithData("MaxCount", result.MaxCount);
        WithData("CurrentCount", result.CurrentCount);
        WithData("RemainingCount", result.RemainingCount);
        WithData("RetryAfterSeconds", (int)(result.RetryAfter?.TotalSeconds ?? 0));
        WithData("RetryAfterMinutes", (int)(result.RetryAfter?.TotalMinutes ?? 0));
        WithData("WindowDurationSeconds", (int)result.WindowDuration.TotalSeconds);
    }

    internal void SetRetryAfterFormatted(string formattedRetryAfter)
    {
        WithData("RetryAfter", formattedRetryAfter);
    }

    internal void SetWindowDescriptionFormatted(string formattedWindowDescription)
    {
        WithData("WindowDescription", formattedWindowDescription);
    }
}

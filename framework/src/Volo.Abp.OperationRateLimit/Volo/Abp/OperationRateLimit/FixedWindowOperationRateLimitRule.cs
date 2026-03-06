using System.Threading.Tasks;
using Volo.Abp.AspNetCore.WebClientInfo;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;

namespace Volo.Abp.OperationRateLimit;

public class FixedWindowOperationRateLimitRule : IOperationRateLimitRule
{
    private const string HostTenantKey = "host";

    protected string PolicyName { get; }
    protected int RuleIndex { get; }
    protected OperationRateLimitRuleDefinition Definition { get; }
    protected IOperationRateLimitStore Store { get; }
    protected ICurrentUser CurrentUser { get; }
    protected ICurrentTenant CurrentTenant { get; }
    protected IWebClientInfoProvider WebClientInfoProvider { get; }

    public FixedWindowOperationRateLimitRule(
        string policyName,
        int ruleIndex,
        OperationRateLimitRuleDefinition definition,
        IOperationRateLimitStore store,
        ICurrentUser currentUser,
        ICurrentTenant currentTenant,
        IWebClientInfoProvider webClientInfoProvider)
    {
        PolicyName = policyName;
        RuleIndex = ruleIndex;
        Definition = definition;
        Store = store;
        CurrentUser = currentUser;
        CurrentTenant = currentTenant;
        WebClientInfoProvider = webClientInfoProvider;
    }

    public virtual async Task<OperationRateLimitRuleResult> AcquireAsync(
        OperationRateLimitContext context)
    {
        var partitionKey = ResolvePartitionKey(context);
        var storeKey = BuildStoreKey(partitionKey);
        var storeResult = await Store.IncrementAsync(storeKey, Definition.Duration, Definition.MaxCount);

        return ToRuleResult(storeResult);
    }

    public virtual async Task<OperationRateLimitRuleResult> CheckAsync(
        OperationRateLimitContext context)
    {
        var partitionKey = ResolvePartitionKey(context);
        var storeKey = BuildStoreKey(partitionKey);
        var storeResult = await Store.GetAsync(storeKey, Definition.Duration, Definition.MaxCount);

        return ToRuleResult(storeResult);
    }

    public virtual async Task ResetAsync(OperationRateLimitContext context)
    {
        var partitionKey = ResolvePartitionKey(context);
        var storeKey = BuildStoreKey(partitionKey);
        await Store.ResetAsync(storeKey);
    }

    protected virtual string ResolvePartitionKey(OperationRateLimitContext context)
    {
        return Definition.PartitionType switch
        {
            OperationRateLimitPartitionType.Parameter =>
                context.Parameter ?? throw new AbpException(
                    $"OperationRateLimitContext.Parameter is required for policy '{PolicyName}' (PartitionByParameter)."),

            OperationRateLimitPartitionType.CurrentUser =>
                CurrentUser.Id?.ToString() ?? throw new AbpException(
                    $"Current user is not authenticated. Policy '{PolicyName}' requires PartitionByCurrentUser."),

            OperationRateLimitPartitionType.CurrentTenant =>
                CurrentTenant.Id?.ToString() ?? HostTenantKey,

            OperationRateLimitPartitionType.ClientIp =>
                WebClientInfoProvider.ClientIpAddress
                ?? throw new AbpException(
                    $"Client IP address could not be determined. Policy '{PolicyName}' requires PartitionByClientIp. " +
                    "Ensure IWebClientInfoProvider is properly configured."),

            OperationRateLimitPartitionType.Email =>
                context.Parameter
                ?? CurrentUser.Email
                ?? throw new AbpException(
                    $"Email is required for policy '{PolicyName}' (PartitionByEmail). Provide it via context.Parameter or ensure the user has an email."),

            OperationRateLimitPartitionType.PhoneNumber =>
                context.Parameter
                ?? CurrentUser.PhoneNumber
                ?? throw new AbpException(
                    $"Phone number is required for policy '{PolicyName}' (PartitionByPhoneNumber). Provide it via context.Parameter or ensure the user has a phone number."),

            OperationRateLimitPartitionType.Custom =>
                Definition.CustomPartitionKeyResolver!(context),

            _ => throw new AbpException($"Unknown partition type: {Definition.PartitionType}")
        };
    }

    protected virtual string BuildStoreKey(string partitionKey)
    {
        // Stable rule descriptor based on content so reordering rules does not change the key.
        // Changing Duration or MaxCount intentionally resets counters for that rule.
        var ruleKey = $"{(long)Definition.Duration.TotalSeconds}_{Definition.MaxCount}_{(int)Definition.PartitionType}";

        // Tenant isolation is opt-in via WithMultiTenancy() on the rule builder.
        // When not set, the key is global (shared across all tenants).
        if (!Definition.IsMultiTenant)
        {
            return $"orl:{PolicyName}:{ruleKey}:{partitionKey}";
        }

        var tenantId = CurrentTenant.Id.HasValue ? CurrentTenant.Id.Value.ToString() : HostTenantKey;
        return $"orl:t:{tenantId}:{PolicyName}:{ruleKey}:{partitionKey}";
    }

    protected virtual OperationRateLimitRuleResult ToRuleResult(OperationRateLimitStoreResult storeResult)
    {
        return new OperationRateLimitRuleResult
        {
            RuleName = $"{PolicyName}:Rule[{(long)Definition.Duration.TotalSeconds}s,{Definition.MaxCount},{Definition.PartitionType}]",
            IsAllowed = storeResult.IsAllowed,
            RemainingCount = storeResult.MaxCount - storeResult.CurrentCount,
            MaxCount = storeResult.MaxCount,
            RetryAfter = storeResult.RetryAfter,
            WindowDuration = Definition.Duration
        };
    }
}

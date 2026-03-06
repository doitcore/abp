using System;

namespace Volo.Abp.OperationRateLimit;

public class OperationRateLimitRuleBuilder
{
    private readonly OperationRateLimitPolicyBuilder? _policyBuilder;
    private TimeSpan _duration;
    private int _maxCount;
    private OperationRateLimitPartitionType? _partitionType;
    private Func<OperationRateLimitContext, string>? _customPartitionKeyResolver;
    private bool _isMultiTenant;

    public OperationRateLimitRuleBuilder()
    {
    }

    internal OperationRateLimitRuleBuilder(OperationRateLimitPolicyBuilder policyBuilder)
    {
        _policyBuilder = policyBuilder;
    }

    public OperationRateLimitRuleBuilder WithFixedWindow(
        TimeSpan duration, int maxCount)
    {
        _duration = duration;
        _maxCount = maxCount;
        return this;
    }

    public OperationRateLimitRuleBuilder WithMultiTenancy()
    {
        _isMultiTenant = true;
        return this;
    }

    /// <summary>
    /// Use context.Parameter as partition key.
    /// </summary>
    public OperationRateLimitPolicyBuilder PartitionByParameter()
    {
        _partitionType = OperationRateLimitPartitionType.Parameter;
        CommitToPolicyBuilder();
        return _policyBuilder!;
    }

    /// <summary>
    /// Auto resolve from ICurrentUser.Id.
    /// </summary>
    public OperationRateLimitPolicyBuilder PartitionByCurrentUser()
    {
        _partitionType = OperationRateLimitPartitionType.CurrentUser;
        CommitToPolicyBuilder();
        return _policyBuilder!;
    }

    /// <summary>
    /// Auto resolve from ICurrentTenant.Id.
    /// </summary>
    public OperationRateLimitPolicyBuilder PartitionByCurrentTenant()
    {
        _partitionType = OperationRateLimitPartitionType.CurrentTenant;
        CommitToPolicyBuilder();
        return _policyBuilder!;
    }

    /// <summary>
    /// Auto resolve from IClientIpAddressProvider.ClientIpAddress.
    /// </summary>
    public OperationRateLimitPolicyBuilder PartitionByClientIp()
    {
        _partitionType = OperationRateLimitPartitionType.ClientIp;
        CommitToPolicyBuilder();
        return _policyBuilder!;
    }

    /// <summary>
    /// Partition by email address.
    /// Resolves from context.Parameter, falls back to ICurrentUser.Email.
    /// </summary>
    public OperationRateLimitPolicyBuilder PartitionByEmail()
    {
        _partitionType = OperationRateLimitPartitionType.Email;
        CommitToPolicyBuilder();
        return _policyBuilder!;
    }

    /// <summary>
    /// Partition by phone number.
    /// Resolves from context.Parameter, falls back to ICurrentUser.PhoneNumber.
    /// </summary>
    public OperationRateLimitPolicyBuilder PartitionByPhoneNumber()
    {
        _partitionType = OperationRateLimitPartitionType.PhoneNumber;
        CommitToPolicyBuilder();
        return _policyBuilder!;
    }

    /// <summary>
    /// Custom partition key resolver from context.
    /// </summary>
    public OperationRateLimitPolicyBuilder PartitionBy(
        Func<OperationRateLimitContext, string> keyResolver)
    {
        _partitionType = OperationRateLimitPartitionType.Custom;
        _customPartitionKeyResolver = Check.NotNull(keyResolver, nameof(keyResolver));
        CommitToPolicyBuilder();
        return _policyBuilder!;
    }

    protected virtual void CommitToPolicyBuilder()
    {
        _policyBuilder?.AddRuleDefinition(Build());
    }

    internal OperationRateLimitRuleDefinition Build()
    {
        if (_duration <= TimeSpan.Zero)
        {
            throw new AbpException(
                "Operation rate limit rule requires a positive duration. " +
                "Call WithFixedWindow(duration, maxCount) before building the rule.");
        }

        if (_maxCount < 0)
        {
            throw new AbpException(
                "Operation rate limit rule requires maxCount >= 0. " +
                "Use maxCount: 0 to completely deny all requests (ban policy).");
        }

        if (!_partitionType.HasValue)
        {
            throw new AbpException(
                "Operation rate limit rule requires a partition type. " +
                "Call PartitionByParameter(), PartitionByCurrentUser(), PartitionByClientIp(), or another PartitionBy*() method.");
        }

        if (_partitionType == OperationRateLimitPartitionType.Custom && _customPartitionKeyResolver == null)
        {
            throw new AbpException(
                "Custom partition type requires a key resolver. " +
                "Call PartitionBy(keyResolver) instead of setting partition type directly.");
        }

        return new OperationRateLimitRuleDefinition
        {
            Duration = _duration,
            MaxCount = _maxCount,
            PartitionType = _partitionType.Value,
            CustomPartitionKeyResolver = _customPartitionKeyResolver,
            IsMultiTenant = _isMultiTenant
        };
    }
}

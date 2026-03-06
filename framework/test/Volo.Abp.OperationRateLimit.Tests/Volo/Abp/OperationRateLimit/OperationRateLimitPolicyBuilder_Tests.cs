using System;
using Shouldly;
using Xunit;

namespace Volo.Abp.OperationRateLimit;

public class OperationRateLimitPolicyBuilder_Tests
{
    [Fact]
    public void Should_Build_Simple_Policy()
    {
        var options = new AbpOperationRateLimitOptions();
        options.AddPolicy("TestPolicy", policy =>
        {
            policy.WithFixedWindow(TimeSpan.FromHours(1), maxCount: 5)
                  .PartitionByParameter();
        });

        var policy = options.Policies["TestPolicy"];

        policy.Name.ShouldBe("TestPolicy");
        policy.Rules.Count.ShouldBe(1);
        policy.Rules[0].Duration.ShouldBe(TimeSpan.FromHours(1));
        policy.Rules[0].MaxCount.ShouldBe(5);
        policy.Rules[0].PartitionType.ShouldBe(OperationRateLimitPartitionType.Parameter);
        policy.ErrorCode.ShouldBeNull();
    }

    [Fact]
    public void Should_Build_Composite_Policy()
    {
        var options = new AbpOperationRateLimitOptions();
        options.AddPolicy("CompositePolicy", policy =>
        {
            policy.AddRule(rule => rule
                .WithFixedWindow(TimeSpan.FromHours(1), maxCount: 3)
                .PartitionByParameter());

            policy.AddRule(rule => rule
                .WithFixedWindow(TimeSpan.FromDays(1), maxCount: 10)
                .PartitionByCurrentUser());
        });

        var policy = options.Policies["CompositePolicy"];

        policy.Name.ShouldBe("CompositePolicy");
        policy.Rules.Count.ShouldBe(2);
        policy.Rules[0].PartitionType.ShouldBe(OperationRateLimitPartitionType.Parameter);
        policy.Rules[0].MaxCount.ShouldBe(3);
        policy.Rules[1].PartitionType.ShouldBe(OperationRateLimitPartitionType.CurrentUser);
        policy.Rules[1].MaxCount.ShouldBe(10);
    }

    [Fact]
    public void Should_Set_ErrorCode()
    {
        var options = new AbpOperationRateLimitOptions();
        options.AddPolicy("ErrorPolicy", policy =>
        {
            policy.WithFixedWindow(TimeSpan.FromHours(1), maxCount: 2)
                  .PartitionByParameter()
                  .WithErrorCode("App:Custom:Error");
        });

        var policy = options.Policies["ErrorPolicy"];
        policy.ErrorCode.ShouldBe("App:Custom:Error");
    }

    [Fact]
    public void Should_Build_Custom_Partition()
    {
        var options = new AbpOperationRateLimitOptions();
        options.AddPolicy("CustomPolicy", policy =>
        {
            policy.AddRule(rule => rule
                .WithFixedWindow(TimeSpan.FromMinutes(30), maxCount: 5)
                .PartitionBy(ctx => $"custom:{ctx.Parameter}"));
        });

        var policy = options.Policies["CustomPolicy"];

        policy.Rules.Count.ShouldBe(1);
        policy.Rules[0].PartitionType.ShouldBe(OperationRateLimitPartitionType.Custom);
        policy.Rules[0].CustomPartitionKeyResolver.ShouldNotBeNull();
    }

    [Fact]
    public void Should_Support_All_Partition_Types()
    {
        var options = new AbpOperationRateLimitOptions();

        options.AddPolicy("P1", p => p.WithFixedWindow(TimeSpan.FromHours(1), 1).PartitionByParameter());
        options.AddPolicy("P2", p => p.WithFixedWindow(TimeSpan.FromHours(1), 1).PartitionByCurrentUser());
        options.AddPolicy("P3", p => p.WithFixedWindow(TimeSpan.FromHours(1), 1).PartitionByCurrentTenant());
        options.AddPolicy("P4", p => p.WithFixedWindow(TimeSpan.FromHours(1), 1).PartitionByClientIp());
        options.AddPolicy("P5", p => p.WithFixedWindow(TimeSpan.FromHours(1), 1).PartitionByEmail());
        options.AddPolicy("P6", p => p.WithFixedWindow(TimeSpan.FromHours(1), 1).PartitionByPhoneNumber());

        options.Policies["P1"].Rules[0].PartitionType.ShouldBe(OperationRateLimitPartitionType.Parameter);
        options.Policies["P2"].Rules[0].PartitionType.ShouldBe(OperationRateLimitPartitionType.CurrentUser);
        options.Policies["P3"].Rules[0].PartitionType.ShouldBe(OperationRateLimitPartitionType.CurrentTenant);
        options.Policies["P4"].Rules[0].PartitionType.ShouldBe(OperationRateLimitPartitionType.ClientIp);
        options.Policies["P5"].Rules[0].PartitionType.ShouldBe(OperationRateLimitPartitionType.Email);
        options.Policies["P6"].Rules[0].PartitionType.ShouldBe(OperationRateLimitPartitionType.PhoneNumber);
    }

    [Fact]
    public void Should_Throw_When_Policy_Has_No_Rules()
    {
        var options = new AbpOperationRateLimitOptions();

        var exception = Assert.Throws<AbpException>(() =>
        {
            options.AddPolicy("EmptyPolicy", policy =>
            {
                // Intentionally not adding any rules
            });
        });

        exception.Message.ShouldContain("no rules");
    }

    [Fact]
    public void Should_Throw_When_WithFixedWindow_Without_PartitionBy()
    {
        var options = new AbpOperationRateLimitOptions();

        var exception = Assert.Throws<AbpException>(() =>
        {
            options.AddPolicy("IncompletePolicy", policy =>
            {
                policy.WithFixedWindow(TimeSpan.FromHours(1), maxCount: 5);
                // Missing PartitionBy*() call - rule never committed
            });
        });

        exception.Message.ShouldContain("no rules");
    }

    [Fact]
    public void Should_Throw_When_AddRule_Without_WithFixedWindow()
    {
        var options = new AbpOperationRateLimitOptions();

        var exception = Assert.Throws<AbpException>(() =>
        {
            options.AddPolicy("NoWindowPolicy", policy =>
            {
                policy.AddRule(rule =>
                {
                    // Missing WithFixedWindow call - duration is zero
                });
            });
        });

        exception.Message.ShouldContain("positive duration");
    }

    [Fact]
    public void Should_Allow_MaxCount_Zero_For_Ban_Policy()
    {
        var options = new AbpOperationRateLimitOptions();

        // maxCount=0 is a valid "ban" policy - always deny
        options.AddPolicy("BanPolicy", policy =>
        {
            policy.WithFixedWindow(TimeSpan.FromHours(1), maxCount: 0)
                  .PartitionByParameter();
        });

        var policy = options.Policies["BanPolicy"];
        policy.Rules[0].MaxCount.ShouldBe(0);
    }

    [Fact]
    public void Should_Throw_When_AddRule_Without_PartitionBy()
    {
        var options = new AbpOperationRateLimitOptions();

        var exception = Assert.Throws<AbpException>(() =>
        {
            options.AddPolicy("NoPartitionPolicy", policy =>
            {
                policy.AddRule(rule => rule
                    .WithFixedWindow(TimeSpan.FromHours(1), maxCount: 5));
                // Missing PartitionBy*() call
            });
        });

        exception.Message.ShouldContain("partition type");
    }

    [Fact]
    public void Should_Throw_When_MaxCount_Is_Negative()
    {
        var options = new AbpOperationRateLimitOptions();

        var exception = Assert.Throws<AbpException>(() =>
        {
            options.AddPolicy("NegativePolicy", policy =>
            {
                policy.WithFixedWindow(TimeSpan.FromHours(1), maxCount: -1)
                      .PartitionByParameter();
            });
        });

        exception.Message.ShouldContain("maxCount >= 0");
    }
}

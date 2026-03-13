using System;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Volo.Abp.OperationRateLimiting;

public class OperationRateLimitingPolicyBuilder_Tests
{
    [Fact]
    public void Should_Build_Simple_Policy()
    {
        var options = new AbpOperationRateLimitingOptions();
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
        policy.Rules[0].PartitionType.ShouldBe(OperationRateLimitingPartitionType.Parameter);
        policy.ErrorCode.ShouldBeNull();
    }

    [Fact]
    public void Should_Build_Composite_Policy()
    {
        var options = new AbpOperationRateLimitingOptions();
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
        policy.Rules[0].PartitionType.ShouldBe(OperationRateLimitingPartitionType.Parameter);
        policy.Rules[0].MaxCount.ShouldBe(3);
        policy.Rules[1].PartitionType.ShouldBe(OperationRateLimitingPartitionType.CurrentUser);
        policy.Rules[1].MaxCount.ShouldBe(10);
    }

    [Fact]
    public void Should_Set_ErrorCode()
    {
        var options = new AbpOperationRateLimitingOptions();
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
        var options = new AbpOperationRateLimitingOptions();
        options.AddPolicy("CustomPolicy", policy =>
        {
            policy.AddRule(rule => rule
                .WithFixedWindow(TimeSpan.FromMinutes(30), maxCount: 5)
                .PartitionBy(ctx => Task.FromResult($"custom:{ctx.Parameter}")));
        });

        var policy = options.Policies["CustomPolicy"];

        policy.Rules.Count.ShouldBe(1);
        policy.Rules[0].PartitionType.ShouldBe(OperationRateLimitingPartitionType.Custom);
        policy.Rules[0].CustomPartitionKeyResolver.ShouldNotBeNull();
    }

    [Fact]
    public void Should_Support_All_Partition_Types()
    {
        var options = new AbpOperationRateLimitingOptions();

        options.AddPolicy("P1", p => p.WithFixedWindow(TimeSpan.FromHours(1), 1).PartitionByParameter());
        options.AddPolicy("P2", p => p.WithFixedWindow(TimeSpan.FromHours(1), 1).PartitionByCurrentUser());
        options.AddPolicy("P3", p => p.WithFixedWindow(TimeSpan.FromHours(1), 1).PartitionByCurrentTenant());
        options.AddPolicy("P4", p => p.WithFixedWindow(TimeSpan.FromHours(1), 1).PartitionByClientIp());
        options.AddPolicy("P5", p => p.WithFixedWindow(TimeSpan.FromHours(1), 1).PartitionByEmail());
        options.AddPolicy("P6", p => p.WithFixedWindow(TimeSpan.FromHours(1), 1).PartitionByPhoneNumber());

        options.Policies["P1"].Rules[0].PartitionType.ShouldBe(OperationRateLimitingPartitionType.Parameter);
        options.Policies["P2"].Rules[0].PartitionType.ShouldBe(OperationRateLimitingPartitionType.CurrentUser);
        options.Policies["P3"].Rules[0].PartitionType.ShouldBe(OperationRateLimitingPartitionType.CurrentTenant);
        options.Policies["P4"].Rules[0].PartitionType.ShouldBe(OperationRateLimitingPartitionType.ClientIp);
        options.Policies["P5"].Rules[0].PartitionType.ShouldBe(OperationRateLimitingPartitionType.Email);
        options.Policies["P6"].Rules[0].PartitionType.ShouldBe(OperationRateLimitingPartitionType.PhoneNumber);
    }

    [Fact]
    public void Should_Throw_When_Policy_Has_No_Rules()
    {
        var options = new AbpOperationRateLimitingOptions();

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
        var options = new AbpOperationRateLimitingOptions();

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
        var options = new AbpOperationRateLimitingOptions();

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
        var options = new AbpOperationRateLimitingOptions();

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
        var options = new AbpOperationRateLimitingOptions();

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
        var options = new AbpOperationRateLimitingOptions();

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

    [Fact]
    public void Should_Allow_Same_Rule_With_Different_MultiTenancy()
    {
        var options = new AbpOperationRateLimitingOptions();

        // Same Duration/MaxCount/PartitionType but different IsMultiTenant should be allowed
        options.AddPolicy("MultiTenancyPolicy", policy =>
        {
            policy.AddRule(rule => rule
                .WithFixedWindow(TimeSpan.FromHours(1), maxCount: 5)
                .PartitionByParameter());

            policy.AddRule(rule => rule
                .WithFixedWindow(TimeSpan.FromHours(1), maxCount: 5)
                .WithMultiTenancy()
                .PartitionByParameter());
        });

        var policy = options.Policies["MultiTenancyPolicy"];
        policy.Rules.Count.ShouldBe(2);
        policy.Rules[0].IsMultiTenant.ShouldBeFalse();
        policy.Rules[1].IsMultiTenant.ShouldBeTrue();
    }

    [Fact]
    public void Should_Allow_Multiple_Custom_Partition_Rules()
    {
        var options = new AbpOperationRateLimitingOptions();

        // Multiple custom partition rules with same Duration/MaxCount should be allowed
        // because they may use different key resolvers
        options.AddPolicy("MultiCustomPolicy", policy =>
        {
            policy.AddRule(rule => rule
                .WithFixedWindow(TimeSpan.FromHours(1), maxCount: 5)
                .PartitionBy(ctx => Task.FromResult($"by-ip:{ctx.Parameter}")));

            policy.AddRule(rule => rule
                .WithFixedWindow(TimeSpan.FromHours(1), maxCount: 5)
                .PartitionBy(ctx => Task.FromResult($"by-device:{ctx.Parameter}")));
        });

        var policy = options.Policies["MultiCustomPolicy"];
        policy.Rules.Count.ShouldBe(2);
        policy.Rules[0].PartitionType.ShouldBe(OperationRateLimitingPartitionType.Custom);
        policy.Rules[1].PartitionType.ShouldBe(OperationRateLimitingPartitionType.Custom);
    }

    [Fact]
    public void AddPolicy_With_Same_Name_Should_Replace_Existing_Policy()
    {
        var options = new AbpOperationRateLimitingOptions();

        options.AddPolicy("MyPolicy", policy =>
        {
            policy.WithFixedWindow(TimeSpan.FromHours(1), maxCount: 5)
                  .PartitionByParameter();
        });

        // Second AddPolicy with the same name replaces the first one entirely
        options.AddPolicy("MyPolicy", policy =>
        {
            policy.WithFixedWindow(TimeSpan.FromMinutes(10), maxCount: 2)
                  .PartitionByCurrentUser();
        });

        options.Policies.Count.ShouldBe(1);

        var policy = options.Policies["MyPolicy"];
        policy.Rules.Count.ShouldBe(1);
        policy.Rules[0].Duration.ShouldBe(TimeSpan.FromMinutes(10));
        policy.Rules[0].MaxCount.ShouldBe(2);
        policy.Rules[0].PartitionType.ShouldBe(OperationRateLimitingPartitionType.CurrentUser);
    }

    [Fact]
    public void ConfigurePolicy_Should_Override_ErrorCode_While_Keeping_Rules()
    {
        var options = new AbpOperationRateLimitingOptions();

        options.AddPolicy("BasePolicy", policy =>
        {
            policy.WithFixedWindow(TimeSpan.FromHours(1), maxCount: 5)
                  .PartitionByParameter();
        });

        options.ConfigurePolicy("BasePolicy", policy =>
        {
            policy.WithErrorCode("App:Custom:Override");
        });

        var result = options.Policies["BasePolicy"];
        result.ErrorCode.ShouldBe("App:Custom:Override");
        result.Rules.Count.ShouldBe(1);
        result.Rules[0].MaxCount.ShouldBe(5);
        result.Rules[0].PartitionType.ShouldBe(OperationRateLimitingPartitionType.Parameter);
    }

    [Fact]
    public void ConfigurePolicy_Should_Add_Additional_Rule_To_Existing_Policy()
    {
        var options = new AbpOperationRateLimitingOptions();

        options.AddPolicy("BasePolicy", policy =>
        {
            policy.WithFixedWindow(TimeSpan.FromMinutes(5), maxCount: 3)
                  .PartitionByParameter();
        });

        options.ConfigurePolicy("BasePolicy", policy =>
        {
            policy.AddRule(rule => rule
                .WithFixedWindow(TimeSpan.FromHours(1), maxCount: 20)
                .PartitionByClientIp());
        });

        var result = options.Policies["BasePolicy"];
        result.Rules.Count.ShouldBe(2);
        result.Rules[0].Duration.ShouldBe(TimeSpan.FromMinutes(5));
        result.Rules[0].MaxCount.ShouldBe(3);
        result.Rules[0].PartitionType.ShouldBe(OperationRateLimitingPartitionType.Parameter);
        result.Rules[1].Duration.ShouldBe(TimeSpan.FromHours(1));
        result.Rules[1].MaxCount.ShouldBe(20);
        result.Rules[1].PartitionType.ShouldBe(OperationRateLimitingPartitionType.ClientIp);
    }

    [Fact]
    public void ConfigurePolicy_ClearRules_Should_Replace_All_Rules()
    {
        var options = new AbpOperationRateLimitingOptions();

        options.AddPolicy("BasePolicy", policy =>
        {
            policy.AddRule(rule => rule
                .WithFixedWindow(TimeSpan.FromHours(1), maxCount: 10)
                .PartitionByParameter());

            policy.AddRule(rule => rule
                .WithFixedWindow(TimeSpan.FromDays(1), maxCount: 50)
                .PartitionByCurrentUser());
        });

        options.ConfigurePolicy("BasePolicy", policy =>
        {
            policy.ClearRules()
                  .WithFixedWindow(TimeSpan.FromMinutes(5), maxCount: 3)
                  .PartitionByEmail();
        });

        var result = options.Policies["BasePolicy"];
        result.Rules.Count.ShouldBe(1);
        result.Rules[0].Duration.ShouldBe(TimeSpan.FromMinutes(5));
        result.Rules[0].MaxCount.ShouldBe(3);
        result.Rules[0].PartitionType.ShouldBe(OperationRateLimitingPartitionType.Email);
    }

    [Fact]
    public void ConfigurePolicy_Should_Support_Chaining()
    {
        var options = new AbpOperationRateLimitingOptions();

        options.AddPolicy("PolicyA", policy =>
        {
            policy.WithFixedWindow(TimeSpan.FromHours(1), maxCount: 5)
                  .PartitionByParameter();
        });

        options.AddPolicy("PolicyB", policy =>
        {
            policy.WithFixedWindow(TimeSpan.FromHours(1), maxCount: 10)
                  .PartitionByCurrentUser();
        });

        // ConfigurePolicy returns AbpOperationRateLimitingOptions for chaining
        options
            .ConfigurePolicy("PolicyA", policy => policy.WithErrorCode("App:LimitA"))
            .ConfigurePolicy("PolicyB", policy => policy.WithErrorCode("App:LimitB"));

        options.Policies["PolicyA"].ErrorCode.ShouldBe("App:LimitA");
        options.Policies["PolicyB"].ErrorCode.ShouldBe("App:LimitB");
    }

    [Fact]
    public void ConfigurePolicy_Should_Throw_When_Policy_Not_Found()
    {
        var options = new AbpOperationRateLimitingOptions();

        var exception = Assert.Throws<AbpException>(() =>
        {
            options.ConfigurePolicy("NonExistentPolicy", policy =>
            {
                policy.WithErrorCode("App:SomeCode");
            });
        });

        exception.Message.ShouldContain("NonExistentPolicy");
    }

    [Fact]
    public void ConfigurePolicy_Should_Preserve_Existing_ErrorCode_When_Not_Overridden()
    {
        var options = new AbpOperationRateLimitingOptions();

        options.AddPolicy("BasePolicy", policy =>
        {
            policy.WithFixedWindow(TimeSpan.FromHours(1), maxCount: 5)
                  .PartitionByParameter()
                  .WithErrorCode("Original:ErrorCode");
        });

        options.ConfigurePolicy("BasePolicy", policy =>
        {
            policy.AddRule(rule => rule
                .WithFixedWindow(TimeSpan.FromMinutes(10), maxCount: 3)
                .PartitionByClientIp());
        });

        var result = options.Policies["BasePolicy"];
        result.ErrorCode.ShouldBe("Original:ErrorCode");
        result.Rules.Count.ShouldBe(2);
        result.Rules[0].Duration.ShouldBe(TimeSpan.FromHours(1));
        result.Rules[0].PartitionType.ShouldBe(OperationRateLimitingPartitionType.Parameter);
        result.Rules[1].Duration.ShouldBe(TimeSpan.FromMinutes(10));
        result.Rules[1].PartitionType.ShouldBe(OperationRateLimitingPartitionType.ClientIp);
    }
}

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace Volo.Abp.OperationRateLimit;

/// <summary>
/// Verifies per-tenant isolation for tenant-scoped partition types and
/// global (cross-tenant) sharing for ClientIp partition type.
/// </summary>
public class OperationRateLimitMultiTenant_Tests : OperationRateLimitTestBase
{
    private readonly ICurrentTenant _currentTenant;
    private readonly IOperationRateLimitChecker _checker;

    private static readonly Guid TenantA = Guid.NewGuid();
    private static readonly Guid TenantB = Guid.NewGuid();

    public OperationRateLimitMultiTenant_Tests()
    {
        _currentTenant = GetRequiredService<ICurrentTenant>();
        _checker = GetRequiredService<IOperationRateLimitChecker>();
    }

    [Fact]
    public async Task Should_Isolate_ByParameter_Between_Tenants()
    {
        // Same parameter value in different tenants should have independent counters.
        var param = $"shared-param-{Guid.NewGuid()}";

        using (_currentTenant.Change(TenantA))
        {
            var ctx = new OperationRateLimitContext { Parameter = param };
            await _checker.CheckAsync("TestMultiTenantByParameter", ctx);
            await _checker.CheckAsync("TestMultiTenantByParameter", ctx);

            // Tenant A exhausted (max=2)
            await Assert.ThrowsAsync<AbpOperationRateLimitException>(async () =>
            {
                await _checker.CheckAsync("TestMultiTenantByParameter", ctx);
            });
        }

        using (_currentTenant.Change(TenantB))
        {
            var ctx = new OperationRateLimitContext { Parameter = param };

            // Tenant B has its own counter and should still be allowed
            await _checker.CheckAsync("TestMultiTenantByParameter", ctx);
            (await _checker.IsAllowedAsync("TestMultiTenantByParameter", ctx)).ShouldBeTrue();
        }
    }

    [Fact]
    public async Task Should_Share_ByClientIp_Across_Tenants()
    {
        // ClientIp counters are global: requests from the same IP are counted together
        // regardless of which tenant context is active.
        // The NullClientIpAddressProvider returns null, which resolves to "unknown" in the rule.

        using (_currentTenant.Change(TenantA))
        {
            var ctx = new OperationRateLimitContext();
            await _checker.CheckAsync("TestMultiTenantByClientIp", ctx);
            await _checker.CheckAsync("TestMultiTenantByClientIp", ctx);
        }

        using (_currentTenant.Change(TenantB))
        {
            var ctx = new OperationRateLimitContext();

            // Tenant B shares the same IP counter; should be at limit now
            await Assert.ThrowsAsync<AbpOperationRateLimitException>(async () =>
            {
                await _checker.CheckAsync("TestMultiTenantByClientIp", ctx);
            });
        }
    }

    [Fact]
    public async Task Should_Isolate_ByParameter_Host_Tenant_From_Named_Tenant()
    {
        // Host context (no tenant) and a specific tenant should have separate counters.
        var param = $"host-vs-tenant-{Guid.NewGuid()}";

        // Host context: exhaust quota
        var hostCtx = new OperationRateLimitContext { Parameter = param };
        await _checker.CheckAsync("TestMultiTenantByParameter", hostCtx);
        await _checker.CheckAsync("TestMultiTenantByParameter", hostCtx);
        await Assert.ThrowsAsync<AbpOperationRateLimitException>(async () =>
        {
            await _checker.CheckAsync("TestMultiTenantByParameter", hostCtx);
        });

        // Tenant A should have its own counter, unaffected by host
        using (_currentTenant.Change(TenantA))
        {
            var tenantCtx = new OperationRateLimitContext { Parameter = param };
            await _checker.CheckAsync("TestMultiTenantByParameter", tenantCtx);
            (await _checker.IsAllowedAsync("TestMultiTenantByParameter", tenantCtx)).ShouldBeTrue();
        }
    }
}

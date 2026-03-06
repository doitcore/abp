using Volo.Abp.Testing;

namespace Volo.Abp.OperationRateLimit;

public class OperationRateLimitTestBase : AbpIntegratedTest<AbpOperationRateLimitTestModule>
{
    protected override void SetAbpApplicationCreationOptions(AbpApplicationCreationOptions options)
    {
        options.UseAutofac();
    }
}

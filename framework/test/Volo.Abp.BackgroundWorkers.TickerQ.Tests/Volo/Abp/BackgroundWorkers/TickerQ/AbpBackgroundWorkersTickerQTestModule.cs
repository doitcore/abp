using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace Volo.Abp.BackgroundWorkers.TickerQ;

[DependsOn(
    typeof(AbpBackgroundWorkersTickerQModule),
    typeof(AbpTestBaseModule),
    typeof(AbpAutofacModule)
)]
public class AbpBackgroundWorkersTickerQTestModule : AbpModule
{
}
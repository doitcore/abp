using Volo.Abp.Modularity;
using Volo.Abp.TickerQ;

namespace Volo.Abp.BackgroundWorkers.TickerQ;

[DependsOn(typeof(AbpBackgroundWorkersModule), typeof(AbpTickerQModule))]
public class AbpBackgroundWorkersTickerQModule : AbpModule
{

}

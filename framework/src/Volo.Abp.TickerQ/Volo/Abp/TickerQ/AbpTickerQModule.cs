using Microsoft.Extensions.DependencyInjection;
using TickerQ.DependencyInjection;
using TickerQ.Utilities;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Modularity;

namespace Volo.Abp.TickerQ;

public class AbpTickerQModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddTickerQ(options =>
        {
            options.SetInstanceIdentifier(context.Services.GetApplicationName());
        });
    }

    public override void OnPostApplicationInitialization(ApplicationInitializationContext context)
    {
        var serviceCollection = context.ServiceProvider.GetRequiredService<IObjectAccessor<IServiceCollection>>();
        if (serviceCollection.Value == null)
        {
            return;
        }

        var tickerQ = serviceCollection.Value.ExecutePreConfiguredActions<AbpTickerQOptions>();
        TickerFunctionProvider.RegisterFunctions(tickerQ.Functions);
        TickerFunctionProvider.RegisterRequestType(tickerQ.RequestTypes);
    }
}

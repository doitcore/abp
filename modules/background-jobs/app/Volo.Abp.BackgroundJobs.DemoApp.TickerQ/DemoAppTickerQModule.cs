using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using TickerQ.Utilities.Interfaces.Managers;
using TickerQ.Utilities.Models.Ticker;
using Volo.Abp.AspNetCore;
using Volo.Abp.Autofac;
using Volo.Abp.BackgroundJobs.DemoApp.Shared;
using Volo.Abp.BackgroundJobs.DemoApp.Shared.Jobs;
using Volo.Abp.BackgroundJobs.TickerQ;
using Volo.Abp.Modularity;

namespace Volo.Abp.BackgroundJobs.DemoApp.TickerQ;

[DependsOn(
    typeof(AbpBackgroundJobsTickerQModule),
    typeof(DemoAppSharedModule),
    typeof(AbpAutofacModule),
    typeof(AbpAspNetCoreModule)
)]
public class DemoAppTickerQModule : AbpModule
{
    public override async Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        app.UseAbpTickerQ();

        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGet("/", async httpContext =>
            {
                await httpContext.Response.WriteAsync("Hello TickerQ!");
            });
        });

        await CancelableBackgroundJobAsync(context.ServiceProvider);
    }

    private async Task CancelableBackgroundJobAsync(IServiceProvider serviceProvider)
    {
        var backgroundJobManager = serviceProvider.GetRequiredService<IBackgroundJobManager>();
        var jobId = await backgroundJobManager.EnqueueAsync(new LongRunningJobArgs { Value = "test-1" });
        await backgroundJobManager.EnqueueAsync(new LongRunningJobArgs { Value = "test-2" });
        Thread.Sleep(1000);

        var timeTickerManager = serviceProvider.GetRequiredService<ITimeTickerManager<TimeTicker>>();
        var result = await timeTickerManager.DeleteAsync(Guid.Parse(jobId));
    }
}

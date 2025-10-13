using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.FileProviders;
using Microsoft.Maui;
using Volo.Abp.Threading;

namespace Volo.Abp.AspNetCore.Components.MauiBlazor.Bundling;

public class AbpBlazorWebView : BlazorWebView
{
    public AbpBlazorWebView()
    {
        BlazorWebViewInitialized += (s, e) =>
        {
            AsyncHelper.RunSync(() => Handler!.GetRequiredService<MauiBlazorCurrentTimezoneService>().InitializeAsync());
        };
    }

    public override IFileProvider CreateFileProvider(string contentRootDir)
    {
        return new CompositeFileProvider(Handler!.GetRequiredService<IMauiBlazorContentFileProvider>(), base.CreateFileProvider(contentRootDir));
    }
}

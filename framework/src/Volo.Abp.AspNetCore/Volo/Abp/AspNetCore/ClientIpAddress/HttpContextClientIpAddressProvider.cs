using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;

namespace Volo.Abp.AspNetCore.ClientIpAddress;

[Dependency(ReplaceServices = true)]
public class HttpContextClientIpAddressProvider : IClientIpAddressProvider, ITransientDependency
{
    protected ILogger<HttpContextClientIpAddressProvider> Logger { get; }
    protected IHttpContextAccessor HttpContextAccessor { get; }

    public HttpContextClientIpAddressProvider(
        ILogger<HttpContextClientIpAddressProvider> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        Logger = logger;
        HttpContextAccessor = httpContextAccessor;
    }

    public string? ClientIpAddress => GetClientIpAddress();

    protected virtual string? GetClientIpAddress()
    {
        try
        {
            return HttpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
        }
        catch (Exception ex)
        {
            Logger.LogException(ex, LogLevel.Warning);
            return null;
        }
    }
}

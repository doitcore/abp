﻿using System;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Volo.Abp.AspNetCore.Components.Web.Extensibility;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http.Client;
using Volo.Abp.Http.Client.Authentication;
using Volo.Abp.MultiTenancy;

namespace Volo.Abp.AspNetCore.Components.Server.Extensibility;

public class BlazorServerLookupApiRequestService : ILookupApiRequestService, ITransientDependency
{
    public IHttpClientFactory HttpClientFactory { get; }
    public IRemoteServiceHttpClientAuthenticator HttpClientAuthenticator { get; }
    public IRemoteServiceConfigurationProvider RemoteServiceConfigurationProvider { get; }
    public ICurrentTenant CurrentTenant { get; }
    public IHttpContextAccessor HttpContextAccessor { get; }
    public NavigationManager NavigationManager { get; }

    public BlazorServerLookupApiRequestService(IHttpClientFactory httpClientFactory,
        IRemoteServiceHttpClientAuthenticator httpClientAuthenticator,
        ICurrentTenant currentTenant,
        IHttpContextAccessor httpContextAccessor,
        NavigationManager navigationManager,
        IRemoteServiceConfigurationProvider remoteServiceConfigurationProvider)
    {
        HttpClientFactory = httpClientFactory;
        HttpClientAuthenticator = httpClientAuthenticator;
        CurrentTenant = currentTenant;
        HttpContextAccessor = httpContextAccessor;
        NavigationManager = navigationManager;
        RemoteServiceConfigurationProvider = remoteServiceConfigurationProvider;
    }

    public async Task<string> SendAsync(string url)
    {
        var client = HttpClientFactory.CreateClient(nameof(BlazorServerLookupApiRequestService));
        var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);

        var uri = new Uri(url, UriKind.RelativeOrAbsolute);
        if (!uri.IsAbsoluteUri)
        {
            var remoteServiceConfig = await RemoteServiceConfigurationProvider.GetConfigurationOrDefaultOrNullAsync("Default");
            if (remoteServiceConfig != null)
            {
                // Blazor tiered mode
                var baseUrl = remoteServiceConfig.BaseUrl;
                client.BaseAddress = new Uri(baseUrl);
                AddHeaders(requestMessage);
                await HttpClientAuthenticator.Authenticate(new RemoteServiceHttpClientAuthenticateContext(client, requestMessage, new RemoteServiceConfiguration(baseUrl), string.Empty));
            }
            else
            {
                // Blazor server  mode
                client.BaseAddress = new Uri(NavigationManager.BaseUri);
                foreach (var header in HttpContextAccessor.HttpContext!.Request.Headers)
                {
                    requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                }
            }
        }

        var response = await client.SendAsync(requestMessage);
        return await response.Content.ReadAsStringAsync();
    }

    protected virtual void AddHeaders(HttpRequestMessage requestMessage)
    {
        if (CurrentTenant.Id.HasValue)
        {
            requestMessage.Headers.Add(TenantResolverConsts.DefaultTenantKey, CurrentTenant.Id.Value.ToString());
        }

        var currentCulture = CultureInfo.CurrentUICulture.Name ?? CultureInfo.CurrentCulture.Name;
        if (!currentCulture.IsNullOrEmpty())
        {
            requestMessage.Headers.AcceptLanguage.Add(new(currentCulture));
        }
    }
}

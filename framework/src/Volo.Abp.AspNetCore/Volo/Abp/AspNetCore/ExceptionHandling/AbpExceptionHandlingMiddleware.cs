﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Volo.Abp.AspNetCore.Middleware;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Authorization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.ExceptionHandling;
using Volo.Abp.Http;
using Volo.Abp.Json;

namespace Volo.Abp.AspNetCore.ExceptionHandling;

public class AbpExceptionHandlingMiddleware : AbpMiddlewareBase, ITransientDependency
{
    private readonly ILogger<AbpExceptionHandlingMiddleware> _logger;

    private readonly Func<object, Task> _clearCacheHeadersDelegate;

    public AbpExceptionHandlingMiddleware(ILogger<AbpExceptionHandlingMiddleware> logger)
    {
        _logger = logger;

        _clearCacheHeadersDelegate = ClearCacheHeaders;
    }

    public async override Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            // We can't do anything if the response has already started, just abort.
            if (context.Response.HasStarted)
            {
                _logger.LogWarning("An exception occurred, but response has already started!");
                throw;
            }

            if (context.Items["_AbpActionInfo"] is AbpActionInfoInHttpContext actionInfo)
            {
                if (actionInfo.IsObjectResult) //TODO: Align with AbpExceptionFilter.ShouldHandleException!
                {
                    await HandleAndWrapException(context, ex);
                    return;
                }
            }

            throw;
        }
    }

    private async Task HandleAndWrapException(HttpContext httpContext, Exception exception)
    {
        var exceptionHandlingOptions = httpContext.RequestServices.GetRequiredService<IOptions<AbpExceptionHandlingOptions>>().Value;

        if (exceptionHandlingOptions.ShouldLogException(exception))
        {
            _logger.LogException(exception);
        }

        await httpContext
            .RequestServices
            .GetRequiredService<IExceptionNotifier>()
            .NotifyAsync(
                new ExceptionNotificationContext(exception)
            );

        if (exception is AbpAuthorizationException)
        {
            await httpContext.RequestServices.GetRequiredService<IAbpAuthorizationExceptionHandler>()
                .HandleAsync(exception.As<AbpAuthorizationException>(), httpContext);
        }
        else
        {
            var errorInfoConverter = httpContext.RequestServices.GetRequiredService<IExceptionToErrorInfoConverter>();
            var statusCodeFinder = httpContext.RequestServices.GetRequiredService<IHttpExceptionStatusCodeFinder>();
            var jsonSerializer = httpContext.RequestServices.GetRequiredService<IJsonSerializer>();

            httpContext.Response.Clear();
            httpContext.Response.StatusCode = (int)statusCodeFinder.GetStatusCode(httpContext, exception);
            httpContext.Response.OnStarting(_clearCacheHeadersDelegate, httpContext.Response);
            httpContext.Response.Headers.Append(AbpHttpConsts.AbpErrorFormat, "true");
            httpContext.Response.Headers.Append("Content-Type", "application/json");

            await httpContext.Response.WriteAsync(
                jsonSerializer.Serialize(
                    new RemoteServiceErrorResponse(
                        errorInfoConverter.Convert(exception, options =>
                        {
                            options.SendExceptionsDetailsToClients = exceptionHandlingOptions.SendExceptionsDetailsToClients;
                            options.SendStackTraceToClients = exceptionHandlingOptions.SendStackTraceToClients;
                            options.SendExceptionDataToClientTypes = exceptionHandlingOptions.SendExceptionDataToClientTypes;
                        })
                    )
                )
            );
        }
    }

    private Task ClearCacheHeaders(object state)
    {
        var response = (HttpResponse)state;

        response.Headers[HeaderNames.CacheControl] = "no-cache";
        response.Headers[HeaderNames.Pragma] = "no-cache";
        response.Headers[HeaderNames.Expires] = "-1";
        response.Headers.Remove(HeaderNames.ETag);

        return Task.CompletedTask;
    }
}

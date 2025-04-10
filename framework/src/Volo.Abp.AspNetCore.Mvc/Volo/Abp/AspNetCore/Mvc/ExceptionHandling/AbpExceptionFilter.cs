using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Volo.Abp.AspNetCore.ExceptionHandling;
using Volo.Abp.AspNetCore.Filters;
using Volo.Abp.Authorization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.ExceptionHandling;
using Volo.Abp.Http;
using Volo.Abp.Json;

namespace Volo.Abp.AspNetCore.Mvc.ExceptionHandling;

public class AbpExceptionFilter : IAsyncExceptionFilter, IAbpFilter, ITransientDependency
{
    public virtual async Task OnExceptionAsync(ExceptionContext context)
    {
        if (!ShouldHandleException(context))
        {
            LogException(context, out _);
            return;
        }

        await HandleAndWrapException(context);
    }

    protected virtual bool ShouldHandleException(ExceptionContext context)
    {
        //TODO: Create DontWrap attribute to control wrapping..?

        if (context.ExceptionHandled)
        {
            return false;
        }

        if (context.ActionDescriptor.IsControllerAction() &&
            context.ActionDescriptor.HasObjectResult())
        {
            return true;
        }

        if (context.HttpContext.Request.CanAccept(MimeTypes.Application.Json))
        {
            return true;
        }

        if (context.HttpContext.Request.IsAjax())
        {
            return true;
        }

        return false;
    }

    protected virtual async Task HandleAndWrapException(ExceptionContext context)
    {
        //TODO: Trigger an AbpExceptionHandled event or something like that.

        LogException(context, out var remoteServiceErrorInfo);

        await context.GetRequiredService<IExceptionNotifier>().NotifyAsync(new ExceptionNotificationContext(context.Exception));

        if (context.Exception is AbpAuthorizationException)
        {
            await context.HttpContext.RequestServices.GetRequiredService<IAbpAuthorizationExceptionHandler>()
                .HandleAsync(context.Exception.As<AbpAuthorizationException>(), context.HttpContext);
        }
        else
        {
            if (!context.HttpContext.Response.HasStarted)
            {
                context.HttpContext.Response.Headers.Append(AbpHttpConsts.AbpErrorFormat, "true");
                context.HttpContext.Response.StatusCode = (int)context
                    .GetRequiredService<IHttpExceptionStatusCodeFinder>()
                    .GetStatusCode(context.HttpContext, context.Exception);
            }
            else
            {
                var logger = context.GetService<ILogger<AbpExceptionFilter>>(NullLogger<AbpExceptionFilter>.Instance)!;
                logger.LogWarning("HTTP response has already started, cannot set headers and status code!");
            }

            context.Result = new ObjectResult(new RemoteServiceErrorResponse(remoteServiceErrorInfo));
        }

        context.ExceptionHandled = true; //Handled!
    }

    protected virtual void LogException(ExceptionContext context, out RemoteServiceErrorInfo remoteServiceErrorInfo)
    {
        var exceptionHandlingOptions = context.GetRequiredService<IOptions<AbpExceptionHandlingOptions>>().Value;
        var exceptionToErrorInfoConverter = context.GetRequiredService<IExceptionToErrorInfoConverter>();
        remoteServiceErrorInfo = exceptionToErrorInfoConverter.Convert(context.Exception, options =>
        {
            options.SendExceptionsDetailsToClients = exceptionHandlingOptions.SendExceptionsDetailsToClients;
            options.SendStackTraceToClients = exceptionHandlingOptions.SendStackTraceToClients;
            options.SendExceptionDataToClientTypes = exceptionHandlingOptions.SendExceptionDataToClientTypes;
        });

        var remoteServiceErrorInfoBuilder = new StringBuilder();
        remoteServiceErrorInfoBuilder.AppendLine($"---------- {nameof(RemoteServiceErrorInfo)} ----------");
        remoteServiceErrorInfoBuilder.AppendLine(context.GetRequiredService<IJsonSerializer>().Serialize(remoteServiceErrorInfo, indented: true));

        if(exceptionHandlingOptions.ShouldLogException(context.Exception))
        {
            var logger = context.GetService<ILogger<AbpExceptionFilter>>(NullLogger<AbpExceptionFilter>.Instance)!;
            var logLevel = context.Exception.GetLogLevel();
            logger.LogWithLevel(logLevel, remoteServiceErrorInfoBuilder.ToString());
            logger.LogException(context.Exception, logLevel);
        }
    }
}

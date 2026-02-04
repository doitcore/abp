using System;
using System.Text;
using System.Text.Json;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Microsoft.Extensions.DependencyInjection;

public static class AbpSwaggerUIOptionsExtensions
{
    /// <summary>
    /// Sets the abp.appPath used by the Swagger UI scripts.
    /// </summary>
    /// <param name="options">The Swagger UI options.</param>
    /// <param name="appPath">The application base path.</param>
    public static void AbpAppPath(this SwaggerUIOptions options, string appPath)
    {
        var normalizedAppPath = NormalizeAppPath(appPath);
        var builder = new StringBuilder(options.HeadContent ?? string.Empty);
        builder.AppendLine("<script>");
        builder.AppendLine("    var abp = abp || {};");
        builder.AppendLine($"    abp.appPath = {JsonSerializer.Serialize(normalizedAppPath)};");
        builder.AppendLine("</script>");
        options.HeadContent = builder.ToString();
    }

    private static string NormalizeAppPath(string appPath)
    {
        return string.IsNullOrWhiteSpace(appPath) ? "/" : appPath.Trim().EnsureStartsWith('/').EnsureEndsWith('/');
    }
}

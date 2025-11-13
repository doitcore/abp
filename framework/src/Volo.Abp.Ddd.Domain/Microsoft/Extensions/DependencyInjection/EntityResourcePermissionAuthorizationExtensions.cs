using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Authorization.Permissions.Resources;

namespace Microsoft.Extensions.DependencyInjection;

public static class ResourcePermissionExtenstions
{
    public static IServiceCollection AddEntityResourcePermissionAuthorization(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationHandler, EntityResourcePermissionRequirementHandler>();
        return services;
    }
}

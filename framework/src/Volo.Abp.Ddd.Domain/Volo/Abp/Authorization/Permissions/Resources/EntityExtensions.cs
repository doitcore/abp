using System.Collections.Generic;
using Volo.Abp.Domain.Entities;

namespace Volo.Abp.Authorization.Permissions.Resources;

public static class EntityExtensions
{
    public static string GetResourceName(this IEntity entity)
    {
        Check.NotNull(entity, nameof(entity));
        return entity.GetType().FullName!;
    }

    public static string GetResourceKey(this IEntity entity)
    {
        Check.NotNull(entity, nameof(entity));
        return entity.GetKeys().JoinAsString(",");
    }
}

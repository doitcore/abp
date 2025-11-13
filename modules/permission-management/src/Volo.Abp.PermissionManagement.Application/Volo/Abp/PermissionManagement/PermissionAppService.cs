using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;
using Volo.Abp.SimpleStateChecking;
using Volo.Abp.PermissionManagement.Localization;

namespace Volo.Abp.PermissionManagement;

[Authorize]
public class PermissionAppService : ApplicationService, IPermissionAppService
{
    protected PermissionManagementOptions Options { get; }
    protected IPermissionManager PermissionManager { get; }
    protected IResourcePermissionManager ResourcePermissionManager { get; }
    protected IPermissionDefinitionManager PermissionDefinitionManager { get; }
    protected ISimpleStateCheckerManager<PermissionDefinition> SimpleStateCheckerManager { get; }

    public PermissionAppService(
        IPermissionManager permissionManager,
        IPermissionDefinitionManager permissionDefinitionManager,
        IResourcePermissionManager resourcePermissionManager,
        IOptions<PermissionManagementOptions> options,
        ISimpleStateCheckerManager<PermissionDefinition> simpleStateCheckerManager)
    {
        LocalizationResource = typeof(AbpPermissionManagementResource);
        ObjectMapperContext = typeof(AbpPermissionManagementApplicationModule);

        Options = options.Value;
        PermissionManager = permissionManager;
        ResourcePermissionManager = resourcePermissionManager;
        PermissionDefinitionManager = permissionDefinitionManager;
        SimpleStateCheckerManager = simpleStateCheckerManager;
    }

    public virtual async Task<GetPermissionListResultDto> GetAsync(string providerName, string providerKey)
    {
        return await GetInternalAsync(null, providerName, providerKey);
    }

    public virtual async Task<GetPermissionListResultDto> GetByGroupAsync(string groupName, string providerName, string providerKey)
    {
        return await GetInternalAsync(groupName, providerName, providerKey);
    }

    protected virtual async Task<GetPermissionListResultDto> GetInternalAsync(string groupName, string providerName, string providerKey)
    {
        await CheckProviderPolicy(providerName);

        var result = new GetPermissionListResultDto
        {
            EntityDisplayName = providerKey,
            Groups = new List<PermissionGroupDto>()
        };

        var multiTenancySide = CurrentTenant.GetMultiTenancySide();
        var permissionGroups = new List<PermissionGroupDto>();

        foreach (var group in (await PermissionDefinitionManager.GetGroupsAsync()).WhereIf(!groupName.IsNullOrWhiteSpace(), x => x.Name == groupName))
        {
            var groupDto = CreatePermissionGroupDto(group);
            var permissions = group.GetPermissionsWithChildren()
                .Where(x => x.IsEnabled)
                .Where(x => !x.Providers.Any() || x.Providers.Contains(providerName))
                .Where(x => x.MultiTenancySide.HasFlag(multiTenancySide));

            var neededCheckPermissions = new List<PermissionDefinition>();
            foreach (var permission in permissions)
            {
                if (permission.Parent != null && !neededCheckPermissions.Contains(permission.Parent))
                {
                    continue;
                }

                if (await SimpleStateCheckerManager.IsEnabledAsync(permission))
                {
                    neededCheckPermissions.Add(permission);
                }
            }

            if (!neededCheckPermissions.Any())
            {
                continue;
            }

            groupDto.Permissions.AddRange(neededCheckPermissions.Select(CreatePermissionGrantInfoDto));
            permissionGroups.Add(groupDto);
        }

        var multipleGrantInfo = await PermissionManager.GetAsync(
            permissionGroups.SelectMany(group => group.Permissions).Select(permission => permission.Name).ToArray(),
            providerName,
            providerKey);

        foreach (var permissionGroup in permissionGroups)
        {
            foreach (var permission in permissionGroup.Permissions)
            {
                var grantInfo = multipleGrantInfo.Result.FirstOrDefault(x => x.Name == permission.Name);
                if (grantInfo == null)
                {
                    continue;
                }

                permission.IsGranted = grantInfo.IsGranted;
                permission.GrantedProviders = grantInfo.Providers.Select(x => new ProviderInfoDto
                {
                    ProviderName = x.Name,
                    ProviderKey = x.Key,
                }).ToList();
            }

            if (permissionGroup.Permissions.Any())
            {
                result.Groups.Add(permissionGroup);
            }
        }

        return result;
    }

    protected virtual PermissionGrantInfoDto CreatePermissionGrantInfoDto(PermissionDefinition permission)
    {
        return new PermissionGrantInfoDto
        {
            Name = permission.Name,
            DisplayName = permission.DisplayName?.Localize(StringLocalizerFactory),
            ParentName = permission.Parent?.Name,
            AllowedProviders = permission.Providers,
            GrantedProviders = new List<ProviderInfoDto>()
        };
    }

    protected virtual PermissionGroupDto CreatePermissionGroupDto(PermissionGroupDefinition group)
    {
        var localizableDisplayName = group.DisplayName as LocalizableString;

        return new PermissionGroupDto
        {
            Name = group.Name,
            DisplayName = group.DisplayName?.Localize(StringLocalizerFactory),
            DisplayNameKey = localizableDisplayName?.Name,
            DisplayNameResource = localizableDisplayName?.ResourceType != null
                ? LocalizationResourceNameAttribute.GetName(localizableDisplayName.ResourceType)
                : null,
            Permissions = new List<PermissionGrantInfoDto>()
        };
    }

    public virtual async Task UpdateAsync(string providerName, string providerKey, UpdatePermissionsDto input)
    {
        await CheckProviderPolicy(providerName);

        foreach (var permissionDto in input.Permissions)
        {
            await PermissionManager.SetAsync(permissionDto.Name, providerName, providerKey, permissionDto.IsGranted);
        }
    }

    public virtual async Task<GetResourcePermissionListResultDto> GetAsync(string resourceName, string resourceKey, string providerName, string providerKey)
    {
        await CheckProviderPolicy(providerName, true);

        var result = new GetResourcePermissionListResultDto
        {
            EntityDisplayName = providerKey,
            Permissions = new List<ResourcePermissionGrantInfoDto>()
        };

        var multiTenancySide = CurrentTenant.GetMultiTenancySide();

        var resourcePermissions = new List<PermissionDefinition>();
        foreach (var resourcePermission in (await PermissionDefinitionManager.GetResourcePermissionsAsync())
                 .Where(x => x.IsEnabled && (!x.Providers.Any() || x.Providers.Contains(providerName)) && x.MultiTenancySide.HasFlag(multiTenancySide)))
        {
            if (await SimpleStateCheckerManager.IsEnabledAsync(resourcePermission))
            {
                resourcePermissions.Add(resourcePermission);
            }
        }

        var multipleGrantInfo = await ResourcePermissionManager.GetAsync(resourcePermissions.Select(x => x.Name).ToArray(), resourceName, resourceKey, providerName, providerKey);
        foreach (var resourcePermission in resourcePermissions)
        {
            var grantInfo = multipleGrantInfo.Result.FirstOrDefault(x => x.Name == resourcePermission.Name);
            if (grantInfo == null)
            {
                continue;
            }

            result.Permissions.Add(new ResourcePermissionGrantInfoDto()
            {
                Name =  resourcePermission.Name,
                DisplayName = resourcePermission.DisplayName?.Localize(StringLocalizerFactory),
                IsGranted = grantInfo.IsGranted,
                AllowedProviders = resourcePermission.Providers,
                GrantedProviders = grantInfo.Providers.Select(x => new ProviderInfoDto
                {
                    ProviderName = x.Name,
                    ProviderKey = x.Key,
                }).ToList()
            });
        }

        return result;
    }

    public virtual async Task UpdateAsync(string resourceName, string resourceKey, string providerName, string providerKey, UpdatePermissionsDto input)
    {
        await CheckProviderPolicy(providerName, true);

        foreach (var permissionDto in input.Permissions)
        {
            await ResourcePermissionManager.SetAsync(permissionDto.Name, resourceName, resourceKey, providerName, providerKey, permissionDto.IsGranted);
        }
    }

    protected virtual async Task CheckProviderPolicy(string providerName, bool isResourcePermission = false)
    {
        var policyName = isResourcePermission
            ? Options.ResourceProviderPolicies.GetOrDefault(providerName)
            : Options.ProviderPolicies.GetOrDefault(providerName);
        if (policyName.IsNullOrEmpty())
        {
            throw new AbpException($"No policy defined to get/set permissions for the provider '{providerName}'. Use {nameof(PermissionManagementOptions)} to map the policy.");
        }
        await AuthorizationService.CheckAsync(policyName);
    }
}

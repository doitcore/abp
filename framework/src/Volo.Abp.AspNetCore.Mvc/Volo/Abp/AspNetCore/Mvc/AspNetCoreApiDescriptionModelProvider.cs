using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Asp.Versioning;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Volo.Abp.AspNetCore.Mvc.ApiExploring;
using Volo.Abp.AspNetCore.Mvc.Conventions;
using Volo.Abp.AspNetCore.Mvc.Utils;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http.Modeling;
using Volo.Abp.Reflection;
using Volo.Abp.Threading;

namespace Volo.Abp.AspNetCore.Mvc;

public class AspNetCoreApiDescriptionModelProvider : IApiDescriptionModelProvider, ITransientDependency
{
    public ILogger<AspNetCoreApiDescriptionModelProvider> Logger { get; set; }

    private readonly AspNetCoreApiDescriptionModelProviderOptions _options;
    private readonly IApiDescriptionGroupCollectionProvider _descriptionProvider;
    private readonly AbpAspNetCoreMvcOptions _abpAspNetCoreMvcOptions;
    private readonly AbpApiDescriptionModelOptions _modelOptions;
    private readonly IXmlDocumentationProvider _xmlDocProvider;

    public AspNetCoreApiDescriptionModelProvider(
        IOptions<AspNetCoreApiDescriptionModelProviderOptions> options,
        IApiDescriptionGroupCollectionProvider descriptionProvider,
        IOptions<AbpAspNetCoreMvcOptions> abpAspNetCoreMvcOptions,
        IOptions<AbpApiDescriptionModelOptions> modelOptions,
        IXmlDocumentationProvider xmlDocProvider)
    {
        _options = options.Value;
        _descriptionProvider = descriptionProvider;
        _abpAspNetCoreMvcOptions = abpAspNetCoreMvcOptions.Value;
        _modelOptions = modelOptions.Value;
        _xmlDocProvider = xmlDocProvider;

        Logger = NullLogger<AspNetCoreApiDescriptionModelProvider>.Instance;
    }

    public ApplicationApiDescriptionModel CreateApiModel(ApplicationApiDescriptionModelRequestDto input)
    {
        //TODO: Can cache the model?

        var model = ApplicationApiDescriptionModel.Create();

        foreach (var descriptionGroupItem in _descriptionProvider.ApiDescriptionGroups.Items)
        {
            foreach (var apiDescription in descriptionGroupItem.Items)
            {
                if (!apiDescription.ActionDescriptor.IsControllerAction())
                {
                    continue;
                }

                AddApiDescriptionToModel(apiDescription, model, input);
            }
        }

        foreach (var (_, module) in model.Modules)
        {
            var controllers = module.Controllers.GroupBy(x => x.Value.Type).ToList();
            foreach (var controller in controllers.Where(x => x.Count() > 1))
            {
                var removedController = module.Controllers.RemoveAll(x => x.Value.IsRemoteService && controller.OrderBy(c => c.Value.ControllerGroupName).Skip(1).Contains(x));
                foreach (var removed in removedController)
                {
                    Logger.LogInformation($"The controller named '{removed.Value.Type}' was removed from ApplicationApiDescriptionModel because it same with other controller.");
                }
            }
        }

        model.NormalizeOrder();
        return model;
    }

    private void AddApiDescriptionToModel(
        ApiDescription apiDescription,
        ApplicationApiDescriptionModel applicationModel,
        ApplicationApiDescriptionModelRequestDto input)
    {
        var controllerType = apiDescription
            .ActionDescriptor
            .AsControllerActionDescriptor()
            .ControllerTypeInfo;

        var setting = FindSetting(controllerType);

        var moduleModel = applicationModel.GetOrAddModule(
            GetRootPath(controllerType, apiDescription.ActionDescriptor, setting),
            GetRemoteServiceName(controllerType, setting)
        );

        var controllerModel = moduleModel.GetOrAddController(
            _options.ControllerNameGenerator(controllerType, setting),
            FindGroupName(controllerType) ?? apiDescription.GroupName,
            apiDescription.IsRemoteService(),
            apiDescription.IsIntegrationService(),
            apiDescription.GetProperty<ApiVersion>()?.ToString(),
            controllerType,
            _modelOptions.IgnoredInterfaces
        );

        var method = apiDescription.ActionDescriptor.GetMethodInfo();

        var uniqueMethodName = _options.ActionNameGenerator(method);
        if (controllerModel.Actions.ContainsKey(uniqueMethodName))
        {
            Logger.LogWarning(
                $"Controller '{controllerModel.ControllerName}' contains more than one action with name '{uniqueMethodName}' for module '{moduleModel.RootPath}'. Ignored: " +
                method);
            return;
        }

        Logger.LogDebug($"ActionApiDescriptionModel.Create: {controllerModel.ControllerName}.{uniqueMethodName}");

        bool? allowAnonymous = null;
        var authorizeModels = new List<AuthorizeDataApiDescriptionModel>();
        if (apiDescription.ActionDescriptor.EndpointMetadata.Any(x => x is IAllowAnonymous))
        {
            allowAnonymous = true;
        }
        else if (apiDescription.ActionDescriptor.EndpointMetadata.Any(x => x is IAuthorizeData))
        {
            allowAnonymous = false;
            var authorizeDatas = apiDescription.ActionDescriptor.EndpointMetadata.Where(x => x is IAuthorizeData).Cast<IAuthorizeData>().ToList();
            authorizeModels.AddRange(authorizeDatas.Select(authorizeData => new AuthorizeDataApiDescriptionModel
            {
                Policy = authorizeData.Policy,
                Roles = authorizeData.Roles
            }));
        }

        var implementFrom = controllerType.FullName;

        var interfaceType = controllerType.GetInterfaces().FirstOrDefault(i => i.GetMethods().Any(x => x.ToString() == method.ToString()));
        if (interfaceType != null)
        {
            implementFrom = TypeHelper.GetFullNameHandlingNullableAndGenerics(interfaceType);
        }

        var actionModel = controllerModel.AddAction(
            uniqueMethodName,
            ActionApiDescriptionModel.Create(
                uniqueMethodName,
                method,
                apiDescription.RelativePath!,
                apiDescription.HttpMethod,
                GetSupportedVersions(controllerType, method, setting),
                allowAnonymous,
                authorizeModels,
                implementFrom
            )
        );

        if (input.IncludeTypes)
        {
            AddCustomTypesToModel(applicationModel, method, input.IncludeDescriptions);
        }

        AddParameterDescriptionsToModel(actionModel, method, apiDescription);

        if (input.IncludeDescriptions)
        {
            if (controllerModel.Summary == null && controllerModel.Description == null && controllerModel.DisplayName == null)
            {
                PopulateControllerDescriptions(controllerModel, controllerType);
            }

            PopulateActionDescriptions(actionModel, method);
            PopulateParameterDescriptions(actionModel, method);
        }
    }

    private static List<string> GetSupportedVersions(Type controllerType, MethodInfo method,
        ConventionalControllerSetting? setting)
    {
        var supportedVersions = new List<ApiVersion>();

        var mapToAttributes = method.GetCustomAttributes<MapToApiVersionAttribute>().ToArray();
        if (mapToAttributes.Any())
        {
            supportedVersions.AddRange(
                mapToAttributes.SelectMany(a => a.Versions)
            );
        }
        else
        {
            supportedVersions.AddRange(
                controllerType.GetCustomAttributes<ApiVersionAttribute>().SelectMany(a => a.Versions)
            );

            setting?.ApiVersions.ForEach(supportedVersions.Add);
        }

        return supportedVersions.Select(v => v.ToString()).Distinct().ToList();
    }

    private void AddCustomTypesToModel(ApplicationApiDescriptionModel applicationModel, MethodInfo method, bool includeDescriptions)
    {
        foreach (var parameterInfo in method.GetParameters())
        {
            AddCustomTypesToModel(applicationModel, parameterInfo.ParameterType, includeDescriptions);
        }

        AddCustomTypesToModel(applicationModel, method.ReturnType, includeDescriptions);
    }

    private void AddCustomTypesToModel(ApplicationApiDescriptionModel applicationModel,
        Type? type, bool includeDescriptions)
    {
        if (type == null)
        {
            return;
        }

        if (type.IsGenericParameter)
        {
            return;
        }

        type = AsyncHelper.UnwrapTask(type);

        if (type == typeof(object) ||
            type == typeof(void) ||
            type == typeof(Enum) ||
            type == typeof(ValueType) ||
            type == typeof(DateOnly) ||
            type == typeof(TimeOnly) ||
            TypeHelper.IsPrimitiveExtended(type))
        {
            return;
        }

        if (TypeHelper.IsDictionary(type, out var keyType, out var valueType))
        {
            AddCustomTypesToModel(applicationModel, keyType, includeDescriptions);
            AddCustomTypesToModel(applicationModel, valueType, includeDescriptions);
            return;
        }

        if (TypeHelper.IsEnumerable(type, out var itemType))
        {
            AddCustomTypesToModel(applicationModel, itemType, includeDescriptions);
            return;
        }

        if (type.IsGenericType && !type.IsGenericTypeDefinition)
        {
            var genericTypeDefinition = type.GetGenericTypeDefinition();

            AddCustomTypesToModel(applicationModel, genericTypeDefinition, includeDescriptions);

            foreach (var genericArgument in type.GetGenericArguments())
            {
                AddCustomTypesToModel(applicationModel, genericArgument, includeDescriptions);
            }

            return;
        }

        var typeName = CalculateTypeName(type);
        if (applicationModel.Types.ContainsKey(typeName))
        {
            return;
        }

        applicationModel.Types[typeName] = TypeApiDescriptionModel.Create(type);

        if (includeDescriptions)
        {
            PopulateTypeDescriptions(applicationModel.Types[typeName], type);
        }

        AddCustomTypesToModel(applicationModel, type.BaseType, includeDescriptions);

        foreach (var propertyInfo in type.GetProperties().Where(p => p.DeclaringType == type))
        {
            AddCustomTypesToModel(applicationModel, propertyInfo.PropertyType, includeDescriptions);
        }
    }

    private static string CalculateTypeName(Type type)
    {
        if (!type.IsGenericTypeDefinition)
        {
            return TypeHelper.GetFullNameHandlingNullableAndGenerics(type);
        }

        var i = 0;
        var argumentList = type
            .GetGenericArguments()
            .Select(_ => "T" + i++)
            .JoinAsString(",");

        return $"{type.FullName!.Left(type.FullName!.IndexOf('`'))}<{argumentList}>";
    }

    private void AddParameterDescriptionsToModel(ActionApiDescriptionModel actionModel, MethodInfo method,
        ApiDescription apiDescription)
    {
        if (!apiDescription.ParameterDescriptions.Any())
        {
            return;
        }

        var parameterDescriptionNames = apiDescription
            .ParameterDescriptions
            .Select(p => p.Name)
            .ToArray();

        var methodParameterNames = method
            .GetParameters()
            .Where(IsNotFromServicesParameter)
            .Select(GetMethodParamName)
            .ToArray();

        var matchedMethodParamNames = ArrayMatcher.Match(
            parameterDescriptionNames,
            methodParameterNames
        );

        for (var i = 0; i < apiDescription.ParameterDescriptions.Count; i++)
        {
            var parameterDescription = apiDescription.ParameterDescriptions[i];
            var matchedMethodParamName = matchedMethodParamNames.Length > i
                ? matchedMethodParamNames[i]
                : parameterDescription.Name;

            actionModel.AddParameter(ParameterApiDescriptionModel.Create(
                    parameterDescription.Name,
                    _options.ApiParameterNameGenerator?.Invoke(parameterDescription),
                    matchedMethodParamName,
                    parameterDescription.Type,
                    parameterDescription.RouteInfo?.IsOptional ?? false,
                    parameterDescription.RouteInfo?.DefaultValue,
                    parameterDescription.RouteInfo?.Constraints?.Select(c => c.GetType().Name).ToArray(),
                    parameterDescription.Source.Id,
                    parameterDescription.ModelMetadata?.ContainerType != null
                        ? parameterDescription.ParameterDescriptor?.Name ?? string.Empty
                        : string.Empty
                )
            );
        }
    }

    private static bool IsNotFromServicesParameter(ParameterInfo parameterInfo)
    {
        return !parameterInfo.IsDefined(typeof(FromServicesAttribute), true);
    }

    public string GetMethodParamName(ParameterInfo parameterInfo)
    {
        var modelNameProvider = parameterInfo.GetCustomAttributes()
            .OfType<IModelNameProvider>()
            .FirstOrDefault();

        if (modelNameProvider == null)
        {
            return parameterInfo.Name!;
        }

        return (modelNameProvider.Name ?? parameterInfo.Name)!;
    }

    private static string GetRootPath(
        [NotNull] Type controllerType,
        [NotNull] ActionDescriptor actionDescriptor,
        ConventionalControllerSetting? setting)
    {
        if (setting != null)
        {
            return setting.RootPath;
        }

        var areaAttr = controllerType.GetCustomAttributes().OfType<AreaAttribute>().FirstOrDefault() ?? actionDescriptor.EndpointMetadata.OfType<AreaAttribute>().FirstOrDefault();
        if (areaAttr != null)
        {
            return areaAttr.RouteValue;
        }

        return ModuleApiDescriptionModel.DefaultRootPath;
    }

    private string GetRemoteServiceName(Type controllerType, ConventionalControllerSetting? setting)
    {
        if (setting != null)
        {
            return setting.RemoteServiceName;
        }

        var remoteServiceAttr =
            controllerType.GetCustomAttributes().OfType<RemoteServiceAttribute>().FirstOrDefault();
        if (remoteServiceAttr?.Name != null)
        {
            return remoteServiceAttr.Name;
        }

        return ModuleApiDescriptionModel.DefaultRemoteServiceName;
    }

    private string? FindGroupName(Type controllerType)
    {
        var controllerNameAttribute =
            controllerType.GetCustomAttributes().OfType<ControllerNameAttribute>().FirstOrDefault();

        if (controllerNameAttribute?.Name != null)
        {
            return controllerNameAttribute.Name;
        }

        return null;
    }

    private ConventionalControllerSetting? FindSetting(Type controllerType)
    {
        foreach (var controllerSetting in _abpAspNetCoreMvcOptions.ConventionalControllers.ConventionalControllerSettings)
        {
            if (controllerSetting.ControllerTypes.Contains(controllerType))
            {
                return controllerSetting;
            }
        }

        return null;
    }

    private void PopulateControllerDescriptions(ControllerApiDescriptionModel controllerModel, Type controllerType)
    {
        controllerModel.Summary = _xmlDocProvider.GetSummary(controllerType);
        controllerModel.Remarks = _xmlDocProvider.GetRemarks(controllerType);
        controllerModel.Description = controllerType.GetCustomAttribute<DescriptionAttribute>()?.Description;
        controllerModel.DisplayName = controllerType.GetCustomAttribute<DisplayAttribute>()?.Name;
    }

    private void PopulateActionDescriptions(ActionApiDescriptionModel actionModel, MethodInfo method)
    {
        actionModel.Summary = _xmlDocProvider.GetSummary(method);
        actionModel.Remarks = _xmlDocProvider.GetRemarks(method);
        actionModel.Description = method.GetCustomAttribute<DescriptionAttribute>()?.Description;
        actionModel.DisplayName = method.GetCustomAttribute<DisplayAttribute>()?.Name;
        actionModel.ReturnValue.Summary = _xmlDocProvider.GetReturns(method);
    }

    private void PopulateParameterDescriptions(ActionApiDescriptionModel actionModel, MethodInfo method)
    {
        foreach (var param in actionModel.ParametersOnMethod)
        {
            var paramInfo = method.GetParameters().FirstOrDefault(p => p.Name == param.Name);
            if (paramInfo == null)
            {
                continue;
            }

            param.Summary = _xmlDocProvider.GetParameterSummary(method, param.Name);
            param.Description = paramInfo.GetCustomAttribute<DescriptionAttribute>()?.Description;
            param.DisplayName = paramInfo.GetCustomAttribute<DisplayAttribute>()?.Name;
        }

        foreach (var param in actionModel.Parameters)
        {
            param.Summary = _xmlDocProvider.GetParameterSummary(method, param.NameOnMethod);
        }
    }

    private void PopulateTypeDescriptions(TypeApiDescriptionModel typeModel, Type type)
    {
        typeModel.Summary = _xmlDocProvider.GetSummary(type);
        typeModel.Remarks = _xmlDocProvider.GetRemarks(type);
        typeModel.Description = type.GetCustomAttribute<DescriptionAttribute>()?.Description;
        typeModel.DisplayName = type.GetCustomAttribute<DisplayAttribute>()?.Name;

        if (typeModel.Properties == null)
        {
            return;
        }

        foreach (var propModel in typeModel.Properties)
        {
            var propInfo = type.GetProperty(propModel.Name, BindingFlags.Instance | BindingFlags.Public);
            if (propInfo == null)
            {
                continue;
            }

            propModel.Summary = _xmlDocProvider.GetSummary(propInfo);
            propModel.Description = propInfo.GetCustomAttribute<DescriptionAttribute>()?.Description;
            propModel.DisplayName = propInfo.GetCustomAttribute<DisplayAttribute>()?.Name;
        }
    }
}

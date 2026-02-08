using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Shouldly;
using Volo.Abp.AspNetCore.Mvc.ApplicationConfigurations;
using Volo.Abp.AspNetCore.Mvc.ApplicationConfigurations.ClientProxies;
using Xunit;

namespace Volo.Abp.AspNetCore.Mvc.Client;

public class MvcCachedApplicationConfigurationClient_Tests : AbpAspNetCoreMvcClientTestBase
{
    private AbpApplicationConfigurationClientProxy _configProxy;
    private AbpApplicationLocalizationClientProxy _localizationProxy;

    private readonly ICachedApplicationConfigurationClient _applicationConfigurationClient;

    public MvcCachedApplicationConfigurationClient_Tests()
    {
        _applicationConfigurationClient = GetRequiredService<ICachedApplicationConfigurationClient>();
    }

    protected override void AfterAddApplication(IServiceCollection services)
    {
        _configProxy = Substitute.For<AbpApplicationConfigurationClientProxy>();
        _localizationProxy = Substitute.For<AbpApplicationLocalizationClientProxy>();

        services.Replace(ServiceDescriptor.Transient(_ => _configProxy));
        services.Replace(ServiceDescriptor.Transient(_ => _localizationProxy));
    }

    [Fact]
    public async Task Should_Use_Concurrent_Requests_When_Culture_Matches()
    {
        var cultureName = "en";

        _configProxy.GetAsync(Arg.Any<ApplicationConfigurationRequestOptions>()).Returns(CreateConfigDto(cultureName));

        var expectedResources = new Dictionary<string, ApplicationLocalizationResourceDto>
        {
            ["TestResource"] = new()
        };

        _localizationProxy.GetAsync(Arg.Any<ApplicationLocalizationRequestDto>()).Returns(new ApplicationLocalizationDto { Resources = expectedResources });

        var result = await _applicationConfigurationClient.GetAsync();

        result.Localization.Resources.ShouldBe(expectedResources);

        await _configProxy.Received(1).GetAsync(Arg.Is<ApplicationConfigurationRequestOptions>(x => x.IncludeLocalizationResources == false));
        await _localizationProxy.Received(1).GetAsync(Arg.Is<ApplicationLocalizationRequestDto>(x => x.CultureName == cultureName && x.OnlyDynamics == true));
    }

    [Fact]
    public async Task Should_Refetch_Localization_When_Culture_Differs()
    {
        var currentCulture = "en";
        var serverCulture = "tr";

        _configProxy.GetAsync(Arg.Any<ApplicationConfigurationRequestOptions>()).Returns(CreateConfigDto(serverCulture));

        var wrongResources = new Dictionary<string, ApplicationLocalizationResourceDto>();
        var correctResources = new Dictionary<string, ApplicationLocalizationResourceDto>
        {
            ["TestResource"] = new()
        };

        _localizationProxy.GetAsync(Arg.Is<ApplicationLocalizationRequestDto>(x => x.CultureName == currentCulture)).Returns(new ApplicationLocalizationDto { Resources = wrongResources });
        _localizationProxy.GetAsync(Arg.Is<ApplicationLocalizationRequestDto>(x => x.CultureName == serverCulture)).Returns(new ApplicationLocalizationDto { Resources = correctResources });

        var result = await _applicationConfigurationClient.GetAsync();

        result.Localization.Resources.ShouldBe(correctResources);

        await _localizationProxy.Received(1).GetAsync(Arg.Is<ApplicationLocalizationRequestDto>(x => x.CultureName == serverCulture));
    }

    private static ApplicationConfigurationDto CreateConfigDto(string cultureName)
    {
        return new ApplicationConfigurationDto
        {
            Localization =
            {
                CurrentCulture = new CurrentCultureDto { Name = cultureName }
            }
        };
    }
}

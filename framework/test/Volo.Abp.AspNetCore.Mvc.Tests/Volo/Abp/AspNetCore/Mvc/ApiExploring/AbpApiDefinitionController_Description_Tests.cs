using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.Http.Modeling;
using Xunit;

namespace Volo.Abp.AspNetCore.Mvc.ApiExploring;

public class AbpApiDefinitionController_Description_Tests : AspNetCoreMvcTestBase
{
    [Fact]
    public async Task Default_Should_Not_Include_Descriptions()
    {
        var model = await GetResponseAsObjectAsync<ApplicationApiDescriptionModel>(
            "/api/abp/api-definition");

        var controller = GetDocumentedController(model);
        controller.Summary.ShouldBeNull();
        controller.Remarks.ShouldBeNull();
        controller.Description.ShouldBeNull();
        controller.DisplayName.ShouldBeNull();
    }

    [Fact]
    public async Task IncludeDescriptions_Should_Populate_Controller_Summary()
    {
        var model = await GetResponseAsObjectAsync<ApplicationApiDescriptionModel>(
            "/api/abp/api-definition?includeDescriptions=true");

        var controller = GetDocumentedController(model);
        controller.Summary.ShouldNotBeNullOrEmpty();
        controller.Summary.ShouldContain("documented application service");
    }

    [Fact]
    public async Task IncludeDescriptions_Should_Populate_Controller_Remarks()
    {
        var model = await GetResponseAsObjectAsync<ApplicationApiDescriptionModel>(
            "/api/abp/api-definition?includeDescriptions=true");

        var controller = GetDocumentedController(model);
        controller.Remarks.ShouldNotBeNullOrEmpty();
        controller.Remarks.ShouldContain("integration tests");
    }

    [Fact]
    public async Task IncludeDescriptions_Should_Populate_Controller_Description_Attribute()
    {
        var model = await GetResponseAsObjectAsync<ApplicationApiDescriptionModel>(
            "/api/abp/api-definition?includeDescriptions=true");

        var controller = GetDocumentedController(model);
        controller.Description.ShouldBe("Documented service description from attribute");
    }

    [Fact]
    public async Task IncludeDescriptions_Should_Populate_Controller_DisplayName_Attribute()
    {
        var model = await GetResponseAsObjectAsync<ApplicationApiDescriptionModel>(
            "/api/abp/api-definition?includeDescriptions=true");

        var controller = GetDocumentedController(model);
        controller.DisplayName.ShouldBe("Documented Service");
    }

    [Fact]
    public async Task IncludeDescriptions_Should_Populate_Action_Descriptions()
    {
        var model = await GetResponseAsObjectAsync<ApplicationApiDescriptionModel>(
            "/api/abp/api-definition?includeDescriptions=true");

        var controller = GetDocumentedController(model);
        var action = GetAction(controller, "GetGreeting");

        action.Summary.ShouldNotBeNullOrEmpty();
        action.Summary.ShouldContain("greeting message");
        action.Remarks.ShouldBeNull();
        action.Description.ShouldBe("Get greeting description from attribute");
        action.DisplayName.ShouldBe("Get Greeting");
    }

    [Fact]
    public async Task IncludeDescriptions_Should_Populate_ReturnValue_Summary()
    {
        var model = await GetResponseAsObjectAsync<ApplicationApiDescriptionModel>(
            "/api/abp/api-definition?includeDescriptions=true");

        var controller = GetDocumentedController(model);
        var action = GetAction(controller, "GetGreeting");

        action.ReturnValue.Summary.ShouldNotBeNullOrEmpty();
        action.ReturnValue.Summary.ShouldContain("personalized greeting");
    }

    [Fact]
    public async Task IncludeDescriptions_Should_Populate_ParameterOnMethod_Summary()
    {
        var model = await GetResponseAsObjectAsync<ApplicationApiDescriptionModel>(
            "/api/abp/api-definition?includeDescriptions=true");

        var controller = GetDocumentedController(model);
        var action = GetAction(controller, "GetGreeting");

        var param = action.ParametersOnMethod.FirstOrDefault(p => p.Name == "name");
        param.ShouldNotBeNull();
        param.Summary.ShouldNotBeNullOrEmpty();
        param.Summary.ShouldContain("name of the person");
    }

    [Fact]
    public async Task IncludeDescriptions_Should_Populate_Parameter_Summary()
    {
        var model = await GetResponseAsObjectAsync<ApplicationApiDescriptionModel>(
            "/api/abp/api-definition?includeDescriptions=true");

        var controller = GetDocumentedController(model);
        var action = GetAction(controller, "GetGreeting");

        var param = action.Parameters.FirstOrDefault(p => p.NameOnMethod == "name");
        param.ShouldNotBeNull();
        param.Summary.ShouldNotBeNullOrEmpty();
        param.Summary.ShouldContain("name of the person");
    }

    [Fact]
    public async Task IncludeDescriptions_With_IncludeTypes_Should_Populate_Type_Descriptions()
    {
        var model = await GetResponseAsObjectAsync<ApplicationApiDescriptionModel>(
            "/api/abp/api-definition?includeDescriptions=true&includeTypes=true");

        model.Types.ShouldNotBeEmpty();

        var documentedDtoType = model.Types.FirstOrDefault(t => t.Key.Contains("DocumentedDto"));
        documentedDtoType.Value.ShouldNotBeNull();
        documentedDtoType.Value.Summary.ShouldNotBeNullOrEmpty();
        documentedDtoType.Value.Summary.ShouldContain("documented DTO");
        documentedDtoType.Value.Description.ShouldBe("Documented DTO description from attribute");
        documentedDtoType.Value.DisplayName.ShouldBe("Documented DTO");
    }

    [Fact]
    public async Task IncludeDescriptions_With_IncludeTypes_Should_Populate_Property_Descriptions()
    {
        var model = await GetResponseAsObjectAsync<ApplicationApiDescriptionModel>(
            "/api/abp/api-definition?includeDescriptions=true&includeTypes=true");

        var documentedDtoType = model.Types.FirstOrDefault(t => t.Key.Contains("DocumentedDto"));
        documentedDtoType.Value.ShouldNotBeNull();
        documentedDtoType.Value.Properties.ShouldNotBeNull();

        var nameProp = documentedDtoType.Value.Properties!.FirstOrDefault(p => p.Name == "Name");
        nameProp.ShouldNotBeNull();
        nameProp.Summary.ShouldNotBeNullOrEmpty();
        nameProp.Summary.ShouldContain("name of the documented item");
        nameProp.Description.ShouldBe("Name description from attribute");
        nameProp.DisplayName.ShouldBe("Item Name");

        var valueProp = documentedDtoType.Value.Properties!.FirstOrDefault(p => p.Name == "Value");
        valueProp.ShouldNotBeNull();
        valueProp.Summary.ShouldNotBeNullOrEmpty();
        valueProp.Description.ShouldBe("Value description from attribute");
        valueProp.DisplayName.ShouldBeNull();
    }

    [Fact]
    public async Task Default_Should_Not_Include_Type_Descriptions()
    {
        var model = await GetResponseAsObjectAsync<ApplicationApiDescriptionModel>(
            "/api/abp/api-definition?includeTypes=true");

        var documentedDtoType = model.Types.FirstOrDefault(t => t.Key.Contains("DocumentedDto"));
        documentedDtoType.Value.ShouldNotBeNull();
        documentedDtoType.Value.Summary.ShouldBeNull();
        documentedDtoType.Value.Description.ShouldBeNull();
    }

    [Fact]
    public async Task Action_Without_Descriptions_Should_Have_Null_Properties()
    {
        var model = await GetResponseAsObjectAsync<ApplicationApiDescriptionModel>(
            "/api/abp/api-definition?includeDescriptions=true");

        var peopleController = model.Modules.Values
            .SelectMany(m => m.Controllers.Values)
            .First(c => c.ControllerName == "People");

        var action = peopleController.Actions.Values.First(a => a.Name == "GetPhones");
        action.Summary.ShouldBeNull();
        action.Description.ShouldBeNull();
        action.DisplayName.ShouldBeNull();
    }

    private static ControllerApiDescriptionModel GetDocumentedController(ApplicationApiDescriptionModel model)
    {
        return model.Modules.Values
            .SelectMany(m => m.Controllers.Values)
            .First(c => c.ControllerName == "Documented");
    }

    private static ActionApiDescriptionModel GetAction(ControllerApiDescriptionModel controller, string actionName)
    {
        return controller.Actions.Values
            .First(a => a.Name == actionName + "Async" || a.Name == actionName);
    }
}

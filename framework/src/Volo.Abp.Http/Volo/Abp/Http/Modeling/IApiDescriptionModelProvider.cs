using System.Threading.Tasks;

namespace Volo.Abp.Http.Modeling;

public interface IApiDescriptionModelProvider
{
    ApplicationApiDescriptionModel CreateApiModel(ApplicationApiDescriptionModelRequestDto input);

    Task<ApplicationApiDescriptionModel> CreateApiModelAsync(ApplicationApiDescriptionModelRequestDto input)
        => Task.FromResult(CreateApiModel(input));
}

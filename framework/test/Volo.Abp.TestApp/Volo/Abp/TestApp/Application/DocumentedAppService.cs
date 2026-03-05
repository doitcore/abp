using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.TestApp.Application.Dto;

namespace Volo.Abp.TestApp.Application;

/// <summary>
/// A documented application service for testing API descriptions.
/// </summary>
/// <remarks>
/// This service is used in integration tests to verify XML doc extraction.
/// </remarks>
[Description("Documented service description from attribute")]
[Display(Name = "Documented Service")]
public class DocumentedAppService : ApplicationService, IDocumentedAppService
{
    /// <summary>
    /// Gets a greeting message for the specified name.
    /// </summary>
    /// <param name="name">The name of the person to greet.</param>
    /// <returns>A personalized greeting message.</returns>
    [Description("Get greeting description from attribute")]
    [Display(Name = "Get Greeting")]
    public Task<string> GetGreetingAsync(string name)
    {
        return Task.FromResult($"Hello, {name}!");
    }

    /// <summary>
    /// Creates a documented item.
    /// </summary>
    /// <param name="input">The input for creating a documented item.</param>
    /// <returns>The created documented item.</returns>
    public Task<DocumentedDto> CreateAsync(DocumentedDto input)
    {
        return Task.FromResult(input);
    }
}

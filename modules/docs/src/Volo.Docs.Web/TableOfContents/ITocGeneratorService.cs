using Volo.Abp.Application.Services;

namespace Volo.Docs.TableOfContents;

public interface ITocGeneratorService : IApplicationService
{
    string GenerateToc(string markdownContent);
}

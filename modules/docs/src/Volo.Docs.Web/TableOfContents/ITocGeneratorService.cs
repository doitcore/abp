using Volo.Abp.Application.Services;

namespace Volo.Docs.TableOfContents;

public interface ITocGeneratorService : IApplicationService
{
    (string TocHtml, string ProcessedContent) GenerateTocAndProcessHeadings(string content);
}

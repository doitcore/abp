using System.Collections.Generic;
using Volo.Abp.Application.Services;
using static Volo.Docs.TableOfContents.TocGeneratorService;

namespace Volo.Docs.TableOfContents;

public interface ITocGeneratorService : IApplicationService
{
    List<Heading> GenerateTocHeadings(string markdownContent);
}

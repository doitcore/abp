using System.Collections.Generic;
using Volo.Abp.Application.Services;

namespace Volo.Docs.TableOfContents;

public interface ITocGeneratorService : IApplicationService
{
    List<TocHeading> GenerateTocHeadings(string markdownContent);
}

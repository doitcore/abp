using System.Collections.Generic;

namespace Volo.Docs.TableOfContents;

public record TocHeading(int Level, string Text, string Id);

public record TocItem(TocHeading Heading, List<TocItem> Children);

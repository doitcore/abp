using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Volo.Abp.DependencyInjection;

namespace Volo.Docs.TableOfContents;

public class TocGeneratorService : ITocGeneratorService, ITransientDependency
{
    public virtual List<TocHeading> GenerateTocHeadings(string markdownContent)
    {
        if (markdownContent.IsNullOrWhiteSpace())
        {
            return null;
        }

        var pipelineBuilder = new MarkdownPipelineBuilder()
            .UseAutoIdentifiers(AutoIdentifierOptions.GitHub)
            .UseAdvancedExtensions();

        var pipeline = pipelineBuilder.Build();

        var document = Markdig.Markdown.Parse(markdownContent, pipeline);

        var headingBlocks = document.Descendants<HeadingBlock>();

        return headingBlocks
            .Select(hb => new TocHeading(hb.Level, GetPlainText(hb.Inline), hb.GetAttributes().Id)).ToList();
    }

    public virtual List<TocItem> GenerateTocItems(List<TocHeading> tocHeadings, int topLevel, int maxLevel)
    {
        return BuildHierarchicalStructure(tocHeadings
            .Where(h => h.Level >= topLevel && h.Level <= maxLevel).ToList(), topLevel);
    }
    
    public virtual int GetTopLevel(List<TocHeading> headings)
    {
        for (var i = 1; i <= 6; i++)
        {
            if (headings.Count(h => h.Level == i) > 1)
            {
                return i;
            }
        }
        return 1;
    }

    public virtual List<TocItem> GenerateTocItems(string markdownContent, int maxLevel, int? topLevel = null)
    {
        var headings = GenerateTocHeadings(markdownContent);
        var topLevelToUse = topLevel ?? GetTopLevel(headings);
        return GenerateTocItems(headings, topLevelToUse, maxLevel);
    }

    protected virtual List<TocItem> BuildHierarchicalStructure(List<TocHeading> headings, int topLevel)
    {
        var result = new List<TocItem>();

        for (var i = 0; i < headings.Count; i++)
        {
            var currentHeading = headings[i];

            if (currentHeading.Level != topLevel)
            {
                continue;
            }

            result.Add(new TocItem(currentHeading, GetDirectChildren(headings, i, currentHeading.Level)));
        }

        return result;
    }

    protected virtual List<TocItem> GetDirectChildren(List<TocHeading> allHeadings, int parentIndex, int parentLevel)
    {
        var children = new List<TocItem>();
        var targetChildLevel = parentLevel + 1;

        for (var i = parentIndex + 1; i < allHeadings.Count; i++)
        {
            var heading = allHeadings[i];
            
            if (heading.Level <= parentLevel)
            {
                break;
            }

            if (heading.Level != targetChildLevel)
            {
                continue;
            }

            children.Add(new TocItem(heading, GetDirectChildren(allHeadings, i, heading.Level)));
        }

        return children;
    }

    protected virtual string GetPlainText(ContainerInline container)
    {
        if (container == null)
        {
            return string.Empty;
        }
       
        if(container.Count() == 1 && container.First() is LiteralInline literalInline)
        {
            return literalInline.Content.ToString();
        }

        var builder = new StringBuilder();
        var inlinesToProcess = new Queue<Inline>();

        foreach (var inline in container)
        {
            inlinesToProcess.Enqueue(inline);
        }

        while (inlinesToProcess.Count > 0)
        {
            var currentInline = inlinesToProcess.Dequeue();

            switch (currentInline)
            {
                case LiteralInline literal:
                    builder.Append(literal.Content);
                    break;

                case CodeInline code:
                    builder.Append(code.Content);
                    break;

                case ContainerInline childContainer:
                    foreach (var childInline in childContainer)
                    {
                        inlinesToProcess.Enqueue(childInline);
                    }
                    break;
            }
        }

        return builder.ToString();
    }
}

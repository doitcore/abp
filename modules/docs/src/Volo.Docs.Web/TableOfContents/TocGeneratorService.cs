using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using Volo.Abp.DependencyInjection;
using Volo.Docs.Markdown;

namespace Volo.Docs.TableOfContents;

public class TocGeneratorService : ITocGeneratorService, ITransientDependency
{
    public record Heading(int Level, string Text, string Id);

    public string GenerateToc(string markdownContent)
    {
        if (markdownContent.IsNullOrWhiteSpace())
        {
            return string.Empty;
        }

        var headingExtractionExtension = new HeadingExtractionExtension();
        var pipelineBuilder = new MarkdownPipelineBuilder()
            .UseAutoIdentifiers(AutoIdentifierOptions.GitHub)
            .UseAdvancedExtensions();
        pipelineBuilder.Use(headingExtractionExtension);

        var pipeline = pipelineBuilder.Build();
        Markdig.Markdown.ToHtml(markdownContent, pipeline);

        var headings = headingExtractionExtension.Headings
            .Select(h => new Heading(h.Level, h.Text, h.Id))
            .ToList();

        return BuildTocHtml(headings);
    }

    private static string BuildTocHtml(List<Heading> headings)
    {
        if (headings == null || headings.Count == 0)
        {
            return string.Empty;
        }

        var relevantHeadings = headings
            .Where(h => h.Level is 2 or 3)
            .ToList();

        if (relevantHeadings.Count == 0)
        {
            relevantHeadings = headings
                .Where(h => h.Level == 1)
                .ToList();
        }

        if (relevantHeadings.Count == 0)
        {
            return string.Empty;
        }

        var baseLevel = relevantHeadings.Min(h => h.Level);
        var normalizedHeadings = relevantHeadings
            .Select(h => h with { Level = h.Level - baseLevel + 1 })
            .ToList();

        var tocBuilder = new StringBuilder();
        var levelStack = new Stack<int>();
        levelStack.Push(0);

        for (var i = 0; i < normalizedHeadings.Count; i++)
        {
            var heading = normalizedHeadings[i];
            var previousLevel = levelStack.Peek();

            if (heading.Level < previousLevel)
            {
                while (heading.Level < levelStack.Peek())
                {
                    tocBuilder.Append("</li></ul>");
                    levelStack.Pop();
                }
            }
            else if (heading.Level > previousLevel)
            {
                tocBuilder.Append("<ul class=\"nav nav-pills flex-column\">");
                levelStack.Push(heading.Level);
            }
            else if (i > 0)
            {
                tocBuilder.Append("</li>");
            }

            var hasChildren = (i + 1 < normalizedHeadings.Count) &&
                              (normalizedHeadings[i + 1].Level > heading.Level);

            var liClass = hasChildren ? "nav-item toc-item-has-children" : "nav-item";

            tocBuilder.Append($"<li class=\"{liClass}\"><a class=\"nav-link\" href=\"#{heading.Id}\">{heading.Text}</a>");
        }

        if (normalizedHeadings.Count > 0)
        {
            tocBuilder.Append("</li>");
        }

        while (levelStack.Count > 1)
        {
            tocBuilder.Append("</ul>");
            levelStack.Pop();
        }
        
        return tocBuilder.ToString();
    }
}

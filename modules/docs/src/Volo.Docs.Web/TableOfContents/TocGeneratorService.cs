using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Volo.Abp.DependencyInjection;
using HtmlAgilityPack;

namespace Volo.Docs.TableOfContents;

public class TocGeneratorService : ITocGeneratorService, ITransientDependency
{
    private readonly HashSet<string> _generatedIds = [];
    public record Heading(int Level, string Text, string Id);

    public (string TocHtml, string ProcessedContent) GenerateTocAndProcessHeadings(string content)
    {
        if (content.IsNullOrWhiteSpace())
        {
            return (string.Empty, string.Empty);
        }

        _generatedIds.Clear();
        var tocHeadings = new List<Heading>();

        var doc = new HtmlDocument();
        doc.LoadHtml(content);

        var nodesWithId = doc.DocumentNode.SelectNodes("//*[@id]");
        if (nodesWithId != null)
        {
            foreach (var node in nodesWithId)
            {
                _generatedIds.Add(node.Id);
            }
        }

        var headingNodes = doc.DocumentNode.SelectNodes("//h1|//h2|//h3|//h4|//h5|//h6");
        if (headingNodes != null)
        {
            foreach (var node in headingNodes)
            {
                var id = node.Id;

                if (id.IsNullOrWhiteSpace())
                {
                    id = GenerateUniqueId(node.InnerText.Trim());
                    node.SetAttributeValue("id", id);
                }

                var level = int.Parse(node.Name.Substring(1));
                if (level == 2 || level == 3)
                {
                    tocHeadings.Add(new Heading(level, node.InnerText.Trim(), id));
                }
            }
        }

        var tocHtml = BuildTocHtml(tocHeadings);

        var processedContent = doc.DocumentNode.OuterHtml;

        return (tocHtml, processedContent);
    }

    private string GenerateUniqueId(string text)
    {
        if (text.IsNullOrWhiteSpace())
        {
            return $"section-{Guid.NewGuid().ToString("N")[..8]}";
        }

        var baseId = text.ToLowerInvariant();

        baseId = Regex.Replace(baseId, @"[^a-z0-9]+", "-", RegexOptions.Compiled);

        baseId = baseId.Trim('-');

        if (baseId.IsNullOrWhiteSpace())
        {
            return $"section-{Guid.NewGuid().ToString("N")[..8]}";
        }

        var finalId = baseId;
        var counter = 1;

        while (!_generatedIds.Add(finalId))
        {
            finalId = $"{baseId}-{++counter}";
        }

        return finalId;
    }

    private static string BuildTocHtml(List<Heading> headings)
    {
        if (headings == null || headings.Count == 0)
        {
            return string.Empty;
        }

        const int H2Level = 2;
        const int H3Level = 3;

        var tocBuilder = new StringBuilder();
        tocBuilder.Append("<ul class=\"nav nav-pills flex-column\">");

        var currentLevel = 0;
        var isFirstH2 = true;

        foreach (var (index, heading) in headings.Select((h, i) => (i, h)))
        {
            var isLastItem = index == headings.Count - 1;
            var nextHeading = isLastItem ? null : headings[index + 1];

            var hasChildren = nextHeading?.Level == H3Level && heading.Level == H2Level;

            if (heading.Level < currentLevel)
            {
                tocBuilder.Append("</ul></li>");
            }
            else if (heading.Level == currentLevel && heading.Level == H2Level && !isFirstH2)
            {
                tocBuilder.Append("</li>");
            }

            if (heading.Level == H2Level)
            {
                var liClass = hasChildren ? "nav-item toc-item-has-children" : "nav-item";
                tocBuilder.Append($"<li class=\"{liClass}\"><a class=\"nav-link\" href=\"#{heading.Id}\">{heading.Text}</a>");
                isFirstH2 = false;
            }
            else if (heading.Level == H3Level)
            {
                if (currentLevel < H3Level)
                {
                    tocBuilder.Append("<ul class=\"nav nav-pills flex-column\">");
                }
                tocBuilder.Append($"<li class=\"nav-item\"><a class=\"nav-link\" href=\"#{heading.Id}\">{heading.Text}</a></li>");
            }

            currentLevel = heading.Level;
        }

        if (currentLevel == H3Level)
        {
            tocBuilder.Append("</ul></li>"); 
        }
        else if (currentLevel == H2Level)
        {
            tocBuilder.Append("</li>");
        }

        tocBuilder.Append("</ul>"); 

        return tocBuilder.ToString();
    }
}

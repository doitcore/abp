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

    public (string TocHtml, string ProcessedContent) GenerateTocAndProcessHeadings(string content)
    {
        if (content.IsNullOrWhiteSpace())
        {
            return (string.Empty, string.Empty);
        }

        _generatedIds.Clear();
        var tocHeadings = new List<(int Level, string Text, string Id)>();

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
                    tocHeadings.Add((level, node.InnerText.Trim(), id));
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

    private static string BuildTocHtml(List<(int Level, string Text, string Id)> headings)
    {
        if (headings == null || headings.Count == 0)
        {
            return string.Empty;
        }

        var tocBuilder = new StringBuilder();
        tocBuilder.Append("<ul class=\"nav nav-pills flex-column\">");

        var currentLevel = 0;
        var isFirstH2 = true;

        foreach (var (index, heading) in headings.Select((h, i) => (i, h)))
        {
            var isLastItem = index == headings.Count - 1;
            var nextHeading = isLastItem ? default : headings[index + 1];
            var hasChildren = nextHeading.Level == 3 && heading.Level == 2;

            if (heading.Level < currentLevel)
            {
                tocBuilder.Append("</ul></li>");
            }
            else if (!isFirstH2 && heading.Level == 2 && currentLevel == 2)
            {
                tocBuilder.Append("</li>");
            }

            if (heading.Level == 2)
            {
                var liClass = hasChildren ? "nav-item toc-item-has-children" : "nav-item";
                tocBuilder.Append($"<li class=\"{liClass}\"><a class=\"nav-link\" href=\"#{heading.Id}\">{heading.Text}</a>");
                isFirstH2 = false;
            }
            else if (heading.Level == 3)
            {
                if (currentLevel != 3)
                {
                    tocBuilder.Append("<ul class=\"nav nav-pills flex-column\">");
                }

                tocBuilder.Append($"<li class=\"nav-item\"><a class=\"nav-link\" href=\"#{heading.Id}\">{heading.Text}</a></li>");
            }

            currentLevel = heading.Level;
        }

        if (currentLevel == 3)
        {
            tocBuilder.Append("</ul></li>");
        }
        else if (currentLevel == 2 && !isFirstH2)
        {
            tocBuilder.Append("</li>");
        }

        tocBuilder.Append("</ul>");

        return tocBuilder.ToString();
    }
}

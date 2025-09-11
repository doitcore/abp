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
    public List<TocHeading> GenerateTocHeadings(string markdownContent)
    {
        if (markdownContent.IsNullOrWhiteSpace())
        {
            return null;
        }

        var pipelineBuilder = new MarkdownPipelineBuilder()
            .UseAutoIdentifiers(AutoIdentifierOptions.GitHub)
            .UseAdvancedExtensions();

        var pipeline = pipelineBuilder.Build();

        var headings = new List<TocHeading>();

        var document = Markdig.Markdown.Parse(markdownContent, pipeline);

        var headingBlocks = document.Descendants<HeadingBlock>();

        foreach (var headingBlock in headingBlocks)
        {
            headings.Add(new TocHeading {
                Level =  headingBlock.Level,
                Text = GetPlainText(headingBlock.Inline),
                Id = headingBlock.GetAttributes()?.Id
            });
        }

        return headings;
    }

    private static string GetPlainText(ContainerInline container)
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

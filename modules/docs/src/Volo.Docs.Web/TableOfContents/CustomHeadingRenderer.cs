using System.Collections.Generic;
using System.Linq;
using System.Text;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Volo.Docs.TableOfContents;

public class CustomHeadingRenderer : MarkdownObjectRenderer<HtmlRenderer, HeadingBlock>
{
    private readonly HeadingExtractionExtension _extension;
    private readonly HeadingRenderer _originalRenderer;

    public CustomHeadingRenderer(HeadingExtractionExtension extension, HeadingRenderer originalRenderer)
    {
        _extension = extension;
        _originalRenderer = originalRenderer ?? new HeadingRenderer();
    }

    protected override void Write(HtmlRenderer renderer, HeadingBlock headingBlock)
    {
        var headingText = GetPlainText(headingBlock.Inline);
        var headingId = headingBlock.TryGetAttributes()?.Id ?? string.Empty;
        _extension.Headings.Add((headingBlock.Level, headingText, headingId));
        _originalRenderer.Write(renderer, headingBlock);
    }

    private static string GetPlainText(ContainerInline container)
    {
        if (container == null)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();

        var inlinesToProcess = new Stack<Inline>();

        // Push items in reverse for left-to-right processing (LIFO stack behavior)
        foreach (var inline in container.Reverse())
        {
            inlinesToProcess.Push(inline);
        }

        while (inlinesToProcess.Count > 0)
        {
            var currentInline = inlinesToProcess.Pop();

            switch (currentInline)
            {
                // Case 1: Simple leaf nodes with text content
                case LiteralInline literal:
                    builder.Append(literal.Content);
                    break;
                case CodeInline code:
                    builder.Append(code.Content);
                    break;

                // Case 2: Container nodes - process their children next
                case ContainerInline childContainer:
                    foreach (var childInline in childContainer.Reverse())
                    {
                        inlinesToProcess.Push(childInline);
                    }
                    break;
            }
        }

        return builder.ToString();
    }
}

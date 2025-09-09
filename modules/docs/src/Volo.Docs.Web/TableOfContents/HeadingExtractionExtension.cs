using System.Collections.Generic;
using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html;

namespace Volo.Docs.TableOfContents;

public class HeadingExtractionExtension : IMarkdownExtension
{
    public List<(int Level, string Text, string Id)> Headings { get; } = [];

    public void Setup(MarkdownPipelineBuilder pipeline)
    {
    }

    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
        if (renderer is not HtmlRenderer)
        {
            return;
        }

        var originalHeadingRenderer = renderer.ObjectRenderers.Find<HeadingRenderer>();
        if (originalHeadingRenderer != null)
        {
            renderer.ObjectRenderers.Remove(originalHeadingRenderer);
        }
        renderer.ObjectRenderers.Add(new CustomHeadingRenderer(this, originalHeadingRenderer));
    }
}

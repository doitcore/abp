using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using iText.Html2pdf;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Action;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;
using Volo.Docs.Utils;
using ITextDocument = iText.Layout.Document;

namespace Volo.Docs.Projects.Pdf.IText;

public class ITextHtmlToPdfRenderer : IHtmlToPdfRenderer ,ITransientDependency
{
    protected IOptions<DocsProjectPdfGeneratorOptions> Options { get; }
    
    public ITextHtmlToPdfRenderer(IOptions<DocsProjectPdfGeneratorOptions> options)
    {
        Options = options;
    }
    
    public virtual Task<MemoryStream> RenderAsync(string title, string html, List<PdfDocument> documents)
    {
        var pdfStream = new MemoryStream();
        var pdfWrite = new PdfWriter(pdfStream);
        var pdfDocument = new iText.Kernel.Pdf.PdfDocument(pdfWrite);
        pdfDocument.GetDocumentInfo().SetTitle(title);
        var textDocument = new ITextDocument(pdfDocument);
        pdfWrite.SetCloseStream(false);
        
        var converter = new ConverterProperties();
        var tagWorkerFactory = new HtmlIdTagWorkerFactory(pdfDocument);
        converter.SetTagWorkerFactory(tagWorkerFactory);
        
        HtmlConverter.ConvertToDocument(html, pdfDocument, converter);
        
        tagWorkerFactory.AddNamedDestinations();
        var pdfOutlines = pdfDocument.GetOutlines(false);
        BuildPdfOutlines(pdfOutlines, documents);
                
        textDocument.Close();
        pdfStream.Position = 0;
        return Task.FromResult(pdfStream);
    }

    private void BuildPdfOutlines(PdfOutline parentOutline, List<PdfDocument> pdfDocumentNodes)
    {
        foreach (var pdfDocumentNode in pdfDocumentNodes)
        {
            if (pdfDocumentNode.IgnoreOnOutline)
            {
                continue;
            }
            
            var outline = parentOutline.AddOutline(pdfDocumentNode.Title);
            if (!pdfDocumentNode.Id.IsNullOrWhiteSpace())
            {
                outline.AddAction(UrlHelper.IsExternalLink(pdfDocumentNode.Id) ? PdfAction.CreateURI(pdfDocumentNode.Id) : PdfAction.CreateGoTo(pdfDocumentNode.Id));
            }

            if (pdfDocumentNode.HasChildren)
            {
                BuildPdfOutlines(outline, pdfDocumentNode.Children);
            }
        }
    }
}
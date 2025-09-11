namespace Volo.Docs.TableOfContents;

public record TocHeading
{
    public int Level { get; set; }
    public string Text { get; set; }
    public string Id { get; set; }
}

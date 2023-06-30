namespace Wikkit.Data;

public class ArticlePageData
{
    public int pageid { get; set; }
    public int ns { get; set; }
    public string title { get; set; }
    public string contentmodel { get; set; }
    public string pagelanguage { get; set; }
    public string pagelanguagehtmlcode { get; set; }
    public string pagelanguagedir { get; set; }
    public string touched { get; set; }
    public int lastrevid { get; set; }
    public int length { get; set; }
    public string fullurl { get; set; }
    public string editurl { get; set; }
    public string canonicalurl { get; set; }
    public ArticleImageData thumbnail { get; set; }
    public List<ImageData> images { get; set; }
    public string description { get; set; }
    public string descriptionsource { get; set; }
    public string extract { get; set; }
    public bool viewed { get; set; }
}


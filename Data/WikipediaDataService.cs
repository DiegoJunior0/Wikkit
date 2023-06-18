using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace Wikkit.Data;
public partial class WikipediaDataService
{
    
    private const string apiEndpoint = "https://en.wikipedia.org/w/api.php";
    private HttpClient _client;

    public WikipediaDataService(HttpClient client)
    {
        _client = client;
    }

    public async Task<ArticleHeaderData> GetRandomArticle()
    {
        UriBuilder builder = new(apiEndpoint)
        {
            Query = "action=query&format=json&list=random&rnnamespace=0"
        };

        string jsonString = await _client.GetStringAsync(builder.Uri.AbsoluteUri);

        // Deserialize the JSON string
        QueryResponseRandom data = JsonSerializer.Deserialize<QueryResponseRandom>(jsonString);

        return data.query.random[0];
    }

    public async Task<List<ArticlePageData>> GetRandomArticles(int articleCount)
    {

        UriBuilder builder = new(apiEndpoint)
        {
            Query = $"action=query&format=json&prop=info|pageimages|description|pageprops|extracts&generator=random&inprop=url&exintro=1&grnnamespace=0&grnlimit={articleCount}"
        };

        string jsonString = await _client.GetStringAsync(builder.Uri.AbsoluteUri);

        // Deserialize the JSON string
        var data = JsonSerializer.Deserialize<QueryResponseInfo>(jsonString);

        return data.query.pages.Values.ToList();
    }

    public async Task<ArticlePageData> GetArticleInfo(int articleID)
    {

        UriBuilder url = new UriBuilder(apiEndpoint)
        {
            Query = $"action=query&format=json&pageids={articleID}&prop=info|pageimages|description|pageprops|extracts&inprop=url"
        };

        //string url = $"https://en.wikipedia.org/w/api.php?action=query&format=json&pageids={articleID}&prop=info|images|description|pageprops|extracts&inprop=url";
        string jsonString = await _client.GetStringAsync(url.Uri.AbsoluteUri);

        var data = JsonSerializer.Deserialize<QueryResponseInfo>(jsonString);

        ArticlePageData articlePageData = data.query.pages[articleID.ToString()];       

        return articlePageData;  

    }

    public async Task<List<ArticlePageData>> GetMostViewed(int offset)
    {
        UriBuilder url = new UriBuilder(apiEndpoint)
        {
            Query = $"action=query&format=json&prop=info|pageprops|pageimages|description|extracts&exintro=true&generator=mostviewed&inprop=url&gpvimlimit=10&gpvimoffset={offset}"
        };

        string jsonString = await _client.GetStringAsync(url.Uri.AbsoluteUri);

        var data = JsonSerializer.Deserialize<QueryResponseInfo>(jsonString);

        return data.query.pages.Values.ToList();

    }

    private async Task<string> GetImageUrl(string imageTitle)
    {
        UriBuilder url = new UriBuilder("https://en.wikipedia.org/w/api.php")
        {
            Query = $"action=query&format=json&prop=imageinfo&iiprop=url&titles={imageTitle}"
        };

        string jsonString = await _client.GetStringAsync(url.Uri.AbsoluteUri);

        JsonDocument data = JsonDocument.Parse(jsonString);

        string imageurl = data.RootElement.GetProperty("query")
    .GetProperty("pages").EnumerateObject().First().Value.GetProperty("imageinfo")[0].GetProperty("url").GetString();

        return imageurl;

    }

    private class QueryDataRandom
    {
        public List<ArticleHeaderData> random { get; set; }
    }

    private class QueryResponseRandom
    {
        public string batchcomplete { get; set; }
        public QueryDataRandom query { get; set; }
    }

    private class QueryResponseInfo
    {
        public string batchcomplete { get; set; }
        public WarningData warnings { get; set; }
        public QueryDataInfo query { get; set;}
    }

    private class WarningData
    {
        public Extracts extracts { get; set; }
    }

    public partial class Extracts
    {
        [JsonPropertyName("*")]
        public string Empty { get; set; }
    }

    private class QueryDataInfo
    {
        public Dictionary<string, ArticlePageData> pages { get; set; }
    }

}

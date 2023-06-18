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

    public async Task<List<ArticlePageData>> GetRandomArticles(int articleCount)
    {

        UriBuilder builder = new(apiEndpoint)
        {
            Query = $"action=query&format=json&prop=info|pageimages|description|pageprops|extracts" +
            $"&generator=random&inprop=url&exintro=1&grnnamespace=0&grnlimit={articleCount}"
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

    public async Task<List<ArticlePageData>> GetMostViewed(int count, int offset)
    {
        UriBuilder url = new UriBuilder(apiEndpoint)
        {
            Query = $"action=query&format=json&prop=info|pageprops|pageimages|description|extracts" +
            $"&exintro=true&generator=mostviewed&inprop=url&gpvimlimit={count}&gpvimoffset={offset}"
        };

        string jsonString = await _client.GetStringAsync(url.Uri.AbsoluteUri);
        
        var data = JsonSerializer.Deserialize<QueryResponseInfo>(jsonString);

        return data.query.pages.Values.Where(p => p.ns != -1).ToList();

    }

    public async Task<(List<ArticlePageData>, string @continue)> GetRecentlyChanged(int count, string cont = "")
    {
        UriBuilder url = new UriBuilder(apiEndpoint)
        {
            Query = $"action=query&format=json&prop=info|pageprops|pageimages|description|extracts" +
            $"&exintro=true&generator=recentchanges&inprop=url&grcnamespace=0&grclimit={count}"
        };

        if (cont != "")
            url.Query += $"&grccontinue={cont}";

        string jsonString = await _client.GetStringAsync(url.Uri.AbsoluteUri);

        var data = JsonSerializer.Deserialize<QueryResponeWContinue>(jsonString);

        return (data.query.pages.Values.ToList(), data.@continue["grccontinue"]);

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

    private class QueryResponseInfo
    {
        public QueryDataInfo query { get; set;}
    }

    private class QueryResponeWContinue
    {
        public QueryDataInfo query { get; set; }
        public Dictionary<string, string> @continue { get; set; }
    }

    private class QueryDataInfo
    {
        public Dictionary<string, ArticlePageData> pages { get; set; }
    }

}

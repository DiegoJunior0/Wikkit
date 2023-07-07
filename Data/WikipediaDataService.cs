using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Web;

namespace Wikkit.Data;
public partial class WikipediaDataService
{
    
    private const string apiEndpoint = "https://en.wikipedia.org/w/api.php";
    private HttpClient _client;
    private readonly ILogger<WikipediaDataService> logger;

    public WikipediaDataService(HttpClient client, ILogger<WikipediaDataService> logger)
    {
        _client = client;
        this.logger = logger;
    }

    private async Task<string> GetWikiJson(string query)
    {

        try
        {
            UriBuilder url = new UriBuilder(apiEndpoint)
            {
                Query = query
            };

            HttpResponseMessage response = await _client.GetAsync(url.Uri.AbsoluteUri);

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                return json;
            }

            Debug.WriteLine(response.ReasonPhrase);
            return "";
            
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return "";  
            throw;
        }

    }

    public async Task<List<ArticlePageData>> GetRandomArticles(int articleCount)
    {

        string query = $"action=query&format=json&prop=info|pageimages|description|extracts&pithumbsize=100" +
        $"&generator=random&inprop=url&exsentences=3&exintro=1&explaintext=1&grnnamespace=0&grnlimit={articleCount}";

        string jsonString = await GetWikiJson(query);

        if (jsonString == "")
        {
            return new List<ArticlePageData> { new ArticlePageData { title = "No results returned" } };
        }

        try
        {
            // Deserialize the JSON string
            var data = JsonSerializer.Deserialize<QueryResponseInfo>(jsonString);

            return data.query.pages.Values.ToList();
        }
        catch (Exception ex)
        {
            return new List<ArticlePageData>{ new ArticlePageData { title = ex.Message } };
        }
        
    }

    public async Task<ArticlePageData> GetArticleInfo(int articleID)
    {

        string query = $"action=query&format=json&pageids={articleID}" +
            $"&prop=info|pageimages|description|extracts&inprop=url&pithumbsize=100" +
            $"&exsentences=3&exintro=1&explaintext=1";

        string jsonString = await GetWikiJson(query);

        if (jsonString == "")
        {
            return new ArticlePageData { title = "No results returned" };
        }

        try
        {
            var data = JsonSerializer.Deserialize<QueryResponseInfo>(jsonString);

            ArticlePageData articlePageData = data.query.pages[articleID.ToString()];

            return articlePageData;
        }
        catch (Exception ex)
        {
            return new ArticlePageData { title = ex.Message };
        }        

    }

    public async Task<List<ArticlePageData>> GetMostViewed(int count, int offset)
    {

        string query = $"action=query&format=json&prop=info|pageimages|description|extracts&pithumbsize=100" +
        $"&exsentences=3&exintro=1&explaintext=1&generator=mostviewed&inprop=url&gpvimlimit={count}&gpvimoffset={offset}";

        string jsonString = await GetWikiJson(query);

        if (jsonString == "")
        {
            return new List<ArticlePageData> { new ArticlePageData { title = "No results returned" } };
        }

        try
        {
            var data = JsonSerializer.Deserialize<QueryResponseInfo>(jsonString);

            return data.query.pages.Values.Where(p => p.ns != -1).ToList();
        }
        catch (Exception ex)
        {
            return new List<ArticlePageData> { new ArticlePageData { title = ex.Message } };
        }

    }

    public async Task<(List<ArticlePageData>, string @continue)> GetRecentlyChanged(int count, string cont = "")
    {
        string query = $"action=query&format=json&prop=info|pageimages|description|extracts&pithumbsize=100" +
            $"&exsentences=3&exintro=1&explaintext=1&generator=recentchanges&inprop=url&grcnamespace=0&grclimit={count}&grcdir=newer";

        if (cont != "")
            query += $"&grccontinue={cont}";

        string jsonString = await GetWikiJson(query);

        if (jsonString == "")
        {
            return (new List<ArticlePageData> { new ArticlePageData { title = "No results returned" } }, "");
        }

        try
        {
            var data = JsonSerializer.Deserialize<QueryResponseWContinue>(jsonString);

            return (data.query.pages.Values.ToList(), data.@continue["grccontinue"].ToString());
        }
        catch (Exception ex)
        {
            return (new List<ArticlePageData> { new ArticlePageData { title = ex.Message } }, "");
        }        

    }

    public async Task<List<ArticlePageData>> GetPhotosOfTheDay(DateTime startDate, DateTime endDate)
    {
        //string query = $"action=query&format=json&prop=info|description|images|cirrusdoc" +
        //    $"&titles=Template:POTD protected/2023-06-24&formatversion=2&inprop=url&cdincludes=auxiliary_text";

        List<ArticlePageData> pages = new();

        DateTime curDate = startDate;

        while (curDate.ToString() != endDate.ToString())
        {

            try
            {
                ArticlePageData page = await GetPictureOfDay(curDate);
                pages.Add(page);
            }
            catch (Exception ex)
            {
                pages.Add(new ArticlePageData { title = ex.Message });
            }       

            curDate = curDate.AddDays(-1);

        }

        return pages;
        
    }

    public async Task<List<ArticlePageData>> GetPicturesOfTheDayRand(int count)
    {
        List<ArticlePageData> pages = new();

        Random rand = new Random();

        DateTime startDate = new(2007, 1, 1);
        DateTime endDate = DateTime.Now;

        int days = (int)(endDate - startDate).TotalDays;

        DateTime curDate = startDate.AddDays(rand.Next(0, days + 1));

        int i = 0;

        while (i < count)
        {

            try
            {
                ArticlePageData page = await GetPictureOfDay(curDate);
                pages.Add(page);
            }
            catch (Exception)
            {
                curDate = startDate.AddDays(rand.Next(0, days + 1));
                continue;
            }            

            curDate = startDate.AddDays(rand.Next(0, days + 1));

            i++;

        }

        return pages;
    }

    private async Task<ArticlePageData> GetPictureOfDay(DateTime date)
    {
        string query = $"action=query&format=json&prop=info|description|images" +
            $"&titles=Template:POTD/{date:yyyy-MM-dd}&inprop=url";

        string jsonString = await GetWikiJson(query);

        if (jsonString == "")
        {
            throw new Exception("No results returned");
        }

        try
        {
            var data = JsonSerializer.Deserialize<QueryResponseInfo>(jsonString);

            ArticlePageData page = data.query.pages.Values.ToList()[0];

            int screenWidth = (int)DeviceDisplay.Current.MainDisplayInfo.Width;

            if (page.images == null)
            {
                throw new Exception($"Page '{page.fullurl}', returned no images");
            }

            //if (page.images.Count > 1)
            //{
            //    page.images[0].url = await GetImageUrl(page.images[1].title, screenWidth);
            //}
            //else
            //{
                page.images[0].url = await GetImageUrl(page.images[0].title, screenWidth);
            //}

            page.title = $"Picture of the Day - {date: yyyy-MM-dd}";

            return page;

        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    public async Task<(List<ArticlePageData>, string @continue)> GetCurrentEvents(DateTime date, string cont = "")
    {
        //action=query&format=json&prop=info%7Cdescription%7Cextracts&titles=Portal%3ACurrent_events%2F2023_June_19
        //&generator=links&inprop=url&exsentences=5&exintro=1&explaintext=1&gplnamespace=0&gpllimit=50

        string dateString = date.ToString("yyyy_MMMM_d");

        string query = $"action=query&format=json&prop=info|pageimages|description|extracts&titles=Portal:Current_events/{dateString}" +
            $"&generator=links&inprop=url&exsentences=3&exintro=1&explaintext=1&gplnamespace=0&gpllimit=20&pithumbsize=100";

        if (cont != "") query += $"&gplcontinue={cont}";

        string jsonString= await GetWikiJson(query);

        if (jsonString == "")
        {
            return (new List<ArticlePageData> { new ArticlePageData { title = "No results returned" } },"");
        }

        try
        {
            var data = JsonSerializer.Deserialize<QueryResponseWContinue>(jsonString);

            if (data.query == null)
            {
                return (new(), "");
            }

            if (data.@continue != null)
            {
                return (data.query.pages.Values.ToList(), data.@continue["gplcontinue"].ToString());
            }

            return (data.query.pages.Values.ToList(), "");

        } catch (Exception ex)
        {
            return (new List<ArticlePageData> { new ArticlePageData { title = ex.Message } }, "");
        }        

    }

    public async Task<(List<ArticlePageData>, int offset)> GetSearch(string searchtext, int offset=0)
    {
        string query = $"action=query&format=json&prop=info|pageimages|description|extracts&pithumbsize=100" +
            $"&exsentences=3&exintro=1&explaintext=1&generator=search&inprop=url" +
            $"&gsrnamespace=0&gsrsearch={searchtext}&gsrlimit=10&gsroffset={offset}&gsrsort=relevance";

        string jsonString = await GetWikiJson(query);

        if (jsonString == "")
        {
            return (new List<ArticlePageData> { new ArticlePageData { title = "No results returned" } }, 0);
        }

        try
        {
            var data = JsonSerializer.Deserialize<QueryResponseWContinue>(jsonString);

            if (data.query == null)
            {
                return (new() { new ArticlePageData { title = "No results"} }, 0);
            }

            List<ArticlePageData> results = data.query.pages.Values.ToList();

            results = results.OrderBy(a => a.index).ToList();

            int cont = data.@continue == null ? 0 : Convert.ToInt32(data.@continue["gsroffset"].ToString());//TODO: Nasty! learn more about the stupid system.text.json

            return (results, cont); 
        }
        catch (Exception ex)
        {
            return (new () { new ArticlePageData { title = ex.Message } }, 0);
        }
    }

    public async Task<string> GetImageUrl(string title, int size)
    {
        string query = $"action=query&format=json&prop=imageinfo&iiprop=url&iiurlwidth={size}&titles={HttpUtility.UrlEncode(title)}";

        string jsonString = await GetWikiJson(query);

        if (jsonString == "")
        {
            return "";
        }

        try
        {
            JsonDocument data = JsonDocument.Parse(jsonString);

            string imageurl = data.RootElement.GetProperty("query")
                .GetProperty("pages").EnumerateObject().First().Value.GetProperty("imageinfo")[0].GetProperty("thumburl").GetString();

            return imageurl;
        }
        catch (Exception ex)
        {
            return ex.Message;           
        }
        
    }

    private class QueryResponseInfo
    {
        public QueryDataInfo query { get; set;}
    }

    private class QueryResponseWContinue
    {
        public QueryDataInfo query { get; set; }
        public Dictionary<string, object> @continue { get; set; }
    }

    private class QueryDataInfo
    {
        public Dictionary<string, ArticlePageData> pages { get; set; }
    }

}

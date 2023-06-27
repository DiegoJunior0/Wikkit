﻿using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;


namespace Wikkit.Data;
public partial class WikipediaDataService
{
    
    private const string apiEndpoint = "https://en.wikipedia.org/w/api.php";
    private HttpClient _client;

    public WikipediaDataService(HttpClient client)
    {
        _client = client;
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

        string query = $"action=query&format=json&prop=info|pageimages|description|extracts" +
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
            $"&prop=info|pageimages|description|extracts&inprop=url";

        string jsonString = await GetWikiJson(query);

        var data = JsonSerializer.Deserialize<QueryResponseInfo>(jsonString);

        ArticlePageData articlePageData = data.query.pages[articleID.ToString()];       

        return articlePageData;  

    }

    public async Task<List<ArticlePageData>> GetMostViewed(int count, int offset)
    {

        string query = $"action=query&format=json&prop=info|pageimages|description|extracts" +
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
        string query = $"action=query&format=json&prop=info|pageimages|description|extracts" +
            $"&exsentences=3&exintro=1&explaintext=1&generator=recentchanges&inprop=url&grcnamespace=0&grclimit={count}";

        if (cont != "")
            query += $"&grccontinue={cont}";

        string jsonString = await GetWikiJson(query);

        var data = JsonSerializer.Deserialize<QueryResponseWContinue>(jsonString);

        return (data.query.pages.Values.ToList(), data.@continue["grccontinue"]);

    }

    public async Task<List<ArticlePageData>> GetPhotosOfTheDay(DateTime startDate, DateTime endDate)
    {
        //string query = $"action=query&format=json&prop=info|description|images|cirrusdoc" +
        //    $"&titles=Template:POTD protected/2023-06-24&formatversion=2&inprop=url&cdincludes=auxiliary_text";

        List<ArticlePageData> pages = new();

        DateTime curDate = startDate;

        while (curDate.ToString() != endDate.ToString())
        {

            string query = $"action=query&format=json&prop=info|description|images|cirrusdoc" +
            $"&titles=Template:POTD/{curDate:yyyy-MM-dd}&inprop=url";

            string jsonString = await GetWikiJson(query);

            if (jsonString == "")
            {
                pages.Add(new ArticlePageData { title = "No results returned" } );
            }

            try
            {
                var data = JsonSerializer.Deserialize<QueryResponseInfo>(jsonString);

                ArticlePageData page = data.query.pages.Values.ToList()[0];

                page.images[0].url = await GetImageUrl(page.images[0].title);

                page.title = $"Picture of the Day - {curDate: yyyy-MM-dd}";

                pages.Add(page);                
                
            }
            catch (Exception ex)
            {
                pages.Add(new ArticlePageData { title = ex.Message } );
            }

            curDate = curDate.AddDays(-1);

        }

        return pages;
        
    }

    public async Task<(List<ArticlePageData>, string @continue)> GetCurrentEvents(DateTime date, string cont = "")
    {
        //action=query&format=json&prop=info%7Cdescription%7Cextracts&titles=Portal%3ACurrent_events%2F2023_June_19
        //&generator=links&inprop=url&exsentences=5&exintro=1&explaintext=1&gplnamespace=0&gpllimit=50

        string dateString = date.ToString("yyyy_MMMM_dd");

        string query = $"action=query&format=json&prop=info|pageimages|description|extracts&titles=Portal:Current_events/{dateString}" +
            $"&generator=links&inprop=url&exsentences=3&exintro=1&explaintext=1&gplnamespace=0&gpllimit=20";

        if (cont != "") query += $"&gplcontinue={cont}";

        string jsonString= await GetWikiJson(query);

        var data = JsonSerializer.Deserialize<QueryResponseWContinue>(jsonString);

        if (data.query == null)
        {
            return (new(), "");
        }

        if (data.@continue != null) {
            return (data.query.pages.Values.ToList(), data.@continue["gplcontinue"]);
        }

        return (data.query.pages.Values.ToList(), "");

    }

    public async Task<string> GetImageUrl(string title)
    {
        string query = $"action=query&format=json&prop=imageinfo&iiprop=url&titles={title}";

        string jsonString = await GetWikiJson(query);

        JsonDocument data = JsonDocument.Parse(jsonString);

        string imageurl = data.RootElement.GetProperty("query")
            .GetProperty("pages").EnumerateObject().First().Value.GetProperty("imageinfo")[0].GetProperty("url").GetString();

        return imageurl;
    }

    private class QueryResponseInfo
    {
        public QueryDataInfo query { get; set;}
    }

    private class QueryResponseWContinue
    {
        public QueryDataInfo query { get; set; }
        public Dictionary<string, string> @continue { get; set; }
    }

    private class QueryDataInfo
    {
        public Dictionary<string, ArticlePageData> pages { get; set; }
    }

}

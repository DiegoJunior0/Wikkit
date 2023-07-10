using System;
using System.Collections.Generic;
using System.Text.Json;
using Wikkit.Data;

namespace Wikkit.Services;

public class BookmarkService
{
    private List<ArticlePageData> articles = new();

    private const string bookmarkspref = "bookmarks";

    public BookmarkService()
    {
        this.articles = new();

        string articlesStr = Preferences.Default.Get(bookmarkspref, "");

        if (articlesStr == "") return;

        try
        {
            articles = JsonSerializer.Deserialize<List<ArticlePageData>>(articlesStr);
        }
        catch (Exception)
        {

        }

    }

    public void AddBookMark(ArticlePageData article)
    {
        articles.Insert(0, article);

        SaveBookmarks();
    }

    public void RemoveBookmark(int pageID)
    {
        articles.RemoveAll(a => a.pageid == pageID);
        
        SaveBookmarks();
    }

    public bool IsBookMark(int articlePageID)
    {
        return articles.Exists(a => a.pageid == articlePageID);
    }

    private void SaveBookmarks()
    {
        Preferences.Set(bookmarkspref, JsonSerializer.Serialize(articles));
    }

    public void ClearAll()
    {
        articles.Clear();

        SaveBookmarks();
    }

    public (List<ArticlePageData>, int remain) GetBookmarks(int index, int count)
    {
        if (index > articles.Count - 1) return (new List<ArticlePageData>(), 0);

        List<ArticlePageData> newArticles = new();

        for (int i = index; i < index + count & i < articles.Count; i++)
        {
            ArticlePageData page = articles[i];

            newArticles.Add(page);
        }

        return (newArticles, Math.Clamp(articles.Count - (index + count), 0, newArticles.Count));
    }

}

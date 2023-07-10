using System;
using System.Collections.Generic;
using System.Text.Json;
using Wikkit.Data;

namespace Wikkit.Services;

public class ArticleHistoryService
{
    private List<ArticlePageData> _articles = new();

    private const int HistoryCount = 50;
    private const string historypref = "history";

    public ArticleHistoryService()
    {
        LoadHistory();
    }

    private void LoadHistory()
    {
        _articles.Clear();

        string articlesStr = Preferences.Default.Get(historypref, "");

        if (articlesStr == "") return;

        try
        {
            _articles = JsonSerializer.Deserialize<List<ArticlePageData>>(articlesStr);
        }
        catch (Exception)
        {

        }
        
    }

    private void SaveHistory()
    {
        Preferences.Set(historypref, JsonSerializer.Serialize(_articles));
    }

    public void AddToHistory(ArticlePageData articleData)
    {

        _articles.RemoveAll(a => a.pageid == articleData.pageid);

        _articles.Insert(0, articleData);

        if (_articles.Count > HistoryCount)
        {
            _articles.RemoveAt(_articles.Count - 1);
        }

        SaveHistory();

    }

    public (List<ArticlePageData>, int remain) GetHistory(int index, int count)
    {
        if (index > _articles.Count - 1) return (new List<ArticlePageData>(), 0);

        List<ArticlePageData> articles = new();

        for (int i = index; i < index + count & i < _articles.Count; i++)
        {
            ArticlePageData page = _articles[i];

            articles.Add(page);
        }

        return (articles, Math.Clamp(_articles.Count - (index + count), 0, articles.Count));
    }

    public void ClearHistory()
    {
        _articles.Clear();

        Preferences.Set(historypref, "");
    }

}

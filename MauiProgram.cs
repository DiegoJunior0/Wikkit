using Microsoft.Extensions.Logging;
using Wikkit.Data;
using Wikkit.Services;
using BlazorBootstrap;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Wikkit;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("Roboto-Regular.ttf", "RobotoRegular");
			});

		builder.Services.AddMauiBlazorWebView();
        builder.Services.AddBlazorBootstrap();


#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();

#endif
        builder.Logging.AddDebug();
        builder.Services.AddScoped<Logger<WikipediaDataService>>();
        builder.Services.AddSingleton<WikipediaDataService>().AddScoped<HttpClient>();
        builder.Services.AddScoped<SettingsService>();
		builder.Services.AddScoped<ArticleHistoryService>();
		builder.Services.AddScoped<BookmarkService>();

        return builder.Build();
	}
}

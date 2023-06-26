using Microsoft.Extensions.Logging;
using Wikkit.Data;
using BlazorBootstrap;
using Microsoft.AspNetCore.Components.Web;
using Wikkit.Services;

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
		builder.Logging.AddDebug();
#endif

        builder.Services.AddSingleton<WikipediaDataService>().AddScoped<HttpClient>();

        return builder.Build();
	}
}

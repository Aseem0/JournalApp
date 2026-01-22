using Microsoft.Extensions.Logging;
using JournalApp.Services;
using MudBlazor.Services;

namespace JournalApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {

            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "journal.db");
            Console.WriteLine($"Database path: {dbPath}");

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
    		builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif
            builder.Services.AddSingleton<DatabaseService>();
            builder.Services.AddSingleton<ThemeService>();
            builder.Services.AddMudServices();
            return builder.Build();
        }
    }
}

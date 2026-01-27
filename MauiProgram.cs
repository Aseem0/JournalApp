using Microsoft.Extensions.Logging;
using JournalApp.Services;
using MudBlazor.Services;
using CommunityToolkit.Maui;
using QuestPDF.Infrastructure;

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
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            // Initialize QuestPDF License
            QuestPDF.Settings.License = LicenseType.Community;

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
    		builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif
            builder.Services.AddSingleton<DatabaseService>();
            builder.Services.AddSingleton<ThemeService>();
            builder.Services.AddSingleton<SecurityService>();
            builder.Services.AddSingleton<PdfExportService>();
            builder.Services.AddSingleton(CommunityToolkit.Maui.Storage.FileSaver.Default);
            builder.Services.AddMudServices();
            return builder.Build();
        }
    }
}

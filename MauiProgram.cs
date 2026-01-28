using Microsoft.Extensions.Logging;
using JournalApp.Services;
using MudBlazor.Services;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Storage;
using QuestPDF.Infrastructure;

namespace JournalApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
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

        // Add Services
        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddMudServices();
        
        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddSingleton<ThemeService>();
        builder.Services.AddSingleton<SecurityService>();
        builder.Services.AddSingleton<PdfExportService>();
        builder.Services.AddSingleton(FileSaver.Default);

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}

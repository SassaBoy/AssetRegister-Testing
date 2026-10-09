using Microsoft.Extensions.Logging;
using MobileApp.Services;
using MobileApp.ViewModels;
using MobileApp.Views;
using ZXing.Net.Maui.Controls;

namespace MobileApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseBarcodeReader();   // turns on the camera scanner (ZXing.Net.MAUI)

        // Services: one copy for the whole app
        builder.Services.AddSingleton<SettingsService>();
        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddSingleton<ApiProvider>();
        builder.Services.AddSingleton<SyncService>();

        // Screens and their view models: a new one each time
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<SetupPage>();
        builder.Services.AddTransient<SetupViewModel>();
        builder.Services.AddTransient<ScanAssetPage>();
        builder.Services.AddTransient<ScanAssetViewModel>();
        builder.Services.AddTransient<NewAssetPage>();
        builder.Services.AddTransient<NewAssetViewModel>();
        builder.Services.AddTransient<SyncPage>();
        builder.Services.AddTransient<SyncViewModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }
}

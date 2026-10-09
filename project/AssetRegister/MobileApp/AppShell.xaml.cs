using MobileApp.Views;

namespace MobileApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Tell Shell which page belongs to which name, so we can use GoToAsync(nameof(...))
        Routing.RegisterRoute(nameof(SetupPage), typeof(SetupPage));
        Routing.RegisterRoute(nameof(ScanAssetPage), typeof(ScanAssetPage));
        Routing.RegisterRoute(nameof(NewAssetPage), typeof(NewAssetPage));
        Routing.RegisterRoute(nameof(SyncPage), typeof(SyncPage));
    }
}

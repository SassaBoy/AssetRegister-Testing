using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MobileApp.Services;
using MobileApp.Views;

namespace MobileApp.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private readonly DatabaseService _db;
    private readonly SettingsService _settings;
    private readonly SyncService _sync;

    // [ObservableProperty] creates the public property "WaitingText" for us and tells the screen when it changes
    [ObservableProperty]
    private string waitingText = "";

    public bool IsSetUp => _settings.IsSetUp;

    public HomeViewModel(DatabaseService db, SettingsService settings, SyncService sync)
    {
        _db = db;
        _settings = settings;
        _sync = sync;
    }

    // Called every time the Home screen shows
    public async Task LoadAsync()
    {
        int waiting = await _db.CountWaiting();
        WaitingText = waiting == 1 ? "1 new asset waiting to sync" : $"{waiting} new assets waiting to sync";
    }

    // Sync when the app opens (only if there is internet)
    public async Task AutoSyncAsync()
    {
        if (!_settings.IsSetUp) return;
        if (Connectivity.Current.NetworkAccess == NetworkAccess.None) return;

        await _sync.SyncAsync();
        await LoadAsync();
    }

    [RelayCommand]
    private async Task ScanAsset() => await Shell.Current.GoToAsync(nameof(ScanAssetPage));

    [RelayCommand]
    private async Task NewAsset() => await Shell.Current.GoToAsync(nameof(NewAssetPage));

    [RelayCommand]
    private async Task OpenSync() => await Shell.Current.GoToAsync(nameof(SyncPage));

    [RelayCommand]
    private async Task OpenSetup() => await Shell.Current.GoToAsync(nameof(SetupPage));
}

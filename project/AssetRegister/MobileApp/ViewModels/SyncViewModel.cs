using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MobileApp.Models;
using MobileApp.Services;

namespace MobileApp.ViewModels;

public partial class SyncViewModel : ObservableObject
{
    private readonly DatabaseService _db;
    private readonly SettingsService _settings;
    private readonly SyncService _sync;

    [ObservableProperty] private string lastSyncText = "";
    [ObservableProperty] private string message = "";
    [ObservableProperty] private bool isBusy;

    public ObservableCollection<NewAsset> NewAssets { get; } = new();

    public SyncViewModel(DatabaseService db, SettingsService settings, SyncService sync)
    {
        _db = db;
        _settings = settings;
        _sync = sync;
    }

    // Reloads the list and the "last sync" text
    public async Task LoadAsync()
    {
        var last = _settings.LastSync;
        LastSyncText = last == null ? "Last sync: never" : $"Last sync: {last:dd MMM yyyy HH:mm}";

        NewAssets.Clear();
        foreach (var item in await _db.GetNewAssets())
            NewAssets.Add(item);
    }

    [RelayCommand]
    private async Task SyncNow()
    {
        IsBusy = true;
        Message = "Syncing...";

        Message = await _sync.SyncAsync();
        await LoadAsync();

        IsBusy = false;
    }
}

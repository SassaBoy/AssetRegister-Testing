
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

    [ObservableProperty]
    private string lastSyncText = "";

    [ObservableProperty]
    private string message = "";

    [ObservableProperty]
    private bool isBusy;

    public ObservableCollection<NewAsset> NewAssets { get; } = new();

    public SyncViewModel(
        DatabaseService db,
        SettingsService settings,
        SyncService sync)
    {
        _db = db;
        _settings = settings;
        _sync = sync;
    }

    public async Task LoadAsync()
    {
        var last = _settings.LastSync;

        LastSyncText = last == null
            ? "Last sync: never"
            : $"Last sync: {last:dd MMM yyyy HH:mm}";

        NewAssets.Clear();

        foreach (var item in await _db.GetNewAssets())
        {
            NewAssets.Add(item);
        }
    }

    [RelayCommand]
    private async Task SyncNow()
    {
        if (IsBusy)
            return;

        IsBusy = true;
        Message = "Syncing...";

        try
        {
            Message = await _sync.SyncAsync();
        }
        catch (Exception ex)
        {
            Message = "Sync failed: " + ex.Message;
        }
        finally
        {
            try
            {
                await LoadAsync();
            }
            catch (Exception ex)
            {
                Message += Environment.NewLine
                    + "Could not refresh the asset list: "
                    + ex.Message;
            }

            IsBusy = false;
        }
    }
}

using MobileApp.Services;

namespace MobileApp.Services;

// Does the sync: first upload the new assets, then download the classes and assets from GP.
public class SyncService
{
    private readonly DatabaseService _db;
    private readonly SettingsService _settings;
    private readonly ApiProvider _apiProvider;

    // stops two syncs from running at the same time
    private bool _isRunning;

    public SyncService(DatabaseService db, SettingsService settings, ApiProvider apiProvider)
    {
        _db = db;
        _settings = settings;
        _apiProvider = apiProvider;
    }

    // Returns a short message for the screen
    public async Task<string> SyncAsync()
    {
        if (_isRunning) return "Sync is already running";
        if (!_settings.IsSetUp) return "Please finish the Setup first";
        if (Connectivity.Current.NetworkAccess == NetworkAccess.None) return "No connection. Your assets are safe on the phone.";

        _isRunning = true;
        try
        {
            var api = _apiProvider.Get();

            // ---- 1. Upload ----
            int sent = 0;
            int failed = 0;
            var waiting = await _db.GetWaitingAssets();

            foreach (var asset in waiting)
            {
                try
                {
                    var result = await api.CreateAssetAsync(asset);

                    // Only mark as synced when GP gave us an Asset ID
                    if (result.Success && !string.IsNullOrEmpty(result.GpAssetId))
                    {
                        asset.Synced = true;
                        asset.GpAssetId = result.GpAssetId;
                        asset.Error = null;
                        sent++;
                    }
                    else
                    {
                        asset.Error = result.Error ?? "GP did not return an Asset ID";
                        failed++;
                    }
                }
                catch (Exception ex)
                {
                    // Network problem: keep the record so the user can retry
                    asset.Error = ex.Message;
                    failed++;
                }

                await _db.UpdateNewAsset(asset);
            }

            // ---- 2. Download ----
            // We get both lists first. The old copy is only replaced if both downloads worked.
            var classes = await api.GetClassesAsync();
            var assets = await api.GetAssetsAsync();
            await _db.ReplaceClasses(classes);
            await _db.ReplaceAssets(assets);

            _settings.LastSync = DateTime.Now;

            return $"Sync done. Sent {sent}, failed {failed}, downloaded {assets.Count} assets.";
        }
        catch (Exception ex)
        {
            return "Sync failed: " + ex.Message;
        }
        finally
        {
            _isRunning = false;
        }
    }
}

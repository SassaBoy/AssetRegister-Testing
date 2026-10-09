using MobileApp.Models;

namespace MobileApp.Services;

// Both the fake service and the real one follow this shape,
// so the rest of the app does not care which one it is talking to.
public interface IAssetApi
{
    // Returns "Connected" or the error text
    Task<string> PingAsync();

    Task<List<AssetClass>> GetClassesAsync();
    Task<List<Asset>> GetAssetsAsync();
    Task<CreateResult> CreateAssetAsync(NewAsset asset);
}

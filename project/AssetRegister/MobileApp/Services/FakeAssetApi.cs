using MobileApp.Models;

namespace MobileApp.Services;

// A pretend service for Step 1, so the app can be built and tested before GP is ready.
// It returns sample assets and always says "saved".
public class FakeAssetApi : IAssetApi
{
    private static int _counter = 1;

    public async Task<string> PingAsync()
    {
        await Task.Delay(300);
        return "Connected (fake service)";
    }

    public async Task<List<AssetClass>> GetClassesAsync()
    {
        await Task.Delay(300);
        return new List<AssetClass>
        {
            new() { Code = "COMPUTERS", Description = "Computer equipment" },
            new() { Code = "FURNITURE", Description = "Office furniture" },
            new() { Code = "VEHICLES",  Description = "Company vehicles" },
            new() { Code = "TOOLS",     Description = "Tools and machinery" }
        };
    }

    public async Task<List<Asset>> GetAssetsAsync()
    {
        await Task.Delay(300);
        return new List<Asset>
        {
            new() { AssetId = "FAKE000001", Barcode = "100001", Description = "Dell Laptop",      Class = "COMPUTERS", Custodian = "Anna",  SerialNumber = "DL-8841" },
            new() { AssetId = "FAKE000002", Barcode = "100002", Description = "Office Desk",      Class = "FURNITURE", Custodian = "Peter", SerialNumber = "" },
            new() { AssetId = "FAKE000003", Barcode = "100003", Description = "Toyota Hilux",     Class = "VEHICLES",  Custodian = "Sam",   SerialNumber = "AHT-22091" },
            new() { AssetId = "FAKE000004", Barcode = "100004", Description = "Cordless Drill",   Class = "TOOLS",     Custodian = "Maria", SerialNumber = "CD-5521" },
            new() { AssetId = "FAKE000005", Barcode = "100005", Description = "Office Chair",     Class = "FURNITURE", Custodian = "Anna",  SerialNumber = "" },
            new() { AssetId = "FAKE000006", Barcode = "100006", Description = "HP Printer",       Class = "COMPUTERS", Custodian = "Peter", SerialNumber = "HP-30417" }
        };
    }

    public async Task<CreateResult> CreateAssetAsync(NewAsset asset)
    {
        await Task.Delay(400);
        return new CreateResult { Success = true, GpAssetId = "FAKE" + (1000 + _counter++) };
    }
}

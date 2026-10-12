using System.Text.Json;
using Microsoft.Maui.Storage;
using MobileApp.Models;

namespace MobileApp.Services;

// Demo-only service. It does not connect to Dynamics GP.
public class FakeAssetApi : IAssetApi
{
    private const string AssetsKey = "FakeAssetApi.Assets";
    private static readonly object LockObject = new();

    private static readonly List<AssetClass> Classes = new()
    {
        new() { Code = "COMPUTER", Description = "Computer equipment" },
        new() { Code = "FURNITURE", Description = "Office furniture" },
        new() { Code = "VEHICLE", Description = "Company vehicles" },
        new() { Code = "EQUIPMENT", Description = "Equipment and machinery" }
    };

    private static List<Asset> LoadAssets()
    {
        lock (LockObject)
        {
            var saved = Preferences.Default.Get(AssetsKey, "");

            if (!string.IsNullOrWhiteSpace(saved))
            {
                try
                {
                    var existing = JsonSerializer.Deserialize<List<Asset>>(saved);
                    if (existing != null)
                        return existing;
                }
                catch (JsonException)
                {
                    // Re-create the demo data if saved demo data is unreadable.
                }
            }

            var samples = new List<Asset>
            {
                new() { AssetId = "FAKE000001", Barcode = "100001", Description = "Dell Laptop", Class = "COMPUTER", Custodian = "Anna", SerialNumber = "DL-8841" },
                new() { AssetId = "FAKE000002", Barcode = "100002", Description = "Office Desk", Class = "FURNITURE", Custodian = "Peter" },
                new() { AssetId = "FAKE000003", Barcode = "100003", Description = "Toyota Hilux", Class = "VEHICLE", Custodian = "Sam", SerialNumber = "AHT-22091" },
                new() { AssetId = "FAKE000004", Barcode = "100004", Description = "Workshop Equipment", Class = "EQUIPMENT", Custodian = "Maria", SerialNumber = "EQ-5521" },
                new() { AssetId = "FAKE000005", Barcode = "100005", Description = "Office Chair", Class = "FURNITURE", Custodian = "Anna" },
                new() { AssetId = "FAKE000006", Barcode = "100006", Description = "HP Printer", Class = "COMPUTER", Custodian = "Peter", SerialNumber = "HP-30417" }
            };

            SaveAssets(samples);
            return samples;
        }
    }

    private static void SaveAssets(List<Asset> assets)
    {
        Preferences.Default.Set(AssetsKey, JsonSerializer.Serialize(assets));
    }

    public Task<string> PingAsync()
    {
        return Task.FromResult("Connected (fake service)");
    }

    public Task<List<AssetClass>> GetClassesAsync()
    {
        return Task.FromResult(Classes.Select(c => new AssetClass
        {
            Code = c.Code,
            Description = c.Description
        }).ToList());
    }

    public Task<List<Asset>> GetAssetsAsync()
    {
        var assets = LoadAssets();

        lock (LockObject)
        {
            return Task.FromResult(assets.Select(a => new Asset
            {
                AssetId = a.AssetId,
                Barcode = a.Barcode,
                Description = a.Description,
                Class = a.Class,
                Custodian = a.Custodian,
                SerialNumber = a.SerialNumber
            }).ToList());
        }
    }

    public Task<CreateResult> CreateAssetAsync(NewAsset asset)
    {
        if (string.IsNullOrWhiteSpace(asset.Barcode) ||
            string.IsNullOrWhiteSpace(asset.Description) ||
            string.IsNullOrWhiteSpace(asset.Class))
        {
            return Task.FromResult(new CreateResult
            {
                Success = false,
                Error = "Barcode, description and class are required."
            });
        }

        lock (LockObject)
        {
            var assets = LoadAssets();
            var barcode = asset.Barcode.Trim();

            if (assets.Any(a => string.Equals(
                a.Barcode.Trim(), barcode, StringComparison.OrdinalIgnoreCase)))
            {
                return Task.FromResult(new CreateResult
                {
                    Success = false,
                    Error = "This barcode is already used by another asset."
                });
            }

            var selectedClass = Classes.FirstOrDefault(c =>
                string.Equals(c.Code, asset.Class.Trim(), StringComparison.OrdinalIgnoreCase));

            if (selectedClass == null)
            {
                return Task.FromResult(new CreateResult
                {
                    Success = false,
                    Error = "The selected asset class is not available."
                });
            }

            var highestNumber = assets
                .Select(a => a.AssetId)
                .Where(id => id.StartsWith("FAKE", StringComparison.OrdinalIgnoreCase))
                .Select(id => int.TryParse(id[4..], out var number) ? number : 0)
                .DefaultIfEmpty(0)
                .Max();

            var newId = $"FAKE{highestNumber + 1:D6}";

            assets.Add(new Asset
            {
                AssetId = newId,
                Barcode = barcode,
                Description = asset.Description.Trim(),
                Class = selectedClass.Code,
                Custodian = asset.Custodian?.Trim() ?? "",
                SerialNumber = asset.SerialNumber?.Trim() ?? ""
            });

            SaveAssets(assets);

            return Task.FromResult(new CreateResult
            {
                Success = true,
                GpAssetId = newId
            });
        }
    }
}

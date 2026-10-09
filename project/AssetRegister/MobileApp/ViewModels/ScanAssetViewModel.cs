using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MobileApp.Services;
using MobileApp.Views;

namespace MobileApp.ViewModels;

public partial class ScanAssetViewModel : ObservableObject
{
    private readonly DatabaseService _db;

    [ObservableProperty] private string scannedCode = "";
    [ObservableProperty] private string message = "Scan a barcode to see the asset.";

    // The details shown on screen
    [ObservableProperty] private bool hasResult;
    [ObservableProperty] private string assetId = "";
    [ObservableProperty] private string description = "";
    [ObservableProperty] private string assetClass = "";
    [ObservableProperty] private string custodian = "";
    [ObservableProperty] private string serialNumber = "";

    // Shows the "Register as new asset" button when the barcode is unknown
    [ObservableProperty] private bool canRegister;

    public ScanAssetViewModel(DatabaseService db)
    {
        _db = db;
    }

    // Called with the scanned (or typed) barcode
    public async Task LookupAsync(string code)
    {
        code = code.Trim();
        ScannedCode = code;
        HasResult = false;
        CanRegister = false;

        // 1. Is it an asset that is already in GP?
        var asset = await _db.FindAssetByBarcode(code);
        if (asset != null)
        {
            Show(asset.AssetId, asset.Description, asset.Class, asset.Custodian, asset.SerialNumber);
            Message = "Asset found";
            return;
        }

        // 2. Is it one we registered on this phone and not yet synced?
        var newAsset = await _db.FindNewAssetByBarcode(code);
        if (newAsset != null)
        {
            string id = newAsset.Synced ? newAsset.GpAssetId ?? "" : "Not in GP yet";
            Show(id, newAsset.Description, newAsset.Class, newAsset.Custodian, newAsset.SerialNumber);
            Message = "Registered on this phone";
            return;
        }

        // 3. Unknown barcode
        Message = $"No asset found for barcode {code}.";
        CanRegister = true;
    }

    private void Show(string id, string desc, string cls, string cust, string serial)
    {
        AssetId = id;
        Description = desc;
        AssetClass = cls;
        Custodian = cust;
        SerialNumber = serial;
        HasResult = true;
    }

    [RelayCommand]
    private async Task RegisterAsNew()
    {
        await Shell.Current.GoToAsync($"{nameof(NewAssetPage)}?barcode={Uri.EscapeDataString(ScannedCode)}");
    }
}

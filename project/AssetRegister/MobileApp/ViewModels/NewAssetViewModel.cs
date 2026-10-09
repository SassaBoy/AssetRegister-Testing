using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MobileApp.Models;
using MobileApp.Services;

namespace MobileApp.ViewModels;

// "barcode" can be passed in when we come from the Scan screen
[QueryProperty(nameof(Barcode), "barcode")]
public partial class NewAssetViewModel : ObservableObject
{
    private readonly DatabaseService _db;
    private readonly SettingsService _settings;

    [ObservableProperty] private string barcode = "";
    [ObservableProperty] private string description = "";
    [ObservableProperty] private string custodian = "";
    [ObservableProperty] private string serialNumber = "";
    [ObservableProperty] private AssetClass? selectedClass;

    public List<AssetClass> Classes { get; private set; } = new();

    public NewAssetViewModel(DatabaseService db, SettingsService settings)
    {
        _db = db;
        _settings = settings;
    }

    // Fills the class picker from the local copy of the GP classes
    public async Task LoadClassesAsync()
    {
        Classes = await _db.GetClasses();
        OnPropertyChanged(nameof(Classes));
    }

    [RelayCommand]
    private async Task Save()
    {
        // Check the required fields
        if (string.IsNullOrWhiteSpace(Barcode) || string.IsNullOrWhiteSpace(Description) || SelectedClass == null)
        {
            await Shell.Current.DisplayAlert("New asset", "Barcode, description and class are required.", "OK");
            return;
        }

        var code = Barcode.Trim();

        // The barcode must not be used already (in GP or on this phone)
        if (await _db.FindAssetByBarcode(code) != null || await _db.FindNewAssetByBarcode(code) != null)
        {
            await Shell.Current.DisplayAlert("New asset", "This barcode is already used by another asset.", "OK");
            return;
        }

        // Always save to the phone first. This works with no internet.
        var asset = new NewAsset
        {
            Barcode = code,
            Description = Description.Trim(),
            Class = SelectedClass.Code,
            Custodian = Custodian.Trim(),
            SerialNumber = SerialNumber.Trim(),
            CreatedBy = _settings.UserName,
            CreatedAt = DateTime.Now,
            Synced = false
        };
        await _db.SaveNewAsset(asset);

        await Shell.Current.DisplayAlert("Saved", "The asset is saved on the phone. Sync to send it to GP.", "OK");
        await Shell.Current.GoToAsync("..");
    }
}

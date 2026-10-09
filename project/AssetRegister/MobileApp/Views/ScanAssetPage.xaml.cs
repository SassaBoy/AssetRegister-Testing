using MobileApp.ViewModels;

namespace MobileApp.Views;

public partial class ScanAssetPage : ContentPage
{
    private readonly ScanAssetViewModel _vm;

    public ScanAssetPage(ScanAssetViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    // Opens the camera. When a code is read, the scanner page calls us back.
    private async void OnScanClicked(object? sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new ScannerPage(async code => await _vm.LookupAsync(code)));
    }

    // For damaged labels: the user types the code by hand
    private async void OnTypeCodeClicked(object? sender, EventArgs e)
    {
        string? code = await DisplayPromptAsync("Type code", "Enter the barcode number");
        if (!string.IsNullOrWhiteSpace(code))
            await _vm.LookupAsync(code);
    }
}

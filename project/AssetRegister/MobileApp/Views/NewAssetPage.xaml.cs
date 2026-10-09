using MobileApp.ViewModels;

namespace MobileApp.Views;

public partial class NewAssetPage : ContentPage
{
    private readonly NewAssetViewModel _vm;

    public NewAssetPage(NewAssetViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadClassesAsync();
    }

    private async void OnScanClicked(object? sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new ScannerPage(code => _vm.Barcode = code));
    }
}

using MobileApp.ViewModels;

namespace MobileApp.Views;

public partial class SyncPage : ContentPage
{
    private readonly SyncViewModel _vm;

    public SyncPage(SyncViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }
}

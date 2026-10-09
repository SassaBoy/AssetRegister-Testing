using MobileApp.ViewModels;

namespace MobileApp.Views;

public partial class HomePage : ContentPage
{
    private readonly HomeViewModel _vm;
    private bool _checkedSetup;
    private bool _triedAutoSync;

    public HomePage(HomeViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // First run: send the user to the Setup screen
        if (!_vm.IsSetUp && !_checkedSetup)
        {
            _checkedSetup = true;
            await Shell.Current.GoToAsync(nameof(SetupPage));
            return;
        }

        await _vm.LoadAsync();

        // Sync once when the app opens
        if (!_triedAutoSync)
        {
            _triedAutoSync = true;
            await _vm.AutoSyncAsync();
        }
    }
}

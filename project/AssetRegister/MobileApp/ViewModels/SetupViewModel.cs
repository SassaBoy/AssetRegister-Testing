using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MobileApp.Services;

namespace MobileApp.ViewModels;

public partial class SetupViewModel : ObservableObject
{
    private readonly SettingsService _settings;
    private readonly ApiProvider _apiProvider;

    [ObservableProperty] private string serviceAddress = "";
    [ObservableProperty] private string accessKey = "";
    [ObservableProperty] private string userName = "";
    [ObservableProperty] private bool useFakeService;
    [ObservableProperty] private string testResult = "";

    public SetupViewModel(SettingsService settings, ApiProvider apiProvider)
    {
        _settings = settings;
        _apiProvider = apiProvider;

        // show what was saved last time
        ServiceAddress = settings.ServiceAddress;
        AccessKey = settings.AccessKey;
        UserName = settings.UserName;
        UseFakeService = settings.UseFakeService;
    }

    private void SaveToSettings()
    {
        _settings.ServiceAddress = ServiceAddress.Trim();
        _settings.AccessKey = AccessKey.Trim();
        _settings.UserName = UserName.Trim();
        _settings.UseFakeService = UseFakeService;
    }

    [RelayCommand]
    private async Task TestConnection()
    {
        SaveToSettings();
        TestResult = "Testing...";
        TestResult = await _apiProvider.Get().PingAsync();
    }

    [RelayCommand]
    private async Task Save()
    {
        if (string.IsNullOrWhiteSpace(UserName))
        {
            await Shell.Current.DisplayAlert("Setup", "Please type your name.", "OK");
            return;
        }
        if (!UseFakeService && (string.IsNullOrWhiteSpace(ServiceAddress) || string.IsNullOrWhiteSpace(AccessKey)))
        {
            await Shell.Current.DisplayAlert("Setup", "Please type the service address and the access key.", "OK");
            return;
        }

        SaveToSettings();
        await Shell.Current.GoToAsync("..");   // go back to Home
    }
}

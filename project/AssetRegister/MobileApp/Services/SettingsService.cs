namespace MobileApp.Services;

// Keeps the Setup screen values on the phone.
// (Preferences is fine for a test project. For a real product the access key
//  should go into SecureStorage instead.)
public class SettingsService
{
    public string ServiceAddress
    {
        get => Preferences.Get(nameof(ServiceAddress), "");
        set => Preferences.Set(nameof(ServiceAddress), value);
    }

    public string AccessKey
    {
        get => Preferences.Get(nameof(AccessKey), "");
        set => Preferences.Set(nameof(AccessKey), value);
    }

    public string UserName
    {
        get => Preferences.Get(nameof(UserName), "");
        set => Preferences.Set(nameof(UserName), value);
    }

    // While we build the app, we talk to a fake service. Switch it off in Setup when GP is ready.
    public bool UseFakeService
    {
        get => Preferences.Get(nameof(UseFakeService), true);
        set => Preferences.Set(nameof(UseFakeService), value);
    }

    public DateTime? LastSync
    {
        get
        {
            var text = Preferences.Get(nameof(LastSync), "");
            return DateTime.TryParse(text, out var date) ? date : null;
        }
        set => Preferences.Set(nameof(LastSync), value?.ToString("o") ?? "");
    }

    // The user has finished the first-run setup
    public bool IsSetUp
    {
        get
        {
            if (string.IsNullOrWhiteSpace(UserName)) return false;
            if (UseFakeService) return true;
            return !string.IsNullOrWhiteSpace(ServiceAddress) && !string.IsNullOrWhiteSpace(AccessKey);
        }
    }
}

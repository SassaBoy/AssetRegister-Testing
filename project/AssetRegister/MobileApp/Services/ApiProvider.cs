namespace MobileApp.Services;

// Hands out the fake or the real service depending on the Setup switch.
public class ApiProvider
{
    private readonly SettingsService _settings;

    public ApiProvider(SettingsService settings)
    {
        _settings = settings;
    }

    public IAssetApi Get()
    {
        if (_settings.UseFakeService)
            return new FakeAssetApi();

        return new RealAssetApi(_settings);
    }
}

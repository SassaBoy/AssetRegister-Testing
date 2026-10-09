using System.Net.Http.Json;
using MobileApp.Models;

namespace MobileApp.Services;

// Talks to the real eConnect service over HTTP(S).
public class RealAssetApi : IAssetApi
{
    private readonly SettingsService _settings;

    public RealAssetApi(SettingsService settings)
    {
        _settings = settings;
    }

    // Makes a HttpClient that already knows the address and the access key
    private HttpClient MakeClient()
    {
        var client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(30);
        client.BaseAddress = new Uri(_settings.ServiceAddress.TrimEnd('/') + "/api/");
        client.DefaultRequestHeaders.Add("X-Api-Key", _settings.AccessKey);
        return client;
    }

    public async Task<string> PingAsync()
    {
        try
        {
            using var client = MakeClient();
            var response = await client.GetAsync("ping");

            if (response.IsSuccessStatusCode) return "Connected";
            if ((int)response.StatusCode == 401) return "Wrong access key";
            return $"Service answered with error {(int)response.StatusCode}";
        }
        catch (Exception ex)
        {
            return "Could not connect: " + ex.Message;
        }
    }

    public async Task<List<AssetClass>> GetClassesAsync()
    {
        using var client = MakeClient();
        var classes = await client.GetFromJsonAsync<List<AssetClass>>("classes");
        return classes ?? new List<AssetClass>();
    }

    public async Task<List<Asset>> GetAssetsAsync()
    {
        var all = new List<Asset>();
        int page = 1;
        const int pageSize = 500;

        using var client = MakeClient();

        // keep asking for pages until we get a short page back
        while (true)
        {
            var batch = await client.GetFromJsonAsync<List<Asset>>($"assets?page={page}&pageSize={pageSize}");
            if (batch == null || batch.Count == 0) break;

            all.AddRange(batch);
            if (batch.Count < pageSize) break;
            page++;
        }
        return all;
    }

    public async Task<CreateResult> CreateAssetAsync(NewAsset asset)
    {
        using var client = MakeClient();

        // send only the fields GP needs
        var body = new
        {
            asset.Barcode,
            asset.Description,
            asset.Class,
            asset.Custodian,
            asset.SerialNumber
        };

        var response = await client.PostAsJsonAsync("assets", body);
        if (!response.IsSuccessStatusCode)
        {
            return new CreateResult { Success = false, Error = $"Service error {(int)response.StatusCode}" };
        }

        var result = await response.Content.ReadFromJsonAsync<CreateResult>();
        return result ?? new CreateResult { Success = false, Error = "Empty answer from service" };
    }
}

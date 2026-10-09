namespace EconnectService;

public class GpWriter
{
    private readonly GpReader _reader;
    private readonly bool _useDemoData;
    private readonly string _assetIdPrefix;

    public GpWriter(GpReader reader, IConfiguration config)
    {
        _reader = reader;
        _useDemoData = config.GetValue<bool>("UseDemoData");
        _assetIdPrefix = config["AssetIdPrefix"] ?? "APP";
    }

    public Task<CreateResult> CreateAssetAsync(NewAssetRequest req)
    {
        if (!_useDemoData)
        {
            return Task.FromResult(new CreateResult
            {
                Success = false,
                Error = "Dynamics GP eConnect is not configured. No asset was created."
            });
        }

        return _reader.CreateDemoAssetAsync(req, _assetIdPrefix);
    }
}

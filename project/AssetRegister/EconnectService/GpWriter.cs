namespace EconnectService;

// Temporary development implementation.
// Does not create assets in Dynamics GP.
public class GpWriter
{
    public GpWriter(GpReader reader, IConfiguration config)
    {
    }

    public Task<CreateResult> CreateAssetAsync(NewAssetRequest req)
    {
        return Task.FromResult(new CreateResult
        {
            Success = false,
            Error = "Dynamics GP eConnect is not configured. No asset was created."
        });
    }
}

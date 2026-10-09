namespace MobileApp.Models;

// The service's answer when we ask it to create an asset in GP
public class CreateResult
{
    public bool Success { get; set; }
    public string? GpAssetId { get; set; }
    public string? Error { get; set; }
}

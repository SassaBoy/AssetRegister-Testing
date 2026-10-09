namespace EconnectService;

// An asset as we send it to the phone.
public class AssetDto
{
    public string AssetId { get; set; } = "";
    public string Barcode { get; set; } = "";
    public string Description { get; set; } = "";
    public string Class { get; set; } = "";
    public string Custodian { get; set; } = "";
    public string SerialNumber { get; set; } = "";
}

public class ClassDto
{
    public string Code { get; set; } = "";
    public string Description { get; set; } = "";
}

// What the phone sends when it wants a new asset created in GP.
public class NewAssetRequest
{
    public string Barcode { get; set; } = "";
    public string Description { get; set; } = "";
    public string Class { get; set; } = "";
    public string Custodian { get; set; } = "";
    public string SerialNumber { get; set; } = "";
}

// What we send back after trying to create the asset.
public class CreateResult
{
    public bool Success { get; set; }
    public string? GpAssetId { get; set; }
    public string? Error { get; set; }
}

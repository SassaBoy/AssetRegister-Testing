using SQLite;

namespace MobileApp.Models;

// An asset copied from GP. The phone only reads these, never changes them.
public class Asset
{
    [PrimaryKey]
    public string AssetId { get; set; } = "";

    [Indexed]
    public string Barcode { get; set; } = "";

    public string Description { get; set; } = "";
    public string Class { get; set; } = "";
    public string Custodian { get; set; } = "";
    public string SerialNumber { get; set; } = "";
}

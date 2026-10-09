using SQLite;

namespace MobileApp.Models;

// An asset that was registered on the phone and is waiting to go to GP.
public class NewAsset
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Barcode { get; set; } = "";
    public string Description { get; set; } = "";
    public string Class { get; set; } = "";
    public string Custodian { get; set; } = "";
    public string SerialNumber { get; set; } = "";

    public string CreatedBy { get; set; } = "";
    public DateTime CreatedAt { get; set; }

    public bool Synced { get; set; }
    public string? GpAssetId { get; set; }
    public string? Error { get; set; }

    // Text for the Sync screen: Waiting, In GP (with id) or Failed (with reason)
    [Ignore]
    public string StatusText
    {
        get
        {
            if (Synced) return $"In GP ({GpAssetId})";
            if (!string.IsNullOrEmpty(Error)) return $"Failed: {Error}";
            return "Waiting";
        }
    }
}

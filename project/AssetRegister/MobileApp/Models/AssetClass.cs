using SQLite;

namespace MobileApp.Models;

// An asset class copied from GP (for example "COMPUTERS")
public class AssetClass
{
    [PrimaryKey]
    public string Code { get; set; } = "";

    public string Description { get; set; } = "";

    // Shown in the class picker
    [Ignore]
    public string Display => $"{Code} - {Description}";
}


using Microsoft.Data.SqlClient;

namespace EconnectService;

public class GpReader
{
    private readonly bool _useDemoData;
    public string ConnectionString { get; }

    private static readonly List<ClassDto> DemoClasses = new()
    {
        new() { Code = "COMPUTER", Description = "Computer Equipment" },
        new() { Code = "FURNITURE", Description = "Office Furniture" },
        new() { Code = "VEHICLE", Description = "Vehicles" },
        new() { Code = "EQUIPMENT", Description = "General Equipment" }
    };

    private static readonly List<AssetDto> DemoAssets = new()
    {
        new() { AssetId = "APP000001", Barcode = "100001", Description = "Laptop", Class = "COMPUTER", Custodian = "Demo User", SerialNumber = "DEMO001" },
        new() { AssetId = "APP000002", Barcode = "100002", Description = "Desktop Computer", Class = "COMPUTER", Custodian = "Demo User", SerialNumber = "DEMO002" },
        new() { AssetId = "APP000003", Barcode = "100003", Description = "Office Desk", Class = "FURNITURE", Custodian = "Demo User", SerialNumber = "DEMO003" },
        new() { AssetId = "APP000004", Barcode = "100004", Description = "Office Chair", Class = "FURNITURE", Custodian = "Demo User", SerialNumber = "DEMO004" },
        new() { AssetId = "APP000005", Barcode = "100005", Description = "Company Vehicle", Class = "VEHICLE", Custodian = "Demo User", SerialNumber = "DEMO005" },
        new() { AssetId = "APP000006", Barcode = "100006", Description = "Printer", Class = "EQUIPMENT", Custodian = "Demo User", SerialNumber = "DEMO006" }
    };

    public GpReader(IConfiguration config)
    {
        _useDemoData = config.GetValue<bool>("UseDemoData");
        ConnectionString = config.GetConnectionString("Gp") ?? "";
    }

    public async Task<List<ClassDto>> GetClassesAsync()
    {
        if (_useDemoData)
            return DemoClasses;

        using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        using var cmd = new SqlCommand(
            "SELECT Code, Description FROM aa_AssetApp_Classes ORDER BY Code", conn);
        using var reader = await cmd.ExecuteReaderAsync();

        var list = new List<ClassDto>();
        while (await reader.ReadAsync())
            list.Add(new ClassDto
            {
                Code = reader.GetString(0),
                Description = reader.GetString(1)
            });

        return list;
    }

    public async Task<List<AssetDto>> GetAssetsAsync(int page, int pageSize)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 2000) pageSize = 500;

        if (_useDemoData)
            return DemoAssets
                .OrderBy(a => a.AssetId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

        using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        var sql = @"SELECT AssetId, Barcode, Description, Class, Custodian, SerialNumber
                    FROM aa_AssetApp_Assets
                    ORDER BY AssetId
                    OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY";

        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@skip", (page - 1) * pageSize);
        cmd.Parameters.AddWithValue("@take", pageSize);

        using var reader = await cmd.ExecuteReaderAsync();
        var list = new List<AssetDto>();

        while (await reader.ReadAsync())
            list.Add(new AssetDto
            {
                AssetId = reader.GetString(0),
                Barcode = reader.GetString(1),
                Description = reader.GetString(2),
                Class = reader.GetString(3),
                Custodian = reader.GetString(4),
                SerialNumber = reader.GetString(5)
            });

        return list;
    }

    public async Task<string?> FindAssetIdByBarcodeAsync(string barcode)
    {
        if (_useDemoData)
            return DemoAssets.FirstOrDefault(a => a.Barcode == barcode)?.AssetId;

        using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        using var cmd = new SqlCommand(
            "SELECT TOP 1 AssetId FROM aa_AssetApp_Assets WHERE Barcode = @b", conn);
        cmd.Parameters.AddWithValue("@b", barcode);

        return await cmd.ExecuteScalarAsync() as string;
    }

    public async Task<string> GetNextAssetIdAsync(string prefix)
    {
        if (_useDemoData)
        {
            var highest = DemoAssets
                .Where(a => a.AssetId.StartsWith(prefix))
                .Select(a => int.TryParse(a.AssetId[prefix.Length..], out var n) ? n : 0)
                .DefaultIfEmpty(0)
                .Max();

            return prefix + (highest + 1).ToString("D6");
        }

        using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        using var cmd = new SqlCommand(
            "SELECT MAX(AssetId) FROM aa_AssetApp_Assets WHERE AssetId LIKE @p + '%'", conn);
        cmd.Parameters.AddWithValue("@p", prefix);

        var highestId = await cmd.ExecuteScalarAsync() as string;
        var number = 0;

        if (!string.IsNullOrEmpty(highestId))
            int.TryParse(highestId[prefix.Length..], out number);

        return prefix + (number + 1).ToString("D6");
    }

    public Task<CreateResult> CreateDemoAssetAsync(NewAssetRequest request, string prefix)
    {
        var barcode = request.Barcode.Trim();
        var description = request.Description.Trim();
        var classCode = request.Class.Trim();

        if (DemoAssets.Any(a =>
            string.Equals(a.Barcode, barcode, StringComparison.OrdinalIgnoreCase)))
        {
            return Task.FromResult(new CreateResult
            {
                Success = false,
                Error = "An asset with this barcode already exists."
            });
        }

        var assetClass = DemoClasses.FirstOrDefault(c =>
            string.Equals(c.Code, classCode, StringComparison.OrdinalIgnoreCase));

        if (assetClass is null)
        {
            return Task.FromResult(new CreateResult
            {
                Success = false,
                Error = "The selected asset class does not exist."
            });
        }

        var highest = DemoAssets
            .Where(a => a.AssetId.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .Select(a => int.TryParse(a.AssetId[prefix.Length..], out var n) ? n : 0)
            .DefaultIfEmpty(0)
            .Max();

        var assetId = prefix + (highest + 1).ToString("D6");

        DemoAssets.Add(new AssetDto
        {
            AssetId = assetId,
            Barcode = barcode,
            Description = description,
            Class = assetClass.Code,
            Custodian = request.Custodian.Trim(),
            SerialNumber = request.SerialNumber.Trim()
        });

        return Task.FromResult(new CreateResult
        {
            Success = true,
            GpAssetId = assetId
        });
    }

}
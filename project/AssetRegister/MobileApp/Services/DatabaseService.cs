using SQLite;
using MobileApp.Models;

namespace MobileApp.Services;

// All the SQLite work lives here: one file on the phone, three tables.
public class DatabaseService
{
    private SQLiteAsyncConnection? _db;

    // Opens the database the first time it is needed and makes sure the tables exist
    private async Task<SQLiteAsyncConnection> GetDb()
    {
        if (_db != null) return _db;

        var path = Path.Combine(FileSystem.AppDataDirectory, "assets.db3");
        _db = new SQLiteAsyncConnection(path);

        await _db.CreateTableAsync<Asset>();
        await _db.CreateTableAsync<AssetClass>();
        await _db.CreateTableAsync<NewAsset>();
        return _db;
    }

    // ----- Assets copied from GP -----

    public async Task<Asset?> FindAssetByBarcode(string barcode)
    {
        var db = await GetDb();
        return await db.Table<Asset>().Where(a => a.Barcode == barcode).FirstOrDefaultAsync();
    }

    // Throws away the old copy and stores the new one
    public async Task ReplaceAssets(List<Asset> assets)
    {
        var db = await GetDb();
        await db.RunInTransactionAsync(conn =>
        {
            conn.DeleteAll<Asset>();
            conn.InsertAll(assets);
        });
    }

    // ----- Classes copied from GP -----

    public async Task<List<AssetClass>> GetClasses()
    {
        var db = await GetDb();
        return await db.Table<AssetClass>().OrderBy(c => c.Code).ToListAsync();
    }

    public async Task ReplaceClasses(List<AssetClass> classes)
    {
        var db = await GetDb();
        await db.RunInTransactionAsync(conn =>
        {
            conn.DeleteAll<AssetClass>();
            conn.InsertAll(classes);
        });
    }

    // ----- New assets registered on the phone -----

    public async Task SaveNewAsset(NewAsset asset)
    {
        var db = await GetDb();
        await db.InsertAsync(asset);
    }

    public async Task UpdateNewAsset(NewAsset asset)
    {
        var db = await GetDb();
        await db.UpdateAsync(asset);
    }

    public async Task<NewAsset?> FindNewAssetByBarcode(string barcode)
    {
        var db = await GetDb();
        return await db.Table<NewAsset>().Where(n => n.Barcode == barcode).FirstOrDefaultAsync();
    }

    public async Task<List<NewAsset>> GetNewAssets()
    {
        var db = await GetDb();
        return await db.Table<NewAsset>().OrderByDescending(n => n.CreatedAt).ToListAsync();
    }

    public async Task<List<NewAsset>> GetWaitingAssets()
    {
        var db = await GetDb();
        return await db.Table<NewAsset>().Where(n => !n.Synced).ToListAsync();
    }

    public async Task<int> CountWaiting()
    {
        var db = await GetDb();
        return await db.Table<NewAsset>().Where(n => !n.Synced).CountAsync();
    }
}

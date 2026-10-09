# Asset Register – Mobile App + eConnect Service

A phone app that scans an asset barcode, shows the asset from Microsoft Dynamics GP,
and registers new assets (also offline). A small service on the GP machine talks to GP.

```
Phone app (.NET MAUI + SQLite)  <--HTTP-->  EconnectService (ASP.NET Core)  -->  SQL views (read) / eConnect (write)  -->  GP
```

## What is in this folder

| Folder | What it is |
|---|---|
| `MobileApp/` | The Android app (.NET MAUI). Models, Services, ViewModels, Views. |
| `EconnectService/` | The small web API. 4 endpoints under `/api`. |
| `sql/create_views.sql` | The two read-only SQL views the service reads from. |

Two rules in this project: **no SQL writes to GP tables (eConnect only)** and **no database password in the app**.

---

## 1. What to download and install

You need a **Windows PC** (GP and eConnect only run on Windows).

1. **Visual Studio 2022 Community** (free) – https://visualstudio.microsoft.com
   When the installer asks for workloads, tick:
   - **.NET Multi-platform App UI development** (this gives you MAUI + the Android tools and emulator)
   - **ASP.NET and web development**
2. **Git** – https://git-scm.com/download/win
3. **SQL Server 2022 Developer edition** and **SQL Server Management Studio (SSMS)** – free from Microsoft.
4. **Microsoft Dynamics GP 18.8.1941** (the 2025 release) and **eConnect 18.8** (on the same install media under *Additional Products*).
   Ask the company how to get the GP media/licence if you do not have it. Use this exact version.
   Then run **GP Utilities** and create the **Fabrikam** sample company (database `TWO`).
5. **Postman** (optional) – handy to test the service before using the phone.
6. A real Android phone: Settings > About phone > tap *Build number* 7 times > Developer options > turn on **USB debugging**.

---

## 2. Open the project

1. Copy this folder somewhere like `C:\Projects\AssetRegister`.
2. Open a terminal in the folder and start Git:
   ```
   git init
   git add .
   git commit -m "Initial project"
   ```
   Commit often (after every working step). The test asks for this.
3. In Visual Studio: **File > Open > Folder**, or open each `.csproj` file. The two projects are separate.
4. For the mobile app: right-click `MobileApp` > *Restore NuGet Packages*, then pick **Android Emulator** or your USB-connected phone in the toolbar and press the green play button.

> If the MAUI project gives odd build errors, the quickest fix is: create a fresh project in Visual Studio
> (*New Project > .NET MAUI App*, name it `MobileApp`), then copy the `Models`, `Services`, `ViewModels`,
> `Views` folders and `App.xaml*`, `AppShell.xaml*`, `MauiProgram.cs` over it, and add the NuGet packages listed in `MobileApp.csproj`.

---

## 3. Build order (follow the test's 5 steps)

### Step 1 – App with the fake service (about 1 week)
- Run the app. On first start the **Setup** screen opens. Type your name, leave **Use fake service** ON, tap Save.
- Press **Sync** – six sample assets and four classes arrive. The app works on a real phone with no GP at all.
- Commit: `git commit -am "Step 1 done"`.

### Step 2 – Scan asset and New asset
- Scan a Code 128 / Code 39 / QR code. Barcodes `100001` to `100006` match the fake sample assets
  (make them with any free online barcode generator and show them on a screen).
- Try an unknown barcode > **Register as new asset** > Save. Turn on airplane mode to prove it works offline.

### Step 3 – GP + service (ping, classes, assets)
1. Install GP, eConnect, create Fabrikam (section 1).
2. Open GP, log in to Fabrikam and check Fixed Assets has assets and classes.
3. In GP (not with SQL!) type a label on about 20 assets: *Cards > Fixed Assets > Asset General* > the **Label** field. Print barcodes for these labels.
4. Open `sql/create_views.sql` in SSMS. **First run the two `SELECT TOP 5 *` lines** to check your real column names (marked `CHECK`), fix the views if needed, then run the script.
5. Copy `Microsoft.Dynamics.GP.eConnect.dll` from the eConnect install folder (usually `C:\Program Files\Microsoft Dynamics\eConnect 18\...`) into `EconnectService\libs\`.
6. Open `EconnectService\appsettings.json`: set a long random `AccessKey`, and check the SQL Server name in the connection string.
7. Run the service (`dotnet run` in the `EconnectService` folder). Test in a browser/Postman: `http://localhost:5080/api/ping` with header `X-Api-Key: your key` should answer `OK`.
8. Let the phone reach it:
   - Phone and PC on the **same Wi-Fi**. Find the PC's IP with `ipconfig` (for example `192.168.1.10`).
   - Allow port 5080 in Windows Firewall (an *Inbound rule* for TCP 5080).
   - In the app Setup: address `http://192.168.1.10:5080`, your key, **turn the fake switch OFF**, tap **Test connection** – it should say *Connected*.
   - Android emulator instead of a phone? Use `http://10.0.2.2:5080`.
9. Press Sync: the Fabrikam assets appear on the phone. Scan a labelled one.

### Step 4 – Create asset in GP with eConnect
- Register a new asset on the phone, press Sync. It should get an Asset ID like `APP000001` and show **In GP**.
- Open GP > Fixed Assets and look for it.
- If GP says *"unable to obtain the next FAINDEX"*: Microsoft documents this. Manually save **one** asset (general + book) in GP first, then retry.
- Failures show the eConnect error text on the Sync screen; tap **Sync now** to retry.

### Step 5 – Testing
- Turn on airplane mode, register 10 assets, turn the internet back on, Sync. All 10 should reach GP.
- Press Sync twice. There must be no duplicates (the service checks the barcode first and returns the existing ID).

---

## 4. Things YOU must check (I could not test these here)

I could not run GP, eConnect or an Android build in the environment that made this project, so
the code has not been compiled against your machine. These are the spots most likely to need a tweak:

1. **SQL view column names** (`sql/create_views.sql`, lines marked `CHECK`): custodian, serial number, class description.
2. **eConnect XML** (`EconnectService/GpWriter.cs`, method `BuildXml`): the outer element name (`FAGeneralType`)
   and the field names for label / custodian / serial number. Microsoft's docs list the node as `taCreateAssetID`
   (fields include ASSETID, ASSETDESC, ASSETCLASSID, ACQDATE). Search the eConnect install folder or help for
   `taCreateAssetID` to copy the exact names.
3. **eConnect with .NET 8**: eConnect was built for .NET Framework. If the service cannot load the dll, the usual
   fix is to run the service as a .NET Framework 4.8 project instead. Tell the company; the 4 endpoints stay the same.
4. **Numbering rule**: I assumed `APP` + 6 digits (`APP000001`). The spec says "the agreed numbering rule" – ask them. It is one setting (`AssetIdPrefix`) plus `GetNextAssetIdAsync`.
5. **HTTP vs HTTPS**: the spec says HTTPS. For local testing I used plain HTTP (`usesCleartextTraffic` in the Android manifest). Mention this and say HTTPS comes before going live.
6. **Open question (spec section 10)**: whether the service should also create the asset book/account records. I left that to the finance user, as the spec's default says. (Section 10 was not in the PDF you pasted, so check it.)
7. **ZXing names**: if `BarcodeFormat` or `BarcodesDetected` do not compile, check the ZXing.Net.Maui version's sample on its GitHub page.

---

## 5. How it works (so you can explain it)

- **Offline first**: *New asset* only writes to SQLite (`DatabaseService.SaveNewAsset`). No internet needed.
- **Sync** (`SyncService`): upload waiting assets one by one, mark `Synced` only when the service returns a GP Asset ID, then download classes and assets and replace the local copy. The local copy is only replaced if both downloads work.
- **Fake vs real service**: both implement `IAssetApi`. `ApiProvider` picks one from the Setup switch. That is why the app could be built before GP existed.
- **Why a service**: eConnect is a Windows component that sits next to GP and cannot run on a phone.
- **Security**: every call needs the `X-Api-Key` header. The SQL connection string exists only on the server.
- **No duplicates**: before creating, the service looks for the barcode in GP. If found, it returns that Asset ID instead of creating a new one. A lock makes sure two requests cannot get the same new ID.
- **Why views**: the views trim GP's padded text and are the only thing the service reads. GP tables are never written to with SQL.
- **MVVM**: Views (`.xaml`) only show things. ViewModels hold the logic and commands. Models are the data.

### Questions they may ask you
- Why SQLite on the phone? (offline storage, no server needed for scanning)
- What happens if sync fails halfway? (each record is saved separately; failed ones keep their error and can be retried)
- Why not insert into GP with SQL? (it skips GP's business rules and can corrupt data; eConnect validates)
- What would you improve with more time? (HTTPS, SecureStorage for the key, tests, retry/backoff, logging)

Read every file once and make sure you can explain it in your own words. Change what you want to change.

## Test data warning
Use only **Fabrikam (TWO)**. Never connect the app or the service to any client database.

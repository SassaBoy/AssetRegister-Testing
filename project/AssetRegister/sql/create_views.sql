-- Read-only views for the Asset Register app.
-- Run this in SQL Server Management Studio against the Fabrikam database (TWO).
-- The service reads ONLY from these views. We never write to GP tables with SQL.

USE TWO;
GO

-- STEP 0: check the real column names in YOUR GP before creating the views.
-- If a column below does not exist, change the view to match what you see here.
-- SELECT TOP 5 * FROM FA00100;
-- SELECT TOP 5 * FROM FA40201;
-- (Column names can differ slightly between GP versions.)

IF OBJECT_ID('dbo.aa_AssetApp_Assets', 'V') IS NOT NULL DROP VIEW dbo.aa_AssetApp_Assets;
GO
CREATE VIEW dbo.aa_AssetApp_Assets AS
SELECT
    RTRIM(LTRIM(ASSETID))                  AS AssetId,       -- GP pads text with spaces, so we trim
    RTRIM(LTRIM(ISNULL(Asset_Label, '')))  AS Barcode,       -- the barcode is the asset label
    RTRIM(LTRIM(ISNULL(ASSETDESC, '')))    AS Description,
    RTRIM(LTRIM(ISNULL(ASSETCLASSID, ''))) AS Class,
    RTRIM(LTRIM(ISNULL(EMPLOYID, '')))     AS Custodian,     -- CHECK: custodian column name
    RTRIM(LTRIM(ISNULL(SERIALNO, '')))     AS SerialNumber   -- CHECK: serial number column name
FROM dbo.FA00100;
GO

IF OBJECT_ID('dbo.aa_AssetApp_Classes', 'V') IS NOT NULL DROP VIEW dbo.aa_AssetApp_Classes;
GO
CREATE VIEW dbo.aa_AssetApp_Classes AS
SELECT
    RTRIM(LTRIM(ASSETCLASSID))               AS Code,
    RTRIM(LTRIM(ISNULL(ASSETCLASSDESC, ''))) AS Description  -- CHECK: description column name
FROM dbo.FA40201;
GO

-- Quick test
SELECT TOP 10 * FROM dbo.aa_AssetApp_Assets;
SELECT * FROM dbo.aa_AssetApp_Classes;

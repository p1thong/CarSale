-- Quick check for Identity columns in CarSalesDB
-- Run this first to see which tables need fixing

USE CarSalesDB;
GO

-- Check current identity columns
PRINT '=== CURRENT IDENTITY COLUMNS ===';
SELECT 
    OBJECT_NAME(object_id) AS TableName,
    name AS ColumnName,
    seed_value,
    increment_value
FROM sys.identity_columns
ORDER BY OBJECT_NAME(object_id);

PRINT '';
PRINT '=== TABLES MISSING IDENTITY ===';
-- Find ID columns that are NOT identity
SELECT 
    t.name AS TableName,
    c.name AS ColumnName,
    'NOT IDENTITY' AS Status
FROM sys.tables t
INNER JOIN sys.columns c ON t.object_id = c.object_id
WHERE c.name LIKE '%Id' 
    AND c.column_id = 1 -- Primary key is usually first column
    AND c.is_identity = 0 -- NOT identity
    AND t.name IN ('Customer', 'Dealer', 'DealerContract', 'Feedback', 'Manufacturer', 
                   'Order', 'Payment', 'Promotion', 'Quotation', 'SalesContract', 
                   'TestDrive', 'User', 'VehicleModel', 'VehicleVariant')
ORDER BY t.name;

PRINT '';
PRINT '=== ALL PRIMARY KEY COLUMNS ===';
-- Show all primary key columns and their identity status
SELECT 
    t.name AS TableName,
    c.name AS ColumnName,
    CASE WHEN c.is_identity = 1 THEN 'YES' ELSE 'NO' END AS IsIdentity,
    c.data_type
FROM sys.tables t
INNER JOIN sys.columns c ON t.object_id = c.object_id
INNER JOIN sys.key_constraints kc ON t.object_id = kc.parent_object_id
INNER JOIN sys.index_columns ic ON kc.parent_object_id = ic.object_id AND kc.unique_index_id = ic.index_id
WHERE ic.column_id = c.column_id
    AND kc.type = 'PK'
    AND t.name IN ('Customer', 'Dealer', 'DealerContract', 'Feedback', 'Manufacturer', 
                   'Order', 'Payment', 'Promotion', 'Quotation', 'SalesContract', 
                   'TestDrive', 'User', 'VehicleModel', 'VehicleVariant')
ORDER BY t.name;
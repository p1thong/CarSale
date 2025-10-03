-- SQL Script to check all Identity columns in database
-- Run this in SQL Server Management Studio or Azure Data Studio

SELECT 
    t.name AS TableName,
    c.name AS ColumnName,
    c.is_identity AS IsIdentity,
    CASE WHEN c.is_identity = 1 THEN 'YES' ELSE 'NO' END AS IdentityStatus,
    ISNULL(ic.seed_value, 0) AS SeedValue,
    ISNULL(ic.increment_value, 0) AS IncrementValue,
    c.column_id AS ColumnOrder
FROM sys.tables t
INNER JOIN sys.columns c ON t.object_id = c.object_id
LEFT JOIN sys.identity_columns ic ON c.object_id = ic.object_id AND c.column_id = ic.column_id
WHERE c.name LIKE '%Id'
    AND t.name IN ('Customer', 'Dealer', 'DealerContract', 'Feedback', 'Manufacturer', 
                   'Order', 'Payment', 'Promotion', 'Quotation', 'SalesContract', 
                   'TestDrive', 'User', 'VehicleModel', 'VehicleVariant')
ORDER BY t.name, c.column_id;

-- Alternative simpler query to check identity columns only
SELECT 
    OBJECT_NAME(object_id) AS TableName,
    name AS ColumnName,
    seed_value,
    increment_value,
    is_not_for_replication
FROM sys.identity_columns
WHERE OBJECT_NAME(object_id) IN ('Customer', 'Dealer', 'DealerContract', 'Feedback', 'Manufacturer', 
                                 'Order', 'Payment', 'Promotion', 'Quotation', 'SalesContract', 
                                 'TestDrive', 'User', 'VehicleModel', 'VehicleVariant')
ORDER BY OBJECT_NAME(object_id);
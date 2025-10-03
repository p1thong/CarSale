-- Fix Identity Columns in CarSalesDB
-- Run this script in SQL Server Management Studio or Azure Data Studio

USE CarSalesDB;
GO

-- First, check which tables don't have identity columns configured
SELECT 
    t.name AS TableName,
    c.name AS ColumnName,
    c.is_identity AS IsIdentity
FROM sys.tables t
INNER JOIN sys.columns c ON t.object_id = c.object_id
WHERE c.name LIKE '%Id' 
    AND c.column_id = 1 -- Primary key is usually first column
    AND t.name IN ('Customer', 'Dealer', 'DealerContract', 'Feedback', 'Manufacturer', 
                   'Order', 'Payment', 'Promotion', 'Quotation', 'SalesContract', 
                   'TestDrive', 'User', 'VehicleModel', 'VehicleVariant')
ORDER BY t.name;

-- Now fix each table that needs identity
-- Note: We need to recreate tables to add IDENTITY property

-- 1. Fix Manufacturer table
IF NOT EXISTS (SELECT 1 FROM sys.identity_columns WHERE OBJECT_NAME(object_id) = 'Manufacturer')
BEGIN
    PRINT 'Fixing Manufacturer table...';
    
    -- Create temp table with data
    SELECT * INTO Manufacturer_Temp FROM Manufacturer;
    
    -- Drop foreign key constraints first
    DECLARE @sql NVARCHAR(MAX) = '';
    SELECT @sql = @sql + 'ALTER TABLE ' + OBJECT_NAME(parent_object_id) + ' DROP CONSTRAINT ' + name + ';' + CHAR(13)
    FROM sys.foreign_keys 
    WHERE referenced_object_id = OBJECT_ID('Manufacturer');
    
    IF LEN(@sql) > 0 EXEC sp_executesql @sql;
    
    -- Drop and recreate table
    DROP TABLE Manufacturer;
    
    CREATE TABLE Manufacturer (
        manufacturerId int IDENTITY(1,1) NOT NULL,
        name varchar(100) NOT NULL,
        country varchar(50) NULL,
        address varchar(200) NULL,
        CONSTRAINT PK__Manufact__02B55389ED519028 PRIMARY KEY (manufacturerId)
    );
    
    -- Restore data (excluding identity column)
    SET IDENTITY_INSERT Manufacturer ON;
    INSERT INTO Manufacturer (manufacturerId, name, country, address)
    SELECT manufacturerId, name, country, address FROM Manufacturer_Temp;
    SET IDENTITY_INSERT Manufacturer OFF;
    
    DROP TABLE Manufacturer_Temp;
    
    PRINT 'Manufacturer table fixed.';
END

-- 2. Fix Customer table
IF NOT EXISTS (SELECT 1 FROM sys.identity_columns WHERE OBJECT_NAME(object_id) = 'Customer')
BEGIN
    PRINT 'Fixing Customer table...';
    
    SELECT * INTO Customer_Temp FROM Customer;
    
    DROP TABLE Customer;
    
    CREATE TABLE Customer (
        customerId int IDENTITY(1,1) NOT NULL,
        dealerId int NOT NULL,
        fullName varchar(100) NOT NULL,
        email varchar(100) NOT NULL,
        phone varchar(20) NULL,
        birthday datetime NULL,
        CONSTRAINT PK__Customer__B611CB7D0049E38E PRIMARY KEY (customerId)
    );
    
    SET IDENTITY_INSERT Customer ON;
    INSERT INTO Customer (customerId, dealerId, fullName, email, phone, birthday)
    SELECT customerId, dealerId, fullName, email, phone, birthday FROM Customer_Temp;
    SET IDENTITY_INSERT Customer OFF;
    
    DROP TABLE Customer_Temp;
    
    PRINT 'Customer table fixed.';
END

-- 3. Fix Dealer table
IF NOT EXISTS (SELECT 1 FROM sys.identity_columns WHERE OBJECT_NAME(object_id) = 'Dealer')
BEGIN
    PRINT 'Fixing Dealer table...';
    
    SELECT * INTO Dealer_Temp FROM Dealer;
    
    DROP TABLE Dealer;
    
    CREATE TABLE Dealer (
        dealerId int IDENTITY(1,1) NOT NULL,
        fullName varchar(100) NOT NULL,
        email varchar(100) NOT NULL,
        password varchar(100) NOT NULL,
        phone varchar(20) NULL,
        transactionId int NULL,
        CONSTRAINT PK__Dealer__5A9E9D961C30970D PRIMARY KEY (dealerId)
    );
    
    SET IDENTITY_INSERT Dealer ON;
    INSERT INTO Dealer (dealerId, fullName, email, password, phone, transactionId)
    SELECT dealerId, fullName, email, password, phone, transactionId FROM Dealer_Temp;
    SET IDENTITY_INSERT Dealer OFF;
    
    DROP TABLE Dealer_Temp;
    
    PRINT 'Dealer table fixed.';
END

-- 4. Fix VehicleModel table
IF NOT EXISTS (SELECT 1 FROM sys.identity_columns WHERE OBJECT_NAME(object_id) = 'VehicleModel')
BEGIN
    PRINT 'Fixing VehicleModel table...';
    
    SELECT * INTO VehicleModel_Temp FROM VehicleModel;
    
    DROP TABLE VehicleModel;
    
    CREATE TABLE VehicleModel (
        vehicleModelId int IDENTITY(1,1) NOT NULL,
        manufacturerId int NOT NULL,
        name varchar(100) NOT NULL,
        category varchar(50) NULL,
        imageUrl varchar(500) NULL,
        CONSTRAINT PK__VehicleM__C59AC3F31B78FC2B PRIMARY KEY (vehicleModelId)
    );
    
    SET IDENTITY_INSERT VehicleModel ON;
    INSERT INTO VehicleModel (vehicleModelId, manufacturerId, name, category, imageUrl)
    SELECT vehicleModelId, manufacturerId, name, category, imageUrl FROM VehicleModel_Temp;
    SET IDENTITY_INSERT VehicleModel OFF;
    
    DROP TABLE VehicleModel_Temp;
    
    PRINT 'VehicleModel table fixed.';
END

-- 5. Fix VehicleVariant table
IF NOT EXISTS (SELECT 1 FROM sys.identity_columns WHERE OBJECT_NAME(object_id) = 'VehicleVariant')
BEGIN
    PRINT 'Fixing VehicleVariant table...';
    
    SELECT * INTO VehicleVariant_Temp FROM VehicleVariant;
    
    DROP TABLE VehicleVariant;
    
    CREATE TABLE VehicleVariant (
        variantId int IDENTITY(1,1) NOT NULL,
        vehicleModelId int NOT NULL,
        version varchar(50) NOT NULL,
        color varchar(30) NOT NULL,
        price decimal(18,2) NOT NULL,
        productYear int NOT NULL,
        Quantity int NULL,
        CONSTRAINT PK__VehicleV__9E8F55A6BD3C0B38 PRIMARY KEY (variantId)
    );
    
    SET IDENTITY_INSERT VehicleVariant ON;
    INSERT INTO VehicleVariant (variantId, vehicleModelId, version, color, price, productYear, Quantity)
    SELECT variantId, vehicleModelId, version, color, price, productYear, Quantity FROM VehicleVariant_Temp;
    SET IDENTITY_INSERT VehicleVariant OFF;
    
    DROP TABLE VehicleVariant_Temp;
    
    PRINT 'VehicleVariant table fixed.';
END

-- 6. Fix other tables similarly...
-- (Add more tables as needed)

-- Recreate foreign key constraints
ALTER TABLE Customer ADD CONSTRAINT FK__Customer__dealer__6FE99F9F 
    FOREIGN KEY (dealerId) REFERENCES Dealer(dealerId);

ALTER TABLE VehicleModel ADD CONSTRAINT FK__VehicleM__manufa__4D5F7D71 
    FOREIGN KEY (manufacturerId) REFERENCES Manufacturer(manufacturerId);

ALTER TABLE VehicleVariant ADD CONSTRAINT FK__VehicleV__vehicl__503BEA1D 
    FOREIGN KEY (vehicleModelId) REFERENCES VehicleModel(vehicleModelId);

-- Add other foreign keys as needed...

-- Verify results
PRINT 'Verification - Tables with IDENTITY columns:';
SELECT 
    OBJECT_NAME(object_id) AS TableName,
    name AS ColumnName,
    seed_value,
    increment_value
FROM sys.identity_columns
WHERE OBJECT_NAME(object_id) IN ('Customer', 'Dealer', 'Manufacturer', 'VehicleModel', 'VehicleVariant')
ORDER BY OBJECT_NAME(object_id);

PRINT 'Identity columns fix completed!';
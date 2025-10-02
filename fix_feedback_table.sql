-- Manual script to fix Feedback table
-- Run this in SQL Server Management Studio or similar tool

-- Check if Feedback table exists and has data
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Feedback')
BEGIN
    -- Drop existing table if empty, or backup data first
    IF (SELECT COUNT(*) FROM Feedback) = 0
    BEGIN
        DROP TABLE Feedback;
        
        -- Recreate table with proper IDENTITY
        CREATE TABLE [Feedback] (
            [feedbackId] int IDENTITY(1,1) NOT NULL,
            [customerId] int NOT NULL,
            [content] varchar(500) NULL,
            [createdAt] datetime NULL,
            [feedbackDate] datetime NULL,
            [rating] int NULL,
            CONSTRAINT [PK__Feedback__2613FD24ACBA58F5] PRIMARY KEY ([feedbackId]),
            CONSTRAINT [FK__Feedback__custom__02FC7413] FOREIGN KEY ([customerId]) REFERENCES [Customer]([customerId])
        );
    END
    ELSE
    BEGIN
        PRINT 'Feedback table has data. Manual migration required.';
        -- If table has data, you need to:
        -- 1. Create new table with IDENTITY
        -- 2. Copy data
        -- 3. Drop old table
        -- 4. Rename new table
    END
END
ELSE
BEGIN
    -- Create new table if doesn't exist
    CREATE TABLE [Feedback] (
        [feedbackId] int IDENTITY(1,1) NOT NULL,
        [customerId] int NOT NULL,
        [content] varchar(500) NULL,
        [createdAt] datetime NULL,
        [feedbackDate] datetime NULL,
        [rating] int NULL,
        CONSTRAINT [PK__Feedback__2613FD24ACBA58F5] PRIMARY KEY ([feedbackId]),
        CONSTRAINT [FK__Feedback__custom__02FC7413] FOREIGN KEY ([customerId]) REFERENCES [Customer]([customerId])
    );
END
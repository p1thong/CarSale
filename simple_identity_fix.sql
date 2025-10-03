-- Simple Identity Fix for CarSalesDB
-- Run this to add IDENTITY to primary key columns that don't have it

USE CarSalesDB;
GO

-- Method 1: Try to add IDENTITY using ALTER (may not work on all SQL versions)
-- If this doesn't work, use the recreate method below

PRINT 'Attempting to add IDENTITY to primary keys...';

-- Check if we can alter columns to add identity (SQL Server 2017+)
-- This is usually not possible, but worth trying

-- Method 2: Simple recreate for key tables
-- Let's start with just the most important ones

-- Fix Feedback table (usually smallest, good for testing)
IF NOT EXISTS (SELECT 1 FROM sys.identity_columns WHERE OBJECT_NAME(object_id) = 'Feedback')
BEGIN
    PRINT 'Fixing Feedback table...';
    
    -- Backup data
    SELECT * INTO Feedback_Backup FROM Feedback;
    
    -- Drop table
    DROP TABLE Feedback;
    
    -- Recreate with IDENTITY
    CREATE TABLE [Feedback] (
        [feedbackId] int IDENTITY(1,1) NOT NULL,
        [customerId] int NOT NULL,
        [testDriveId] int NULL,
        [rating] int NULL,
        [comment] text NULL,
        [feedbackDate] datetime NULL,
        CONSTRAINT [PK__Feedback__2613FD4DB0C5FF56] PRIMARY KEY ([feedbackId])
    );
    
    -- Restore data
    IF EXISTS (SELECT 1 FROM Feedback_Backup)
    BEGIN
        SET IDENTITY_INSERT Feedback ON;
        INSERT INTO Feedback (feedbackId, customerId, testDriveId, rating, comment, feedbackDate)
        SELECT feedbackId, customerId, testDriveId, rating, comment, feedbackDate 
        FROM Feedback_Backup;
        SET IDENTITY_INSERT Feedback OFF;
    END
    
    -- Clean up
    DROP TABLE Feedback_Backup;
    
    PRINT 'Feedback table completed.';
END
ELSE
BEGIN
    PRINT 'Feedback table already has IDENTITY.';
END

-- Test the fix
PRINT 'Testing Feedback identity...';
INSERT INTO Feedback (customerId, rating, comment, feedbackDate) 
VALUES (1, 5, 'Test identity insert', GETDATE());

SELECT TOP 1 feedbackId, comment FROM Feedback ORDER BY feedbackId DESC;

PRINT 'Simple identity fix completed!';
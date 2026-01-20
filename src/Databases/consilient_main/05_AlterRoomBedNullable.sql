-- Migration script to make Room and Bed columns nullable in staging.ProviderAssignments
-- Run this if the table already exists with NOT NULL constraints

IF EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'staging'
    AND TABLE_NAME = 'ProviderAssignments'
    AND COLUMN_NAME = 'Room'
    AND IS_NULLABLE = 'NO'
)
BEGIN
    ALTER TABLE [staging].[ProviderAssignments] ALTER COLUMN [Room] nvarchar(20) NULL;
    PRINT 'Altered Room column to allow NULL';
END

IF EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'staging'
    AND TABLE_NAME = 'ProviderAssignments'
    AND COLUMN_NAME = 'Bed'
    AND IS_NULLABLE = 'NO'
)
BEGIN
    ALTER TABLE [staging].[ProviderAssignments] ALTER COLUMN [Bed] nvarchar(5) NULL;
    PRINT 'Altered Bed column to allow NULL';
END
GO

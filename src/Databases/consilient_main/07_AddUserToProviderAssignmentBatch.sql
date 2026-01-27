BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260127152408_AddUserToProviderAssignmentBatch'
)
BEGIN
    ALTER TABLE [staging].[ProviderAssignmentBatches] ADD [CreatedByUserId] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260127152408_AddUserToProviderAssignmentBatch'
)
BEGIN
    CREATE INDEX [IX_ProviderAssignmentBatches_CreatedByUserId] ON [staging].[ProviderAssignmentBatches] ([CreatedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260127152408_AddUserToProviderAssignmentBatch'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260127152408_AddUserToProviderAssignmentBatch', N'9.0.12');
END;

COMMIT;
GO


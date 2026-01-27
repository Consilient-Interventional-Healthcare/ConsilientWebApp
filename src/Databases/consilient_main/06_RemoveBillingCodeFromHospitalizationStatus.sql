BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260127014033_RemoveBillingCodeFromHospitalizationStatus'
)
BEGIN
    DECLARE @var sysname;
    SELECT @var = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clinical].[HospitalizationStatuses]') AND [c].[name] = N'BillingCode');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [Clinical].[HospitalizationStatuses] DROP CONSTRAINT [' + @var + '];');
    ALTER TABLE [Clinical].[HospitalizationStatuses] DROP COLUMN [BillingCode];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260127014033_RemoveBillingCodeFromHospitalizationStatus'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260127014033_RemoveBillingCodeFromHospitalizationStatus', N'9.0.12');
END;

COMMIT;
GO


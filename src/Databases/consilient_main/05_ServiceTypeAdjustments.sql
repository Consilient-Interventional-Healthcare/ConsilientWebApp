BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124010312_ServiceTypeAdjustments'
)
BEGIN
    ALTER TABLE [Clinical].[Visits] DROP CONSTRAINT [FK_Visits_ServiceTypes_ServiceTypeId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124010312_ServiceTypeAdjustments'
)
BEGIN
    DROP INDEX [IX_Visits_ServiceTypeId] ON [Clinical].[Visits];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124010312_ServiceTypeAdjustments'
)
BEGIN
    DECLARE @var sysname;
    SELECT @var = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clinical].[Visits]') AND [c].[name] = N'ServiceTypeId');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [Clinical].[Visits] DROP CONSTRAINT [' + @var + '];');
    ALTER TABLE [Clinical].[Visits] DROP COLUMN [ServiceTypeId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124010312_ServiceTypeAdjustments'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clinical].[ServiceTypes]') AND [c].[name] = N'CPTCode');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Clinical].[ServiceTypes] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [Clinical].[ServiceTypes] DROP COLUMN [CPTCode];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124010312_ServiceTypeAdjustments'
)
BEGIN
    IF SCHEMA_ID(N'Billing') IS NULL EXEC(N'CREATE SCHEMA [Billing];');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124010312_ServiceTypeAdjustments'
)
BEGIN
    CREATE TABLE [Billing].[BillingCodes] (
        [Id] int NOT NULL,
        [Code] varchar(20) NOT NULL,
        [Description] nvarchar(200) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_BillingCodes] PRIMARY KEY ([Id]),
        CONSTRAINT [AK_BillingCodes_Code] UNIQUE ([Code])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124010312_ServiceTypeAdjustments'
)
BEGIN
    CREATE TABLE [Billing].[VisitServiceBillings] (
        [Id] int NOT NULL IDENTITY,
        [VisitId] int NOT NULL,
        [ServiceTypeId] int NOT NULL,
        [BillingCodeId] int NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_VisitServiceBilling] PRIMARY KEY ([Id]),
        CONSTRAINT [AK_VisitServiceBillings_VisitId_ServiceTypeId_BillingCodeId] UNIQUE ([VisitId], [ServiceTypeId], [BillingCodeId]),
        CONSTRAINT [FK_VisitServiceBillings_BillingCodes_BillingCodeId] FOREIGN KEY ([BillingCodeId]) REFERENCES [Billing].[BillingCodes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_VisitServiceBillings_ServiceTypes_ServiceTypeId] FOREIGN KEY ([ServiceTypeId]) REFERENCES [Clinical].[ServiceTypes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_VisitServiceBillings_Visits_VisitId] FOREIGN KEY ([VisitId]) REFERENCES [Clinical].[Visits] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124010312_ServiceTypeAdjustments'
)
BEGIN
    CREATE INDEX [IX_VisitServiceBillings_BillingCodeId] ON [Billing].[VisitServiceBillings] ([BillingCodeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124010312_ServiceTypeAdjustments'
)
BEGIN
    CREATE INDEX [IX_VisitServiceBillings_ServiceTypeId] ON [Billing].[VisitServiceBillings] ([ServiceTypeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124010312_ServiceTypeAdjustments'
)
BEGIN
    CREATE INDEX [IX_VisitServiceBillings_VisitId] ON [Billing].[VisitServiceBillings] ([VisitId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124010312_ServiceTypeAdjustments'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260124010312_ServiceTypeAdjustments', N'9.0.12');
END;

COMMIT;
GO


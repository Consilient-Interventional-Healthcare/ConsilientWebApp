-- ============================================
-- EF Core Migration Script (Auto-Generated)
-- ============================================
-- Context:     ConsilientDbContext
-- Migration:   20260130200015_AddServiceTypeBillingCodes
-- Generated:   2026-01-30T20:01:58.1576101Z
-- ============================================
-- WARNING: This file is auto-generated.
-- It will be deleted by SquashMigrations.
-- Do not manually edit unless necessary.
-- ============================================

-- Required SET options for SQL Server indexed views and computed columns
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130200015_AddServiceTypeBillingCodes'
)
BEGIN
    CREATE TABLE [Billing].[ServiceTypeBillingCodes] (
        [Id] int NOT NULL IDENTITY,
        [ServiceTypeId] int NOT NULL,
        [BillingCodeId] int NOT NULL,
        [IsDefault] bit NOT NULL DEFAULT CAST(0 AS bit),
        [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_ServiceTypeBillingCode] PRIMARY KEY ([Id]),
        CONSTRAINT [AK_ServiceTypeBillingCodes_ServiceTypeId_BillingCodeId] UNIQUE ([ServiceTypeId], [BillingCodeId]),
        CONSTRAINT [FK_ServiceTypeBillingCodes_BillingCodes] FOREIGN KEY ([BillingCodeId]) REFERENCES [Billing].[BillingCodes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ServiceTypeBillingCodes_ServiceTypes] FOREIGN KEY ([ServiceTypeId]) REFERENCES [Clinical].[ServiceTypes] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130200015_AddServiceTypeBillingCodes'
)
BEGIN
    CREATE INDEX [IX_ServiceTypeBillingCodes_BillingCodeId] ON [Billing].[ServiceTypeBillingCodes] ([BillingCodeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130200015_AddServiceTypeBillingCodes'
)
BEGIN
    CREATE INDEX [IX_ServiceTypeBillingCodes_ServiceTypeId] ON [Billing].[ServiceTypeBillingCodes] ([ServiceTypeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130200015_AddServiceTypeBillingCodes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260130200015_AddServiceTypeBillingCodes', N'9.0.12');
END;

COMMIT;
GO


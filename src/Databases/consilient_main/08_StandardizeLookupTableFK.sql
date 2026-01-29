SET QUOTED_IDENTIFIER ON;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260129014249_StandardizeLookupTableFK'
)
BEGIN
    DECLARE @var sysname;
    SELECT @var = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[staging].[ProviderAssignments]') AND [c].[name] = N'ExclusionReason');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [staging].[ProviderAssignments] DROP CONSTRAINT [' + @var + '];');
    ALTER TABLE [staging].[ProviderAssignments] DROP COLUMN [ExclusionReason];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260129014249_StandardizeLookupTableFK'
)
BEGIN
    EXEC sp_rename N'[Clinical].[Providers].[Type]', N'ProviderTypeId', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260129014249_StandardizeLookupTableFK'
)
BEGIN
    EXEC sp_rename N'[staging].[ProviderAssignmentBatches].[Status]', N'StatusId', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260129014249_StandardizeLookupTableFK'
)
BEGIN

                    -- Drop FK constraints referencing VisitEventTypes
                    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_VisitEvents_VisitEventTypes_EventTypeId')
                        ALTER TABLE [Clinical].[VisitEvents] DROP CONSTRAINT [FK_VisitEvents_VisitEventTypes_EventTypeId];

                    -- Drop FK constraints referencing ServiceTypes
                    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_VisitServiceBillings_ServiceTypes_ServiceTypeId')
                        ALTER TABLE [Billing].[VisitServiceBillings] DROP CONSTRAINT [FK_VisitServiceBillings_ServiceTypes_ServiceTypeId];

                    -- Drop FK constraints referencing HospitalizationStatuses
                    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Hospitalizations_HospitalizationStatuses_HospitalizationStatusId')
                        ALTER TABLE [Clinical].[Hospitalizations] DROP CONSTRAINT [FK_Hospitalizations_HospitalizationStatuses_HospitalizationStatusId];
                    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_HospitalizationStatusHistories_HospitalizationStatuses_NewStatusId')
                        ALTER TABLE [Clinical].[HospitalizationStatusHistories] DROP CONSTRAINT [FK_HospitalizationStatusHistories_HospitalizationStatuses_NewStatusId];
                    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ProviderAssignments_HospitalizationStatuses_ResolvedHospitalizationStatusId')
                        ALTER TABLE [staging].[ProviderAssignments] DROP CONSTRAINT [FK_ProviderAssignments_HospitalizationStatuses_ResolvedHospitalizationStatusId];
                
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260129014249_StandardizeLookupTableFK'
)
BEGIN

                    -- VisitEventTypes: Remove IDENTITY, add DisplayOrder
                    SELECT [Id], [Code], [Name], [CreatedAtUtc], [UpdatedAtUtc] INTO #VisitEventTypes_Backup FROM [Clinical].[VisitEventTypes];
                    DROP TABLE [Clinical].[VisitEventTypes];
                    CREATE TABLE [Clinical].[VisitEventTypes] (
                        [Id] int NOT NULL,
                        [Code] nvarchar(50) NOT NULL,
                        [Name] nvarchar(100) NOT NULL,
                        [DisplayOrder] int NOT NULL DEFAULT 0,
                        [CreatedAtUtc] datetime2 NOT NULL,
                        [UpdatedAtUtc] datetime2 NOT NULL,
                        [RowVersion] rowversion NOT NULL,
                        CONSTRAINT [PK_VisitEventType] PRIMARY KEY ([Id])
                    );
                    INSERT INTO [Clinical].[VisitEventTypes] ([Id], [Code], [Name], [DisplayOrder], [CreatedAtUtc], [UpdatedAtUtc])
                    SELECT [Id], [Code], [Name], 0, [CreatedAtUtc], [UpdatedAtUtc] FROM #VisitEventTypes_Backup;
                    DROP TABLE #VisitEventTypes_Backup;
                
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260129014249_StandardizeLookupTableFK'
)
BEGIN

                    -- ServiceTypes: Remove IDENTITY, rename Description to Name, add Code and DisplayOrder
                    SELECT [Id], [Description], [CreatedAtUtc], [UpdatedAtUtc] INTO #ServiceTypes_Backup FROM [Clinical].[ServiceTypes];
                    DROP TABLE [Clinical].[ServiceTypes];
                    CREATE TABLE [Clinical].[ServiceTypes] (
                        [Id] int NOT NULL,
                        [Code] nvarchar(50) NOT NULL DEFAULT '',
                        [Name] nvarchar(100) NOT NULL,
                        [DisplayOrder] int NOT NULL DEFAULT 0,
                        [CreatedAtUtc] datetime2 NOT NULL,
                        [UpdatedAtUtc] datetime2 NOT NULL,
                        [RowVersion] rowversion NOT NULL,
                        CONSTRAINT [PK_ServiceType] PRIMARY KEY ([Id])
                    );
                    INSERT INTO [Clinical].[ServiceTypes] ([Id], [Code], [Name], [DisplayOrder], [CreatedAtUtc], [UpdatedAtUtc])
                    SELECT [Id], REPLACE([Description], ' ', ''), [Description], 0, [CreatedAtUtc], [UpdatedAtUtc] FROM #ServiceTypes_Backup;
                    DROP TABLE #ServiceTypes_Backup;
                
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260129014249_StandardizeLookupTableFK'
)
BEGIN

                    -- HospitalizationStatuses: Remove IDENTITY, drop Color
                    SELECT [Id], [Code], [Name], [DisplayOrder], [CreatedAtUtc], [UpdatedAtUtc] INTO #HospitalizationStatuses_Backup FROM [Clinical].[HospitalizationStatuses];
                    DROP TABLE [Clinical].[HospitalizationStatuses];
                    CREATE TABLE [Clinical].[HospitalizationStatuses] (
                        [Id] int NOT NULL,
                        [Code] nvarchar(50) NOT NULL,
                        [Name] nvarchar(100) NOT NULL,
                        [DisplayOrder] int NOT NULL,
                        [CreatedAtUtc] datetime2 NOT NULL,
                        [UpdatedAtUtc] datetime2 NOT NULL,
                        [RowVersion] rowversion NOT NULL,
                        CONSTRAINT [PK_HospitalizationStatus] PRIMARY KEY ([Id])
                    );
                    INSERT INTO [Clinical].[HospitalizationStatuses] ([Id], [Code], [Name], [DisplayOrder], [CreatedAtUtc], [UpdatedAtUtc])
                    SELECT [Id], [Code], [Name], [DisplayOrder], [CreatedAtUtc], [UpdatedAtUtc] FROM #HospitalizationStatuses_Backup;
                    DROP TABLE #HospitalizationStatuses_Backup;
                
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260129014249_StandardizeLookupTableFK'
)
BEGIN

                    -- Recreate FK constraints for VisitEventTypes
                    ALTER TABLE [Clinical].[VisitEvents] ADD CONSTRAINT [FK_VisitEvents_VisitEventTypes_EventTypeId]
                        FOREIGN KEY ([EventTypeId]) REFERENCES [Clinical].[VisitEventTypes] ([Id]) ON DELETE NO ACTION;

                    -- Recreate FK constraints for ServiceTypes
                    ALTER TABLE [Billing].[VisitServiceBillings] ADD CONSTRAINT [FK_VisitServiceBillings_ServiceTypes_ServiceTypeId]
                        FOREIGN KEY ([ServiceTypeId]) REFERENCES [Clinical].[ServiceTypes] ([Id]) ON DELETE NO ACTION;

                    -- Recreate FK constraints for HospitalizationStatuses
                    ALTER TABLE [Clinical].[Hospitalizations] ADD CONSTRAINT [FK_Hospitalizations_HospitalizationStatuses_HospitalizationStatusId]
                        FOREIGN KEY ([HospitalizationStatusId]) REFERENCES [Clinical].[HospitalizationStatuses] ([Id]) ON DELETE NO ACTION;
                    ALTER TABLE [Clinical].[HospitalizationStatusHistories] ADD CONSTRAINT [FK_HospitalizationStatusHistories_HospitalizationStatuses_NewStatusId]
                        FOREIGN KEY ([NewStatusId]) REFERENCES [Clinical].[HospitalizationStatuses] ([Id]) ON DELETE NO ACTION;
                    ALTER TABLE [staging].[ProviderAssignments] ADD CONSTRAINT [FK_ProviderAssignments_HospitalizationStatuses_ResolvedHospitalizationStatusId]
                        FOREIGN KEY ([ResolvedHospitalizationStatusId]) REFERENCES [Clinical].[HospitalizationStatuses] ([Id]) ON DELETE NO ACTION;
                
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260129014249_StandardizeLookupTableFK'
)
BEGIN
    CREATE TABLE [staging].[ProviderAssignmentBatchStatuses] (
        [Id] int NOT NULL,
        [Code] nvarchar(50) NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [DisplayOrder] int NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_ProviderAssignmentBatchStatuses] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260129014249_StandardizeLookupTableFK'
)
BEGIN
    CREATE TABLE [Clinical].[ProviderTypes] (
        [Id] int NOT NULL,
        [Code] nvarchar(50) NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [DisplayOrder] int NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_ProviderTypes] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260129014249_StandardizeLookupTableFK'
)
BEGIN

                    -- Seed ProviderTypes
                    INSERT INTO [Clinical].[ProviderTypes] ([Id], [Code], [Name], [DisplayOrder])
                    VALUES
                    (0, N'MD', N'Physician', 1),
                    (1, N'NP', N'Nurse Practitioner', 2);

                    -- Seed ProviderAssignmentBatchStatuses
                    INSERT INTO [staging].[ProviderAssignmentBatchStatuses] ([Id], [Code], [Name], [DisplayOrder])
                    VALUES
                    (0, N'Pending', N'Pending', 1),
                    (1, N'Imported', N'Imported', 2),
                    (2, N'Resolved', N'Resolved', 3),
                    (3, N'Processed', N'Processed', 4);
                
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260129014249_StandardizeLookupTableFK'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ServiceTypes_Code] ON [Clinical].[ServiceTypes] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260129014249_StandardizeLookupTableFK'
)
BEGIN
    CREATE INDEX [IX_Providers_ProviderTypeId] ON [Clinical].[Providers] ([ProviderTypeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260129014249_StandardizeLookupTableFK'
)
BEGIN
    CREATE UNIQUE INDEX [IX_HospitalizationStatuses_Code] ON [Clinical].[HospitalizationStatuses] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260129014249_StandardizeLookupTableFK'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ProviderAssignmentBatchStatuses_Code] ON [staging].[ProviderAssignmentBatchStatuses] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260129014249_StandardizeLookupTableFK'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ProviderTypes_Code] ON [Clinical].[ProviderTypes] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260129014249_StandardizeLookupTableFK'
)
BEGIN
    ALTER TABLE [staging].[ProviderAssignmentBatches] ADD CONSTRAINT [FK_ProviderAssignmentBatches_ProviderAssignmentBatchStatuses_StatusId] FOREIGN KEY ([StatusId]) REFERENCES [staging].[ProviderAssignmentBatchStatuses] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260129014249_StandardizeLookupTableFK'
)
BEGIN
    ALTER TABLE [Clinical].[Providers] ADD CONSTRAINT [FK_Providers_ProviderTypes_ProviderTypeId] FOREIGN KEY ([ProviderTypeId]) REFERENCES [Clinical].[ProviderTypes] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260129014249_StandardizeLookupTableFK'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260129014249_StandardizeLookupTableFK', N'9.0.12');
END;

COMMIT;
GO


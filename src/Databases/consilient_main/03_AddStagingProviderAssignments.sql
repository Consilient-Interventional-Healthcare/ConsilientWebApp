BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260123130002_AddStagingProviderAssignments'
)
BEGIN
    IF SCHEMA_ID(N'staging') IS NULL EXEC(N'CREATE SCHEMA [staging];');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260123130002_AddStagingProviderAssignments'
)
BEGIN
    CREATE TABLE [staging].[ProviderAssignmentBatches] (
        [Id] uniqueidentifier NOT NULL,
        [Date] date NOT NULL,
        [FacilityId] int NOT NULL,
        [Status] int NOT NULL DEFAULT 0,
        [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_ProviderAssignmentBatches] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260123130002_AddStagingProviderAssignments'
)
BEGIN
    CREATE TABLE [staging].[ProviderAssignments] (
        [Id] int NOT NULL IDENTITY,
        [Age] int NOT NULL,
        [AttendingMD] nvarchar(255) NOT NULL,
        [HospitalNumber] nvarchar(50) NOT NULL,
        [Admit] smalldatetime NOT NULL,
        [Dob] date NULL,
        [FacilityId] int NOT NULL,
        [Mrn] nvarchar(50) NOT NULL,
        [Name] nvarchar(255) NOT NULL,
        [Insurance] nvarchar(255) NOT NULL,
        [NursePractitioner] nvarchar(255) NOT NULL,
        [IsCleared] nvarchar(50) NOT NULL,
        [Location] nvarchar(255) NOT NULL,
        [ServiceDate] date NOT NULL,
        [H_P] nvarchar(255) NOT NULL,
        [PsychEval] nvarchar(255) NOT NULL,
        [BatchId] uniqueidentifier NOT NULL,
        [NormalizedPatientLastName] nvarchar(100) NULL,
        [NormalizedPatientFirstName] nvarchar(100) NULL,
        [NormalizedPhysicianLastName] nvarchar(100) NULL,
        [NormalizedNursePractitionerLastName] nvarchar(100) NULL,
        [Room] nvarchar(20) NULL,
        [Bed] nvarchar(5) NULL,
        [ResolvedPhysicianId] int NULL,
        [ResolvedHospitalizationId] int NULL,
        [ResolvedPatientId] int NULL,
        [ResolvedNursePractitionerId] int NULL,
        [ResolvedVisitId] int NULL,
        [ResolvedHospitalizationStatusId] int NULL,
        [ShouldImport] bit NOT NULL DEFAULT CAST(0 AS bit),
        [Imported] bit NOT NULL DEFAULT CAST(0 AS bit),
        [ValidationErrors] nvarchar(max) NULL,
        [ExclusionReason] nvarchar(500) NULL,
        [PatientWasCreated] bit NOT NULL DEFAULT CAST(0 AS bit),
        [PatientFacilityWasCreated] bit NOT NULL DEFAULT CAST(0 AS bit),
        [PhysicianWasCreated] bit NOT NULL DEFAULT CAST(0 AS bit),
        [NursePractitionerWasCreated] bit NOT NULL DEFAULT CAST(0 AS bit),
        [HospitalizationWasCreated] bit NOT NULL DEFAULT CAST(0 AS bit),
        [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_ProviderAssignments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProviderAssignments_HospitalizationStatuses_ResolvedHospitalizationStatusId] FOREIGN KEY ([ResolvedHospitalizationStatusId]) REFERENCES [Clinical].[HospitalizationStatuses] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProviderAssignments_Hospitalizations_ResolvedHospitalizationId] FOREIGN KEY ([ResolvedHospitalizationId]) REFERENCES [Clinical].[Hospitalizations] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProviderAssignments_Patients_ResolvedPatientId] FOREIGN KEY ([ResolvedPatientId]) REFERENCES [Clinical].[Patients] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProviderAssignments_ProviderAssignmentBatches_BatchId] FOREIGN KEY ([BatchId]) REFERENCES [staging].[ProviderAssignmentBatches] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ProviderAssignments_Providers_ResolvedNursePractitionerId] FOREIGN KEY ([ResolvedNursePractitionerId]) REFERENCES [Clinical].[Providers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProviderAssignments_Providers_ResolvedPhysicianId] FOREIGN KEY ([ResolvedPhysicianId]) REFERENCES [Clinical].[Providers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProviderAssignments_Visits_ResolvedVisitId] FOREIGN KEY ([ResolvedVisitId]) REFERENCES [Clinical].[Visits] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260123130002_AddStagingProviderAssignments'
)
BEGIN
    CREATE INDEX [IX_ProviderAssignmentBatches_FacilityId_Date] ON [staging].[ProviderAssignmentBatches] ([FacilityId], [Date]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260123130002_AddStagingProviderAssignments'
)
BEGIN
    CREATE INDEX [IX_ProviderAssignmentBatches_Status] ON [staging].[ProviderAssignmentBatches] ([Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260123130002_AddStagingProviderAssignments'
)
BEGIN
    CREATE INDEX [IX_ProviderAssignments_BatchId] ON [staging].[ProviderAssignments] ([BatchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260123130002_AddStagingProviderAssignments'
)
BEGIN
    CREATE INDEX [IX_ProviderAssignments_ResolvedHospitalizationId] ON [staging].[ProviderAssignments] ([ResolvedHospitalizationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260123130002_AddStagingProviderAssignments'
)
BEGIN
    CREATE INDEX [IX_ProviderAssignments_ResolvedHospitalizationStatusId] ON [staging].[ProviderAssignments] ([ResolvedHospitalizationStatusId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260123130002_AddStagingProviderAssignments'
)
BEGIN
    CREATE INDEX [IX_ProviderAssignments_ResolvedNursePractitionerId] ON [staging].[ProviderAssignments] ([ResolvedNursePractitionerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260123130002_AddStagingProviderAssignments'
)
BEGIN
    CREATE INDEX [IX_ProviderAssignments_ResolvedPatientId] ON [staging].[ProviderAssignments] ([ResolvedPatientId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260123130002_AddStagingProviderAssignments'
)
BEGIN
    CREATE INDEX [IX_ProviderAssignments_ResolvedPhysicianId] ON [staging].[ProviderAssignments] ([ResolvedPhysicianId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260123130002_AddStagingProviderAssignments'
)
BEGIN
    CREATE INDEX [IX_ProviderAssignments_ResolvedVisitId] ON [staging].[ProviderAssignments] ([ResolvedVisitId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260123130002_AddStagingProviderAssignments'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260123130002_AddStagingProviderAssignments', N'9.0.11');
END;

COMMIT;
GO


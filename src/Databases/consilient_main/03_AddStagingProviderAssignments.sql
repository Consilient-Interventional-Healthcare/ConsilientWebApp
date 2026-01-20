BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260119231310_AddStagingProviderAssignments'
)
BEGIN
    IF SCHEMA_ID(N'staging') IS NULL EXEC(N'CREATE SCHEMA [staging];');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260119231310_AddStagingProviderAssignments'
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
        CONSTRAINT [PK_ProviderAssignments] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260119231310_AddStagingProviderAssignments'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260119231310_AddStagingProviderAssignments', N'9.0.11');
END;

COMMIT;
GO


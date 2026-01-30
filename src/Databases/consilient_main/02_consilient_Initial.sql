-- ============================================
-- EF Core Migration Script (Auto-Generated)
-- ============================================
-- Context:     ConsilientDbContext
-- Migration:   20260130144349_Initial
-- Generated:   2026-01-30T14:43:52.9598212Z
-- ============================================
-- WARNING: This file is auto-generated.
-- It will be deleted by SquashMigrations.
-- Do not manually edit unless necessary.
-- ============================================

-- Required SET options for SQL Server indexed views and computed columns
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    IF SCHEMA_ID(N'Billing') IS NULL EXEC(N'CREATE SCHEMA [Billing];');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    IF SCHEMA_ID(N'Compensation') IS NULL EXEC(N'CREATE SCHEMA [Compensation];');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    IF SCHEMA_ID(N'Clinical') IS NULL EXEC(N'CREATE SCHEMA [Clinical];');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    IF SCHEMA_ID(N'staging') IS NULL EXEC(N'CREATE SCHEMA [staging];');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
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
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE TABLE [Compensation].[Employees] (
        [Id] int NOT NULL IDENTITY,
        [FirstName] nvarchar(50) NOT NULL,
        [LastName] nvarchar(50) NOT NULL,
        [TitleExtension] nvarchar(2) NULL,
        [Role] int NOT NULL,
        [Email] nvarchar(100) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Employees] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE TABLE [Clinical].[Facilities] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [Abbreviation] nvarchar(10) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Facilities] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE TABLE [Clinical].[HospitalizationStatuses] (
        [Id] int NOT NULL,
        [Code] nvarchar(20) NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [DisplayOrder] int NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_HospitalizationStatus] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE TABLE [Clinical].[Insurances] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(10) NOT NULL,
        [Description] nvarchar(100) NOT NULL,
        [PhysicianIncluded] bit NULL DEFAULT CAST(0 AS bit),
        [IsContracted] bit NULL DEFAULT CAST(0 AS bit),
        [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Insurances] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE TABLE [Clinical].[Patients] (
        [Id] int NOT NULL IDENTITY,
        [BirthDate] date NULL,
        [FirstName] nvarchar(50) NOT NULL,
        [Gender] int NULL,
        [LastName] nvarchar(50) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Patients] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
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
    WHERE [MigrationId] = N'20260130144349_Initial'
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
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE TABLE [Clinical].[ServiceTypes] (
        [Id] int NOT NULL,
        [Code] nvarchar(50) NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [DisplayOrder] int NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_ServiceType] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE TABLE [Clinical].[VisitEventTypes] (
        [Id] int NOT NULL,
        [Code] nvarchar(50) NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [DisplayOrder] int NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_VisitEventType] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE TABLE [Compensation].[ProviderContracts] (
        [Id] int NOT NULL IDENTITY,
        [EmployeeID] int NOT NULL,
        [FacilityId] int NOT NULL,
        [StartDate] date NOT NULL,
        [EndDate] date NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_ProviderContract] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProviderContracts_Employee] FOREIGN KEY ([EmployeeID]) REFERENCES [Compensation].[Employees] ([Id]),
        CONSTRAINT [FK_ProviderContracts_Facility] FOREIGN KEY ([FacilityId]) REFERENCES [Clinical].[Facilities] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE TABLE [Clinical].[Hospitalizations] (
        [Id] int NOT NULL IDENTITY,
        [PatientId] int NOT NULL,
        [CaseId] int NOT NULL,
        [FacilityId] int NOT NULL,
        [PsychEvaluation] bit NOT NULL,
        [AdmissionDate] datetime2 NOT NULL,
        [DischargeDate] datetime2 NULL,
        [HospitalizationStatusId] int NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Hospitalization] PRIMARY KEY ([Id]),
        CONSTRAINT [AK_Hospitalizations_CaseId] UNIQUE ([CaseId]),
        CONSTRAINT [AK_Hospitalizations_CaseId_PatientId] UNIQUE ([CaseId], [PatientId]),
        CONSTRAINT [FK_Hospitalizations_Facilities_FacilityId] FOREIGN KEY ([FacilityId]) REFERENCES [Clinical].[Facilities] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Hospitalizations_HospitalizationStatuses_HospitalizationStatusId] FOREIGN KEY ([HospitalizationStatusId]) REFERENCES [Clinical].[HospitalizationStatuses] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Hospitalizations_Patients_PatientId] FOREIGN KEY ([PatientId]) REFERENCES [Clinical].[Patients] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE TABLE [Clinical].[PatientFacilities] (
        [Id] int NOT NULL IDENTITY,
        [PatientId] int NOT NULL,
        [FacilityId] int NOT NULL,
        [MRN] nvarchar(450) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_PatientFacility] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PatientFacilities_Facilities] FOREIGN KEY ([FacilityId]) REFERENCES [Clinical].[Facilities] ([Id]),
        CONSTRAINT [FK_PatientFacilities_Patients] FOREIGN KEY ([PatientId]) REFERENCES [Clinical].[Patients] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE TABLE [staging].[ProviderAssignmentBatches] (
        [Id] uniqueidentifier NOT NULL,
        [Date] date NOT NULL,
        [FacilityId] int NOT NULL,
        [StatusId] int NOT NULL DEFAULT 0,
        [CreatedByUserId] int NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_ProviderAssignmentBatches] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProviderAssignmentBatches_ProviderAssignmentBatchStatuses_StatusId] FOREIGN KEY ([StatusId]) REFERENCES [staging].[ProviderAssignmentBatchStatuses] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE TABLE [Clinical].[Providers] (
        [Id] int NOT NULL IDENTITY,
        [FirstName] nvarchar(50) NOT NULL,
        [LastName] nvarchar(50) NOT NULL,
        [TitleExtension] nvarchar(10) NULL,
        [ProviderTypeId] int NOT NULL,
        [Email] nvarchar(100) NOT NULL,
        [EmployeeId] int NULL,
        [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Provider] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Providers_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Compensation].[Employees] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_Providers_ProviderTypes_ProviderTypeId] FOREIGN KEY ([ProviderTypeId]) REFERENCES [Clinical].[ProviderTypes] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE TABLE [Clinical].[HospitalizationInsurances] (
        [Id] int NOT NULL IDENTITY,
        [HospitalizationId] int NOT NULL,
        [InsuranceId] int NOT NULL,
        [StartDate] datetime2 NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_HospitalizationInsurance] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_HospitalizationInsurances_Hospitalizations] FOREIGN KEY ([HospitalizationId]) REFERENCES [Clinical].[Hospitalizations] ([Id]),
        CONSTRAINT [FK_HospitalizationInsurances_Insurances] FOREIGN KEY ([InsuranceId]) REFERENCES [Clinical].[Insurances] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE TABLE [Clinical].[HospitalizationStatusHistories] (
        [Id] int NOT NULL IDENTITY,
        [HospitalizationId] int NOT NULL,
        [NewStatusId] int NOT NULL,
        [ChangedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [ChangedByUserId] int NULL,
        [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_HospitalizationStatusHistory] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_HospitalizationStatusHistories_HospitalizationStatuses_NewStatusId] FOREIGN KEY ([NewStatusId]) REFERENCES [Clinical].[HospitalizationStatuses] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_HospitalizationStatusHistories_Hospitalizations_HospitalizationId] FOREIGN KEY ([HospitalizationId]) REFERENCES [Clinical].[Hospitalizations] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE TABLE [Clinical].[Visits] (
        [Id] int NOT NULL IDENTITY,
        [DateServiced] date NOT NULL,
        [HospitalizationId] int NOT NULL,
        [IsScribeServiceOnly] bit NOT NULL DEFAULT CAST(0 AS bit),
        [Room] nvarchar(20) NOT NULL,
        [Bed] nvarchar(5) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Visit] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Visits_Hospitalizations_HospitalizationId] FOREIGN KEY ([HospitalizationId]) REFERENCES [Clinical].[Hospitalizations] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
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
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE TABLE [Clinical].[VisitAttendants] (
        [Id] int NOT NULL IDENTITY,
        [VisitId] int NOT NULL,
        [ProviderId] int NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_VisitAttendant] PRIMARY KEY ([Id]),
        CONSTRAINT [AK_VisitAttendants_VisitId_ProviderId] UNIQUE ([VisitId], [ProviderId]),
        CONSTRAINT [FK_VisitAttendants_Providers_ProviderId] FOREIGN KEY ([ProviderId]) REFERENCES [Clinical].[Providers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_VisitAttendants_Visits_VisitId] FOREIGN KEY ([VisitId]) REFERENCES [Clinical].[Visits] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE TABLE [Clinical].[VisitEvents] (
        [Id] int NOT NULL IDENTITY,
        [VisitId] int NOT NULL,
        [EventTypeId] int NOT NULL,
        [EventOccurredAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [Description] nvarchar(max) NOT NULL,
        [EnteredByUserId] int NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_VisitEvent] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_VisitEvents_VisitEventTypes_EventTypeId] FOREIGN KEY ([EventTypeId]) REFERENCES [Clinical].[VisitEventTypes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_VisitEvents_Visits_VisitId] FOREIGN KEY ([VisitId]) REFERENCES [Clinical].[Visits] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
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
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE INDEX [IX_HospitalizationInsurances_HospitalizationId] ON [Clinical].[HospitalizationInsurances] ([HospitalizationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE INDEX [IX_HospitalizationInsurances_InsuranceId] ON [Clinical].[HospitalizationInsurances] ([InsuranceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE INDEX [IX_Hospitalizations_FacilityId] ON [Clinical].[Hospitalizations] ([FacilityId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE INDEX [IX_Hospitalizations_HospitalizationStatusId] ON [Clinical].[Hospitalizations] ([HospitalizationStatusId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE INDEX [IX_Hospitalizations_PatientId] ON [Clinical].[Hospitalizations] ([PatientId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_HospitalizationStatuses_Code] ON [Clinical].[HospitalizationStatuses] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE INDEX [IX_HospitalizationStatusHistories_ChangedAt] ON [Clinical].[HospitalizationStatusHistories] ([ChangedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE INDEX [IX_HospitalizationStatusHistories_ChangedByUserId] ON [Clinical].[HospitalizationStatusHistories] ([ChangedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE INDEX [IX_HospitalizationStatusHistories_HospitalizationId] ON [Clinical].[HospitalizationStatusHistories] ([HospitalizationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE INDEX [IX_HospitalizationStatusHistories_NewStatusId] ON [Clinical].[HospitalizationStatusHistories] ([NewStatusId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PatientFacilities_FacilityId_MRN] ON [Clinical].[PatientFacilities] ([FacilityId], [MRN]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE INDEX [IX_PatientFacilities_PatientId] ON [Clinical].[PatientFacilities] ([PatientId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE INDEX [IX_ProviderAssignmentBatches_CreatedByUserId] ON [staging].[ProviderAssignmentBatches] ([CreatedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE INDEX [IX_ProviderAssignmentBatches_FacilityId_Date] ON [staging].[ProviderAssignmentBatches] ([FacilityId], [Date]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE INDEX [IX_ProviderAssignmentBatches_Status] ON [staging].[ProviderAssignmentBatches] ([StatusId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ProviderAssignmentBatchStatuses_Code] ON [staging].[ProviderAssignmentBatchStatuses] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE INDEX [IX_ProviderAssignments_BatchId] ON [staging].[ProviderAssignments] ([BatchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE INDEX [IX_ProviderAssignments_ResolvedHospitalizationId] ON [staging].[ProviderAssignments] ([ResolvedHospitalizationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE INDEX [IX_ProviderAssignments_ResolvedHospitalizationStatusId] ON [staging].[ProviderAssignments] ([ResolvedHospitalizationStatusId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE INDEX [IX_ProviderAssignments_ResolvedNursePractitionerId] ON [staging].[ProviderAssignments] ([ResolvedNursePractitionerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE INDEX [IX_ProviderAssignments_ResolvedPatientId] ON [staging].[ProviderAssignments] ([ResolvedPatientId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE INDEX [IX_ProviderAssignments_ResolvedPhysicianId] ON [staging].[ProviderAssignments] ([ResolvedPhysicianId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE INDEX [IX_ProviderAssignments_ResolvedVisitId] ON [staging].[ProviderAssignments] ([ResolvedVisitId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE INDEX [IX_ProviderContracts_EmployeeID] ON [Compensation].[ProviderContracts] ([EmployeeID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE INDEX [IX_ProviderContracts_FacilityId] ON [Compensation].[ProviderContracts] ([FacilityId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Providers_EmployeeId] ON [Clinical].[Providers] ([EmployeeId]) WHERE [EmployeeId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE INDEX [IX_Providers_ProviderTypeId] ON [Clinical].[Providers] ([ProviderTypeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ProviderTypes_Code] ON [Clinical].[ProviderTypes] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ServiceTypes_Code] ON [Clinical].[ServiceTypes] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE INDEX [IX_VisitAttendants_ProviderId] ON [Clinical].[VisitAttendants] ([ProviderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE INDEX [IX_VisitAttendants_VisitId] ON [Clinical].[VisitAttendants] ([VisitId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE INDEX [IX_VisitEvents_EnteredByUserId] ON [Clinical].[VisitEvents] ([EnteredByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE INDEX [IX_VisitEvents_EventOccurredAt] ON [Clinical].[VisitEvents] ([EventOccurredAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE INDEX [IX_VisitEvents_EventTypeId] ON [Clinical].[VisitEvents] ([EventTypeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE INDEX [IX_VisitEvents_VisitId] ON [Clinical].[VisitEvents] ([VisitId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_VisitEventTypes_Code] ON [Clinical].[VisitEventTypes] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE INDEX [IX_Visits_DateServiced] ON [Clinical].[Visits] ([DateServiced]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE INDEX [IX_Visits_HospitalizationId] ON [Clinical].[Visits] ([HospitalizationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE INDEX [IX_VisitServiceBillings_BillingCodeId] ON [Billing].[VisitServiceBillings] ([BillingCodeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE INDEX [IX_VisitServiceBillings_ServiceTypeId] ON [Billing].[VisitServiceBillings] ([ServiceTypeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    CREATE INDEX [IX_VisitServiceBillings_VisitId] ON [Billing].[VisitServiceBillings] ([VisitId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260130144349_Initial'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260130144349_Initial', N'9.0.12');
END;

COMMIT;
GO


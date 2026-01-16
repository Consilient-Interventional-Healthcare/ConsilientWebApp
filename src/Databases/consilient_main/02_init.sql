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
    WHERE [MigrationId] = N'20260116154956_Initial'
)
BEGIN
    IF SCHEMA_ID(N'Compensation') IS NULL EXEC(N'CREATE SCHEMA [Compensation];');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260116154956_Initial'
)
BEGIN
    IF SCHEMA_ID(N'Clinical') IS NULL EXEC(N'CREATE SCHEMA [Clinical];');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260116154956_Initial'
)
BEGIN
    IF SCHEMA_ID(N'staging') IS NULL EXEC(N'CREATE SCHEMA [staging];');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260116154956_Initial'
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
    WHERE [MigrationId] = N'20260116154956_Initial'
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
    WHERE [MigrationId] = N'20260116154956_Initial'
)
BEGIN
    CREATE TABLE [Clinical].[HospitalizationStatuses] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(20) NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [BillingCode] nvarchar(max) NOT NULL,
        [Color] nvarchar(20) NOT NULL,
        [DisplayOrder] int NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_HospitalizationStatus] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260116154956_Initial'
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
    WHERE [MigrationId] = N'20260116154956_Initial'
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
    WHERE [MigrationId] = N'20260116154956_Initial'
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
        [ResolvedProviderId] int NULL,
        [ResolvedHospitalizationId] int NULL,
        [ResolvedPatientId] int NULL,
        [ResolvedNursePracticionerId] int NULL,
        [BatchId] uniqueidentifier NOT NULL,
        [Imported] bit NOT NULL DEFAULT CAST(0 AS bit),
        [ValidationErrors] nvarchar(max) NULL,
        [ExclusionReason] nvarchar(500) NULL,
        [ShouldImport] bit NOT NULL DEFAULT CAST(0 AS bit),
        [NeedsNewPatient] bit NOT NULL DEFAULT CAST(0 AS bit),
        [NeedsNewHospitalization] bit NOT NULL DEFAULT CAST(0 AS bit),
        [ResolvedVisitId] int NULL,
        [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_ProviderAssignments] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260116154956_Initial'
)
BEGIN
    CREATE TABLE [Clinical].[ServiceTypes] (
        [Id] int NOT NULL IDENTITY,
        [Description] nvarchar(100) NOT NULL,
        [CPTCode] int NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_ServiceTypes] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260116154956_Initial'
)
BEGIN
    CREATE TABLE [Clinical].[VisitEventTypes] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(50) NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_VisitEventType] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260116154956_Initial'
)
BEGIN
    CREATE TABLE [Clinical].[Providers] (
        [Id] int NOT NULL IDENTITY,
        [FirstName] nvarchar(50) NOT NULL,
        [LastName] nvarchar(50) NOT NULL,
        [TitleExtension] nvarchar(10) NULL,
        [Type] int NOT NULL,
        [Email] nvarchar(100) NOT NULL,
        [EmployeeId] int NULL,
        [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Provider] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Providers_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Compensation].[Employees] ([Id]) ON DELETE SET NULL
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260116154956_Initial'
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
    WHERE [MigrationId] = N'20260116154956_Initial'
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
    WHERE [MigrationId] = N'20260116154956_Initial'
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
    WHERE [MigrationId] = N'20260116154956_Initial'
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
    WHERE [MigrationId] = N'20260116154956_Initial'
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
    WHERE [MigrationId] = N'20260116154956_Initial'
)
BEGIN
    CREATE TABLE [Clinical].[Visits] (
        [Id] int NOT NULL IDENTITY,
        [DateServiced] date NOT NULL,
        [HospitalizationId] int NOT NULL,
        [IsScribeServiceOnly] bit NOT NULL DEFAULT CAST(0 AS bit),
        [ServiceTypeId] int NOT NULL,
        [Room] nvarchar(20) NOT NULL,
        [Bed] nvarchar(5) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Visit] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Visits_Hospitalizations_HospitalizationId] FOREIGN KEY ([HospitalizationId]) REFERENCES [Clinical].[Hospitalizations] ([Id]),
        CONSTRAINT [FK_Visits_ServiceTypes_ServiceTypeId] FOREIGN KEY ([ServiceTypeId]) REFERENCES [Clinical].[ServiceTypes] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260116154956_Initial'
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
    WHERE [MigrationId] = N'20260116154956_Initial'
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
    WHERE [MigrationId] = N'20260116154956_Initial'
)
BEGIN
    CREATE INDEX [IX_HospitalizationInsurances_HospitalizationId] ON [Clinical].[HospitalizationInsurances] ([HospitalizationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260116154956_Initial'
)
BEGIN
    CREATE INDEX [IX_HospitalizationInsurances_InsuranceId] ON [Clinical].[HospitalizationInsurances] ([InsuranceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260116154956_Initial'
)
BEGIN
    CREATE INDEX [IX_Hospitalizations_FacilityId] ON [Clinical].[Hospitalizations] ([FacilityId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260116154956_Initial'
)
BEGIN
    CREATE INDEX [IX_Hospitalizations_HospitalizationStatusId] ON [Clinical].[Hospitalizations] ([HospitalizationStatusId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260116154956_Initial'
)
BEGIN
    CREATE INDEX [IX_Hospitalizations_PatientId] ON [Clinical].[Hospitalizations] ([PatientId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260116154956_Initial'
)
BEGIN
    CREATE INDEX [IX_HospitalizationStatusHistories_ChangedAt] ON [Clinical].[HospitalizationStatusHistories] ([ChangedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260116154956_Initial'
)
BEGIN
    CREATE INDEX [IX_HospitalizationStatusHistories_ChangedByUserId] ON [Clinical].[HospitalizationStatusHistories] ([ChangedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260116154956_Initial'
)
BEGIN
    CREATE INDEX [IX_HospitalizationStatusHistories_HospitalizationId] ON [Clinical].[HospitalizationStatusHistories] ([HospitalizationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260116154956_Initial'
)
BEGIN
    CREATE INDEX [IX_HospitalizationStatusHistories_NewStatusId] ON [Clinical].[HospitalizationStatusHistories] ([NewStatusId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260116154956_Initial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PatientFacilities_FacilityId_MRN] ON [Clinical].[PatientFacilities] ([FacilityId], [MRN]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260116154956_Initial'
)
BEGIN
    CREATE INDEX [IX_PatientFacilities_PatientId] ON [Clinical].[PatientFacilities] ([PatientId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260116154956_Initial'
)
BEGIN
    CREATE INDEX [IX_ProviderContracts_EmployeeID] ON [Compensation].[ProviderContracts] ([EmployeeID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260116154956_Initial'
)
BEGIN
    CREATE INDEX [IX_ProviderContracts_FacilityId] ON [Compensation].[ProviderContracts] ([FacilityId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260116154956_Initial'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Providers_EmployeeId] ON [Clinical].[Providers] ([EmployeeId]) WHERE [EmployeeId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260116154956_Initial'
)
BEGIN
    CREATE INDEX [IX_VisitAttendants_ProviderId] ON [Clinical].[VisitAttendants] ([ProviderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260116154956_Initial'
)
BEGIN
    CREATE INDEX [IX_VisitAttendants_VisitId] ON [Clinical].[VisitAttendants] ([VisitId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260116154956_Initial'
)
BEGIN
    CREATE INDEX [IX_VisitEvents_EnteredByUserId] ON [Clinical].[VisitEvents] ([EnteredByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260116154956_Initial'
)
BEGIN
    CREATE INDEX [IX_VisitEvents_EventOccurredAt] ON [Clinical].[VisitEvents] ([EventOccurredAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260116154956_Initial'
)
BEGIN
    CREATE INDEX [IX_VisitEvents_EventTypeId] ON [Clinical].[VisitEvents] ([EventTypeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260116154956_Initial'
)
BEGIN
    CREATE INDEX [IX_VisitEvents_VisitId] ON [Clinical].[VisitEvents] ([VisitId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260116154956_Initial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_VisitEventTypes_Code] ON [Clinical].[VisitEventTypes] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260116154956_Initial'
)
BEGIN
    CREATE INDEX [IX_Visits_DateServiced] ON [Clinical].[Visits] ([DateServiced]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260116154956_Initial'
)
BEGIN
    CREATE INDEX [IX_Visits_HospitalizationId] ON [Clinical].[Visits] ([HospitalizationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260116154956_Initial'
)
BEGIN
    CREATE INDEX [IX_Visits_ServiceTypeId] ON [Clinical].[Visits] ([ServiceTypeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260116154956_Initial'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260116154956_Initial', N'9.0.11');
END;

COMMIT;
GO


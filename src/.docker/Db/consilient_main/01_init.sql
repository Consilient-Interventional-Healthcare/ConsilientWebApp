SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
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
    WHERE [MigrationId] = N'20251124205308_Initial'
)
BEGIN
	IF DATABASE_PRINCIPAL_ID('compensation_full') IS NULL
		CREATE ROLE [compensation_full]

	IF DATABASE_PRINCIPAL_ID('clinical_full') IS NULL
		CREATE ROLE [clinical_full]

	IF DATABASE_PRINCIPAL_ID('admin_full') IS NULL
		CREATE ROLE [admin_full]

    IF SCHEMA_ID(N'Compensation') IS NULL EXEC(N'CREATE SCHEMA [Compensation];');
    IF SCHEMA_ID(N'Clinical') IS NULL EXEC(N'CREATE SCHEMA [Clinical];');

    CREATE TABLE [Compensation].[Employees] (
        [Id] int NOT NULL IDENTITY,
        [FirstName] nvarchar(50) NOT NULL,
        [LastName] nvarchar(50) NOT NULL,
        [TitleExtension] nvarchar(2) NULL,
        [IsProvider] bit NOT NULL DEFAULT CAST(0 AS bit),
        [Role] nvarchar(50) NOT NULL,
        [IsAdministrator] bit NOT NULL DEFAULT CAST(0 AS bit),
        [Email] nvarchar(100) NULL,
        [CanApproveVisits] bit NOT NULL DEFAULT CAST(0 AS bit),
        [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Employees] PRIMARY KEY ([Id])
    );

    CREATE TABLE [Clinical].[Facilities] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [Abbreviation] nvarchar(10) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Facilities] PRIMARY KEY ([Id])
    );
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

    CREATE TABLE [Clinical].[ServiceTypes] (
        [Id] int NOT NULL IDENTITY,
        [Description] nvarchar(100) NOT NULL,
        [CPTCode] int NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_ServiceTypes] PRIMARY KEY ([Id])
    );

    CREATE TABLE [Clinical].[VisitEventTypes] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(50) NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_VisitEventType] PRIMARY KEY ([Id])
    );
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
    CREATE TABLE [Clinical].[PatientFacilities] (
        [Id] int NOT NULL IDENTITY,
        [PatientId] int NOT NULL,
        [FacilityId] int NOT NULL,
        [MRN] int NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_PatientFacility] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PatientFacilities_Facilities] FOREIGN KEY ([FacilityId]) REFERENCES [Clinical].[Facilities] ([Id]),
        CONSTRAINT [FK_PatientFacilities_Patients] FOREIGN KEY ([PatientId]) REFERENCES [Clinical].[Patients] ([Id])
    );

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
        CONSTRAINT [FK_HospitalizationStatusHistories_Users_ChangedByUserId] CHECK (([ChangedByUserId] IS NULL) OR EXISTS (SELECT 1 FROM [Identity].[Users] WHERE [Id] = [ChangedByUserId])),
        CONSTRAINT [FK_HospitalizationStatusHistories_HospitalizationStatuses_NewStatusId] FOREIGN KEY ([NewStatusId]) REFERENCES [Clinical].[HospitalizationStatuses] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_HospitalizationStatusHistories_Hospitalizations_HospitalizationId] FOREIGN KEY ([HospitalizationId]) REFERENCES [Clinical].[Hospitalizations] ([Id]) ON DELETE NO ACTION
    );
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
    
    CREATE TABLE [Clinical].[VisitAttendants] (
        [Id] int NOT NULL IDENTITY,
        [VisitId] int NOT NULL,
        [EmployeeId] int NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_VisitAttendant] PRIMARY KEY ([Id]),
        CONSTRAINT [AK_VisitAttendants_VisitId_EmployeeId] UNIQUE ([VisitId], [EmployeeId]),
        CONSTRAINT [FK_VisitAttendants_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Compensation].[Employees] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_VisitAttendants_Visits_VisitId] FOREIGN KEY ([VisitId]) REFERENCES [Clinical].[Visits] ([Id]) ON DELETE NO ACTION
    );

    CREATE TABLE [Clinical].[VisitEvents] (
        [Id] int NOT NULL IDENTITY,
        [VisitId] int NOT NULL,
        [EventTypeId] int NOT NULL,
        [EventOccurredAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [Description] nvarchar(max) NOT NULL,
        [EnteredByUserId] int NOT NULL,
        [EmployeeId] int NULL,
        [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_VisitEvent] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_VisitEvents_Users_EnteredByUserId] CHECK (EXISTS (SELECT 1 FROM [Identity].[Users] WHERE [Id] = [EnteredByUserId])),
        CONSTRAINT [FK_VisitEvents_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Compensation].[Employees] ([Id]),
        CONSTRAINT [FK_VisitEvents_VisitEventTypes_EventTypeId] FOREIGN KEY ([EventTypeId]) REFERENCES [Clinical].[VisitEventTypes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_VisitEvents_Visits_VisitId] FOREIGN KEY ([VisitId]) REFERENCES [Clinical].[Visits] ([Id]) ON DELETE NO ACTION
    );
    CREATE INDEX [IX_HospitalizationInsurances_HospitalizationId] ON [Clinical].[HospitalizationInsurances] ([HospitalizationId]);
    CREATE INDEX [IX_HospitalizationInsurances_InsuranceId] ON [Clinical].[HospitalizationInsurances] ([InsuranceId]);

    CREATE INDEX [IX_Hospitalizations_FacilityId] ON [Clinical].[Hospitalizations] ([FacilityId]);

    CREATE INDEX [IX_Hospitalizations_PatientId] ON [Clinical].[Hospitalizations] ([PatientId]);
    CREATE INDEX [IX_HospitalizationStatusHistories_ChangedAt] ON [Clinical].[HospitalizationStatusHistories] ([ChangedAt]);
    CREATE INDEX [IX_HospitalizationStatusHistories_ChangedByUserId] ON [Clinical].[HospitalizationStatusHistories] ([ChangedByUserId]);

    CREATE INDEX [IX_HospitalizationStatusHistories_HospitalizationId] ON [Clinical].[HospitalizationStatusHistories] ([HospitalizationId]);
    CREATE INDEX [IX_HospitalizationStatusHistories_NewStatusId] ON [Clinical].[HospitalizationStatusHistories] ([NewStatusId]);
    CREATE UNIQUE INDEX [IX_PatientFacilities_FacilityId_MRN] ON [Clinical].[PatientFacilities] ([FacilityId], [MRN]);
    CREATE INDEX [IX_PatientFacilities_PatientId] ON [Clinical].[PatientFacilities] ([PatientId]);
    CREATE INDEX [IX_VisitAttendants_EmployeeId] ON [Clinical].[VisitAttendants] ([EmployeeId]);

    CREATE INDEX [IX_VisitAttendants_VisitId] ON [Clinical].[VisitAttendants] ([VisitId]);
    CREATE INDEX [IX_VisitEvents_EmployeeId] ON [Clinical].[VisitEvents] ([EmployeeId]);
    CREATE INDEX [IX_VisitEvents_EnteredByUserId] ON [Clinical].[VisitEvents] ([EnteredByUserId]);
    CREATE INDEX [IX_VisitEvents_EventOccurredAt] ON [Clinical].[VisitEvents] ([EventOccurredAt]);
    CREATE INDEX [IX_VisitEvents_EventTypeId] ON [Clinical].[VisitEvents] ([EventTypeId]);
    CREATE INDEX [IX_VisitEvents_VisitId] ON [Clinical].[VisitEvents] ([VisitId]);

    CREATE UNIQUE INDEX [IX_VisitEventTypes_Code] ON [Clinical].[VisitEventTypes] ([Code]);
    CREATE INDEX [IX_Visits_DateServiced] ON [Clinical].[Visits] ([DateServiced]);

    CREATE INDEX [IX_Visits_HospitalizationId] ON [Clinical].[Visits] ([HospitalizationId]);
    
    CREATE INDEX [IX_Visits_ServiceTypeId] ON [Clinical].[Visits] ([ServiceTypeId]);

    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251124205308_Initial', N'9.0.11');
END;

COMMIT;
GO


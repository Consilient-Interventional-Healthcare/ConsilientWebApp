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
        [MRN] int NOT NULL,
        [FirstName] nvarchar(50) NOT NULL,
        [LastName] nvarchar(50) NOT NULL,
        [BirthDate] date NULL,
        [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Patients] PRIMARY KEY ([Id]),
        CONSTRAINT [AK_Patients_MRN] UNIQUE ([MRN])
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

    CREATE TABLE [Clinical].[VisitsStaging] (
        [Id] int NOT NULL IDENTITY,
        [DateServiced] date NOT NULL,
        [PatientId] int NOT NULL,
        [FacilityId] int NOT NULL,
        [AdmissionNumber] int NULL,
        [InsuranceId] int NULL,
        [ServiceTypeId] int NULL,
        [PhysicianEmployeeId] int NOT NULL,
        [NursePractitionerEmployeeId] int NULL,
        [ScribeEmployeeId] int NULL,
        [NursePractitionerApproved] bit NOT NULL DEFAULT CAST(0 AS bit),
        [PhysicianApproved] bit NOT NULL DEFAULT CAST(0 AS bit),
        [PhysicianApprovedBy] nvarchar(100) NULL,
        [PhysicianApprovedDateTime] datetime2 NULL,
        [AddedToMainTable] bit NOT NULL DEFAULT CAST(0 AS bit),
        [CosigningPhysicianEmployeeId] int NULL,
        [IsScribeServiceOnly] bit NOT NULL DEFAULT CAST(0 AS bit),
        [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_VisitsStaging] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_VisitsStaging_CosignPhysicianEmployee] FOREIGN KEY ([CosigningPhysicianEmployeeId]) REFERENCES [Compensation].[Employees] ([Id]),
        CONSTRAINT [FK_VisitsStaging_Facility] FOREIGN KEY ([FacilityId]) REFERENCES [Clinical].[Facilities] ([Id]),
        CONSTRAINT [FK_VisitsStaging_Insurance] FOREIGN KEY ([InsuranceId]) REFERENCES [Clinical].[Insurances] ([Id]),
        CONSTRAINT [FK_VisitsStaging_NursePractitioner] FOREIGN KEY ([NursePractitionerEmployeeId]) REFERENCES [Compensation].[Employees] ([Id]),
        CONSTRAINT [FK_VisitsStaging_Patient] FOREIGN KEY ([PatientId]) REFERENCES [Clinical].[Patients] ([Id]),
        CONSTRAINT [FK_VisitsStaging_Physician] FOREIGN KEY ([PhysicianEmployeeId]) REFERENCES [Compensation].[Employees] ([Id]),
        CONSTRAINT [FK_VisitsStaging_Scribe] FOREIGN KEY ([ScribeEmployeeId]) REFERENCES [Compensation].[Employees] ([Id]),
        CONSTRAINT [FK_VisitsStaging_ServiceType] FOREIGN KEY ([ServiceTypeId]) REFERENCES [Clinical].[ServiceTypes] ([Id])
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

    CREATE INDEX [IX_HospitalizationInsurances_HospitalizationId] ON [Clinical].[HospitalizationInsurances] ([HospitalizationId]);
	
    CREATE INDEX [IX_HospitalizationInsurances_InsuranceId] ON [Clinical].[HospitalizationInsurances] ([InsuranceId]);

    CREATE INDEX [IX_Hospitalizations_FacilityId] ON [Clinical].[Hospitalizations] ([FacilityId]);

    CREATE INDEX [IX_Hospitalizations_PatientId] ON [Clinical].[Hospitalizations] ([PatientId]);

    CREATE INDEX [IX_VisitAttendants_EmployeeId] ON [Clinical].[VisitAttendants] ([EmployeeId]);

    CREATE INDEX [IX_VisitAttendants_VisitId] ON [Clinical].[VisitAttendants] ([VisitId]);

    CREATE INDEX [IX_Visits_DateServiced] ON [Clinical].[Visits] ([DateServiced]);

    CREATE INDEX [IX_Visits_HospitalizationId] ON [Clinical].[Visits] ([HospitalizationId]);
    
    CREATE INDEX [IX_Visits_ServiceTypeId] ON [Clinical].[Visits] ([ServiceTypeId]);

    CREATE INDEX [IX_VisitsStaging_CosigningPhysicianEmployeeId] ON [Clinical].[VisitsStaging] ([CosigningPhysicianEmployeeId]);

    CREATE INDEX [IX_VisitsStaging_FacilityId] ON [Clinical].[VisitsStaging] ([FacilityId]);

    CREATE INDEX [IX_VisitsStaging_InsuranceId] ON [Clinical].[VisitsStaging] ([InsuranceId]);

    CREATE INDEX [IX_VisitsStaging_NursePractitionerEmployeeId] ON [Clinical].[VisitsStaging] ([NursePractitionerEmployeeId]);

    CREATE INDEX [IX_VisitsStaging_PatientId] ON [Clinical].[VisitsStaging] ([PatientId]);

    CREATE INDEX [IX_VisitsStaging_PhysicianEmployeeId] ON [Clinical].[VisitsStaging] ([PhysicianEmployeeId]);

    CREATE INDEX [IX_VisitsStaging_ScribeEmployeeId] ON [Clinical].[VisitsStaging] ([ScribeEmployeeId]);

    CREATE INDEX [IX_VisitsStaging_ServiceTypeId] ON [Clinical].[VisitsStaging] ([ServiceTypeId]);
	
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251124205308_Initial', N'9.0.11');
END;

COMMIT;
GO


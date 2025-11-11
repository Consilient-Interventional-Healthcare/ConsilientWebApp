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
    WHERE [MigrationId] = N'20251111023227_Initial'
)
BEGIN
	IF DATABASE_PRINCIPAL_ID('compensation_full') IS NULL
		CREATE ROLE [compensation_full]

	IF DATABASE_PRINCIPAL_ID('clinical_full') IS NULL
		CREATE ROLE [clinical_full]

	IF DATABASE_PRINCIPAL_ID('billing_full') IS NULL
		CREATE ROLE [billing_full]

	IF DATABASE_PRINCIPAL_ID('admin_full') IS NULL
		CREATE ROLE [admin_full]

    IF SCHEMA_ID(N'Billing') IS NULL EXEC(N'CREATE SCHEMA [Billing];');
    IF SCHEMA_ID(N'Clinical') IS NULL EXEC(N'CREATE SCHEMA [Clinical];');
    IF SCHEMA_ID(N'Compensation') IS NULL EXEC(N'CREATE SCHEMA [Compensation];');
    IF SCHEMA_ID(N'Reference') IS NULL EXEC(N'CREATE SCHEMA [Reference];');
    
    CREATE TABLE [Compensation].[Employees] (
        [EmployeeID] int NOT NULL IDENTITY,
        [FirstName] nvarchar(50) NULL,
        [LastName] nvarchar(50) NULL,
        [TitleExtension] nvarchar(2) NULL,
        [IsProvider] bit NOT NULL,
        [Role] nvarchar(50) NULL,
        [IsAdministrator] bit NOT NULL,
        [Email] nvarchar(100) NULL,
        [CanApproveVisits] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
        [UpdatedAtUtc] datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Employees] PRIMARY KEY ([EmployeeID])
    );
    
    ALTER TABLE [Compensation].[Employees] ADD  DEFAULT ((0)) FOR [IsProvider]
    ALTER TABLE [Compensation].[Employees] ADD  DEFAULT ((0)) FOR [IsAdministrator]
    ALTER TABLE [Compensation].[Employees] ADD  DEFAULT ((0)) FOR [CanApproveVisits]

    CREATE TABLE [Clinical].[Facilities] (
        [FacilityID] int NOT NULL IDENTITY,
        [FacilityName] nvarchar(100) NULL,
        [FacilityAbbreviation] nvarchar(10) NULL,
        [CreatedAtUtc] datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
        [UpdatedAtUtc] datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Facilities] PRIMARY KEY ([FacilityID])
    );
    
    CREATE TABLE [Clinical].[Insurances] (
        [InsuranceID] int NOT NULL IDENTITY,
        [InsuranceCode] nvarchar(10) NULL,
        [InsuranceDescription] nvarchar(100) NULL,
        [PhysicianIncluded] bit NULL DEFAULT CAST(0 AS bit),
        [IsContracted] bit NULL DEFAULT CAST(0 AS bit),
        [CreatedAtUtc] datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
        [UpdatedAtUtc] datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Insurances] PRIMARY KEY ([InsuranceID])
    );
       
    CREATE TABLE [Clinical].[Patients] (
        [PatientID] int NOT NULL IDENTITY,
        [PatientMRN] int NOT NULL,
        [PatientFirstName] nvarchar(50) NULL,
        [PatientLastName] nvarchar(50) NULL,
        [PatientBirthDate] date NULL,
        [CreatedAtUtc] datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
        [UpdatedAtUtc] datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Patients] PRIMARY KEY ([PatientID]),
        CONSTRAINT [AK_Patients_PatientMrn] UNIQUE ([PatientMRN])
    );
    
    CREATE TABLE [Clinical].[ServiceTypes] (
        [ServiceTypeID] int NOT NULL IDENTITY,
        [Description] nvarchar(100) NULL,
        [CPTCode] int NULL,
        [CreatedAtUtc] datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
        [UpdatedAtUtc] datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_ServiceTypes] PRIMARY KEY ([ServiceTypeID])
    );
    
    CREATE TABLE [Clinical].[Hospitalizations] (
        [Id] int NOT NULL IDENTITY,
        [PatientId] int NOT NULL,
        [CaseId] int NOT NULL,
        [FacilityId] int NOT NULL,
        [AdmissionDate] date NOT NULL,
        [DischargeDate] date NULL,
        [CreatedAtUtc] datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
        [UpdatedAtUtc] datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Hospitalization] PRIMARY KEY ([Id]),
        CONSTRAINT [AK_Hospitalizations_CaseId] UNIQUE ([CaseId]),
        CONSTRAINT [AK_Hospitalizations_CaseId_PatientId] UNIQUE ([CaseId], [PatientId]),
        CONSTRAINT [FK_Hospitalizations_Facilities_FacilityId] FOREIGN KEY ([FacilityId]) REFERENCES [Clinical].[Facilities] ([FacilityID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Hospitalizations_Patients_PatientId] FOREIGN KEY ([PatientId]) REFERENCES [Clinical].[Patients] ([PatientID]) ON DELETE NO ACTION
    );
    
    CREATE TABLE [Clinical].[PatientVisits_Staging] (
        [PatientVisit_StagingID] int NOT NULL IDENTITY,
        [DateServiced] date NOT NULL,
        [PatientID] int NOT NULL,
        [FacilityID] int NOT NULL,
        [AdmissionNumber] int NULL,
        [InsuranceID] int NULL,
        [ServiceTypeID] int NULL,
        [PhysicianEmployeeID] int NOT NULL,
        [NursePractitionerEmployeeID] int NULL,
        [ScribeEmployeeID] int NULL,
        [NursePractitionerApproved] bit NOT NULL,
        [PhysicianApproved] bit NOT NULL,
        [PhysicianApprovedBy] nvarchar(100) NULL,
        [PhysicianApprovedDateTime] datetime NULL,
        [AddedToMainTable] bit NOT NULL,
        [CosigningPhysicianEmployeeID] int NULL,
        [IsScribeServiceOnly] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
        [UpdatedAtUtc] datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_PatientVisits_Staging] PRIMARY KEY ([PatientVisit_StagingID]),
        CONSTRAINT [FK_PatientVisits_Staging_CosignPhysicianEmployee] FOREIGN KEY ([CosigningPhysicianEmployeeID]) REFERENCES [Compensation].[Employees] ([EmployeeID]),
        CONSTRAINT [FK_PatientVisits_Staging_Facility] FOREIGN KEY ([FacilityID]) REFERENCES [Clinical].[Facilities] ([FacilityID]),
        CONSTRAINT [FK_PatientVisits_Staging_Insurance] FOREIGN KEY ([InsuranceID]) REFERENCES [Clinical].[Insurances] ([InsuranceID]),
        CONSTRAINT [FK_PatientVisits_Staging_NursePractitioner] FOREIGN KEY ([NursePractitionerEmployeeID]) REFERENCES [Compensation].[Employees] ([EmployeeID]),
        CONSTRAINT [FK_PatientVisits_Staging_Patient] FOREIGN KEY ([PatientID]) REFERENCES [Clinical].[Patients] ([PatientID]),
        CONSTRAINT [FK_PatientVisits_Staging_Physician] FOREIGN KEY ([PhysicianEmployeeID]) REFERENCES [Compensation].[Employees] ([EmployeeID]),
        CONSTRAINT [FK_PatientVisits_Staging_Scribe] FOREIGN KEY ([ScribeEmployeeID]) REFERENCES [Compensation].[Employees] ([EmployeeID]),
        CONSTRAINT [FK_PatientVisits_Staging_ServiceType] FOREIGN KEY ([ServiceTypeID]) REFERENCES [Clinical].[ServiceTypes] ([ServiceTypeID])
    );
    
    ALTER TABLE [Clinical].[PatientVisits_Staging] ADD  DEFAULT ((0)) FOR [NursePractitionerApproved]
    ALTER TABLE [Clinical].[PatientVisits_Staging] ADD  DEFAULT ((0)) FOR [PhysicianApproved]
    ALTER TABLE [Clinical].[PatientVisits_Staging] ADD  DEFAULT ((0)) FOR [AddedToMainTable]
    ALTER TABLE [Clinical].[PatientVisits_Staging] ADD  DEFAULT ((0)) FOR [IsScribeServiceOnly]
    
    CREATE TABLE [Clinical].[PatientVisits] (
        [PatientVisitID] int NOT NULL IDENTITY,
        [CosigningPhysicianEmployeeID] int NULL,
        [DateServiced] date NOT NULL,
        [HospitalizationID] int NOT NULL,
        [InsuranceID] int NULL,
        [IsScribeServiceOnly] bit NOT NULL,
        [NursePractitionerEmployeeID] int NULL,
        [PhysicianEmployeeID] int NOT NULL,
        [ScribeEmployeeID] int NULL,
        [ServiceTypeID] int NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
        [UpdatedAtUtc] datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_PatientVisits] PRIMARY KEY ([PatientVisitID]),
        CONSTRAINT [FK_PatientVisits_CosignPhysicianEmployee] FOREIGN KEY ([CosigningPhysicianEmployeeID]) REFERENCES [Compensation].[Employees] ([EmployeeID]),
        CONSTRAINT [FK_PatientVisits_Hospitalizations] FOREIGN KEY ([HospitalizationID]) REFERENCES [Clinical].[Hospitalizations] ([Id]),
        CONSTRAINT [FK_PatientVisits_Insurances] FOREIGN KEY ([InsuranceID]) REFERENCES [Clinical].[Insurances] ([InsuranceID]),
        CONSTRAINT [FK_PatientVisits_NursePractitioner] FOREIGN KEY ([NursePractitionerEmployeeID]) REFERENCES [Compensation].[Employees] ([EmployeeID]),
        CONSTRAINT [FK_PatientVisits_Physician] FOREIGN KEY ([PhysicianEmployeeID]) REFERENCES [Compensation].[Employees] ([EmployeeID]),
        CONSTRAINT [FK_PatientVisits_Scribe] FOREIGN KEY ([ScribeEmployeeID]) REFERENCES [Compensation].[Employees] ([EmployeeID]),
        CONSTRAINT [FK_PatientVisits_ServiceType] FOREIGN KEY ([ServiceTypeID]) REFERENCES [Clinical].[ServiceTypes] ([ServiceTypeID])
    );
    
    ALTER TABLE [Clinical].[PatientVisits] ADD  DEFAULT ((0)) FOR [IsScribeServiceOnly]
    
    CREATE INDEX [IX_Hospitalizations_FacilityId] ON [Clinical].[Hospitalizations] ([FacilityId]);
    
    CREATE INDEX [IX_Hospitalizations_PatientId] ON [Clinical].[Hospitalizations] ([PatientId]);
    
    CREATE INDEX [IX_PatientVisits_CosigningPhysicianEmployeeID] ON [Clinical].[PatientVisits] ([CosigningPhysicianEmployeeID]);
    
    CREATE INDEX [IX_PatientVisits_HospitalizationID] ON [Clinical].[PatientVisits] ([HospitalizationID]);
    
    CREATE INDEX [IX_PatientVisits_InsuranceID] ON [Clinical].[PatientVisits] ([InsuranceID]);
    
    CREATE INDEX [IX_PatientVisits_NursePractitionerEmployeeID] ON [Clinical].[PatientVisits] ([NursePractitionerEmployeeID]);
    
    CREATE INDEX [IX_PatientVisits_PhysicianEmployeeID] ON [Clinical].[PatientVisits] ([PhysicianEmployeeID]);
    
    CREATE INDEX [IX_PatientVisits_ScribeEmployeeID] ON [Clinical].[PatientVisits] ([ScribeEmployeeID]);
    
    CREATE INDEX [IX_PatientVisits_ServiceTypeID] ON [Clinical].[PatientVisits] ([ServiceTypeID]);
    
    CREATE INDEX [IX_PatientVisits_Staging_CosigningPhysicianEmployeeID] ON [Clinical].[PatientVisits_Staging] ([CosigningPhysicianEmployeeID]);
    
    CREATE INDEX [IX_PatientVisits_Staging_FacilityID] ON [Clinical].[PatientVisits_Staging] ([FacilityID]);
    
    CREATE INDEX [IX_PatientVisits_Staging_InsuranceID] ON [Clinical].[PatientVisits_Staging] ([InsuranceID]);
    
    CREATE INDEX [IX_PatientVisits_Staging_NursePractitionerEmployeeID] ON [Clinical].[PatientVisits_Staging] ([NursePractitionerEmployeeID]);
    
    CREATE INDEX [IX_PatientVisits_Staging_PatientID] ON [Clinical].[PatientVisits_Staging] ([PatientID]);
    
    CREATE INDEX [IX_PatientVisits_Staging_PhysicianEmployeeID] ON [Clinical].[PatientVisits_Staging] ([PhysicianEmployeeID]);
    
    CREATE INDEX [IX_PatientVisits_Staging_ScribeEmployeeID] ON [Clinical].[PatientVisits_Staging] ([ScribeEmployeeID]);
    
    CREATE INDEX [IX_PatientVisits_Staging_ServiceTypeID] ON [Clinical].[PatientVisits_Staging] ([ServiceTypeID]);
    
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251111023227_Initial', N'9.0.10');
END;
COMMIT;
GO
/*
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [Clinical].[vw_PatientVisits]
AS 
SELECT
	PV.PatientVisitID,
	PV.DateServiced,
	P.PatientFirstName + ' ' + P.PatientLastName AS PatientName,
	P.PatientMRN,
	F.FacilityName,
	I.CodeAndDescription AS Insurance,
	ST.CPTCode AS ServiceType,
	PE.FullName AS Physician,
	NPE.FullName AS NursePractitioner,
	SE.FullName AS Scribe,
	PV.IsScribeServiceOnly
FROM
	Clinical.PatientVisits PV
LEFT JOIN
	Clinical.Patients P ON PV.PatientID = P.PatientID
LEFT JOIN
	Clinical.Facilities F ON PV.FacilityID = F.FacilityID
LEFT JOIN
	Clinical.Insurances I ON PV.InsuranceID = I.InsuranceID
LEFT JOIN
	Clinical.ServiceTypes ST ON PV.ServiceTypeID = ST.ServiceTypeID
LEFT JOIN
	Compensation.Employees PE ON PV.PhysicianEmployeeID = PE.EmployeeID
LEFT JOIN
	Compensation.Employees NPE ON PV.NursePractitionerEmployeeID = NPE.EmployeeID
LEFT JOIN
	Compensation.Employees SE ON PV.ScribeEmployeeID = SE.EmployeeID
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [Clinical].[vw_PatientVisits_CompareToLive]
AS 
SELECT
	PV.DateServiced AS ServiceDTS,
	P.PatientFullName AS PatientNM,
	P.PatientMRN AS MRN,
	P.PatientBirthDate AS PatientBirthDTS,
	ST.CPTCode AS CPTCD,
	I.InsuranceDescription AS InsuranceNM,
	PE.LastName AS AttendingPhysicianJoinID,
	NPE.LastName AS NursePractitionerJoinID,
	SE.LastName AS ScribeNM,
	NULL AS ImportFileNM,
	NULL AS ModifiedDTS,
	PV.AdmissionNumber AS CaseID,
	CPE.LastName AS CosignPhysicianJoinID
FROM
	Clinical.PatientVisits PV
LEFT JOIN
	Clinical.Patients P ON PV.PatientID = P.PatientID
LEFT JOIN
	Clinical.Facilities F ON PV.FacilityID = F.FacilityID
LEFT JOIN
	Clinical.Insurances I ON PV.InsuranceID = I.InsuranceID
LEFT JOIN
	Clinical.ServiceTypes ST ON PV.ServiceTypeID = ST.ServiceTypeID
LEFT JOIN
	Compensation.Employees PE ON PV.PhysicianEmployeeID = PE.EmployeeID
LEFT JOIN
	Compensation.Employees NPE ON PV.NursePractitionerEmployeeID = NPE.EmployeeID
LEFT JOIN
	Compensation.Employees SE ON PV.ScribeEmployeeID = SE.EmployeeID
LEFT JOIN
	Compensation.Employees CPE ON PV.CosigningPhysicianEmployeeID = CPE.EmployeeID
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [Clinical].[vw_PatientVisits_Staging]
AS 
SELECT
	PVS.PatientVisit_StagingID,
	PVS.DateServiced,
	P.PatientFirstName + ' ' + P.PatientLastName AS PatientName,
	F.FacilityName,
	I.CodeAndDescription AS Insurance,
	ST.CPTCode AS ServiceType,
	PE.FullName AS Physician,
	NPE.FullName AS NursePractitioner,
	SE.FullName AS Scribe,
	PVS.AddedToMainTable
FROM
	Clinical.PatientVisits_Staging PVS
LEFT JOIN
	Clinical.Patients P ON PVS.PatientID = P.PatientID
LEFT JOIN
	Clinical.Facilities F ON PVS.FacilityID = F.FacilityID
LEFT JOIN
	Clinical.Insurances I ON PVS.InsuranceID = I.InsuranceID
LEFT JOIN
	Clinical.ServiceTypes ST ON PVS.ServiceTypeID = ST.ServiceTypeID
LEFT JOIN
	Compensation.Employees PE ON PVS.PhysicianEmployeeID = PE.EmployeeID
LEFT JOIN
	Compensation.Employees NPE ON PVS.NursePractitionerEmployeeID = NPE.EmployeeID
LEFT JOIN
	Compensation.Employees SE ON PVS.ScribeEmployeeID = SE.EmployeeID
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Billing].[JasperAdmissionReportStaging](
	[CaseID] [varchar](255) NOT NULL,
	[ProgramNM] [varchar](max) NULL,
	[PatientNM] [varchar](max) NULL,
	[MRN] [varchar](max) NOT NULL,
	[PatientSexDSC] [varchar](max) NULL,
	[PatientAgeNBR] [int] NULL,
	[PatientBirthDTS] [date] NULL,
	[CallDTS] [varchar](255) NULL,
	[IntakeDSC] [varchar](255) NULL,
	[AdmitDTS] [date] NOT NULL,
	[AdmitTimeVAL] [varchar](255) NULL,
	[DischargeDTS] [date] NULL,
	[DischargeTimeVAL] [varchar](255) NULL,
	[SocialSecurityNBR] [varchar](255) NULL,
	[PatientAddressDSC] [varchar](max) NULL,
	[PatientCityNM] [varchar](max) NULL,
	[PatientZipCD] [varchar](max) NULL,
	[CountyCD] [varchar](max) NULL,
	[PatientPhoneNBR] [varchar](max) NULL,
	[UnitNBR] [varchar](max) NULL,
	[RoomNBR] [varchar](max) NULL,
	[LengthOfStayNBR] [int] NULL,
	[AttendingPhysicianNM] [varchar](max) NULL,
	[NursePractitionerNM] [varchar](max) NULL,
	[CoAttendingNM] [varchar](max) NULL,
	[PhyInc] [varchar](max) NULL,
	[PhyIncOP] [varchar](max) NULL,
	[HPInc] [varchar](max) NULL,
	[HPIncOp] [varchar](max) NULL,
	[FC] [varchar](max) NULL,
	[FCDSC] [varchar](max) NULL,
	[PrimaryInsuranceNM] [varchar](max) NULL,
	[PrimaryInsuranceSubscriberID] [varchar](max) NULL,
	[PrimaryInsuranceGroup] [varchar](max) NULL,
	[PrimaryInsuranceGroupNM] [varchar](max) NULL,
	[SecondaryInsuranceNM] [varchar](max) NULL,
	[RefCategory] [varchar](max) NULL,
	[RefSource] [varchar](max) NULL,
	[CaseManagerNM] [varchar](max) NULL,
	[AdmitDiagnosisCD] [varchar](max) NULL,
	[AdmitTypeDSC] [varchar](max) NULL,
	[Internist] [varchar](max) NULL,
	[PatientRaceDSC] [varchar](max) NULL,
	[PatientEthnicityDSC] [varchar](max) NULL,
	[PatientLanguageDSC] [varchar](max) NULL,
	[PatientGenderIdentityDSC] [varchar](max) NULL,
	[PatientSexualOrientationDSC] [varchar](max) NULL,
	[PatientResidencyStatusDSC] [varchar](max) NULL,
	[PatientVeteranStatusDSC] [varchar](max) NULL,
	[NotesTXT] [varchar](max) NULL,
	[InitialAuthorizationDayCNT] [int] NULL,
	[ConcurrentAuthorizationDayCNT] [int] NULL,
	[TotalAuthorizationDayCNT] [int] NULL,
	[AuthorizationNBR] [varchar](max) NULL,
	[ADayLOS] [int] NULL,
	[ProblemNoteTXT] [varchar](max) NULL,
	[PrimaryInsuranceGroupID] [varchar](max) NULL,
	[ImportFileNM] [varchar](255) NOT NULL,
	[ModifiedDTS] [date] NULL,
	[HospitalID] [varchar](max) NULL,
 CONSTRAINT [PK_JasperAdmissionReportStaging] PRIMARY KEY CLUSTERED 
(
	[CaseID] ASC,
	[ImportFileNM] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Reference].[County](
	[CountyCD] [char](3) NOT NULL,
	[CountyNM] [varchar](50) NOT NULL,
 CONSTRAINT [PK_CountyLookup] PRIMARY KEY CLUSTERED 
(
	[CountyCD] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_CountyLookup_CountyNM] UNIQUE NONCLUSTERED 
(
	[CountyNM] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [Billing].[vw_JasperReport]
AS 
WITH Jasper AS(
SELECT
	j.CaseID
	,j.MRN
	,j.PatientNM
	,j.AdmitDTS
	,j.DischargeDTS
	,j.PatientAddressDSC
	,j.PatientCityNM
	,j.PatientZipCD
	,c.CountyNM AS PatientCountyNM
	,j.PatientPhoneNBR
	,j.PatientBirthDTS
	,j.PatientSexDSC
	,j.AdmitDiagnosisCD
	,j.PrimaryInsuranceNM
	,j.PrimaryInsuranceSubscriberID
	,j.PrimaryInsuranceGroupID
	,j.SecondaryInsuranceNM
	,CASE WHEN j.HospitalID = 'ASR' THEN 1 WHEN j.HospitalID = 'AVM' THEN 2 ELSE NULL END AS FacilityID
	,j.ImportFileNM
	,j.ModifiedDTS
	,CASE WHEN j.PhyInc = 'Yes' THEN 1 ELSE 0 END AS PhysicianIncludedFLG
	,ROW_NUMBER() OVER(PARTITION BY j.CaseID ORDER BY j.ModifiedDTS DESC) AS JasperSEQ
FROM
	Billing.JasperAdmissionReportStaging AS j

	LEFT JOIN Reference.County AS c
		ON j.CountyCD = c.CountyCD
)
SELECT
	CaseID
	,MRN
	,PatientNM
	,AdmitDTS
	,DischargeDTS
	,PatientAddressDSC
	,PatientCityNM
	,PatientZipCD
	,PatientCountyNM
	,PatientPhoneNBR
	,PatientBirthDTS
	,PatientSexDSC
	,AdmitDiagnosisCD
	,PrimaryInsuranceNM
	,PrimaryInsuranceSubscriberID
	,PrimaryInsuranceGroupID
	,SecondaryInsuranceNM
	,FacilityID
	,PhysicianIncludedFLG
	,ImportFileNM
	,ModifiedDTS
FROM
	Jasper
WHERE
	JasperSEQ = 1
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Billing].[Batch](
	[BatchID] [varchar](255) NOT NULL,
	[ServiceDTS] [date] NOT NULL,
	[AdmitDTS] [date] NOT NULL,
	[DischargeDTS] [date] NULL,
	[PatientNM] [varchar](255) NOT NULL,
	[MRN] [varchar](255) NOT NULL,
	[PatientPhoneNBR] [varchar](255) NULL,
	[PatientStreetAddressTXT] [varchar](255) NULL,
	[PatientCityNM] [varchar](255) NULL,
	[PatientZipCD] [varchar](255) NULL,
	[PatientCountyNM] [varchar](255) NULL,
	[SexDSC] [varchar](555) NULL,
	[PatientBirthDTS] [date] NULL,
	[PatientAgeAtVisitNBR] [int] NULL,
	[NonShortDoyleFLG] [bit] NOT NULL,
	[CPTCD] [varchar](255) NOT NULL,
	[AdmitDiagnosisCD] [varchar](255) NULL,
	[DischargeDiagnosisCD] [varchar](255) NULL,
	[RenderingProviderNM] [varchar](255) NOT NULL,
	[LocumTenensFLG] [bit] NULL,
	[RenderingLocationNM] [varchar](200) NOT NULL,
	[PrimaryInsuranceNM] [varchar](200) NOT NULL,
	[PrimaryInsuranceSubscriberID] [varchar](100) NOT NULL,
	[SecondaryInsuranceNM] [varchar](200) NULL,
	[BatchDTS] [datetime] NOT NULL,
	[CaseID] [varchar](255) NULL,
	[InsuranceCarrierGroupNM] [varchar](255) NULL,
	[CredentialedDTS] [date] NULL,
	[ProviderCarrierID] [varchar](255) NULL,
	[CountyFLG] [bit] NULL,
	[DischargeDiagnosisDSC] [varchar](255) NULL
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [Billing].[vw_CurrentBatch]
AS 
WITH MostRecentBatch AS(
SELECT
    MAX(BatchDTS) AS MostRecentBatchDTS
FROM
    Billing.Batch
)
SELECT
    BatchID
    ,CaseID
    ,ServiceDTS
    ,AdmitDTS
    ,DischargeDTS
    ,PatientNM
    ,MRN
    ,PatientPhoneNBR
    ,PatientStreetAddressTXT
    ,PatientCityNM
    ,PatientZipCD
    ,PatientCountyNM
    ,SexDSC
    ,PatientBirthDTS
    ,PatientAgeAtVisitNBR
    ,NonShortDoyleFLG
    ,CPTCD
    ,AdmitDiagnosisCD
    ,DischargeDiagnosisCD
    ,RenderingProviderNM
    ,LocumTenensFLG
    ,RenderingLocationNM
    ,PrimaryInsuranceNM
    ,PrimaryInsuranceSubscriberID
    ,SecondaryInsuranceNM
    ,BatchDTS
FROM
    Billing.Batch AS b

    CROSS JOIN MostRecentBatch AS mrb
WHERE
    b.BatchDTS = mrb.MostRecentBatchDTS
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Billing].[JasperReportStaging](
	[CaseID] [varchar](255) NOT NULL,
	[ProgramNM] [varchar](max) NULL,
	[PatientNM] [varchar](max) NULL,
	[AdmitDTS] [date] NULL,
	[DischargeDTS] [date] NULL,
	[TimeOfDayNBR] [time](7) NULL,
	[OPLastVisit] [varchar](max) NULL,
	[MRN] [varchar](max) NULL,
	[PatientBirthDTS] [date] NULL,
	[PatientSexDSC] [varchar](max) NULL,
	[PatientGenderIdentityDSC] [varchar](max) NULL,
	[PatientSexualOrientationDSC] [varchar](max) NULL,
	[PatientAgeNBR] [int] NULL,
	[PatientRaceDSC] [varchar](max) NULL,
	[PatientEthnicityDSC] [varchar](max) NULL,
	[PatientLanguageDSC] [varchar](max) NULL,
	[PatientResidencyStatusDSC] [varchar](max) NULL,
	[PatientVeteranStatusDSC] [varchar](max) NULL,
	[AdmitTypeDSC] [varchar](max) NULL,
	[DischargeTypeDSC] [varchar](max) NULL,
	[OSHPDAdmitSourceDSC] [varchar](max) NULL,
	[AdmitSourceDSC] [varchar](max) NULL,
	[UnitNBR] [varchar](max) NULL,
	[AttendingPhysicianNM] [varchar](max) NULL,
	[CoAttendingNM] [varchar](max) NULL,
	[NursePractitionerNM] [varchar](max) NULL,
	[Medical] [varchar](max) NULL,
	[PhyInc] [varchar](max) NULL,
	[PhyIncOP] [varchar](max) NULL,
	[HPInc] [varchar](max) NULL,
	[HPIncOp] [varchar](max) NULL,
	[LengthOfStayNBR] [int] NULL,
	[FC] [int] NULL,
	[PrimaryInsuranceNM] [varchar](max) NULL,
	[PrimaryInsuranceSubscriberID] [varchar](max) NULL,
	[PrimaryInsuranceGroup] [varchar](max) NULL,
	[PrimaryInsuranceGroupNM] [varchar](max) NULL,
	[OPCov] [varchar](max) NULL,
	[PHPCov] [varchar](max) NULL,
	[PrimaryInsuranceGroupID] [varchar](max) NULL,
	[Cstat] [varchar](max) NULL,
	[PrimaryInsuranceAuthorizationNBR] [varchar](max) NULL,
	[PrimaryInsuranceAuthorizedDaysNBR] [int] NULL,
	[ADayLOS] [int] NULL,
	[AncillaryAuthorizedDaysNBR] [int] NULL,
	[AncillaryADayLOS] [int] NULL,
	[DayTypeDSC] [varchar](max) NULL,
	[SecondaryInsuranceNM] [varchar](max) NULL,
	[SecondaryInsuranceAuthorizationNBR] [varchar](max) NULL,
	[SecondaryInsuranceAuthorizedDaysNBR] [int] NULL,
	[SecondADayLOS] [int] NULL,
	[LOC] [varchar](max) NULL,
	[AdmitDiagnosisCD] [varchar](max) NULL,
	[DischargeDiagnosisDSC] [varchar](max) NULL,
	[CaseManagerNM] [varchar](max) NULL,
	[PFSRNM] [varchar](max) NULL,
	[PatientAddressDSC] [varchar](max) NULL,
	[PatientCityNM] [varchar](max) NULL,
	[PatientZipCD] [varchar](max) NULL,
	[PatientCountyNM] [varchar](max) NULL,
	[PatientPhoneNBR] [varchar](max) NULL,
	[RefCategory] [varchar](max) NULL,
	[RefSource] [varchar](max) NULL,
	[Inactive] [varchar](max) NULL,
	[LTDaysBal] [int] NULL,
	[FullDaysBal] [int] NULL,
	[CoDaysBal] [int] NULL,
	[ResDaysBal] [int] NULL,
	[HospitalID] [varchar](max) NULL,
	[ServiceTypeDSC] [varchar](max) NULL,
	[ReferToIOPPHP] [varchar](max) NULL,
	[AISCD] [varchar](max) NULL,
	[ImportFileNM] [varchar](255) NOT NULL,
	[ModifiedDTS] [datetime2](7) NULL,
 CONSTRAINT [PK_JasperReportStaging_CaseID_ImportFileNM] PRIMARY KEY CLUSTERED 
(
	[CaseID] ASC,
	[ImportFileNM] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [Billing].[vw_JasperDischargeReport]
AS 
WITH Jasper AS(
SELECT
	j.CaseID
	,j.MRN
	,j.PatientNM
	,j.AdmitDTS
	,j.DischargeDTS
	,j.PatientAddressDSC
	,j.PatientCityNM
	,j.PatientZipCD
	,j.PatientCountyNM
	,j.PatientPhoneNBR
	,j.PatientBirthDTS
	,j.PatientSexDSC
	,j.AdmitDiagnosisCD
	,j.DischargeDiagnosisDSC
	,j.PrimaryInsuranceNM
	,j.PrimaryInsuranceSubscriberID
	,j.PrimaryInsuranceGroupID
	,j.SecondaryInsuranceNM
	,CASE WHEN j.HospitalID = 'ASR' THEN 1 WHEN j.HospitalID = 'AVM' THEN 2 ELSE NULL END AS FacilityID
	,CASE WHEN j.PhyInc = 'Yes' THEN 1 ELSE 0 END AS PhysicianIncludedFLG
	,j.ImportFileNM
	,j.ModifiedDTS
	,ROW_NUMBER() OVER(PARTITION BY j.CaseID, j.HospitalID ORDER BY j.ModifiedDTS DESC) AS JasperSEQ
FROM
	Billing.JasperReportStaging AS j
)
SELECT
	CaseID
	,MRN
	,PatientNM
	,AdmitDTS
	,DischargeDTS
	,PatientAddressDSC
	,PatientCityNM
	,PatientZipCD
	,PatientCountyNM
	,PatientPhoneNBR
	,PatientBirthDTS
	,PatientSexDSC
	,AdmitDiagnosisCD
	,DischargeDiagnosisDSC
	,PrimaryInsuranceNM
	,PrimaryInsuranceSubscriberID
	,PrimaryInsuranceGroupID
	,SecondaryInsuranceNM
	,FacilityID
	,PhysicianIncludedFLG
	,ImportFileNM
	,ModifiedDTS
FROM
	Jasper
WHERE
	JasperSEQ = 1
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Billing].[AgingVisits](
	[AgingID] [varchar](255) NOT NULL,
	[ServiceDTS] [date] NOT NULL,
	[PatientNM] [varchar](255) NOT NULL,
	[MRN] [varchar](255) NOT NULL,
	[CPTCD] [varchar](255) NOT NULL,
	[AttendingPhysicianNM] [varchar](255) NOT NULL,
	[MidlevelNM] [varchar](255) NULL,
	[RenderingProviderNM] [varchar](255) NULL,
	[RenderingLocationNM] [varchar](255) NOT NULL,
	[PendingReasonDSC] [varchar](255) NOT NULL,
	[CaseID] [varchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[AgingID] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Billing].[CPTCode](
	[CPTCD] [varchar](255) NOT NULL,
	[CPTDSC] [varchar](100) NULL,
	[RevenueAMT] [decimal](10, 2) NULL,
	[FacilityID] [int] NOT NULL,
	[StartDTS] [date] NOT NULL,
	[EndDTS] [date] NULL,
 CONSTRAINT [PK_CPTCode_CPTCD_FacilityID_StartDTS] PRIMARY KEY CLUSTERED 
(
	[CPTCD] ASC,
	[FacilityID] ASC,
	[StartDTS] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Billing].[Credentialing](
	[EmployeeID] [int] NOT NULL,
	[CarrierNM] [nvarchar](100) NOT NULL,
	[CredentialedDTS] [date] NULL,
	[ProviderCarrierID] [nvarchar](50) NULL,
 CONSTRAINT [PK_Credentialing] PRIMARY KEY CLUSTERED 
(
	[EmployeeID] ASC,
	[CarrierNM] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Billing].[ExpensePeriod](
	[PayrollPeriodStartDTS] [date] NOT NULL,
	[PayrollPeriodEndDTS] [date] NOT NULL,
	[PayrollPeriodDSC] [varchar](50) NOT NULL,
	[PayrollPayDTS] [date] NOT NULL,
	[PayrollProcessingStartDTS] [date] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[PayrollPeriodDSC] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Billing].[InsuranceLookup](
	[PrimaryInsuranceNM] [varchar](255) NOT NULL,
	[InsuranceCarrierGroupNM] [varchar](100) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[PrimaryInsuranceNM] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Billing].[InvoiceClinical](
	[InvoiceID] [varchar](255) NOT NULL,
	[ServiceDTS] [date] NOT NULL,
	[WeekendFLG] [int] NOT NULL,
	[InvoicePeriodDSC] [varchar](255) NOT NULL,
	[InvoicePeriodEndDTS] [date] NOT NULL,
	[AttendingPhysicianNM] [varchar](255) NOT NULL,
	[MidlevelNM] [varchar](255) NULL,
	[PatientNM] [varchar](255) NOT NULL,
	[MRN] [varchar](255) NOT NULL,
	[PrimaryInsuranceNM] [varchar](255) NOT NULL,
	[CPTCD] [varchar](255) NOT NULL,
	[CPTDSC] [varchar](255) NOT NULL,
	[RenderingLocationNM] [varchar](555) NOT NULL,
	[InvoiceAMT] [decimal](18, 2) NOT NULL,
	[InvoiceEligibleDTS] [date] NOT NULL,
	[CaseID] [varchar](255) NULL
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Billing].[InvoicePeriod](
	[InvoicePeriodStartDTS] [date] NULL,
	[InvoicePeriodEndDTS] [date] NULL,
	[InvoicePeriodDSC] [varchar](50) NOT NULL,
	[IsInvoicedFLG] [varchar](3) NULL,
	[InvoiceProcessingStartDTS] [date] NULL,
	[InvoiceProcessingEndDTS] [date] NULL,
PRIMARY KEY CLUSTERED 
(
	[InvoicePeriodDSC] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Billing].[InvoiceWeekendRevenue](
	[WeekendStartDTS] [date] NOT NULL,
	[ProviderNM] [varchar](255) NOT NULL,
	[InvoiceAMT] [decimal](18, 2) NULL,
	[InvoiceEligibilityDTS] [date] NOT NULL,
	[DaysWorkedOnWeekendCNT] [int] NULL,
 CONSTRAINT [PK_InvoiceWeekendRevenue] PRIMARY KEY CLUSTERED 
(
	[WeekendStartDTS] ASC,
	[ProviderNM] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Billing].[JasperReport](
	[AdNum] [int] NOT NULL,
	[Program] [nvarchar](255) NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[DOA] [date] NOT NULL,
	[DOD] [date] NOT NULL,
	[TOD] [time](7) NOT NULL,
	[OP_Last_Visit] [nvarchar](255) NULL,
	[MedRec] [int] NOT NULL,
	[DOB] [date] NOT NULL,
	[Sex] [nvarchar](255) NOT NULL,
	[Gender_Identity] [nvarchar](255) NULL,
	[Sexual_Orientation] [nvarchar](255) NULL,
	[Age] [tinyint] NOT NULL,
	[Race] [nvarchar](255) NOT NULL,
	[Ethnicity] [nvarchar](255) NOT NULL,
	[Primary_Language] [nvarchar](255) NOT NULL,
	[Residency_Status] [nvarchar](255) NULL,
	[Veteran_Status] [nvarchar](255) NULL,
	[Adm_Type] [nvarchar](255) NULL,
	[Dis_Type] [nvarchar](255) NOT NULL,
	[OshpdSoaRoute] [nvarchar](255) NOT NULL,
	[Adm_Source] [nvarchar](255) NOT NULL,
	[Unit] [nvarchar](255) NOT NULL,
	[Attending] [nvarchar](255) NOT NULL,
	[Co_Attending] [nvarchar](255) NULL,
	[Ad_CoAttn_Clinician] [nvarchar](255) NULL,
	[Medical] [nvarchar](255) NULL,
	[PhyInc] [bit] NOT NULL,
	[PhyIncOP] [bit] NOT NULL,
	[HPInc] [bit] NOT NULL,
	[HPIncOp] [bit] NOT NULL,
	[LOS] [int] NOT NULL,
	[FC] [tinyint] NOT NULL,
	[Prim_Insurance] [nvarchar](255) NOT NULL,
	[SubscriberID] [nvarchar](255) NOT NULL,
	[Group_ID] [nvarchar](255) NULL,
	[Group_Name] [nvarchar](255) NULL,
	[OPCov] [nvarchar](255) NULL,
	[PHPCov] [nvarchar](255) NULL,
	[C_Group] [nvarchar](255) NOT NULL,
	[C_Stat] [nvarchar](255) NOT NULL,
	[Auth] [nvarchar](255) NULL,
	[Auth_Days] [int] NOT NULL,
	[ADay_LOS] [int] NOT NULL,
	[Ancillary_Auth_Days] [tinyint] NOT NULL,
	[Ancillary_ADay_LOS] [int] NOT NULL,
	[Day_Type] [nvarchar](255) NULL,
	[_2nd_Insurance] [nvarchar](255) NULL,
	[_2nd_Auth] [bigint] NULL,
	[_2nd_Auth_Days] [int] NULL,
	[_2nd_ADay_LOS] [int] NULL,
	[LOC] [varchar](2000) NULL,
	[Ad_Dx] [nvarchar](255) NOT NULL,
	[Dishc_Dx] [nvarchar](255) NULL,
	[Case_Manager] [nvarchar](255) NOT NULL,
	[PFSR] [nvarchar](255) NULL,
	[Patient_Street] [nvarchar](255) NOT NULL,
	[Patient_City] [nvarchar](255) NOT NULL,
	[Patient_Zip] [nvarchar](255) NULL,
	[County] [nvarchar](255) NULL,
	[Pat_Phone] [nvarchar](255) NULL,
	[Ref_Category] [nvarchar](255) NOT NULL,
	[Ref_Source] [nvarchar](255) NOT NULL,
	[Inactive] [nvarchar](255) NULL,
	[LT_Days_Bal] [tinyint] NULL,
	[Full_Days_Bal] [tinyint] NULL,
	[Co_Days_Bal] [tinyint] NULL,
	[Res_Days_Bal] [tinyint] NULL,
	[HospID] [nvarchar](255) NOT NULL,
	[ServiceType] [nvarchar](255) NOT NULL,
	[Refer_To_IOP_PHP] [nvarchar](255) NULL,
	[AIS_Code] [nvarchar](255) NOT NULL,
	[File] [nvarchar](255) NOT NULL,
 CONSTRAINT [PK_JasperReport] PRIMARY KEY CLUSTERED 
(
	[AdNum] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Billing].[OnCallRevenue](
	[ServiceStartDTS] [datetime] NOT NULL,
	[ProviderNM] [nvarchar](200) NULL,
	[HourCNT] [int] NULL,
	[RateAMT] [decimal](18, 2) NULL,
	[InvoiceAMT] [decimal](18, 2) NULL,
	[InvoiceEligibilityDTS] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[ServiceStartDTS] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Billing].[SantaRosaInsuranceContract](
	[InsuranceCD] [varchar](255) NULL,
	[InsuranceNM] [varchar](255) NOT NULL,
	[StartDTS] [date] NULL,
	[PhysicianIncludedFLG] [bit] NULL,
	[ContractedFLG] [bit] NULL,
	[NotesTXT] [varchar](255) NULL,
	[SendToRBCTXT] [varchar](255) NULL,
	[SendToInsuranceFLG] [bit] NULL,
	[NonShortDoyleOnlyFLG] [bit] NULL,
	[InvoiceHospitalFLG] [bit] NULL,
	[EffectiveStartDTS] [date] NOT NULL,
	[EffectiveEndDTS] [date] NULL,
 CONSTRAINT [PK_SantaRosaInsuranceContract] PRIMARY KEY CLUSTERED 
(
	[InsuranceNM] ASC,
	[EffectiveStartDTS] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Billing].[Visits](
	[VisitForPaymentID] [varchar](255) NOT NULL,
	[ServiceDTS] [date] NOT NULL,
	[PresentOnJasperReportFLG] [bit] NOT NULL,
	[AdmissionReportAdmitDTS] [date] NULL,
	[DischargeReportDischargeDTS] [date] NULL,
	[WeekendFLG] [bit] NOT NULL,
	[PatientNM] [varchar](255) NOT NULL,
	[MRN] [varchar](255) NOT NULL,
	[PatientStreetAddressTXT] [varchar](255) NULL,
	[PatientCityNM] [varchar](100) NULL,
	[PatientZipCD] [varchar](255) NULL,
	[PatientCountyNM] [varchar](255) NULL,
	[PatientPhoneNBR] [varchar](255) NULL,
	[PatientBirthDTS] [date] NULL,
	[PatientAgeAtVisitNBR] [int] NULL,
	[SexDSC] [varchar](255) NULL,
	[CPTCD] [varchar](255) NOT NULL,
	[DischargeDiagnosisCD] [varchar](255) NULL,
	[AdmitDiagnosisCD] [varchar](255) NULL,
	[AttendingPhysicianNM] [varchar](255) NOT NULL,
	[MidlevelNM] [varchar](255) NULL,
	[InvoicePeriodDSC] [varchar](255) NOT NULL,
	[InvoicePeriodEndDTS] [date] NOT NULL,
	[AdmissionReportPrimaryInsuranceNM] [varchar](255) NULL,
	[AdmissionReportPrimaryInsuranceSubscriberID] [varchar](255) NULL,
	[AdmissionReportSecondaryInsuranceNM] [varchar](255) NULL,
	[AdmissionReportInclusiveFLG] [bit] NULL,
	[AdmissionReportSendToInsuranceFLG] [bit] NULL,
	[AdmissionReportNonShortDoyleOnlyFLG] [bit] NULL,
	[AdmissionReportInvoiceHospitalFLG] [bit] NULL,
	[RenderingLocationNM] [varchar](255) NOT NULL,
	[RenderingProviderNM] [varchar](255) NULL,
	[LocumTenensFLG] [bit] NULL,
	[CaseID] [varchar](255) NULL,
	[AdmissionReportInsuranceCarrierGroupNM] [varchar](255) NULL,
	[CredentialedDTS] [date] NULL,
	[ProviderCarrierID] [varchar](255) NULL,
	[FacilityID] [int] NULL,
	[DischargeDiagnosisDSC] [varchar](255) NULL,
	[PresentOnDischargeJasperReportFLG] [bit] NULL,
	[AdmissionReportCountyFLG] [bit] NULL,
	[DischargeReportPrimaryInsuranceNM] [varchar](250) NULL,
	[DischargeReportPrimaryInsuranceSubscriberID] [varchar](255) NULL,
	[DischargeReportInclusiveFLG] [bit] NULL,
	[DischargeReportSecondaryInsuranceNM] [varchar](255) NULL,
	[DischargeReportSendToInsuranceFLG] [bit] NULL,
	[DischargeReportNonShortDoyleOnlyFLG] [bit] NULL,
	[DischargeReportInvoiceHospitalFLG] [bit] NULL,
	[DischargeReportInsuranceCarrierGroupNM] [varchar](255) NULL,
	[DischargeReportCountyFLG] [bit] NULL,
	[AdmissionReportPhysicianIncludedFLG] [bit] NULL,
	[DischargeReportPhysicianIncludedFLG] [bit] NULL,
	[DischargeReportAdmitDTS] [date] NULL,
	[AdmissionReportDischargeDTS] [date] NULL
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Billing].[VisitsReadyForPayment](
	[VisitForPaymentID] [varchar](255) NOT NULL,
	[ServiceDTS] [date] NOT NULL,
	[CompleteDataForBillingFLG] [int] NOT NULL,
	[AdmitDTS] [date] NULL,
	[DischargeDTS] [date] NULL,
	[WeekendFLG] [int] NULL,
	[PatientNM] [varchar](255) NOT NULL,
	[MRN] [varchar](255) NOT NULL,
	[PatientBirthDTS] [date] NULL,
	[SexDSC] [varchar](255) NULL,
	[CPTCD] [varchar](255) NOT NULL,
	[DischargeDiagnosisCD] [varchar](255) NULL,
	[AdmitDiagnosisCD] [varchar](255) NULL,
	[AttendingPhysicianNM] [varchar](255) NOT NULL,
	[MidlevelNM] [varchar](255) NULL,
	[InvoicePeriodDSC] [varchar](255) NOT NULL,
	[PrimaryInsuranceNM] [varchar](255) NULL,
	[PrimaryInsuranceSubscriberID] [varchar](255) NULL,
	[SecondaryInsuranceNM] [varchar](255) NULL,
	[InclusiveFLG] [int] NULL,
	[RenderingLocationNM] [varchar](255) NOT NULL,
	[RenderingProviderNM] [varchar](255) NULL,
	[CreatedDTS] [date] NULL
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Clinical].[InternalMedicineOvernightOnCall](
	[OvernightOnCallStartDTS] [date] NOT NULL,
	[ProviderJoinID] [varchar](255) NULL,
	[ProviderID] [int] NOT NULL,
	[FacilityID] [int] NOT NULL,
 CONSTRAINT [InternalMedicineOvernightOnCall_PK] PRIMARY KEY CLUSTERED 
(
	[OvernightOnCallStartDTS] ASC,
	[ProviderID] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Clinical].[PatientVisit](
	[ServiceDTS] [date] NOT NULL,
	[PatientNM] [varchar](255) NOT NULL,
	[MRN] [varchar](255) NOT NULL,
	[PatientBirthDTS] [date] NULL,
	[CPTCD] [varchar](255) NOT NULL,
	[InsuranceNM] [varchar](255) NULL,
	[AttendingPhysicianJoinID] [varchar](255) NULL,
	[NursePractitionerJoinID] [varchar](255) NULL,
	[ScribeNM] [varchar](255) NULL,
	[ImportFileNM] [varchar](255) NULL,
	[ModifiedDTS] [datetime] NULL,
	[CaseID] [varchar](255) NULL,
	[CosignPhysicianJoinID] [varchar](255) NULL,
 CONSTRAINT [PK_PatientVisit] PRIMARY KEY CLUSTERED 
(
	[ServiceDTS] ASC,
	[MRN] ASC,
	[CPTCD] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Clinical].[PatientVisit_Staging](
	[ServiceDTS] [date] NOT NULL,
	[PatientNM] [varchar](255) NOT NULL,
	[MRN] [varchar](255) NOT NULL,
	[PatientBirthDTS] [date] NULL,
	[CPTCD] [varchar](255) NOT NULL,
	[InsuranceNM] [varchar](255) NULL,
	[AttendingPhysicianJoinID] [varchar](255) NULL,
	[NursePractitionerJoinID] [varchar](255) NULL,
	[ScribeNM] [varchar](255) NULL,
	[ImportFileNM] [varchar](255) NULL,
	[ModifiedDTS] [datetime] NULL,
	[IsApproved] [bit] NULL,
	[ApprovedBy] [nvarchar](100) NULL,
	[ApprovedDate] [datetime] NULL,
 CONSTRAINT [PK_PatientVisit_Staging_ServiceDTS_MRN_PatientNM] PRIMARY KEY CLUSTERED 
(
	[ServiceDTS] ASC,
	[MRN] ASC,
	[CPTCD] ASC,
	[PatientNM] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Compensation].[Contract](
	[ContractID] [varchar](255) NOT NULL,
	[ContractNM] [varchar](255) NOT NULL,
	[PayTypeNM] [varchar](255) NOT NULL,
	[WeekendFLG] [int] NULL,
	[SupervisingFLG] [int] NULL,
	[CPTCD] [varchar](255) NULL,
	[ExpenseAMT] [decimal](19, 2) NOT NULL,
	[DailySupervisionExclusiveFLG] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[ContractID] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Compensation].[PayEntity](
	[ProviderJoinID] [varchar](255) NOT NULL,
	[EntityNM] [varchar](255) NULL,
	[EffectiveStartDTS] [date] NOT NULL,
	[EffectiveEndDTS] [date] NULL,
	[CreatedDTS] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[ProviderJoinID] ASC,
	[EffectiveStartDTS] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Compensation].[Payroll](
	[PayrollID] [varchar](255) NOT NULL,
	[PayrollPayDTS] [date] NULL,
	[PayrollPeriodDSC] [varchar](255) NULL,
	[ServiceDTS] [date] NOT NULL,
	[ProviderNM] [varchar](255) NOT NULL,
	[AccompanyingProviderNM] [varchar](255) NULL,
	[ServiceDSC] [varchar](255) NULL,
	[RateAMT] [decimal](18, 2) NULL,
	[ExpenseAMT] [decimal](18, 2) NULL,
	[EarningsTypeDSC] [varchar](255) NOT NULL,
	[CountCNT] [int] NULL,
	[PayToNM] [varchar](255) NULL,
	[CreatedDTS] [datetime] NULL,
	[FacilityNM] [varchar](255) NOT NULL,
 CONSTRAINT [pk_Payroll] PRIMARY KEY CLUSTERED 
(
	[PayrollID] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Compensation].[ProviderContract](
	[ProviderJoinID] [varchar](255) NOT NULL,
	[FacilityID] [int] NOT NULL,
	[ContractNM] [varchar](255) NOT NULL,
	[StartDTS] [date] NOT NULL,
	[EndDTS] [date] NULL,
 CONSTRAINT [PK__ProviderContract] PRIMARY KEY CLUSTERED 
(
	[ProviderJoinID] ASC,
	[FacilityID] ASC,
	[StartDTS] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Compensation].[ProviderRate](
	[ProviderNM] [varchar](255) NULL,
	[ProviderJoinID] [varchar](255) NOT NULL,
	[CPTCD] [varchar](255) NOT NULL,
	[ExpenseAMT] [decimal](19, 2) NULL,
	[SupervisingFLG] [int] NOT NULL,
	[RateStartDTS] [date] NOT NULL,
	[RateEndDTS] [date] NULL,
PRIMARY KEY CLUSTERED 
(
	[ProviderJoinID] ASC,
	[CPTCD] ASC,
	[SupervisingFLG] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Compensation].[WeekendCoverageRate](
	[ProviderCredentialsDSC] [varchar](50) NOT NULL,
	[WeekendRateAMT] [decimal](19, 2) NULL,
	[RateStartDTS] [date] NOT NULL,
	[RateEndDTS] [date] NULL,
	[ScribeFLG] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ProviderCredentialsDSC] ASC,
	[RateStartDTS] ASC,
	[ScribeFLG] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Reference].[Provider](
	[ProviderNM] [varchar](255) NULL,
	[ProviderJoinID] [varchar](255) NOT NULL,
	[ProviderCredentialsDSC] [varchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[ProviderJoinID] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Compensation].[FacilityPay](
	[FacilityPayID] [int] IDENTITY(1,1) NOT NULL,
	[FacilityID] [int] NOT NULL,
	[ServiceTypeID] [int] NOT NULL,
	[RevenueAmount] [decimal](18, 2) NOT NULL,
	[StartDate] [date] NOT NULL,
	[EndDate] [date] NOT NULL,
 CONSTRAINT [PK_FacilityPays] PRIMARY KEY CLUSTERED 
(
	[FacilityPayID] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Compensation].[PayrollData](
	[PayrollDataID] [int] IDENTITY(1,1) NOT NULL,
	[PayrollPeriodID] [int] NOT NULL,
	[ProviderPayID] [int] NOT NULL,
	[Count] [int] NOT NULL,
	[Date] [date] NOT NULL,
 CONSTRAINT [PK_PayrollDatum] PRIMARY KEY CLUSTERED 
(
	[PayrollDataID] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Compensation].[PayrollPeriods](
	[PayrollPeriodID] [int] IDENTITY(1,1) NOT NULL,
	[StartDate] [date] NOT NULL,
	[EndDate] [date] NOT NULL,
	[PayrollDate] [date] NOT NULL,
	[PayrollProcessingStartDate] [date] NULL,
 CONSTRAINT [PK_PayrollPeriods] PRIMARY KEY CLUSTERED 
(
	[PayrollPeriodID] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Compensation].[ProviderContracts](
	[ProviderContractID] [int] IDENTITY(1,1) NOT NULL,
	[EmployeeID] [int] NOT NULL,
	[ContractID] [int] NOT NULL,
	[StartDate] [date] NOT NULL,
	[EndDate] [date] NOT NULL,
 CONSTRAINT [PK_ProviderContracts] PRIMARY KEY CLUSTERED 
(
	[ProviderContractID] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Compensation].[ProviderPay](
	[ProviderPayID] [int] IDENTITY(1,1) NOT NULL,
	[EmployeeID] [int] NOT NULL,
	[FacilityID] [int] NOT NULL,
	[ServiceTypeID] [int] NULL,
	[Description] [nvarchar](100) NOT NULL,
	[PayAmount] [decimal](10, 2) NOT NULL,
	[PayUnit] [nvarchar](100) NOT NULL,
 CONSTRAINT [PK_ProviderPay] PRIMARY KEY CLUSTERED 
(
	[ProviderPayID] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Clinical].[PatientVisit_Staging] ADD  DEFAULT ((0)) FOR [IsApproved]
GO
--ALTER TABLE [Compensation].[Contracts] ADD  DEFAULT ((0)) FOR [WeekendFlag]
--GO
--ALTER TABLE [Compensation].[Contracts] ADD  DEFAULT ((0)) FOR [SupervisingFlag]
--GO
ALTER TABLE [Compensation].[PayrollData] ADD  DEFAULT ((0)) FOR [Count]
GO
--ALTER TABLE [Compensation].[Contracts]  WITH CHECK ADD  CONSTRAINT [FK_Contracts_Employee] FOREIGN KEY([EmployeeID])
--REFERENCES [Compensation].[Employees] ([EmployeeID])
--GO
--ALTER TABLE [Compensation].[Contracts] CHECK CONSTRAINT [FK_Contracts_Employee]
--GO
--ALTER TABLE [Compensation].[Contracts]  WITH CHECK ADD  CONSTRAINT [FK_Contracts_Facility] FOREIGN KEY([FacilityID])
--REFERENCES [Clinical].[Facilities] ([FacilityID])
--GO
--ALTER TABLE [Compensation].[Contracts] CHECK CONSTRAINT [FK_Contracts_Facility]
--GO
--ALTER TABLE [Compensation].[Contracts]  WITH CHECK ADD  CONSTRAINT [FK_Contracts_ServiceType] FOREIGN KEY([ServiceTypeID])
--REFERENCES [Clinical].[ServiceTypes] ([ServiceTypeID])
--GO
--ALTER TABLE [Compensation].[Contracts] CHECK CONSTRAINT [FK_Contracts_ServiceType]
--GO
ALTER TABLE [Compensation].[FacilityPay]  WITH CHECK ADD  CONSTRAINT [FK_FacilityPay_Facility] FOREIGN KEY([FacilityID])
REFERENCES [Clinical].[Facilities] ([FacilityID])
GO
ALTER TABLE [Compensation].[FacilityPay] CHECK CONSTRAINT [FK_FacilityPay_Facility]
GO
ALTER TABLE [Compensation].[FacilityPay]  WITH CHECK ADD  CONSTRAINT [FK_FacilityPay_ServiceType] FOREIGN KEY([ServiceTypeID])
REFERENCES [Clinical].[ServiceTypes] ([ServiceTypeID])
GO
ALTER TABLE [Compensation].[FacilityPay] CHECK CONSTRAINT [FK_FacilityPay_ServiceType]
GO
ALTER TABLE [Compensation].[PayrollData]  WITH CHECK ADD  CONSTRAINT [FK_PayrollData_PayrollPeriod] FOREIGN KEY([PayrollPeriodID])
REFERENCES [Compensation].[PayrollPeriods] ([PayrollPeriodID])
GO
ALTER TABLE [Compensation].[PayrollData] CHECK CONSTRAINT [FK_PayrollData_PayrollPeriod]
GO
ALTER TABLE [Compensation].[PayrollData]  WITH CHECK ADD  CONSTRAINT [FK_PayrollData_ProviderPay] FOREIGN KEY([ProviderPayID])
REFERENCES [Compensation].[ProviderPay] ([ProviderPayID])
GO
ALTER TABLE [Compensation].[PayrollData] CHECK CONSTRAINT [FK_PayrollData_ProviderPay]
GO
--ALTER TABLE [Compensation].[ProviderContracts]  WITH CHECK ADD  CONSTRAINT [FK_ProviderContracts_Contract] FOREIGN KEY([ContractID])
--REFERENCES [Compensation].[Contracts] ([ContractID])
--GO
ALTER TABLE [Compensation].[ProviderContracts] CHECK CONSTRAINT [FK_ProviderContracts_Contract]
GO
ALTER TABLE [Compensation].[ProviderContracts]  WITH CHECK ADD  CONSTRAINT [FK_ProviderContracts_Employee] FOREIGN KEY([EmployeeID])
REFERENCES [Compensation].[Employees] ([EmployeeID])
GO
ALTER TABLE [Compensation].[ProviderContracts] CHECK CONSTRAINT [FK_ProviderContracts_Employee]
GO
ALTER TABLE [Compensation].[ProviderPay]  WITH CHECK ADD  CONSTRAINT [FK_ProviderPay_Employee] FOREIGN KEY([EmployeeID])
REFERENCES [Compensation].[Employees] ([EmployeeID])
GO
ALTER TABLE [Compensation].[ProviderPay] CHECK CONSTRAINT [FK_ProviderPay_Employee]
GO
ALTER TABLE [Compensation].[ProviderPay]  WITH CHECK ADD  CONSTRAINT [FK_ProviderPay_Facility] FOREIGN KEY([FacilityID])
REFERENCES [Clinical].[Facilities] ([FacilityID])
GO
ALTER TABLE [Compensation].[ProviderPay] CHECK CONSTRAINT [FK_ProviderPay_Facility]
GO
ALTER TABLE [Compensation].[ProviderPay]  WITH CHECK ADD  CONSTRAINT [FK_ProviderPay_ServiceType] FOREIGN KEY([ServiceTypeID])
REFERENCES [Clinical].[ServiceTypes] ([ServiceTypeID])
GO
ALTER TABLE [Compensation].[ProviderPay] CHECK CONSTRAINT [FK_ProviderPay_ServiceType]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetBillingBatch]
AS
BEGIN
WITH VisitsReadyForPayment AS(
SELECT
	CONCAT(p.ServiceDTS,' - ',p.MRN,' - ', p.CPTCD) AS VisitForPaymentID 
	,p.ServiceDTS
	,CASE WHEN j.MedRec IS NULL THEN 0 ELSE 1 END AS CompleteDataForBillingFLG
	,j.doa AS AdmitDTS
	,j.dod AS DischargeDTS
	,CASE WHEN DATEPART(WEEKDAY, p.ServiceDTS) IN (1,7) THEN 1 ELSE 0 END AS WeekendFLG 
	,p.PatientNM
	,p.MRN
	,j.Patient_Street AS PatientStreetAddressTXT
	,j.Patient_City AS PatientCityNM
	,j.Patient_Zip AS PatientZipCD
	,j.Pat_Phone AS PatientPhoneNBR
	,j.DOB AS PatientBirthDTS
	,DATEDIFF(YEAR, j.DOB, p.ServiceDTS) 
    - CASE 
        WHEN DATEADD(YEAR, DATEDIFF(YEAR, j.DOB, p.ServiceDTS), j.DOB) > p.ServiceDTS
        THEN 1 
        ELSE 0 
      END AS PatientAgeAtVisitNBR
	,j.Sex AS SexDSC
	,p.CPTCD
	,j.Dishc_DX AS DischargeDiagnosisCD
	,j.Ad_Dx AS AdmitDiagnosisCD
	,CONCAT(md.ProviderNM,' ',md.ProviderCredentialsDSC) AS AttendingPhysicianNM
	,CASE WHEN p.NursePractitionerJoinID IS NULL THEN NULL
		ELSE CONCAT(np.ProviderNM,' ',np.ProviderCredentialsDSC)
		END AS MidlevelNM
	,i.InvoicePeriodDSC
	,j.Prim_Insurance AS PrimaryInsuranceNM
	,j.SubscriberID AS PrimaryInsuranceSubscriberID
	,j._2nd_Insurance AS SecondaryInsuranceNM
	,COALESCE(l.PhysicianIncludedFLG,0) AS InclusiveFLG
	,CASE WHEN j.C_Group = 'MCAL' THEN 1 ELSE COALESCE(l.SendToInsuranceFLG,1) END AS SendToInsuranceFLG
	,COALESCE(l.NonShortDoyleOnlyFLG,0) AS NonShortDoyleOnlyFLG
	,COALESCE(l.InvoiceHospitalFLG,0) AS InvoiceHospitalFLG
	,'Santa Rosa Behavioral Healthcare Hospital' AS RenderingLocationNM -- Will need to add logic in here and possible from data sources to differentiate now that we have a new one
	,CASE 
		WHEN p.ServiceDTS = CONVERT(DATE,'2025-07-29') AND AttendingPhysicianJoinID = 'Johnson' 
			THEN 'Tyler Torrico MD'
		WHEN p.AttendingPhysicianJoinID = 'Giurgius'
			THEN NULL
		WHEN
			p.AttendingPhysicianJoinID = 'Kletz'
			THEN NULL
		WHEN AttendingPhysicianJoinID = 'Johnson'
			THEN NULL
		ELSE CONCAT(md.ProviderNM,' ',md.ProviderCredentialsDSC)
	END AS RenderingProviderNM
	,NULL AS LocumTenensFLG
 FROM
	Consilient.Clinical.PatientVisit AS p

	INNER JOIN Consilient.Billing.InvoicePeriod AS i
		ON p.ServiceDTS BETWEEN i.InvoicePeriodStartDTS AND InvoicePeriodEndDTS

	LEFT JOIN Consilient.Billing.JasperReport AS j
		ON p.mrn = j.MedRec
			AND p.ServiceDTS BETWEEN j.doa AND COALESCE(j.dod,GETDATE())

	LEFT JOIN Consilient.Billing.SantaRosaInsuranceContract AS l
		ON j.C_Group = l.InsuranceCD

	LEFT JOIN Consilient.Reference.Provider AS md
		ON p.AttendingPhysicianJoinID = md.ProviderJoinID

	LEFT JOIN Consilient.Reference.Provider AS np
		ON p.NursePractitionerJoinID = np.ProviderJoinID
)
,preBatch AS(
SELECT
	b.VisitForPaymentID AS BatchID
	,b.ServiceDTS
	,b.AdmitDTS
	,b.DischargeDTS
	,b.PatientNM
	,b.MRN
	,b.PatientPhoneNBR
	,b.PatientStreetAddressTXT
	,b.PatientCityNM
	,b.PatientZipCD
	,b.SexDSC
	,b.PatientBirthDTS
	,b.PatientAgeAtVisitNBR
	,CASE WHEN b.PatientAgeAtVisitNBR < 22 OR b.PatientAgeAtVisitNBR > 64 THEN 1 ELSE 0 END AS NonShortDoyleFLG
	,b.CPTCD
	,b.AdmitDiagnosisCD
	,b.DischargeDiagnosisCD
	,b.RenderingProviderNM
	,b.LocumTenensFLG
	,b.RenderingLocationNM
	,b.PrimaryInsuranceNM
	,b.PrimaryInsuranceSubscriberID
	,b.SecondaryInsuranceNM
	,b.InclusiveFLG
	,b.SendToInsuranceFLG
	,b.NonShortDoyleOnlyFLG
	,b.InvoiceHospitalFLG
	,GETDATE() AS BatchDTS
FROM	
	VisitsReadyForPayment AS b
WHERE
	b.CompleteDataForBillingFLG = 1
	AND b.RenderingProviderNM IS NOT NULL
	AND b.SendToInsuranceFLG = 1
)
,Batch AS(
SELECT
	BatchID
	,ServiceDTS
	,AdmitDTS
	,DischargeDTS
	,PatientNM
	,MRN
	,PatientPhoneNBR
	,PatientStreetAddressTXT
	,PatientCityNM
	,PatientZipCD
	,SexDSC
	,PatientBirthDTS
	,PatientAgeAtVisitNBR
	,NonShortDoyleFLG
	,CPTCD
	,AdmitDiagnosisCD
	,DischargeDiagnosisCD
	,RenderingProviderNM
	,LocumTenensFLG
	,RenderingLocationNM
	,PrimaryInsuranceNM
	,PrimaryInsuranceSubscriberID
	,SecondaryInsuranceNM
	,InclusiveFLG
	,SendToInsuranceFLG
	,NonShortDoyleOnlyFLG
	,BatchDTS
	,InvoiceHospitalFLG
FROM
	preBatch
WHERE
	NOT (NonShortDoyleOnlyFLG = 1 AND NonShortDoyleFLG = 0) --Removes Short Doyle patients when county only pays for non short doyle
)
--Turn this into a sp that does the following
Uploads results of this query to a Batch table (all batches/PatientVisits submitted to Regena already does this
Also creates list of patients to invoice to the hospital and inserts into a a table showing that
Also creates a list of patients where we don't have complete info from Jasper Report and starts counting age of visits so we can follow up when they hit a certain threshhold
 --most of these should update as Jasper report gets updated
 --If it's all in one SP, we won't have to run a sequence of SPs every day

--This table doesn't currently have a pk due to patient that duplicates on June 12 Need to resolve this so we can know keep track of which results have
already been inserted into the table as it will grow incrementally
INSERT INTO Consilient.Billing.Batch(
	BatchID
	,ServiceDTS
	,AdmitDTS
	,DischargeDTS
	,PatientNM
	,MRN
	,PatientPhoneNBR
	,PatientStreetAddressTXT
	,PatientCityNM
	,PatientZipCD
	,SexDSC
	,PatientBirthDTS
	,PatientAgeAtVisitNBR
	,NonShortDoyleFLG
	,CPTCD
	,AdmitDiagnosisCD
	,DischargeDiagnosisCD
	,RenderingProviderNM
	,LocumTenensFLG
	,RenderingLocationNM
	,PrimaryInsuranceNM
	,PrimaryInsuranceSubscriberID
	,SecondaryInsuranceNM
	,InclusiveFLG
	,SendToInsuranceFLG
	,NonShortDoyleOnlyFLG
	,BatchDTS
	,InvoiceHospitalFLG
	)
SELECT
	BatchID
	,ServiceDTS
	,AdmitDTS
	,DischargeDTS
	,PatientNM
	,MRN
	,PatientPhoneNBR
	,PatientStreetAddressTXT
	,PatientCityNM
	,PatientZipCD
	,SexDSC
	,PatientBirthDTS
	,PatientAgeAtVisitNBR
	,NonShortDoyleFLG
	,CPTCD
	,AdmitDiagnosisCD
	,DischargeDiagnosisCD
	,RenderingProviderNM
	,LocumTenensFLG
	,RenderingLocationNM
	,PrimaryInsuranceNM
	,PrimaryInsuranceSubscriberID
	,SecondaryInsuranceNM
	,InclusiveFLG
	,SendToInsuranceFLG
	,NonShortDoyleOnlyFLG
	,BatchDTS
	,InvoiceHospitalFLG
FROM
	Batch AS b
WHERE
	NOT EXISTS (
    SELECT 
		1 
    FROM
		Consilient.Billing.Batch AS t
    WHERE 
        t.BatchID = b.BatchID
		)
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetClinicalInvoice]
AS
BEGIN
WITH VisitsReadyForPayment AS(
SELECT
	CONCAT(p.ServiceDTS,' - ',p.MRN,' - ', p.CPTCD) AS VisitForPaymentID 
	,p.ServiceDTS
	,CASE WHEN j.MedRec IS NULL THEN 0 ELSE 1 END AS CompleteDataForBillingFLG
	,j.doa AS AdmitDTS
	,j.dod AS DischargeDTS
	,CASE WHEN DATEPART(WEEKDAY, p.ServiceDTS) IN (1,7) THEN 1 ELSE 0 END AS WeekendFLG 
	,p.PatientNM
	,p.MRN
	,j.Patient_Street AS PatientStreetAddressTXT
	,j.Patient_City AS PatientCityNM
	,j.Patient_Zip AS PatientZipCD
	,j.Pat_Phone AS PatientPhoneNBR
	,j.DOB AS PatientBirthDTS
	,DATEDIFF(YEAR, j.DOB, p.ServiceDTS) 
    - CASE 
        WHEN DATEADD(YEAR, DATEDIFF(YEAR, j.DOB, p.ServiceDTS), j.DOB) > p.ServiceDTS
        THEN 1 
        ELSE 0 
      END AS PatientAgeAtVisitNBR
	,j.Sex AS SexDSC
	,p.CPTCD
	,j.Dishc_DX AS DischargeDiagnosisCD
	,j.Ad_Dx AS AdmitDiagnosisCD
	,CONCAT(md.ProviderNM,' ',md.ProviderCredentialsDSC) AS AttendingPhysicianNM
	,CASE WHEN p.NursePractitionerJoinID IS NULL THEN NULL
		ELSE CONCAT(np.ProviderNM,' ',np.ProviderCredentialsDSC)
		END AS MidlevelNM
	,i.InvoicePeriodDSC
	,j.Prim_Insurance AS PrimaryInsuranceNM
	,j.SubscriberID AS PrimaryInsuranceSubscriberID
	,j._2nd_Insurance AS SecondaryInsuranceNM
	,COALESCE(l.PhysicianIncludedFLG,0) AS InclusiveFLG
	,CASE WHEN j.C_Group = 'MCAL' THEN 1 ELSE COALESCE(l.SendToInsuranceFLG,1) END AS SendToInsuranceFLG
	,COALESCE(l.NonShortDoyleOnlyFLG,0) AS NonShortDoyleOnlyFLG
	,COALESCE(l.InvoiceHospitalFLG,0) AS InvoiceHospitalFLG
	,'Santa Rosa Behavioral Healthcare Hospital' AS RenderingLocationNM -- Will need to add logic in here and possible from data sources to differentiate now that we have a new one
	,CASE 
		WHEN p.ServiceDTS = CONVERT(DATE,'2025-07-29') AND AttendingPhysicianJoinID = 'Johnson' 
			THEN 'Tyler Torrico MD'
		WHEN p.AttendingPhysicianJoinID = 'Giurgius'
			THEN NULL
		WHEN
			p.AttendingPhysicianJoinID = 'Kletz'
			THEN NULL
		WHEN AttendingPhysicianJoinID = 'Johnson'
			THEN NULL
		ELSE CONCAT(md.ProviderNM,' ',md.ProviderCredentialsDSC)
	END AS RenderingProviderNM
	,NULL AS LocumTenensFLG
 FROM
	Consilient.Clinical.PatientVisit AS p

	INNER JOIN Consilient.Billing.InvoicePeriod AS i
		ON p.ServiceDTS BETWEEN i.InvoicePeriodStartDTS AND InvoicePeriodEndDTS

	LEFT JOIN Consilient.Billing.JasperReport AS j
		ON p.mrn = j.MedRec
			AND p.ServiceDTS BETWEEN j.doa AND COALESCE(j.dod,GETDATE())

	LEFT JOIN Consilient.Billing.SantaRosaInsuranceContract AS l
		ON j.C_Group = l.InsuranceCD

	LEFT JOIN Consilient.Reference.Provider AS md
		ON p.AttendingPhysicianJoinID = md.ProviderJoinID

	LEFT JOIN Consilient.Reference.Provider AS np
		ON p.NursePractitionerJoinID = np.ProviderJoinID
)
,Invoice AS(
SELECT --Need to clean out columns not needed downstream
Need to build out in Power BI Report Builder
	b.VisitForPaymentID AS InvoiceID
	,b.AttendingPhysicianNM
	,b.MidlevelNM
	,b.ServiceDTS
	,b.InvoicePeriodDSC
	,b.PatientNM
	,b.MRN
	,b.CPTCD
	,b.RenderingLocationNM
	,COALESCE(b.InclusiveFLG,0) AS InclusiveFLG
	,CASE 
		WHEN RenderingLocationNM = 'Santa Rosa Behavioral Healthcare Hospital' AND ServiceDTS > CONVERT(DATE,'2025-07-13') AND InclusiveFLG = 1 THEN 1
		--Everything at Ventura is Contractually Invoiced for now
		WHEN RenderingLocationNM = 'Ventura' THEN 1
		ELSE 0
	END AS InvoiceFLG
	,GETDATE() AS BatchDTS
FROM	
	VisitsReadyForPayment AS b
)
,ClinicalInvoice AS(
SELECT
	i.InvoiceID
	,i.ServiceDTS
	,InvoicePeriodDSC
	,AttendingPhysicianNM
	,MidlevelNM
	,i.PatientNM
	,i.MRN
	,i.CPTCD
	,c.CPTDSC
	,i.RenderingLocationNM
	,c.RevenueAMT AS InvoiceAMT
	,CONVERT(DATE,GETDATE()) AS InvoiceEligibleDTS
FROM
	Invoice AS i

	LEFT JOIN Billing.CPTCode AS c
		ON i.CPTCD = c.CPTCD
WHERE
	InvoiceFLG = 1
)
INSERT INTO Consilient.Billing.InvoiceClinical(
	InvoiceID
	,ServiceDTS
	,WeekendFLG
	,InvoicePeriodDSC
	,AttendingPhysicianNM
	,MidlevelNM
	,PatientNM
	,MRN
	,CPTCD
	,CPTDSC
	,RenderingLocationNM
	,InvoiceAMT
	,InvoiceEligibleDTS
	)
SELECT
	InvoiceID
	,ServiceDTS
	,CASE WHEN DATEPART(WEEKDAY, ServiceDTS) IN (1,7) THEN 1 ELSE 0 END AS WeekendFLG
	,InvoicePeriodDSC --Becomes insignificant once data lag is introduced
	,AttendingPhysicianNM
	,MidlevelNM
	,PatientNM
	,MRN
	,CPTCD
	,CPTDSC
	,RenderingLocationNM
	,InvoiceAMT
	,InvoiceEligibleDTS --Use this as parameter for Invoice Generation in PBI Report Builder so patients
	whose insurance data lagged from previous invoice periods will get invoiced as soon as data is present
FROM
	ClinicalInvoice AS v
WHERE
	NOT EXISTS (
    SELECT 
		1 
    FROM
		Consilient.Billing.InvoiceClinical AS t
    WHERE 
        t.InvoiceID = v.InvoiceID

		)
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetClinicianEarnings]
@PayrollDTS DATE
AS
BEGIN
--
Payroll SP V2
Key difference is that that this query is table driven rather than code driven.

As long as the contract and providercontract tables are set up correctly. This logic will build payroll for all providers unless new factors
that affect pay are added in.

8/8/2025 Reconciled to 7/5 payroll. Only difference is 1 DC Summary for Giurgius, suspect it's a row that got deleted in data cleanup

Potential Issue: Right now Supervision and Non-supervision days are exclusive of each other. This will break when an MD gets pay both for
Supervision and nonsupervision in the same day. Need to code for that when it comes up.

Would like to break these CTEs up and put this into an Orchestration tool (Perhaps Azure Data Factory) To break out each individual CTE into its
own query that persists a table. Also need to build out queries that clearly define the grain, metric tables that grab the info we need,
and a summary table that left joins the population to all metric tables for the final table that drives the end solution

This runs against the prod structure, may need to modify when pointed toward Matt's dev structure

WITH PatientVisitsInPayroll AS(
SELECT
	md.FullName AS ProviderNM
	,md.LastName AS ProviderJoinID --Need to add EmployeeID to contract tables to replace this join
	,np.FullName AS AccompanyingProviderNM
	,f.FacilityName AS FacilityNM 
	,f.FacilityID
	,pv.IsSupervising AS SupervisingFLG
	,CASE WHEN DATEPART(WEEKDAY, pv.DateServiced) IN (1,7) THEN 1 ELSE 0 END AS WeekendFLG
	,1 AS MDFLG
	,pv.DateServiced AS ServiceDTS
	,pat.PatientFullName AS PatientNM
	,pat.PatientMRN AS MRN
	,st.CPTCode AS CPTCD
	,ep.PayrollPeriodDSC
	,ep.PayrollPayDTS
FROM
	Consilient.Clinical.PatientVisits AS pv

	INNER JOIN Consilient.Billing.ExpensePeriod AS ep
		ON pv.DateServiced BETWEEN ep.PayrollPeriodStartDTS AND ep.PayrollPeriodEndDTS

	INNER JOIN Consilient.Compensation.Employees AS md
		ON pv.PhysicianEmployeeID = md.EmployeeID

	LEFT JOIN Consilient.Compensation.Employees AS np
		ON pv.NursePractitionerEmployeeID = np.EmployeeID

	LEFT JOIN Consilient.Clinical.Facilities AS f
		ON pv.FacilityID = f.FacilityID

	INNER JOIN Consilient.Clinical.Patients AS pat
		ON pv.PatientID = pat.PatientID

	INNER JOIN Consilient.Clinical.ServiceTypes AS st
		ON pv.ServiceTypeID = st.ServiceTypeID
WHERE
	ep.PayrollPayDTS = @PayrollDTS
	AND pv.IsScribeServiceOnly = 0

UNION ALL

SELECT
	np.FullName AS ProviderNM
	,np.LastName AS ProviderJoinID
	,md.FullName AS AccompanyingProviderNM
	,f.FacilityName AS FacilityNM
	,f.FacilityID
	,0 AS SupervisingFLG
	,CASE WHEN DATEPART(WEEKDAY, pv.DateServiced) IN (1,7) THEN 1 ELSE 0 END AS WeekendFLG
	,0 AS MDFLG
	,pv.DateServiced AS ServiceDTS
	,pat.PatientFullName AS PatientNM
	,pat.PatientMRN AS MRN
	,st.CPTCode AS CPTCD
	,ep.PayrollPeriodDSC
	,ep.PayrollPayDTS
FROM
	Consilient.Clinical.PatientVisits AS pv

	INNER JOIN Consilient.Billing.ExpensePeriod AS ep
		ON pv.DateServiced BETWEEN ep.PayrollPeriodStartDTS AND ep.PayrollPeriodEndDTS

	INNER JOIN Consilient.Compensation.Employees AS md
		ON pv.PhysicianEmployeeID = md.EmployeeID

	LEFT JOIN Consilient.Compensation.Employees AS np
		ON pv.NursePractitionerEmployeeID = np.EmployeeID

	LEFT JOIN Consilient.Clinical.Facilities AS f
		ON pv.FacilityID = f.FacilityID

	INNER JOIN Consilient.Clinical.Patients AS pat
		ON pv.PatientID = pat.PatientID

	INNER JOIN Consilient.Clinical.ServiceTypes AS st
		ON pv.ServiceTypeID = st.ServiceTypeID
WHERE
	ep.PayrollPayDTS = @PayrollDTS
	AND pv.NursePractitionerEmployeeID IS NOT NULL
	AND pv.IsScribeServiceOnly = 0
)
,PayrollWithContract AS(
SELECT
	p.ProviderNM
	,p.AccompanyingProviderNM
	,p.FacilityNM
	,p.SupervisingFLG
	,p.WeekendFLG
	,p.MDFLG
	,p.ServiceDTS
	,p.PatientNM
	,p.MRN
	,p.CPTCD
	,p.PayrollPeriodDSC
	,p.PayrollPayDTS
	,pc.ContractNM
	,pc.ProviderJoinID
FROM
	PatientVisitsInPayroll AS p

	INNER JOIN Compensation.ProviderContract AS pc
		ON p.ProviderJoinID = pc.ProviderJoinID
			AND p.FacilityID = pc.FacilityID
			AND p.ServiceDTS BETWEEN pc.StartDTS AND COALESCE(pc.EndDTS,GETDATE())
)
,PerPatientEarnings AS(
SELECT
	p.ProviderNM
	,p.AccompanyingProviderNM
	,p.FacilityNM
	,p.SupervisingFLG
	,p.WeekendFLG
	,p.MDFLG
	,p.ServiceDTS
	,p.PatientNM
	,p.MRN
	,p.CPTCD
	,p.PayrollPeriodDSC
	,p.PayrollPayDTS
	,p.ContractNM
	,p.ProviderJoinID
	,c.PayTypeNM
	,c.ExpenseAMT
	,'Per Patient' AS EarningsTypeDSC
FROM
	PayrollWithContract AS p

	INNER JOIN Consilient.Compensation.Contract AS c
		ON p.ContractNM = c.ContractNM
			AND c.PayTypeNM = 'Per Patient'
			AND p.CPTCD = c.CPTCD
			AND p.SupervisingFLG = c.SupervisingFLG
			AND p.WeekendFLG = c.WeekendFLG		
)
,DailyWork AS(
SELECT
	ServiceDTS
	,FacilityNM
	,WeekendFLG
	,ProviderNM
	,ProviderJoinID
	,SupervisingFLG
	,AccompanyingProviderNM
	,PayrollPeriodDSC
	,PayrollPayDTS
	,ContractNM
	,COUNT(MRN) AS VisitCNT
	,'Daily' AS EarningsTypeDSC
	,ROW_NUMBER() OVER(PARTITION BY p.ServiceDTS,p.ProviderNM ORDER BY p.SupervisingFLG DESC) AS DailySupervisionSEQ
FROM
	PayrollWithContract AS p
WHERE
	MDFLG = 1
GROUP BY
	ServiceDTS
	,FacilityNM
	,WeekendFLG
	,ProviderNM
	,ProviderJoinID
	,SupervisingFLG
	,AccompanyingProviderNM
	,PayrollPeriodDSC
	,PayrollPayDTS
	,ContractNM
)
,DailyPay AS(
SELECT
	dw.ServiceDTS
	,dw.FacilityNM
	,dw.WeekendFLG
	,dw.ProviderNM
	,dw.ProviderJoinID
	,dw.SupervisingFLG
	,dw.AccompanyingProviderNM
	,dw.PayrollPeriodDSC
	,dw.PayrollPayDTS
	,dw.ContractNM
	,dw.EarningsTypeDSC
	,c.PayTypeNM
	,c.ExpenseAMT
	,dw.DailySupervisionSEQ
FROM
	DailyWork AS dw

	INNER JOIN Consilient.Compensation.Contract AS c
		ON dw.ContractNM = c.ContractNM
			AND c.PayTypeNM <> 'Per Patient'
			AND dw.WeekendFLG = c.WeekendFLG
			AND dw.SupervisingFLG = c.SupervisingFLG
WHERE
	dw.DailySupervisionSEQ = 1 
	OR dw.WeekendFLG = 1
)
,Pay AS(
SELECT
	ppe.ProviderNM
	,ppe.ProviderJoinID
	,ppe.FacilityNM
	,ppe.ServiceDTS
	,ppe.AccompanyingProviderNM
	,c.CPTDSC
	,ppe.PayrollPeriodDSC
	,ppe.PayrollPayDTS
	,ppe.PayTypeNM
	,ppe.EarningsTypeDSC
	,ppe.ExpenseAMT AS RateAMT
	,COUNT(ppe.MRN) AS CountCNT
	,SUM(ppe.ExpenseAMT) AS ExpenseAMT
FROM
	PerPatientEarnings AS ppe

		LEFT JOIN Consilient.Billing.CPTCode AS c
		ON ppe.CPTCD = c.CPTCD and c.FacilityID = 1
GROUP BY
	ppe.ProviderNM
	,ppe.ProviderJoinID
	,ppe.FacilityNM
	,ppe.ServiceDTS
	,ppe.AccompanyingProviderNM
	,c.CPTDSC
	,ppe.PayrollPeriodDSC
	,ppe.PayrollPayDTS
	,ppe.PayTypeNM
	,ppe.EarningsTypeDSC
	,ppe.ExpenseAMT

UNION

SELECT
	ProviderNM
	,ProviderJoinID
	,FacilityNM
	,ServiceDTS
	,AccompanyingProviderNM
	,NULL AS CPTDSC
	,PayrollPeriodDSC
	,PayrollPayDTS
	,PayTypeNM
	,EarningsTypeDSC
	,ExpenseAMT AS RateAMT
	,1 AS CountCNT
	,ExpenseAMT
FROM
	DailyPay

UNION

SELECT
	md.FullName ProviderNM
	,md.LastName -- need to add id to contract and fix the join
	,f.FacilityName AS FacilityNM
	,oc.OvernightOnCallStartDTS AS ServiceDTS
	,NULL AS AccompanyingProviderNM
	,NULL AS CPTDSC
	,ep.PayrollPeriodDSC
	,ep.PayrollPayDTS
	,c.PayTypeNM
	,'Daily' AS EarningsTypeDSC
	,c.ExpenseAMT RateAMT
	,1 AS CountCNT
	,c.ExpenseAMT	
FROM
	Consilient.Clinical.InternalMedicineOvernightOnCall AS oc

	LEFT JOIN Consilient.Compensation.Employees AS md
		ON ProviderID = md.EmployeeID

	LEFT JOIN Consilient.Reference.Provider AS p
		ON oc.ProviderJoinID = p.ProviderJoinID 

	INNER JOIN Consilient.Compensation.ProviderContract AS pc
		ON oc.ProviderJoinID = pc.ProviderJoinID
			AND oc.OvernightOnCallStartDTS BETWEEN pc.StartDTS AND COALESCE(pc.EndDTS,GETDATE())

	INNER JOIN Consilient.Compensation.Contract AS c
		ON pc.ContractNM = c.ContractNM
	
	INNER JOIN Consilient.Billing.ExpensePeriod AS ep
		ON oc.OvernightOnCallStartDTS BETWEEN ep.PayrollPeriodStartDTS AND ep.PayrollPeriodEndDTS

	INNER JOIN Consilient.Clinical.Facilities AS f
		ON oc.FacilityID = f.FacilityID
WHERE
	PayrollPayDTS = @PayrollDTS
)
,ClinicianPay AS(
SELECT
	CONCAT(p.ServiceDTS,'.',p.ProviderNM,'.',p.EarningsTypeDSC,'.',COALESCE(p.CPTDSC,p.PayTypeNM),p.AccompanyingProviderNM) AS PayrollID
	,p.PayrollPayDTS
	,p.PayrollPeriodDSC
	,p.ServiceDTS
	,p.ProviderNM
	,p.AccompanyingProviderNM
	,COALESCE(p.CPTDSC,p.PayTypeNM) AS ServiceDSC --Change this column in existing payroll table and PowerBi Report Builder
	,RateAMT
	,p.ExpenseAMT
	,p.EarningsTypeDSC 
	,p.CountCNT --Need to add this to existing payroll and PowerBI Report Builder
	,p.FacilityNM --Need to add this to existing payroll and PowerBI Report Builder
	,COALESCE(pe.EntityNM,p.ProviderNM) AS PayToNM
	,GETDATE() AS CreatedDTS
FROM
	Pay AS p

	LEFT JOIN Consilient.Compensation.PayEntity AS pe
		ON p.ProviderJoinID = pe.ProviderJoinID
			AND p.ServiceDTS BETWEEN pe.EffectiveStartDTS AND COALESCE(pe.EffectiveEndDTS,GETDATE())
WHERE
	p.ExpenseAMT <> 0

)
INSERT Consilient.Compensation.Payroll(
	PayrollID
	,PayrollPayDTS
	,PayrollPeriodDSC
	,ServiceDTS
	,ProviderNM
	,AccompanyingProviderNM
	,ServiceDSC --CPTDSC Need to modify this name in the table
	,RateAMT
	,ExpenseAMT
	,EarningsTypeDSC
	,CountCNT --ProcedureCNT Need to Modify This in the table
	,FacilityNM --Need to add this field to the table
	,PayToNM 
	,CreatedDTS
	)
SELECT
	PayrollID
	,PayrollPayDTS
	,PayrollPeriodDSC
	,ServiceDTS
	,ProviderNM
	,AccompanyingProviderNM
	,ServiceDSC --Change this column in existing payroll table and PowerBi Report Builder
	,RateAMT
	,ExpenseAMT
	,EarningsTypeDSC 
	,CountCNT --Need to add this to existing payroll and PowerBI Report Builder
	,FacilityNM --Need to add this to existing payroll and PowerBI Report Builder
	,PayToNM
	,CreatedDTS
FROM
	ClinicianPay AS c
WHERE
	NOT EXISTS (
    SELECT 
		1 
    FROM
		Consilient.Compensation.Payroll AS p
    WHERE 
        p.PayrollID = c.PayrollID 
		)
ORDER BY
	c.PayrollID
;
SELECT * FROM Consilient.Compensation.Payroll WHERE PayrollPayDTS = @PayrollDTS
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetCMOEarningsStatements]
AS
BEGIN
WITH WeekdayPay AS(
SELECT DISTINCT
	e.PayrollPayDTS
	,e.PayrollPeriodDSC
	,p.ServiceDTS
	,rpmd.ProviderNM +' ' + rpmd.ProviderCredentialsDSC AS ProviderNM
	,rpnp.ProviderNM +' '+ rpnp.ProviderCredentialsDSC AS AccompanyingProviderNM
	,NULL AS CPTDSC
	,NULL AS RateAMT
	,CASE WHEN rpnp.ProviderNM IS NULL THEN 1500 ELSE 400 END AS ExpenseAMT
	,'Daily' AS EarningsTypeDSC
	,NULL AS ProcedureCNT
	,CASE WHEN pe.EntityNM IS NULL THEN rpmd.ProviderNM +' ' + rpmd.ProviderCredentialsDSC ELSE pe.EntityNM END AS PayToNM
FROM
    Clinical.PatientVisit AS p
	
	LEFT JOIN Billing.ExpensePeriod AS e
		ON p.ServiceDTS <= e.PayrollPeriodEndDTS
			AND p.ServiceDTS >= e.PayrollPeriodStartDTS
			
	LEFT JOIN Reference.Provider AS rpmd
		ON p.AttendingPhysicianJoinID = rpmd.ProviderJoinID
    
	LEFT JOIN Reference.Provider AS rpnp 
            ON p.NursePractitionerJoinID = rpnp.ProviderJoinID

	LEFT JOIN Compensation.PayEntity AS pe
		ON p.AttendingPhysicianJoinID = pe.ProviderJoinID
WHERE 
	p.AttendingPhysicianJoinID IN ('Giurgius','Johnson')
	AND CONVERT(DATE,'2025-08-01') BETWEEN e.PayrollProcessingStartDTS AND e.PayrollPayDTS
	AND DATEPART(WEEKDAY, p.ServiceDTS) NOT IN (1,7)
)
,Dedup AS(
SELECT
	PayrollPayDTS
	,PayrollPeriodDSC
	,ServiceDTS
	,ProviderNM
	,AccompanyingProviderNM
	,CPTDSC
	,RateAMT
	,ExpenseAMT
	,EarningsTypeDSC
	,ProcedureCNT
	,PayToNM
	,ROW_NUMBER() OVER(PARTITION BY ProviderNM,ServiceDTS ORDER BY AccompanyingProviderNM DESC) AS DailyPaySEQ
FROM	
	WeekdayPay
)
,WeekendPay AS(
SELECT DISTINCT
	e.PayrollPayDTS
	,e.PayrollPeriodDSC
	,p.ServiceDTS
	,rpmd.ProviderNM +' ' + rpmd.ProviderCredentialsDSC AS ProviderNM
	,rpnp.ProviderNM +' '+ rpnp.ProviderCredentialsDSC AS AccompanyingProviderNM
	,NULL AS CPTDSC
	,NULL AS RateAMT
	,2250 ExpenseAMT
	,'Daily' AS EarningsTypeDSC
	,NULL AS ProcedureCNT
	,CASE WHEN pe.EntityNM IS NULL THEN rpmd.ProviderNM +' ' + rpmd.ProviderCredentialsDSC ELSE pe.EntityNM END AS PayToNM
FROM
	Clinical.PatientVisit AS p
	
	LEFT JOIN Billing.ExpensePeriod AS e
		ON p.ServiceDTS <= e.PayrollPeriodEndDTS
			AND p.ServiceDTS >= e.PayrollPeriodStartDTS

	LEFT JOIN Reference.Provider AS rpmd
		ON p.AttendingPhysicianJoinID = rpmd.ProviderJoinID
		
	LEFT JOIN Reference.Provider AS rpnp
		ON p.NursePractitionerJoinID = rpnp.ProviderJoinID

	LEFT JOIN Compensation.PayEntity AS pe
		ON p.AttendingPhysicianJoinID = pe.ProviderJoinID
WHERE 
	p.AttendingPhysicianJoinID IN ('Giurgius','Johnson')
	AND CONVERT(DATE,'2025-08-01') BETWEEN e.PayrollProcessingStartDTS AND e.PayrollPayDTS
	AND DATEPART(WEEKDAY, p.ServiceDTS) IN (1,7)
)
,WeekendDeDup AS(
SELECT
	PayrollPayDTS
	,PayrollPeriodDSC
	,ServiceDTS
	,ProviderNM
	,AccompanyingProviderNM
	,CPTDSC
	,RateAMT
	,ExpenseAMT
	,EarningsTypeDSC
	,ProcedureCNT
	,PayToNM
	,ROW_NUMBER() OVER(PARTITION BY ProviderNM,ServiceDTS ORDER BY AccompanyingProviderNM DESC) AS DailyPaySEQ
FROM
	WeekendPay
)
,CMOPay AS( --This was originally written for Shadee, our CMO, but CMO pay isn't really included here and it's morphing more into a standard physician query, should change wording
SELECT
	PayrollPayDTS
	,PayrollPeriodDSC
	,ServiceDTS
	,ProviderNM
	,AccompanyingProviderNM
	,CPTDSC
	,RateAMT
	,ExpenseAMT
	,EarningsTypeDSC
	,ProcedureCNT
	,PayToNM
FROM
	Dedup
WHERE
	DailyPaySEQ = 1

UNION ALL

SELECT
	PayrollPayDTS
	,PayrollPeriodDSC
	,ServiceDTS
	,ProviderNM
	,AccompanyingProviderNM
	,CPTDSC
	,RateAMT
	,ExpenseAMT
	,EarningsTypeDSC
	,ProcedureCNT
	,PayToNM
FROM
	WeekendDeDup
WHERE
	DailyPaySEQ = 1

UNION ALL

SELECT
	e.PayrollPayDTS
	,e.PayrollPeriodDSC
	,p.ServiceDTS
	,rpmd.ProviderNM +' ' + rpmd.ProviderCredentialsDSC AS ProviderNM
	,rpnp.ProviderNM +' '+ rpnp.ProviderCredentialsDSC AS AccompanyingProviderNM
	,c.CPTDSC
	,pr.ExpenseAMT AS RateAMT
	,SUM(pr.ExpenseAMT)
	,'Per Patient' AS EarningsTypeDSC
	,COUNT(c.CPTDSC) AS ProcedureCNT
	,CASE WHEN pe.EntityNM IS NULL THEN rpmd.ProviderNM +' ' + rpmd.ProviderCredentialsDSC ELSE pe.EntityNM END AS PayToNM
FROM 
	Clinical.PatientVisit AS p
	
	LEFT JOIN Billing.ExpensePeriod AS e 
		ON p.ServiceDTS >= e.PayrollPeriodStartDTS
			AND p.ServiceDTS <= e.PayrollPeriodEndDTS
			
	LEFT JOIN Billing.CPTCode AS c
		ON p.CPTCD = c.CPTCD

	LEFT JOIN Compensation.ProviderRate AS pr
		ON p.AttendingPhysicianJoinID = pr.ProviderJoinID
			AND p.CPTCD = pr.CPTCD
		
	LEFT JOIN Reference.Provider AS rpmd
		ON p.AttendingPhysicianJoinID = rpmd.ProviderJoinID
		
	LEFT JOIN Reference.Provider AS rpnp
		ON p.NursePractitionerJoinID = rpnp.ProviderJoinID

	LEFT JOIN Compensation.PayEntity AS pe
		ON p.AttendingPhysicianJoinID = pe.ProviderJoinID
WHERE 
	p.AttendingPhysicianJoinID IN ('Giurgius','Johnson')
	AND c.CPTCD IN ('90792','99239')
	AND CONVERT(DATE,'2025-08-01') BETWEEN e.PayrollProcessingStartDTS AND e.PayrollPayDTS
	AND DATEPART(WEEKDAY, p.ServiceDTS) NOT IN (1,7)
GROUP BY
	e.PayrollPayDTS
	,e.PayrollPeriodDSC
	,rpmd.ProviderNM +' ' + rpmd.ProviderCredentialsDSC
	,rpnp.ProviderNM +' '+ rpnp.ProviderCredentialsDSC
	,p.ServiceDTS
	,c.CPTDSC
	,pr.ExpenseAMT
	,pe.EntityNM
)
INSERT INTO Consilient.Compensation.Payroll(
	PayrollID
	,PayrollPayDTS
	,PayrollPeriodDSC
	,ServiceDTS
	,ProviderNM
	,AccompanyingProviderNM
	,CPTDSC
	,RateAMT
	,ExpenseAMT
	,EarningsTypeDSC
	,ProcedureCNT
	,PayToNM
	,CreatedDTS
	)
SELECT
	CONCAT(c.ServiceDTS,'.',c.ProviderNM,'.',c.EarningsTypeDSC,'.',c.CPTDSC) AS PayrollID
	,c.PayrollPayDTS
	,c.PayrollPeriodDSC
	,c.ServiceDTS
	,c.ProviderNM
	,c.AccompanyingProviderNM
	,c.CPTDSC
	,c.RateAMT
	,c.ExpenseAMT
	,c.EarningsTypeDSC
	,c.ProcedureCNT
	,c.PayToNM
	,GETDATE() AS CreatedDTS
FROM
	CMOPay AS c
WHERE
	NOT EXISTS (
    SELECT 
		1 
    FROM
		Consilient.Compensation.Payroll AS p
    WHERE 
        p.PayrollID = CONCAT(c.ServiceDTS,'.',c.ProviderNM,'.',c.EarningsTypeDSC,'.',c.CPTDSC)

		)
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetInvoiceWeekendRevenue]
AS
BEGIN
WITH Weekend AS(
SELECT DISTINCT
	ServiceDTS
	,CONCAT(md.ProviderNM,' ',md.ProviderCredentialsDSC) AS AttendingPhysicianNM
	,CASE WHEN pv.NursePractitionerJoinID IS NULL THEN NULL ELSE CONCAT(np.ProviderNM,' ',np.ProviderCredentialsDSC) END AS MidLevelNM
	,NursePractitionerJoinID
	,i.InvoicePeriodDSC
FROM
	Consilient.Clinical.PatientVisit AS pv

	INNER JOIN Billing.InvoicePeriod AS i
		ON pv.ServiceDTS BETWEEN i.InvoicePeriodStartDTS AND i.InvoicePeriodEndDTS

	LEFT JOIN Consilient.Reference.Provider AS md
		ON pv.AttendingPhysicianJoinID = md.ProviderJoinID

	LEFT JOIN Consilient.Reference.Provider AS np
		ON pv.NursePractitionerJoinID = np.ProviderJoinID
WHERE
	DATEPART(WEEKDAY, ServiceDTS) IN (1,7)
	--AND pv.ServiceDTS BETWEEN CONVERT(DATE,'2025-07-14') AND CONVERT(DATE,'2025-08-01') 
)
,WeekendPhysician AS(
SELECT DISTINCT
	ServiceDTS
	,AttendingPhysicianNM
	,1500.00 As InvoiceAMT
FROM
	Weekend
)
,WeekendMidlevel AS(
SELECT DISTINCT
	ServiceDTS
	,MidlevelNM
	,750.00 As InvoiceAMT
FROM
	Weekend
WHERE
	MidlevelNM IS NOT NULL
)
,WeekendData AS(
SELECT
	ServiceDTS
	,AttendingPhysicianNM As ProviderNM
	,InvoiceAMT
    -- Find the Saturday of the weekend this date belongs to:
    ,DATEADD(DAY, - (DATEPART(WEEKDAY, ServiceDTS) + @@DATEFIRST - 7) % 7, ServiceDTS) AS WeekendStartDTS
FROM
	WeekendPhysician

UNION

SELECT
	ServiceDTS
	,MidlevelNM AS ProviderNM
	,InvoiceAMT
    -- Find the Saturday of the weekend this date belongs to:
    ,DATEADD(DAY, - (DATEPART(WEEKDAY, ServiceDTS) + @@DATEFIRST - 7) % 7, ServiceDTS) AS WeekendStartDTS
FROM
	WeekendMidlevel
)
,InvoiceWeekendRevenue AS(
SELECT
  WeekendStartDTS
  ,ProviderNM
  ,COUNT(*) AS DaysWorkedOnWeekendCNT
  ,InvoiceAMT --750 NP
FROM
	WeekendData
GROUP BY
	WeekendStartDTS
	,ProviderNM
	,InvoiceAMT
HAVING
	COUNT(*) = 2 --No Stipend if whole weekend isn't worked
)
INSERT INTO Consilient.Billing.InvoiceWeekendRevenue(
	WeekendStartDTS
	,ProviderNM
	,DaysWorkedOnWeekendCNT
	,InvoiceAMT
	)
SELECT
	WeekendStartDTS
	,ProviderNM
	,DaysWorkedOnWeekendCNT
	,InvoiceAMT
FROM
	InvoiceWeekendRevenue AS i
WHERE
	NOT EXISTS (
    SELECT 
		1 
    FROM
		Consilient.Billing.InvoiceWeekendRevenue AS t
    WHERE 
        t.WeekendStartDTS = i.WeekendStartDTS
		AND t.ProviderNM = i.ProviderNM
		)
END



GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE
	[dbo].[GetMidlevelEarningsStatements]
AS
BEGIN
WITH MidlevelPay AS(
SELECT
	e.PayrollPayDTS
	,e.PayrollPeriodDSC
	,p.ServiceDTS
	,rpnp.ProviderNM +' ' +rpnp.ProviderCredentialsDSC AS ProviderNM
	,rpmd.ProviderNM +' '+ rpmd.ProviderCredentialsDSC AS AccompanyingProviderNM
	,c.CPTDSC
	,npr.ExpenseAMT AS RateAMT
	,SUM(npr.ExpenseAMT) AS ExpenseAMT
	,COUNT(c.CPTDSC) AS ProcedureCNT
	,'Per Patient' AS EarningsTypeDSC
	,CASE WHEN pe.EntityNM IS NULL THEN rpnp.ProviderNM +' ' + rpnp.ProviderCredentialsDSC ELSE pe.EntityNM END AS PayToNM
FROM
	Clinical.PatientVisit AS p
	
	INNER JOIN Billing.ExpensePeriod AS e
		ON p.ServiceDTS BETWEEN e.PayrollPeriodStartDTS AND e.PayrollPeriodEndDTS
		
	LEFT JOIN Billing.CPTCode AS c
		ON p.CPTCD = c.CPTCD
			
	INNER JOIN Compensation.ProviderRate AS npr
		ON p.NursePractitionerJoinID = npr.ProviderJoinID
			AND p.CPTCD = npr.CPTCD
			
	LEFT JOIN Reference.Provider AS rpmd
		ON p.AttendingPhysicianJoinID = rpmd.ProviderJoinID
		
	LEFT JOIN Reference.Provider AS rpnp
		ON p.NursePractitionerJoinID = rpnp.ProviderJoinID

	LEFT JOIN Compensation.PayEntity AS pe
		ON p.NursePractitionerJoinID = pe.ProviderJoinID
WHERE
	CONVERT(DATE,'2025-08-01') BETWEEN e.PayrollProcessingStartDTS AND e.PayrollPayDTS
GROUP BY
	e.PayrollPayDTS
	,e.PayrollPeriodDSC
	,p.ServiceDTS
	,rpnp.ProviderNM +' ' +rpnp.ProviderCredentialsDSC
	,rpmd.ProviderNM +' '+ rpmd.ProviderCredentialsDSC
	,c.CPTDSC
	,npr.ExpenseAMT
	,pe.EntityNM
)
INSERT INTO Consilient.Compensation.Payroll(
	PayrollID
	,PayrollPayDTS
	,PayrollPeriodDSC
	,ServiceDTS
	,ProviderNM
	,AccompanyingProviderNM
	,CPTDSC
	,RateAMT
	,ExpenseAMT
	,EarningsTypeDSC
	,ProcedureCNT
	,PayToNM
	,CreatedDTS
	)
SELECT
	CONCAT(c.ServiceDTS,'.',c.ProviderNM,'.',c.EarningsTypeDSC,'.',c.CPTDSC,'.',c.AccompanyingProviderNM) AS PayrollID --Added Accompanying ProviderNM to pk 8/1/25, should I modify previous data to include this in the value?
	,c.PayrollPayDTS
	,c.PayrollPeriodDSC
	,c.ServiceDTS
	,c.ProviderNM
	,c.AccompanyingProviderNM
	,c.CPTDSC
	,c.RateAMT
	,c.ExpenseAMT
	,c.EarningsTypeDSC
	,c.ProcedureCNT
	,c.PayToNM
	,GETDATE() AS CreatedDTS
FROM
	Midlevelpay AS c
WHERE
	NOT EXISTS (
    SELECT 
		1 
    FROM
		Consilient.Compensation.Payroll AS p
    WHERE 
        p.PayrollID = CONCAT(c.ServiceDTS,'.',c.ProviderNM,'.',c.EarningsTypeDSC,'.',c.CPTDSC)
		)
END



GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetPerPatientEarningsStatements]
AS
BEGIN
WITH PerPatientPay AS(
SELECT
	e.PayrollPayDTS
	,e.PayrollPeriodDSC
	,p.ServiceDTS
	,rpmd.ProviderNM +' ' + rpmd.ProviderCredentialsDSC AS ProviderNM
	,rpnp.ProviderNM +' '+ rpnp.ProviderCredentialsDSC AS AccompanyingProviderNM
	,c.CPTDSC
	,pr.ExpenseAMT AS RateAMT
	,SUM(pr.ExpenseAMT) AS ExpenseAMT
	,'Per Patient' AS EarningsTypeDSC
	,COUNT(c.CPTDSC) AS ProcedureCNT
	,CASE WHEN pe.EntityNM IS NULL THEN rpmd.ProviderNM +' ' + rpmd.ProviderCredentialsDSC ELSE pe.EntityNM END AS PayToNM
FROM 
	Clinical.PatientVisit AS p
	
	LEFT JOIN Billing.ExpensePeriod AS e 
		ON p.ServiceDTS >= e.PayrollPeriodStartDTS
			AND p.ServiceDTS <= e.PayrollPeriodEndDTS
			
	LEFT JOIN Billing.CPTCode AS c
		ON p.CPTCD = c.CPTCD

	LEFT JOIN Compensation.ProviderRate AS pr
		ON p.AttendingPhysicianJoinID = pr.ProviderJoinID
			AND p.CPTCD = pr.CPTCD
			AND CASE WHEN p.NursePractitionerJoinID IS NULL THEN 0 ELSE 1 END = pr.SupervisingFLG --Crappy join, need to rework
		
	LEFT JOIN Reference.Provider AS rpmd
		ON p.AttendingPhysicianJoinID = rpmd.ProviderJoinID
		
	LEFT JOIN Reference.Provider AS rpnp
		ON p.NursePractitionerJoinID = rpnp.ProviderJoinID

	LEFT JOIN Compensation.PayEntity AS pe
		ON p.AttendingPhysicianJoinID = pe.ProviderJoinID
WHERE 
	p.AttendingPhysicianJoinID = 'Torrico'
	AND CONVERT(DATE,'2025-07-16') BETWEEN e.PayrollProcessingStartDTS AND e.PayrollPayDTS
GROUP BY
	e.PayrollPayDTS
	,e.PayrollPeriodDSC
	,rpmd.ProviderNM +' ' + rpmd.ProviderCredentialsDSC
	,rpnp.ProviderNM +' '+ rpnp.ProviderCredentialsDSC
	,p.ServiceDTS
	,c.CPTDSC
	,pr.ExpenseAMT
	,pe.EntityNM
)
INSERT INTO Consilient.Compensation.Payroll(
	PayrollID
	,PayrollPayDTS
	,PayrollPeriodDSC
	,ServiceDTS
	,ProviderNM
	,AccompanyingProviderNM
	,CPTDSC
	,RateAMT
	,ExpenseAMT
	,EarningsTypeDSC
	,ProcedureCNT
	,PayToNM
	,CreatedDTS
	)
SELECT
	CONCAT(c.ServiceDTS,'.',c.ProviderNM,'.',c.EarningsTypeDSC,'.',c.CPTDSC) AS PayrollID
	,c.PayrollPayDTS
	,c.PayrollPeriodDSC
	,c.ServiceDTS
	,c.ProviderNM
	,c.AccompanyingProviderNM
	,c.CPTDSC
	,c.RateAMT
	,c.ExpenseAMT
	,c.EarningsTypeDSC
	,c.ProcedureCNT
	,c.PayToNM
	,GETDATE() AS CreatedDTS
FROM
	PerPatientPay AS c
WHERE
	NOT EXISTS (
    SELECT 
		1 
    FROM
		Consilient.Compensation.Payroll AS p
    WHERE 
        p.PayrollID = CONCAT(c.ServiceDTS,'.',c.ProviderNM,'.',c.EarningsTypeDSC,'.',c.CPTDSC)

		)
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetVisitsForReadyPayment]
AS
BEGIN
WITH VisitsReadyForPayment AS(
SELECT
	CONCAT(p.ServiceDTS,' - ',p.MRN,' - ', p.CPTCD) AS VisitForPaymentID 
	,p.ServiceDTS
	,CASE WHEN j.MedRec IS NULL THEN 0 ELSE 1 END AS CompleteDataForBillingFLG
	,j.doa AS AdmitDTS
	,j.dod AS DischargeDTS
	,CASE WHEN DATEPART(WEEKDAY, p.ServiceDTS) IN (1,7) THEN 1 ELSE 0 END AS WeekendFLG 
	,p.PatientNM
	,p.MRN
	,j.DOB AS PatientBirthDTS
	,DATEDIFF(YEAR, j.DOB, GETDATE()) 
    - CASE 
        WHEN DATEADD(YEAR, DATEDIFF(YEAR, j.DOB, GETDATE()), j.DOB) > GETDATE() 
        THEN 1 
        ELSE 0 
      END AS PatientAgeNBR -- Under 22 1: bill to Santa Rosa 22 and Over: Send to billers
	,j.Sex AS SexDSC
	,p.CPTCD
	,j.Dishc_DX AS DischargeDiagnosisCD
	,j.Ad_Dx AS AdmitDiagnosisCD
	,CONCAT(md.ProviderNM,' ',md.ProviderCredentialsDSC) AS AttendingPhysicianNM
	,CASE WHEN p.NursePractitionerJoinID IS NULL THEN NULL
		ELSE CONCAT(np.ProviderNM,' ',np.ProviderCredentialsDSC)
		END AS MidlevelNM
	,i.InvoicePeriodDSC
	,j.Prim_Insurance AS PrimaryInsuranceNM
	,j.SubscriberID AS PrimaryInsuranceSubscriberID
	,_2nd_Insurance AS SecondaryInsuranceNM
	,l.Physician_Included AS InclusiveFLG--Physician Inlcuded 1: 1 Bill to Santa Rosa 2: 0 and NULL Send to Billers
	,'Santa Rosa Behavioral Healthcare Hospital' AS RenderingLocationNM -- Will need to add logic in here and possible from data sources to differentiate now that we have a new one
	,CASE 
		WHEN p.ServiceDTS = CONVERT(DATE,'2025-07-29') AND AttendingPhysicianJoinID = 'Johnson' 
			THEN 'Tyler Torrico MD'
		WHEN p.AttendingPhysicianJoinID = 'Giurgius'
			THEN 'Tyler Torrico MD'
		WHEN
			p.AttendingPhysicianJoinID = 'Kletz'
			THEN NULL
		WHEN AttendingPhysicianJoinID = 'Johnson'
			THEN NULL
		ELSE CONCAT(md.ProviderNM,' ',md.ProviderCredentialsDSC)
	END AS RenderingProviderNM
FROM
	Consilient.Clinical.PatientVisit AS p

	INNER JOIN Consilient.Billing.InvoicePeriod AS i
		ON p.ServiceDTS BETWEEN i.InvoicePeriodStartDTS AND InvoicePeriodEndDTS

	LEFT JOIN Consilient.Billing.JasperReport AS j
		ON p.mrn = j.MedRec
			AND p.ServiceDTS BETWEEN j.doa AND COALESCE(j.dod,GETDATE())

	LEFT JOIN Consilient.Billing.SantaRosaInsuranceInclusiveList AS l
		ON j.C_Group = l.Code

	LEFT JOIN Consilient.Reference.Provider AS md
		ON p.AttendingPhysicianJoinID = md.ProviderJoinID

	LEFT JOIN Consilient.Reference.Provider AS np
		ON p.NursePractitionerJoinID = np.ProviderJoinID
)
--This table doesn't currently have a pk due to patient that duplicates on June 12 Need to resolve this so we can know keep track of which results have
already been inserted into the table as it will grow incrementally
INSERT INTO Consilient.Billing.VisitsReadyForPayment(
	VisitForPaymentID
	,ServiceDTS
	,CompleteDataForBillingFLG
	,AdmitDTS
	,DischargeDTS
	,WeekendFLG 
	,PatientNM
	,MRN
	,PatientBirthDTS
	,SexDSC
	,CPTCD
	,DischargeDiagnosisCD
	,AdmitDiagnosisCD
	,AttendingPhysicianNM
	,MidlevelNM
	,InvoicePeriodDSC
	,PrimaryInsuranceNM
	,PrimaryInsuranceSubscriberID
	,SecondaryInsuranceNM
	,InclusiveFLG
	,RenderingLocationNM
	,RenderingProviderNM
	,CreatedDTS
	)
SELECT
	VisitForPaymentID
	,ServiceDTS
	,CompleteDataForBillingFLG
	,AdmitDTS
	,DischargeDTS
	,WeekendFLG 
	,PatientNM
	,MRN
	,PatientBirthDTS
	,SexDSC
	,CPTCD
	,DischargeDiagnosisCD
	,AdmitDiagnosisCD
	,AttendingPhysicianNM
	,MidlevelNM
	,InvoicePeriodDSC
	,PrimaryInsuranceNM
	,PrimaryInsuranceSubscriberID
	,SecondaryInsuranceNM
	,InclusiveFLG
	,RenderingLocationNM
	,RenderingProviderNM
	,GETDATE() AS CreatedDTS
FROM
	VisitsReadyForPayment AS v
WHERE
	NOT EXISTS (
    SELECT 
		1 
    FROM
		Consilient.Billing.VisitsReadyForPayment AS t
    WHERE 
        t.VisitForPaymentID = v.VisitForPaymentID

		)
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetWeekEndPhysicianEarningsStatements]
AS
BEGIN
WITH WeekendPhysicianPay AS(
SELECT DISTINCT
	e.PayrollPayDTS
	,e.PayrollPeriodDSC
	,p.ServiceDTS
	,pr.ProviderNM +' '+ pr.ProviderCredentialsDSC AS ProviderNM
	,--CASE WHEN DATEPART(WEEKDAY, p.ServiceDTS) IN (1,7) THEN 2250 ELSE 2250 END Get rid of below Case Statement that was written
	to account for the unathorized weekend worked and return to the commented out case statement
	CASE WHEN p.ServiceDTS = CONVERT(DATE,'2025-06-21') THEN 500 ELSE 2250 END AS ExpenseAMT
	,COUNT(p.PatientNM) AS ProcedureCNT
	,'Weekend Coverage' AS EarningsTypeDSC
	,CASE WHEN pe.EntityNM IS NULL THEN pr.ProviderNM +' '+ pr.ProviderCredentialsDSC ELSE pe.EntityNM END AS PayToNM
FROM
	Clinical.PatientVisit AS p
	
	LEFT JOIN Billing.ExpensePeriod AS e
		ON p.ServiceDTS >= e.PayrollPeriodStartDTS
		AND p.ServiceDTS <= e.PayrollPeriodEndDTS
		
	LEFT JOIN Reference.Provider AS pr
		ON p.AttendingPhysicianJoinID = pr.ProviderJoinID

	LEFT JOIN Compensation.PayEntity AS pe
		ON p.AttendingPhysicianJoinID = pe.ProviderJoinID
WHERE
	CONVERT(DATE,'2025-08-01') BETWEEN e.PayrollProcessingStartDTS AND e.PayrollPayDTS
	AND DATEPART(WEEKDAY, p.ServiceDTS) IN (1,7)
	AND pr.ProviderJoinID = 'Kletz'
GROUP BY
	e.PayrollPayDTS
	,e.PayrollPeriodDSC
	,p.ServiceDTS
	,pr.ProviderNM +' '+ pr.ProviderCredentialsDSC
	,pe.EntityNM
)
INSERT INTO Consilient.Compensation.Payroll(
	PayrollID
	,PayrollPayDTS
	,PayrollPeriodDSC
	,ServiceDTS
	,ProviderNM
	,AccompanyingProviderNM
	,CPTDSC
	,RateAMT
	,ExpenseAMT
	,EarningsTypeDSC
	,ProcedureCNT
	,PayToNM
	,CreatedDTS
	)
SELECT
	CONCAT(c.ServiceDTS,'.',c.ProviderNM,'.',c.EarningsTypeDSC) AS PayrollID
	,c.PayrollPayDTS
	,c.PayrollPeriodDSC
	,c.ServiceDTS
	,c.ProviderNM
	,NULL AS AccompanyingProviderNM
	,NULL AS CPTDSC
	,2250 AS RateAMT
	,c.ExpenseAMT
	,c.EarningsTypeDSC
	,c.ProcedureCNT
	,c.PayToNM
	,GETDATE() AS CreatedDTS
FROM
	WeekendPhysicianPay AS c
WHERE
	NOT EXISTS (
    SELECT 
		1 
    FROM
		Consilient.Compensation.Payroll AS p
    WHERE 
        p.PayrollID = CONCAT(c.ServiceDTS,'.',c.ProviderNM,'.',c.EarningsTypeDSC)
		)
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[ProcessBillingAndInvoiceTables]
AS
BEGIN
SET NOCOUNT ON;   
--    ------------------------------------------------------------------------------------------------------------------------------------
--    -- Step 1: Combine Visit data with Jasper Report and generate columns needed for billing and invoicing -> Consilient.Billing.Visits
--    ------------------------------------------------------------------------------------------------------------------------------------
TRUNCATE TABLE Consilient.Billing.Visits;

INSERT INTO Consilient.Billing.Visits (--Need to add a primary key once the 6/12 encounter join issue is fixed
	VisitForPaymentID 
	,FacilityID
	,CaseID
	,ServiceDTS
	,PresentOnJasperReportFLG
	,AdmissionReportAdmitDTS
	,DischargeReportAdmitDTS
	,AdmissionReportDischargeDTS
	,DischargeReportDischargeDTS
	,WeekendFLG 
	,PatientNM
	,MRN
	,PatientStreetAddressTXT
	,PatientCityNM
	,PatientZipCD
	,PatientCountyNM
	,PatientPhoneNBR
	,PatientBirthDTS
	,PatientAgeAtVisitNBR
	,SexDSC
	,CPTCD
	,AdmitDiagnosisCD
	,AttendingPhysicianNM
	,MidlevelNM
	,InvoicePeriodDSC
	,InvoicePeriodEndDTS
	,AdmissionReportPrimaryInsuranceNM
	,DischargeReportPrimaryInsuranceNM
	,AdmissionReportPrimaryInsuranceSubscriberID
	,DischargeReportPrimaryInsuranceSubscriberID
	,AdmissionReportSecondaryInsuranceNM
	,DischargeReportSecondaryInsuranceNM
	,AdmissionReportInclusiveFLG
	,DischargeReportInclusiveFLG
	,AdmissionReportSendToInsuranceFLG
	,DischargeReportSendToInsuranceFLG
	,AdmissionReportNonShortDoyleOnlyFLG
	,DischargeReportNonShortDoyleOnlyFLG
	,AdmissionReportInvoiceHospitalFLG
	,DischargeReportInvoiceHospitalFLG
	,RenderingLocationNM -- Will need to add logic in here and possible from data sources to differentiate now that we have a new one
	,RenderingProviderNM
	,AdmissionReportInsuranceCarrierGroupNM
	,DischargeReportInsuranceCarrierGroupNM
	,CredentialedDTS
	,ProviderCarrierID
	,DischargeDiagnosisDSC
	,PresentOnDischargeJasperReportFLG
	,AdmissionReportCountyFLG
	,DischargeReportCountyFLG
	,AdmissionReportPhysicianIncludedFLG
	,DischargeReportPhysicianIncludedFLG
    )
--Persist Results in Consilient.Billing.Visits
SELECT DISTINCT --9,427
	CONCAT(p.DateServiced,' - ',pat.PatientMRN,' - ', st.CPTCode) AS VisitForPaymentID 
	,p.FacilityID
	,p.AdmissionNumber AS CaseID
	,p.DateServiced AS ServiceDTS
	,CASE WHEN j.MRN IS NULL THEN 0 ELSE 1 END AS PresentOnJasperReportFLG
	,j.AdmitDTS AS AdmissionReportAdmitDTS
	,dj.AdmitDTS AS DischargeReportAdmitDTS
	,j.DischargeDTS AS AdmissionReportDischargeDTS
	,dj.DischargeDTS AS DischargeReportDischargeDTS
	,CASE WHEN DATEPART(WEEKDAY, p.DateServiced) IN (1,7) THEN 1 ELSE 0 END AS WeekendFLG 
	,pat.PatientFullName AS PatientNM
	,pat.PatientMRN AS MRN
	,j.PatientAddressDSC AS PatientStreetAddressTXT
	,j.PatientCityNM
	,j.PatientZipCD
	,j.PatientCountyNM
	,j.PatientPhoneNBR
	,j.PatientBirthDTS
	,(DATEDIFF(YEAR, j.PatientBirthDTS, p.DateServiced) 
    - CASE 
        WHEN DATEADD(YEAR, DATEDIFF(YEAR, j.PatientBirthDTS, p.DateServiced), j.PatientBirthDTS) > p.DateServiced
        THEN 1 
        ELSE 0 
      END) AS PatientAgeAtVisitNBR
	,j.PatientSexDSC AS SexDSC
	,st.CPTCode AS CPTCD
	,j.AdmitDiagnosisCD
	,md.FullName AS AttendingPhysicianNM
	,np.FullName AS MidlevelNM
	,i.InvoicePeriodDSC
	,i.InvoicePeriodEndDTS
	,j.PrimaryInsuranceNM AS AdmissionReportPrimaryInsuranceNM
	,dj.PrimaryInsuranceNM AS DischargeReportPrimaryInsuranceNM
	,j.PrimaryInsuranceSubscriberID AS AdmissionReportPrimaryInsuranceSubscriberID
	,dj.PrimaryInsuranceSubscriberID AS DischargeReportPrimaryInsuranceSubscriberID
	,j.SecondaryInsuranceNM AS AdmissionReportSecondaryInsuranceNM
	,dj.SecondaryInsuranceNM AS DischargeReportSecondaryInsuranceNM
	,CASE WHEN j.MRN IS NULL THEN NULL ELSE COALESCE(l.PhysicianIncludedFLG,0) END AS AdmissionReportInclusiveFLG
	,CASE WHEN dj.MRN IS NULL THEN NULL ELSE COALESCE(dl.PhysicianIncludedFLG,0) END AS DischargeReportInclusiveFLG
	,CASE WHEN j.MRN IS NULL THEN NULL
		WHEN j.PrimaryInsuranceGroupID = 'MCAL' THEN 1 ELSE COALESCE(l.SendToInsuranceFLG,1) END AS AdmissionReportSendToInsuranceFLG
	,CASE WHEN dj.MRN IS NULL THEN NULL
		WHEN dj.PrimaryInsuranceGroupID = 'MCAL' THEN 1 ELSE COALESCE(dl.SendToInsuranceFLG,1) END AS DischargeReportSendToInsuranceFLG
	,CASE WHEN j.MRN IS NULL THEN NULL ELSE COALESCE(l.NonShortDoyleOnlyFLG,0) END AS NonShortDoyleOnlyFLG
	,CASE WHEN dj.MRN IS NULL THEN NULL ELSE COALESCE(dl.NonShortDoyleOnlyFLG,0) END AS NonShortDoyleOnlyFLG
	,CASE WHEN j.MRN IS NULL THEN NULL ELSE COALESCE(l.InvoiceHospitalFLG,0) END AS AdmissionReportInvoiceHospitalFLG
	,CASE WHEN dj.MRN IS NULL THEN NULL ELSE COALESCE(dl.InvoiceHospitalFLG,0) END AS DischargeReportInvoiceHospitalFLG
	,f.FacilityName AS RenderingLocationNM -- Will need to add logic in here and possible from data sources to differentiate now that we have a new one
	,CASE 
		WHEN p.DateServiced = CONVERT(DATE,'2025-07-29') AND md.EmployeeID = 4
			THEN 'Tyler Torrico MD'
		ELSE md.FullName
	END AS RenderingProviderNM
	,COALESCE(il.InsuranceCarrierGroupNM,'Unmapped') AS AdmissionReportInsuranceCarrierGroupNM
	,COALESCE(dil.InsuranceCarrierGroupNM,'Unmapped') AS DischargeReportInsuranceCarrierGroupNM
	,c.CredentialedDTS
	,c.ProviderCarrierID
	,dj.DischargeDiagnosisDSC
	,CASE WHEN dj.MRN IS NULL THEN 0 ELSE 1 END AS PresentOnDischargeJasperReportFLG
	,CASE WHEN il.InsuranceCarrierGroupNM = 'MCAL' AND p.FacilityID = 1 THEN 1
		  WHEN p.FacilityID = 2 and j.PrimaryInsuranceNM LIKE '%County%' THEN 1
		  ELSE 0 END AS AdmissionReportCountyFLG
	,CASE WHEN dil.InsuranceCarrierGroupNM = 'MCAL' AND p.FacilityID = 1 THEN 1 
		   WHEN p.FacilityID = 2 and dj.PrimaryInsuranceNM LIKE '%County%' THEN 1
		ELSE 0 END AS DischargeReportCountyFLG
	,j.PhysicianIncludedFLG AS AdmissionReportPhysicianIncludedFLG --Currently just used on Ventura as Santa Rosa links to Insurance Lookup table for non short doyle info
	,dj.PhysicianIncludedFLG AS DischargeReportPhysicianIncludedFLG
 FROM
	Consilient.Clinical.PatientVisits AS p

	INNER JOIN Consilient.Billing.InvoicePeriod AS i
		ON p.DateServiced BETWEEN i.InvoicePeriodStartDTS AND InvoicePeriodEndDTS

	INNER JOIN Consilient.Clinical.Patients AS pat
		ON p.PatientID = pat.PatientID

	INNER JOIN Consilient.Clinical.ServiceTypes AS st
		ON p.ServiceTypeID = st.ServiceTypeID

	LEFT JOIN Consilient.Billing.vw_JasperReport AS j
		ON (p.FacilityID = j.FacilityID
			AND p.DateServiced < CONVERT(DATE,'2025-08-01')
			AND pat.PatientMRN = j.MRN
			AND p.DateServiced BETWEEN j.AdmitDTS AND COALESCE(j.DischargeDTS,GETDATE()))
			OR
			(p.FacilityID = j.FacilityID
			AND p.DateServiced >= CONVERT(DATE,'2025-08-01')
			AND	p.AdmissionNumber = j.CaseID
			AND p.DateServiced BETWEEN j.AdmitDTS AND COALESCE(j.DischargeDTS,GETDATE())
			)

	LEFT JOIN Consilient.Billing.vw_JasperDischargeReport AS dj
		ON (p.FacilityID = dj.FacilityID
			AND p.DateServiced < CONVERT(DATE,'2025-08-01')
			AND pat.PatientMRN = dj.MRN
			AND p.DateServiced BETWEEN dj.AdmitDTS AND COALESCE(dj.DischargeDTS,GETDATE()))
			OR
			(p.FacilityID = dj.FacilityID
			AND p.DateServiced >= CONVERT(DATE,'2025-08-01')
			AND	p.AdmissionNumber = dj.CaseID
			AND p.DateServiced BETWEEN dj.AdmitDTS AND COALESCE(j.DischargeDTS,GETDATE())
			)

	LEFT JOIN Consilient.Billing.SantaRosaInsuranceContract AS l
		ON j.PrimaryInsuranceGroupID = l.InsuranceCD
			AND p.DateServiced BETWEEN l.EffectiveStartDTS AND COALESCE(l.EffectiveEndDTS,GETDATE())

	LEFT JOIN Consilient.Billing.SantaRosaInsuranceContract AS dl
		ON dj.PrimaryInsuranceGroupID = dl.InsuranceCD
			AND p.DateServiced BETWEEN dl.EffectiveStartDTS AND COALESCE(dl.EffectiveEndDTS,GETDATE())

	LEFT JOIN Consilient.Compensation.Employees AS md
		ON p.PhysicianEmployeeID = md.EmployeeID

	LEFT JOIN Consilient.Compensation.Employees AS np
		ON p.NursePractitionerEmployeeID = np.EmployeeID

	INNER JOIN Consilient.Clinical.Facilities AS f
		ON p.FacilityID = f.FacilityID

	LEFT JOIN Consilient.Billing.InsuranceLookup AS il
		ON j.PrimaryInsuranceNM = il.PrimaryInsuranceNM

	LEFT JOIN Consilient.Billing.InsuranceLookup AS dil
		ON dj.PrimaryInsuranceNM = dil.PrimaryInsuranceNM

	LEFT JOIN Consilient.Billing.Credentialing AS c
		ON md.EmployeeID = c.EmployeeID
			AND il.InsuranceCarrierGroupNM = c.CarrierNM
--Shows Duplicates
WHERE
	CONCAT(p.DateServiced,' - ',pat.PatientMRN,' - ', st.CPTCode)  IN ('2025-06-12 - 120696 - 90792'
,'2025-06-12 - 120696 - 99239'--Travis Smith known issue
,'2025-08-17 - 126343 - 99233'
,'2025-08-14 - 126343 - 90792'--Veronica Holt looks like she might be duplicated on the Jasper Report
,'2025-08-15 - 126343 - 99233'
,'2025-08-16 - 126343 - 99233')

;
    ------------------------------------------------------------------------------------------------------------------------------------
    -- Step 2: Create batches to send to Billing Team -> Consilient.Billing.Batch
    ------------------------------------------------------------------------------------------------------------------------------------
WITH SantaRosaBatch AS(
SELECT
	b.VisitForPaymentID AS BatchID
	,b.CaseID
	,b.ServiceDTS
	,b.AdmissionReportAdmitDTS AS AdmitDTS
	,b.AdmissionReportDischargeDTS AS DischargeDTS
	,b.PatientNM
	,b.MRN
	,b.PatientPhoneNBR
	,b.PatientStreetAddressTXT
	,b.PatientCityNM
	,b.PatientZipCD
	,b.PatientCountyNM
	,b.SexDSC
	,b.PatientBirthDTS
	,b.PatientAgeAtVisitNBR
	,CASE WHEN b.PatientAgeAtVisitNBR < 22 OR b.PatientAgeAtVisitNBR > 64 THEN 1 ELSE 0 END AS NonShortDoyleFLG
	,b.CPTCD
	,b.AdmitDiagnosisCD
	,NULL AS DischargeDiagnosisDSC
	,b.RenderingProviderNM
	,b.RenderingLocationNM
	,b.AdmissionReportPrimaryInsuranceNM AS PrimaryInsuranceNM
	,b.AdmissionReportPrimaryInsuranceSubscriberID AS PrimaryInsuranceSubscriberID
	,b.AdmissionReportSecondaryInsuranceNM AS SecondaryInsuranceNM
	,b.AdmissionReportInclusiveFLG AS InclusiveFLG
	,b.AdmissionReportSendToInsuranceFLG AS SendToInsuranceFLG
	,b.AdmissionReportNonShortDoyleOnlyFLG AS NonShortDoyleOnlyFLG
	,b.AdmissionReportInvoiceHospitalFLG AS InvoiceHospitalFLG
	,b.AdmissionReportInsuranceCarrierGroupNM AS InsuranceCarrierGroupNM
	,b.CredentialedDTS
	,b.ProviderCarrierID
	,b.AdmissionReportCountyFLG AS CountyFLG
	,GETDATE() AS BatchDTS
FROM	
	Consilient.Billing.Visits AS b	
WHERE
	b.PresentOnJasperReportFLG = 1
	AND b.RenderingProviderNM IS NOT NULL
	AND b.AdmissionReportSendToInsuranceFLG = 1
	AND b.AdmissionReportCountyFLG = 0
	AND b.RenderingLocationNM = 'Aurora Santa Rosa Hospital Behavioral Healthcare Hospital'

UNION ALL

SELECT
	b.VisitForPaymentID AS BatchID
	,b.CaseID
	,b.ServiceDTS
	,b.DischargeReportAdmitDTS AS AdmitDTS
	,b.DischargeReportDischargeDTS AS DischargeDTS
	,b.PatientNM
	,b.MRN
	,b.PatientPhoneNBR
	,b.PatientStreetAddressTXT
	,b.PatientCityNM
	,b.PatientZipCD
	,b.PatientCountyNM
	,b.SexDSC
	,b.PatientBirthDTS
	,b.PatientAgeAtVisitNBR
	,CASE WHEN b.PatientAgeAtVisitNBR < 22 OR b.PatientAgeAtVisitNBR > 64 THEN 1 ELSE 0 END AS NonShortDoyleFLG
	,b.CPTCD
	,b.AdmitDiagnosisCD
	,b.DischargeDiagnosisDSC
	,b.RenderingProviderNM
	,b.RenderingLocationNM
	,b.DischargeReportPrimaryInsuranceNM AS PrimaryInsuranceNM
	,b.DischargeReportPrimaryInsuranceSubscriberID AS PrimaryInsuranceSubscriberID
	,b.DischargeReportSecondaryInsuranceNM AS SecondaryInsuranceNM
	,b.DischargeReportInclusiveFLG AS InclusiveFLG
	,b.DischargeReportSendToInsuranceFLG AS SendToInsuranceFLG
	,b.DischargeReportNonShortDoyleOnlyFLG AS NonShortDoyleOnlyFLG
	,b.DischargeReportInvoiceHospitalFLG AS InvoiceHospitalFLG
	,b.DischargeReportInsuranceCarrierGroupNM AS InsuranceCarrierGroupNM
	,b.CredentialedDTS
	,b.ProviderCarrierID
	,b.DischargeReportCountyFLG AS CountyFLG
	,GETDATE() AS BatchDTS
FROM	
	Consilient.Billing.Visits AS b	
WHERE
	b.PresentOnDischargeJasperReportFLG = 1
	AND b.RenderingProviderNM IS NOT NULL
	AND b.DischargeReportSendToInsuranceFLG = 1
	AND b.DischargeReportCountyFLG = 1
	AND b.DischargeDiagnosisDSC IS NOT NULL
	AND b.RenderingLocationNM = 'Aurora Santa Rosa Hospital Behavioral Healthcare Hospital'
)
,VenturaBatch AS(
SELECT
	b.VisitForPaymentID AS BatchID
	,b.CaseID
	,b.ServiceDTS
	,b.AdmissionReportAdmitDTS AS AdmitDTS
	,b.AdmissionReportDischargeDTS AS DischargeDTS
	,b.PatientNM
	,b.MRN
	,b.PatientPhoneNBR
	,b.PatientStreetAddressTXT
	,b.PatientCityNM
	,b.PatientZipCD
	,b.PatientCountyNM
	,b.SexDSC
	,b.PatientBirthDTS
	,b.PatientAgeAtVisitNBR
	,CASE WHEN b.PatientAgeAtVisitNBR < 22 OR b.PatientAgeAtVisitNBR > 64 THEN 1 ELSE 0 END AS NonShortDoyleFLG
	,b.CPTCD
	,b.AdmitDiagnosisCD
	,NULL AS DischargeDiagnosisDSC
	,b.RenderingProviderNM
	,b.RenderingLocationNM
	,b.AdmissionReportPrimaryInsuranceNM AS PrimaryInsuranceNM
	,b.AdmissionReportPrimaryInsuranceSubscriberID AS PrimaryInsuranceSubscriberID
	,b.AdmissionReportSecondaryInsuranceNM AS SecondaryInsuranceNM
	,b.AdmissionReportInclusiveFLG AS InclusiveFLG
	,NULL AS SendToInsuranceFLG
	,NULL AS NonShortDoyleOnlyFLG
	,NULL AS InvoiceHospitalFLG
	,b.AdmissionReportInsuranceCarrierGroupNM AS InsuranceCarrierGroupNM
	,b.CredentialedDTS
	,b.ProviderCarrierID
	,b.AdmissionReportCountyFLG AS CountyFLG
	,b.AdmissionReportPhysicianIncludedFLG AS PhysicianIncludedFLG
	,GETDATE() AS BatchDTS
FROM	
	Consilient.Billing.Visits AS b	
WHERE
	b.PresentOnJasperReportFLG = 1
	AND b.AdmissionReportPhysicianIncludedFLG = 0
	AND b.AdmissionReportCountyFLG = 0
	AND	b.RenderingLocationNM = 'Aurora Vista Del Mar Ventura'

UNION ALL

SELECT
	b.VisitForPaymentID AS BatchID
	,b.CaseID
	,b.ServiceDTS
	,b.DischargeReportAdmitDTS AS AdmitDTS
	,b.DischargeReportDischargeDTS AS DischargeDTS
	,b.PatientNM
	,b.MRN
	,b.PatientPhoneNBR
	,b.PatientStreetAddressTXT
	,b.PatientCityNM
	,b.PatientZipCD
	,b.PatientCountyNM
	,b.SexDSC
	,b.PatientBirthDTS
	,b.PatientAgeAtVisitNBR
	,CASE WHEN b.PatientAgeAtVisitNBR < 22 OR b.PatientAgeAtVisitNBR > 64 THEN 1 ELSE 0 END AS NonShortDoyleFLG
	,b.CPTCD
	,b.AdmitDiagnosisCD
	,b.DischargeDiagnosisDSC
	,b.RenderingProviderNM
	,b.RenderingLocationNM
	,b.DischargeReportPrimaryInsuranceNM AS PrimaryInsuranceNM
	,b.DischargeReportPrimaryInsuranceSubscriberID AS PrimaryInsuranceSubscriberID
	,b.DischargeReportSecondaryInsuranceNM AS SecondaryInsuranceNM
	,b.DischargeReportInclusiveFLG AS InclusiveFLG
	,NULL AS SendToInsuranceFLG
	,NULL AS NonShortDoyleOnlyFLG
	,NULL AS InvoiceHospitalFLG
	,b.DischargeReportInsuranceCarrierGroupNM AS InsuranceCarrierGroupNM
	,b.CredentialedDTS
	,b.ProviderCarrierID
	,b.DischargeReportCountyFLG AS CountyFLG
	,b.DischargeReportPhysicianIncludedFLG AS PhysicianIncludedFLG
	,GETDATE() AS BatchDTS
FROM	
	Consilient.Billing.Visits AS b	
WHERE
	b.PresentOnDischargeJasperReportFLG = 1
	AND b.DischargeReportPhysicianIncludedFLG = 0
	AND b.DischargeReportCountyFLG = 1
	AND b.DischargeDiagnosisDSC IS NOT NULL
	AND	b.RenderingLocationNM = 'Aurora Vista Del Mar Ventura'
)
,Batch AS(
SELECT
	BatchID
	,CaseID
	,ServiceDTS
	,AdmitDTS
	,DischargeDTS
	,PatientNM
	,MRN
	,PatientPhoneNBR
	,PatientStreetAddressTXT
	,PatientCityNM
	,PatientZipCD
	,PatientCountyNM
	,SexDSC
	,PatientBirthDTS
	,PatientAgeAtVisitNBR
	,NonShortDoyleFLG
	,CPTCD
	,AdmitDiagnosisCD
	,DischargeDiagnosisDSC
	,RenderingProviderNM
	,RenderingLocationNM
	,PrimaryInsuranceNM
	,PrimaryInsuranceSubscriberID
	,SecondaryInsuranceNM
	,InsuranceCarrierGroupNM
	,CredentialedDTS
	,ProviderCarrierID
	,CountyFLG
	,BatchDTS
FROM
	SantaRosaBatch
WHERE
	NOT (NonShortDoyleOnlyFLG = 1 AND NonShortDoyleFLG = 0) --Removes Short Doyle patients when county only pays for non short doyle

UNION ALL

SELECT
	BatchID
	,CaseID
	,ServiceDTS
	,AdmitDTS
	,DischargeDTS
	,PatientNM
	,MRN
	,PatientPhoneNBR
	,PatientStreetAddressTXT
	,PatientCityNM
	,PatientZipCD
	,PatientCountyNM
	,SexDSC
	,PatientBirthDTS
	,PatientAgeAtVisitNBR
	,NonShortDoyleFLG
	,CPTCD
	,AdmitDiagnosisCD
	,DischargeDiagnosisDSC
	,RenderingProviderNM
	,RenderingLocationNM
	,PrimaryInsuranceNM
	,PrimaryInsuranceSubscriberID
	,SecondaryInsuranceNM
	,InsuranceCarrierGroupNM
	,CredentialedDTS
	,ProviderCarrierID
	,CountyFLG
	,BatchDTS
FROM
	VenturaBatch
)
INSERT INTO Consilient.Billing.Batch(
	BatchID
	,CaseID
	,ServiceDTS
	,AdmitDTS
	,DischargeDTS
	,PatientNM
	,MRN
	,PatientPhoneNBR
	,PatientStreetAddressTXT
	,PatientCityNM
	,PatientZipCD
	,PatientCountyNM
	,SexDSC
	,PatientBirthDTS
	,PatientAgeAtVisitNBR
	,NonShortDoyleFLG
	,CPTCD
	,AdmitDiagnosisCD
	,DischargeDiagnosisDSC
	,RenderingProviderNM
	,RenderingLocationNM
	,PrimaryInsuranceNM
	,PrimaryInsuranceSubscriberID
	,SecondaryInsuranceNM
	,InsuranceCarrierGroupNM
	,CredentialedDTS
	,ProviderCarrierID
	,CountyFLG
	,BatchDTS
	)
SELECT
	BatchID
	,CaseID
	,ServiceDTS
	,AdmitDTS
	,DischargeDTS
	,PatientNM
	,MRN
	,PatientPhoneNBR
	,PatientStreetAddressTXT
	,PatientCityNM
	,PatientZipCD
	,PatientCountyNM
	,SexDSC
	,PatientBirthDTS
	,PatientAgeAtVisitNBR
	,NonShortDoyleFLG
	,CPTCD
	,AdmitDiagnosisCD
	,DischargeDiagnosisDSC
	,RenderingProviderNM
	,RenderingLocationNM
	,PrimaryInsuranceNM
	,PrimaryInsuranceSubscriberID
	,SecondaryInsuranceNM
	,InsuranceCarrierGroupNM
	,CredentialedDTS
	,ProviderCarrierID
	,CountyFLG
	,BatchDTS
FROM
	Batch AS b
WHERE
	NOT EXISTS (
    SELECT 
		1 
    FROM
		Consilient.Billing.Batch AS t
    WHERE 
        t.BatchID = b.BatchID
	)
;
----    ------------------------------------------------------------------------------------------------------------------------------------
----    -- Step 3 : Create Invoice to send to Hospital -> Consilient.Billing.InvoiceClinical
----    ------------------------------------------------------------------------------------------------------------------------------------
WITH Invoice AS(
SELECT --Need to clean out columns not needed downstream
Need to build out in Power BI Report Builder
	b.VisitForPaymentID AS InvoiceID
	,b.CaseID
	,b.FacilityID
	,b.AttendingPhysicianNM
	,b.MidlevelNM
	,b.ServiceDTS
	,b.InvoicePeriodDSC
	,b.InvoicePeriodEndDTS
	,b.PatientNM
	,b.MRN
	,b.CPTCD
	,CASE WHEN b.FacilityID = 1 THEN b.AdmissionReportPrimaryInsuranceNM
	ELSE NULL
	END AS PrimaryInsuranceNM
	,b.RenderingLocationNM
	,COALESCE(b.AdmissionReportInclusiveFLG,0) AS InclusiveFLG
	,CASE 
		WHEN b.FacilityID = 1 AND ServiceDTS > CONVERT(DATE,'2025-07-13') AND b.AdmissionReportInclusiveFLG = 1 THEN 1
		--No Ventura Contract
		WHEN b.FacilityID = 2 THEN 0
		ELSE 0
	END AS InvoiceFLG
	,CONVERT(DATE,GETDATE()) AS InvoiceEligibleDTS
FROM	
	Consilient.Billing.Visits AS b
WHERE
	b.AdmissionReportCountyFLG = 1
)
,ClinicalInvoice AS(
SELECT
	i.InvoiceID
	,i.CaseID
	,i.ServiceDTS
	,InvoicePeriodDSC
	,InvoicePeriodEndDTS
	,AttendingPhysicianNM
	,MidlevelNM
	,i.PatientNM
	,i.MRN
	,i.PrimaryInsuranceNM
	,i.CPTCD
	,CASE WHEN i.CPTCD IN ('99231','99232','99233') THEN 'Progress Note' ELSE t.Description END AS CPTDSC
	,i.RenderingLocationNM
	,c.RevenueAMT AS InvoiceAMT
	,InvoiceEligibleDTS
FROM
	Invoice AS i

	LEFT JOIN Billing.CPTCode AS c
		ON i.CPTCD = c.CPTCD
			AND i.FacilityID = c.FacilityID 

	LEFT JOIN Consilient.Clinical.ServiceTypes AS t
		ON i.CPTCD = t.CPTCode
WHERE
	InvoiceFLG = 1

UNION ALL

SELECT
	i.InvoiceID
	,i.CaseID
	,i.ServiceDTS
	,InvoicePeriodDSC
	,InvoicePeriodEndDTS
	,AttendingPhysicianNM
	,MidlevelNM
	,i.PatientNM
	,i.MRN
	,i.PrimaryInsuranceNM
	,i.CPTCD
	,'Indigent Patient Compensation' AS CPTDSC
	,i.RenderingLocationNM
	,60.00 AS InvoiceAMT
	,InvoiceEligibleDTS
FROM
	Invoice AS i

	LEFT JOIN Billing.CPTCode AS c
		ON i.CPTCD = c.CPTCD
			AND i.FacilityID = c.FacilityID 

	LEFT JOIN Consilient.Clinical.ServiceTypes AS t
		ON i.CPTCD = t.CPTCode
WHERE
	PrimaryInsuranceNM LIKE '%I/P%'
	AND i.ServiceDTS > CONVERT(DATE,'2025-07-13')
	AND i.FacilityID = 1
)
INSERT INTO Consilient.Billing.InvoiceClinical(
	InvoiceID
	,CaseID
	,ServiceDTS
	,WeekendFLG
	,InvoicePeriodDSC
	,InvoicePeriodEndDTS
	,AttendingPhysicianNM
	,MidlevelNM
	,PatientNM
	,MRN
	,PrimaryInsuranceNM
	,CPTCD
	,CPTDSC
	,RenderingLocationNM
	,InvoiceAMT
	,InvoiceEligibleDTS
	)
SELECT
	InvoiceID
	,CaseID
	,ServiceDTS
	,CASE WHEN DATEPART(WEEKDAY, ServiceDTS) IN (1,7) THEN 1 ELSE 0 END AS WeekendFLG
	,InvoicePeriodDSC --Becomes insignificant once data lag is introduced
	,InvoicePeriodEndDTS
	,AttendingPhysicianNM
	,MidlevelNM
	,PatientNM
	,MRN
	,PrimaryInsuranceNM
	,CPTCD
	,CPTDSC
	,RenderingLocationNM
	,InvoiceAMT
	,InvoiceEligibleDTS --Use this as parameter for Invoice Generation in PBI Report Builder so patients
	whose insurance data lagged from previous invoice periods will get invoiced as soon as data is present
FROM
	ClinicalInvoice AS v
WHERE
	NOT EXISTS (
    SELECT 
		1 
    FROM
		Consilient.Billing.InvoiceClinical AS t
    WHERE 
        t.InvoiceID = v.InvoiceID
		)
;
----	------------------------------------------------------------------------------------------------------------------------------------
----    -- Step 4 : Create Period Based Invoice for Contractual Hospital Invoicing -> Consilient.Billing.InvoiceWeekendRevenue
----	--Need to create this as Ventura will be just contract based for first two months, then it will be included in step 3
----	--Current assumption is that most contracts will follow this pattern
----    ------------------------------------------------------------------------------------------------------------------------------------
----    ------------------------------------------------------------------------------------------------------------------------------------
----    -- Step 5 : Create Weekend Pay Portion of Invoice -> Consilient.Billing.InvoiceWeekendRevenue
----    ------------------------------------------------------------------------------------------------------------------------------------
WITH Visits AS(
SELECT
	ServiceDTS
	,AttendingPhysicianNM
	,FacilityID
	,PatientNM
	,CASE WHEN MidlevelNM IS NULL THEN 0 ELSE 1 END AS SupervisingFLG
FROM
	Consilient.Billing.Visits
),PhysicianWeekend AS(
SELECT DISTINCT
	DATEADD(DAY, - (DATEPART(WEEKDAY, b.ServiceDTS) + @@DATEFIRST - 7) % 7, b.ServiceDTS) AS WeekendStartDTS
	,b.AttendingPhysicianNM
	,SupervisingFLG
FROM
	Visits AS b
WHERE
	DATEPART(WEEKDAY, b.ServiceDTS) IN (1,7)
	AND b.FacilityID = 1 --Only contracted for this at Santa Rosa
)
,PhysicianWeekendVisitCountPerDayPre AS(
SELECT
	DATEADD(DAY, - (DATEPART(WEEKDAY, b.ServiceDTS) + @@DATEFIRST - 7) % 7, b.ServiceDTS) AS WeekendStartDTS
	,b.ServiceDTS
	,CASE WHEN DATEPART(WEEKDAY, b.ServiceDTS) = 1 THEN 'Sunday' ELSE 'Saturday' END AS DayOfWeekendNM
	,b.AttendingPhysicianNM
	,b.SupervisingFLG
	,COUNT(b.PatientNM) AS VisitCNT
FROM
	Visits AS b
WHERE
	DATEPART(WEEKDAY, b.ServiceDTS) IN (1,7)
	AND b.FacilityID = 1 --Only contracted for this at Santa Rosa
GROUP BY
	DATEADD(DAY, - (DATEPART(WEEKDAY, b.ServiceDTS) + @@DATEFIRST - 7) % 7, b.ServiceDTS)
	,b.ServiceDTS
	,b.AttendingPhysicianNM
	,b.SupervisingFLG
)
,PhysicianWeekendVisitCountPerDay AS(
SELECT DISTINCT
	w.WeekendStartDTS
	,w.AttendingPhysicianNM
	,sats.VisitCNT AS SaturdaySupervisngVisitCNT
	,sato.VisitCNT AS SaturdayOwnVisitCNT
	,suns.VisitCNT AS SundaySupervisingVisitCNT
	,suno.VisitCNT AS SundayOwnVisitCNT
	,CASE WHEN (sats.VisitCNT IS NOT NULL OR sato.VisitCNT IS NOT NULL) AND (suns.VisitCNT IS NOT NULL OR suno.VisitCNT IS NOT NULL) THEN 1 ELSE 0 END AS WorkedBothDaysFLG
	
FROM
	PhysicianWeekend AS w

	LEFT JOIN PhysicianWeekendVisitCountPerDayPre AS sats
		ON w.WeekendStartDTS = sats.WeekendStartDTS
			AND w.AttendingPhysicianNM = sats.AttendingPhysicianNM
			AND sats.DayOfWeekendNM = 'Saturday'
			AND sats.SupervisingFLG = 1

	LEFT JOIN PhysicianWeekendVisitCountPerDayPre AS sato
		ON w.WeekendStartDTS = sato.WeekendStartDTS
			AND w.AttendingPhysicianNM = sato.AttendingPhysicianNM
			AND sato.DayOfWeekendNM = 'Saturday'
			AND sato.SupervisingFLG = 0

	LEFT JOIN PhysicianWeekendVisitCountPerDayPre AS suns
		ON w.WeekendStartDTS = suns.WeekendStartDTS
			AND w.AttendingPhysicianNM = suns.AttendingPhysicianNM
			AND suns.DayOfWeekendNM = 'Sunday'
			AND suns.SupervisingFLG = 1

	LEFT JOIN PhysicianWeekendVisitCountPerDayPre AS suno
		ON w.WeekendStartDTS = suno.WeekendStartDTS
			AND w.AttendingPhysicianNM = suno.AttendingPhysicianNM
			AND suno.DayOfWeekendNM = 'Sunday'
			AND suno.SupervisingFLG = 0
)
,PhysicianWeekendPay AS(
SELECT
	WeekendStartDTS
	,AttendingPhysicianNM
	,SaturdayOwnVisitCNT
	,SundayOwnVisitCNT
	,WorkedBothDaysFLG
	,1500.00 AS RevenueAMT
FROM
	PhysicianWeekendVisitCountPerDay
WHERE
	(SaturdayOwnVisitCNT >= 12 OR SundayOwnVisitCNT >= 12 ) --Threshold to qualify for billing per Tyler as of 10/31/25
	AND WorkedBothDaysFLG = 1 --Must work both days of weekend, supervising or not
)
,MidlevelPre AS(
SELECT DISTINCT
	DATEADD(DAY, - (DATEPART(WEEKDAY, b.ServiceDTS) + @@DATEFIRST - 7) % 7, b.ServiceDTS) AS WeekendStartDTS
	,b.ServiceDTS
	,CASE WHEN DATEPART(WEEKDAY, b.ServiceDTS) = 1 THEN 'Sunday' ELSE 'Saturday' END AS DayOfWeekendNM
	,b.MidlevelNM
	
FROM
	Consilient.Billing.Visits AS b
WHERE
	DATEPART(WEEKDAY, b.ServiceDTS) IN (1,7)
	AND b.FacilityID = 1 --Only contracted for this at Santa Rosa
	AND MidlevelNM IS NOT NULL
)
,MidlevelPre2 AS(
SELECT DISTINCT
	m.WeekendStartDTS
	,m.MidlevelNM
	,CASE WHEN sat.MidlevelNM IS NULL THEN 0 ELSE 1 END AS WorkedSatrudayFLG
	,CASE WHEN sun.MidlevelNM IS NULL THEN 0 ELSE 1 END AS WorkedSundayFLG

FROM
	MidlevelPre AS m

	LEFT JOIN MidlevelPre AS sat
		ON m.WeekendStartDTS = sat.WeekendStartDTS
			AND sat.DayOfWeekendNM = 'Saturday'

	LEFT JOIN MidlevelPre AS sun
		ON m.WeekendStartDTS = sun.WeekendStartDTS
			AND sun.DayOfWeekendNM = 'Sunday'
)
,MidlevelPay AS(
SELECT
	WeekendStartDTS
	,MidlevelNM
	,750 AS RevenueAMT
FROM
	MidlevelPre2
WHERE
	WorkedSatrudayFLG = 1
	AND WorkedSundayFLG = 1
)
,WeekendPayFinal AS(
SELECT
	WeekendStartDTS
	,AttendingPhysicianNM AS ProviderNM
	,2 AS DaysWorkedOnWeekendCNT
	,RevenueAMT AS InvoiceAMT
	,CONVERT(DATE,GetDate()) AS InvoiceEligibilityDTSInvoiceEligibilityDTS
FROM
	PhysicianWeekendPay

UNION ALL

SELECT
	WeekendStartDTS
	,MidlevelNM AS ProviderNM
	,2 AS DaysWorkedOnWeekendCNT
	,RevenueAMT AS InvoiceAMT
	,CONVERT(DATE,GetDate()) AS InvoiceEligibilityDTSInvoiceEligibilityDTS

FROM
	MidlevelPay
)
INSERT INTO Consilient.Billing.InvoiceWeekendRevenue(
	WeekendStartDTS
	,ProviderNM
	,DaysWorkedOnWeekendCNT
	,InvoiceAMT
	,InvoiceEligibilityDTS
	)
SELECT
	i.WeekendStartDTS
	,ProviderNM
	,DaysWorkedOnWeekendCNT
	,InvoiceAMT
	,CONVERT(DATE,GetDate()) AS InvoiceEligibilityDTS
FROM
	WeekendPayFinal AS i
WHERE
	NOT EXISTS (
    SELECT 
		1 
    FROM
		Consilient.Billing.InvoiceWeekendRevenue AS t
    WHERE 
        t.WeekendStartDTS = i.WeekendStartDTS
		AND t.ProviderNM = i.ProviderNM
		)
----	------------------------------------------------------------------------------------------------------------------------------------
----    -- Step 6 : Create Unpaid Visit Aging List Where patient demo/insurance isn't yet available on 
----	-- Jasper Report. -> Consilient.Billing.UnpaidVisitAging
----    ------------------------------------------------------------------------------------------------------------------------------------
TRUNCATE TABLE Consilient.Billing.AgingVisits;

INSERT INTO Consilient.Billing.AgingVisits(
	AgingID
	,CaseID
	,ServiceDTS
	,PatientNM
	,MRN
	,CPTCD
	,AttendingPhysicianNM
	,MidlevelNM
	,RenderingProviderNM
	,RenderingLocationNM
	,PendingReasonDSC
)
SELECT
	VisitForPaymentID AS AgingID
	,CaseID
	,ServiceDTS
	,PatientNM
	,MRN
	,CPTCD
	,AttendingPhysicianNM
	,MidlevelNM
	,RenderingProviderNM
	,RenderingLocationNM
	,'Jasper Report' AS PendingReasonDSC
FROM
	Billing.Visits
WHERE
	PresentOnJasperReportFLG = 0

UNION ALL

SELECT
	VisitForPaymentID AS AgingID
	,CaseID
	,ServiceDTS
	,PatientNM
	,MRN
	,CPTCD
	,AttendingPhysicianNM
	,MidlevelNM
	,RenderingProviderNM
	,RenderingLocationNM
	,'Credentialing' AS PendingReasonDSC
FROM
	Billing.Visits
WHERE
	PresentOnJasperReportFLG = 1
	AND RenderingProviderNM IS NULL
----	------------------------------------------------------------------------------------------------------------------------------------
----    -- Step  : Create On Call Invoice for Santa Rosa
----    ------------------------------------------------------------------------------------------------------------------------------------
INSERT INTO Consilient.Billing.OnCallRevenue(
	ServiceStartDTS
	,ProviderNM
	,HourCNT
	,RateAMT
	,InvoiceAMT
	,InvoiceEligibilityDTS
)
SELECT
	o.OvernightOnCallStartDTS AS ServiceStartDTS
	,md.FullName AS ProviderNM
	,10 AS HourCNT
	,60.00 AS RateAMT
	,600.00 AS InvoiceAMT
	,CONVERT(DATE,GETDATE()) AS InvoiceEligibilityDTS
FROM
	Consilient.Clinical.InternalMedicineOvernightOnCall AS o

	INNER JOIN Consilient.Compensation.Employees AS md
		ON o.ProviderID = md.EmployeeID
WHERE
	NOT EXISTS (
    SELECT 
		1 
    FROM
		Consilient.Billing.OnCallRevenue AS t
    WHERE 
        t.ServiceStartDTS = o.OvernightOnCallStartDTS
		AND t.ProviderNM = md.FullName
		)
END;
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TYPE dbo.Assignments AS TABLE
(
    CaseId               NVARCHAR(50)    NOT NULL,
    FirstName            NVARCHAR(50)    NULL,
    LastName             NVARCHAR(50)    NULL,
    Mrn                  NVARCHAR(50)    NOT NULL,
    Sex                  NVARCHAR(10)    NULL,
    Dob                  DATE            NULL,
    DateServiced         DATE            NOT NULL,
    Room                 NVARCHAR(50)    NOT NULL,
    Bed                  NVARCHAR(50)    NOT NULL,
    Doa                  DATETIME2       NULL,
    Los                  INT             NOT NULL,
    AttendingPhysician   NVARCHAR(200)   NOT NULL,
    PrimaryInsurance     NVARCHAR(200)   NOT NULL,
    AdmDx                NVARCHAR(500)   NOT NULL,
    FacilityId           INT             NOT NULL
);
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Stored procedure that merges rows from the table-valued parameter into dbo.Patients
CREATE OR ALTER PROCEDURE dbo.ImportAssignments
    @Rows dbo.PatientDataType READONLY
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRAN;

            -- Validate that all FacilityIds exist in the Facilities table
            IF EXISTS (
                SELECT 1
                FROM @Rows r
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM [Clinical].[Facilities] f
                    WHERE f.FacilityID = r.FacilityId
                )
            )
            BEGIN
                DECLARE @InvalidFacilities NVARCHAR(MAX);
                DECLARE @ErrorMessage NVARCHAR(MAX);
                
                SELECT @InvalidFacilities = STRING_AGG(CAST(r.FacilityId AS NVARCHAR(10)), ', ')
                FROM (
                    SELECT DISTINCT r.FacilityId
                    FROM @Rows r
                    WHERE NOT EXISTS (
                        SELECT 1
                        FROM [Clinical].[Facilities] f
                        WHERE f.FacilityID = r.FacilityId
                    )
                ) r;
                
                SET @ErrorMessage = 'One or more FacilityIds do not exist in the Facilities table: ' + @InvalidFacilities;
                THROW 50001, @ErrorMessage, 1;
            END;

            -- Insert missing patients (where MRN doesn't exist in Patients table)
            INSERT INTO [Clinical].[Patients] (
                [PatientMRN],
                [PatientFirstName],
                [PatientLastName],
                [PatientBirthDate]
            )
            SELECT DISTINCT
                CAST(r.Mrn AS INT) AS PatientMRN,
                r.FirstName AS PatientFirstName,
                r.LastName AS PatientLastName,
                CAST(r.Dob AS DATE) AS PatientBirthDate
            FROM @Rows r
            WHERE NOT EXISTS (
                SELECT 1 
                FROM [Clinical].[Patients] p 
                WHERE p.PatientMRN = CAST(r.Mrn AS INT)
            )
            AND ISNUMERIC(r.Mrn) = 1;  -- Ensure MRN is numeric before casting

            -- Insert missing hospitalizations (where CaseId doesn't exist in Hospitalizations table)
            INSERT INTO [Clinical].[Hospitalizations] (
                [CaseId],
                [PatientId],
                [FacilityId],
                [AdmissionDate],
                [DischargeDate]
            )
            SELECT DISTINCT
                r.CaseId,
                p.PatientID,
                r.FacilityId,
                CAST(r.Doa AS DATE) AS AdmissionDate,
                NULL AS DischargeDate
            FROM @Rows r
            INNER JOIN [Clinical].[Patients] p ON p.PatientMRN = CAST(r.Mrn AS INT)
            WHERE NOT EXISTS (
                SELECT 1 
                FROM [Clinical].[Hospitalizations] h 
                WHERE h.CaseId = r.CaseId
            )
            AND ISNUMERIC(r.Mrn) = 1;  -- Ensure MRN is numeric before casting

            -- Insert patient visits (where combination doesn't exist)
            INSERT INTO [Clinical].[PatientVisits] (
                [HospitalizationID],
                [DateServiced],
                [ServiceTypeID],
                [PhysicianEmployeeID],
                [InsuranceID],
                [NursePractitionerEmployeeID],
                [CosigningPhysicianEmployeeID],
                [ScribeEmployeeID]
            )
            SELECT DISTINCT
                h.Id AS HospitalizationID,
                CAST(ISNULL(r.DateServiced, r.Doa) AS DATE) AS DateServiced,
                r.ServiceTypeId,
                r.PhysicianEmployeeId,
                r.InsuranceId,
                NULL AS NursePractitionerEmployeeID,
                NULL AS CosigningPhysicianEmployeeID,
                NULL AS ScribeEmployeeID
            FROM @Rows r
            INNER JOIN [Clinical].[Patients] p ON p.PatientMRN = CAST(r.Mrn AS INT)
            INNER JOIN [Clinical].[Hospitalizations] h ON h.CaseId = r.CaseId AND h.PatientId = p.PatientID
            WHERE r.ServiceTypeId IS NOT NULL  -- Only insert if we have service type
                AND r.PhysicianEmployeeId IS NOT NULL  -- Only insert if we have physician
                AND NOT EXISTS (
                    SELECT 1 
                    FROM [Clinical].[PatientVisits] v 
                    WHERE v.HospitalizationID = h.Id
                        AND v.DateServiced = CAST(ISNULL(r.DateServiced, r.Doa) AS DATE)
                        AND v.ServiceTypeID = r.ServiceTypeId
                        AND v.PhysicianEmployeeID = r.PhysicianEmployeeId
                )
            AND ISNUMERIC(r.Mrn) = 1;
        COMMIT TRAN;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRAN;
        THROW;
    END CATCH
END;
GO
*/
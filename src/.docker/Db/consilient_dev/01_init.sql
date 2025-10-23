/****** Object:  DatabaseRole [compensation_full]    Script Date: 10/23/2025 6:44:53 AM ******/
CREATE ROLE [compensation_full]
GO
/****** Object:  DatabaseRole [clinical_full]    Script Date: 10/23/2025 6:44:53 AM ******/
CREATE ROLE [clinical_full]
GO
/****** Object:  DatabaseRole [admin_full]    Script Date: 10/23/2025 6:44:53 AM ******/
CREATE ROLE [admin_full]
GO
/****** Object:  Schema [Billing]    Script Date: 10/23/2025 6:44:54 AM ******/
CREATE SCHEMA [Billing]
GO
/****** Object:  Schema [Clinical]    Script Date: 10/23/2025 6:44:54 AM ******/
CREATE SCHEMA [Clinical]
GO
/****** Object:  Schema [Compensation]    Script Date: 10/23/2025 6:44:54 AM ******/
CREATE SCHEMA [Compensation]
GO

/****** Object:  Table [Clinical].[Insurances]    Script Date: 10/23/2025 6:44:54 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Clinical].[Insurances](
	[InsuranceID] [int] IDENTITY(1,1) NOT NULL,
	[InsuranceCode] [nvarchar](10) NULL,
	[InsuranceDescription] [nvarchar](100) NULL,
	[PhysicianIncluded] [bit] NULL,
	[IsContracted] [bit] NULL,
	[CodeAndDescription]  AS ((isnull([InsuranceCode],'')+' - ')+isnull([InsuranceDescription],'')),
 CONSTRAINT [PK_Insurances] PRIMARY KEY CLUSTERED 
(
	[InsuranceID] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [Clinical].[PatientVisits_Staging]    Script Date: 10/23/2025 6:44:54 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Clinical].[PatientVisits_Staging](
	[PatientVisit_StagingID] [int] IDENTITY(1,1) NOT NULL,
	[DateServiced] [date] NOT NULL,
	[PatientID] [int] NOT NULL,
	[FacilityID] [int] NOT NULL,
	[AdmissionNumber] [int] NULL,
	[InsuranceID] [int] NULL,
	[ServiceTypeID] [int] NULL,
	[PhysicianEmployeeID] [int] NOT NULL,
	[NursePractitionerEmployeeID] [int] NULL,
	[ScribeEmployeeID] [int] NULL,
	[NursePractitionerApproved] [bit] NOT NULL,
	[PhysicianApproved] [bit] NOT NULL,
	[PhysicianApprovedBy] [nvarchar](100) NULL,
	[PhysicianApprovedDateTime] [datetime] NULL,
	[AddedToMainTable] [bit] NOT NULL,
	[CosigningPhysicianEmployeeID] [int] NULL,
	[IsScribeServiceOnly] [bit] NOT NULL,
 CONSTRAINT [PK_PatientVisits_Staging] PRIMARY KEY CLUSTERED 
(
	[PatientVisit_StagingID] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [Compensation].[Employees]    Script Date: 10/23/2025 6:44:54 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Compensation].[Employees](
	[EmployeeID] [int] IDENTITY(1,1) NOT NULL,
	[FirstName] [nvarchar](50) NULL,
	[LastName] [nvarchar](50) NULL,
	[TitleExtension] [nvarchar](2) NULL,
	[IsProvider] [bit] NOT NULL,
	[Role] [nvarchar](50) NULL,
	[FullName]  AS (case when [TitleExtension] IS NULL then (isnull([FirstName],'')+' ')+isnull([LastName],'') else (((isnull([FirstName],'')+' ')+isnull([LastName],''))+', ')+isnull([TitleExtension],'') end),
	[IsAdministrator] [bit] NOT NULL,
	[Email] [nvarchar](100) NULL,
	[CanApproveVisits] [bit] NOT NULL,
 CONSTRAINT [PK_Employees] PRIMARY KEY CLUSTERED 
(
	[EmployeeID] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [Clinical].[Patients]    Script Date: 10/23/2025 6:44:54 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Clinical].[Patients](
	[PatientID] [int] IDENTITY(1,1) NOT NULL,
	[PatientMRN] [int] NOT NULL,
	[PatientFirstName] [nvarchar](50) NULL,
	[PatientLastName] [nvarchar](50) NULL,
	[PatientBirthDate] [date] NULL,
	[PatientFullName]  AS ((isnull([PatientFirstName],'')+' ')+isnull([PatientLastName],'')),
 CONSTRAINT [PK_Patients] PRIMARY KEY CLUSTERED 
(
	[PatientID] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [Clinical].[Facilities]    Script Date: 10/23/2025 6:44:54 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Clinical].[Facilities](
	[FacilityID] [int] IDENTITY(1,1) NOT NULL,
	[FacilityName] [nvarchar](100) NULL,
	[FacilityAbbreviation] [nvarchar](10) NULL,
 CONSTRAINT [PK_Facilities] PRIMARY KEY CLUSTERED 
(
	[FacilityID] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [Clinical].[ServiceTypes]    Script Date: 10/23/2025 6:44:54 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Clinical].[ServiceTypes](
	[ServiceTypeID] [int] IDENTITY(1,1) NOT NULL,
	[Description] [nvarchar](100) NULL,
	[CPTCode] [int] NULL,
	[CodeAndDescription]  AS ((isnull(CONVERT([nvarchar],[CPTCode]),'')+' - ')+isnull([Description],'')),
 CONSTRAINT [PK_ServiceTypes] PRIMARY KEY CLUSTERED 
(
	[ServiceTypeID] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [Clinical].[vw_PatientVisits_Staging]    Script Date: 10/23/2025 6:44:54 AM ******/
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
	ST.CodeAndDescription AS ServiceType,
	PE.FullName AS Physician,
	NPE.FullName AS NursePractitioner,
	SE.FullName AS Scribe
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
/****** Object:  Table [Clinical].[PatientVisits]    Script Date: 10/23/2025 6:44:54 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Clinical].[PatientVisits](
	[PatientVisitID] [int] IDENTITY(1,1) NOT NULL,
	[DateServiced] [date] NOT NULL,
	[PatientID] [int] NOT NULL,
	[FacilityID] [int] NOT NULL,
	[AdmissionNumber] [int] NULL,
	[InsuranceID] [int] NULL,
	[ServiceTypeID] [int] NOT NULL,
	[PhysicianEmployeeID] [int] NOT NULL,
	[NursePractitionerEmployeeID] [int] NULL,
	[IsSupervising]  AS (case when [NursePractitionerEmployeeID] IS NULL then (0) else (1) end),
	[ScribeEmployeeID] [int] NULL,
	[CosigningPhysicianEmployeeID] [int] NULL,
	[IsScribeServiceOnly] [bit] NOT NULL,
 CONSTRAINT [PK_PatientVisits] PRIMARY KEY CLUSTERED 
(
	[PatientVisitID] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  View [Clinical].[vw_PatientVisits]    Script Date: 10/23/2025 6:44:54 AM ******/
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
	F.FacilityName,
	I.CodeAndDescription AS Insurance,
	ST.CodeAndDescription AS ServiceType,
	PE.FullName AS Physician,
	NPE.FullName AS NursePractitioner,
	SE.FullName AS Scribe
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
/****** Object:  Table [Compensation].[Contracts]    Script Date: 10/23/2025 6:44:54 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Compensation].[Contracts](
	[ContractID] [int] IDENTITY(1,1) NOT NULL,
	[ContractName] [nvarchar](100) NULL,
	[EmployeeID] [int] NOT NULL,
	[FacilityID] [int] NOT NULL,
	[ServiceTypeID] [int] NOT NULL,
	[PayType] [nvarchar](20) NOT NULL,
	[WeekendFlag] [bit] NOT NULL,
	[SupervisingFlag] [bit] NOT NULL,
	[Amount] [decimal](18, 2) NULL,
 CONSTRAINT [PK_Contracts] PRIMARY KEY CLUSTERED 
(
	[ContractID] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [Compensation].[FacilityPay]    Script Date: 10/23/2025 6:44:54 AM ******/
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
/****** Object:  Table [Compensation].[PayrollData]    Script Date: 10/23/2025 6:44:54 AM ******/
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
/****** Object:  Table [Compensation].[PayrollPeriods]    Script Date: 10/23/2025 6:44:54 AM ******/
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
/****** Object:  Table [Compensation].[ProviderContracts]    Script Date: 10/23/2025 6:44:54 AM ******/
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
/****** Object:  Table [Compensation].[ProviderPay]    Script Date: 10/23/2025 6:44:54 AM ******/
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






ALTER TABLE [Clinical].[Insurances] ADD  DEFAULT ((0)) FOR [PhysicianIncluded]
GO
ALTER TABLE [Clinical].[Insurances] ADD  DEFAULT ((0)) FOR [IsContracted]
GO
ALTER TABLE [Clinical].[PatientVisits] ADD  DEFAULT ((0)) FOR [IsScribeServiceOnly]
GO
ALTER TABLE [Clinical].[PatientVisits_Staging] ADD  DEFAULT ((0)) FOR [NursePractitionerApproved]
GO
ALTER TABLE [Clinical].[PatientVisits_Staging] ADD  DEFAULT ((0)) FOR [PhysicianApproved]
GO
ALTER TABLE [Clinical].[PatientVisits_Staging] ADD  DEFAULT ((0)) FOR [AddedToMainTable]
GO
ALTER TABLE [Clinical].[PatientVisits_Staging] ADD  DEFAULT ((0)) FOR [IsScribeServiceOnly]
GO
ALTER TABLE [Compensation].[Contracts] ADD  DEFAULT ((0)) FOR [WeekendFlag]
GO
ALTER TABLE [Compensation].[Contracts] ADD  DEFAULT ((0)) FOR [SupervisingFlag]
GO
ALTER TABLE [Compensation].[Employees] ADD  DEFAULT ((0)) FOR [IsProvider]
GO
ALTER TABLE [Compensation].[Employees] ADD  DEFAULT ((0)) FOR [IsAdministrator]
GO
ALTER TABLE [Compensation].[Employees] ADD  DEFAULT ((0)) FOR [CanApproveVisits]
GO
ALTER TABLE [Compensation].[PayrollData] ADD  DEFAULT ((0)) FOR [Count]
GO
ALTER TABLE [Clinical].[PatientVisits]  WITH CHECK ADD  CONSTRAINT [FK_PatientVisits_CosignPhysicianEmployee] FOREIGN KEY([CosigningPhysicianEmployeeID])
REFERENCES [Compensation].[Employees] ([EmployeeID])
GO
ALTER TABLE [Clinical].[PatientVisits] CHECK CONSTRAINT [FK_PatientVisits_CosignPhysicianEmployee]
GO
ALTER TABLE [Clinical].[PatientVisits]  WITH CHECK ADD  CONSTRAINT [FK_PatientVisits_Facility] FOREIGN KEY([FacilityID])
REFERENCES [Clinical].[Facilities] ([FacilityID])
GO
ALTER TABLE [Clinical].[PatientVisits] CHECK CONSTRAINT [FK_PatientVisits_Facility]
GO
ALTER TABLE [Clinical].[PatientVisits]  WITH CHECK ADD  CONSTRAINT [FK_PatientVisits_Insurances] FOREIGN KEY([InsuranceID])
REFERENCES [Clinical].[Insurances] ([InsuranceID])
GO
ALTER TABLE [Clinical].[PatientVisits] CHECK CONSTRAINT [FK_PatientVisits_Insurances]
GO
ALTER TABLE [Clinical].[PatientVisits]  WITH CHECK ADD  CONSTRAINT [FK_PatientVisits_NursePractitioner] FOREIGN KEY([NursePractitionerEmployeeID])
REFERENCES [Compensation].[Employees] ([EmployeeID])
GO
ALTER TABLE [Clinical].[PatientVisits] CHECK CONSTRAINT [FK_PatientVisits_NursePractitioner]
GO
ALTER TABLE [Clinical].[PatientVisits]  WITH CHECK ADD  CONSTRAINT [FK_PatientVisits_Patient] FOREIGN KEY([PatientID])
REFERENCES [Clinical].[Patients] ([PatientID])
GO
ALTER TABLE [Clinical].[PatientVisits] CHECK CONSTRAINT [FK_PatientVisits_Patient]
GO
ALTER TABLE [Clinical].[PatientVisits]  WITH CHECK ADD  CONSTRAINT [FK_PatientVisits_Physician] FOREIGN KEY([PhysicianEmployeeID])
REFERENCES [Compensation].[Employees] ([EmployeeID])
GO
ALTER TABLE [Clinical].[PatientVisits] CHECK CONSTRAINT [FK_PatientVisits_Physician]
GO
ALTER TABLE [Clinical].[PatientVisits]  WITH CHECK ADD  CONSTRAINT [FK_PatientVisits_Scribe] FOREIGN KEY([ScribeEmployeeID])
REFERENCES [Compensation].[Employees] ([EmployeeID])
GO
ALTER TABLE [Clinical].[PatientVisits] CHECK CONSTRAINT [FK_PatientVisits_Scribe]
GO
ALTER TABLE [Clinical].[PatientVisits]  WITH CHECK ADD  CONSTRAINT [FK_PatientVisits_ServiceType] FOREIGN KEY([ServiceTypeID])
REFERENCES [Clinical].[ServiceTypes] ([ServiceTypeID])
GO
ALTER TABLE [Clinical].[PatientVisits] CHECK CONSTRAINT [FK_PatientVisits_ServiceType]
GO
ALTER TABLE [Clinical].[PatientVisits_Staging]  WITH CHECK ADD  CONSTRAINT [FK_PatientVisits_Staging_CosignPhysicianEmployee] FOREIGN KEY([CosigningPhysicianEmployeeID])
REFERENCES [Compensation].[Employees] ([EmployeeID])
GO
ALTER TABLE [Clinical].[PatientVisits_Staging] CHECK CONSTRAINT [FK_PatientVisits_Staging_CosignPhysicianEmployee]
GO
ALTER TABLE [Clinical].[PatientVisits_Staging]  WITH CHECK ADD  CONSTRAINT [FK_PatientVisits_Staging_Facility] FOREIGN KEY([FacilityID])
REFERENCES [Clinical].[Facilities] ([FacilityID])
GO
ALTER TABLE [Clinical].[PatientVisits_Staging] CHECK CONSTRAINT [FK_PatientVisits_Staging_Facility]
GO
ALTER TABLE [Clinical].[PatientVisits_Staging]  WITH CHECK ADD  CONSTRAINT [FK_PatientVisits_Staging_Insurance] FOREIGN KEY([InsuranceID])
REFERENCES [Clinical].[Insurances] ([InsuranceID])
GO
ALTER TABLE [Clinical].[PatientVisits_Staging] CHECK CONSTRAINT [FK_PatientVisits_Staging_Insurance]
GO
ALTER TABLE [Clinical].[PatientVisits_Staging]  WITH CHECK ADD  CONSTRAINT [FK_PatientVisits_Staging_NursePractitioner] FOREIGN KEY([NursePractitionerEmployeeID])
REFERENCES [Compensation].[Employees] ([EmployeeID])
GO
ALTER TABLE [Clinical].[PatientVisits_Staging] CHECK CONSTRAINT [FK_PatientVisits_Staging_NursePractitioner]
GO
ALTER TABLE [Clinical].[PatientVisits_Staging]  WITH CHECK ADD  CONSTRAINT [FK_PatientVisits_Staging_Patient] FOREIGN KEY([PatientID])
REFERENCES [Clinical].[Patients] ([PatientID])
GO
ALTER TABLE [Clinical].[PatientVisits_Staging] CHECK CONSTRAINT [FK_PatientVisits_Staging_Patient]
GO
ALTER TABLE [Clinical].[PatientVisits_Staging]  WITH CHECK ADD  CONSTRAINT [FK_PatientVisits_Staging_Physician] FOREIGN KEY([PhysicianEmployeeID])
REFERENCES [Compensation].[Employees] ([EmployeeID])
GO
ALTER TABLE [Clinical].[PatientVisits_Staging] CHECK CONSTRAINT [FK_PatientVisits_Staging_Physician]
GO
ALTER TABLE [Clinical].[PatientVisits_Staging]  WITH CHECK ADD  CONSTRAINT [FK_PatientVisits_Staging_Scribe] FOREIGN KEY([ScribeEmployeeID])
REFERENCES [Compensation].[Employees] ([EmployeeID])
GO
ALTER TABLE [Clinical].[PatientVisits_Staging] CHECK CONSTRAINT [FK_PatientVisits_Staging_Scribe]
GO
ALTER TABLE [Clinical].[PatientVisits_Staging]  WITH CHECK ADD  CONSTRAINT [FK_PatientVisits_Staging_ServiceType] FOREIGN KEY([ServiceTypeID])
REFERENCES [Clinical].[ServiceTypes] ([ServiceTypeID])
GO
ALTER TABLE [Clinical].[PatientVisits_Staging] CHECK CONSTRAINT [FK_PatientVisits_Staging_ServiceType]
GO
ALTER TABLE [Compensation].[Contracts]  WITH CHECK ADD  CONSTRAINT [FK_Contracts_Employee] FOREIGN KEY([EmployeeID])
REFERENCES [Compensation].[Employees] ([EmployeeID])
GO
ALTER TABLE [Compensation].[Contracts] CHECK CONSTRAINT [FK_Contracts_Employee]
GO
ALTER TABLE [Compensation].[Contracts]  WITH CHECK ADD  CONSTRAINT [FK_Contracts_Facility] FOREIGN KEY([FacilityID])
REFERENCES [Clinical].[Facilities] ([FacilityID])
GO
ALTER TABLE [Compensation].[Contracts] CHECK CONSTRAINT [FK_Contracts_Facility]
GO
ALTER TABLE [Compensation].[Contracts]  WITH CHECK ADD  CONSTRAINT [FK_Contracts_ServiceType] FOREIGN KEY([ServiceTypeID])
REFERENCES [Clinical].[ServiceTypes] ([ServiceTypeID])
GO
ALTER TABLE [Compensation].[Contracts] CHECK CONSTRAINT [FK_Contracts_ServiceType]
GO
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
ALTER TABLE [Compensation].[ProviderContracts]  WITH CHECK ADD  CONSTRAINT [FK_ProviderContracts_Contract] FOREIGN KEY([ContractID])
REFERENCES [Compensation].[Contracts] ([ContractID])
GO
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

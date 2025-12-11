using Microsoft.EntityFrameworkCore.Migrations;

namespace Consilient.Infrastructure.Migrations
{
    public partial class CreateDoctorAssignmentsStagingTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {   
            var sql = @"
IF NOT EXISTS (SELECT 1 FROM sys.schemas s WHERE s.name = N'staging')
BEGIN
    EXEC('CREATE SCHEMA [staging]');
END;

IF OBJECT_ID(N'staging.DoctorAssignments', N'U') IS NULL
BEGIN
    CREATE TABLE staging.DoctorAssignments
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Age INT NOT NULL,
        AttendingMD NVARCHAR(200) NOT NULL DEFAULT N'',
        HospitalNumber NVARCHAR(100) NOT NULL DEFAULT N'',
        Admit DATETIME2(7) NOT NULL,
        Dob DATETIME2(7) NULL,
        FacilityId INT NULL,
        Mrn NVARCHAR(100) NOT NULL DEFAULT N'',
        Name NVARCHAR(200) NOT NULL DEFAULT N'',
        Insurance NVARCHAR(200) NOT NULL DEFAULT N'',
        NursePractitioner NVARCHAR(200) NOT NULL DEFAULT N'',
        IsCleared NVARCHAR(50) NOT NULL DEFAULT N'',
        Location NVARCHAR(200) NOT NULL DEFAULT N'',
        ServiceDate DATE NULL,
        H_P NVARCHAR(200) NOT NULL DEFAULT N'',
        PsychEval NVARCHAR(200) NOT NULL DEFAULT N'',
        CreatedAtUTC DATETIME2(7) NOT NULL CONSTRAINT DF_Staging_DoctorAssignments_CreatedAtUTC DEFAULT SYSUTCDATETIME(),
        ResolvedProviderId INT NULL,
        ResolvedHospitalizationId INT NULL,
        ResolvedPatientId INT NULL,
        ResolvedNursePracticionerId INT NULL,
        BatchId UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Staging_DoctorAssignments_BatchId DEFAULT NEWID(),
        Imported BIT NOT NULL CONSTRAINT DF_Staging_DoctorAssignments_Imported DEFAULT (0)
    );

    CREATE NONCLUSTERED INDEX IX_Staging_DoctorAssignments_BatchId ON staging.DoctorAssignments(BatchId);
    CREATE NONCLUSTERED INDEX IX_Staging_DoctorAssignments_Mrn ON staging.DoctorAssignments(Mrn);
END;
";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'staging.DoctorAssignments', N'U') IS NOT NULL
BEGIN
    DROP TABLE staging.DoctorAssignments;
END;
");
        }
    }
}
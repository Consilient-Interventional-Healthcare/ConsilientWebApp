using Consilient.WebApp.Models;
using Microsoft.EntityFrameworkCore;

namespace Consilient.WebApp.Data;

public partial class ConsilientContext : DbContext
{
    public ConsilientContext()
    {
    }

    public ConsilientContext(DbContextOptions<ConsilientContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Contract> Contracts { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Facility> Facilities { get; set; }

    public virtual DbSet<FacilityPay> FacilityPays { get; set; }

    public virtual DbSet<Insurance> Insurances { get; set; }

    public virtual DbSet<Patient> Patients { get; set; }

    public virtual DbSet<PatientVisit> PatientVisits { get; set; }

    public virtual DbSet<PatientVisitsStaging> PatientVisitsStagings { get; set; }

    public virtual DbSet<PayrollDatum> PayrollData { get; set; }

    public virtual DbSet<PayrollPeriod> PayrollPeriods { get; set; }

    public virtual DbSet<ProviderContract> ProviderContracts { get; set; }

    public virtual DbSet<ProviderPay> ProviderPays { get; set; }

    public virtual DbSet<ServiceType> ServiceTypes { get; set; }

    public virtual DbSet<VwPatientVisit> VwPatientVisits { get; set; }

    public virtual DbSet<VwPatientVisitsCompareToLive> VwPatientVisitsCompareToLives { get; set; }

    public virtual DbSet<VwPatientVisitsStaging> VwPatientVisitsStagings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Contract>(entity =>
        {
            entity.ToTable("Contracts", "Compensation");

            entity.Property(e => e.ContractId).HasColumnName("ContractID");
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ContractName).HasMaxLength(100);
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.FacilityId).HasColumnName("FacilityID");
            entity.Property(e => e.PayType).HasMaxLength(20);
            entity.Property(e => e.ServiceTypeId).HasColumnName("ServiceTypeID");

            entity.HasOne(d => d.Employee).WithMany(p => p.Contracts)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Contracts_Employee");

            entity.HasOne(d => d.Facility).WithMany(p => p.Contracts)
                .HasForeignKey(d => d.FacilityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Contracts_Facility");

            entity.HasOne(d => d.ServiceType).WithMany(p => p.Contracts)
                .HasForeignKey(d => d.ServiceTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Contracts_ServiceType");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.ToTable("Employees", "Compensation");

            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.FullName)
                .HasMaxLength(105)
                .HasComputedColumnSql("(case when [TitleExtension] IS NULL then (isnull([FirstName],'')+' ')+isnull([LastName],'') else (((isnull([FirstName],'')+' ')+isnull([LastName],''))+', ')+isnull([TitleExtension],'') end)", false);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.Role).HasMaxLength(50);
            entity.Property(e => e.TitleExtension).HasMaxLength(2);
        });

        modelBuilder.Entity<Facility>(entity =>
        {
            entity.ToTable("Facilities", "Clinical");

            entity.Property(e => e.FacilityId).HasColumnName("FacilityID");
            entity.Property(e => e.FacilityAbbreviation).HasMaxLength(10);
            entity.Property(e => e.FacilityName).HasMaxLength(100);
        });

        modelBuilder.Entity<FacilityPay>(entity =>
        {
            entity.HasKey(e => e.FacilityPayId).HasName("PK_FacilityPays");

            entity.ToTable("FacilityPay", "Compensation");

            entity.Property(e => e.FacilityPayId).HasColumnName("FacilityPayID");
            entity.Property(e => e.FacilityId).HasColumnName("FacilityID");
            entity.Property(e => e.RevenueAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ServiceTypeId).HasColumnName("ServiceTypeID");

            entity.HasOne(d => d.Facility).WithMany(p => p.FacilityPays)
                .HasForeignKey(d => d.FacilityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FacilityPay_Facility");

            entity.HasOne(d => d.ServiceType).WithMany(p => p.FacilityPays)
                .HasForeignKey(d => d.ServiceTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FacilityPay_ServiceType");
        });

        modelBuilder.Entity<Insurance>(entity =>
        {
            entity.ToTable("Insurances", "Clinical");

            entity.Property(e => e.InsuranceId).HasColumnName("InsuranceID");
            entity.Property(e => e.CodeAndDescription)
                .HasMaxLength(113)
                .HasComputedColumnSql("((isnull([InsuranceCode],'')+' - ')+isnull([InsuranceDescription],''))", false);
            entity.Property(e => e.InsuranceCode).HasMaxLength(10);
            entity.Property(e => e.InsuranceDescription).HasMaxLength(100);
            entity.Property(e => e.IsContracted).HasDefaultValue(false);
            entity.Property(e => e.PhysicianIncluded).HasDefaultValue(false);
        });

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.ToTable("Patients", "Clinical");

            entity.Property(e => e.PatientId).HasColumnName("PatientID");
            entity.Property(e => e.PatientFirstName).HasMaxLength(50);
            entity.Property(e => e.PatientFullName)
                .HasMaxLength(101)
                .HasComputedColumnSql("((isnull([PatientFirstName],'')+' ')+isnull([PatientLastName],''))", false);
            entity.Property(e => e.PatientLastName).HasMaxLength(50);
            entity.Property(e => e.PatientMrn).HasColumnName("PatientMRN");
        });

        modelBuilder.Entity<PatientVisit>(entity =>
        {
            entity.ToTable("PatientVisits", "Clinical");

            entity.Property(e => e.PatientVisitId).HasColumnName("PatientVisitID");
            entity.Property(e => e.CosigningPhysicianEmployeeId).HasColumnName("CosigningPhysicianEmployeeID");
            entity.Property(e => e.FacilityId).HasColumnName("FacilityID");
            entity.Property(e => e.InsuranceId).HasColumnName("InsuranceID");
            entity.Property(e => e.IsSupervising).HasComputedColumnSql("(case when [NursePractitionerEmployeeID] IS NULL then (0) else (1) end)", false);
            entity.Property(e => e.NursePractitionerEmployeeId).HasColumnName("NursePractitionerEmployeeID");
            entity.Property(e => e.PatientId).HasColumnName("PatientID");
            entity.Property(e => e.PhysicianEmployeeId).HasColumnName("PhysicianEmployeeID");
            entity.Property(e => e.ScribeEmployeeId).HasColumnName("ScribeEmployeeID");
            entity.Property(e => e.ServiceTypeId).HasColumnName("ServiceTypeID");

            entity.HasOne(d => d.CosigningPhysicianEmployee).WithMany(p => p.PatientVisitCosigningPhysicianEmployees)
                .HasForeignKey(d => d.CosigningPhysicianEmployeeId)
                .HasConstraintName("FK_PatientVisits_CosignPhysicianEmployee");

            entity.HasOne(d => d.Facility).WithMany(p => p.PatientVisits)
                .HasForeignKey(d => d.FacilityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PatientVisits_Facility");

            entity.HasOne(d => d.Insurance).WithMany(p => p.PatientVisits)
                .HasForeignKey(d => d.InsuranceId)
                .HasConstraintName("FK_PatientVisits_Insurances");

            entity.HasOne(d => d.NursePractitionerEmployee).WithMany(p => p.PatientVisitNursePractitionerEmployees)
                .HasForeignKey(d => d.NursePractitionerEmployeeId)
                .HasConstraintName("FK_PatientVisits_NursePractitioner");

            entity.HasOne(d => d.Patient).WithMany(p => p.PatientVisits)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PatientVisits_Patient");

            entity.HasOne(d => d.PhysicianEmployee).WithMany(p => p.PatientVisitPhysicianEmployees)
                .HasForeignKey(d => d.PhysicianEmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PatientVisits_Physician");

            entity.HasOne(d => d.ScribeEmployee).WithMany(p => p.PatientVisitScribeEmployees)
                .HasForeignKey(d => d.ScribeEmployeeId)
                .HasConstraintName("FK_PatientVisits_Scribe");

            entity.HasOne(d => d.ServiceType).WithMany(p => p.PatientVisits)
                .HasForeignKey(d => d.ServiceTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PatientVisits_ServiceType");
        });

        modelBuilder.Entity<PatientVisitsStaging>(entity =>
        {
            entity.HasKey(e => e.PatientVisitStagingId);

            entity.ToTable("PatientVisits_Staging", "Clinical");

            entity.Property(e => e.PatientVisitStagingId).HasColumnName("PatientVisit_StagingID");
            entity.Property(e => e.CosigningPhysicianEmployeeId).HasColumnName("CosigningPhysicianEmployeeID");
            entity.Property(e => e.FacilityId).HasColumnName("FacilityID");
            entity.Property(e => e.InsuranceId).HasColumnName("InsuranceID");
            entity.Property(e => e.NursePractitionerEmployeeId).HasColumnName("NursePractitionerEmployeeID");
            entity.Property(e => e.PatientId).HasColumnName("PatientID");
            entity.Property(e => e.PhysicianApprovedBy).HasMaxLength(100);
            entity.Property(e => e.PhysicianApprovedDateTime).HasColumnType("datetime");
            entity.Property(e => e.PhysicianEmployeeId).HasColumnName("PhysicianEmployeeID");
            entity.Property(e => e.ScribeEmployeeId).HasColumnName("ScribeEmployeeID");
            entity.Property(e => e.ServiceTypeId).HasColumnName("ServiceTypeID");

            entity.HasOne(d => d.CosigningPhysicianEmployee).WithMany(p => p.PatientVisitsStagingCosigningPhysicianEmployees)
                .HasForeignKey(d => d.CosigningPhysicianEmployeeId)
                .HasConstraintName("FK_PatientVisits_Staging_CosignPhysicianEmployee");

            entity.HasOne(d => d.Facility).WithMany(p => p.PatientVisitsStagings)
                .HasForeignKey(d => d.FacilityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PatientVisits_Staging_Facility");

            entity.HasOne(d => d.Insurance).WithMany(p => p.PatientVisitsStagings)
                .HasForeignKey(d => d.InsuranceId)
                .HasConstraintName("FK_PatientVisits_Staging_Insurance");

            entity.HasOne(d => d.NursePractitionerEmployee).WithMany(p => p.PatientVisitsStagingNursePractitionerEmployees)
                .HasForeignKey(d => d.NursePractitionerEmployeeId)
                .HasConstraintName("FK_PatientVisits_Staging_NursePractitioner");

            entity.HasOne(d => d.Patient).WithMany(p => p.PatientVisitsStagings)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PatientVisits_Staging_Patient");

            entity.HasOne(d => d.PhysicianEmployee).WithMany(p => p.PatientVisitsStagingPhysicianEmployees)
                .HasForeignKey(d => d.PhysicianEmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PatientVisits_Staging_Physician");

            entity.HasOne(d => d.ScribeEmployee).WithMany(p => p.PatientVisitsStagingScribeEmployees)
                .HasForeignKey(d => d.ScribeEmployeeId)
                .HasConstraintName("FK_PatientVisits_Staging_Scribe");

            entity.HasOne(d => d.ServiceType).WithMany(p => p.PatientVisitsStagings)
                .HasForeignKey(d => d.ServiceTypeId)
                .HasConstraintName("FK_PatientVisits_Staging_ServiceType");
        });

        modelBuilder.Entity<PayrollDatum>(entity =>
        {
            entity.HasKey(e => e.PayrollDataId).HasName("PK_PayrollDatum");

            entity.ToTable("PayrollData", "Compensation");

            entity.Property(e => e.PayrollDataId).HasColumnName("PayrollDataID");
            entity.Property(e => e.PayrollPeriodId).HasColumnName("PayrollPeriodID");
            entity.Property(e => e.ProviderPayId).HasColumnName("ProviderPayID");

            entity.HasOne(d => d.PayrollPeriod).WithMany(p => p.PayrollData)
                .HasForeignKey(d => d.PayrollPeriodId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PayrollData_PayrollPeriod");

            entity.HasOne(d => d.ProviderPay).WithMany(p => p.PayrollData)
                .HasForeignKey(d => d.ProviderPayId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PayrollData_ProviderPay");
        });

        modelBuilder.Entity<PayrollPeriod>(entity =>
        {
            entity.ToTable("PayrollPeriods", "Compensation");

            entity.Property(e => e.PayrollPeriodId).HasColumnName("PayrollPeriodID");
        });

        modelBuilder.Entity<ProviderContract>(entity =>
        {
            entity.ToTable("ProviderContracts", "Compensation");

            entity.Property(e => e.ProviderContractId).HasColumnName("ProviderContractID");
            entity.Property(e => e.ContractId).HasColumnName("ContractID");
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");

            entity.HasOne(d => d.Contract).WithMany(p => p.ProviderContracts)
                .HasForeignKey(d => d.ContractId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProviderContracts_Contract");

            entity.HasOne(d => d.Employee).WithMany(p => p.ProviderContracts)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProviderContracts_Employee");
        });

        modelBuilder.Entity<ProviderPay>(entity =>
        {
            entity.ToTable("ProviderPay", "Compensation");

            entity.Property(e => e.ProviderPayId).HasColumnName("ProviderPayID");
            entity.Property(e => e.Description).HasMaxLength(100);
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.FacilityId).HasColumnName("FacilityID");
            entity.Property(e => e.PayAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.PayUnit).HasMaxLength(100);
            entity.Property(e => e.ServiceTypeId).HasColumnName("ServiceTypeID");

            entity.HasOne(d => d.Employee).WithMany(p => p.ProviderPays)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProviderPay_Employee");

            entity.HasOne(d => d.Facility).WithMany(p => p.ProviderPays)
                .HasForeignKey(d => d.FacilityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProviderPay_Facility");

            entity.HasOne(d => d.ServiceType).WithMany(p => p.ProviderPays)
                .HasForeignKey(d => d.ServiceTypeId)
                .HasConstraintName("FK_ProviderPay_ServiceType");
        });

        modelBuilder.Entity<ServiceType>(entity =>
        {
            entity.ToTable("ServiceTypes", "Clinical");

            entity.Property(e => e.ServiceTypeId).HasColumnName("ServiceTypeID");
            entity.Property(e => e.CodeAndDescription)
                .HasMaxLength(133)
                .HasComputedColumnSql("((isnull(CONVERT([nvarchar],[CPTCode]),'')+' - ')+isnull([Description],''))", false);
            entity.Property(e => e.Cptcode).HasColumnName("CPTCode");
            entity.Property(e => e.Description).HasMaxLength(100);
        });

        modelBuilder.Entity<VwPatientVisit>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_PatientVisits", "Clinical");

            entity.Property(e => e.FacilityName).HasMaxLength(100);
            entity.Property(e => e.Insurance).HasMaxLength(113);
            entity.Property(e => e.NursePractitioner).HasMaxLength(105);
            entity.Property(e => e.PatientName).HasMaxLength(101);
            entity.Property(e => e.PatientVisitId).HasColumnName("PatientVisitID");
            entity.Property(e => e.Physician).HasMaxLength(105);
            entity.Property(e => e.Scribe).HasMaxLength(105);
            entity.Property(e => e.ServiceType).HasMaxLength(133);
        });

        modelBuilder.Entity<VwPatientVisitsCompareToLive>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_PatientVisits_CompareToLive", "Clinical");

            entity.Property(e => e.AttendingPhysicianJoinId)
                .HasMaxLength(50)
                .HasColumnName("AttendingPhysicianJoinID");
            entity.Property(e => e.CaseId).HasColumnName("CaseID");
            entity.Property(e => e.CosignPhysicianJoinId)
                .HasMaxLength(50)
                .HasColumnName("CosignPhysicianJoinID");
            entity.Property(e => e.Cptcd).HasColumnName("CPTCD");
            entity.Property(e => e.ImportFileNm).HasColumnName("ImportFileNM");
            entity.Property(e => e.InsuranceNm)
                .HasMaxLength(100)
                .HasColumnName("InsuranceNM");
            entity.Property(e => e.ModifiedDts).HasColumnName("ModifiedDTS");
            entity.Property(e => e.Mrn).HasColumnName("MRN");
            entity.Property(e => e.NursePractitionerJoinId)
                .HasMaxLength(50)
                .HasColumnName("NursePractitionerJoinID");
            entity.Property(e => e.PatientBirthDts).HasColumnName("PatientBirthDTS");
            entity.Property(e => e.PatientNm)
                .HasMaxLength(101)
                .HasColumnName("PatientNM");
            entity.Property(e => e.ScribeNm)
                .HasMaxLength(50)
                .HasColumnName("ScribeNM");
            entity.Property(e => e.ServiceDts).HasColumnName("ServiceDTS");
        });

        modelBuilder.Entity<VwPatientVisitsStaging>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_PatientVisits_Staging", "Clinical");

            entity.Property(e => e.FacilityName).HasMaxLength(100);
            entity.Property(e => e.Insurance).HasMaxLength(113);
            entity.Property(e => e.NursePractitioner).HasMaxLength(105);
            entity.Property(e => e.PatientName).HasMaxLength(101);
            entity.Property(e => e.PatientVisitStagingId).HasColumnName("PatientVisit_StagingID");
            entity.Property(e => e.Physician).HasMaxLength(105);
            entity.Property(e => e.Scribe).HasMaxLength(105);
            entity.Property(e => e.ServiceType).HasMaxLength(133);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

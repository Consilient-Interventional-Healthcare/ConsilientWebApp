using Microsoft.EntityFrameworkCore;

namespace Consilient.Data;

public partial class ConsilientDbContext : DbContext
{
    public ConsilientDbContext()
    {
    }

    public ConsilientDbContext(DbContextOptions<ConsilientDbContext> options)
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
        // Apply all IEntityTypeConfiguration implementations from this assembly (Mappings folder).
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }
}

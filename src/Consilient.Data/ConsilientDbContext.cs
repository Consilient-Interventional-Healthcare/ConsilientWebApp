using Consilient.Data.Entities;
using Consilient.Data.Entities.Billing;
using Consilient.Data.Entities.Clinical;
using Consilient.Data.Entities.Compensation;
using Consilient.Data.Entities.Staging;
using Microsoft.EntityFrameworkCore;

namespace Consilient.Data;

public class ConsilientDbContext : DbContext
{
    public ConsilientDbContext()
    {
    }

    public ConsilientDbContext(DbContextOptions<ConsilientDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Employee> Employees { get; set; } = null!;

    public virtual DbSet<Facility> Facilities { get; set; } = null!;

    public virtual DbSet<Hospitalization> Hospitalizations { get; set; } = null!;

    public virtual DbSet<HospitalizationStatus> HospitalizationStatuses { get; set; } = null!;

    public virtual DbSet<Insurance> Insurances { get; set; } = null!;

    public virtual DbSet<Patient> Patients { get; set; } = null!;

    public virtual DbSet<PatientFacility> PatientFacilities { get; set; } = null!;
    public virtual DbSet<Provider> Providers { get; set; } = null!;

    public virtual DbSet<ServiceType> ServiceTypes { get; set; } = null!;

    public virtual DbSet<ProviderAssignment> StagingProviderAssignments { get; set; } = null!;

    public virtual DbSet<ProviderAssignmentBatch> StagingProviderAssignmentBatches { get; set; } = null!;

    public virtual DbSet<VisitEvent> VisitEvents { get; set; } = null!;

    public virtual DbSet<VisitEventType> VisitEventTypes { get; set; } = null!;

    public virtual DbSet<Visit> Visits { get; set; } = null!;

    public virtual DbSet<BillingCode> BillingCodes { get; set; } = null!;

    public virtual DbSet<VisitServiceBilling> VisitServiceBillings { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations from Consilient.Data.Configurations and all sub-namespaces
        // except Identity (which uses a separate DbContext)
        modelBuilder.ApplyConfigurationsFromAssembly(
            GetType().Assembly,
            type => type.Namespace != null
                && type.Namespace.StartsWith("Consilient.Data.Configurations")
                && !type.Namespace.StartsWith("Consilient.Data.Configurations.Identity"));
    }

    internal static class Schemas
    {
        public const string Clinical = "Clinical";
        public const string Compensation = "Compensation";
        public const string Billing = "Billing";
    }
}
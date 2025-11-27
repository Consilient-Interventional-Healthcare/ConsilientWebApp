using Consilient.Data.Configurations;
using Consilient.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Consilient.Data
{
    public class ConsilientDbContext : DbContext
    {
        internal static class Schemas
        {
            public const string Clinical = "Clinical";
        }

        public ConsilientDbContext()
        {
        }

        public ConsilientDbContext(DbContextOptions<ConsilientDbContext> options)
            : base(options)
        {
        }

        //public virtual DbSet<Contract> Contracts { get; set; }

        public  virtual DbSet<Employee> Employees { get; set; } = null!;

        public  virtual DbSet<Facility> Facilities { get; set; } = null!;

        //public virtual DbSet<FacilityPay> FacilityPays { get; set; }
        public  virtual DbSet<Hospitalization> Hospitalizations { get; set; } = null!;
        public  virtual DbSet<HospitalizationStatus> HospitalizationStatuses { get; set; } = null!;
        public  virtual DbSet<Insurance> Insurances { get; set; } = null!;

        public  virtual DbSet<Patient> Patients { get; set; } = null!;

        public  virtual DbSet<Visit> Visits { get; set; } = null!;

        public  virtual DbSet<VisitStaging> VisitsStaging { get; set; } = null!;

        //public virtual DbSet<PayrollDatum> PayrollData { get; set; }

        //public virtual DbSet<PayrollPeriod> PayrollPeriods { get; set; }

        //public virtual DbSet<ProviderContract> ProviderContracts { get; set; }

        //public virtual DbSet<ProviderPay> ProviderPays { get; set; }

        public virtual DbSet<ServiceType> ServiceTypes { get; set; } = null!;

        //public virtual DbSet<VwPatientVisit> VwPatientVisits { get; set; }

        //public virtual DbSet<VwPatientVisitsCompareToLive> VwPatientVisitsCompareToLives { get; set; }

        //public virtual DbSet<VwPatientVisitsStaging> VwPatientVisitsStagings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Only apply configurations in the base "Consilient.Data.Configurations" namespace.
            // This intentionally excludes sub-namespaces such as "Consilient.Data.Configurations.Identity"
            // so Identity entity configurations are not picked up by the main Consilient migration.
            modelBuilder.ApplyConfigurationsFromAssembly(
                GetType().Assembly,
                type => type.Namespace != null && type.Namespace == typeof(EmployeeConfiguration).Namespace);
        }
    }
}
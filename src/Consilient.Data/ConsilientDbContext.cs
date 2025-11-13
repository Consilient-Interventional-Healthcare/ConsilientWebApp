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

        public virtual DbSet<Employee> Employees { get; set; }

        public virtual DbSet<Facility> Facilities { get; set; }

        //public virtual DbSet<FacilityPay> FacilityPays { get; set; }
        public virtual DbSet<Hospitalization> Hospitalizations { get; set; }

        public virtual DbSet<Insurance> Insurances { get; set; }

        public virtual DbSet<Patient> Patients { get; set; }

        public virtual DbSet<Visit> Visits { get; set; }

        public virtual DbSet<VisitStaging> VisitsStaging { get; set; }

        //public virtual DbSet<PayrollDatum> PayrollData { get; set; }

        //public virtual DbSet<PayrollPeriod> PayrollPeriods { get; set; }

        //public virtual DbSet<ProviderContract> ProviderContracts { get; set; }

        //public virtual DbSet<ProviderPay> ProviderPays { get; set; }

        public virtual DbSet<ServiceType> ServiceTypes { get; set; }

        //public virtual DbSet<VwPatientVisit> VwPatientVisits { get; set; }

        //public virtual DbSet<VwPatientVisitsCompareToLive> VwPatientVisitsCompareToLives { get; set; }

        //public virtual DbSet<VwPatientVisitsStaging> VwPatientVisitsStagings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
        }
    }
}
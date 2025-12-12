using Consilient.Data.Configurations;
using Consilient.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Consilient.Data
{
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

        public virtual DbSet<ServiceType> ServiceTypes { get; set; } = null!;

        public virtual DbSet<DoctorAssignment> StagingDoctorAssignments { get; set; } = null!;

        public virtual DbSet<VisitEvent> VisitEvents { get; set; } = null!;

        public virtual DbSet<VisitEventType> VisitEventTypes { get; set; } = null!;

        public virtual DbSet<Visit> Visits { get; set; } = null!;

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

        internal static class Schemas
        {
            public const string Clinical = "Clinical";
        }
    }
}
using Consilient.Common;
using Consilient.Data.Entities;
using Consilient.Data.GraphQL.Models;
using EntityGraphQL.Schema;
using EntityGraphQL.Schema.FieldExtensions;
using Microsoft.EntityFrameworkCore;

namespace Consilient.Data.GraphQL
{
    public static class GraphQlSchemaConfigurator
    {
        public static void ConfigureSchema(SchemaProvider<ConsilientDbContext> schema)
        {
            ArgumentNullException.ThrowIfNull(schema, nameof(schema));
            AddTypes(schema);

            var query = schema.Query();
            AddToQuery(query);
        }

        private static void AddTypes(SchemaProvider<ConsilientDbContext> schema)
        {
            schema.AddEnum<ProviderType>(nameof(ProviderType), "Provider type enum");

            var visitType = schema.AddType<Visit>(ToGraphQlName(nameof(Visit)), $"{ToGraphQlName(nameof(Visit))} object");
            visitType.AddField(m => m.DateServiced, nameof(Visit.DateServiced));
            visitType.AddField(m => m.Hospitalization, nameof(Visit.Hospitalization));
            visitType.AddField(m => m.Id, nameof(Visit.Id));
            visitType.AddField(m => m.IsScribeServiceOnly, nameof(Visit.IsScribeServiceOnly));
            visitType.AddField(m => m.Room, nameof(Visit.Room));
            visitType.AddField(m => m.Bed, nameof(Visit.Bed));
            visitType.AddField("patient", m => new VisitPatient
            {
                Id = m.Hospitalization.Patient.Id,
                FirstName = m.Hospitalization.Patient.FirstName,
                LastName = m.Hospitalization.Patient.LastName,
                BirthDate = m.Hospitalization.Patient.BirthDate,
                Mrn = m.Hospitalization.Patient.PatientFacilities
                    .Where(pf => pf.FacilityId == m.Hospitalization.FacilityId)
                    .Select(pf => pf.Mrn)
                    .FirstOrDefault() ?? string.Empty
            }, "Patient with MRN at facility");
            visitType.AddField(m => m.VisitAttendants, nameof(Visit.VisitAttendants));

            var visitPatientType = schema.AddType<VisitPatient>("visitPatient", "Patient object with facility-specific MRN");
            visitPatientType.AddField(m => m.Id, nameof(VisitPatient.Id));
            visitPatientType.AddField(m => m.FirstName, nameof(VisitPatient.FirstName));
            visitPatientType.AddField(m => m.LastName, nameof(VisitPatient.LastName));
            visitPatientType.AddField(m => m.BirthDate, nameof(VisitPatient.BirthDate));
            visitPatientType.AddField(m => m.Mrn, nameof(VisitPatient.Mrn));

            var patientType = schema.AddType<Patient>(ToGraphQlName(nameof(Patient)), $"{ToGraphQlName(nameof(Patient))} object");
            patientType.AddField(m => m.BirthDate, nameof(Patient.BirthDate));
            patientType.AddField(m => m.FirstName, nameof(Patient.FirstName));
            patientType.AddField(m => m.Id, nameof(Patient.Id));
            patientType.AddField(m => m.LastName, nameof(Patient.LastName));

            //var employeeType = schema.AddType<Employee>(ToGraphQlName(nameof(Employee)), $"{ToGraphQlName(nameof(Employee))} object");
            //employeeType.AddField(m => m.Email, nameof(Employee.Email));
            //employeeType.AddField(m => m.Id, nameof(Employee.Id));
            //employeeType.AddField(m => m.FirstName, nameof(Employee.FirstName));
            //employeeType.AddField(m => m.LastName, nameof(Employee.LastName));
            //employeeType.AddField(m => m.Role, nameof(Employee.Role));

            var facilityType = schema.AddType<Facility>(ToGraphQlName(nameof(Facility)), $"{ToGraphQlName(nameof(Facility))} object");
            facilityType.AddField(m => m.Abbreviation, nameof(Facility.Abbreviation));
            facilityType.AddField(m => m.Id, nameof(Facility.Id));
            facilityType.AddField(m => m.Name, nameof(Facility.Name));

            var insuranceType = schema.AddType<Insurance>(ToGraphQlName(nameof(Insurance)), $"{ToGraphQlName(nameof(Insurance))} object");
            insuranceType.AddField(m => m.Code, nameof(Insurance.Code));
            insuranceType.AddField(m => m.Description, nameof(Insurance.Description));
            insuranceType.AddField(m => m.Id, nameof(Insurance.Id));

            var serviceTypeType = schema.AddType<ServiceType>(ToGraphQlName(nameof(ServiceType)), $"{ToGraphQlName(nameof(ServiceType))} object");
            serviceTypeType.AddField(m => m.Cptcode, nameof(ServiceType.Cptcode));
            serviceTypeType.AddField(m => m.Id, nameof(ServiceType.Id));
            serviceTypeType.AddField(m => m.Description, nameof(ServiceType.Description));

            var providerType = schema.AddType<Provider>(ToGraphQlName(nameof(Provider)), $"{ToGraphQlName(nameof(Provider))} object");
            providerType.AddField(m => m.Id, nameof(Provider.Id));
            providerType.AddField(m => m.FirstName, nameof(Provider.FirstName));
            providerType.AddField(m => m.LastName, nameof(Provider.LastName));
            providerType.AddField(m => m.TitleExtension, nameof(Provider.TitleExtension));
            providerType.AddField(m => m.Type, nameof(Provider.Type));
            providerType.AddField(m => m.Email, nameof(Provider.Email));
            providerType.AddField(m => m.EmployeeId, nameof(Provider.EmployeeId));

            var visitAttendantType = schema.AddType<VisitAttendant>(ToGraphQlName(nameof(VisitAttendant)), $"{ToGraphQlName(nameof(VisitAttendant))} object");
            visitAttendantType.AddField(m => m.Id, nameof(VisitAttendant.Id));
            visitAttendantType.AddField(m => m.VisitId, nameof(VisitAttendant.VisitId));
            visitAttendantType.AddField(m => m.ProviderId, nameof(VisitAttendant.ProviderId));
            visitAttendantType.AddField(m => m.Provider, nameof(VisitAttendant.Provider));

            var hospitalizationStatusType = schema.AddType<HospitalizationStatus>(ToGraphQlName(nameof(HospitalizationStatus)), $"{ToGraphQlName(nameof(HospitalizationStatus))} object");
            hospitalizationStatusType.AddField(m => m.Id, nameof(HospitalizationStatus.Id));
            hospitalizationStatusType.AddField(m => m.Code, nameof(HospitalizationStatus.Code));
            hospitalizationStatusType.AddField(m => m.Name, nameof(HospitalizationStatus.Name));
            hospitalizationStatusType.AddField(m => m.BillingCode, nameof(HospitalizationStatus.BillingCode));
            hospitalizationStatusType.AddField(m => m.Color, nameof(HospitalizationStatus.Color));
            hospitalizationStatusType.AddField(m => m.DisplayOrder, nameof(HospitalizationStatus.DisplayOrder));

            var hospitalizationType = schema.AddType<Hospitalization>(ToGraphQlName(nameof(Hospitalization)), $"{ToGraphQlName(nameof(Hospitalization))} object");
            hospitalizationType.AddField(m => m.Id, nameof(Hospitalization.Id));
            hospitalizationType.AddField(m => m.PatientId, nameof(Hospitalization.PatientId));
            hospitalizationType.AddField(m => m.CaseId, nameof(Hospitalization.CaseId));
            hospitalizationType.AddField(m => m.FacilityId, nameof(Hospitalization.FacilityId));
            hospitalizationType.AddField(m => m.PsychEvaluation, nameof(Hospitalization.PsychEvaluation));
            hospitalizationType.AddField(m => m.AdmissionDate, nameof(Hospitalization.AdmissionDate));
            hospitalizationType.AddField(m => m.DischargeDate, nameof(Hospitalization.DischargeDate));
            hospitalizationType.AddField(m => m.HospitalizationStatusId, nameof(Hospitalization.HospitalizationStatusId));
            hospitalizationType.AddField(m => m.Patient, nameof(Hospitalization.Patient));
            hospitalizationType.AddField(m => m.Facility, nameof(Hospitalization.Facility));
            hospitalizationType.AddField(m => m.HospitalizationStatus, nameof(Hospitalization.HospitalizationStatus));
        }

        private static void AddToQuery(SchemaType<ConsilientDbContext> query)
        {
            query
                .AddField(
                    "visits",
                    new
                    {
                        dateServiced = ArgumentHelper.Required<string>(),
                        facilityId = ArgumentHelper.Required<int>()
                    },
                    (ctx, args) => ctx.Visits
                        .AsNoTracking()
                        .Where(v => v.DateServiced == DateOnly.ParseExact(args.dateServiced, "yyyy-MM-dd") && v.Hospitalization.FacilityId == args.facilityId)
                        .OrderBy(p => p.Id),
                    "List of visits for a specific date and facility")
                .UseSort();

            //var employeesField = ToGraphQlName(nameof(ConsilientDbContext.Employees));
            //query
            //    .AddField(employeesField, (ctx) => ctx.Employees.AsNoTracking().OrderBy(p => p.Id), $"List of {employeesField}")
            //    .UseFilter()
            //    .UseSort();

            var patientsField = ToGraphQlName(nameof(ConsilientDbContext.Patients));
            query
                .AddField(patientsField, (ctx) => ctx.Patients.AsNoTracking().OrderBy(p => p.Id), $"List of {patientsField}")
                .UseFilter()
                .UseSort();
        }

        private static string ToGraphQlName(string pascalName) => string.IsNullOrEmpty(pascalName) ? pascalName : char.ToLowerInvariant(pascalName[0]) + pascalName[1..];
    }

}

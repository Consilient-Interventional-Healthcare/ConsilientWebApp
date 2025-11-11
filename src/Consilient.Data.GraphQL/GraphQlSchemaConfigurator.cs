using Consilient.Data.Entities;
using EntityGraphQL.Schema;
using EntityGraphQL.Schema.FieldExtensions;
using Microsoft.EntityFrameworkCore;

namespace Consilient.Data.GraphQL
{
    internal static class GraphQlSchemaConfigurator
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
            var patientVisitType = schema.AddType<Visit>(ToGraphQlName(nameof(Visit)), $"{ToGraphQlName(nameof(Visit))} object");
            //patientVisitType.AddField(m => m.AdmissionNumber, nameof(PatientVisit.AdmissionNumber));
            patientVisitType.AddField(m => m.CosigningPhysicianEmployee, nameof(Visit.CosigningPhysicianEmployee));
            patientVisitType.AddField(m => m.DateServiced, nameof(Visit.DateServiced));
            patientVisitType.AddField(m => m.Facility, nameof(Visit.Facility));
            patientVisitType.AddField(m => m.Id, nameof(Visit.Id));
            patientVisitType.AddField(m => m.Insurance, nameof(Visit.Insurance));
            patientVisitType.AddField(m => m.IsScribeServiceOnly, nameof(Visit.IsScribeServiceOnly));
            patientVisitType.AddField(m => m.NursePractitionerEmployee, nameof(Visit.NursePractitionerEmployee));
            patientVisitType.AddField(m => m.Patient, nameof(Visit.Patient));
            patientVisitType.AddField(m => m.PhysicianEmployee, nameof(Visit.PhysicianEmployee));
            patientVisitType.AddField(m => m.ScribeEmployee, nameof(Visit.ScribeEmployee));
            patientVisitType.AddField(m => m.ServiceType, nameof(Visit.ServiceType));

            var patientType = schema.AddType<Patient>(ToGraphQlName(nameof(Patient)), $"{ToGraphQlName(nameof(Patient))} object");
            patientType.AddField(m => m.PatientBirthDate, nameof(Patient.PatientBirthDate));
            patientType.AddField(m => m.PatientFirstName, nameof(Patient.PatientFirstName));
            patientType.AddField(m => m.Id, nameof(Patient.Id));
            patientType.AddField(m => m.PatientLastName, nameof(Patient.PatientLastName));
            patientType.AddField(m => m.PatientMrn, nameof(Patient.PatientMrn));

            var employeeType = schema.AddType<Employee>(ToGraphQlName(nameof(Employee)), $"{ToGraphQlName(nameof(Employee))} object");
            employeeType.AddField(m => m.CanApproveVisits, nameof(Employee.CanApproveVisits));
            employeeType.AddField(m => m.Email, nameof(Employee.Email));
            employeeType.AddField(m => m.Id, nameof(Employee.Id));
            employeeType.AddField(m => m.FirstName, nameof(Employee.FirstName));
            employeeType.AddField(m => m.IsAdministrator, nameof(Employee.IsAdministrator));
            employeeType.AddField(m => m.IsProvider, nameof(Employee.IsProvider));
            employeeType.AddField(m => m.LastName, nameof(Employee.LastName));
            employeeType.AddField(m => m.Role, nameof(Employee.Role));

            var facilityType = schema.AddType<Facility>(ToGraphQlName(nameof(Facility)), $"{ToGraphQlName(nameof(Facility))} object");
            facilityType.AddField(m => m.FacilityAbbreviation, nameof(Facility.FacilityAbbreviation));
            facilityType.AddField(m => m.Id, nameof(Facility.Id));
            facilityType.AddField(m => m.FacilityName, nameof(Facility.FacilityName));

            var insuranceType = schema.AddType<Insurance>(ToGraphQlName(nameof(Insurance)), $"{ToGraphQlName(nameof(Insurance))} object");
            insuranceType.AddField(m => m.InsuranceCode, nameof(Insurance.InsuranceCode));
            insuranceType.AddField(m => m.InsuranceDescription, nameof(Insurance.InsuranceDescription));
            insuranceType.AddField(m => m.Id, nameof(Insurance.Id));

            var serviceTypeType = schema.AddType<ServiceType>(ToGraphQlName(nameof(ServiceType)), $"{ToGraphQlName(nameof(ServiceType))} object");
            serviceTypeType.AddField(m => m.Cptcode, nameof(ServiceType.Cptcode));
            serviceTypeType.AddField(m => m.Id, nameof(ServiceType.Id));
            serviceTypeType.AddField(m => m.Description, nameof(ServiceType.Description));
        }

        private static void AddToQuery(SchemaType<ConsilientDbContext> query)
        {
            var patientVisitsField = ToGraphQlName(nameof(ConsilientDbContext.PatientVisits));
            query
                .AddField(patientVisitsField, (ctx) => ctx.PatientVisits.AsNoTracking().OrderBy(p => p.Id), $"List of {patientVisitsField}")
                .UseFilter()
                .UseSort();

            var employeesField = ToGraphQlName(nameof(ConsilientDbContext.Employees));
            query
                .AddField(employeesField, (ctx) => ctx.Employees.AsNoTracking().OrderBy(p => p.Id), $"List of {employeesField}")
                .UseFilter()
                .UseSort();

            var patientsField = ToGraphQlName(nameof(ConsilientDbContext.Patients));
            query
                .AddField(patientsField, (ctx) => ctx.Patients.AsNoTracking().OrderBy(p => p.Id), $"List of {patientsField}")
                .UseFilter()
                .UseSort();
        }

        private static string ToGraphQlName(string pascalName) => string.IsNullOrEmpty(pascalName) ? pascalName : char.ToLowerInvariant(pascalName[0]) + pascalName[1..];
    }

}

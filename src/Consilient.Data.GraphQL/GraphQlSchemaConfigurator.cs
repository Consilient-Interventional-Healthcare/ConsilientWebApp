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
            var visitType = schema.AddType<Visit>(ToGraphQlName(nameof(Visit)), $"{ToGraphQlName(nameof(Visit))} object");
            visitType.AddField(m => m.DateServiced, nameof(Visit.DateServiced));
            visitType.AddField(m => m.Hospitalization, nameof(Visit.Hospitalization));
            visitType.AddField(m => m.Id, nameof(Visit.Id));
            visitType.AddField(m => m.IsScribeServiceOnly, nameof(Visit.IsScribeServiceOnly));
            visitType.AddField("patient", m => m.Hospitalization.Patient, nameof(Hospitalization.Patient));

            var patientType = schema.AddType<Patient>(ToGraphQlName(nameof(Patient)), $"{ToGraphQlName(nameof(Patient))} object");
            patientType.AddField(m => m.BirthDate, nameof(Patient.BirthDate));
            patientType.AddField(m => m.FirstName, nameof(Patient.FirstName));
            patientType.AddField(m => m.Id, nameof(Patient.Id));
            patientType.AddField(m => m.LastName, nameof(Patient.LastName));
            //patientType.AddField(m => m.Mrn, nameof(Patient.Mrn));

            var employeeType = schema.AddType<Employee>(ToGraphQlName(nameof(Employee)), $"{ToGraphQlName(nameof(Employee))} object");
            //employeeType.AddField(m => m.CanApproveVisits, nameof(Employee.CanApproveVisits));
            employeeType.AddField(m => m.Email, nameof(Employee.Email));
            employeeType.AddField(m => m.Id, nameof(Employee.Id));
            employeeType.AddField(m => m.FirstName, nameof(Employee.FirstName));
            //employeeType.AddField(m => m.IsAdministrator, nameof(Employee.IsAdministrator));
            //employeeType.AddField(m => m.IsProvider, nameof(Employee.IsProvider));
            employeeType.AddField(m => m.LastName, nameof(Employee.LastName));
            employeeType.AddField(m => m.Role, nameof(Employee.Role));

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
        }

        private static void AddToQuery(SchemaType<ConsilientDbContext> query)
        {
            var visitsField = ToGraphQlName(nameof(ConsilientDbContext.Visits));
            query
                .AddField(visitsField, (ctx) => ctx.Visits.AsNoTracking().OrderBy(p => p.Id), $"List of {visitsField}")
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

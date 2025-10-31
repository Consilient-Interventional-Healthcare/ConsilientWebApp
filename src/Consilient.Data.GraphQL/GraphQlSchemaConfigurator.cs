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
            AddPatientVisitsToQuery(query);
        }

        private static void AddTypes(SchemaProvider<ConsilientDbContext> schema)
        {
            var patientVisitType = schema.AddType<PatientVisit>(nameof(PatientVisit), "An PatientVisit object");
            patientVisitType.AddField(m => m.AdmissionNumber, nameof(PatientVisit.AdmissionNumber));
            patientVisitType.AddField(m => m.CosigningPhysicianEmployee, nameof(PatientVisit.CosigningPhysicianEmployee));
            patientVisitType.AddField(m => m.DateServiced, nameof(PatientVisit.DateServiced));
            patientVisitType.AddField(m => m.Facility, nameof(PatientVisit.Facility));
            patientVisitType.AddField(m => m.Insurance, nameof(PatientVisit.Insurance));
            patientVisitType.AddField(m => m.IsScribeServiceOnly, nameof(PatientVisit.IsScribeServiceOnly));
            patientVisitType.AddField(m => m.IsSupervising, nameof(PatientVisit.IsSupervising));
            patientVisitType.AddField(m => m.NursePractitionerEmployee, nameof(PatientVisit.NursePractitionerEmployee));
            patientVisitType.AddField(m => m.Patient, nameof(PatientVisit.Patient));
            patientVisitType.AddField(m => m.PatientVisitId, nameof(PatientVisit.PatientVisitId));
            patientVisitType.AddField(m => m.PhysicianEmployee, nameof(PatientVisit.PhysicianEmployee));
            patientVisitType.AddField(m => m.ScribeEmployee, nameof(PatientVisit.ScribeEmployee));
            patientVisitType.AddField(m => m.ServiceType, nameof(PatientVisit.ServiceType));

            var patientType = schema.AddType<Patient>(nameof(Patient), "A Patient object");
            patientType.AddField(m => m.PatientBirthDate, nameof(Patient.PatientBirthDate));
            patientType.AddField(m => m.PatientFirstName, nameof(Patient.PatientFirstName));
            patientType.AddField(m => m.PatientId, nameof(Patient.PatientId));
            patientType.AddField(m => m.PatientLastName, nameof(Patient.PatientLastName));
            patientType.AddField(m => m.PatientMrn, nameof(Patient.PatientMrn));

            var employeeType = schema.AddType<Employee>(nameof(Employee), "An Employee object");
            employeeType.AddField(m => m.CanApproveVisits, nameof(Employee.CanApproveVisits));
            employeeType.AddField(m => m.Email, nameof(Employee.Email));
            employeeType.AddField(m => m.EmployeeId, nameof(Employee.EmployeeId));
            employeeType.AddField(m => m.FirstName, nameof(Employee.FirstName));
            employeeType.AddField(m => m.IsAdministrator, nameof(Employee.IsAdministrator));
            employeeType.AddField(m => m.IsProvider, nameof(Employee.IsProvider));
            employeeType.AddField(m => m.LastName, nameof(Employee.LastName));
            employeeType.AddField(m => m.Role, nameof(Employee.Role));
            
            var facilityType = schema.AddType<Facility>(nameof(Facility), "A Facility object");
            facilityType.AddField(m => m.FacilityAbbreviation, nameof(Facility.FacilityAbbreviation));
            facilityType.AddField(m => m.FacilityId, nameof(Facility.FacilityId));
            facilityType.AddField(m => m.FacilityName, nameof(Facility.FacilityName));

            var insuranceType = schema.AddType<Insurance>(nameof(Insurance), "An Insurance object");
            insuranceType.AddField(m => m.InsuranceCode, nameof(Insurance.InsuranceCode));
            insuranceType.AddField(m => m.InsuranceDescription, nameof(Insurance.InsuranceDescription));
            insuranceType.AddField(m => m.InsuranceId, nameof(Insurance.InsuranceId));
            
            var serviceTypeType = schema.AddType<ServiceType>(typeof(ServiceType).Name, "A service type object");
            serviceTypeType.AddField(m => m.Cptcode, nameof(ServiceType.Cptcode));
            serviceTypeType.AddField(m => m.ServiceTypeId, nameof(ServiceType.ServiceTypeId));
            serviceTypeType.AddField(m => m.Description, nameof(ServiceType.Description));
        }

        private static void AddPatientVisitsToQuery(SchemaType<ConsilientDbContext> query)
        {
            query
                .AddField("patientVisits", (ctx) => ctx.PatientVisits.AsNoTracking().OrderBy(p => p.PatientVisitId), "A list of patientVisits")
                .UseFilter()
                .UseSort();
        }
    }

}

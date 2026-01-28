using Consilient.Common;
using Consilient.Data.Entities;
using Consilient.Data.Entities.Clinical;
using EntityGraphQL.Schema;
using EntityGraphQL.Schema.FieldExtensions;
using Microsoft.EntityFrameworkCore;

namespace Consilient.Data.GraphQL;

public static partial class GraphQlSchemaConfigurator
{
    public static void ConfigureSchema(SchemaProvider<ConsilientDbContext> schema)
    {
        ArgumentNullException.ThrowIfNull(schema, nameof(schema));

        AddCoreTypes(schema);
        AddProviderAssignmentTypes(schema);
        AddVisitTypes(schema);
        AddDailyLogTypes(schema);
        AddLogEntryTypes(schema);

        var query = schema.Query();
        AddCoreQueries(query);
        AddProviderAssignmentQuery(query);
        AddVisitQuery(query);
        AddDailyLogQuery(query);
        AddLogEntryQuery(query);
    }

    private static void AddCoreTypes(SchemaProvider<ConsilientDbContext> schema)
    {
        schema.AddEnum<ProviderType>(nameof(ProviderType), "Provider type enum");

        var patientType = schema.AddType<Patient>(ToGraphQlName(nameof(Patient)), $"{ToGraphQlName(nameof(Patient))} object");
        patientType.AddField(m => m.BirthDate, nameof(Patient.BirthDate));
        patientType.AddField(m => m.FirstName, nameof(Patient.FirstName));
        patientType.AddField(m => m.Id, nameof(Patient.Id));
        patientType.AddField(m => m.LastName, nameof(Patient.LastName));

        var facilityType = schema.AddType<Facility>(ToGraphQlName(nameof(Facility)), $"{ToGraphQlName(nameof(Facility))} object");
        facilityType.AddField(m => m.Abbreviation, nameof(Facility.Abbreviation));
        facilityType.AddField(m => m.Id, nameof(Facility.Id));
        facilityType.AddField(m => m.Name, nameof(Facility.Name));

        var insuranceType = schema.AddType<Insurance>(ToGraphQlName(nameof(Insurance)), $"{ToGraphQlName(nameof(Insurance))} object");
        insuranceType.AddField(m => m.Code, nameof(Insurance.Code));
        insuranceType.AddField(m => m.Description, nameof(Insurance.Description));
        insuranceType.AddField(m => m.Id, nameof(Insurance.Id));

        var serviceTypeType = schema.AddType<ServiceTypeEntity>(ToGraphQlName(nameof(ServiceTypeEntity)), $"{ToGraphQlName(nameof(ServiceTypeEntity))} object");
        serviceTypeType.AddField(m => m.Id, nameof(ServiceTypeEntity.Id));
        serviceTypeType.AddField(m => m.Name, nameof(ServiceTypeEntity.Name));

        var providerType = schema.AddType<Provider>(ToGraphQlName(nameof(Provider)), $"{ToGraphQlName(nameof(Provider))} object");
        providerType.AddField(m => m.Id, nameof(Provider.Id));
        providerType.AddField(m => m.FirstName, nameof(Provider.FirstName));
        providerType.AddField(m => m.LastName, nameof(Provider.LastName));
        providerType.AddField(m => m.TitleExtension, nameof(Provider.TitleExtension));
        providerType.AddField(m => m.Type, nameof(Provider.Type));
        providerType.AddField(m => m.Email, nameof(Provider.Email));
        providerType.AddField(m => m.EmployeeId, nameof(Provider.EmployeeId));

        var hospitalizationStatusType = schema.AddType<HospitalizationStatusEntity>(ToGraphQlName(nameof(HospitalizationStatusEntity)), $"{ToGraphQlName(nameof(HospitalizationStatusEntity))} object");
        hospitalizationStatusType.AddField(m => m.Id, nameof(HospitalizationStatusEntity.Id));
        hospitalizationStatusType.AddField(m => m.Code, nameof(HospitalizationStatusEntity.Code));
        hospitalizationStatusType.AddField(m => m.Name, nameof(HospitalizationStatusEntity.Name));
        hospitalizationStatusType.AddField(m => m.DisplayOrder, nameof(HospitalizationStatusEntity.DisplayOrder));

        var hospitalizationType = schema.AddType<Hospitalization>(ToGraphQlName(nameof(Hospitalization)), $"{ToGraphQlName(nameof(Hospitalization))} object");
        hospitalizationType.AddField(m => m.Id, nameof(Hospitalization.Id));
        hospitalizationType.AddField(m => m.PatientId, nameof(Hospitalization.PatientId));
        hospitalizationType.AddField(m => m.CaseId, nameof(Hospitalization.CaseId));
        hospitalizationType.AddField(m => m.FacilityId, nameof(Hospitalization.FacilityId));
        hospitalizationType.AddField(m => m.PsychEvaluation, nameof(Hospitalization.PsychEvaluation));
        hospitalizationType.AddField(m => m.AdmissionDate, nameof(Hospitalization.AdmissionDate));
        hospitalizationType.AddField(m => m.DischargeDate, nameof(Hospitalization.DischargeDate));
        hospitalizationType.AddField("hospitalizationStatusId", m => (int)m.Status, nameof(Hospitalization.Status));
        hospitalizationType.AddField(m => m.Patient, nameof(Hospitalization.Patient));
        hospitalizationType.AddField(m => m.Facility, nameof(Hospitalization.Facility));
        hospitalizationType.AddField("hospitalizationStatus", m => m.HospitalizationStatusNavigation, "HospitalizationStatusNavigation");
    }

    private static void AddCoreQueries(SchemaType<ConsilientDbContext> query)
    {
        var patientsField = ToGraphQlName(nameof(ConsilientDbContext.Patients));
        query
            .AddField(patientsField, (ctx) => ctx.Patients.AsNoTracking().OrderBy(p => p.Id), $"List of {patientsField}")
            .UseFilter()
            .UseSort();
    }

    private static string ToGraphQlName(string pascalName) => string.IsNullOrEmpty(pascalName) ? pascalName : char.ToLowerInvariant(pascalName[0]) + pascalName[1..];
}

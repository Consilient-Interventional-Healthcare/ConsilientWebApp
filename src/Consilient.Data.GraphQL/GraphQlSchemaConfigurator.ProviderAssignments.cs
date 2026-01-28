using Consilient.Data.Entities.Staging;
using Consilient.Data.GraphQL.Models;
using EntityGraphQL.Schema;

namespace Consilient.Data.GraphQL;

public static partial class GraphQlSchemaConfigurator
{
    private static void AddProviderAssignmentTypes(SchemaProvider<ConsilientDbContext> schema)
    {
        // Register the batch status enum
        schema.AddEnum<ProviderAssignmentBatchStatus>("providerAssignmentBatchStatus", "Status of a provider assignment batch");

        // Register the nested types
        var providerAssignmentPatientType = schema.AddType<ProviderAssignmentPatient>("providerAssignmentPatient", "Patient data from resolved patient or staging");
        providerAssignmentPatientType.AddField(m => m.FirstName, nameof(ProviderAssignmentPatient.FirstName));
        providerAssignmentPatientType.AddField(m => m.LastName, nameof(ProviderAssignmentPatient.LastName));
        providerAssignmentPatientType.AddField(m => m.Mrn, nameof(ProviderAssignmentPatient.Mrn));

        var providerAssignmentProviderType = schema.AddType<ProviderAssignmentProvider>("providerAssignmentProvider", "Provider data from resolved provider or staging");
        providerAssignmentProviderType.AddField(m => m.FirstName, nameof(ProviderAssignmentProvider.FirstName));
        providerAssignmentProviderType.AddField(m => m.LastName, nameof(ProviderAssignmentProvider.LastName));

        var providerAssignmentHospitalizationType = schema.AddType<ProviderAssignmentHospitalization>("providerAssignmentHospitalization", "Hospitalization data from resolved hospitalization or staging");
        providerAssignmentHospitalizationType.AddField(m => m.CaseId, nameof(ProviderAssignmentHospitalization.CaseId));
        providerAssignmentHospitalizationType.AddField(m => m.AdmissionDate, nameof(ProviderAssignmentHospitalization.AdmissionDate));

        var providerAssignmentVisitType = schema.AddType<ProviderAssignmentVisit>("providerAssignmentVisit", "Visit data from resolved visit or staging");
        providerAssignmentVisitType.AddField(m => m.Room, nameof(ProviderAssignmentVisit.Room));
        providerAssignmentVisitType.AddField(m => m.Bed, nameof(ProviderAssignmentVisit.Bed));

        var providerAssignmentHospitalizationStatusType = schema.AddType<ProviderAssignmentHospitalizationStatus>("providerAssignmentHospitalizationStatus", "Hospitalization status data from resolved status");
        providerAssignmentHospitalizationStatusType.AddField(m => m.Name, nameof(ProviderAssignmentHospitalizationStatus.Name));
        providerAssignmentHospitalizationStatusType.AddField(m => m.Code, nameof(ProviderAssignmentHospitalizationStatus.Code));
        providerAssignmentHospitalizationStatusType.AddField(m => m.Color, nameof(ProviderAssignmentHospitalizationStatus.Color));

        // Register the ProviderAssignment entity as a GraphQL type
        var paType = schema.AddType<ProviderAssignment>("providerAssignment", "Provider assignment staging record");

        // Scalar fields - always cheap to fetch
        paType.AddField(m => m.Id, nameof(ProviderAssignment.Id));
        paType.AddField(m => m.BatchId, nameof(ProviderAssignment.BatchId));
        paType.AddField("date", m => m.ServiceDate, nameof(ProviderAssignment.ServiceDate));
        paType.AddField(m => m.FacilityId, nameof(ProviderAssignment.FacilityId));

        // Resolved IDs - expose separately so client can check if resolved
        paType.AddField(m => m.ResolvedPatientId, nameof(ProviderAssignment.ResolvedPatientId));
        paType.AddField(m => m.ResolvedPhysicianId, nameof(ProviderAssignment.ResolvedPhysicianId));
        paType.AddField(m => m.ResolvedNursePractitionerId, nameof(ProviderAssignment.ResolvedNursePractitionerId));
        paType.AddField(m => m.ResolvedHospitalizationId, nameof(ProviderAssignment.ResolvedHospitalizationId));
        paType.AddField(m => m.ResolvedVisitId, nameof(ProviderAssignment.ResolvedVisitId));
        paType.AddField("resolvedHospitalizationStatusId", m => (int?)m.ResolvedHospitalizationStatus, nameof(ProviderAssignment.ResolvedHospitalizationStatus));

        // IsNew flags
        paType.AddField(m => m.PatientWasCreated, nameof(ProviderAssignment.PatientWasCreated));
        paType.AddField(m => m.PhysicianWasCreated, nameof(ProviderAssignment.PhysicianWasCreated));
        paType.AddField(m => m.NursePractitionerWasCreated, nameof(ProviderAssignment.NursePractitionerWasCreated));
        paType.AddField(m => m.HospitalizationWasCreated, nameof(ProviderAssignment.HospitalizationWasCreated));

        // Import status fields
        paType.AddField(m => m.ShouldImport, nameof(ProviderAssignment.ShouldImport));
        paType.AddField(m => m.Imported, nameof(ProviderAssignment.Imported));
        paType.AddField(m => m.ValidationErrorsJson, nameof(ProviderAssignment.ValidationErrorsJson));

        // Computed fields - only fetched when requested
        // Patient: uses navigation property when resolved, falls back to staging data
        paType.AddField("patient", pa => new ProviderAssignmentPatient
        {
            FirstName = pa.ResolvedPatient != null ? pa.ResolvedPatient.FirstName : (pa.NormalizedPatientFirstName ?? string.Empty),
            LastName = pa.ResolvedPatient != null ? pa.ResolvedPatient.LastName : (pa.NormalizedPatientLastName ?? string.Empty),
            Mrn = pa.Mrn
        }, "Patient data - from Patient table if resolved, otherwise from staging");

        // Physician: uses navigation property when resolved, falls back to staging data
        paType.AddField("physician", pa => new ProviderAssignmentProvider
        {
            FirstName = pa.ResolvedPhysician != null ? pa.ResolvedPhysician.FirstName : string.Empty,
            LastName = pa.ResolvedPhysician != null ? pa.ResolvedPhysician.LastName : (pa.NormalizedPhysicianLastName ?? string.Empty)
        }, "Physician data - from Provider table if resolved, otherwise from staging");

        // Nurse Practitioner: uses navigation property when resolved, falls back to staging data
        paType.AddField("nursePractitioner", pa => new ProviderAssignmentProvider
        {
            FirstName = pa.ResolvedNursePractitioner != null ? pa.ResolvedNursePractitioner.FirstName : string.Empty,
            LastName = pa.ResolvedNursePractitioner != null ? pa.ResolvedNursePractitioner.LastName : (pa.NormalizedNursePractitionerLastName ?? string.Empty)
        }, "Nurse practitioner data - from Provider table if resolved, otherwise from staging");

        // Hospitalization: uses navigation property when resolved, falls back to staging data
        paType.AddField("hospitalization", pa => new ProviderAssignmentHospitalization
        {
            CaseId = pa.ResolvedHospitalization != null ? pa.ResolvedHospitalization.CaseId.ToString() : pa.HospitalNumber,
            AdmissionDate = pa.ResolvedHospitalization != null ? pa.ResolvedHospitalization.AdmissionDate : pa.Admit
        }, "Hospitalization data - from Hospitalization table if resolved, otherwise from staging");

        // Visit: uses navigation property when resolved, falls back to staging data
        paType.AddField("visit", pa => new ProviderAssignmentVisit
        {
            Room = pa.ResolvedVisit != null ? pa.ResolvedVisit.Room : pa.Room,
            Bed = pa.ResolvedVisit != null ? pa.ResolvedVisit.Bed : pa.Bed
        }, "Visit data - from Visit table if resolved, otherwise from staging");

        paType.AddField("hospitalizationStatus", pa => new ProviderAssignmentHospitalizationStatus
        {
            Name = pa.ResolvedHospitalizationStatusNavigation != null ? pa.ResolvedHospitalizationStatusNavigation.Name : null,
            Code = pa.ResolvedHospitalizationStatusNavigation != null ? pa.ResolvedHospitalizationStatusNavigation.Code : null,
            Color = null,
        }, "HospitalizationStatus data - from HospitalizationStatus table if resolved, otherwise null");

        // Register the batch type
        var batchType = schema.AddType<ProviderAssignmentBatch>(
            "providerAssignmentBatch",
            "Provider assignment batch with metadata and items");

        batchType.AddField("batchId", m => m.Id, "Batch ID");
        batchType.AddField(m => m.Date, "Date");
        batchType.AddField(m => m.FacilityId, "FacilityId");
        batchType.AddField(m => m.Status, "Status");

        batchType.AddField("items", "Provider assignment items in this batch")
            .Resolve<ConsilientDbContext>((batch, ctx) => ctx.StagingProviderAssignments
                .Where(pa => pa.BatchId == batch.Id)
                .OrderBy(pa => pa.ServiceDate));
    }

    private static void AddProviderAssignmentQuery(SchemaType<ConsilientDbContext> query)
    {
        query.AddField(
            "providerAssignmentBatch",
            new
            {
                batchId = ArgumentHelper.Required<Guid>()
            },
            (ctx, args) => ctx.StagingProviderAssignmentBatches
                .Where(b => b.Id == args.batchId)
                .Select(b => b)
                .FirstOrDefault(),
            "Provider assignment batch with metadata and items");
    }
}

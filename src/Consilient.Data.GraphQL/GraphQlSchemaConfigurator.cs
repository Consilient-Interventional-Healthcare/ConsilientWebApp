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

            AddProviderAssignmentTypes(schema);

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

            query
                .AddField(
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
            providerAssignmentVisitType.AddField(m => m.Imported, nameof(ProviderAssignmentVisit.Imported));

            var providerAssignmentHospitalizationStatusType = schema.AddType<ProviderAssignmentHospitalizationStatus>("providerAssignmentHospitalizationStatus", "Hospitalization status data from resolved status");
            providerAssignmentHospitalizationStatusType.AddField(m => m.Name, nameof(ProviderAssignmentHospitalizationStatus.Name));
            providerAssignmentHospitalizationStatusType.AddField(m => m.BillingCode, nameof(ProviderAssignmentHospitalizationStatus.BillingCode));
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
            paType.AddField(m => m.ResolvedHospitalizationStatusId, nameof(ProviderAssignment.ResolvedHospitalizationStatusId));

            // IsNew flags
            paType.AddField(m => m.PatientWasCreated, nameof(ProviderAssignment.PatientWasCreated));
            paType.AddField(m => m.PhysicianWasCreated, nameof(ProviderAssignment.PhysicianWasCreated));
            paType.AddField(m => m.NursePractitionerWasCreated, nameof(ProviderAssignment.NursePractitionerWasCreated));
            paType.AddField(m => m.HospitalizationWasCreated, nameof(ProviderAssignment.HospitalizationWasCreated));

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
                Bed = pa.ResolvedVisit != null ? pa.ResolvedVisit.Bed : pa.Bed,
                Imported = pa.ResolvedVisitId != null
            }, "Visit data - from Visit table if resolved, otherwise from staging");

            paType.AddField("hospitalizationStatus", pa => new ProviderAssignmentHospitalizationStatus
            {
                Name = pa.ResolvedHospitalizationStatus != null ? pa.ResolvedHospitalizationStatus.Name : null,
                BillingCode = pa.ResolvedHospitalizationStatus != null ? pa.ResolvedHospitalizationStatus.BillingCode : null,
                Code = pa.ResolvedHospitalizationStatus != null ? pa.ResolvedHospitalizationStatus.Code : null,
                Color = pa.ResolvedHospitalizationStatus != null ? pa.ResolvedHospitalizationStatus.Color : null,
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
    }

}

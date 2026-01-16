namespace Consilient.ProviderAssignments.Contracts
{
    public interface IProviderAssignmentsResolver
    {
        // 1. Read from staging table the batch matching the batchId
        // 2. Validate data integrity, annotate errors if any
        // 3. Check if there are items that need resolution
        // 4. Resolve attendingPhysicianId via attendingMD
        // 5. Resolve nursePractitionerId via nursePractitioner
        // 6. Select records that need to be resolved following the following rules
        //    a. If attendingPhysician works only for consilient, then record should be marked as going into the database
        //    b. If attendingPhysician works for multiple facilities, check if there is a nurse practitioner assigned and if the nurse practitioner works for consilient, then record should be marked as going into the database
        // 7. Indicate reason for exclusion for other records
        // 8. Resolve patientId via Mrn. If not found, indicate new patient record should be created
        // 9. Resolve hospitalizationId via HospitalizationNumber. If not found, indicate new hospitalization record should be created. Use Psych Eval to select a default hospitalization status.
        // 10. Update staging table with resolution results
        Task ResolveAsync(Guid batchId, CancellationToken cancellationToken);
    }
}

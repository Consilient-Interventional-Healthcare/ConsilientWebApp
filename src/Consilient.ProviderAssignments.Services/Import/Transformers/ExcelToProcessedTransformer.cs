using Consilient.Infrastructure.ExcelImporter.Contracts;
using Consilient.ProviderAssignments.Contracts.Import;

namespace Consilient.ProviderAssignments.Services.Import.Transformers;

/// <summary>
/// Transforms a raw Excel row into a fully processed provider assignment.
/// Handles:
/// - Trimming string fields
/// - Injecting import context (FacilityId, ServiceDate)
/// - Parsing derived fields via <see cref="NameParser"/>
/// </summary>
public class ExcelToProcessedTransformer(int facilityId, DateOnly serviceDate)
    : IRowEnricher<ExcelProviderAssignmentRow, ProcessedProviderAssignment>
{
    private readonly int _facilityId = facilityId;
    private readonly DateOnly _serviceDate = serviceDate;

    public ProcessedProviderAssignment Enrich(ExcelProviderAssignmentRow row)
    {
        ArgumentNullException.ThrowIfNull(row);

        var trimmedRow = TrimStrings(row);

        var (room, bed) = NameParser.ParseLocation(trimmedRow.Location);
        var (lastName, firstName) = NameParser.SplitPatientName(trimmedRow.Name);

        return new ProcessedProviderAssignment
        {
            FacilityId = _facilityId,
            ServiceDate = _serviceDate,
            Raw = trimmedRow,
            Room = room,
            Bed = bed,
            NormalizedPatientLastName = NameParser.NormalizeCase(lastName),
            NormalizedPatientFirstName = NameParser.NormalizeCase(firstName),
            NormalizedPhysicianLastName = NameParser.ExtractProviderLastName(trimmedRow.AttendingMD),
            NormalizedNursePractitionerLastName = NameParser.ExtractProviderLastName(trimmedRow.NursePractitioner)
        };
    }

    private static ExcelProviderAssignmentRow TrimStrings(ExcelProviderAssignmentRow row)
    {
        return row with
        {
            Name = row.Name?.Trim() ?? string.Empty,
            Location = row.Location?.Trim() ?? string.Empty,
            HospitalNumber = row.HospitalNumber?.Trim() ?? string.Empty,
            Mrn = row.Mrn?.Trim() ?? string.Empty,
            Insurance = row.Insurance?.Trim() ?? string.Empty,
            NursePractitioner = row.NursePractitioner?.Trim() ?? string.Empty,
            IsCleared = row.IsCleared?.Trim() ?? string.Empty,
            H_P = row.H_P?.Trim() ?? string.Empty,
            PsychEval = row.PsychEval?.Trim() ?? string.Empty,
            AttendingMD = row.AttendingMD?.Trim() ?? string.Empty
        };
    }
}

using Consilient.Infrastructure.ExcelImporter.Core;
using Consilient.Infrastructure.ExcelImporter.Domain;

namespace Consilient.Infrastructure.ExcelImporter.Transformers;

public class SetImportParametersTransformer : IRowTransformer<DoctorAssignment>
{
    private readonly int _facilityId;
    private readonly DateOnly _serviceDate;

    public SetImportParametersTransformer(int facilityId, DateOnly serviceDate)
    {
        _facilityId = facilityId;
        _serviceDate = serviceDate;
    }

    public DoctorAssignment Transform(DoctorAssignment row)
    {
        return row with
        {
            FacilityId = _facilityId,
            ServiceDate = _serviceDate
        };
    }
}

using Consilient.DoctorAssignments.Contracts;
using Consilient.Infrastructure.ExcelImporter.Core;

namespace Consilient.DoctorAssignments.Services.Importer
{
    public class SetImportParametersTransformer(int facilityId, DateOnly serviceDate) : IRowTransformer<ExternalDoctorAssignment>
    {
        private readonly int _facilityId = facilityId;
        private readonly DateOnly _serviceDate = serviceDate;

        public ExternalDoctorAssignment Transform(ExternalDoctorAssignment row)
        {
            return row with
            {
                FacilityId = _facilityId,
                ServiceDate = _serviceDate
            };
        }
    }
}

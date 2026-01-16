using Consilient.ProviderAssignments.Contracts;
using Consilient.Infrastructure.ExcelImporter.Core;

namespace Consilient.ProviderAssignments.Services.Importer
{
    public class SetImportParametersTransformer(int facilityId, DateOnly serviceDate) : IRowTransformer<ExternalProviderAssignment>
    {
        private readonly int _facilityId = facilityId;
        private readonly DateOnly _serviceDate = serviceDate;

        public ExternalProviderAssignment Transform(ExternalProviderAssignment row)
        {
            return row with
            {
                FacilityId = _facilityId,
                ServiceDate = _serviceDate
            };
        }
    }
}

using Consilient.Infrastructure.ExcelImporter.Core;
using Consilient.ProviderAssignments.Contracts;

namespace Consilient.Infrastructure.ExcelImporter.Transformers
{
    public class CalculateAgeFromDobTransformer : IRowTransformer<ExternalProviderAssignment>
    {
        public ExternalProviderAssignment Transform(ExternalProviderAssignment row)
        {
            if (row.Dob.HasValue && row.Age == 0)
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                var age = today.Year - row.Dob.Value.Year;
                if (row.Dob.Value > today.AddYears(-age))
                {
                    age--;
                }

                return row with { Age = age };
            }

            return row;
        }
    }
}

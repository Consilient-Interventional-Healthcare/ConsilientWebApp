using Consilient.Infrastructure.ExcelImporter.Core;
using Consilient.Infrastructure.ExcelImporter.Domain;

namespace Consilient.Infrastructure.ExcelImporter.Transformers
{
    public class CalculateAgeFromDobTransformer : IRowTransformer<DoctorAssignment>
    {
        public DoctorAssignment Transform(DoctorAssignment row)
        {
            if (row.Dob.HasValue && row.Age == 0)
            {
                var today = DateTime.Today;
                var age = today.Year - row.Dob.Value.Year;
                if (row.Dob.Value.Date > today.AddYears(-age))
                {
                    age--;
                }

                return row with { Age = age };
            }

            return row;
        }
    }
}

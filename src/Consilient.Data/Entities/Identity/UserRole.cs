using Consilient.Data.Contracts;
using Microsoft.AspNetCore.Identity;

namespace Consilient.Data.Entities.Identity
{
    public class UserRole : IdentityUserRole<int>, IAuditableEntity
    {
        public DateTime CreatedAtUtc { get; private set; }

        public DateTime UpdatedAtUtc { get; private set; }

        public byte[] RowVersion { get; private set; } = [];

        public void SetCreatedAtUtc(DateTime createdAtUtc)
        {
            CreatedAtUtc = createdAtUtc;
        }

        public void SetUpdatedAtUtc(DateTime updatedAtUtc)
        {
            UpdatedAtUtc = updatedAtUtc;
        }
    }
}

using Consilient.Data.Contracts;

namespace Consilient.Data.Entities
{
    public abstract class BaseEntity<TID> : IEntity<TID>, IAuditableEntity
        where TID : struct, IEquatable<TID>
    {
        public TID Id { get; private set; }

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

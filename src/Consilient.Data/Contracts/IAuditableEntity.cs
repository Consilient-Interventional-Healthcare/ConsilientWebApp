namespace Consilient.Data.Contracts
{
    public interface IAuditableEntity
    {
        DateTime CreatedAtUtc { get; }
        DateTime UpdatedAtUtc { get; }
        byte[] RowVersion { get; }

        public void SetCreatedAtUtc(DateTime createdAtUtc);
        public void SetUpdatedAtUtc(DateTime updatedAtUtc);
    }
}

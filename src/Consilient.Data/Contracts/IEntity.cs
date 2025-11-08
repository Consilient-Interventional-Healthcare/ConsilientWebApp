namespace Consilient.Data.Contracts
{
    public interface IEntity<TID> where TID : struct, IEquatable<TID>
    {
        TID Id { get; }
        byte[] RowVersion { get; }
    }
}

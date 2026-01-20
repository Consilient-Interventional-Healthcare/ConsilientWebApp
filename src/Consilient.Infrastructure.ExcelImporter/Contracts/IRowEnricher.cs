namespace Consilient.Infrastructure.ExcelImporter.Contracts
{
    /// <summary>
    /// Transforms a raw row into an enriched row of a different type.
    /// Used for type-changing transformations in the import pipeline.
    /// </summary>
    public interface IRowEnricher<TInput, TOutput>
        where TInput : class
        where TOutput : class
    {
        TOutput Enrich(TInput row);
    }
}

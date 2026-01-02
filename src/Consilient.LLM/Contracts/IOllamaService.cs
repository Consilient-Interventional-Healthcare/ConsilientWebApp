namespace Consilient.LLM.Contracts
{
    public interface IOllamaService
    {
        Task<string> AskAsync(string prompt, CancellationToken cancellationToken = default);
    }
}

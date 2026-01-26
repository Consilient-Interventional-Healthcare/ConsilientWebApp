using Consilient.LLM.Contracts;
using OllamaSharp;

namespace Consilient.LLM
{
    public class OllamaService(OllamaServiceOptions configuration) : IOllamaService
    {
        private readonly OllamaApiClient _client = new(configuration.BaseUrl);
        private readonly string _defaultModel = configuration.DefaultModel;

        public async Task<string> AskAsync(string prompt, CancellationToken cancellationToken = default)
        {
            await foreach (var response in _client.GenerateAsync(new OllamaSharp.Models.GenerateRequest
            {
                Prompt = prompt,
                Model = _defaultModel
            }, cancellationToken))
            {
                if (response != null)
                {
                    return response.Response;
                }
            }
            return string.Empty;
        }
    }
}

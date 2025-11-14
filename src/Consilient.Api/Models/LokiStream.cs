namespace Consilient.Api.Models
{
    public class LokiStream
    {
        public Dictionary<string, string> Stream { get; set; } = null!;
        public List<List<string>> Values { get; set; } = null!;
    }
}

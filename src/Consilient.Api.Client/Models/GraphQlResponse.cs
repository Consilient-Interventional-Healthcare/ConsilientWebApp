namespace Consilient.Api.Client.Models
{
    public class GraphQlResponse
    {
        public List<GraphQlError>? Errors { get; set; }
        public Dictionary<string, object?>? Data { get; set; }
        public Dictionary<string, object>? Extensions { get; set; }
    }
}

namespace Consilient.WebApp.Models.AuthorizationModels
{
    public class ClientPrincipal
    {
        public string AuthenticationType { get; set; } = string.Empty;
        public IEnumerable<ClientClaim> Claims { get; set; } = [];
    }

}

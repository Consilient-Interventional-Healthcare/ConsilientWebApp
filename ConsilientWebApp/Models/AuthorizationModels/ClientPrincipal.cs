namespace ConsilientWebApp.Models.AuthorizationModels
{
    public class ClientPrincipal
    {
        public string AuthenticationType { get; set; }
        public IEnumerable<ClientClaim> Claims { get; set; }
    }

}

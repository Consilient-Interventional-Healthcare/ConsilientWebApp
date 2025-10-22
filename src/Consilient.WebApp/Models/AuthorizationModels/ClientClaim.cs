namespace Consilient.WebApp.Models.AuthorizationModels
{
    public class ClientClaim
    {
        public string Typ { get; set; } = string.Empty;
        public string Val { get; set; } = string.Empty;
        public string Type => Typ;
        public string Value => Val;
    }
}

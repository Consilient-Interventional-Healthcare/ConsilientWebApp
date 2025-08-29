namespace ConsilientWebApp.Models.AuthorizationModels
{
    public class ClientClaim
    {
        public string Typ { get; set; }
        public string Val { get; set; }
        public string Type => Typ;
        public string Value => Val;
    }
}

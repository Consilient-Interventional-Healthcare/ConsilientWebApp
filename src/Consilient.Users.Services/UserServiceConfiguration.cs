namespace Consilient.Users.Services
{
    public class UserServiceConfiguration
    {
        public bool AutoProvisionUser { get; set; } = false;
        public string[] AllowedEmailDomains { get; set; } = null!;
    }
}

namespace Consilient.Users.Services
{
    public class PasswordPolicyOptions
    {
        public bool RequireDigit { get; set; } = true;
        public int RequiredLength { get; set; } = 8;
        public bool RequireNonAlphanumeric { get; set; } = false;
        public bool RequireUppercase { get; set; } = true;
        public bool RequireLowercase { get; set; } = true;
        public int RequiredUniqueChars { get; set; } = 1;
    }
}
namespace Login.Core.Configuration
{
    public class AuthenticationConfiguration
    {
        public AuthenticationConfiguration()
        {}

        public string CookieName { get; set; }
        public string GoogleClientId { get; set; }
        public string GoogleClientSecret { get; set; }
    }
}

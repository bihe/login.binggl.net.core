namespace Login.Web.Infrastructure.Configuration
{
    public class AuthenticationConfiguration
    {
        public AuthenticationConfiguration()
        {}

        public string CookieName { get; set; }
        public int CookieExpiryDays { get; set; }
        public string GoogleClientId { get; set; }
        public string GoogleClientSecret { get; set; }
    }
}

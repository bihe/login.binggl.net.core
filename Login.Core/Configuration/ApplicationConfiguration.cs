namespace Login.Core.Configuration
{
    public class ApplicationConfiguration
    {
        public ApplicationConfiguration()
        { }

        public AuthenticationConfiguration Authentication { get; set; }
        public JwtConfiguration Jwt { get; set; }
        public string ApplicationSalt { get; set; }
    }
}

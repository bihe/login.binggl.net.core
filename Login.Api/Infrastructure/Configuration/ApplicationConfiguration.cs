namespace Login.Api.Infrastructure.Configuration
{
    public class ApplicationConfiguration
    {
        public ApplicationConfiguration()
        { }

        public string Name { get; set; }
        public string BaseUrl { get; set; }
        public AuthenticationConfiguration Authentication { get; set; }
        public JwtConfiguration Jwt { get; set; }
        public string ApplicationSalt { get; set; }
    }
}

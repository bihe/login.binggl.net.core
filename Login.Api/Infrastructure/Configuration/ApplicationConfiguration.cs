using Commons.Api.Configuration;

namespace Login.Api.Infrastructure.Configuration
{
    public class ApplicationConfiguration : BaseApplicationConfiguration
    {
        public ApplicationConfiguration()
        { }

        public AuthenticationConfiguration Authentication { get; set; }
        public JwtConfiguration Jwt { get; set; }
    }
}

using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Jose;
using Login.Core.Middleware;
using Newtonsoft.Json;
using Xunit;

namespace Login.XTests.Jwt
{
    public class JwtTests
    {
        [Fact]
        public async Task TestJwtSigning()
        {
            var payload = new Dictionary<string, object>()
            {
                { "sub", "a.b@c.de" },
                { "exp", 1300819380L }
            };

            var tokenSecretKey = Encoding.UTF8.GetBytes("TokenSecretKey");

            var token = JWT.Encode(payload, tokenSecretKey, JwsAlgorithm.HS256);

            Assert.NotNull(token);

            string json = JWT.Decode(token, tokenSecretKey);

            var obj = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

            Assert.NotNull(obj);
            Assert.Equal(payload["sub"], obj["sub"]);
            Assert.Equal(payload["exp"], obj["exp"]);

            await Task.FromResult(1);
        }

        [Fact]
        public async Task TestJwtMiddleware()
        {
            // MockMock

            var middleware = new JwtProcessor(null /*Microsoft.AspNetCore.Http.RequestDelegate*/,
                null /*Microsoft.Extensions.Logging.ILogger*/,
                null /*Login.Core.Services.ILoginService*/,
                null /*Microsoft.Extensions.Options.IOptions<Core.Configuration>*/);

            await middleware.Invoke(null /*Microsoft.AspNetCore.Http.HttpContext*/);

            // check
        }

    }
}

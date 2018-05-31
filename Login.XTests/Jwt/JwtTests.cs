using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Jose;
using Newtonsoft.Json;
using Xunit;

using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;

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

        //[Fact]
        //public async Task TestJwtMiddleware()
        //{
        //    var email = "a.b@c.de";
        //    var username = "tuser";
        //    var displayName = "Test User";
        //    var user = new User
        //    {
        //        Created = DateTime.Now,
        //        DisplayName = displayName,
        //        Email = email,
        //        Modified = DateTime.Now,
        //        Name = username,
        //        Sites = new List<UserSite>()
        //        {
        //            new UserSite
        //            {
        //                Created = DateTime.Now,
        //                Modified = DateTime.Now,
        //                Name = "testsite",
        //                Url = "http://www.test.com",
        //                PermissionList = "user"
        //            }
        //        }
        //    };

        //    // MockMock
        //    var mockHttpContext = new Mock<DefaultHttpContext>();

        //    var mockLogger = new Mock<ILogger<JwtProcessor>>();

        //    var mockLoginService = new Mock<ILoginService>();
        //    mockLoginService
        //        .Setup(x => x.GetUserByEmail(email, true))
        //        .Returns(Task.FromResult(user));

        //    mockLoginService
        //        .Setup(x => x.SaveLoginSession(username, displayName, Core.Enums.LoginType.DIRECT))
        //        .Returns(Task.FromResult(0));

        //    var middleware = new JwtProcessor(next: (innerHttpContext) => Task.FromResult(0),
        //        logger: mockLogger.Object,
        //        loginService: mockLoginService.Object,
        //        jwtCookieDomain: "domain",
        //        jwtCookieExpiryDays: 7,
        //        jwtCookieName: "name",
        //        jwtCookiePath: "/",
        //        tokenSecret: "12345678");

        //    await middleware.Invoke(mockHttpContext.Object);


        //    mockLoginService.Verify(service => service.SaveLoginSession(username, displayName, Core.Enums.LoginType.DIRECT), Times.AtMostOnce);


        //    // check
        //}

    }
}

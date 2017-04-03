using System;
using System.Text;
using System.Threading.Tasks;
using Jose;
using Login.Common.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Login.Common.Middleware
{
  public class JwtProcessor
    {
        readonly RequestDelegate next;
        readonly ILogger logger;
        readonly string tokenSecret;
        readonly string jwtCookieName;
        readonly string jwtCookieDomain;
        readonly string jwtCookiePath;
        readonly int jwtCookieExpiryDays;

        public JwtProcessor(RequestDelegate next, ILogger<JwtProcessor> logger, IOptions<ApplicationConfiguration> appConfig)
        {
            this.next = next;
            this.logger = logger;
            this.tokenSecret = appConfig?.Value?.Jwt?.TokenSecret ?? "";
            this.jwtCookieName = appConfig?.Value?.Jwt?.CookieName ?? "";
            this.jwtCookieDomain = appConfig?.Value?.Jwt?.CookieDomain ?? "";
            this.jwtCookiePath = appConfig?.Value?.Jwt?.CookiePath ?? "";
            this.jwtCookieExpiryDays = appConfig?.Value?.Jwt?.CookieExpiryDays ?? 0;
        }

        public JwtProcessor(RequestDelegate next, ILogger<JwtProcessor> logger, string tokenSecret, string jwtCookieName, string jwtCookieDomain, string jwtCookiePath, int jwtCookieExpiryDays)
        {
            this.next = next;
            this.logger = logger;
            this.tokenSecret = tokenSecret;
            this.jwtCookieName = jwtCookieName;
            this.jwtCookieDomain = jwtCookieDomain;
            this.jwtCookiePath = jwtCookiePath;
            this.jwtCookieExpiryDays = jwtCookieExpiryDays;
        }

        public async Task Invoke(HttpContext context /* other scoped dependencies */)
        {
            logger.LogInformation($"Processing middleware: [JwtProcessor]");

            try
            {
                if (context.User != null && context.User.Identity != null && context.User.Identity.IsAuthenticated)
                {
                    // the authentication middleware did it's job
                    // verify if there is a specific cookie available

                    var cookie = context.Request.Cookies[this.jwtCookieName];
                    if (string.IsNullOrEmpty(cookie))
                    {
                        logger.LogInformation($"Cookie '{this.jwtCookieName}' not available - create a new one!");

                        var tokenSecretKey = Encoding.UTF8.GetBytes(this.tokenSecret);

                        // TODO: create the payload object
                        // task of the repository

                        var token = JWT.Encode("<payload>", tokenSecretKey, JwsAlgorithm.HS256);

                        var expires = new DateTimeOffset(DateTime.Now.AddDays(this.jwtCookieExpiryDays));

                        context.Response.Cookies.Append(this.jwtCookieName, token, new CookieOptions
                        {
                            HttpOnly = true,
                            Domain = this.jwtCookieDomain,
                            Expires = expires,
                            Path = this.jwtCookiePath
                        });
                    }
                }
            }
            catch (Exception EX)
            {
                logger.LogError(500, EX, $"Error during processing of middleware!");
            }

            await next(context);
        }
    }
}
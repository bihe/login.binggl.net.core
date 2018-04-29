using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Jose;
using Login.Web.Features.Shared.Models;
using Login.Web.Features.User;
using Login.Web.Infrastructure.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Login.Web.Infrastructure.Middleware
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

        public async Task Invoke(HttpContext context, ILoginService loginService)
        {
            logger.LogInformation($"Processing middleware: 'JwtProcessor' for Request [Method: {context.Request.Method}, Path: {context.Request.Path}]");

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

                        List<Claim> claims = new List<Claim>(context.User.Claims);

                        var tokenSecretKey = Encoding.UTF8.GetBytes(this.tokenSecret);
                        var userEmail = claims.Where(x => x.Type == ClaimTypes.Email).FirstOrDefault()?.Value;
                        var user = await loginService.GetUserByEmail(userEmail);

                        if(user != null)
                        {
                            var query = from u in user.Sites select (u.Name + "|" + u.Url + "|" + u.PermissionList);
                            var expires = new DateTimeOffset(DateTime.Now.AddDays(this.jwtCookieExpiryDays));
                            var payload = new JwtPayload
                            {
                                jti = Guid.NewGuid().ToString("N"),
                                iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                                iss = "login.binggl.net",
                                exp = expires.ToUnixTimeSeconds(),
                                sub = "login.User",
                                Type = "login.User", // backwards compatibility
                                Claims = query.ToList(),
                                DisplayName = user.DisplayName,
                                Email = user.Email,
                                UserId = claims.Where(x => x.Type == ClaimTypes.Sid).FirstOrDefault()?.Value,
                                UserName = user.Name,
                                GivenName = claims.Where(x => x.Type == ClaimTypes.GivenName).FirstOrDefault()?.Value,
                                Surname = claims.Where(x => x.Type == ClaimTypes.Surname).FirstOrDefault()?.Value
                            };
                            var token = JWT.Encode(payload, tokenSecretKey, JwsAlgorithm.HS256);

                            var type = LoginType.DIRECT;
                            if(context.Request.Path.HasValue)
                            {
                                if(context.Request.Path.Value.IndexOf("auth/flow") > -1)
                                {
                                    type = LoginType.FORWARD;
                                }
                            }

                            // save the login process
                            await loginService.SaveLoginSession(user.Name, user.DisplayName, type);

                            context.Response.Cookies.Append(this.jwtCookieName, token, new CookieOptions
                            {
                                HttpOnly = true,
#if DEBUG
                                Secure = false,
#else
                                Secure = true,
#endif
                                Domain = this.jwtCookieDomain,
                                Expires = expires,
                                Path = this.jwtCookiePath
                            });

                        }
                        else
                        {
                            this.logger.LogWarning($"Could not get user '{userEmail}' from database!");
                        }
                    }
                }
                else
                {
                    logger.LogWarning($"User is not authenticated!");
                }
            }
            catch (Exception EX)
            {
                logger.LogError(500, EX, $"Error during processing of middleware!");
                throw;
            }

            await next(context);
        }
    }

    public static class JwtProcessorMiddlewareExtension
    {
        public static IApplicationBuilder UseJwtProcessor(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<JwtProcessor>();
        }
    }
}

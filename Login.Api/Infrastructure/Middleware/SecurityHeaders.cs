using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Login.Web.Infrastructure.Middleware
{
    public class SecurityHeaders
    {
        readonly RequestDelegate _next;
        readonly SecurityHeaderOptions _options;
        static readonly Dictionary<string,string> HttpSecurityHeaders = new Dictionary<string, string>
        {
            { "X-Frame-Options", "SAMEORIGIN"},
            { "X-XSS-Protection", "1; mode=block"},
            { "Access-Control-Allow-Origin", "APP_BASE_URL"},
            { "Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload"},
            { "Referrer-Policy", "same-origin"},
#if DEBUG
            { "Content-Security-Policy", "default-src APP_BASE_URL; script-src APP_BASE_URL 'unsafe-inline' 'unsafe-eval'; style-src APP_BASE_URL 'unsafe-inline'; img-src APP_BASE_URL 'self' data:"},
#else
            { "Content-Security-Policy", "default-src APP_BASE_URL; script-src APP_BASE_URL 'unsafe-inline'; style-src APP_BASE_URL 'unsafe-inline'; img-src APP_BASE_URL 'self' data:"},
#endif
        };

        public SecurityHeaders(RequestDelegate next, SecurityHeaderOptions options)
        {
            this._next = next;
            this._options = options;
        }

        public async Task Invoke(HttpContext context /* other scoped dependencies */)
        {
            foreach (var key in HttpSecurityHeaders.Keys)
            {
                if (!context.Response.Headers.ContainsKey(key))
                {
                    var headerValue = HttpSecurityHeaders[key];
                    if (key == "Access-Control-Allow-Origin" || key == "Content-Security-Policy")
                    {
                        headerValue = headerValue.Replace("APP_BASE_URL", this._options.ApplicationBaseUrl);
                    }
                    context.Response.Headers.Add(key, headerValue);
                }
            }

            await _next(context);
        }
    }

    public class SecurityHeaderOptions
    {
        public string ApplicationBaseUrl { get; set; }
    }

    public static class SecurityHeadersMiddlewareExtension
    {
        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder, Action<SecurityHeaderOptions> headerOptions)
        {
            var options = new SecurityHeaderOptions();
            headerOptions(options);
            return builder.UseMiddleware<SecurityHeaders>(options);
        }
    }
}

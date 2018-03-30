using System;
using System.Threading.Tasks;
using Login.Core.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Login.Core.Middleware
{
    public class SecurityHeaders
    {
        readonly RequestDelegate _next;

        public SecurityHeaders(RequestDelegate next)
        {
            this._next = next;
        }

        public async Task Invoke(HttpContext context /* other scoped dependencies */)
        {
            await _next(context);
        }
    }

    public static class SecurityHeadersMiddlewareExtension
    {
        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SecurityHeaders>();
        }
    }
}

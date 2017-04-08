using System;
using System.Threading.Tasks;
using Login.Core.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Login.Core.Middleware
{
    public class ErrorHandling
    {
        private readonly RequestDelegate next;
        private readonly ILogger logger;
        private IFlashService flash;
        private IMessageIntegrity messageIntegrity;
        

        public ErrorHandling(RequestDelegate next, IFlashService flash, IMessageIntegrity messageIntegrity, ILogger<ErrorHandling> logger)
        {
            this.next = next;
            this.flash = flash;
            this.logger = logger;
            this.messageIntegrity = messageIntegrity;
        }

        public async Task Invoke(HttpContext context /* other scoped dependencies */)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                logger.LogError($"Error during request '{ex}'. Req [Method: {context.Request.Method}, Host: {context.Request.Host}, Path: {context.Request.Path}]");
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var message = exception.Message;
            if(exception.InnerException != null)
            {
                message = exception.InnerException.Message;
            }

            var key = messageIntegrity.Encode(context.TraceIdentifier);
            flash.Set(key, message);
            context.Response.Redirect($"/error/{key}");

            return Task.FromResult(0);
        }
    }

    public static class ErrorHandlingMiddlewareExtension
    {
        public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorHandling>();
        }
    }
}

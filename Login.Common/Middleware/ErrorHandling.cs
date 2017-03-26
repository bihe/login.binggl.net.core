using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace Login.Common.Middleware
{
    public class ErrorHandling
    {
        private readonly RequestDelegate next;

        public ErrorHandling(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context /* other scoped dependencies */)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var message = exception.Message;
            if(exception.InnerException != null)
            {
                message = exception.InnerException.Message;
            }

            byte[] encodedBytes =Encoding.UTF8.GetBytes(message);
            string encodedString = Convert.ToBase64String(encodedBytes);

            context.Response.Redirect($"/error/{encodedString}");

            return Task.FromResult(0);
        }
    }
}

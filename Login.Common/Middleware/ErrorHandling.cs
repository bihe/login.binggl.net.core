﻿using System;
using System.Threading.Tasks;
using Login.Contracts.Services;
using Microsoft.AspNetCore.Http;

namespace Login.Common.Middleware
{
  public class ErrorHandling
    {
        private readonly RequestDelegate next;
        private IFlashService flash;
        private IMessageIntegrity messageIntegrity;

        public ErrorHandling(RequestDelegate next, IFlashService flash, IMessageIntegrity messageIntegrity)
        {
            this.next = next;
            this.flash = flash;
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
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Login.Web.Features.Shared.Exceptions
{
    public class ApplicationException : Exception
    {
        public int StatusCode { get; set; }

        public ApplicationException(string message) : base(message)
        {}

        public ApplicationException(string message, Exception inner) : base(message, inner)
        {}

        public ApplicationException(string message, int statusCode) : base(message)
        {
            this.StatusCode = statusCode;
        }
    }
}

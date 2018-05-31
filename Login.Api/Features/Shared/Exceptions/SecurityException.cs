using System;
using System.Collections.Generic;
using System.Text;

namespace Login.Api.Features.Shared.Exceptions
{
    public class SecurityException : ApplicationException
    {
        public SecurityException(string message) : base(message, 403)
        { }
    }
}

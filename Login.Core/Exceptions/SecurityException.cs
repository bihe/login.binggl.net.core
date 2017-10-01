using System;
using System.Collections.Generic;
using System.Text;

namespace Login.Core.Exceptions
{
    public class SecurityException : ApplicationException
    {
        public SecurityException(string message) : base(message, 403)
        { }
    }
}

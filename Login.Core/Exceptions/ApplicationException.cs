using System;
using System.Collections.Generic;
using System.Text;

namespace Login.Core.Exceptions
{
    public class ApplicationException : Exception
    {
        public ApplicationException(string message) : base(message)
        {
        }

        public ApplicationException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}

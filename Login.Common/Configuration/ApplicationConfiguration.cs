using System;
using System.Collections.Generic;
using System.Text;

namespace Login.Common.Configuration
{
    public class ApplicationConfiguration
    {
        public ApplicationConfiguration()
        { }

        public AuthenticationConfiguration Authentication { get; set; }
        public string Secret { get; set; }
    }
}

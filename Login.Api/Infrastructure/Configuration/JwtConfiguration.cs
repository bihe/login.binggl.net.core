using System;
using System.Collections.Generic;
using System.Text;

namespace Login.Web.Infrastructure.Configuration
{
    public class JwtConfiguration
    {
        public JwtConfiguration()
        {}

        public string TokenSecret { get; set; }
        public string CookieName { get; set; }
        public string CookieDomain { get; set; }
        public string CookiePath { get; set; }
        public int CookieExpiryDays { get; set; }
    }
}

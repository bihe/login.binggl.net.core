using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Builder;

namespace Login.Core.Middleware.Authentication
{
    public class CustomCookieAuthenticationOptions : CookieAuthenticationOptions
    {
        public CustomCookieAuthenticationOptions() : base()
        {}

        public string JwtCookieName { get; set; }
        public string JwtTokenSecret { get; set; }
    }
}

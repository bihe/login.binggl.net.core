﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Login.Common.Configuration
{
    public class AuthenticationConfiguration
    {
        public AuthenticationConfiguration()
        {}

        public string CookieName { get; set; }
        public string GoogleClientId { get; set; }
        public string GoogleClientSecret { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Login.Core.Configuration
{
    public static class Constants
    {
        public const string APP_NAME = "App.Name";

        public const string ROLE_USER = "User";

        public const string APP_VERSION = "App.Version";

        public const string AUTHENTICATION_SCHEME_COOKIES = "LoginCookieMiddleware";

        public const string AUTHENTICATION_SCHEME_EXTERNAL_OAUTH = "oidc";
    }
}

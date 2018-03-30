using System;
using System.Collections.Generic;
using System.Text;

namespace Login.Web.Features.Api.ViewModels
{
    public enum LoginState
    {
        Error = 0,
        Success
    }

    public class LoginInfo
    {
        public LoginState State { get; set; }
        public string Error { get; set; }
        public string Success { get; set; }
    }
}

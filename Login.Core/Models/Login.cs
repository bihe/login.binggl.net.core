using System;
using System.Collections.Generic;
using System.Text;

namespace Login.Core.Models
{
    public class Login : BaseModel
    {
        public Login()
        {}

        public int Id { get; set; }
        public string UserDisplayName { get; set; }
        public string UserName { get; set; }
        public LoginType Type { get; set; }
    }
}

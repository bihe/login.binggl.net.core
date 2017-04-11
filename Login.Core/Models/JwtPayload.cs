using System;
using System.Collections.Generic;
using System.Text;

namespace Login.Core.Models
{
    public class JwtPayload
    {
        public JwtPayload()
        {
            this.Claims = new List<string>();
        }

        public string Type { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public List<string> Claims { get; set; }
        public string UserId { get; set; }
        public string DisplayName { get; set; }
        public DateTime Issued { get; set; }
        public string Surname { get; set; }
        public string GivenName { get; set; }
    }
}

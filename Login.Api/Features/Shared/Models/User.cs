using System;
using System.Collections.Generic;
using System.Text;

namespace Login.Web.Features.Shared.Models
{
    public class User : BaseModel
    {
        public User()
        {
            this.Sites = new List<UserSite>();
        }

        public string Name { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public List<UserSite> Sites { get; set; }

        public byte[] Timestamp { get; set; }


        public override string ToString()
        {
            return $"{DisplayName} ({Name})";
        }
    }
}

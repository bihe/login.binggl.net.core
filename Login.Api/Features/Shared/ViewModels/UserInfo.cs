using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Login.Web.Features.Shared.ViewModels
{
    public class UserInfo
    {
        public UserInfo()
        {
            this.SitePermissions = new List<SiteInfo>();
        }

        public string ThisSite { get; set; }
        public bool Editable { get; set; } = false;
        public string Id { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public string UserName { get; set; }
        public List<SiteInfo> SitePermissions { get; set; }
    }
}

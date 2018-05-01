using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Login.Api.Features.Shared.ViewModels
{
    public class SiteInfo
    {
        public SiteInfo()
        {
            this.Permissions = new List<string>();
        }

        public string Name { get; set; }
        public string Url { get; set; }
        public List<string> Permissions { get; set; }
    }
}

using Models = Login.Api.Features.Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Login.Api.Features.User
{
    public interface ILoginService
    {
        Task<Models.User> GetUserByEmail(string email, bool noCache=false);

        Task<Models.UserSite> GetSiteByName(string siteName, bool noCache = false);

        Task SaveLoginSession(string username, string displayname, Models.LoginType loginType);

        bool IsValidRedirectUrl(Models.User user, String siteName, String redirectUrl);
    }
}

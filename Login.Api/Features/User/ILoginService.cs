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

        Task<Models.UserSite> GetSiteById(string id, bool noCache = false);

        void SaveLoginSession(string username, string displayname, Models.LoginType loginType);

        bool IsValidRedirectUrl(Models.User user, String siteName, String redirectUrl);

        bool SaveSiteList(List<Models.UserSite> sitesToSave, List<Models.UserSite> sitesToDelete);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Login.Api.Features.Shared.Api;
using Login.Api.Features.Shared.ViewModels;
using Login.Api.Infrastructure.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Login.Api.Features.User
{
    [Authorize]
    [Route("api/v1/users")]
    [ApiController]
    public class UserApiController : AbstractApiController
    {
        private readonly ILogger _logger;
        private readonly ILoginService _loginService;
        private readonly ApplicationConfiguration _appConfig;

        public UserApiController(ILogger<UserApiController> logger, ILoginService loginService, IOptions<ApplicationConfiguration> appConfig)
        {
            this._loginService = loginService;
            this._logger = logger;
            this._appConfig = appConfig.Value;
        }

        [HttpGet]
        public async Task<ActionResult<UserInfo>> Get()
        {
            _logger.LogDebug($"The current user is `{this.User.Identity.Name}`; User has Admin-Role? `{this.User.IsInRole(Constants.ROLE_ADMIN)}`");

            var user = await this._loginService.GetUserByEmail(this.AuthenticatedUserEmail);
            return GetUserInfo(user);
        }

        [HttpPost]
        public async Task<ActionResult<UserInfo>> Save([FromBody] UserInfo payload)
        {
            _logger.LogDebug($"The current user is `{this.User.Identity.Name}`; User has Admin-Role? `{this.User.IsInRole(Constants.ROLE_ADMIN)}`");

            var user = await this._loginService.GetUserByEmail(email: this.AuthenticatedUserEmail, noCache: true);
            if (user is null)
            {
                return NotFound("Could not find the user!");
            }

            if (payload.SitePermissions != null && payload.SitePermissions.Count > 0)
            {
                var sites = new List<Shared.Models.UserSite>();

                var idsInList = from p in payload.SitePermissions where !string.IsNullOrEmpty(p.Id) select p.Id;
                var sitesInList = from p in payload.SitePermissions where !string.IsNullOrEmpty(p.Id) select p;
                var newSites = from p in payload.SitePermissions where string.IsNullOrEmpty(p.Id) select p;
                var existingIds = from s in user.Sites select s.Id;
                var missingIds = existingIds.Except(idsInList);

                foreach (var site in sitesInList)
                {
                    var foundSite = await this._loginService.GetSiteById(site.Id, noCache: false);
                    if (foundSite != null)
                    {
                        foundSite.Name = site.Name;
                        foundSite.Url = site.Url;
                        foundSite.Permissions = site.Permissions;
                        sites.Add(foundSite);
                    }
                }

                foreach (var site in newSites)
                {
                    var newSite = new Shared.Models.UserSite();
                    newSite.Name = site.Name;
                    newSite.Url = site.Url;
                    newSite.Permissions = site.Permissions;
                    newSite.User = user;

                    sites.Add(newSite);
                }

                var sitesToDelete = new List<Shared.Models.UserSite>();
                foreach (var id in missingIds)
                {
                    var site = await this._loginService.GetSiteById(id, noCache: false);
                    if (site != null) sitesToDelete.Add(site);
                }

                var result = this._loginService.SaveSiteList(sites, sitesToDelete);
                user = await this._loginService.GetUserByEmail(email: this.AuthenticatedUserEmail, noCache: true);
            }

            return GetUserInfo(user);;
        }

        UserInfo GetUserInfo(Shared.Models.User user)
        {
            var sitePermissions = from u in user.Sites select new SiteInfo() { Id = u.Id, Name = u.Name, Url = u.Url, Permissions = u.Permissions };
            var userInfo = new UserInfo
            {
                ThisSite = this._appConfig.Name,
                Editable = this.User.IsInRole(Constants.ROLE_ADMIN) ? true: false,
                DisplayName = user.DisplayName,
                Email = user.Email,
                Id = Hash(user.Email),
                UserName = user.Name,
                SitePermissions = new System.Collections.Generic.List<SiteInfo>(sitePermissions)
            };
            return userInfo;
        }
    }
}

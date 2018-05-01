using System;
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
            var sitePermissions = from u in user.Sites select new SiteInfo() { Name = u.Name, Url = u.Url, Permissions = u.Permissions };
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

        [HttpPost]
        public async Task<ActionResult<UserInfo>> Save([FromBody] UserInfo payload)
        {
            return null;
        }
    }

}

using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Login.Core.Configuration;
using Login.Core.Services;
using Login.Web.Features.Shared.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Login.Web.Features.Api
{
    [Authorize]
    [Route("api/v1/users")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly ILoginService _loginService;
        private readonly ApplicationConfiguration _appConfig;

        public ApiController(ILogger<ApiController> logger, ILoginService loginService, IOptions<ApplicationConfiguration> appConfig)
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

        string AuthenticatedUserEmail
        {
            get
            {
                return this.User.Identity.IsAuthenticated ? this.User.Claims.Where(x => x.Type == ClaimTypes.Email).FirstOrDefault()?.Value : "";
            }
        }

        string Hash(string value)
        {
            using (var algorithm = SHA256.Create())
            {
                var hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(value));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
    }
}

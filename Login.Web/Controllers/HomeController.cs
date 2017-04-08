using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

using Login.Core.Configuration;
using Login.Core.Services;
using Login.Web.ViewModels;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using System;

namespace Login.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHtmlLocalizer<HomeController> localizer;
        private readonly ILogger logger;
        private IFlashService flash;
        private IMessageIntegrity messageIntegrity;
        private ILoginService loginService;

        public HomeController(IHtmlLocalizer<HomeController> localizer, ILogger<HomeController> logger, IFlashService flash, IMessageIntegrity messageIntegrity, ILoginService loginService)
        {
            this.localizer = localizer;
            this.logger = logger;
            this.flash = flash;
            this.messageIntegrity = messageIntegrity;
            this.loginService = loginService;
        }


        [Authorize]
        public IActionResult Index()
        {
            this.CommonViewData();

            logger.LogDebug("The current user is {0}", this.User.Identity.Name);

            var loginInfo = new LoginInfo
            {
                State = LoginState.Success
            };

            return View(loginInfo);
        }

        [Route("api/user/{nocache?}")]
        [Authorize]
        public async Task<IActionResult> CurrentUser(bool nocache)
        {
            var user = await this.loginService.GetUserByEmail(this.AuthenticatedUserEmail, nocache);
            if(user == null)
            {
                // throw something;
            }

            var sitePermissions = from u in user.Sites select new SiteInfo() { Name = u.Name, Url = u.Url, Permissions = u.Permissions };

            return Json(new UserInfo
            {
                DisplayName = user.DisplayName,
                Email = user.Email,
                Id = Hash(user.Email),
                UserName = user.Name,
                SitePermissions = new System.Collections.Generic.List<SiteInfo>(sitePermissions)
            });
        }

        [Route("auth/flow")]
        [Authorize]
        public async Task<IActionResult> AuthenticationFlow()
        {
            var user = await this.loginService.GetUserByEmail(this.AuthenticatedUserEmail);
            if (user == null)
            {
                // throw something;
            }

            return Json(new UserInfo
            {
                DisplayName = user.DisplayName,
                Email = user.Email,
                Id = Hash(user.Email),
                UserName = user.Name,
            });
        }

        [Route("error/{key?}")]
        public IActionResult Error(string key)
        {
            this.CommonViewData();
            var message = "";
            if (string.IsNullOrEmpty(key))
                message = "Error in user authorization!";
            else
            {
                if (this.messageIntegrity.Verify(key))
                { 
                    message = this.flash.Get(key);
                    message = System.Net.WebUtility.HtmlEncode(message);
                }
            }

            var loginInfo = new LoginInfo
            {
                State = LoginState.Error,
                Error = string.Format(localizer["auth_login_error"].Value, message)
            };

            return View(loginInfo);
        }


        void CommonViewData()
        {
            ViewData[Constants.APP_NAME] = "login.binggl.net";
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
                // Create the at_hash using the access token returned by CreateAccessTokenAsync.
                var hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(value));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

    }
}

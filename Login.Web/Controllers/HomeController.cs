using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Login.Core.Configuration;
using Login.Core.Exceptions;
using Login.Core.Services;
using Login.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Login.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHtmlLocalizer<HomeController> localizer;
        private readonly ILogger logger;
        private readonly IOptions<ApplicationConfiguration> appConfig;
        private IFlashService flash;
        private IMessageIntegrity messageIntegrity;
        private ILoginService loginService;

        public HomeController(IHtmlLocalizer<HomeController> localizer, ILogger<HomeController> logger, 
            IFlashService flash, IMessageIntegrity messageIntegrity, ILoginService loginService, 
            IOptions<ApplicationConfiguration> appConfig)
        {
            this.localizer = localizer;
            this.logger = logger;
            this.flash = flash;
            this.messageIntegrity = messageIntegrity;
            this.loginService = loginService;
            this.appConfig = appConfig;
        }

        [Route("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            this.CommonViewData();

            var cookies = HttpContext.Request.Cookies.Keys;
            foreach (var cookie in cookies)
            {
                HttpContext.Response.Cookies.Delete(cookie, new CookieOptions
                {
                    Domain = appConfig.Value.Jwt.CookieDomain
                });
            }

            await HttpContext.Authentication.SignOutAsync(Constants.AUTH_SCHEME);

            var loginInfo = new LoginInfo
            {
                State = LoginState.Success,
                Success = localizer["auth_logout"].Value
            };

            return View("Error", loginInfo);
        }

        [Route("api/user/{nocache?}")]
        [Authorize]
        public async Task<IActionResult> CurrentUser(bool nocache)
        {
            var user = await this.loginService.GetUserByEmail(this.AuthenticatedUserEmail, nocache);
            if(user == null)
            {
                throw new ApplicationException("No user is available!");
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
        public async Task<IActionResult> AuthenticationFlow([FromQuery(Name = "~site")] string site, [FromQuery(Name = "~url")] string url)
        {
            if(string.IsNullOrEmpty(site) || string.IsNullOrEmpty(url))
            {
                throw new ApplicationException("Invalid paramters supplied for auth-flow!");
            }
            
            var user = await this.loginService.GetUserByEmail(this.AuthenticatedUserEmail);
            if (user == null)
            {
                throw new ApplicationException("No user is available!");
            }

            if (!this.loginService.IsValidRedirectUrl(user, site, url))
            {
                var loginInfo = new LoginInfo
                {
                    State = LoginState.Error,
                    Error = string.Format(localizer["auth_flow_redirect_error"].Value, url)
                };

                return View("Error", loginInfo); ;
            }

            return Redirect(url);
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
                var hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(value));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

    }
}

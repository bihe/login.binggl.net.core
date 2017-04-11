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
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Login.Web.Controllers
{
    [Authorize(ActiveAuthenticationSchemes = Constants.AUTH_SCHEME)]
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
                Success = localizer["The user was successfully loged out!"].Value
            };

            return View("Error", loginInfo);
        }

        [Route("api/user/{nocache?}")]
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
        [AllowAnonymous]
        public async Task<IActionResult> AuthenticationFlow([FromQuery(Name = "~site")] string site, [FromQuery(Name = "~url")] string url)
        {
            if(string.IsNullOrEmpty(site) || string.IsNullOrEmpty(url))
            {
                throw new ApplicationException("Invalid paramters supplied for auth-flow!");
            }

            // if no authenticated user email is available - challenge the user by an auth request
            if (string.IsNullOrEmpty(this.AuthenticatedUserEmail))
            {
                var props = new AuthenticationProperties
                {
                    // redirect where I came from
                    RedirectUri = $"/auth/flow?~site={site}&~url={url}"
                };
                return new ChallengeResult(Constants.AUTH_OAUTH_SCHEME, props);
            }
            else
            {
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
                        Error = string.Format(localizer["Could not redirect to the desired url {0}."].Value, url)
                    };
                    return View("Error", loginInfo); ;
                }
                return Redirect(url);
            }
        }

        [Route("error/{key?}")]
        [AllowAnonymous]
        public IActionResult Error(string key)
        {
            this.CommonViewData();
            var message = string.Format(localizer["Failed to login the user!"].Value);

            if (string.IsNullOrEmpty(key))
                message = localizer["No active user token available - login is needed!"].Value;
            else
            {
                if (this.messageIntegrity.Verify(key))
                { 
                    message = this.flash.Get(key);
                    //message = System.Net.WebUtility.HtmlEncode(message);
                }
            }

            var loginInfo = new LoginInfo
            {
                State = LoginState.Error,
                Error = message
            };

            return View(loginInfo);
        }

        [Route("login")]
        [AllowAnonymous]
        public IActionResult Login()
        {
            var props = new AuthenticationProperties
            {
                RedirectUri = "/"
            };

            return new ChallengeResult(Constants.AUTH_OAUTH_SCHEME, props);
        }

        public async Task<IActionResult> Index()
        {
            this.CommonViewData();

            logger.LogDebug("The current user is {0}", this.User.Identity.Name);
            var user = await this.loginService.GetUserByEmail(this.AuthenticatedUserEmail);
            var sitePermissions = from u in user.Sites select new SiteInfo() { Name = u.Name, Url = u.Url, Permissions = u.Permissions };
            var userInfo = new UserInfo
            {
                DisplayName = user.DisplayName,
                Email = user.Email,
                Id = Hash(user.Email),
                UserName = user.Name,
                SitePermissions = new System.Collections.Generic.List<SiteInfo>(sitePermissions)
            };

            return View(userInfo);
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

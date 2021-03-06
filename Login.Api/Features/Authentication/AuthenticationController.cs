using System;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Login.Api.Features.Shared.ViewModels;
using Login.Api.Infrastructure.Configuration;
using Login.Api.Features.User;
using Commons.Api.FlashScope;
using Commons.Api.Messages;
using Commons.Api.Controller;

namespace Login.Api.Features.Authentication
{
    /// <summary>
    /// main controller of application
    /// </summary>
    [Authorize]
    public class AuthenticationController : AbstractMvcController
    {
        private static readonly string AssemblyVersion = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

        private readonly IHtmlLocalizer<AuthenticationController> localizer;
        private readonly ILogger logger;
        private readonly IOptions<ApplicationConfiguration> appConfig;
        private ILoginService loginService;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="logger"></param>
        /// <param name="flash"></param>
        /// <param name="messageIntegrity"></param>
        /// <param name="loginService"></param>
        /// <param name="appConfig"></param>
        public AuthenticationController(IHtmlLocalizer<AuthenticationController> localizer, ILogger<AuthenticationController> logger,
            IFlashService flash, IMessageIntegrity messageIntegrity, ILoginService loginService,
            IOptions<ApplicationConfiguration> appConfig) : base(logger, flash, messageIntegrity)
        {
            this.localizer = localizer;
            this.logger = logger;
            this.loginService = loginService;
            this.appConfig = appConfig;
        }

        /// <summary>
        /// show logoff page
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        [Route("logoff")]
        public IActionResult LogOff()
        {
            this.CommonViewData();

            var loginInfo = new LoginInfo
            {
                State = LoginState.Success,
                Success = localizer["The user was successfully loged out!"].Value
            };

            return View("Error", loginInfo);
        }

        /// <summary>
        /// perform logout
        /// </summary>
        /// <returns></returns>
        [Route("logout")]
        [HttpGet]
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

            if (HttpContext.User.Identity.IsAuthenticated)
            {
                await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }

            var loginInfo = new LoginInfo
            {
                State = LoginState.Success,
                Success = localizer["The user was successfully loged out!"].Value
            };

            return View("Error", loginInfo);
        }

        /// <summary>
        /// start the authtentication flow
        /// </summary>
        /// <param name="site"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("auth/flow")]
        [AllowAnonymous]
        public async Task<IActionResult> AuthenticationFlow([FromQuery(Name = "~site")] string site, [FromQuery(Name = "~url")] string url)
        {
            if(string.IsNullOrEmpty(site) || string.IsNullOrEmpty(url))
            {
                throw new Commons.Api.Exceptions.ApplicationException("Invalid paramters supplied for auth-flow!");
            }

            // if no authenticated user email is available - challenge the user by an auth request
            if (string.IsNullOrEmpty(this.AuthenticatedUserEmail))
            {
                var props = new AuthenticationProperties
                {
                    // redirect where I came from
                    RedirectUri = $"/auth/flow?~site={site}&~url={url}"
                };
                //return await HttpContext.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, props);
                return new ChallengeResult(OpenIdConnectDefaults.AuthenticationScheme, props);
            }
            else
            {
                var user = await this.loginService.GetUserByEmail(this.AuthenticatedUserEmail);
                if (user == null)
                {
                    throw new Commons.Api.Exceptions.ApplicationException("No user is available!");
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

        protected override ActionResult PrepareErrorResult(string key)
        {
            this.CommonViewData();
            var message = string.Format(localizer["Failed to login the user!"].Value);

            if (string.IsNullOrEmpty(key))
                message = localizer["No active user token available - login is needed!"].Value;
            else
            {
                if (_messageIntegrity.Verify(key))
                {
                    message = _flash.Get(key);
                }
            }

            return View(new LoginInfo
            {
                State = LoginState.Error,
                Error = message
            });
        }

        /// <summary>
        /// Perform a login
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("login")]
        [AllowAnonymous]
        public async Task Login()
        {
            var props = new AuthenticationProperties
            {
                RedirectUri = "/"
            };

            if (HttpContext.User == null || !HttpContext.User.Identity.IsAuthenticated)
            {
                await HttpContext.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, props);
            }
        }

        [HttpGet]
        [Route("relogin")]
        public async Task ReLogin()
        {
            if (HttpContext.User != null && HttpContext.User.Identity.IsAuthenticated)
            {
                // logoff first, clear authentication holders
                var cookies = HttpContext.Request.Cookies.Keys;
                foreach (var cookie in cookies)
                {
                    HttpContext.Response.Cookies.Delete(cookie, new CookieOptions
                    {
                        Domain = appConfig.Value.Jwt.CookieDomain
                    });
                }

                if (HttpContext.User.Identity.IsAuthenticated)
                {
                    await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                }

                var props = new AuthenticationProperties
                {
                    RedirectUri = "/"
                };

                await HttpContext.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, props);
            }
            else
            {
                HttpContext.Response.Redirect("/Error");
            }
        }


#if !BLAZOR
        [HttpGet]
        [Route("/")]
        [Route("/ui/{path?}/{subpath?}")]
        public IActionResult Index()
        {
            return Redirect("/ui/index.html");
        }
#endif

        void CommonViewData()
        {
            ViewData[Constants.APP_NAME] = this.appConfig.Value.Name;
            ViewData[Constants.APP_VERSION] = AssemblyVersion;
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

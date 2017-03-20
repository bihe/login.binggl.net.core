using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;

using Login.Common.Configuration;
using Login.Common.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Login.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHtmlLocalizer<HomeController> localizer;
        private readonly ILogger logger;

        public HomeController(IHtmlLocalizer<HomeController> localizer, ILogger<HomeController> logger)
        {
            this.localizer = localizer;
            this.logger = logger;
        }


        public IActionResult Index()
        {
            ViewData[Constants.APP_NAME] = "login.binggl.net";

            var loginInfo = new LoginInfo
            {
                State = LoginState.Error,
                Error = string.Format(localizer["auth_login_error"].Value, "Backend authentication system not setup!")
            };

            return View(loginInfo);
        }

        [Authorize]
        public IActionResult Secure()
        {
            logger.LogDebug("The current user is {0}", this.User.Identity.ToString());


            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}

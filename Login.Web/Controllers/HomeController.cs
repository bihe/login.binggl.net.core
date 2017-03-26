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
using Microsoft.AspNetCore.Diagnostics;

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

        [Route("error/{message?}")]
        public IActionResult Error(string message)
        {
            this.CommonViewData();

            if(string.IsNullOrEmpty(message))
                message = "Error in user authorization!";
            else
            {
                try
                {
                    byte[] decodedBytes = Convert.FromBase64String(message);
                    message = System.Text.Encoding.UTF8.GetString(decodedBytes);
                    message = System.Net.WebUtility.HtmlEncode(message);
                }
                catch(Exception)
                {
                    message = "Error in user authorization!";
                }
            }

            var loginInfo = new LoginInfo
            {
                State = LoginState.Error,
                Error = string.Format(localizer["auth_login_error"].Value, message)
            };

            return View("Index", loginInfo);
        }


        void CommonViewData()
        {
            ViewData[Constants.APP_NAME] = "login.binggl.net";
        }

    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;

using Login.Common.Configuration;
using Login.Common.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Login.Contracts.Services;

namespace Login.Web.Controllers
{
  public class HomeController : Controller
    {
        private readonly IHtmlLocalizer<HomeController> localizer;
        private readonly ILogger logger;
        private IFlashService flash;
        private IMessageIntegrity messageIntegrity;

        public HomeController(IHtmlLocalizer<HomeController> localizer, ILogger<HomeController> logger, IFlashService flash, IMessageIntegrity messageIntegrity)
        {
            this.localizer = localizer;
            this.logger = logger;
            this.flash = flash;
            this.messageIntegrity = messageIntegrity;
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

    }
}

using System;
using System.Linq;
using System.Reflection;
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
using Microsoft.Extensions.PlatformAbstractions;

namespace Login.Web.Features.Api
{
    [Authorize]
    [Route("api/v1/appinfo")]
    [ApiController]
    public class VersionController : AbstractApiController
    {
        private static readonly string _assemblyVersion = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        private readonly ILogger _logger;
        private readonly ApplicationConfiguration _appConfig;

        public VersionController(ILogger<ApiController> logger, IOptions<ApplicationConfiguration> appConfig)
        {
            this._logger = logger;
            this._appConfig = appConfig.Value;
        }

        [ResponseCache(Duration = 60)]
        [HttpGet]
        public ActionResult<AppInfo> Get()
        {
            var runtimeInfo = PlatformServices.Default.Application.RuntimeFramework;

            return new AppInfo {
                Version = _assemblyVersion,
                Runtime = runtimeInfo.FullName
            };
        }
    }

    public class AppInfo
    {
        public string Version { get; set; }
        public string Runtime { get; set; }
    }
}

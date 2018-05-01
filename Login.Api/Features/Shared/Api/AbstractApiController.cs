using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Login.Api.Features.Shared.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Login.Api.Features.Shared.Api
{
    public abstract class AbstractApiController : ControllerBase
    {
        protected string AuthenticatedUserEmail
        {
            get
            {
                return this.User.Identity.IsAuthenticated ? this.User.Claims.Where(x => x.Type == ClaimTypes.Email).FirstOrDefault()?.Value : "";
            }
        }

        protected string Hash(string value)
        {
            using (var algorithm = SHA256.Create())
            {
                var hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(value));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
    }
}

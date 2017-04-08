using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace Login.Core.Services
{
    public interface IAuthorization
    {
        /// <summary>
        /// validate the given token and add setup claimsprincipal
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task PerformPostTokenValidationAuthorization(TokenValidatedContext context);
    }
}

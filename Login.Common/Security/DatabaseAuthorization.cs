using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Login.Common.Configuration;
using Login.Contracts.Repository;
using Login.Contracts.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Login.Common.Security
{
    public class DatabaseAuthorization : IAuthorization
    {
        ILoginRepository repository;
        readonly ILogger logger;

        public DatabaseAuthorization(ILoginRepository repository, ILogger<DatabaseAuthorization> logger)
        {
            this.repository = repository;
            this.logger = logger;
        }

        public Task PerformPostTokenValidationAuthorization(TokenValidatedContext context)
        {   
            var claimsIdentity = context.Ticket.Principal.Identity as ClaimsIdentity;
            var externalLookupEmail = claimsIdentity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email).Value;

            logger.LogDebug("Final phase of token-validation; check supplied identiy with database entries. Supplied: {0}", externalLookupEmail);

            var awaiter = repository.GetUserByEmail(externalLookupEmail);
            awaiter.Wait();
            var lookupUser = awaiter.Result;

            logger.LogDebug("User from lookup: {0}", lookupUser);

            if(lookupUser == null)
            {
                logger.LogWarning("Could not find a user by email {0}", externalLookupEmail);
                //throw new SecurityTokenValidationException("Supplied user is not allowed to access the system!");
                return Task.FromException(new SecurityTokenValidationException("Supplied user is not allowed to access the system!"));
            }

            IList<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name,  claimsIdentity.Claims.FirstOrDefault(x => x.Type == "name")?.Value ?? "")
                , new Claim(ClaimTypes.Sid,  claimsIdentity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value ?? "")
                , new Claim(ClaimTypes.GivenName,  claimsIdentity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.GivenName)?.Value ?? "")
                , new Claim(ClaimTypes.Surname, claimsIdentity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Surname)?.Value ?? "")
                , new Claim(ClaimTypes.Email, claimsIdentity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value ?? "")
                , new Claim(ClaimTypes.Role, Constants.ROLE_USER)
            };

            logger.LogDebug("User {0} has {1} sites assigned.", externalLookupEmail, lookupUser?.Sites?.Count ?? 0);

            var identity = new ClaimsIdentity(claims, "Google" /*a.Ticket.AuthenticationScheme*/);
            var principal = new ClaimsPrincipal(identity);

            context.Ticket = new AuthenticationTicket(principal, context.Properties, "remote_system" /*a.Ticket.AuthenticationScheme*/);

            return Task.FromResult(0);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace Login.Web.Security
{
    public class Authorization
    {
        public Authorization()
        {

        }

        public Task TokenValidated(TokenValidatedContext context)
        {
            var claimsIdentity = context.Ticket.Principal.Identity as ClaimsIdentity;
            var externalLookupEmail = claimsIdentity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email).Value;

            // TODO: lookup email in database!

            IList<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name,  claimsIdentity.Claims.FirstOrDefault(x => x.Type == "name")?.Value ?? "")
                , new Claim(ClaimTypes.Sid,  claimsIdentity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value ?? "")
                , new Claim(ClaimTypes.GivenName,  claimsIdentity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.GivenName)?.Value ?? "")
                , new Claim(ClaimTypes.Surname, claimsIdentity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Surname)?.Value ?? "")
                , new Claim(ClaimTypes.Email, claimsIdentity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value ?? "")
                , new Claim(ClaimTypes.Role, "<ROLE>")
            };

            var identity = new ClaimsIdentity(claims, "Google" /*a.Ticket.AuthenticationScheme*/);
            var principal = new ClaimsPrincipal(identity);

            context.Ticket = new AuthenticationTicket(principal, context.Properties, "remote_system" /*a.Ticket.AuthenticationScheme*/);

            //if(!true)
            //{
            //    throw new SecurityTokenValidationException();
            //}

            return Task.FromResult(0);
        }
    }
}

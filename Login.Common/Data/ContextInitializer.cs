using System;
using System.Linq;

using Microsoft.EntityFrameworkCore;

using Login.Common.Data;

namespace Login.Common.Data
{
    public static class ContextInitializer
    {
        public static void InitialData(LoginContext context)
        {
            context.Database.EnsureCreated();

            if(context.Users.Any())
            {
                return;
            }


            var user1 = context.Users.Add(new Contracts.Models.User { Name = "User1", DisplayName = "User 1", Email = "user@user1.com" });

            context.SaveChanges();

            var site1 = context.UserSites.Add(new Contracts.Models.UserSite { Name = "Site1", Url = "http://www.site1.com", PermissionList = "Permission1;Permission2", User = user1.Entity });

            context.SaveChanges();
        }
    }
}

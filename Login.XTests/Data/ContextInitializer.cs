﻿using Login.Core.Data;

namespace Login.Tests.Data
{
  public static class ContextInitializer
    {
        public static void Initialize(LoginContext context, bool forceDropDatabase = false)
        {
            context.Database.EnsureCreated();

            if (forceDropDatabase)
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }

            context.Logins.Add(new Core.Models.Login { Type = Core.Enums.LoginType.DIRECT, UserName = "User1", UserDisplayName = "User 1" });
            context.Logins.Add(new Core.Models.Login { Type = Core.Enums.LoginType.FORWARD, UserName = "User2", UserDisplayName = "User 2" });

            context.SaveChanges();

            var user1 = context.Users.Add(new Core.Models.User { Name = "User1", DisplayName = "User 1", Email = "user1@site.com" });
            var user2 = context.Users.Add(new Core.Models.User { Name = "User2", DisplayName = "User 2", Email = "user2@site.com" });

            context.SaveChanges();

            var site1 = context.UserSites.Add(new Core.Models.UserSite { Name = "Site1", Url = "http://www.site1.com", PermissionList = "Permission1;Permission2", User = user1.Entity });
            var site2 = context.UserSites.Add(new Core.Models.UserSite { Name = "Site2", Url = "http://www.site2.com", PermissionList = "Permission2", User = user2.Entity });

            context.SaveChanges();


        }
    }
}
using System.Threading.Tasks;
using System.Collections.Generic;
using Login.Api.Features.Shared.Models;
using Login.Api.Features.Shared.Persistence;
using Login.Api.Features.User;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using Microsoft.Extensions.Logging;

namespace Login.XTests.Repository
{
    public class RepositoryTests
    {
        static DbContextOptions<LoginContext> options = new DbContextOptionsBuilder<LoginContext>()
                .UseInMemoryDatabase(databaseName: "Test_Login_Databases_Repository_Tests")
                // don't raise the error warning us that the in memory db doesn't support transactions
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

        [Fact]
        public async Task TestEmailLookup()
        {
            var user = await Repository.GetUserByEmail("user1@site.com");
            Assert.NotNull(user);
        }

        [Fact]
        public async Task TestSiteAndPermissionRelation()
        {
            var site = await Repository.GetSiteById("site1");
            Assert.NotNull(site);
            Assert.Equal("http://www.site1.com", site.Url);
            Assert.Equal("Permission1;Permission2", site.PermissionList);
            Assert.Equal("Permission1", site.Permissions[0]);

            Assert.Equal("user1@site.com", site.UserEmail);
            Assert.Equal("User1", site.User.Name);
        }

        [Fact]
        public async Task TestSiteCreation()
        {
            var context = Repository;

            var sites = new List<UserSite>();

            var user = await context.GetUserByEmail("user1@site.com");
            sites.AddRange(user.Sites);
            sites[0].Name = sites[0].Name + "_CHANGED";

            var newSite = new UserSite();
            newSite.Id = System.Guid.NewGuid().ToString("N");
            newSite.Name = "NEW SITE NAME";
            newSite.Url = "NEW SITE URL";
            newSite.Permissions = new List<string> { "a", "b" };
            newSite.User = user;

            sites.Add(newSite);

            var result = context.SaveSiteList(sites, null);
            Assert.True(result);

            var user1 = await context.GetUserByEmail("user1@site.com");
            Assert.Equal(sites.Count, user1.Sites.Count);
            Assert.Equal(newSite.Name, sites[sites.Count - 1].Name);
        }

        ILoginService Repository
        {
            get
            {
                var context = new LoginContext(options);
                var mockLogger = Mock.Of<ILogger<LoginService>>();
                Data.ContextInitializer.Initialize(context, true);
                ILoginService repo = new LoginService(context, null /*Cache*/, mockLogger);
                return repo;
            }
        }

    }
}

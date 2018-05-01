using System.Threading.Tasks;
using Login.Api.Features.Shared.Persistence;
using Login.Api.Features.User;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Login.XTests.Repository
{
    public class RepositoryTests
    {
        static DbContextOptions<LoginContext> options = new DbContextOptionsBuilder<LoginContext>()
                .UseInMemoryDatabase(databaseName: "Test_Login_Databases_Repository_Tests")
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
            var site = await Repository.GetSiteByName("site1");
            Assert.NotNull(site);
            Assert.Equal("http://www.site1.com", site.Url);
            Assert.Equal("Permission1;Permission2", site.PermissionList);
            Assert.Equal("Permission1", site.Permissions[0]);

            Assert.Equal("user1@site.com", site.UserEmail);
            Assert.Equal("User1", site.User.Name);
        }

        ILoginService Repository
        {
            get
            {
                var context = new LoginContext(options);
                Data.ContextInitializer.Initialize(context, true);
                ILoginService repo = new LoginService(context, null /*Logger*/, null /*Cache*/);
                return repo;
            }
        }

    }
}

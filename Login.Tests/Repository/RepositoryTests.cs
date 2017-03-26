using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Login.Common.Data;
using Login.Common.Repository;
using Login.Contracts.Repository;
using Login.Tests.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Login.Tests.Repository
{
    [TestClass]
    public class RepositoryTests
    {
        static DbContextOptions<LoginContext> options;

        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            options = new DbContextOptionsBuilder<LoginContext>()
                .UseInMemoryDatabase(databaseName: "Test_Login_Databases_Repository_Tests")
                .Options;
        }


        [TestMethod]
        public async Task TestEmailLookup()
        {
            var user = await Repository.GetUserByEmail("user1@site.com");
            Assert.IsNotNull(user);
        }

        [TestMethod]
        public async Task TestSiteAndPermissionRelation()
        {
            var site = await Repository.GetSiteByName("site1");
            Assert.IsNotNull(site);
            Assert.AreEqual("http://www.site1.com", site.Url);
            Assert.AreEqual("Permission1;Permission2", site.PermissionList);
            Assert.AreEqual("Permission1", site.Permissions[0]);

            Assert.AreEqual("user1@site.com", site.UserEmail);
            Assert.AreEqual("User1", site.User.Name);
        }




        ILoginRepository Repository
        {
            get
            {
                var context = new LoginContext(options);
                Login.Tests.Data.ContextInitializer.Initialize(context, true);
                ILoginRepository repo = new DatabaseRepository(context);
                return repo;
            }
        }

    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Login.Common.Data;
using Login.Common.Repository;
using Login.Contracts.Repository;
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
                .UseInMemoryDatabase(databaseName: "Add_writes_to_database")
                .Options;
        }


        [TestMethod]
        public async Task TestEmailLookup()
        {
            var context = new LoginContext(options);
            ILoginRepository repo = new DatabaseRepository(context);

            var user = await repo.GetUserByEmail("hugo");
            Assert.IsNotNull(user);


            context.Dispose();
        }
    }
}

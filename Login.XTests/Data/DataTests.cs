using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Xunit;
using Login.Web.Features.Shared.Persistence;
using Models = Login.Web.Features.Shared.Models;

namespace Login.XTests.Data
{
    public class DataTests
    {
        [Fact]
        public async Task TestBasicDataAccess()
        {
            var options = new DbContextOptionsBuilder<LoginContext>()
                .UseInMemoryDatabase(databaseName: "Test_Login_Databases_Basic_Tests")
                .Options;

            using (var context = new LoginContext(options))
            {
                context.Logins.Add(new Models.Login { Type = Models.LoginType.DIRECT, UserName = "abc", UserDisplayName = "Hugo" });
                context.SaveChanges();

                var allEntires = await context.Logins.ToListAsync();

                Assert.NotNull(allEntires);
                Assert.True(allEntires.Count == 1);
                Assert.True(allEntires[0].Id > 0);
                Assert.True(allEntires[0].Created > DateTime.MinValue);
            }

        }
    }
}

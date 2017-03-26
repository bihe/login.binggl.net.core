﻿using System;
using System.Collections.Generic;
using System.Text;
using Login.Common.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Login.Tests.Data
{
    [TestClass]
    public class DataTests
    {
        [TestMethod]
        public async Task TestBasicDataAccess()
        {
            var options = new DbContextOptionsBuilder<LoginContext>()
                .UseInMemoryDatabase(databaseName: "Test_Login_Databases_Basic_Tests")
                .Options;

            using (var context = new LoginContext(options))
            {
                context.Logins.Add(new Contracts.Models.Login { Type = Contracts.Enums.LoginType.DIRECT, UserName = "abc", UserDisplayName = "Hugo" });
                context.SaveChanges();

                var allEntires = await context.Logins.ToListAsync();

                Assert.IsNotNull(allEntires);
                Assert.IsTrue(allEntires.Count == 1);
                Assert.IsTrue(allEntires[0].Id > 0);
                Assert.IsTrue(allEntires[0].Created > DateTime.MinValue);
            }

        }
    }
}

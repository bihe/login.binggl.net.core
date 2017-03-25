using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Login.Common.Data;
using Login.Contracts.Models;
using Login.Contracts.Repository;

namespace Login.Common.Repository
{
    public class DatabaseRepository : ILoginRepository
    {
        private LoginContext context;

        public DatabaseRepository(LoginContext context)
        {
            this.context = context;
        }

        public async Task<User> GetUserByEmail(string email)
        {
            throw new NotImplementedException();
        }
    }
}

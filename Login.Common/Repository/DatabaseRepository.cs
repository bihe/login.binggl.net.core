using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

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
            var query = from u in context.Users where email.ToLower() == u.Email.ToLower() select u;
            return await query.FirstOrDefaultAsync();
        }

        public async Task<UserSite> GetSiteByName(string siteName)
        {
            var query = from s in context.UserSites where siteName.ToLower() == s.Name.ToLower() select s;
            return await query.FirstOrDefaultAsync();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Login.Contracts.Models;

namespace Login.Contracts.Repository
{
    public interface ILoginRepository
    {
        Task<User> GetUserByEmail(string email);

        Task<UserSite> GetSiteByName(string siteName);
    }
}

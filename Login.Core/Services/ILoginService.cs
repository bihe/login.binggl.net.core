﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Login.Core.Models;

namespace Login.Core.Services
{
    public interface ILoginService
    {
        Task<User> GetUserByEmail(string email, bool noCache=false);

        Task<UserSite> GetSiteByName(string siteName, bool noCache = false);
    }
}
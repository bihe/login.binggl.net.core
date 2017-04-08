using System;
using System.Collections.Generic;
using System.Text;

namespace Login.Core.Services
{
    public interface IFlashService
    {
        void Set(string key, string value);

        string Get(string key);
    }
}

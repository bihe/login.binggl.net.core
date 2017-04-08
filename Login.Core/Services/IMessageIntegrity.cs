using System;
using System.Collections.Generic;
using System.Text;

namespace Login.Core.Services
{
    public interface IMessageIntegrity
    {
        string Encode(string key);

        bool Verify(string encodedKey);
    }
}

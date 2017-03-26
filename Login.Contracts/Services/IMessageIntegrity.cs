using System;
using System.Collections.Generic;
using System.Text;

namespace Login.Contracts.Services
{
    public interface IMessageIntegrity
    {
        string Encode(string key);

        bool Verify(string encodedKey);
    }
}

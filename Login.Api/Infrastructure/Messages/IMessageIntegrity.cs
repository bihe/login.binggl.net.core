using System;
using System.Collections.Generic;
using System.Text;

namespace Login.Api.Infrastructure.Messages
{
    public interface IMessageIntegrity
    {
        string Encode(string key);

        bool Verify(string encodedKey);
    }
}

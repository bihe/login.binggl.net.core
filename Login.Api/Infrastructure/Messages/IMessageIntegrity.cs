using System;
using System.Collections.Generic;
using System.Text;

namespace Login.Web.Infrastructure.Messages
{
    public interface IMessageIntegrity
    {
        string Encode(string key);

        bool Verify(string encodedKey);
    }
}

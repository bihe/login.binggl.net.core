﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Login.Common.Configuration;
using Login.Contracts.Services;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Login.Common.Services
{
    public class HashedMessageIntegrity : IMessageIntegrity
    {
        private IOptions<ApplicationConfiguration> appConfig;

        public HashedMessageIntegrity(IOptions<ApplicationConfiguration> appConfig)
        {
            this.appConfig = appConfig;
        }

        public string Encode(string key)
        {
            var payload = new Holder { Hash = this.Hash(key), Payload = key };
            var serialized = JsonConvert.SerializeObject(payload);

            byte[] encodedBytes = Encoding.UTF8.GetBytes(serialized);
            string encodedString = Convert.ToBase64String(encodedBytes);

            return encodedString;
        }

        public bool Verify(string encodedKey)
        {
            try
            {
                byte[] decodedBytes = Convert.FromBase64String(encodedKey);
                var jsonPayload = Encoding.UTF8.GetString(decodedBytes);

                var payload = JsonConvert.DeserializeObject<Holder>(jsonPayload);
                var verifyHash = this.Hash(payload.Payload);

                return payload.Hash.SequenceEqual(verifyHash);
            }
            catch (Exception)
            { }
            return false;
        }

        private byte[] Hash(string value)
        {
            var payload = $"{this.appConfig.Value.Secret}.{value}";
            byte[] hash = null;
            using (var algorithm = SHA256.Create())
            {
                hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(payload));
            }
            return hash;
        }

        internal class Holder
        {
            public string Payload { get; set; }
            public byte[] Hash { get; set; }
        }
    }
}

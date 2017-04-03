using System.Collections.Concurrent;
using Login.Contracts.Services;

namespace Login.Common.Services
{
  public class MemoryFlashService : IFlashService
    {
        private readonly ConcurrentDictionary<string, string> flashMessageHolder = new ConcurrentDictionary<string,string>();
        
        public string Get(string key)
        {
            string value = "";
            this.flashMessageHolder.TryRemove(key, out value);
            return value;
        }

        public void Set(string key, string value)
        {
            this.flashMessageHolder.AddOrUpdate(key, value, (k, existingValue)  => {
                existingValue = value;
                return existingValue;
            });
        }
    }
}

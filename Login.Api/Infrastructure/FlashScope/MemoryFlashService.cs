using System.Collections.Concurrent;

namespace Login.Api.Infrastructure.FlashScope
{
    public class MemoryFlashService : IFlashService
    {
        private readonly ConcurrentDictionary<string, string> flashMessageHolder = new ConcurrentDictionary<string,string>();
        
        public string Get(string key)
        {
            this.flashMessageHolder.TryRemove(key, out string value);
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

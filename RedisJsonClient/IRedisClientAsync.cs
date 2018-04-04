using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace RedisJsonClient
{
    public interface IRedisClientAsync
    {
        Task<bool> SetAsync<T>([NotNull] string key, T value, TimeSpan? expireIn = null);
        Task<bool> TrySetAsync<T>([NotNull] string key, T value, TimeSpan? expireIn = null);

        Task<ConditionalValue<T>> GetAsync<T>([NotNull] string key);
        Task<ConditionalValue<T>> TryGetAsync<T>([NotNull] string key);
        Task<T> TryGetAsync<T>([NotNull] string key, T fallback);

        Task<T> GetOrTryUpdateAsync<T>([NotNull] string key, Func<Task<T>> valueProvider, TimeSpan? expireIn = null);

        Task<HashSet<string>> QueryKeysAsync([NotNull] string pattern,
            [CanBeNull] Func<string, string> transformKey = null);

        Task DeleteKeysAsync([NotNull] IEnumerable<string> keysToDelete);
    }
}
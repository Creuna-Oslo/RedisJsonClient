using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace RedisJsonClient
{
    public interface IRedisClient : IRedisClientAsync
    {
        void Set<T>([NotNull] string key, T value, TimeSpan? expireIn = null);
        [NotNull]
        T Get<T>([NotNull] string key);
        ConditionalValue<T> TryGet<T>([NotNull] string key);
        T TryGet<T>([NotNull] string key, T fallback);
        bool TrySet<T>([NotNull] string key, T value, TimeSpan? expireIn = null);
        T GetOrTryUpdate<T>([NotNull] string key, Func<T> valueProvider, TimeSpan? expireIn = null);
        T GetOrUpdate<T>([NotNull] string key, Func<T> valueProvider, TimeSpan? expireIn = null);
        [NotNull]
        HashSet<string> QueryKeys([NotNull] string pattern, [CanBeNull] Func<string, string> transformKey = null);
        void DeleteKeys([NotNull] IEnumerable<string> keysToDelete);
    }
}
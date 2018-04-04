using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace RedisJsonClient
{
    public partial class RedisClient
    {
        protected virtual bool TrySerialize<T>(string redisKey, T value, out string json, bool throwExceptions)
        {
            try
            {
                json = JsonSerializer.Serialize(value);
                return true;
            }
            catch (Exception ex)
            {
                json = null;
                OnSerializationError(redisKey, value, ex, throwExceptions);
                return false;
            }
        }

        protected virtual void OnSerializationError(string redisKey, object value, Exception ex, bool throwExceptions)
        {
            Log?.ErrorSerializing(new RedisLogMessage(redisKey, value?.ToString(), ex)
            {
                ThrowErrors = throwExceptions
            });
            if (throwExceptions)
                throw new RedisClientException($"Can not serialize: '{redisKey}';", ex);
        }

        protected virtual void OnNotFound(string redisKey)
        {
            Log?.NotFound(new RedisLogMessage(redisKey));
            throw new RedisClientException($"Data not found for key: '{redisKey}'");
        }

        protected virtual void OnGetError(string redisKey, Exception ex, bool throwExceptions)
        {

            Log?.ReadError(new RedisLogMessage(redisKey)
            {
                Error = ex,
                ThrowErrors = throwExceptions
            });

            if (throwExceptions)
                throw new RedisClientException($"Can not read data for key: '{redisKey}';", ex);
        }

        protected virtual void OnParseError<T>(string redisKey, string json, Exception ex, bool throwExceptions)
        {
            Log?.ErrorDeserializing(new RedisLogMessage(redisKey, json, ex)
            {
                ThrowErrors = throwExceptions
            });
            if (throwExceptions)
                throw new RedisClientException($"Can not parse data for key: '{redisKey}';", ex);
        }

        protected virtual void OnSetError(string redisKey, string json, Exception ex, bool throwExceptions)
        {
            Log?.WriteError(new RedisLogMessage(redisKey, json, ex)
            {
                ThrowErrors = throwExceptions
            });
            if (throwExceptions)
                throw new RedisClientException($"Can not write: '{redisKey}';", ex);
        }

        public virtual Task<ConditionalValue<T>> TryGetAsync<T>(string key, bool throwExceptions)
        {
            var redisKey = ToRedisKey(key);
            try
            {
                var db = GetDatabase();
                return db.StringGetAsync(redisKey)
                    .ContinueWith(x => TryParseRedisValue<T>(redisKey, x.Result, throwExceptions));
            }
            catch (Exception ex)
            {
                OnGetError(redisKey, ex, throwExceptions);
                return Task.FromResult(ConditionalValue<T>.NoValue);
            }
        }

        protected virtual Task<bool> TrySetAsync<T>([NotNull] string key, T value, TimeSpan? expireIn, bool throwExceptions)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            var redisKey = ToRedisKey(key);
            string json;
            if (!TrySerialize(redisKey, value, out json, throwExceptions))
                return Task.FromResult(false);

            try
            {
                var db = GetDatabase();
                return db.StringSetAsync(redisKey, json, expireIn);
            }
            catch (Exception ex)
            {
                OnSetError(redisKey, json, ex, throwExceptions);
                return Task.FromResult(false);
            }
        }

        public virtual async Task<T> GetOrTryUpdateAsync<T>(string key, Func<Task<T>> valueProvider, TimeSpan? expireIn = null)
        {
            var existing = await TryGetAsync<T>(key);
            if (existing.HasValue)
                return existing.Value;
            var value = await valueProvider();
#pragma warning disable 4014 // no need to wait while value is set
            SetAsync(key, value, expireIn);
#pragma warning restore 4014
            return value;
        }

        public virtual Task<bool> SetAsync<T>(string key, T value, TimeSpan? expireIn = null)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            return TrySetAsync(key, value, expireIn, throwExceptions: true);
        }

        public virtual Task<bool> TrySetAsync<T>(string key, T value, TimeSpan? expireIn = null)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            return TrySetAsync(key, value, expireIn, throwExceptions: false);
        }
        
        public virtual Task<ConditionalValue<T>> TryGetAsync<T>(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            var result = TryGetAsync<T>(key, throwExceptions: false);
            return result;
        }

        public async Task<T> TryGetAsync<T>(string key, T fallback)
        {
            var existing = await TryGetAsync<T>(key);
            return existing.HasValue ? existing.Value : fallback;
        }

        public virtual async Task<ConditionalValue<T>> GetAsync<T>(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            var redisKey = ToRedisKey(key);
            var result = await TryGetAsync<T>(key, throwExceptions: true);
            if (!result.HasValue)
            {
                OnNotFound(redisKey);
            }
            return result;
        }
        
        public virtual Task<HashSet<string>> QueryKeysAsync(string pattern, Func<string, string> transformKey = null)
        {
            return Task.Run(() => QueryKeys(pattern, transformKey));
        }

        public virtual Task DeleteKeysAsync(IEnumerable<string> keysToDelete)
        {
            return Task.Run(() => DeleteKeys(keysToDelete));
        }
    }
}

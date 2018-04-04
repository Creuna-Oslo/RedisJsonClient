using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using StackExchange.Redis;

namespace RedisJsonClient
{
    public partial class RedisClient : IRedisClient
    {
        public RedisClient([NotNull] IRedisConfiguration configuration, 
            [NotNull] IJsonSerializer jsonSerializer,
            [CanBeNull] IRedisClientLogger logger = null)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            JsonSerializer = jsonSerializer ?? throw new ArgumentNullException(nameof(jsonSerializer));
            Log = logger;
        }

        [CanBeNull]
        protected ConnectionMultiplexer Redis { get; private set; }
        [NotNull]
        protected IJsonSerializer JsonSerializer { get; }
        [CanBeNull]
        protected IRedisClientLogger Log { get; }
        [NotNull]
        protected IRedisConfiguration Configuration { get; }

        [NotNull]
        protected virtual IDatabase GetDatabase()
        {
            var result = TryGetDatabase(true);
            if (result == null)
                throw new RedisClientException($"No database #{Configuration.RedisDatabase} in '{Configuration.RedisConnectionString}'");
            return result;
        }

        protected virtual void EnsureRedisConnected()
        {
            if (Redis == null)
            {
                Redis = ConnectionMultiplexer.Connect(Configuration.RedisConnectionString);
            }
        }

        protected virtual IDatabase TryGetDatabase(bool throwExceptions = false)
        {
            try
            {
                EnsureRedisConnected();
                return Redis?.GetDatabase(Configuration.RedisDatabase);
            }
            catch (Exception ex)
            {
                var message =
                    $"Unable to connect to '{Configuration.RedisConnectionString}' database #{Configuration.RedisDatabase}";
                Log?.NoDatabase(message, ex, throwExceptions);
                if (throwExceptions)
                    throw new RedisClientException(message, ex);
                return null;
            }
        }

        public virtual void Set<T>(string key, T value, TimeSpan? expireIn = null)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            TrySet(key, value, true, expireIn);
        }

        protected virtual bool TrySet<T>([NotNull] string key, T value, bool throwExceptions, TimeSpan? expireIn)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            var redisKey = ToRedisKey(key);
            string json;

            if (!TrySerialize(redisKey, value, out json, throwExceptions))
                return false;

            try
            {
                var db = GetDatabase();
                db.StringSet(redisKey, json, expireIn);
                return true;
            }
            catch (Exception ex)
            {
                OnSetError(redisKey, json, ex, throwExceptions);
            }

            return false;
        }

        public virtual bool TrySet<T>(string key, T value, TimeSpan? expireIn = null)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            return TrySet(key, value, false, expireIn);
        }

        protected virtual string ToRedisKey([NotNull] string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            var result = KeyTool.CreateKey(Configuration.KeyPrefix, key);
            return result;
        }

        protected virtual string FromRedisKey([NotNull] string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            var result = KeyTool.Extract(Configuration.KeyPrefix, key);
            return result;
        }

        public virtual T Get<T>(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            var redisKey = ToRedisKey(key);
            var value = TryGet<T>(key, true);

            if (value.HasValue)
            {
                OnNotFound(redisKey);
            }
            return value.Value;
        }

        public virtual ConditionalValue<T> TryGet<T>(string key)
        {
            return TryGet<T>(key, throwExceptions: false);
        }

        public virtual T TryGet<T>(string key, T fallback)
        {
            return TryGet<T>(key).GetValueOrDefault(fallback);
        }

        protected virtual ConditionalValue<T> TryGet<T>(string key, bool throwExceptions)
        {
            var redisKey = ToRedisKey(key);
            RedisValue value;
            try
            {
                var db = GetDatabase();
                value = db.StringGet(redisKey);
            }
            catch (Exception ex)
            {
                OnGetError(redisKey, ex, throwExceptions);
                return ConditionalValue<T>.NoValue;
            }

            return TryParseRedisValue<T>(redisKey, value, throwExceptions);
        }
        
        protected virtual ConditionalValue<T> TryParseRedisValue<T>(string redisKey, RedisValue redisValue, bool throwExceptions)
        {
            string json = redisValue;

            if (json == null)
            {
                return ConditionalValue<T>.NoValue;
            }

            try
            {
                var result = new ConditionalValue<T>(JsonSerializer.Deserialize<T>(json));
                return result;
            }
            catch (Exception ex)
            {
                OnParseError<T>(redisKey, json, ex, throwExceptions);
                return ConditionalValue<T>.NoValue;
            }
        }

        public virtual T GetOrTryUpdate<T>(string key, Func<T> valueProvider, TimeSpan? expireIn = null)
        {
            return GetOrUpdate(key, valueProvider, (k, v, e) => TrySet(k, v, e), expireIn);
        }

        protected virtual T GetOrUpdate<T>(string key, Func<T> valueProvider, Action<string, T, TimeSpan?> setter, TimeSpan? expireIn)
        {
            var redisKey = ToRedisKey(key);
            var value = TryGet<T>(key);
            
            if (!value.HasValue)
            {
                try
                {
                    var result = valueProvider();
                    setter(key, result, expireIn);
                    return result;
                }
                catch (Exception ex)
                {
                    Log?.ErrorGettingValueFromProvider(new RedisLogMessage(redisKey)
                    {
                        Error = ex
                    });
                    throw new RedisClientException($"Can not get value from provider for key: '{redisKey}'", ex);
                }
            }

            return value;
        }
        
        public virtual T GetOrUpdate<T>(string key, Func<T> valueProvider, TimeSpan? expireIn = null)
        {
            return GetOrUpdate(key, valueProvider, Set, expireIn);
        }

        public virtual HashSet<string> QueryKeys(string pattern, Func<string, string> transformKey = null)
        {
            if (pattern == null) throw new ArgumentNullException(nameof(pattern));
            EnsureRedisConnected();
            if (Redis == null)
                throw new RedisClientException($"No connection to ${Configuration.RedisConnectionString}");
            var redisKey = ToRedisKey(pattern);
            var pageSize = Configuration.MaxKeys;
            var result = new HashSet<string>();

            // ReSharper disable once PossibleNullReferenceException
            foreach (var endpoint in Redis.GetEndPoints())
            {
                // ReSharper disable once PossibleNullReferenceException
                var server = Redis.GetServer(endpoint);
                var serverKeys = server.Keys(Configuration.RedisDatabase, $"{redisKey}*",
                    pageSize,
                    CommandFlags.PreferSlave);
                foreach (var serverKey in serverKeys)
                {
                    var resultKey = transformKey != null
                        ? transformKey(FromRedisKey(serverKey))
                        : FromRedisKey(serverKey);
                    result.Add(resultKey);
                }
            }

            return result;
        }

        public virtual void DeleteKeys(IEnumerable<string> keysToDelete)
        {
            var db = GetDatabase();
            var redisKeys = keysToDelete.Select(x => (RedisKey) ToRedisKey(x)).ToArray();
            db.KeyDelete(redisKeys);
        }
    }
}
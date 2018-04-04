using System;

namespace RedisJsonClient
{
    public interface IRedisClientLogger
    {
        void ErrorDeserializing(RedisLogMessage redisLogMessage);
        void NotFound(RedisLogMessage redisLogMessage);
        void ErrorGettingValueFromProvider(RedisLogMessage redisLogMessage);
        void ErrorSerializing(RedisLogMessage redisLogMessage);
        void WriteError(RedisLogMessage redisLogMessage);
        void ReadError(RedisLogMessage redisLogMessage);
        void NoDatabase(string message, Exception exception, bool throwExceptions);
    }
}
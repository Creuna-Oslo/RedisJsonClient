using System;

namespace RedisJsonClient
{
    public class TraceRedisClientLogger : IRedisClientLogger
    {
        public virtual void ErrorDeserializing(RedisLogMessage redisLogMessage)
        {
            System.Diagnostics.Trace.WriteLine($"Error deserializing json: {redisLogMessage}");
        }

        public virtual void NotFound(RedisLogMessage redisLogMessage)
        {
            System.Diagnostics.Trace.WriteLine($"Value not found in redis: {redisLogMessage}");
        }

        public virtual void ErrorGettingValueFromProvider(RedisLogMessage redisLogMessage)
        {
            System.Diagnostics.Trace.WriteLine($"Error getting value from provider: {redisLogMessage}");
        }

        public virtual void ErrorSerializing(RedisLogMessage redisLogMessage)
        {
            System.Diagnostics.Trace.WriteLine($"Error serializing object: {redisLogMessage}");
        }

        public virtual void WriteError(RedisLogMessage redisLogMessage)
        {
            System.Diagnostics.Trace.WriteLine($"Error writing redis value: {redisLogMessage}");
        }

        public virtual void ReadError(RedisLogMessage redisLogMessage)
        {
            System.Diagnostics.Trace.WriteLine($"Error reading redis value: {redisLogMessage}");
        }

        public virtual void NoDatabase(string message, Exception exception, bool throwExceptions)
        {
            System.Diagnostics.Trace.WriteLine($"No redis database found. Message: {message}, throw: {throwExceptions}, error: {exception}");
        }
    }
}
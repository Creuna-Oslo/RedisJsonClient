using System;
using System.Text;
using JetBrains.Annotations;

namespace RedisJsonClient
{
    public class RedisLogMessage
    {
        public RedisLogMessage()
        {}

        public RedisLogMessage(string redisKey)
            : this(redisKey, null, null)
        { }
        public RedisLogMessage(string redisKey, string redisValue, [CanBeNull] Exception error)
        {
            RedisKey = redisKey;
            RedisValue = redisValue;
            Error = error;
        }

        public string RedisKey { get; set; }
        public string RedisValue { get; set; }
        public Exception Error { get; set; }

        public bool ThrowErrors { get; set; } = true;

        public override string ToString()
        {
            var buffer = new StringBuilder();
            if (Error != null)
                buffer.Append($"Error message: '{Error.Message}'. ");
            buffer.Append($"Key: {RedisKey ?? "(no key). "}");
            if (!string.IsNullOrEmpty(RedisValue))
                buffer.Append($"Value: '{RedisValue}'. ");
            if (Error != null)
                buffer.Append($"Error details: {Error}. ");
            return buffer.ToString();
        }
    }
}
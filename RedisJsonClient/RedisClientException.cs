using System;
using System.Runtime.Serialization;

namespace RedisJsonClient
{
    [Serializable]
    public class RedisClientException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public RedisClientException()
        {
        }

        public RedisClientException(string message) : base(message)
        {
        }

        public RedisClientException(string message, Exception inner) : base(message, inner)
        {
        }

        protected RedisClientException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
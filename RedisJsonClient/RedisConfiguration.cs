namespace RedisJsonClient
{
    public class RedisConfiguration : IRedisConfiguration
    {
        public string KeyPrefix { get; set; }
        public string RedisConnectionString { get; set; }
        public int RedisDatabase { get; set; }
        public int MaxKeys { get; set; }
    }
}
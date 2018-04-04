namespace RedisJsonClient
{
    public interface IRedisConfiguration
    {
        string KeyPrefix { get; }
        string RedisConnectionString { get; }
        int RedisDatabase { get; }
        int MaxKeys { get; }
    }
}
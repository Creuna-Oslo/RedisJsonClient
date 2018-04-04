namespace RedisJsonClient
{
    public interface IJsonSerializer
    {
        T Deserialize<T>(string json);
        string Serialize<T>(T value);
    }
}
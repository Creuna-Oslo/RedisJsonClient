using Newtonsoft.Json;

namespace RedisJsonClient
{
    public class NewtonsoftJsonSerializer : IJsonSerializer
    {
        public NewtonsoftJsonSerializer(JsonConverter[] customConverters)
        {
            CustomConverters = customConverters ?? new JsonConverter[] { };
        }
        public virtual T Deserialize<T>(string json)
        {
            var result = JsonConvert.DeserializeObject<T>(json, CustomConverters);
            return result;
        }

        public virtual string Serialize<T>(T value)
        {
            var result = JsonConvert.SerializeObject(value, Formatting.None, CustomConverters);
            return result;
        }

        protected virtual JsonConverter[] CustomConverters { get; } 

    }
}
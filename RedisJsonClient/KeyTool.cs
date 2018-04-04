using System;
using System.Linq;
using JetBrains.Annotations;

namespace RedisJsonClient
{
    public static class KeyTool
    {
        public static string CreateKey(params string[] parts)
        {
            var nonEmptyParts = parts.Where(x => !string.IsNullOrWhiteSpace(x));
            var result = string.Join(":", nonEmptyParts);
            return result;
        }

        public static string Extract([CanBeNull] string prefix, [NotNull] string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (prefix == null)
                return key;
            if (!prefix.EndsWith(":"))
                prefix = prefix + ":";
            return key.StartsWith(prefix) ? key.Substring(prefix.Length) : key;
        }

        public static string[] SplitKey([NotNull] string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            var result = key.Split(new[] {":"}, StringSplitOptions.RemoveEmptyEntries);
            return result;
        }
    }
}
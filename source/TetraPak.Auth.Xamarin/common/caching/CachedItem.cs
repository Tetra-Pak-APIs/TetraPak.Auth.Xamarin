using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TetraPak.Auth.Xamarin.common
{
    public struct CachedItem<TValue>
    {
        internal DateTime Expires { get; set; }
        public string Key { get; set; }
        public bool IsExpired => DateTime.Now >= Expires;
        public TValue Value { get; }

        public CachedItem(string key, TValue value, DateTime expires)
        {
            Key = key;
            Expires = expires;
            Value = value;
        }
    }

    public class CachedItemDTO
    {
        [JsonPropertyName("key")]
        public string Key { get; set; }

        [JsonPropertyName("expires")]
        public string Expires { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }
    }

    public static class CachedItemHelper
    {
        internal static CachedItem<T> FromJson<T>(this string json)
        {
            var dto = JsonSerializer.Deserialize<CachedItemDTO>(json);
            var value = JsonSerializer.Deserialize<T>(dto.Value);
            var expires = DateTime.ParseExact(dto.Expires, "O", CultureInfo.InvariantCulture);
            return new CachedItem<T>(dto.Key, value, expires);
        }

        internal static string ToJson<T>(this CachedItem<T> item)
        {
            var dto = new CachedItemDTO
            {
                Key = item.Key,
                Expires = item.Expires.ToString("O"),
                Value = JsonSerializer.Serialize(item.Value)
            };
            return JsonSerializer.Serialize(dto);
        }
    }
}
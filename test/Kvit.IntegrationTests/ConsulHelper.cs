using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Kvit.IntegrationTests
{
    internal static class ConsulHelper
    {
        private static readonly HttpClient _httpClient = new()
        {
            BaseAddress = new Uri(ProcessHelper.TestConsulUrl + "/v1/kv/")
        };

        public static Task AddDataToConsulAsync(string key, string value)
        {
            return _httpClient.PutAsJsonAsync(key, value);
        }

        public static Task AddDirectoryToConsulAsync(string key)
        {
            var normalizedKey = key.EndsWith("/")
                ? key
                : string.Concat(key, "/");
            return _httpClient.PutAsync(normalizedKey, new StringContent(""));
        }
    }
}
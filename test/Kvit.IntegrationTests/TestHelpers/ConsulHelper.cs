using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Kvit.IntegrationTests.TestHelpers
{
    internal static class ConsulHelper
    {
        internal record ConsulGetResultItem(string Value);

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

        public async static Task<string> GetValueFromConsulAsync(string key)
        {
            var results = await _httpClient.GetFromJsonAsync<ConsulGetResultItem[]>(key);
            if (results?.FirstOrDefault().Value == null)
            {
                return string.Empty;
            }

            return Encoding.UTF8.GetString(Convert.FromBase64String(results.First().Value));
        }

        public static Task DeleteAllKeys()
        {
            return _httpClient.DeleteAsync("?recurse=true");
        }
    }
}
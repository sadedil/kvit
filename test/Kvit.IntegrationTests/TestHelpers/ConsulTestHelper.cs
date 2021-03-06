using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Kvit.IntegrationTests.TestHelpers
{
    internal static class ConsulTestHelper
    {
        internal record ConsulGetResultItem(string Value);

        private static readonly HttpClient _httpClient = new()
        {
            BaseAddress = new Uri(ProcessTestHelper.TestConsulUrl + "/v1/kv/")
        };

        public static Task AddDataToConsulAsync(string key, object value)
        {
            return _httpClient.PutAsJsonAsync(key, value, new JsonSerializerOptions() {WriteIndented = true});
        }

        public static Task AddDirectoryToConsulAsync(string key)
        {
            var normalizedKey = key.EndsWith("/")
                ? key
                : string.Concat(key, "/");
            return _httpClient.PutAsync(normalizedKey, new StringContent(""));
        }

        public static async Task<string> GetValueFromConsulAsync(string key)
        {
            try
            {
                var results = await _httpClient.GetFromJsonAsync<ConsulGetResultItem[]>(key);
                if (results?.FirstOrDefault().Value == null)
                {
                    return string.Empty;
                }

                return Encoding.UTF8.GetString(Convert.FromBase64String(results.First().Value));
            }
            catch (HttpRequestException hre)
            {
                if (hre.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }

                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static Task DeleteAllKeys()
        {
            return _httpClient.DeleteAsync("?recurse=true");
        }
    }
}
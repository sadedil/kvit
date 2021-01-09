using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Consul;

namespace Kvit.Commands
{
    public class FetchCommand : BaseCommand
    {
        protected override string CommandAlias => "f";
        protected override string CommandDescription => "Fetch the given Consul KV directory to file system";

        public FetchCommand() : base("fetch")
        {
            AddOption(new Option<Uri>("--address")
            {
                Description = "Consul Url like http://localhost:8500",
            });

            AddOption(new Option<string>("--token")
            {
                Description = "Consul Token like P@ssW0rd",
            });

            Handler = CommandHandler.Create<Uri, string>(ExecuteAsync);
        }

        private async Task ExecuteAsync(Uri address, string token)
        {
            var nonFolderPairs = await GetNonFolderPairsAsync(address, token);
            if (nonFolderPairs == null)
            {
                return;
            }

            var fetchResults = new List<FetchResult>();
            foreach (var nonFolderPair in nonFolderPairs)
            {
                var fetchResult = new FetchResult
                {
                    Key = nonFolderPair.Key
                };
                fetchResults.Add(fetchResult);

                try
                {
                    var fileName = Path.Combine(Common.BaseDirectory, nonFolderPair.Key);

                    var content = nonFolderPair.Value == null
                        ? string.Empty
                        : Encoding.UTF8.GetString(nonFolderPair.Value);

                    Directory.CreateDirectory(Path.GetDirectoryName(fileName));
                    await File.WriteAllTextAsync(fileName, content);

                    fetchResult.IsSucceeded = true;
                }
                catch (IOException ex)
                {
                    fetchResult.IsSucceeded = false;
                    fetchResult.ErrorMessage = $"Key couldn't be fetched. Make sure there is no Key and Folder pair with the same name in the same directory. Error: {ex.Message}";
                }
                catch (Exception ex)
                {
                    fetchResult.IsSucceeded = false;
                    fetchResult.ErrorMessage = $"Key couldn't be fetched. Error: {ex.Message}";
                }
            }

            if (fetchResults.All(f => f.IsSucceeded))
            {
                Console.WriteLine("All keys successfully fetched.");
            }
            else
            {
                var unSuccessfulResults = fetchResults.Where(fr => !fr.IsSucceeded).ToList();
                await Console.Error.WriteLineAsync($"{unSuccessfulResults.Count} / {fetchResults.Count} key(s) cannot be fetched");
                foreach (var fetchResult in unSuccessfulResults)
                {
                    await Console.Error.WriteLineAsync($"{fetchResult.Key}: {fetchResult.ErrorMessage}");
                }
            }
        }

        private async Task<IEnumerable<KVPair>> GetNonFolderPairsAsync(Uri address, string token)
        {
            try
            {
                using var client = ConsulHelper.CreateConsulClient(address, token);
                var pairs = await client.KV.List(prefix: "/");
                return pairs.Response.Where(p => !p.Key.EndsWith("/")).AsEnumerable();
            }
            catch (Exception ex)
            {
                await Console.Error.WriteLineAsync($"Cannot get keys. Error: {ex.Message}");
                return null;
            }
        }

        private class FetchResult
        {
            public bool IsSucceeded { get; set; }
            public string Key { get; set; }
            public string ErrorMessage { get; set; }
        }
    }
}
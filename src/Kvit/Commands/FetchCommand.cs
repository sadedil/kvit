using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kvit.Commands
{
    public class FetchCommand : BaseCommand
    {
        protected override string CommandAlias => "f";
        protected override string CommandDescription => "Fetch the given Consul KV directory to file system";

        public FetchCommand() : base("fetch")
        {
            var addressOption = new Option<Uri>("--address")
            {
                Description = "Consul Url like http://localhost:8500",
            };

            var tokenOption = new Option<string>("--token")
            {
                Description = "Consul Token like P@ssW0rd",
            };

            AddOption(addressOption);
            AddOption(tokenOption);

            this.SetHandler(ExecuteAsync, addressOption, tokenOption);
        }

        private async Task<int> ExecuteAsync(Uri address, string token)
        {
            using var client = ConsulHelper.CreateConsulClient(address, token);
            Console.WriteLine($"Fetch started. Address: {client.Config?.Address}");

            var nonFolderPairs = await Common.GetNonFolderPairsAsync(client);

            if (nonFolderPairs == null)
            {
                return 1;
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
                return 0;
            }

            var unSuccessfulResults = fetchResults.Where(fr => !fr.IsSucceeded).ToList();
            await Console.Error.WriteLineAsync($"{unSuccessfulResults.Count} / {fetchResults.Count} key(s) cannot be fetched");
            foreach (var fetchResult in unSuccessfulResults)
            {
                await Console.Error.WriteLineAsync($"{fetchResult.Key}: {fetchResult.ErrorMessage}");
            }

            return 1;
        }

        private class FetchResult
        {
            public bool IsSucceeded { get; set; }
            public string Key { get; set; }
            public string ErrorMessage { get; set; }
        }
    }
}
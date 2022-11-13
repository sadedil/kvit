using Consul;
using Kvit.Extensions;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kvit.Commands
{
    public class PushCommand : BaseCommand
    {
        protected override string CommandAlias => "p";
        protected override string CommandDescription => "Pulls the given directory to file system";

        public PushCommand() : base("push")
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
            Console.WriteLine($"Push started. Address: {client?.Config?.Address}");

            var usedFiles = Common.GetLocalFiles();

            var txnOps = new List<KVTxnOp>();
            foreach (var filePath in usedFiles)
            {
                var consulPath = Path.GetRelativePath(Common.BaseDirectoryFullPath, filePath).ToUnixPath();

                var textContent = await File.ReadAllTextAsync(filePath, Encoding.UTF8);

                txnOps.Add(new KVTxnOp(consulPath, KVTxnVerb.Set)
                {
                    Value = Encoding.UTF8.GetBytes(textContent)
                });

                Console.WriteLine($"Key: {consulPath}");
            }

            Console.WriteLine($"{txnOps.Count} key(s) prepared. Trying to push...");

            foreach (var txnOpsChunk in txnOps.ChunkBy(Common.ConsulTransactionMaximumOperationCount))
            {
                var pushResponse = await client.KV.Txn(txnOpsChunk);

                if (pushResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    Console.WriteLine($"{pushResponse.Response.Results.Count} key(s) pushed.");
                }
                else
                {
                    Console.WriteLine($"{pushResponse.Response.Results.Count} key(s) pushed. ");
                    Console.WriteLine($"{pushResponse.Response.Errors.Count} key(s) couldn't be pushed due to errors: ");

                    foreach (var errorMessage in pushResponse.Response.Errors.Select(x => x.What).Distinct())
                    {
                        Console.WriteLine($"{errorMessage}");
                    }
                }
            }

            return 0;
        }
    }
}
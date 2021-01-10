using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Consul;
using Kvit.Extensions;

namespace Kvit.Commands
{
    public class PushCommand : BaseCommand
    {
        protected override string CommandAlias => "p";
        protected override string CommandDescription => "Pulls the given directory to file system";

        public PushCommand() : base("push")
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
            using var client = ConsulHelper.CreateConsulClient(address, token);

            var baseDirectoryInfo = new DirectoryInfo(Common.BaseDirectory);
            var kvitDirectoryPath = Path.Combine(baseDirectoryInfo.ToString(), ".kvit/");

            var allFilenames = Directory.GetFiles(Common.BaseDirectory, "*.*", SearchOption.AllDirectories);
            var usedFiles = allFilenames
                .Select(fn => new FileInfo(fn))
                .Where(fi => !fi.ToString().StartsWith(kvitDirectoryPath))
                .Where(fi => fi.Name != ".DS_Store")
                .ToList();

            var txnOps = new List<KVTxnOp>();

            foreach (var fileInfo in usedFiles)
            {
                // TODO: Low: Find an elegant way to do normalize paths
                var fullDirectoryPath = baseDirectoryInfo.FullName.EndsWith("/")
                    ? baseDirectoryInfo.FullName
                    : $"{baseDirectoryInfo.FullName}/";

                var consulPath = fileInfo.FullName.StartsWith(fullDirectoryPath)
                    ? fileInfo.FullName.Substring(baseDirectoryInfo.FullName.Length)
                    : fileInfo.FullName;

                consulPath = consulPath.Trim('/');

                var textContent = await File.ReadAllTextAsync(fileInfo.FullName, Encoding.UTF8);

                txnOps.Add(new KVTxnOp(consulPath, KVTxnVerb.Set)
                {
                    Value = Encoding.UTF8.GetBytes(textContent)
                });

                Console.WriteLine($"Key: {consulPath}");
            }

            foreach (var txnOpsChunk in txnOps.ChunkBy(Common.ConsulTransactionMaximumOperationCount))
            {
                await client.KV.Txn(txnOpsChunk);
            }

            Console.WriteLine($"{txnOps.Count} key(s) pushed. Address: {client?.Config?.Address}");
        }
    }
}
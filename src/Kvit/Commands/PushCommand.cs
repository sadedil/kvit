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
            Console.WriteLine($"Push started. Address: {client?.Config?.Address}");

            var baseDirectoryInfo = new DirectoryInfo(Common.BaseDirectory);
            var usedFiles = Directory
                .GetFiles(Common.BaseDirectory, "*.*", SearchOption.AllDirectories)
                .Select(f => new FileInfo(f).FullName.ToUnixPath().TrimEnd('/'))
                .Where(f => !f.StartsWith(Common.DotKvitDirectoryFullPath))
                .Where(f => f != ".DS_Store")
                .Where(f => f != "desktop.ini")
                .ToList();

            var txnOps = new List<KVTxnOp>();
            foreach (var filePath in usedFiles)
            {
                var consulPath = filePath.StartsWith(Common.BaseDirectoryFullPath)
                    ? filePath.Substring(Common.BaseDirectoryFullPath.Length)
                    : filePath;

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
                await client.KV.Txn(txnOpsChunk);
            }

            Console.WriteLine($"{txnOps.Count} key(s) pushed.");
        }
    }
}
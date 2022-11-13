using Consul;
using DiffPlex.DiffBuilder;
using Kvit.Extensions;
using Spectre.Console;
using System;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kvit.Commands
{
    public class DiffCommand : BaseCommand
    {
        protected override string CommandAlias => "d";
        protected override string CommandDescription => "Show the differences between Consul KV directory and file system";

        internal const string FileContentsAreEqualSign = "==";
        internal const string FileContentsAreDifferentSign = "xx";
        internal const string OnlyInFileSystemSign = "<<";
        internal const string OnlyInConsulSign = "<<";

        public DiffCommand() : base("diff")
        {
            var addressOption = new Option<Uri>("--address")
            {
                Description = "Consul Url like http://localhost:8500",
            };

            var tokenOption = new Option<string>("--token")
            {
                Description = "Consul Token like P@ssW0rd",
            };

            var allOption = new Option<bool>("--all")
            {
                Description = "List all differences, including identical files",
            };

            AddOption(addressOption);
            AddOption(tokenOption);
            AddOption(allOption);

            this.SetHandler<Uri, string, bool>(ExecuteAsync, addressOption, tokenOption, allOption);
            //Handler = CommandHandler.Create<Uri, string, bool>(ExecuteAsync);
        }

        private async Task<int> ExecuteAsync(Uri address, string token, bool all)
        {
            using var client = ConsulHelper.CreateConsulClient(address, token);
            Console.WriteLine($"Diff started. Address: {client.Config?.Address}");

            return await CompareFiles(client, includeIdenticalResults: all);
        }

        private static async Task<int> CompareFiles(ConsulClient client, bool includeIdenticalResults)
        {
            var nonFolderPairs = await Common.GetNonFolderPairsAsync(client);
            if (nonFolderPairs == null)
            {
                return 1;
            }

            var localFiles = Common.GetLocalFiles();
            localFiles = localFiles.Select(f => Path.GetRelativePath(Common.BaseDirectoryFullPath, f).ToUnixPath()).ToList();

            var table = new Table();
            table.HorizontalBorder();
            table.BorderColor(Color.Grey74);
            table.AddColumn(new TableColumn("Files".ToSilver(isBold: true)));
            table.AddColumn(new TableColumn("Consul - Local".ToSilver(isBold: true)).Centered());

            var commonPairs = nonFolderPairs.Where(p => localFiles.Contains(p.Key));
            var consulFiles = nonFolderPairs.Select(p => p.Key);
            var missingFilesOnConsul = localFiles.Where(f => !consulFiles.Contains(f)).ToList();
            var missingFilesOnFileSystem = consulFiles.Where(k => !localFiles.Contains(k)).ToList();
            var exitCode = 0;
            if (missingFilesOnConsul.Any() || missingFilesOnFileSystem.Any())
            {
                exitCode = 2;
            }

            foreach (var pair in commonPairs)
            {
                var filePath = Path.Combine(Common.BaseDirectoryFullPath, pair.Key);
                var fileContent = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
                var consulContent = Encoding.UTF8.GetString(pair.Value ?? new byte[] { });
                var diffModel = InlineDiffBuilder.Diff(consulContent, fileContent);

                if (diffModel.HasDifferences)
                {
                    exitCode = 2;
                    table.AddRow(pair.Key.ToRed(), FileContentsAreDifferentSign.ToRed());
                }
                else if (includeIdenticalResults)
                {
                    table.AddRow(pair.Key.ToSilver(), FileContentsAreEqualSign.ToSilver());
                }
            }

            foreach (var file in missingFilesOnConsul)
            {
                table.AddRow(file.ToYellow(), OnlyInFileSystemSign.ToYellow());
            }

            foreach (var file in missingFilesOnFileSystem)
            {
                table.AddRow(file.ToYellow(), OnlyInConsulSign.ToYellow());
            }

            AnsiConsole.Write(table);
            return exitCode;
        }
    }
}
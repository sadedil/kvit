using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Consul;
using Spectre.Console;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Kvit.Extensions;

namespace Kvit.Commands
{
    public class DiffCommand : BaseCommand
    {
        protected override string CommandAlias => "d";
        protected override string CommandDescription => "Show the differences between Consul KV directory and file system";
        public DiffCommand() : base("diff")
        {
            AddOption(new Option<Uri>("--address")
            {
                Description = "Consul Url like http://localhost:8500",
            });

            AddOption(new Option<string>("--token")
            {
                Description = "Consul Token like P@ssW0rd",
            });

            AddOption(new Option<string>("--file")
            {
                Description = "Compares the contents of all files",
            });

            Handler = CommandHandler.Create<Uri, string, string>(ExecuteAsync);
        }

        private async Task<int> ExecuteAsync(Uri address, string token, string file)
        {
            using var client = ConsulHelper.CreateConsulClient(address, token);
            Console.WriteLine($"Diff started. Address: {client?.Config?.Address}");

            if (file == null)
            {
                return await CompareFiles(client);
            }
            else
            {
                try
                {
                    var pair = await client.KV.Get(file);
                    return await CompareFileContents(pair.Response);
                }
                catch (Exception ex)
                {
                    await Console.Error.WriteLineAsync($"Cannot get keys. Error: {ex.Message}");
                    return 1;
                }
            }
        }

        private async Task<int> CompareFiles(ConsulClient client)
        {
            var nonFolderPairs = await Common.GetNonFolderPairsAsync(client);
            if (nonFolderPairs == null)
            {
                return 1;
            }
            var localFiles = Common.GetLocalFiles();
            localFiles = localFiles.Select(f => Path.GetRelativePath(Common.BaseDirectoryFullPath, f).ToUnixPath()).ToList();
            
            var table = new Table();
            table.BorderColor(Color.Blue);
            table.AddColumn(new TableColumn(("Files").Colorize("blue"))); 
            table.AddColumn(new TableColumn(("Consul - Local").Colorize("blue")).Centered());

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
                var consulContent = Encoding.UTF8.GetString(pair.Value ?? new byte[]{});
                var diffModel = InlineDiffBuilder.Diff(consulContent, fileContent);
                string resultColumn;
                if (diffModel.HasDifferences)
                {
                    exitCode = 2;
                    resultColumn = ("xx").Colorize("red");
                }
                else
                {
                    resultColumn = ("==").Colorize("green");
                }
                
                table.AddRow(pair.Key.Colorize("yellow"), resultColumn);
            }
            foreach (var file in missingFilesOnConsul)
            {
                table.AddRow(file.Colorize("yellow"), ("<<").Colorize("blue"));
            }
            foreach (var file in missingFilesOnFileSystem)
            {
                table.AddRow(file.Colorize("yellow"), (">>").Colorize("blue"));
            }

            AnsiConsole.Render(table);
            return exitCode;
        }

        private async Task<int> CompareFileContents(KVPair pair)
        {
            var filePath = Path.Combine(Common.BaseDirectoryFullPath, pair.Key);
            if (!File.Exists(filePath))
            {
                await Console.Error.WriteLineAsync($"File not exist.");
                return 1;
            }

            var table = new Table();
            table.BorderColor(Color.Blue);
            table.AddColumn(new TableColumn(("Consul").Colorize("blue"))); 
            table.AddColumn(new TableColumn(("Local").Colorize("blue")));
            
            var fileContent = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
            var consulContent = Encoding.UTF8.GetString(pair.Value ?? Array.Empty<byte>());
            var diffModel = SideBySideDiffBuilder.Diff(consulContent, fileContent);
            
            var oldLines = diffModel.OldText.Lines;
            var newLines = diffModel.NewText.Lines;
            var maxRow = Math.Max(newLines.Count, oldLines.Count);
            
            for (var i = 0; i < maxRow; i++)
            {
                var oldLine = oldLines[i];
                var oldDiffPosition = oldLine.Position.HasValue ? oldLine.Position.Value.ToString() : "  ";
                var oldDiffRow = GenerateDiff(oldLine);
                oldDiffRow = $"{oldDiffPosition.Colorize("yellow")} {oldDiffRow}";
            
                var newLine = newLines[i];
                var newDiffPosition = newLine.Position.HasValue ? newLine.Position.Value.ToString() : "  ";
                var newDiffRow = GenerateDiff(newLine);
                newDiffRow = $"{newDiffPosition.Colorize("yellow")} {newDiffRow}";

                table.AddRow(oldDiffRow, newDiffRow);
            }

            AnsiConsole.Render(table);
            if (diffModel.NewText.HasDifferences || diffModel.OldText.HasDifferences)
            {
                return 2;
            }
            return 0;
        }

        private string GenerateDiff(DiffPiece diffPiece)
        {
            var diffRow = string.Empty;
            switch (diffPiece.Type)
            {
                case ChangeType.Deleted:
                    diffRow = diffPiece.Text.Colorize("red");
                    break;
                case ChangeType.Inserted:
                    diffRow = diffPiece.Text.Colorize("green");
                    break;
                case ChangeType.Modified:
                    diffRow = diffPiece.SubPieces.Aggregate(diffRow, (current, subPiece) => $"{current}{GenerateDiff(subPiece)}");
                    break;
                case ChangeType.Unchanged:
                    diffRow = diffPiece.Text.Colorize("blue");
                    break;
            }

            return diffRow;
        }
    }
}
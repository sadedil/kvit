using Consul;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
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
    public class CompareCommand : BaseCommand
    {
        protected override string CommandAlias => "c";
        protected override string CommandDescription => "Compares the given key's content between Consul and file system";

        public CompareCommand() : base("compare")
        {
            var addressOption = new Option<Uri>("--address")
            {
                Description = "Consul Url like http://localhost:8500",
            };

            var tokenOption = new Option<string>("--token")
            {
                Description = "Consul Token like P@ssW0rd",
            };

            var fileArgument = new Argument<string>("file")
            {
                Description = "File name to compare",
                Arity = new ArgumentArity(1, 1)
            };

            AddOption(addressOption);
            AddOption(tokenOption);
            AddArgument(fileArgument);

            this.SetHandler(ExecuteAsync, addressOption, tokenOption, fileArgument);
        }

        private async Task<int> ExecuteAsync(Uri address, string token, string file)
        {
            using var client = ConsulHelper.CreateConsulClient(address, token);
            Console.WriteLine($"Diff started. Address: {client.Config?.Address}");

            try
            {
                var pair = await client.KV.Get(file);
                if (pair?.Response == null)
                {
                    await Console.Error.WriteLineAsync($"Cannot find key on consul: {file}");
                    return 1;
                }

                return await CompareFileContents(pair.Response);
            }
            catch (Exception ex)
            {
                await Console.Error.WriteLineAsync($"Cannot compare file: {ex.Message}");
                return 1;
            }
        }

        private async Task<int> CompareFileContents(KVPair pair)
        {
            var filePath = Path.Combine(Common.BaseDirectoryFullPath, pair.Key);
            if (!File.Exists(filePath))
            {
                await Console.Error.WriteLineAsync($"File not exist: {filePath}");
                return 1;
            }

            var table = new Table();
            table.Expand();
            table.HorizontalBorder();
            table.BorderColor(Color.Grey74);
            table.AddColumn(new TableColumn("Consul".ToSilver(isBold: true)));
            table.AddColumn(new TableColumn("Local".ToSilver(isBold: true)));

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
                oldDiffRow = $"{oldDiffPosition.ToGrey()} {oldDiffRow}";

                var newLine = newLines[i];
                var newDiffPosition = newLine.Position.HasValue ? newLine.Position.Value.ToString() : "  ";
                var newDiffRow = GenerateDiff(newLine);
                newDiffRow = $"{newDiffPosition.ToGrey()} {newDiffRow}";

                table.AddRow(oldDiffRow, newDiffRow);
            }

            AnsiConsole.Write(table);
            if (diffModel.NewText.HasDifferences || diffModel.OldText.HasDifferences)
            {
                return 2;
            }

            return 0;
        }

        private static string GenerateDiff(DiffPiece diffPiece)
        {
            var diffRow = string.Empty;
            diffRow = diffPiece.Type switch
            {
                ChangeType.Deleted => diffPiece.Text.ToRed(),
                ChangeType.Inserted => diffPiece.Text.ToGreen(),
                ChangeType.Modified => diffPiece.SubPieces.Aggregate(diffRow, (current, subPiece) => $"{current}{GenerateDiff(subPiece)}"),
                ChangeType.Unchanged => diffPiece.Text.ToSilver(),
                _ => diffRow
            };

            return diffRow;
        }
    }
}
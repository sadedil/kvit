using System.CommandLine;
using System.Threading.Tasks;
using Kvit.Commands;

namespace Kvit
{
    internal static class Program
    {
        private static Task Main(string[] args)
        {
            var rootCommand = new RootCommand
            {
                Description = "Kvit syncs your Consul KV values with file system",
            };

            rootCommand.AddCommand(new FetchCommand());
            rootCommand.AddCommand(new PushCommand());

            return rootCommand.InvokeAsync(args);
        }
    }
}
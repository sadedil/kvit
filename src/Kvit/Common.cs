using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Kvit.Extensions;
using Consul;

namespace Kvit
{
    public static class Common
    {
        public const int ConsulTransactionMaximumOperationCount = 64;
        public static readonly Uri DefaultConsulUri = new Uri("http://localhost:8500");

        public static readonly string BaseDirectory; // Will be filled in static constructor
        public static readonly string BaseDirectoryFullPath; // Will be filled in static constructor
        private static readonly string DotKvitDirectoryFullPath; // Will be filled in static constructor

        private static readonly string[] _ignoredFiles = {".DS_Store", "desktop.ini"};

        static Common()
        {
            var baseDirectory = Environment.GetEnvironmentVariable(EnvironmentVariables.KVIT_BASE_DIR)?.ToUnixPath() ?? "./";
            var baseDirectoryInfoFullName = new DirectoryInfo(baseDirectory).FullName.ToUnixPath();
            var baseDirectoryFullPath = baseDirectoryInfoFullName.EndsWith("/") ? baseDirectoryInfoFullName : $"{baseDirectoryInfoFullName}/";
            var dotKvitDirectoryFullPath = new DirectoryInfo(Path.Combine(baseDirectoryFullPath, ".kvit")).FullName.ToUnixPath();

            BaseDirectory = baseDirectory;
            BaseDirectoryFullPath = baseDirectoryFullPath;
            DotKvitDirectoryFullPath = dotKvitDirectoryFullPath;
        }

        public static async Task<List<KVPair>> GetNonFolderPairsAsync(ConsulClient client)
        {
            try
            {
                var pairs = await client.KV.List(prefix: "/");
                return pairs.Response.Where(p => !p.Key.EndsWith("/")).ToList();
            }
            catch (Exception ex)
            {
                await Console.Error.WriteLineAsync($"Cannot get keys. Error: {ex.Message}");
                return null;
            }
        }

        public static List<string> GetLocalFiles()
        {
            return Directory
                .GetFiles(BaseDirectory, "*.*", SearchOption.AllDirectories)
                .Select(f => f.ToUnixPath())
                .Where(f => !f.StartsWith(DotKvitDirectoryFullPath))
                .Where(f => !_ignoredFiles.Any(i => string.Equals(Path.GetFileName(f), i, StringComparison.InvariantCultureIgnoreCase)))
                .ToList();
        }
    }
}
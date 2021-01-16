using System;
using System.IO;
using Kvit.Extensions;

namespace Kvit
{
    public static class Common
    {
        public const int ConsulTransactionMaximumOperationCount = 64;
        public static readonly Uri DefaultConsulUri = new Uri("http://localhost:8500");

        public static readonly string BaseDirectory = null; // Will be filled in static constructor
        public static readonly string BaseDirectoryFullPath = null; // Will be filled in static constructor
        public static readonly string DotKvitDirectoryFullPath = null; // Will be filled in static constructor

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
    }
}
using System.Diagnostics;
using System.IO;

namespace Kvit.IntegrationTests
{
    public static class ProcessHelper
    {
        internal static readonly string TestConsulBaseDir = Path.Combine(Path.GetTempPath(), "kvit_test");
        internal const string TestConsulUrl = "http://localhost:8900";

        public static (string stdout, string stderr) RunKvit(string args)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $" run -- {args}",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = "../../../../../src/Kvit",
                    Environment =
                    {
                        {"KVIT_BASE_DIR", TestConsulBaseDir},
                        {"KVIT_ADDRESS", TestConsulUrl},
                        // {"KVIT_TOKEN", ""}
                    }
                }
            };
            process.Start();

            var stdout = process.StandardOutput.ReadToEnd();
            var stderr = process.StandardError.ReadToEnd();

            return (stdout, stderr);
        }
    }
}
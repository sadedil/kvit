using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Kvit.IntegrationTests.TestHelpers
{
    internal static class ProcessHelper
    {
        private const string KvitProjectPathRelativeToUnitTestDirectory = "../../../../../src/Kvit";
        internal const string TestConsulUrl = "http://localhost:8900";

        internal static (string stdout, string stderr, int exitCode) RunKvit(bool runWithBaseDir, string baseDir, string args)
        {
            return runWithBaseDir
                ? RunKvitWithBaseDirEnvironmentVariable(baseDir, args)
                : RunKvitWithoutBaseDirEnvironmentVariable(baseDir, args);
        }

        internal static string CreateRandomBaseDir()
        {
            var baseDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "kvit_test");
            Directory.CreateDirectory(baseDir);

            return baseDir;
        }

        /// <summary>
        /// This proccess runs with KVIT_BASE_DIR environment variable
        /// It use KVIT_BASE_DIR directory for fetch and push operations
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static (string stdout, string stderr, int exitCode) RunKvitWithBaseDirEnvironmentVariable(string baseDir, string args)
        {
            //var baseDir = CreateRandomBaseDir();

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"run --no-build -- {args}",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = KvitProjectPathRelativeToUnitTestDirectory,
                    Environment =
                    {
                        {"KVIT_BASE_DIR", baseDir},
                        {"KVIT_ADDRESS", TestConsulUrl}
                    }
                }
            };
            process.Start();
            
            var stdout = process.StandardOutput.ReadToEnd();
            var stderr = process.StandardError.ReadToEnd();
            process.WaitForExit();

            return (stdout, stderr, process.ExitCode);
        }

        /// <summary>
        /// This proccess runs without KVIT_BASE_DIR environment variable
        /// It use current working directory for fetch and push operations
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static (string stdout, string stderr, int exitCode) RunKvitWithoutBaseDirEnvironmentVariable(string baseDir, string args)
        {
            var unitTestAssemblyFile = Assembly.GetExecutingAssembly().Location; // Kvit.IntegrationTests.dll
            var unitTestAssemblyDirectory = new FileInfo(unitTestAssemblyFile).Directory.FullName;
            var projectAbsolutePath = Path.Combine(unitTestAssemblyDirectory, KvitProjectPathRelativeToUnitTestDirectory);

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $" run --no-build --project {projectAbsolutePath} -- {args}",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = baseDir,
                    Environment =
                    {
                        {"KVIT_ADDRESS", TestConsulUrl},
                    }
                }
            };
            process.Start();

            var stdout = process.StandardOutput.ReadToEnd();
            var stderr = process.StandardError.ReadToEnd();
            process.WaitForExit();

            return (stdout, stderr, process.ExitCode);
        }

        internal static void BuildKvit()
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"build",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = KvitProjectPathRelativeToUnitTestDirectory,
                }
            };
            process.Start();
            process.WaitForExit();
        }
    }
}
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Kvit.IntegrationTests.TestHelpers
{
    public static class ProcessHelper
    {
        private const string KvitProjectPathRelativeToUnitTestDirectory = "../../../../../src/Kvit";
        internal const string TestConsulUrl = "http://localhost:8900";

        internal static (string baseDir, string stdout, string stderr) RunKvit(bool runWithBaseDir, string args)
        {
            return runWithBaseDir
                ? RunKvitWithBaseDirEnvironmentVariable(args)
                : RunKvitWithoutBaseDirEnvironmentVariable(args);
        }

        private static string GetRandomBaseDir() => Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "kvit_test");

        /// <summary>
        /// This proccess runs with KVIT_BASE_DIR environment variable
        /// It use KVIT_BASE_DIR directory for fetch and push operations
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static (string baseDir, string stdout, string stderr) RunKvitWithBaseDirEnvironmentVariable(string args)
        {
            var randomBaseDir = GetRandomBaseDir();

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
                        {"KVIT_BASE_DIR", randomBaseDir},
                        {"KVIT_ADDRESS", TestConsulUrl}
                    }
                }
            };
            process.Start();

            var stdout = process.StandardOutput.ReadToEnd();
            var stderr = process.StandardError.ReadToEnd();

            return (randomBaseDir, stdout, stderr);
        }

        /// <summary>
        /// This proccess runs without KVIT_BASE_DIR environment variable
        /// It use current working directory for fetch and push operations
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static (string baseDir, string stdout, string stderr) RunKvitWithoutBaseDirEnvironmentVariable(string args)
        {
            var randomBaseDir = GetRandomBaseDir();
            Directory.CreateDirectory(randomBaseDir);
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
                    WorkingDirectory = randomBaseDir,
                    Environment =
                    {
                        {"KVIT_ADDRESS", TestConsulUrl},
                    }
                }
            };
            process.Start();

            var stdout = process.StandardOutput.ReadToEnd();
            var stderr = process.StandardError.ReadToEnd();

            return (randomBaseDir, stdout, stderr);
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
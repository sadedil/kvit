using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Kvit.Commands;
using Kvit.IntegrationTests.TestHelpers;
using Shouldly;
using Xunit;

namespace Kvit.IntegrationTests
{
    [Collection(nameof(CliCollection))]
    public class DiffTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Kvit_Diff_ShouldShow_Missing_Files_between_FileSystem_and_Consul(bool runWithBaseDir)
        {
            // Arrange
            await ConsulTestHelper.DeleteAllKeys();

            var baseDir = ProcessTestHelper.CreateRandomBaseDir();

            // Populate the file system
            FileTestHelper.WriteAllText(Path.Combine(baseDir, "file"), @"""text""");
            FileTestHelper.WriteAllText(Path.Combine(baseDir, "folder1", "file2"), @"{""myNameIsFile2"": ""yes""}");
            FileTestHelper.WriteAllText(Path.Combine(baseDir, "folder1", "folder1.1", "file3"), @"{""iaminasubfolder"": ""absolutely""}");
            FileTestHelper.WriteAllText(Path.Combine(baseDir, "file0"), @"""555""");
            FileTestHelper.WriteAllText(Path.Combine(baseDir, "key1"), @"""value""");
            FileTestHelper.WriteAllText(Path.Combine(baseDir, "folder1", "file1"), "true");

            // Populate the Consul instance
            await ConsulTestHelper.AddDirectoryToConsulAsync("dir1");
            await ConsulTestHelper.AddDataToConsulAsync("file0", "555");
            await ConsulTestHelper.AddDataToConsulAsync("key1", "value1");
            await ConsulTestHelper.AddDataToConsulAsync("folder1/file1", "true");
            await ConsulTestHelper.AddDataToConsulAsync("key", "value");
            await ConsulTestHelper.AddDataToConsulAsync("dir1/key1_in_dir1", "value2");
            await ConsulTestHelper.AddDataToConsulAsync("dir2/dir3/dir4/key_in_subfolder", "value3");

            // Act
            var (stdout, stderr, exitCode) = ProcessTestHelper.RunKvit(runWithBaseDir, baseDir, "diff --all");

            // Assert
            var stdoutLines = stdout.Split(Environment.NewLine).ToArray();

            stdoutLines.ShouldSatisfyAllConditions(
                () => stdoutLines[2].ShouldContain("Files"),
                () => stdoutLines[2].ShouldContain("Consul - Local")
            );
            stdoutLines.ShouldSatisfyAllConditions(
                () => stdoutLines[4].ShouldContain("file0"),
                () => stdoutLines[4].ShouldContain(DiffCommand.FileContentsAreEqualSign)
            );
            stdoutLines.ShouldSatisfyAllConditions(
                () => stdoutLines[5].ShouldContain("folder1/file1"),
                () => stdoutLines[5].ShouldContain(DiffCommand.FileContentsAreDifferentSign)
            );
            stdoutLines.ShouldSatisfyAllConditions(
                () => stdoutLines[6].ShouldContain("key1"),
                () => stdoutLines[6].ShouldContain(DiffCommand.FileContentsAreDifferentSign)
            );
            stdoutLines.ShouldSatisfyAllConditions(
                () => stdoutLines[7].ShouldContain("file"),
                () => stdoutLines[7].ShouldContain(DiffCommand.OnlyInFileSystemSign)
            );
            stdoutLines.ShouldSatisfyAllConditions(
                () => stdoutLines[8].ShouldContain("folder1/file2"),
                () => stdoutLines[8].ShouldContain(DiffCommand.OnlyInFileSystemSign)
            );
            stdoutLines.ShouldSatisfyAllConditions(
                () => stdoutLines[9].ShouldContain("folder1/folder1.1/file3"),
                () => stdoutLines[9].ShouldContain(DiffCommand.OnlyInFileSystemSign)
            );
            stdoutLines.ShouldSatisfyAllConditions(
                () => stdoutLines[10].ShouldContain("dir1/key1_in_dir1"),
                () => stdoutLines[10].ShouldContain(DiffCommand.OnlyInConsulSign)
            );
            stdoutLines.ShouldSatisfyAllConditions(
                () => stdoutLines[11].ShouldContain("dir2/dir3/dir4/key_in_subfolder"),
                () => stdoutLines[11].ShouldContain(DiffCommand.OnlyInConsulSign)
            );
            stdoutLines.ShouldSatisfyAllConditions(
                () => stdoutLines[11].ShouldContain("key"),
                () => stdoutLines[11].ShouldContain(DiffCommand.OnlyInConsulSign)
            );

            exitCode.ShouldBe(2);
            stderr.ShouldBeEmpty();
        }
    }
}
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Kvit.IntegrationTests.TestHelpers;
using Shouldly;
using Xunit;

namespace Kvit.IntegrationTests
{
    [Collection(nameof(CliCollection))]
    public class CliTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Kvit_Version_ShouldReturn_Semver_Result(bool runWithBaseDir)
        {
            // Arrange & Act
            var baseDir = ProcessHelper.CreateRandomBaseDir();
            var (stdout, stderr) = ProcessHelper.RunKvit(runWithBaseDir, baseDir, "--version");

            // Assert
            stdout.ShouldMatch(@"\d+\.\d+\.\d+");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Kvit_Help_ShouldReturn_UsageInfo(bool runWithBaseDir)
        {
            // Arrange & Act
            var baseDir = ProcessHelper.CreateRandomBaseDir();
            var (stdout, stderr) = ProcessHelper.RunKvit(runWithBaseDir, baseDir, "--help");

            // Assert
            stdout.ShouldContain("Kvit:");
            stdout.ShouldContain("Usage:");
            stdout.ShouldContain("Options:");
            stdout.ShouldContain("Commands:");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Kvit_WithoutArguments_ShouldShow_UsageInfo(bool runWithBaseDir)
        {
            // Arrange & Act
            var baseDir = ProcessHelper.CreateRandomBaseDir();
            var (stdoutHelp, stderrHelp) = ProcessHelper.RunKvit(runWithBaseDir, baseDir, "--help");
            var (stdoutDefault, stderrDefault) = ProcessHelper.RunKvit(runWithBaseDir, baseDir, "");

            // Assert
            stdoutHelp.ShouldBe(stdoutDefault);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Kvit_Fetch_ShouldCreate_Keys_and_Folders_on_FileSystem(bool runWithBaseDir)
        {
            // Arrange
            await ConsulHelper.DeleteAllKeys();
            await ConsulHelper.AddDirectoryToConsulAsync("dir1");
            await ConsulHelper.AddDataToConsulAsync("key1", "value1");
            await ConsulHelper.AddDataToConsulAsync("dir1/key1_in_dir1", "value2");
            await ConsulHelper.AddDataToConsulAsync("dir2/dir3/dir4/key_in_subfolder", "value3");

            // Act
            var baseDir = ProcessHelper.CreateRandomBaseDir();
            var (stdout, stderr) = ProcessHelper.RunKvit(runWithBaseDir, baseDir, "fetch");

            // Assert
            stdout.ShouldContain("All keys successfully fetched");
            stderr.ShouldBeEmpty();

            // Assert files
            var fetchedFiles = Directory.GetFiles(baseDir, "*.*", SearchOption.AllDirectories);
            var file1 = fetchedFiles.FirstOrDefault(f => f.EndsWith("key1"));
            var file2 = fetchedFiles.FirstOrDefault(f => f.EndsWith($"dir1{Path.DirectorySeparatorChar}key1_in_dir1"));
            var file3 = fetchedFiles.FirstOrDefault(f => f.EndsWith($"dir2{Path.DirectorySeparatorChar}dir3{Path.DirectorySeparatorChar}dir4{Path.DirectorySeparatorChar}key_in_subfolder"));

            file1.ShouldNotBeNull();
            file2.ShouldNotBeNull();
            file3.ShouldNotBeNull();

            // Assert File Contents
            var file1Content = await File.ReadAllTextAsync(file1);
            var file2Content = await File.ReadAllTextAsync(file2);
            var file3Content = await File.ReadAllTextAsync(file3);

            file1Content.ShouldBe(@"""value1""");
            file2Content.ShouldBe(@"""value2""");
            file3Content.ShouldBe(@"""value3""");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Kvit_Push_ShouldCreate_Keys_and_Folders_on_Consul(bool runWithBaseDir)
        {
            // Arrange
            await ConsulHelper.DeleteAllKeys();

            var baseDir = ProcessHelper.CreateRandomBaseDir();
            FileHelper.WriteAllText(Path.Combine(baseDir, "file0"), "555");
            FileHelper.WriteAllText(Path.Combine(baseDir, "folder1", "file1"), "true");
            FileHelper.WriteAllText(Path.Combine(baseDir, "folder1", "file2"), @"{""myNameIsFile2"": ""yes""}");
            FileHelper.WriteAllText(Path.Combine(baseDir, "folder1", "folder1.1", "file3"), @"{""iaminasubfolder"": ""absolutely""}");

            // Act
            var (stdout, stderr) = ProcessHelper.RunKvit(runWithBaseDir, baseDir, "push");

            // Assert
            stdout.ShouldContain("key(s) pushed");
            stderr.ShouldBeEmpty();

            // Assert keys
            var file0Content = await ConsulHelper.GetValueFromConsulAsync("file0");
            var file1Content = await ConsulHelper.GetValueFromConsulAsync("folder1/file1");
            var file2Content = await ConsulHelper.GetValueFromConsulAsync("folder1/file2");
            var file3Content = await ConsulHelper.GetValueFromConsulAsync("folder1/folder1.1/file3");

            file0Content.ShouldBe("555");
            file1Content.ShouldBe("true");
            file2Content.ShouldBe(@"{""myNameIsFile2"": ""yes""}");
            file3Content.ShouldBe(@"{""iaminasubfolder"": ""absolutely""}");
        }
    }
}
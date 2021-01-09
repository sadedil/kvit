using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Kvit.IntegrationTests
{
    public class FetchTests
    {
        [Fact]
        public async Task Kvit_Version_ShouldReturn_Semver_Result()
        {
            // Arrange & Act
            var (stdout, stderr) = ProcessHelper.RunKvit("--version");

            // Assert
            stdout.ShouldMatch(@"\d+\.\d+\.\d+");
        }

        [Fact]
        public async Task Kvit_Help_ShouldReturn_UsageInfo()
        {
            // Arrange & Act
            var (stdout, stderr) = ProcessHelper.RunKvit("--help");

            // Assert
            stdout.ShouldContain("Kvit:");
            stdout.ShouldContain("Usage:");
            stdout.ShouldContain("Options:");
            stdout.ShouldContain("Commands:");
        }

        [Fact]
        public async Task Kvit_WithoutArguments_ShouldShow_UsageInfo()
        {
            // Arrange & Act
            var (stdoutHelp, stderrHelp) = ProcessHelper.RunKvit("--help");
            var (stdoutDefault, stderrDefault) = ProcessHelper.RunKvit("");

            // Assert
            stdoutHelp.ShouldBe(stdoutDefault);
        }

        [Fact]
        public async Task Kvit_Fetch_ShouldCreate_Keys_and_Folders()
        {
            // Arrange
            await ConsulHelper.AddDirectoryToConsulAsync("dir1");
            await ConsulHelper.AddDataToConsulAsync("key1", "value1");
            await ConsulHelper.AddDataToConsulAsync("dir1/key1_in_dir1", "value2");
            await ConsulHelper.AddDataToConsulAsync("dir2/dir3/dir4/key_in_subfolder", "value3");

            // Act
            var (stdout, stderr) = ProcessHelper.RunKvit("fetch");

            // Assert
            stdout.ShouldContain("All keys successfully fetched");
            stderr.ShouldBeEmpty();

            // Assert files
            var fetchedFiles = Directory.GetFiles(ProcessHelper.TestConsulBaseDir, "*.*", SearchOption.AllDirectories);
            var file1 = fetchedFiles.FirstOrDefault(f => f.EndsWith("key1"));
            var file2 = fetchedFiles.FirstOrDefault(f => f.EndsWith("dir1/key1_in_dir1"));
            var file3 = fetchedFiles.FirstOrDefault(f => f.EndsWith("dir2/dir3/dir4/key_in_subfolder"));

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
    }
}
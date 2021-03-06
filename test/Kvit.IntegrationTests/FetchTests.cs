using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Kvit.IntegrationTests.TestHelpers;
using Shouldly;
using Xunit;

namespace Kvit.IntegrationTests
{
    [Collection(nameof(CliCollection))]
    public class FetchTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Kvit_Fetch_ShouldCreate_Keys_and_Folders_on_FileSystem(bool runWithBaseDir)
        {
            // Arrange
            await ConsulTestHelper.DeleteAllKeys();
            await ConsulTestHelper.AddDirectoryToConsulAsync("dir1");
            await ConsulTestHelper.AddDataToConsulAsync("key1", "value1");
            await ConsulTestHelper.AddDataToConsulAsync("dir1/key1_in_dir1", "value2");
            await ConsulTestHelper.AddDataToConsulAsync("dir2/dir3/dir4/key_in_subfolder", "value3");

            // Act
            var baseDir = ProcessTestHelper.CreateRandomBaseDir();
            var (stdout, stderr, exitCode) = ProcessTestHelper.RunKvit(runWithBaseDir, baseDir, "fetch");

            // Assert
            stdout.ShouldContain("All keys successfully fetched");
            stderr.ShouldBeEmpty();
            exitCode.ShouldBe(0);

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
    }
}
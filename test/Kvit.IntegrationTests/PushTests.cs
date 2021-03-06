using System.IO;
using System.Threading.Tasks;
using Kvit.IntegrationTests.TestHelpers;
using Shouldly;
using Xunit;

namespace Kvit.IntegrationTests
{
    [Collection(nameof(CliCollection))]
    public class PushTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Kvit_Push_ShouldCreate_Keys_and_Folders_on_Consul(bool runWithBaseDir)
        {
            // Arrange
            await ConsulTestHelper.DeleteAllKeys();

            var baseDir = ProcessTestHelper.CreateRandomBaseDir();
            FileTestHelper.WriteAllText(Path.Combine(baseDir, "file0"), "555");
            FileTestHelper.WriteAllText(Path.Combine(baseDir, "folder1", "file1"), "true");
            FileTestHelper.WriteAllText(Path.Combine(baseDir, "folder1", "file2"), @"{""myNameIsFile2"": ""yes""}");
            FileTestHelper.WriteAllText(Path.Combine(baseDir, "folder1", "folder1.1", "file3"), @"{""iaminasubfolder"": ""absolutely""}");

            // Act
            var (stdout, stderr, exitCode) = ProcessTestHelper.RunKvit(runWithBaseDir, baseDir, "push");

            // Assert
            stdout.ShouldContain("key(s) pushed");
            stderr.ShouldBeEmpty();
            exitCode.ShouldBe(0);

            // Assert keys
            var file0Content = await ConsulTestHelper.GetValueFromConsulAsync("file0");
            var file1Content = await ConsulTestHelper.GetValueFromConsulAsync("folder1/file1");
            var file2Content = await ConsulTestHelper.GetValueFromConsulAsync("folder1/file2");
            var file3Content = await ConsulTestHelper.GetValueFromConsulAsync("folder1/folder1.1/file3");

            file0Content.ShouldBe("555");
            file1Content.ShouldBe("true");
            file2Content.ShouldBe(@"{""myNameIsFile2"": ""yes""}");
            file3Content.ShouldBe(@"{""iaminasubfolder"": ""absolutely""}");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Kvit_Push_ShouldIgnore_OperatingSystemSpecific_Files(bool runWithBaseDir)
        {
            // Arrange
            await ConsulTestHelper.DeleteAllKeys();

            var baseDir = ProcessTestHelper.CreateRandomBaseDir();
            FileTestHelper.WriteAllText(Path.Combine(baseDir, "file0"), "555");
            FileTestHelper.WriteAllText(Path.Combine(baseDir, ".DS_Store"), "bla bla for mac");
            FileTestHelper.WriteAllText(Path.Combine(baseDir, "desktop.ini"), "bla bla for windows");
            FileTestHelper.WriteAllText(Path.Combine(baseDir, "subfolder", ".DS_Store"), "bla bla for mac in sub folder");
            FileTestHelper.WriteAllText(Path.Combine(baseDir, "subfolder", "desktop.ini"), "bla bla for windows in sub folder");

            // Act
            var (stdout, stderr, exitCode) = ProcessTestHelper.RunKvit(runWithBaseDir, baseDir, "push");

            // Assert
            stdout.ShouldContain("key(s) pushed");
            stderr.ShouldBeEmpty();
            exitCode.ShouldBe(0);

            // Assert keys
            var file0Content = await ConsulTestHelper.GetValueFromConsulAsync("file0");
            var dsStoreContent = await ConsulTestHelper.GetValueFromConsulAsync(".DS_Store");
            var desktopIniContent = await ConsulTestHelper.GetValueFromConsulAsync("desktop.ini");
            var dsStoreInSubFolderContent = await ConsulTestHelper.GetValueFromConsulAsync("subfolder/.DS_Store");
            var desktopIniInSubFolderContent = await ConsulTestHelper.GetValueFromConsulAsync("subfolder/desktop.ini");
            
            file0Content.ShouldBe("555");
            dsStoreContent.ShouldBeNullOrEmpty();
            desktopIniContent.ShouldBeNullOrEmpty();
            dsStoreInSubFolderContent.ShouldBeNullOrEmpty();
            desktopIniInSubFolderContent.ShouldBeNullOrEmpty();
        }
    }
}
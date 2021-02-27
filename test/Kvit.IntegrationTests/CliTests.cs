using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kvit.IntegrationTests.TestHelpers;
using Newtonsoft.Json;
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
            var (stdout, stderr, exitCode) = ProcessHelper.RunKvit(runWithBaseDir, baseDir, "--version");

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
            var (stdout, stderr, exitCode) = ProcessHelper.RunKvit(runWithBaseDir, baseDir, "--help");

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
            var (stdoutHelp, stderrHelp, exitCodeHelp) = ProcessHelper.RunKvit(runWithBaseDir, baseDir, "--help");
            var (stdoutDefault, stderrDefault, exitCodeDefault) = ProcessHelper.RunKvit(runWithBaseDir, baseDir, "");

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
            var (stdout, stderr, exitCode) = ProcessHelper.RunKvit(runWithBaseDir, baseDir, "fetch");

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
            var (stdout, stderr, exitCode) = ProcessHelper.RunKvit(runWithBaseDir, baseDir, "push");

            // Assert
            stdout.ShouldContain("key(s) pushed");
            stderr.ShouldBeEmpty();
            exitCode.ShouldBe(0);

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

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Kvit_Diff_ShouldShow_Missing_Files_between_FileSystem_and_Consul(bool runWithBaseDir)
        {
            // Arrange
            await ConsulHelper.DeleteAllKeys();

            var baseDir = ProcessHelper.CreateRandomBaseDir();

            // missingFilesOnConsul
            FileHelper.WriteAllText(Path.Combine(baseDir, "file"), @"""text""");
            FileHelper.WriteAllText(Path.Combine(baseDir, "folder1", "file2"), @"{""myNameIsFile2"": ""yes""}");
            FileHelper.WriteAllText(Path.Combine(baseDir, "folder1", "folder1.1", "file3"), @"{""iaminasubfolder"": ""absolutely""}");
            FileHelper.WriteAllText(Path.Combine(baseDir, "file0"), @"""555""");
            FileHelper.WriteAllText(Path.Combine(baseDir, "key1"), @"""value""");
            FileHelper.WriteAllText(Path.Combine(baseDir, "folder1", "file1"), "true");

            // missingFilesOnFileSystem
            await ConsulHelper.AddDirectoryToConsulAsync("dir1");
            await ConsulHelper.AddDataToConsulAsync("file0", "555");
            await ConsulHelper.AddDataToConsulAsync("key1", "value1");
            await ConsulHelper.AddDataToConsulAsync("folder1/file1", "true");
            await ConsulHelper.AddDataToConsulAsync("key", "value");
            await ConsulHelper.AddDataToConsulAsync("dir1/key1_in_dir1", "value2");
            await ConsulHelper.AddDataToConsulAsync("dir2/dir3/dir4/key_in_subfolder", "value3");
            
            // Act
            var (stdout, stderr, exitCode) = ProcessHelper.RunKvit(runWithBaseDir, baseDir, "diff");
            
            // Assert
            var stdoutLines = stdout.Split("\n").Select(x => x.Split("│")).ToArray();

            stdoutLines.ShouldSatisfyAllConditions(
                () => stdoutLines[2][1].ShouldContain("Files"),
                () => stdoutLines[2][2].ShouldContain("Consul - Local")
            );
            stdoutLines.ShouldSatisfyAllConditions(
                () => stdoutLines[4][1].ShouldContain("file0"),
                () => stdoutLines[4][2].ShouldContain("==")
            );
            stdoutLines.ShouldSatisfyAllConditions(
                () => stdoutLines[5][1].ShouldContain("folder1/file1"),
                () => stdoutLines[5][2].ShouldContain("xx")
            );
            stdoutLines.ShouldSatisfyAllConditions(
                () => stdoutLines[6][1].ShouldContain("key1"),
                () => stdoutLines[6][2].ShouldContain("xx")
            );
            stdoutLines.ShouldSatisfyAllConditions(
                () => stdoutLines[7][1].ShouldContain("file"),
                () => stdoutLines[7][2].ShouldContain("<<")
            );
            stdoutLines.ShouldSatisfyAllConditions(
                () => stdoutLines[8][1].ShouldContain("folder1/file2"),
                () => stdoutLines[8][2].ShouldContain("<<")
            );
            stdoutLines.ShouldSatisfyAllConditions(
                () => stdoutLines[9][1].ShouldContain("folder1/folder1.1/file3"),
                () => stdoutLines[9][2].ShouldContain("<<")
            );
            stdoutLines.ShouldSatisfyAllConditions(
                () => stdoutLines[10][1].ShouldContain("dir1/key1_in_dir1"),
                () => stdoutLines[10][2].ShouldContain(">>")
            );
            stdoutLines.ShouldSatisfyAllConditions(
                () => stdoutLines[11][1].ShouldContain("dir2/dir3/dir4/key_in_subfolder"),
                () => stdoutLines[11][2].ShouldContain(">>")
            );
            stdoutLines.ShouldSatisfyAllConditions(
                () => stdoutLines[11][1].ShouldContain("key"),
                () => stdoutLines[11][2].ShouldContain(">>")
            );

            exitCode.ShouldBe(2);
            stderr.ShouldBeEmpty();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Kvit_Diff_ShouldShow_File_Contents_are_Equal_between_FileSystem_and_Consul(bool runWithBaseDir)
        {
            // Arrange
            await ConsulHelper.DeleteAllKeys();

            var baseDir = ProcessHelper.CreateRandomBaseDir();
            
            var testObj = new {
                key1 = "value1",
                key2 = new {
                    key3 = "value3",
                    key4 = new {
                        key5 = "value5"
                    }
                }
            };
            
            FileHelper.WriteAllText(Path.Combine(baseDir, "file1"), JsonConvert.SerializeObject(testObj, Formatting.Indented));

            await ConsulHelper.AddDataToConsulAsync("file1", testObj);

            // Act
            var (stdout, stderr, exitCode) = ProcessHelper.RunKvit(runWithBaseDir, baseDir, "diff --file file1");

            // Assert
            var stdoutLines = stdout.Split("\n").Select(x => x.Split("│")).ToArray();

            stdoutLines.ShouldSatisfyAllConditions(
                () => stdoutLines[2][1].ShouldContain("Consul"),
                () => stdoutLines[2][2].ShouldContain("Local")
            );
            stdoutLines.ShouldSatisfyAllConditions(
                () => stdoutLines[5][1].ShouldContain(@"""key1"": ""value1"""),
                () => stdoutLines[5][2].ShouldContain(@"""key1"": ""value1""")
            );
            stdoutLines.ShouldSatisfyAllConditions(
                () => stdoutLines[7][1].ShouldContain(@"""key3"": ""value3"""),
                () => stdoutLines[7][2].ShouldContain(@"""key3"": ""value3""")
            );
            stdoutLines.ShouldSatisfyAllConditions(
                () => stdoutLines[9][1].ShouldContain(@"""key5"": ""value5"""),
                () => stdoutLines[9][2].ShouldContain(@"""key5"": ""value5""")
            );

            exitCode.ShouldBe(0);
            stderr.ShouldBeEmpty();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Kvit_Diff_ShouldShow_File_Contents_not_Equal_between_FileSystem_and_Consul(bool runWithBaseDir)
        {
            // Arrange
            await ConsulHelper.DeleteAllKeys();

            var baseDir = ProcessHelper.CreateRandomBaseDir();
            
            var localObj = new {
                key1 = "value1",
                key2 = new {
                    key3 = "value3",
                    key4 = new {
                        key5 = "value5"
                    }
                }
            };
            
            FileHelper.WriteAllText(Path.Combine(baseDir, "file1"), JsonConvert.SerializeObject(localObj, Formatting.Indented));

            var consulObj = new {
                key1 = "value1",
                key2 = new {
                    key3 = "value2",
                    key4 = new {
                        key5 = "value"
                    }
                }
            };
            await ConsulHelper.AddDataToConsulAsync("file1", consulObj);

            // Act
            var (stdout, stderr, exitCode) = ProcessHelper.RunKvit(runWithBaseDir, baseDir, "diff --file file1");

            // Assert
            var stdoutLines = stdout.Split("\n").Select(x => x.Split("│")).ToArray();

            stdoutLines.ShouldSatisfyAllConditions(
                () => stdoutLines[2][1].ShouldContain("Consul"),
                () => stdoutLines[2][2].ShouldContain("Local")
            );
            stdoutLines.ShouldSatisfyAllConditions(
                () => stdoutLines[5][1].ShouldContain(@"""key1"": ""value1"""),
                () => stdoutLines[5][2].ShouldContain(@"""key1"": ""value1""")
            );
            stdoutLines.ShouldSatisfyAllConditions(
                () => stdoutLines[7][1].ShouldContain(@"""value2"""),
                () => stdoutLines[7][2].ShouldContain(@"""value3""")
            );
            stdoutLines.ShouldSatisfyAllConditions(
                () => stdoutLines[9][1].ShouldContain(@"""value"""),
                () => stdoutLines[9][2].ShouldContain(@"""value5""")
            );

            exitCode.ShouldBe(2);
            stderr.ShouldBeEmpty();
        }
    }
}
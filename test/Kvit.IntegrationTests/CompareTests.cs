using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Kvit.IntegrationTests.TestHelpers;
using Newtonsoft.Json;
using Shouldly;
using Xunit;

namespace Kvit.IntegrationTests
{
    [Collection(nameof(CliCollection))]
    public class CompareTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Kvit_Compare_ShouldShow_File_Contents_are_Equal_between_FileSystem_and_Consul(bool runWithBaseDir)
        {
            // Arrange
            await ConsulTestHelper.DeleteAllKeys();

            var baseDir = ProcessTestHelper.CreateRandomBaseDir();

            var testObj = new
            {
                key1 = "value1",
                key2 = new
                {
                    key3 = "value3",
                    key4 = new
                    {
                        key5 = "value5"
                    }
                }
            };

            // ReSharper disable once MethodHasAsyncOverload
            FileTestHelper.WriteAllText(Path.Combine(baseDir, "file1"), JsonConvert.SerializeObject(testObj, Formatting.Indented));

            await ConsulTestHelper.AddDataToConsulAsync("file1", testObj);

            // Act
            var (stdout, stderr, exitCode) = ProcessTestHelper.RunKvit(runWithBaseDir, baseDir, "compare file1");

            // Assert
            var stdoutLines = stdout
                .Split(Environment.NewLine)
                .Select(ProcessTestHelper.StripAnsiEscapeCodes)
                .ToArray();

            stdoutLines.ShouldSatisfyAllConditions(
                () => stdoutLines[2].ShouldContain("Consul"),
                () => stdoutLines[2].ShouldContain("Local")
            );
            stdoutLines.ShouldSatisfyAllConditions(
                () => stdoutLines[5].ShouldContain(@"""key1"": ""value1"""),
                () => stdoutLines[5].ShouldContain(@"""key1"": ""value1""")
            );
            stdoutLines.ShouldSatisfyAllConditions(
                () => stdoutLines[7].ShouldContain(@"""key3"": ""value3"""),
                () => stdoutLines[7].ShouldContain(@"""key3"": ""value3""")
            );
            stdoutLines.ShouldSatisfyAllConditions(
                () => stdoutLines[9].ShouldContain(@"""key5"": ""value5"""),
                () => stdoutLines[9].ShouldContain(@"""key5"": ""value5""")
            );

            exitCode.ShouldBe(0);
            stderr.ShouldBeEmpty();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Kvit_Compare_ShouldShow_File_Contents_not_Equal_between_FileSystem_and_Consul(bool runWithBaseDir)
        {
            // Arrange
            await ConsulTestHelper.DeleteAllKeys();

            var baseDir = ProcessTestHelper.CreateRandomBaseDir();

            var localObj = new
            {
                key1 = "value1",
                key2 = new
                {
                    key3 = "value3",
                    key4 = new
                    {
                        key5 = "value5"
                    }
                }
            };

            // ReSharper disable once MethodHasAsyncOverload
            FileTestHelper.WriteAllText(Path.Combine(baseDir, "file1"), JsonConvert.SerializeObject(localObj, Formatting.Indented));

            var consulObj = new
            {
                key1 = "value1",
                key2 = new
                {
                    key3 = "value2",
                    key4 = new
                    {
                        key5 = "value"
                    }
                }
            };
            await ConsulTestHelper.AddDataToConsulAsync("file1", consulObj);

            // Act
            var (stdout, stderr, exitCode) = ProcessTestHelper.RunKvit(runWithBaseDir, baseDir, "compare file1");

            // Assert
            var stdoutLines = stdout
                .Split(Environment.NewLine)
                .Select(ProcessTestHelper.StripAnsiEscapeCodes)
                .ToArray();

            stdoutLines.ShouldSatisfyAllConditions(
                () => stdoutLines[2].ShouldContain("Consul"),
                () => stdoutLines[2].ShouldContain("Local")
            );
            stdoutLines.ShouldSatisfyAllConditions(
                () => stdoutLines[5].ShouldContain(@"""key1"": ""value1"""),
                () => stdoutLines[5].ShouldContain(@"""key1"": ""value1""")
            );
            stdoutLines.ShouldSatisfyAllConditions(
                () => stdoutLines[7].ShouldContain(@"""value2"""),
                () => stdoutLines[7].ShouldContain(@"""value3""")
            );
            stdoutLines.ShouldSatisfyAllConditions(
                () => stdoutLines[9].ShouldContain(@"""value"""),
                () => stdoutLines[9].ShouldContain(@"""value5""")
            );

            exitCode.ShouldBe(2);
            stderr.ShouldBeEmpty();
        }
    }
}
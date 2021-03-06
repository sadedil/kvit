using Kvit.IntegrationTests.TestHelpers;
using Shouldly;
using Xunit;

namespace Kvit.IntegrationTests
{
    [Collection(nameof(CliCollection))]
    public class VersionTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Kvit_Version_ShouldReturn_Semver_Result(bool runWithBaseDir)
        {
            // Arrange & Act
            var baseDir = ProcessTestHelper.CreateRandomBaseDir();
            var (stdout, stderr, exitCode) = ProcessTestHelper.RunKvit(runWithBaseDir, baseDir, "--version");

            // Assert
            stdout.ShouldMatch(@"\d+\.\d+\.\d+");
        }
    }
}
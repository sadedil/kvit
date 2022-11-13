using Kvit.IntegrationTests.TestHelpers;
using Shouldly;
using Xunit;

namespace Kvit.IntegrationTests
{
    [Collection(nameof(CliCollection))]
    public class HelpTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Kvit_Help_ShouldReturn_UsageInfo(bool runWithBaseDir)
        {
            // Arrange & Act
            var baseDir = ProcessTestHelper.CreateRandomBaseDir();
            var (stdout, stderr, exitCode) = ProcessTestHelper.RunKvit(runWithBaseDir, baseDir, "--help");

            // Assert
            stdout.ShouldContain("Description:");
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
            var baseDir = ProcessTestHelper.CreateRandomBaseDir();
            var (stdoutHelp, stderrHelp, exitCodeHelp) = ProcessTestHelper.RunKvit(runWithBaseDir, baseDir, "--help");
            var (stdoutDefault, stderrDefault, exitCodeDefault) = ProcessTestHelper.RunKvit(runWithBaseDir, baseDir, "");

            // Assert
            stdoutHelp.ShouldBe(stdoutDefault);
        }
    }
}
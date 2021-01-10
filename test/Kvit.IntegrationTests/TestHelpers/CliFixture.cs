using System;
using JetBrains.Annotations;

namespace Kvit.IntegrationTests.TestHelpers
{
    [UsedImplicitly]
    internal class CliFixture : IDisposable
    {
        public CliFixture()
        {
            // Let's build our real app once
            // Then we can use like "dotnet run --no-build"
            ProcessHelper.BuildKvit();
        }

        public void Dispose()
        {
        }
    }
}
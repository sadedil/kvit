using Xunit;

namespace Kvit.IntegrationTests.TestHelpers
{
    [CollectionDefinition(nameof(CliCollection))]
    public class CliCollection : ICollectionFixture<CliFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
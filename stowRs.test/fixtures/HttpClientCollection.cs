using Xunit;

namespace stowRs.test.fixtures
{
    [CollectionDefinition("Http client collection")]
    public class HttpClientCollection: ICollectionFixture<StowRsTestFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
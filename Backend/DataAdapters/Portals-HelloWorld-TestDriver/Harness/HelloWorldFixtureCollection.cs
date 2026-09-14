using Xunit;

namespace SmintIo.Portals.ConnectorSDK.TestDriver.HelloWorld.Test.Harness
{
    [CollectionDefinition(nameof(HelloWorldFixtureCollection))]
    public class HelloWorldFixtureCollection : ICollectionFixture<HelloWorldFixture>
    {
    }
}

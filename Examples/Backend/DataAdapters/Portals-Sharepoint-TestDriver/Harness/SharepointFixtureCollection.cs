using Xunit;

namespace SmintIo.Portals.ConnectorSDK.TestDriver.Sharepoint.Test.Harness
{
    [CollectionDefinition(nameof(SharepointFixtureCollection))]
    public class SharepointFixtureCollection : ICollectionFixture<SharepointFixture>
    {
    }
}

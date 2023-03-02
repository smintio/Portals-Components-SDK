using Xunit;

namespace SmintIo.Portals.ConnectorSDK.TestDriver.Picturepark.Test.Harness
{
    [CollectionDefinition(nameof(PictureparkFixtureCollection))]
    public class PictureparkFixtureCollection : ICollectionFixture<PictureparkFixture>
    {
    }
}

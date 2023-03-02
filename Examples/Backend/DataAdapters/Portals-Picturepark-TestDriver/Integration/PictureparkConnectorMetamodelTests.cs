using System;
using SmintIo.Portals.Connector.Test.Tests;
using SmintIo.Portals.ConnectorSDK.Metamodel;
using SmintIo.Portals.ConnectorSDK.TestDriver.Picturepark.Test.Harness;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets;
using Xunit;

namespace SmintIo.Portals.ConnectorSDK.TestDriver.Picturepark.Test.Integration
{
    [Collection(nameof(PictureparkFixtureCollection))]
    public class PictureparkConnectorMetamodelTests : ConnectorMetamodelTests
    {
        private readonly PictureparkFixture _fixture;

        public PictureparkConnectorMetamodelTests(PictureparkFixture fixture)
        {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        }

        protected override bool UseIntegrationLayer => _fixture.DataAdapter is IAssetsIntegrationLayerApiProvider;

        protected override ConnectorMetamodel GetConnectorMetamodel() => _fixture.Metamodel;
    }
}

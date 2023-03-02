using System;
using SmintIo.Portals.Connector.Test.Tests;
using SmintIo.Portals.ConnectorSDK.Metamodel;
using SmintIo.Portals.ConnectorSDK.TestDriver.Sharepoint.Test.Harness;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets;
using Xunit;

namespace SmintIo.Portals.ConnectorSDK.TestDriver.Sharepoint.Test.Integration
{
    [Collection(nameof(SharepointFixtureCollection))]
    public class SharePointConnectorMetamodelTests : ConnectorMetamodelTests
    {
        private readonly SharepointFixture _fixture;

        public SharePointConnectorMetamodelTests(SharepointFixture fixture)
        {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        }

        protected override bool UseIntegrationLayer => _fixture.DataAdapter is IAssetsIntegrationLayerApiProvider;

        protected override ConnectorMetamodel GetConnectorMetamodel() => _fixture.Metamodel;
    }
}

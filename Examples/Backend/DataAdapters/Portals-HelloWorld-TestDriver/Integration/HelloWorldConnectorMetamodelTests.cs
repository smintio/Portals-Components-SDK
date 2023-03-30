using System;
using SmintIo.Portals.Connector.Test.Tests;
using SmintIo.Portals.ConnectorSDK.Metamodel;
using SmintIo.Portals.ConnectorSDK.TestDriver.HelloWorld.Test.Harness;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets;
using Xunit;

namespace SmintIo.Portals.ConnectorSDK.TestDriver.HelloWorld.Test.Integration
{
    /// <summary>
    /// Common Smint.Io tests that cover the core functionality of the <see cref="ConnectorMetamodel"/>
    /// </summary>
    [Collection(nameof(HelloWorldFixtureCollection))]
    public class HelloWorldConnectorMetamodelTests : ConnectorMetamodelTests
    {
        private readonly HelloWorldFixture _fixture;

        public HelloWorldConnectorMetamodelTests(HelloWorldFixture fixture)
        {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        }

        protected override bool UseIntegrationLayer => _fixture.DataAdapter is IAssetsIntegrationLayerApiProvider;

        protected override ConnectorMetamodel GetConnectorMetamodel() => _fixture.Metamodel;
    }
}

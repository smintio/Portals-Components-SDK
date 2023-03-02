using System;
using SmintIo.Portals.ConnectorSDK.Connectors;
using SmintIo.Portals.ConnectorSDK.TestDriver.Sharepoint.Test.Harness;
using SmintIo.Portals.DataAdapter.Test.Tests;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Impl;
using SmintIo.Portals.DataAdapterSDK.TestDriver.Models;
using Xunit;

namespace SmintIo.Portals.ConnectorSDK.TestDriver.Sharepoint.Test.Integration
{
    [Collection(nameof(SharepointFixtureCollection))]
    public class SharePointDataAdapterIntegrationLayerStartupTests : DataAdapterIntegrationLayerStartupTests
    {
        private readonly SharepointFixture _fixture;

        public SharePointDataAdapterIntegrationLayerStartupTests(SharepointFixture fixture)
        {
            _fixture = fixture;
        }

        protected override AssetItemOption SampleImageAsset => _fixture.AssetOptions.ImageAsset;

        protected override AssetItemOption SampleVideoAsset => _fixture.AssetOptions.VideoAsset;

        protected override AssetItemOption SampleAudioAsset => _fixture.AssetOptions.AudioAsset;

        protected override AssetItemOption SampleDocumentAsset => _fixture.AssetOptions.DocumentAsset;

        protected override IConnector GetConnector() => _fixture.Connector;

        protected override DataAdapterBaseImpl GetDataAdapter(Type componentImplementation) => _fixture.DataAdapter;
    }
}

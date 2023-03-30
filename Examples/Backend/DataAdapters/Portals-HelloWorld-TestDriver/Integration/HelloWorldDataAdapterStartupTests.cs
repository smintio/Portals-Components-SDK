using System;
using SmintIo.Portals.ConnectorSDK.Connectors;
using SmintIo.Portals.ConnectorSDK.TestDriver.HelloWorld.Test.Harness;
using SmintIo.Portals.DataAdapter.Test.Tests;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Impl;
using SmintIo.Portals.DataAdapterSDK.TestDriver.Models;
using Xunit;

namespace SmintIo.Portals.ConnectorSDK.TestDriver.HelloWorld.Test.Integration
{
    /// <summary>
    /// Common Smint.Io tests that cover the core functionality of the data adapter startup
    /// </summary>
    [Collection(nameof(HelloWorldFixtureCollection))]
    public class HelloWorldDataAdapterStartupTests : DataAdapterStartupTests
    {
        private readonly HelloWorldFixture _fixture;

        public HelloWorldDataAdapterStartupTests(HelloWorldFixture fixture)
        {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        }

        protected override AssetItemOption SampleImageAsset => _fixture.AssetOptions.ImageAsset;

        protected override AssetItemOption SampleVideoAsset => _fixture.AssetOptions.VideoAsset;

        protected override AssetItemOption SampleAudioAsset => _fixture.AssetOptions.AudioAsset;

        protected override AssetItemOption SampleDocumentAsset => _fixture.AssetOptions.DocumentAsset;

        protected override IConnector GetConnector() => _fixture.Connector;

        protected override DataAdapterBaseImpl GetDataAdapter(Type componentImplementation) => _fixture.DataAdapter;
    }
}

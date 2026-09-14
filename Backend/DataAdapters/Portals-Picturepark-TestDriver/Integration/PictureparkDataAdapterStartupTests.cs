using System;
using FluentAssertions;
using SmintIo.Portals.ConnectorSDK.Connectors;
using SmintIo.Portals.ConnectorSDK.TestDriver.Picturepark.Test.Harness;
using SmintIo.Portals.DataAdapter.Picturepark.ExternalUsers;
using SmintIo.Portals.DataAdapter.Test.Tests;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Impl;
using SmintIo.Portals.DataAdapterSDK.TestDriver.Models;
using SmintIo.Portals.SDK.Core.Components;
using Xunit;

namespace SmintIo.Portals.ConnectorSDK.TestDriver.Picturepark.Test.Integration
{
    [Collection(nameof(PictureparkFixtureCollection))]
    public class PictureparkDataAdapterStartupTests : DataAdapterStartupTests
    {
        private readonly PictureparkFixture _fixture;

        public PictureparkDataAdapterStartupTests(PictureparkFixture fixture)
        {
            _fixture = fixture;
        }

        protected override AssetItemOption SampleImageAsset => _fixture.AssetOptions.ImageAsset;

        protected override AssetItemOption SampleVideoAsset => _fixture.AssetOptions.VideoAsset;

        protected override AssetItemOption SampleAudioAsset => _fixture.AssetOptions.AudioAsset;

        protected override AssetItemOption SampleDocumentAsset => _fixture.AssetOptions.DocumentAsset;

        protected override IConnector GetConnector() => _fixture.Connector;

        protected override IComponentConfiguration GetDataAdapterComponentConfiguration() => _fixture.DataAdapterConfiguration;

        protected override DataAdapterBaseImpl GetDataAdapter(Type componentImplementation)
        {
            if (componentImplementation == typeof(PictureparkExternalUsersDataAdapter))
            {
                return _fixture.ExternalUsersDataAdapter;
            }

            return _fixture.DataAdapter;
        }

        public override void Startup_Type_Should_Have_Preserve_Metadata_DataAdapter_Configuration()
        {
            AssertDataAdapterStartup(das =>
            {
                if (das.ComponentImplementation == typeof(PictureparkExternalUsersDataAdapter))
                {
                    return;
                }

                PreserveMetadataDataAdapterConfiguration.IsAssignableFrom(das.ConfigurationImplementation).Should().BeTrue();
            });
        }

        public override void Startup_Type_Should_Have_Output_Format_DataAdapter_Configuration()
        {
            AssertDataAdapterStartup(das =>
            {
                if (das.ComponentImplementation == typeof(PictureparkExternalUsersDataAdapter))
                {
                    return;
                }

                OutputFormatDataAdapterConfiguration.IsAssignableFrom(das.ConfigurationImplementation).Should().BeTrue();
            });
        }
    }
}

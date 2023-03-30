using System;
using FluentAssertions;
using SmintIo.Portals.ConnectorSDK.Metamodel;
using SmintIo.Portals.ConnectorSDK.TestDriver.HelloWorld.Test.Harness;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Impl;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets.Models;
using SmintIo.Portals.DataAdapterSDK.TestDriver.Harness;
using SmintIo.Portals.DataAdapterSDK.TestDriver.Models;
using SmintIo.Portals.DataAdapterSDK.TestDriver.Tests;
using SmintIo.Portals.SDK.Core.Models.Metamodel.Data;
using Xunit;

namespace SmintIo.Portals.ConnectorSDK.TestDriver.HelloWorld.Test.Integration
{
    /// <summary>
    /// Common Smint.Io tests that cover the core functionality of the data adapter assets
    /// </summary>
    [Collection(nameof(HelloWorldFixtureCollection))]
    public class HelloWorldDataAdapterAssetsTests : DataAdapterAssetsTests
    {
        private readonly HelloWorldFixture _fixture;

        public HelloWorldDataAdapterAssetsTests(HelloWorldFixture fixture)
        {
            _fixture = fixture;
        }

        protected override bool SupportAssetVersions => false;

        protected override string SampleFormGroupDefinitionId => _fixture.AssetOptions.FormGroupDefinitionId;

        protected override string SampleFormGroupDefinitionName => _fixture.AssetOptions.FormGroupDefinitionName;

        protected override string SampleFormItemDefinitionId => _fixture.AssetOptions.FormItemDefinitionId;

        protected override string SampleFormItemDefinitionQueryString => _fixture.AssetOptions.FormItemDefinitionQueryString;

        protected override int? SamplePage => 0;

        protected override AssetItemOption SampleImageAsset => _fixture.AssetOptions.ImageAsset;

        protected override AssetItemOption SampleVideoAsset => _fixture.AssetOptions.VideoAsset;

        protected override AssetItemOption SampleAudioAsset => _fixture.AssetOptions.AudioAsset;

        protected override AssetItemOption SampleDocumentAsset => _fixture.AssetOptions.DocumentAsset;

        protected override AssetIdentifier ValidButNotFoundAssetIdentifier => new(_fixture.AssetOptions.ValidButNotFoundAssetIdentifier);

        protected override ConnectorMetamodel GetConnectorMetamodel() => _fixture.Metamodel;

        protected override AssetsDataAdapterBaseImpl GetDataAdapter(Type componentImplementation) => _fixture.DataAdapter;

        protected override void AssertAssetThumbnailDownloadStream(AssetDownloadStreamModel assetDownloadStreamModel)
        {
            assetDownloadStreamModel.Should().NotBeNull();

            assetDownloadStreamModel.FileSizeInBytes.Should().BePositive();
            assetDownloadStreamModel.MediaType.Should().NotBeEmpty();
            assetDownloadStreamModel.Stream.Should().NotBeNull();
        }

        protected override void AssertBasicAssetDataObjectProperties(AssetDataObject assetDataObject)
        {
            assetDataObject.Id.Should().NotBeNullOrEmpty();
            assetDataObject.Name.Should().NotBeEmpty();

            assetDataObject.ContentType.Should().NotBeNull();

            assetDataObject.ETag.Should().NotBeNullOrEmpty();
        }

        protected override void AssertProgressMonitor(object result, TestProgressMonitor progressMonitor)
        {
            result.Should().NotBeNull();

            progressMonitor.CurrentValue.Should().Be(progressMonitor.Maximum);
            progressMonitor.ReportProgressInvocations.Should().Be(2);
            progressMonitor.FinishedInvocations.Should().Be(1);
        }

        protected override void AssertAvailableThumbnails(AssetDataObject assetDataObject)
        {
            assetDataObject.IsThumbnailPreviewAvailable.Should().BeTrue();
            assetDataObject.IsThumbnailLargeAvailable.Should().BeTrue();
            assetDataObject.IsThumbnailMediumAvailable.Should().BeTrue();
            assetDataObject.IsThumbnailSmallAvailable.Should().BeTrue();

            if (assetDataObject.ContentType.Id == ContentTypeEnumDataObject.Video.Id)
            {
                assetDataObject.IsPlaybackLargeAvailable.Should().BeTrue();
            }
            else if (assetDataObject.ContentType.Id == ContentTypeEnumDataObject.Audio.Id)
            {
                assetDataObject.IsPlaybackSmallAvailable.Should().BeTrue();
            }
            else if (assetDataObject.ContentType.Id == ContentTypeEnumDataObject.Document.Id)
            {
                assetDataObject.IsPdfPreviewAvailable.Should().BeTrue();
            }
        }
    }
}

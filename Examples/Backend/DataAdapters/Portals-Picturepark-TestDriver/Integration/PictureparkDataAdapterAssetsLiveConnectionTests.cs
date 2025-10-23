using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using SmintIo.Portals.ConnectorSDK.Metamodel;
using SmintIo.Portals.ConnectorSDK.TestDriver.Picturepark.Test.Harness;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Impl;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets.Parameters;
using SmintIo.Portals.DataAdapterSDK.TestDriver.Harness;
using SmintIo.Portals.DataAdapterSDK.TestDriver.Models;
using SmintIo.Portals.DataAdapterSDK.TestDriver.Tests;
using SmintIo.Portals.SDK.Core.Components;
using SmintIo.Portals.SDK.Core.Models.Metamodel.Data;
using Xunit;

namespace SmintIo.Portals.ConnectorSDK.TestDriver.Picturepark.Test.Integration
{
    [Collection(nameof(PictureparkFixtureCollection))]
    public class PictureparkDataAdapterAssetsLiveConnectionTests : DataAdapterAssetsLiveConnectionTests
    {
        private readonly PictureparkFixture _fixture;

        public PictureparkDataAdapterAssetsLiveConnectionTests(PictureparkFixture fixture)
        {
            _fixture = fixture;
        }

        protected override bool SupportAssetVersions => false;

        protected override string SampleFormGroupDefinitionId => _fixture.AssetOptions.FormGroupDefinitionId;

        protected override string SampleFormGroupDefinitionName => _fixture.AssetOptions.FormGroupDefinitionName;

        protected override string SampleFormItemDefinitionId => _fixture.AssetOptions.FormItemDefinitionId;

        protected override string SampleFormItemDefinitionQueryString => _fixture.AssetOptions.FormItemDefinitionQueryString;

        protected override string DefaultPlayBackPreviewOutputFormat => "VideoLarge";

        protected override AssetItemOption SampleImageAsset => _fixture.AssetOptions.ImageAsset;

        protected override AssetItemOption SampleVideoAsset => _fixture.AssetOptions.VideoAsset;

        protected override AssetItemOption SampleAudioAsset => _fixture.AssetOptions.AudioAsset;

        protected override AssetItemOption SampleDocumentAsset => _fixture.AssetOptions.DocumentAsset;

        protected override AssetIdentifier ValidButNotFoundAssetIdentifier => new(_fixture.AssetOptions.ValidButNotFoundAssetIdentifier);

        protected override ConnectorMetamodel GetConnectorMetamodel() => _fixture.Metamodel;

        protected override IComponentConfiguration GetDataAdapterComponentConfiguration() => _fixture.DataAdapterConfiguration;

        protected override AssetsDataAdapterBaseImpl GetDataAdapter(Type componentImplementation) => _fixture.DataAdapter;

        protected override void AssertProgressMonitor(object result, TestProgressMonitor progressMonitor)
        {
            result.Should().NotBeNull();
        }

        protected override void AssertBasicAssetDataObjectProperties(AssetDataObject assetDataObject)
        {
            assetDataObject.Id.Should().NotBeNullOrEmpty();
            assetDataObject.Name.Should().NotBeEmpty();
            assetDataObject.CreatedAt.Should().HaveValue();
            assetDataObject.ModifiedAt.Should().HaveValue();

            assetDataObject.ContentType.Should().NotBeNull();

            assetDataObject.Version.Should().NotBeNullOrEmpty();
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
                assetDataObject.IsPlaybackSmallAvailable.Should().BeTrue();
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

        protected override void AssertRawData(AssetDataObject assetDataObject)
        {
            assetDataObject.RawData.Should().NotBeNull();
            assetDataObject.RawData.Should().HaveCountGreaterOrEqualTo(0);

            if (!assetDataObject.RawData.Any())
            {
                Assert.Fail("No raw data");
            }

            var connectorMetamodel = GetConnectorMetamodel();

            if (connectorMetamodel == null)
            {
                Assert.Fail("Unable to get connector metamodel");
            }

            if (connectorMetamodel.Entities == null)
            {
                Assert.Fail("The memamodel has no entities");
            }

            foreach (var rawData in assetDataObject.RawData)
            {
                rawData.Should().NotBeNull();

                rawData.Values.Should().NotBeNull();

                if (rawData.Values.Count == 0)
                {
                    continue;
                }

                rawData.Values.Should().NotContainNulls().And.OnlyHaveUniqueItems();

                var entityModel = connectorMetamodel.Entities.SingleOrDefault(e => e.Key == rawData.MetamodelEntityKey);

                entityModel.Should().NotBeNull($"Raw data '{rawData.MetamodelEntityKey}' should have matching entity model");

                AssertRawDataHasValidStructure(rawData);
            }
        }

        [Fact]
        public Task GetAssetsPermissions_ShouldReturn_ResultAsync()
        {
            return AssertDataAdapterAsync(async da =>
            {
                var getAssetsPermissionsParameters = new GetAssetsPermissionsParameters
                {
                    AssetIds = new[] { SampleImageAssetIdentifier }
                };

                var result = await da.GetAssetsPermissionsAsync(getAssetsPermissionsParameters);

                result.Should().NotBeNull();
                result.AssetDataObjects.Should().NotBeNullOrEmpty().And.NotContainNulls().And.OnlyHaveUniqueItems();
            });
        }

        public override Task GetRandomAssets_ShouldThrow_NotSupported_NoParametersAsync()
        {
            // For PP we return an Image instead of 'not supported exception'

            return AssertDataAdapterAsync(async da =>
            {
                var result = await da.GetRandomAssetsAsync(new GetRandomAssetsParameters());

                AssertContentType(result, ContentTypeEnumDataObject.Image);
            });
        }
    }
}

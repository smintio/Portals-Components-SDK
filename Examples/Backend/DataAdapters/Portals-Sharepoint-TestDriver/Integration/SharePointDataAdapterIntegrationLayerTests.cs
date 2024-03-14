using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using SmintIo.Portals.Connector.SharePoint.Extensions;
using SmintIo.Portals.ConnectorSDK.Metamodel;
using SmintIo.Portals.ConnectorSDK.TestDriver.Sharepoint.Test.Harness;
using SmintIo.Portals.DataAdapter.Test.Tests;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Impl;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets.Models;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets.Parameters;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets.Results;
using SmintIo.Portals.DataAdapterSDK.Providers;
using SmintIo.Portals.DataAdapterSDK.TestDriver.Harness;
using SmintIo.Portals.DataAdapterSDK.TestDriver.Models;
using SmintIo.Portals.DataAdapterSDK.TestDriver.Providers;
using SmintIo.Portals.SDK.Core.Models.Metamodel.Data;
using Xunit;

namespace SmintIo.Portals.ConnectorSDK.TestDriver.Sharepoint.Test.Integration
{
    [Collection(nameof(SharepointFixtureCollection))]
    public class SharePointDataAdapterIntegrationLayerTests : DataAdapterIntegrationLayerTests
    {
        private readonly SharepointFixture _fixture;

        public SharePointDataAdapterIntegrationLayerTests(SharepointFixture fixture)
        {
            _fixture = fixture;
        }

        protected override bool SupportAssetVersions => false;

        protected override int? Page => _fixture.AssetOptions.IntegrationLayerPage;

        protected override int? PageSize => _fixture.AssetOptions.IntegrationLayerPageSize;

        protected override AssetIdentifier UnknownAssetIdentifier => new(_fixture.AssetOptions.UnknownAssetIdentifier);

        protected override AssetIdentifier ValidButNotFoundAssetIdentifier => new(_fixture.AssetOptions.ValidButNotFoundAssetIdentifier);

        protected override string UnknownContinuationId => "https://graph.microsoft.com/v1.0/sites/smintio.sharepoint.com,ea4ab684-207f-42e0-91c7-1cc5686f294c,f828b6a7-30a3-4179-b859-5683f621ed73/drive/root/microsoft.graph.delta()?top=50";

        protected override AssetItemOption SampleImageAsset => _fixture.AssetOptions.ImageAsset;

        protected override AssetItemOption SampleVideoAsset => _fixture.AssetOptions.VideoAsset;

        protected override AssetItemOption SampleAudioAsset => _fixture.AssetOptions.AudioAsset;

        protected override AssetItemOption SampleDocumentAsset => _fixture.AssetOptions.DocumentAsset;

        protected override string ResultSetUuid => _fixture.AssetOptions.IntegrationLayerResultSetUuid;

        protected override string ContinuationId => _fixture.AssetOptions.IntegrationLayerContinuationId;

        protected override FolderItemOption SampleFolder => _fixture.AssetOptions.Folders.FirstOrDefault();

        protected override ICollection<FolderItemOption> SampleFolders => _fixture.AssetOptions.Folders;

        protected override ConnectorMetamodel GetConnectorMetamodel() => _fixture.Metamodel;

        protected override AssetsDataAdapterBaseImpl GetDataAdapter(Type componentImplementation) => _fixture.DataAdapter;

        protected override void AssertBasicAssetDataObjectProperties(AssetDataObject assetDataObject)
        {
            assetDataObject.Id.Should().NotBeNullOrEmpty();
            assetDataObject.Name.Should().NotBeEmpty();
        }

        protected override void AssertAvailableThumbnails(AssetDataObject assetDataObject)
        {
            if (assetDataObject.ContentType.Id == ContentTypeEnumDataObject.Image.Id)
            {
                assetDataObject.IsThumbnailPreviewAvailable.Should().BeTrue();
                assetDataObject.IsThumbnailLargeAvailable.Should().BeTrue();
                assetDataObject.IsThumbnailMediumAvailable.Should().BeTrue();
                assetDataObject.IsThumbnailSmallAvailable.Should().BeTrue();
            }
            else if (assetDataObject.ContentType.Id == ContentTypeEnumDataObject.Video.Id)
            {
                assetDataObject.IsPlaybackLargeAvailable.Should().BeTrue();

                assetDataObject.IsThumbnailPreviewAvailable.Should().BeTrue();
                assetDataObject.IsThumbnailLargeAvailable.Should().BeTrue();
                assetDataObject.IsThumbnailMediumAvailable.Should().BeTrue();
                assetDataObject.IsThumbnailSmallAvailable.Should().BeTrue();
            }
            else if (assetDataObject.ContentType.Id == ContentTypeEnumDataObject.Audio.Id)
            {
                assetDataObject.IsPlaybackLargeAvailable.Should().BeTrue();
            }
            else if (assetDataObject.ContentType.Id == ContentTypeEnumDataObject.Document.Id)
            {
                assetDataObject.IsPdfPreviewAvailable.Should().BeTrue();

                assetDataObject.IsThumbnailPreviewAvailable.Should().BeTrue();
                assetDataObject.IsThumbnailLargeAvailable.Should().BeTrue();
                assetDataObject.IsThumbnailMediumAvailable.Should().BeTrue();
                assetDataObject.IsThumbnailSmallAvailable.Should().BeTrue();
            }
        }

        protected override void AssertContentMetadata(AssetDataObject assetDataObject)
        {
            if (assetDataObject.VideoMetadata != null)
            {
                assetDataObject.VideoMetadata.Height.Should().NotBeNull();
                assetDataObject.VideoMetadata.Width.Should().NotBeNull();
                assetDataObject.ThumbnailAspectRatio.Should().NotBeNull();
            }
        }

        protected override void AssertFolderContentsDetails(GetFolderContentsResult folderContentsResult)
        {
            folderContentsResult.Should().NotBeNull();

            folderContentsResult.Details.Should().NotBeNull();
            folderContentsResult.Details.CurrentItemsPerPage.Should().NotBeNull();
        }

        protected override void AssertGetChanges(ChangeModel[] changes)
        {
            base.AssertGetChanges(changes);

            foreach (var change in changes)
            {
                if (change.Type != ChangeType.FolderUpdate)
                {
                    continue;
                }

                change.RecursionIsHandledByDataAdapter.Should().BeTrue();
            }
        }

        protected override void AssertProgressMonitor(object result, TestProgressMonitor progressMonitor)
        {
            result.Should().NotBeNull();

            progressMonitor.CurrentValue.Should().Be(0);
            progressMonitor.ReportProgressInvocations.Should().Be(0);
            progressMonitor.FinishedInvocations.Should().Be(1);
        }

        protected override async Task RegisterMockupDataAsync()
        {
            var smintIoIntegrationLayerProviderFieldInfo = _fixture.DataAdapter
                .GetType()
                .GetField("_smintIoIntegrationLayerProvider", BindingFlags.NonPublic | BindingFlags.Instance);

            var smintIoIntegrationLayerProvider = (ISmintIoIntegrationLayerProvider)smintIoIntegrationLayerProviderFieldInfo?.GetValue(_fixture.DataAdapter) as InMemorySmintIoIntegrationLayerProvider;

            if (smintIoIntegrationLayerProvider == null || smintIoIntegrationLayerProvider.HasAssetDataObjects)
            {
                return;
            }

            var getFolderContentsParameters = new GetFolderContentsParameters
            {
                FolderId = new FolderIdentifier(SampleFolder.Id)
            };

            var getFolderContentsResult = await (_fixture.DataAdapter as IAssetsIntegrationLayerApiProvider).GetFolderContentsForIntegrationLayerAsync(getFolderContentsParameters);

            if (getFolderContentsResult == null || getFolderContentsResult.AssetDataObjects == null)
            {
                return;
            }

            foreach (var assetDataObject in getFolderContentsResult.AssetDataObjects)
            {
                smintIoIntegrationLayerProvider.SetAsset(assetDataObject);
            }
        }
    }
}

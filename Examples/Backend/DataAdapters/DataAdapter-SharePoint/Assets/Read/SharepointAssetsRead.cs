using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SmintIo.Portals.DataAdapter.SharePoint.Assets.Common;
using SmintIo.Portals.DataAdapter.SharePoint.Assets.Models;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Impl;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets.Models;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets.Parameters;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets.Results;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Progress;
using SmintIo.Portals.SDK.Core.Http.Prefab.Exceptions;
using SmintIo.Portals.SDK.Core.Http.Prefab.Models;
using SmintIo.Portals.SDK.Core.Models.Metamodel.Data;

namespace SmintIo.Portals.DataAdapter.SharePoint.Assets
{
    public partial class SharepointAssetsDataAdapter : AssetsDataAdapterBaseImpl, IAssetsIntegrationLayerApiProvider
    {
        private static readonly Regex _downloadUrlRegex = new Regex("downloadUrl\":\"(.*?)\"", RegexOptions.IgnoreCase);
        private static readonly Regex _spItemUrlRegex = new Regex("g_fileInfo =(.*?); var g_webApplicationUrls", RegexOptions.IgnoreCase);

        protected override bool HasPlaybackPreviewOutputFormat(AssetDataObject assetDataObject)
        {
            return false;
        }

        public async override Task<AssetDownloadStreamModel> GetAssetThumbnailDownloadStreamAsync(
            AssetIdentifier assetId,
            ContentTypeEnumDataObject contentType,
            AssetThumbnailSize assetThumbnailSize,
            string thumbnailSpec)
        {
            if (assetId == null)
            {
                throw new ArgumentNullException(nameof(assetId));
            }

            var useIntegrationLayerProvider = _smintIoIntegrationLayerProvider != null;

            var isStreaming = assetThumbnailSize == AssetThumbnailSize.PlaybackStreaming;

            if (isStreaming)
            {
                // we get the streaming object instead

                useIntegrationLayerProvider = false;
            }

            if (useIntegrationLayerProvider)
            {
                var assetDownloadUrlModel = await _smintIoIntegrationLayerProvider.GetAssetThumbnailDownloadUrlAsync(Context, assetId, contentType, assetThumbnailSize).ConfigureAwait(false);

                if (assetDownloadUrlModel == null || string.IsNullOrEmpty(assetDownloadUrlModel.Url))
                {
                    throw new Exception("Asset download URL is empty");
                }

                var streamResponse = await _sharepointClient.GetHttpStreamResponseWithoutBackoffAsync(assetDownloadUrlModel.Url, cancelRequestDelay: TimeSpan.FromSeconds(5)).ConfigureAwait(false);

                if (streamResponse == null)
                {
                    return null;
                }

                if (streamResponse.Stream == null)
                {
                    throw new Exception("Stream is null");
                }

                return new AssetDownloadStreamModel(
                    streamResponse.FileName,
                    streamResponse.FileSizeInBytes,
                    streamResponse.MediaType,
                    streamResponse.Stream);
            }

            if (string.Equals(thumbnailSpec, SharepointContentConverter.ConvertThumbnailSpec))
            {
                var streamResponse = await _sharepointClient.GetDriveItemPdfContentAsync(assetId.UnscopedId).ConfigureAwait(false);

                if (streamResponse == null)
                {
                    // not supported

                    return null;
                }

                if (streamResponse.Stream == null)
                {
                    throw new Exception("Stream is null");
                }

                var fileName = streamResponse.FileName;

                if (string.IsNullOrEmpty(fileName))
                {
                    fileName = Guid.NewGuid().ToString();
                }

                return new AssetDownloadStreamModel(
                    fileName,
                    streamResponse.FileSizeInBytes,
                    mediaType: streamResponse.MediaType,
                    streamResponse.Stream);
            }

            if (isStreaming ||
                assetThumbnailSize == AssetThumbnailSize.PlaybackLarge ||
                assetThumbnailSize == AssetThumbnailSize.PlaybackSmall)
            {
                var streamResponse = await GetPreviewContentAsync(assetId, contentType, isStreaming).ConfigureAwait(false);

                if (streamResponse == null)
                {
                    return null;
                }

                if (streamResponse.Stream == null)
                {
                    throw new Exception("Stream is null");
                }

                var fileName = streamResponse.FileName;

                if (string.IsNullOrEmpty(fileName))
                {
                    fileName = Guid.NewGuid().ToString();
                }

                return new AssetDownloadStreamModel(
                    fileName,
                    streamResponse.FileSizeInBytes,
                    mediaType: streamResponse.MediaType,
                    streamResponse.Stream);
            }

            var isPdfPreview = assetThumbnailSize == AssetThumbnailSize.PdfPreview;

            if (isPdfPreview)
            {
                var streamResponse = await _sharepointClient.GetDriveItemContentAsync(assetId.UnscopedId).ConfigureAwait(false);

                if (streamResponse == null)
                {
                    return null;
                }

                if (streamResponse.Stream == null)
                {
                    throw new Exception("Stream is null");
                }

                return new AssetDownloadStreamModel(
                    streamResponse.FileName,
                    streamResponse.FileSizeInBytes,
                    streamResponse.MediaType,
                    streamResponse.Stream);
            }
            else
            {
                var driveItem = await _sharepointClient.GetDriveItemAsync(assetId.UnscopedId).ConfigureAwait(false);

                if (driveItem == null)
                {
                    return null;
                }

                var thumbnailSet = driveItem.Thumbnails.FirstOrDefault();

                if (thumbnailSet == null)
                {
                    _logger.LogWarning($"Thumbnail set is null for asset ID {assetId.UnscopedId}, drive item {driveItem.Id} - no thumbnails present, ignoring and returning null");

                    // no thumbnails available

                    return null;
                }

                string thumbnailUrl = null;
                var computePreview = false;

                switch (assetThumbnailSize)
                {
                    case AssetThumbnailSize.Preview:
                        computePreview = true;
                        break;

                    case AssetThumbnailSize.Large:
                        thumbnailUrl = thumbnailSet.Large?.Url;
                        break;

                    case AssetThumbnailSize.Small:
                        thumbnailUrl = thumbnailSet.Small?.Url;
                        break;

                    default:
                        thumbnailUrl = thumbnailSet.Medium?.Url;
                        break;
                }

                StreamResponse streamResponse;

                if (computePreview)
                {
                    streamResponse = await _sharepointClient.GetDriveItemThumbnailContentAsync(assetId.UnscopedId, thumbnailSet.Id, size: "c1600x1600").ConfigureAwait(false);
                }
                else
                {
                    if (string.IsNullOrEmpty(thumbnailUrl))
                    {
                        throw new Exception("Thumbnail is not present");
                    }

                    streamResponse = await _sharepointClient.GetHttpStreamResponseWithSharepointErrorHandlingAsync(thumbnailUrl).ConfigureAwait(false);
                }

                if (streamResponse == null)
                {
                    _logger.LogWarning($"Stream response is null for asset ID {assetId.UnscopedId}, drive item {driveItem.Id} - no thumbnails present, ignoring and returning null");

                    // no thumbnails available

                    return null;
                }

                if (streamResponse.Stream == null)
                {
                    throw new Exception("Stream response stream is null");
                }

                var fileName = streamResponse.FileName;

                if (string.IsNullOrEmpty(fileName))
                {
                    fileName = Guid.NewGuid().ToString();
                }

                return new AssetDownloadStreamModel(
                    fileName,
                    streamResponse.FileSizeInBytes,
                    mediaType: streamResponse.MediaType,
                    streamResponse.Stream);
            }
        }

        private async Task<StreamResponse> GetPreviewContentAsync(AssetIdentifier assetId, ContentTypeEnumDataObject contentType, bool isStreaming)
        {
            var useMediaPresentation = isStreaming && contentType.Id == ContentTypeEnumDataObject.Video.Id;

            var url = await GetAssetItemPreviewUrlAsync(assetId.UnscopedId, useMediaPresentation).ConfigureAwait(false);

            return await _sharepointClient.GetHttpStreamResponseWithSharepointErrorHandlingAsync(url).ConfigureAwait(false);
        }

        private async Task<string> GetAssetItemPreviewUrlAsync(string assetId, bool useMediaPresentation)
        {
            var itemPreviewInfo = await _sharepointClient.GetDriveItemPreviewInfoAsync(assetId).ConfigureAwait(false);

            var hasItemPreviewInfo = !string.IsNullOrEmpty(itemPreviewInfo?.GetUrl);

            if (!hasItemPreviewInfo)
            {
                return null;
            }

            var requestHeaders = new Dictionary<string, string>
            {
                { "user-agent", "Smint.io/1.0.0" }
            };

            var urlContent = await _sharepointClient.GetHttpStreamResponseAsStringAsync(itemPreviewInfo.GetUrl, accessToken: null, requestHeaders).ConfigureAwait(false);

            if (string.IsNullOrEmpty(urlContent))
            {
                return null;
            }

            if (useMediaPresentation)
            {
                var mediaPresentationUrl = GetMediaPresentationUrl(urlContent);

                if (!string.IsNullOrEmpty(mediaPresentationUrl))
                {
                    return mediaPresentationUrl;
                }
            }

            var downloadUrlMatch = _downloadUrlRegex.Match(urlContent);

            if (string.IsNullOrEmpty(downloadUrlMatch.Value) || downloadUrlMatch.Groups.Count < 2)
            {
                throw new InvalidOperationException(
                    $"Could not determine item preview url for asset '{assetId}'. Revisit what Microsoft has changed.");
            }

            var url = downloadUrlMatch.Groups[1].Value;

            return url;
        }

        private string GetMediaPresentationUrl(string rawHtml)
        {
            try
            {
                var sharepointItemPreviewText = _spItemUrlRegex.Match(rawHtml).Groups[1].Value.Trim();

                if (string.IsNullOrEmpty(sharepointItemPreviewText))
                {
                    return null;
                }
                    
                var sharepointItemPreview = JsonConvert.DeserializeObject<SharepointItemPreview>(sharepointItemPreviewText);

                if (string.IsNullOrEmpty(sharepointItemPreview.MediaServiceFastMetadata))
                {
                    return null;
                }
                
                var mediaServiceFastMetadata = JsonConvert.DeserializeObject<MediaServiceFastMetadata>(sharepointItemPreview.MediaServiceFastMetadata);

                if (mediaServiceFastMetadata == null)
                {
                    return null;
                }

                sharepointItemPreview.ParsedMediaServiceFastMetadata = mediaServiceFastMetadata;

                return sharepointItemPreview.ToString();
            }
            catch (Exception ex)
            {
                // We will fall back to piped file stream.

                _logger.LogError(ex, $"SharePoint item preview manifest parsing failed. Revisit what Microsoft has changed.");

                return null;
            }
        }

        public async override Task<AssetDownloadStreamModel> GetAssetDownloadStreamAsync(AssetIdentifier assetId, AssetDownloadItemMappingModel downloadItemMapping)
        {
            if (assetId == null)
            {
                throw new ArgumentNullException(nameof(assetId));
            }

            if (downloadItemMapping == null)
            {
                throw new ArgumentNullException(nameof(downloadItemMapping));
            }

            StreamResponse streamResponse;

            if (Enum.TryParse<AssetThumbnailSize>(downloadItemMapping?.ItemId, out var assetThumbnailSize))
            {
                var assetDownloadUrlModel = await _smintIoIntegrationLayerProvider.GetAssetThumbnailDownloadUrlAsync(Context, assetId, contentType: null, assetThumbnailSize).ConfigureAwait(false);

                if (assetDownloadUrlModel == null || string.IsNullOrEmpty(assetDownloadUrlModel.Url))
                {
                    throw new Exception("Asset download URL is empty");
                }

                streamResponse = await _sharepointClient.GetHttpStreamResponseAsync(assetDownloadUrlModel.Url).ConfigureAwait(false);
            }
            else
            {
                streamResponse = await _sharepointClient.GetDriveItemContentAsync(assetId.UnscopedId).ConfigureAwait(false);
            }

            if (streamResponse == null)
            {
                return null;
            }

            if (streamResponse.Stream == null)
            {
                throw new Exception("Stream is null");
            }

            return new AssetDownloadStreamModel(
                streamResponse.FileName,
                streamResponse.FileSizeInBytes,
                streamResponse.MediaType,
                streamResponse.Stream);
        }

        public override Task<GetAssetResult> GetAssetAsync(GetAssetParameters parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (_smintIoIntegrationLayerProvider == null)
            {
                throw new NotImplementedException();
            }

            return _smintIoIntegrationLayerProvider.GetAssetAsync(Context, parameters);
        }

        public async override Task<GetAssetsResult> GetAssetsAsync(GetAssetsParameters parameters, IProgressMonitor progressMonitor)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (_smintIoIntegrationLayerProvider == null)
            {
                throw new NotImplementedException();
            }

            var unscopedIds = parameters
                .AssetIds?
                .Select(i => i.UnscopedId)
                .ToList();

            if (progressMonitor != null)
            {
                progressMonitor.Maximum = unscopedIds.Count * 2;

                await progressMonitor.ReportProgressAsync(unscopedIds.Count, null).ConfigureAwait(false);
            }

            var assets = await _smintIoIntegrationLayerProvider.GetAssetsAsync(Context, parameters).ConfigureAwait(false);

            if (progressMonitor != null)
            {
                await progressMonitor.ReportProgressAsync(unscopedIds.Count, null).ConfigureAwait(false);
                await progressMonitor.FinishedAsync(null).ConfigureAwait(false);
            }

            return assets;
        }

        public override Task<GetAssetsPermissionsResult> GetAssetsPermissionsAsync(GetAssetsPermissionsParameters parameters)
        {
            return Task.FromResult<GetAssetsPermissionsResult>(null);
        }

        public override Task<FolderDownloadStreamModel> GetFolderThumbnailDownloadStreamAsync(FolderIdentifier assetId, FolderThumbnailSize size, string thumbnailSpec)
        {
            throw new NotImplementedException();
        }
    }
}
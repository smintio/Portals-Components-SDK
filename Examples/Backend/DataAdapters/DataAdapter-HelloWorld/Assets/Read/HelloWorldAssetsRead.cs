using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Logging;
using SmintIo.Portals.Connector.HelloWorld.Models.Responses;
using SmintIo.Portals.DataAdapter.HelloWorld.Assets.Common;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Impl;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets.Models;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets.Parameters;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets.Results;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Progress;
using SmintIo.Portals.SDK.Core.Models.Metamodel.Data;
using SmintIo.Portals.SDK.Core.Rest.Prefab.Exceptions;

namespace SmintIo.Portals.DataAdapter.HelloWorld.Assets
{
    public partial class HelloWorldAssetsDataAdapter : AssetsDataAdapterBaseImpl
    {
        public async override Task<AssetDownloadStreamModel> GetAssetThumbnailDownloadStreamAsync(
            AssetIdentifier assetId,
            ContentTypeEnumDataObject contentType,
            AssetThumbnailSize assetThumbnailSize,
            string thumbnailSpec,
            long? maxFileSizeBytes)
        {
            if (assetId == null)
            {
                throw new ArgumentNullException(nameof(assetId));
            }

            if (contentType == null)
            {
                throw new ArgumentNullException(nameof(contentType));
            }

            if (contentType.Id == ContentTypeEnumDataObject.Unknown.Id || contentType.Id == ContentTypeEnumDataObject.Other.Id)
            {
                throw new NotImplementedException($"Content type '{contentType.Id}'");
            }

            var helloWorldAssetResponse = await _helloWorldClient.GetAssetAsync(assetId.UnscopedId).ConfigureAwait(false);

            if (helloWorldAssetResponse == null)
            {
                return null;
            }

            var extension = helloWorldAssetResponse.FileExtension.ToLower();

            string thumbnailUrl;

            switch (assetThumbnailSize)
            {
                case AssetThumbnailSize.Preview:
                case AssetThumbnailSize.Large:
                case AssetThumbnailSize.Medium:
                case AssetThumbnailSize.Small:
                    thumbnailUrl = GetDynamicAssetThumbnailUrl(helloWorldAssetResponse, thumbnailSpec);

                    break;
                case AssetThumbnailSize.PlaybackLarge:
                    thumbnailUrl = helloWorldAssetResponse.PreviewUrl;

                    if (string.IsNullOrEmpty(thumbnailUrl))
                    {
                        _logger.LogWarning($"No large preview for asset {assetId.UnscopedId} with extension {extension}");

                        return null;
                    }

                    break;
                case AssetThumbnailSize.PlaybackSmall:
                    thumbnailUrl = helloWorldAssetResponse.PreviewUrl;

                    if (string.IsNullOrEmpty(thumbnailUrl))
                    {
                        _logger.LogWarning($"No small record playback preview for asset {assetId.UnscopedId}");

                        return null;
                    }

                    break;
                case AssetThumbnailSize.PdfPreview:
                    if (!string.Equals(extension, "pdf"))
                    {
                        thumbnailUrl = helloWorldAssetResponse.PreviewUrl;

                        if (string.IsNullOrEmpty(thumbnailUrl))
                        {
                            _logger.LogWarning($"No PDF preview for asset {assetId.UnscopedId} with extension {extension}");

                            return null;
                        }
                    }
                    else
                    {
                        thumbnailUrl = helloWorldAssetResponse.DownloadUrl;

                        if (string.IsNullOrEmpty(thumbnailUrl))
                        {
                            _logger.LogWarning($"No original for asset {assetId.UnscopedId}");

                            return null;
                        }
                    }

                    break;
                default:
                    throw new NotImplementedException($"Thumbnail size '{assetThumbnailSize}'");
            }

            if (string.IsNullOrEmpty(thumbnailUrl))
            {
                _logger.LogWarning($"Unable to find thumbnail URL for asset {assetId.UnscopedId} and size {assetThumbnailSize}");

                return null;
            }

            // In real world scenario client.GetHttpClientStreamResponseWithBackoffAsync should be considered. 

            var streamResponse = await _helloWorldClient.GetStreamResponseWithoutBackoffAsync(thumbnailUrl).ConfigureAwait(false);

            if (streamResponse == null)
            {
                return null;
            }

            return new AssetDownloadStreamModel(
                streamResponse.FileName,
                streamResponse.FileSizeInBytes,
                streamResponse.MediaType,
                streamResponse.Stream);
        }

        private static string GetDynamicAssetThumbnailUrl(HelloWorldAssetResponse helloWorldAssetRespons, string thumbnailSpec)
        {
            string thumbnailUrl;

            var previewUriBuilder = new UriBuilder(helloWorldAssetRespons.PreviewUrl);

            var query = HttpUtility.ParseQueryString(previewUriBuilder.Query);

            if (!string.IsNullOrEmpty(thumbnailSpec))
            {
                query["width"] = thumbnailSpec;
            }

            previewUriBuilder.Query = query.ToString();

            thumbnailUrl = previewUriBuilder.Uri.ToString();

            return thumbnailUrl;
        }

        public async override Task<AssetDownloadStreamModel> GetAssetDownloadStreamAsync(AssetIdentifier assetId, AssetDownloadItemMappingModel assetDownloadItemMappingModel, long? maxFileSizeBytes)
        {
            if (assetId == null)
            {
                throw new ArgumentNullException(nameof(assetId));
            }

            if (assetDownloadItemMappingModel == null)
            {
                throw new ArgumentNullException(nameof(assetDownloadItemMappingModel));
            }

            if (assetDownloadItemMappingModel.ItemId == nameof(AssetThumbnailSize.PdfPreview))
            {
                var pdfAssetStreamModel = await GetAssetThumbnailDownloadStreamAsync(assetId, ContentTypeEnumDataObject.Document, AssetThumbnailSize.PdfPreview, thumbnailSpec: null, maxFileSizeBytes).ConfigureAwait(false);

                return pdfAssetStreamModel;
            }
            else if (assetDownloadItemMappingModel.ItemId == nameof(AssetThumbnailSize.Preview))
            {
                var previewAssetStreamModel = await GetAssetThumbnailDownloadStreamAsync(assetId, ContentTypeEnumDataObject.Image, AssetThumbnailSize.Large, thumbnailSpec: null, maxFileSizeBytes).ConfigureAwait(false);

                return previewAssetStreamModel;
            }
            else if (assetDownloadItemMappingModel.ItemId == nameof(AssetThumbnailSize.PlaybackLarge))
            {
                var playbackAssetStreamModel = await GetAssetThumbnailDownloadStreamAsync(assetId, ContentTypeEnumDataObject.Video, AssetThumbnailSize.PlaybackLarge, thumbnailSpec: null, maxFileSizeBytes).ConfigureAwait(false);

                return playbackAssetStreamModel;
            }

            var helloWorldAssetResponse = await _helloWorldClient.GetAssetAsync(assetId.UnscopedId).ConfigureAwait(false);

            if (helloWorldAssetResponse == null || string.IsNullOrEmpty(helloWorldAssetResponse.DownloadUrl))
            {
                return null;
            }

            var streamResponse = await _helloWorldClient.GetStreamResponseWithoutBackoffAsync(helloWorldAssetResponse.DownloadUrl).ConfigureAwait(false);

            if (streamResponse == null)
            {
                return null;
            }

            return new AssetDownloadStreamModel(
                streamResponse.FileName,
                streamResponse.FileSizeInBytes,
                streamResponse.MediaType,
                streamResponse.Stream);
        }

        public override async Task<GetAssetResult> GetAssetAsync(GetAssetParameters parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (parameters.AssetId == null)
            {
                throw new ArgumentNullException(nameof(parameters.AssetId));
            }

            var helloWorldAssetResponse = await _helloWorldClient.GetAssetAsync(parameters.AssetId.UnscopedId.ToString()).ConfigureAwait(false);

            if (helloWorldAssetResponse == null)
            {
                throw new ExternalDependencyException(ExternalDependencyStatusEnum.AssetNotFound, "The asset was not found", parameters.AssetId.UnscopedId);
            }

            var customFieldById = await _helloWorldClient.GetCustomFieldByIdAsync(getFreshData: false).ConfigureAwait(false);

            var converter = new HelloWorldContentConverter(_logger, Context, _entityModelProvider, customFieldById);

            var assetDataObject = converter.GetAssetDataObject(helloWorldAssetResponse);

            return new GetAssetResult
            {
                AssetDataObject = assetDataObject
            };
        }

        public async override Task<GetAssetsResult> GetAssetsAsync(GetAssetsParameters parameters, IProgressMonitor progressMonitor)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (parameters.AssetIds == null)
            {
                throw new ArgumentException(nameof(parameters.AssetIds));
            }

            var unscopedIds = parameters
                .AssetIds
                .Select(i => i.UnscopedId)
                .ToArray();

            if (!unscopedIds.Any())
            {
                return new GetAssetsResult
                {
                    AssetDataObjects = Array.Empty<AssetDataObject>()
                };
            }

            if (progressMonitor != null)
            {
                progressMonitor.Maximum = unscopedIds.Length * 2;

                await progressMonitor.ReportProgressAsync(unscopedIds.Length, null).ConfigureAwait(false);
            }


            var helloWorldAssetResponses = await _helloWorldClient.GetAssetsAsync(unscopedIds).ConfigureAwait(false);

            if (helloWorldAssetResponses == null || helloWorldAssetResponses.Count == 0)
            {
                if (progressMonitor != null)
                {
                    await progressMonitor.ReportProgressAsync(progressMonitor.Maximum, null).ConfigureAwait(false);

                    await progressMonitor.FinishedAsync(null).ConfigureAwait(false);
                }

                return new GetAssetsResult
                {
                    AssetDataObjects = Array.Empty<AssetDataObject>()
                };
            }

            if (helloWorldAssetResponses.Count != unscopedIds.Length)
            {
                throw new ExternalDependencyException(ExternalDependencyStatusEnum.AssetsNotFound, "One of the assets was not found");
            }

            var customFieldById = await _helloWorldClient.GetCustomFieldByIdAsync(getFreshData: false).ConfigureAwait(false);

            var converter = new HelloWorldContentConverter(_logger, Context, _entityModelProvider, customFieldById);

            var assetDataObjects = helloWorldAssetResponses.Assets
                .Select(ri => converter.GetAssetDataObject(ri))
                .ToArray();

            if (progressMonitor != null)
            {
                await progressMonitor.ReportProgressAsync(progressMonitor.Maximum, null).ConfigureAwait(false);

                await progressMonitor.FinishedAsync(null).ConfigureAwait(false);
            }

            return new GetAssetsResult
            {
                AssetDataObjects = assetDataObjects.ToArray()
            };
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;
using SmintIo.Portals.DataAdapter.Picturepark.Assets.Common;
using SmintIo.Portals.DataAdapter.Picturepark.Resources;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Impl;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets.Models;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets.Parameters;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets.Results;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Progress;
using SmintIo.Portals.SDK.Core.Http.Prefab.Models;
using SmintIo.Portals.SDK.Core.Models.Metamodel;
using SmintIo.Portals.SDK.Core.Models.Metamodel.Data;
using SmintIo.Portals.SDK.Core.Models.Strings;
using SmintIo.Portals.SDK.Core.Rest.Prefab.Exceptions;
using PictureParkContentType = Picturepark.SDK.V1.Contract.ContentType;

namespace SmintIo.Portals.DataAdapter.Picturepark.Assets
{
    public partial class PictureparkAssetsDataAdapter : AssetsDataAdapterBaseImpl
    {
        public override async Task<GetAssetResult> GetAssetAsync(GetAssetParameters parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (parameters.AssetId == null)
            {
                throw new ArgumentException(nameof(parameters.AssetId));
            }

            var unscopedId = parameters.AssetId.UnscopedId;

            var (content, originalContentType) = await _client.GetContentAsync(unscopedId);

            var converter = new PictureparkContentConverter(
                _logger,
                Context,
                _entityModelProvider,
                _configuration.ListNameAttribute,
                _configuration.ListNameAttribute2,
                _configuration.ResolveListDataAttributes,
                new PictureparkPostProcessObjectConverter(_entityModelProvider));

            var assetDataObject = converter.Convert(content, titleDisplayPattern: null);

            if (originalContentType != null)
            {
                if (assetDataObject.InternalMetadata == null)
                {
                    assetDataObject.InternalMetadata = new DataObjectInternalMetadata();
                }

                assetDataObject.InternalMetadata.ThumbnailContentType = PictureparkContentConverter.Convert(originalContentType);
            }

            return new GetAssetResult()
            {
                AssetDataObject = assetDataObject
            };
        }

        public override async Task<GetAssetsResult> GetAssetsAsync(GetAssetsParameters parameters, IProgressMonitor progressMonitor)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (parameters.AssetIds == null)
            {
                throw new ArgumentException(nameof(parameters.AssetIds));
            }

            ICollection<ContentDetail> contents = await _client.GetContentsAsync(parameters?.AssetIds?.Select(i => i.UnscopedId).ToList());

            var converter = new PictureparkContentConverter(
                _logger,
                Context,
                _entityModelProvider,
                _configuration.ListNameAttribute,
                _configuration.ListNameAttribute2,
                _configuration.ResolveListDataAttributes,
                new PictureparkPostProcessObjectConverter(_entityModelProvider));

            var assetDataObjects = contents
                .Select(c => converter.Convert(c, _configuration.GalleryTitleDisplayPattern))
                .ToArray();

            return new GetAssetsResult()
            {
                AssetDataObjects = assetDataObjects
            };
        }

        public override async Task<GetAssetsPermissionsResult> GetAssetsPermissionsAsync(GetAssetsPermissionsParameters parameters)
        {
            if (parameters == null || parameters.AssetIds == null)
            {
                return null;
            }

            ICollection<ContentDetail> contents = await _client.GetContentPermissionsAsync(parameters?.AssetIds?.Select(i => i.UnscopedId).ToList());

            var converter = new PictureparkContentConverter(
                _logger,
                Context,
                _entityModelProvider,
                _configuration.ListNameAttribute,
                _configuration.ListNameAttribute2,
                resolveListDataAttributes: null);

            var assetDataObjects = contents
                .Select(c => converter.Convert(c, titleDisplayPattern: null))
                .ToArray();

            return new GetAssetsPermissionsResult()
            {
                AssetDataObjects = assetDataObjects
            };
        }

        public override async Task<GetAssetsDownloadItemMappingsResult> GetAssetsDownloadItemMappingsAsync(GetAssetsDownloadItemMappingsParameters parameters, IProgressMonitor progressMonitor)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (parameters.AssetIds == null)
            {
                throw new ArgumentException(nameof(parameters.AssetIds));
            }

            ICollection<ContentDetail> contents = await _client.GetContentOutputsAsync(parameters?.AssetIds?.Select(i => i.UnscopedId).ToList());

            if (contents == null || contents.Count == 0)
            {
                return new GetAssetsDownloadItemMappingsResult()
                {
                    AssetDownloadItemMappings = new List<AssetDownloadItemMappingsModel>()
                };
            }

            if (contents.Count != parameters.AssetIds.Length)
            {
                throw new ExternalDependencyException(ExternalDependencyStatusEnum.AssetsNotFound, "One of the assets was not found");
            }

            HashSet<string> outputFormatIds = new HashSet<string>();

            foreach (var content in contents)
            {
                if (content.Outputs == null || content.Outputs.Count == 0)
                    continue;

                var outputs = content.Outputs;

                foreach (var output in outputs)
                {
                    var outputFormatId = output.OutputFormatId;

                    if (string.IsNullOrEmpty(outputFormatId))
                        continue;

                    var contentType = PictureparkContentConverter.Convert(content.ContentType);

                    if (!OutputFormatApplies(contentType.Id, outputFormatId))
                    {
                        continue;
                    }

                    outputFormatIds.Add(outputFormatId);
                }
            }

            if (outputFormatIds.Count == 0)
            {
                return new GetAssetsDownloadItemMappingsResult()
                {
                    AssetDownloadItemMappings = new List<AssetDownloadItemMappingsModel>()
                };
            }

            ICollection<OutputFormatDetail> outputFormatDetails = await _client.GetOutputFormatsAsync(outputFormatIds).ConfigureAwait(false);

            if (outputFormatDetails == null || outputFormatDetails.Count == 0)
            {
                return new GetAssetsDownloadItemMappingsResult()
                {
                    AssetDownloadItemMappings = new List<AssetDownloadItemMappingsModel>()
                };
            }

            var mappedOutputFormatDetails = outputFormatDetails.ToDictionary(outputFormatDetail => outputFormatDetail.Id, outputFormatDetail => outputFormatDetail);

            var result = new List<AssetDownloadItemMappingsModel>();

            var converter = new PictureparkContentConverter(
                _logger,
                Context,
                _entityModelProvider,
                _configuration.ListNameAttribute,
                _configuration.ListNameAttribute2,
                resolveListDataAttributes: null);

            foreach (var content in contents)
            {
                var assetDataObject = converter.Convert(content, titleDisplayPattern: null);

                var contentType = assetDataObject.ContentType;

                var assetDownloadItemMappingModels = new List<AssetDownloadItemMappingModel>();

                var outputs = content.Outputs;

                // make sure original is first, if it is present

                outputs = outputs
                    .OrderByDescending(output => string.Equals(output.OutputFormatId, "Original"))
                    .ToList();

                foreach (var output in outputs)
                { 
                    if (output.RenderingState == OutputRenderingState.Failed ||
                        output.RenderingState == OutputRenderingState.NoLicense)
                    {
                        // ignore likely unrecoverable ones...

                        continue;
                    }

                    if (output.RenderingState == OutputRenderingState.Skipped &&
                    output.DynamicRendering != true)
                    {
                        // This is no dynamic render, but skipped
                        // so we skip as well

                        continue;
                    }

                    var outputFormatId = output.OutputFormatId;

                    if (!mappedOutputFormatDetails.TryGetValue(outputFormatId, out var outputOutputFormatDetails))
                    {
                        continue;
                    }

                    var assetDownloadItemMappingModel = new AssetDownloadItemMappingModel()
                    {
                        GroupId = assetDataObject.ContentType.Id,
                        ItemId = outputFormatId,
                        Version = assetDataObject.Version,
                        Description = new LocalizedStringsModel(outputOutputFormatDetails.Names),
                        FileSizeInBytes = output.Detail.FileSizeInBytes,
                        // use this as "intermediate" storage for the default implementation
                        RequiresHiResDownloadPermission = !outputOutputFormatDetails.ViewForAll
                    };

                    assetDownloadItemMappingModel.GroupName = content.ContentType switch
                    {
                        PictureParkContentType.Unknown => MetamodelMessageTranslationsProvider.GetLocalizedStringsModel(nameof(MetamodelMessages.da_assets_unknown)),
                        PictureParkContentType.Bitmap => MetamodelMessageTranslationsProvider.GetLocalizedStringsModel(nameof(MetamodelMessages.da_assets_bitmap)),
                        PictureParkContentType.VectorGraphic => MetamodelMessageTranslationsProvider.GetLocalizedStringsModel(nameof(MetamodelMessages.da_assets_vector_graphic)),
                        PictureParkContentType.RawImage => MetamodelMessageTranslationsProvider.GetLocalizedStringsModel(nameof(MetamodelMessages.da_assets_raw_image)),
                        PictureParkContentType.InterchangeDocument => MetamodelMessageTranslationsProvider.GetLocalizedStringsModel(nameof(MetamodelMessages.da_assets_interchange_document)),
                        PictureParkContentType.WordProcessingDocument => MetamodelMessageTranslationsProvider.GetLocalizedStringsModel(nameof(MetamodelMessages.da_assets_word_processing_document)),
                        PictureParkContentType.TextDocument => MetamodelMessageTranslationsProvider.GetLocalizedStringsModel(nameof(MetamodelMessages.da_assets_text_document)),
                        PictureParkContentType.DesktopPublishingDocument => MetamodelMessageTranslationsProvider.GetLocalizedStringsModel(nameof(MetamodelMessages.da_assets_desktop_publishing_document)),
                        PictureParkContentType.Presentation => MetamodelMessageTranslationsProvider.GetLocalizedStringsModel(nameof(MetamodelMessages.da_assets_presentation)),
                        PictureParkContentType.Spreadsheet => MetamodelMessageTranslationsProvider.GetLocalizedStringsModel(nameof(MetamodelMessages.da_assets_spread_sheet)),
                        PictureParkContentType.Archive => MetamodelMessageTranslationsProvider.GetLocalizedStringsModel(nameof(MetamodelMessages.da_assets_archive)),
                        PictureParkContentType.Audio => MetamodelMessageTranslationsProvider.GetLocalizedStringsModel(nameof(MetamodelMessages.da_assets_audio)),
                        PictureParkContentType.Video => MetamodelMessageTranslationsProvider.GetLocalizedStringsModel(nameof(MetamodelMessages.da_assets_video)),
                        PictureParkContentType.Font => MetamodelMessageTranslationsProvider.GetLocalizedStringsModel(nameof(MetamodelMessages.da_assets_font)),
                        PictureParkContentType.Multimedia => MetamodelMessageTranslationsProvider.GetLocalizedStringsModel(nameof(MetamodelMessages.da_assets_multimedia)),
                        PictureParkContentType.Application => MetamodelMessageTranslationsProvider.GetLocalizedStringsModel(nameof(MetamodelMessages.da_assets_application)),
                        PictureParkContentType.SourceCode => MetamodelMessageTranslationsProvider.GetLocalizedStringsModel(nameof(MetamodelMessages.da_assets_source_code)),
                        PictureParkContentType.Database => MetamodelMessageTranslationsProvider.GetLocalizedStringsModel(nameof(MetamodelMessages.da_assets_database)),
                        PictureParkContentType.Cad => MetamodelMessageTranslationsProvider.GetLocalizedStringsModel(nameof(MetamodelMessages.da_assets_cad)),
                        PictureParkContentType.Model3d => MetamodelMessageTranslationsProvider.GetLocalizedStringsModel(nameof(MetamodelMessages.da_assets_model_3d)),
                        PictureParkContentType.Virtual => MetamodelMessageTranslationsProvider.GetLocalizedStringsModel(nameof(MetamodelMessages.da_assets_virtual)),
                        _ => assetDataObject.ContentType.ListDisplayName,
                    };

                    assetDownloadItemMappingModels.Add(assetDownloadItemMappingModel);
                }

                var finalAssetDownloadItemMappingModels = ReduceAssetDownloadItemMappings(assetDataObject, assetDownloadItemMappingModels);

                var assetDownloadItemMappingsModel = new AssetDownloadItemMappingsModel()
                {
                    AssetDataObject = assetDataObject,
                    AssetDownloadItemMappings = finalAssetDownloadItemMappingModels
                };

                result.Add(assetDownloadItemMappingsModel);
            }

            return new GetAssetsDownloadItemMappingsResult()
            {
                AssetDownloadItemMappings = result
            };
        }

        protected override bool DefaultOutputFormatIsRestricted(string contentTypeId, string outputFormatId)
        {
            return !string.Equals(outputFormatId, "Original") &&
                   !string.Equals(outputFormatId, "HiResJPEG") &&
                   !string.Equals(outputFormatId, "VideoLarge") &&
                   !string.Equals(outputFormatId, "Preview") &&
                   !string.Equals(outputFormatId, "VideoSmall") &&
                   !string.Equals(outputFormatId, "AudioSmall");
        }

        protected override bool DefaultIsHiRes(string outputFormatId, bool requiresHiResDownloadPermission)
        {
            return string.Equals(outputFormatId, "Original") ||
                   string.Equals(outputFormatId, "HiResJPEG") ||
                   string.Equals(outputFormatId, "VideoLarge") ||
                   requiresHiResDownloadPermission;
        }

        public override async Task<AssetDownloadStreamModel> GetAssetThumbnailDownloadStreamAsync(AssetIdentifier assetId, ContentTypeEnumDataObject contentType, AssetThumbnailSize size, string thumbnailSpec, long? maxFileSizeBytes)
        {
            var unscopedId = assetId?.UnscopedId;

            if (!string.IsNullOrEmpty(thumbnailSpec))
            {
                // we have display content ID override

                unscopedId = thumbnailSpec;
            }

            StreamResponse streamResponse;

            if (contentType == ContentTypeEnumDataObject.Video &&
                size == AssetThumbnailSize.PlaybackLarge)
            {
                streamResponse = await _client.GetPlaybackDownloadStreamAsync(unscopedId, "VideoLarge", maxFileSizeBytes).ConfigureAwait(false);
            }
            else if (contentType == ContentTypeEnumDataObject.Video &&
                size == AssetThumbnailSize.PlaybackSmall)
            {
                streamResponse = await _client.GetPlaybackDownloadStreamAsync(unscopedId, "VideoSmall", maxFileSizeBytes).ConfigureAwait(false);
            }
            else if (contentType == ContentTypeEnumDataObject.Audio &&
                size == AssetThumbnailSize.PlaybackSmall)
            {
                streamResponse = await _client.GetPlaybackDownloadStreamAsync(unscopedId, "AudioSmall", maxFileSizeBytes).ConfigureAwait(false);
            }
            else if (size == AssetThumbnailSize.PdfPreview)
            {
                streamResponse = await _client.GetPlaybackDownloadStreamAsync(unscopedId, "Pdf", maxFileSizeBytes).ConfigureAwait(false);
            }
            else
            {
                streamResponse = await _client.GetImageDownloadStreamAsync(unscopedId, ToPictureparkThumbnailSize(size), maxFileSizeBytes).ConfigureAwait(false);
            }

            return GetAssetDownloadStreamModel(streamResponse);
        }

        public override async Task<AssetDownloadStreamModel> GetAssetDownloadStreamAsync(AssetIdentifier assetId, AssetDownloadItemMappingModel downloadItemMapping, long? maxFileSizeBytes)
        {
            if (assetId == null)
            {
                throw new ArgumentNullException(nameof(assetId));
            }

            if (downloadItemMapping == null)
            {
                throw new ArgumentNullException(nameof(downloadItemMapping));
            }

            var streamResponse = await _client.GetDownloadStreamForOutputFormatIdAsync(assetId.UnscopedId, downloadItemMapping.ItemId, maxFileSizeBytes).ConfigureAwait(false);

            return GetAssetDownloadStreamModel(streamResponse);
        }

        private static AssetDownloadStreamModel GetAssetDownloadStreamModel(StreamResponse streamResponse)
        {
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

        private ThumbnailSize ToPictureparkThumbnailSize(AssetThumbnailSize size)
        {
            switch (size)
            {
                case AssetThumbnailSize.Preview: return ThumbnailSize.Preview;
                case AssetThumbnailSize.Large: return ThumbnailSize.Large;
                case AssetThumbnailSize.Medium: return ThumbnailSize.Medium;
                case AssetThumbnailSize.Small: return ThumbnailSize.Small;
                default: throw new NotSupportedException($"Unknown thumbnail size {size}");
            }
        }

        public async Task<ICollection<OutputFormatInfo>> GetOutputFormatsAsync()
        {
            if (_client == null)
                return null;

            return await _client.GetOutputFormatsAsync().ConfigureAwait(false);
        }

        public override Task<FolderDownloadStreamModel> GetFolderThumbnailDownloadStreamAsync(FolderIdentifier assetId, FolderThumbnailSize size, string thumbnailSpec)
        {
            throw new NotImplementedException();
        }
    }
}
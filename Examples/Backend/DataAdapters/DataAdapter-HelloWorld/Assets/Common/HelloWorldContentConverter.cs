using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using SmintIo.Portals.Connector.HelloWorld.Metamodel;
using SmintIo.Portals.Connector.HelloWorld.Models.Common;
using SmintIo.Portals.Connector.HelloWorld.Models.Responses;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Permissions;
using SmintIo.Portals.SDK.Core.Extensions;
using SmintIo.Portals.SDK.Core.Models.Context;
using SmintIo.Portals.SDK.Core.Models.Metamodel;
using SmintIo.Portals.SDK.Core.Models.Metamodel.Data;
using SmintIo.Portals.SDK.Core.Models.Metamodel.Data.ContentTypes;
using SmintIo.Portals.SDK.Core.Models.Metamodel.Model;
using SmintIo.Portals.SDK.Core.Models.Strings;

namespace SmintIo.Portals.DataAdapter.HelloWorld.Assets.Common
{
    public class HelloWorldContentConverter : CustomFieldDictionaryObjectConverter
    {
        public static readonly ICollection<string> _convertibleDocumentExtensions = new HashSet<string>()
        {
            "afp",
            "doc",
            "docx",
            "ppt",
            "pptx"
        };

        private readonly IDataAdapterContextModel _dataAdapterContext;
        private readonly IEntityModelProvider _entityModelProvider;

        public HelloWorldContentConverter(
            ILogger logger,
            IDataAdapterContextModel dataAdapterContext,
            IEntityModelProvider entityModelProvider,
            IDictionary<string, HelloWorldCustomFieldResponse> customFieldById)
            : base(logger, entityModelProvider, customFieldById)
        {
            _dataAdapterContext = dataAdapterContext;
            _entityModelProvider = entityModelProvider;
        }

        public AssetDataObject GetAssetDataObject(HelloWorldAssetResponse helloWorldAssetResponse)
        {
            if (helloWorldAssetResponse == null)
            {
                throw new ArgumentNullException(nameof(helloWorldAssetResponse));
            }

            var contentType = GetContentType(helloWorldAssetResponse.ContentType);

            var localizedTags = GetLocalizedTags(helloWorldAssetResponse.Tags);

            var eTag = $"{helloWorldAssetResponse.FileName}-{helloWorldAssetResponse.FileExtension}-{helloWorldAssetResponse.FileSize}";

            var assetDataObject = new AssetDataObject(_dataAdapterContext)
            {
                Id = helloWorldAssetResponse.Id,
                Name = helloWorldAssetResponse.FileName.Localize(),
                ContentType = contentType,
                CreatedAt = helloWorldAssetResponse.CreatedAt,
                CreatedBy = helloWorldAssetResponse.CreatedBy,
                ModifiedAt = helloWorldAssetResponse.ModifiedAt,
                ModifiedBy = helloWorldAssetResponse.ModifiedBy,
                ParentFolderIds = helloWorldAssetResponse.ParantFolders,
                LocalizedTags = localizedTags,
                Version = helloWorldAssetResponse.Version,
                ETag = eTag
            };

            SetContentTypeMetadata(assetDataObject, helloWorldAssetResponse);

            SetFileMetaData(assetDataObject, helloWorldAssetResponse);

            SetRawData(assetDataObject, helloWorldAssetResponse);

            SetThumbnails(assetDataObject);

            SetPermissionUuids(assetDataObject);

            return assetDataObject;
        }

        private static LocalizedStringsArrayModel GetLocalizedTags(ICollection<HelloWorldTagResponse> tags)
        {
            if (tags == null)
            {
                return null;
            }

            var localizedStringsArrayByCulture = new List<KeyValuePair<string, string[]>>();

            foreach (var tag in tags)
            {
                var defaultTagPair = new KeyValuePair<string, string[]>(LocalizedStringsArrayModel.DefaultCulture, tag.Labels);

                if (tag.LabelsTranslationByCulture != null && tag.LabelsTranslationByCulture.Any())
                {
                    foreach (var labelsTranslationPair in tag.LabelsTranslationByCulture)
                    {
                        var translationPair = new KeyValuePair<string, string[]>(labelsTranslationPair.Key, labelsTranslationPair.Value);

                        localizedStringsArrayByCulture.Add(translationPair);
                    }
                }

                localizedStringsArrayByCulture.Add(defaultTagPair);
            }

            return new LocalizedStringsArrayModel(localizedStringsArrayByCulture);
        }

        private static ContentTypeEnumDataObject GetContentType(HelloWorldContentType contentType)
        {
            return contentType switch
            {
                HelloWorldContentType.Image => ContentTypeEnumDataObject.Image,
                HelloWorldContentType.Video => ContentTypeEnumDataObject.Video,
                HelloWorldContentType.Audio => ContentTypeEnumDataObject.Audio,
                HelloWorldContentType.Document => ContentTypeEnumDataObject.Document,
                _ => ContentTypeEnumDataObject.Other
            };
        }

        private static void SetContentTypeMetadata(AssetDataObject assetDataObject, HelloWorldAssetResponse helloWorldAssetResponse)
        {
            if (assetDataObject.ContentType.Id == ContentTypeEnumDataObject.Image.Id)
            {
                if (helloWorldAssetResponse.ImagePreview != null)
                {
                    assetDataObject.ImageMetadata = new ImageMetadataDataObject
                    {
                        Height = helloWorldAssetResponse.ImagePreview.Height,
                        Width = helloWorldAssetResponse.ImagePreview.Width
                    };
                }
            }
            else if (assetDataObject.ContentType.Id == ContentTypeEnumDataObject.Video.Id)
            {
                if (helloWorldAssetResponse.VideoPreview != null)
                {
                    assetDataObject.VideoMetadata = new VideoMetadataDataObject
                    {
                        Height = helloWorldAssetResponse.VideoPreview.Height,
                        Width = helloWorldAssetResponse.VideoPreview.Width,
                        VideoStreams = new[]
                        {
                            new VideoStreamDataObject
                            {
                                FrameRate = helloWorldAssetResponse.VideoPreview.FrameRate,
                                DurationInSeconds =  helloWorldAssetResponse.VideoPreview.DurationInSeconds
                            }
                        }
                    };
                }
            }

            // Nothing for Audio or Documents
        }

        private static void SetFileMetaData(AssetDataObject assetDataObject, HelloWorldAssetResponse helloWorldAssetResponse)
        {
            assetDataObject.FileMetadata = new FileMetadataDataObject
            {
                Name = assetDataObject.Name,
                FileName = helloWorldAssetResponse.FileName,
                FileExtension = helloWorldAssetResponse.FileExtension,
                FileSizeInBytes = helloWorldAssetResponse.FileSize,
                MediaType = helloWorldAssetResponse.MimeType,
                Title = helloWorldAssetResponse.Title
            };
        }

        private void SetRawData(AssetDataObject assetDataObject, HelloWorldAssetResponse helloWorldAssetResponse)
        {
            if (_entityModelProvider == null)
            {
                return;
            }

            if (helloWorldAssetResponse.CustomFieldValues == null)
            {
                return;
            }

            var objectsByKey = helloWorldAssetResponse.CustomFieldValues.ToDictionary(cfv => $"p_cf_{cfv.CustomFieldId}", cfv => cfv as object);

            objectsByKey[HelloWorldMetamodelBuilder.ContentTypeId] = helloWorldAssetResponse.ContentType;

            var dataObject = GetDataObject(HelloWorldMetamodelBuilder.RootEntityKey, objectsByKey);

            assetDataObject.RawData = new[] { dataObject };
        }

        private static void SetThumbnails(AssetDataObject assetDataObject)
        {
            assetDataObject.IsThumbnailPreviewAvailable = true;
            assetDataObject.IsThumbnailLargeAvailable = true;
            assetDataObject.IsThumbnailMediumAvailable = true;
            assetDataObject.IsThumbnailSmallAvailable = true;

            const string previewSpec = "2048";
            const string largeSpec = "1024";
            const string mediumSpec = "640";
            const string smallSpec = "320";

            assetDataObject.InternalMetadata = new DataObjectInternalMetadata
            {
                PreviewThumbnailSpec = previewSpec,
                LargeThumbnailSpec = largeSpec,
                MediumThumbnailSpec = mediumSpec,
                SmallThumbnailSpec = smallSpec
            };

            if (assetDataObject.ContentType.Id == ContentTypeEnumDataObject.Image.Id)
            {
                if (assetDataObject.ImageMetadata != null)
                {
                    var width = assetDataObject.ImageMetadata.Width;
                    var height = assetDataObject.ImageMetadata.Height;

                    assetDataObject.ThumbnailAspectRatio = ComputeAspectRatio(width, height);
                }
            }
            else if (assetDataObject.ContentType.Id == ContentTypeEnumDataObject.Video.Id)
            {
                assetDataObject.IsPlaybackLargeAvailable = true;

                if (assetDataObject.VideoMetadata != null)
                {
                    var width = assetDataObject.VideoMetadata.Width;
                    var height = assetDataObject.VideoMetadata.Height;

                    assetDataObject.ThumbnailAspectRatio = ComputeAspectRatio(width, height);
                }

                if (assetDataObject.ThumbnailAspectRatio == null)
                {
                    // assume its 16:9
                    // and 16:9 is very common for video (HD, 4K)

                    assetDataObject.ThumbnailAspectRatio = 16.0m / 9.0m;
                }
            }
            else if (assetDataObject.ContentType.Id == ContentTypeEnumDataObject.Audio.Id)
            {
                assetDataObject.IsPlaybackSmallAvailable = true;
            }
            else if (assetDataObject.ContentType.Id == ContentTypeEnumDataObject.Document.Id)
            {
                var fileExtension = assetDataObject.FileMetadata.FileExtension.ToLowerInvariant();

                if (string.Equals(fileExtension, "pdf") || _convertibleDocumentExtensions.Contains(fileExtension))
                {
                    assetDataObject.IsPdfPreviewAvailable = true;
                }
            }
        }

        private static decimal? ComputeAspectRatio(int? width, int? height)
        {
            if (width == null || height == null || height == 0)
            {
                return null;
            }

            return (decimal)width / (decimal)height;
        }

        private static void SetPermissionUuids(AssetDataObject assetDataObject)
        {
            // Since we are not using pass through, it is safe to consider that we have read, search and download permissions.

            assetDataObject.PermissionUuids = new[]
            {
                DataAdapterPermission.SearchAssetsPermissionUuid,
                DataAdapterPermission.ReadAssetDetailsPermissionUuid,
                DataAdapterPermission.DownloadAssetLayoutFilesPermissionUuid,
                DataAdapterPermission.DownloadAssetsHiResPermissionUuid
            };
        }
    }
}
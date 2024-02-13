using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using SmintIo.Portals.Connector.SharePoint.Client;
using SmintIo.Portals.Connector.SharePoint.Extensions;
using SmintIo.Portals.Connector.SharePoint.MicrosoftGraph.Metamodel;
using SmintIo.Portals.Connector.SharePoint.Models;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Converters;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Converters.Extensions;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Permissions;
using SmintIo.Portals.SDK.Core.Extensions;
using SmintIo.Portals.SDK.Core.Models.Context;
using SmintIo.Portals.SDK.Core.Models.Metamodel;
using SmintIo.Portals.SDK.Core.Models.Metamodel.Data;
using SmintIo.Portals.SDK.Core.Models.Metamodel.Data.ContentTypes;
using SmintIo.Portals.SDK.Core.Models.Metamodel.Model;
using SmintIo.Portals.SDK.Core.Models.Other;

namespace SmintIo.Portals.DataAdapter.SharePoint.Assets.Common
{
    public class SharepointContentConverter : JsonElementDictionaryObjectConverter
    {
        public const string ConvertThumbnailSpec = "convert";

        private static readonly ICollection<DataType> _sharepointTranslationPropertyDataTypes = new HashSet<DataType>
        {
            DataType.Enum,
            DataType.DataObject,
            DataType.LocalizedStringsModel,
            DataType.LocalizedStringsArrayModel
        };

        private static readonly IReadOnlyDictionary<string, Action<AssetDataObject, DriveItem, bool>> _contentTypeMetadataActionDict = new Dictionary<string, Action<AssetDataObject, DriveItem, bool>>
        {
            { ContentTypeEnumDataObject.Image.Id, SetImageMetadata },
            { ContentTypeEnumDataObject.Audio.Id, SetAudioMetadata },
            { ContentTypeEnumDataObject.Video.Id, SetVideoMetadata },
            { ContentTypeEnumDataObject.Document.Id, SetDocumentMetadata }
        };

        private readonly bool _fetchAdditionalData;
        private readonly IDataAdapterContextModel _dataAdapterContext;
        private readonly ISharepointClient _sharepointClient;

        public SharepointContentConverter(
            ILogger logger,
            IDataAdapterContextModel dataAdapterContext,
            ISharepointClient sharepointClient,
            IEntityModelProvider entityModelProvider)
            : this(
                  logger,
                  dataAdapterContext,
                  sharepointClient,
                  entityModelProvider,
                  postProcessObjectConverter: entityModelProvider != null ? new SharepointPostProcessObjectConverter(entityModelProvider) : null)
        {
        }

        public SharepointContentConverter(
            ILogger logger,
            IDataAdapterContextModel dataAdapterContext,
            ISharepointClient sharepointClient,
            IEntityModelProvider entityModelProvider,
            IPostProcessObjectConverter postProcessObjectConverter)
            : base(logger, entityModelProvider, postProcessObjectConverter)
        {
            _fetchAdditionalData = entityModelProvider != null;
            _dataAdapterContext = dataAdapterContext;
            _sharepointClient = sharepointClient;
        }

        protected override ICollection<DataType> SupportedTranslationPropertyDataTypes
        {
            get
            {
                return _sharepointTranslationPropertyDataTypes;
            }
        }

        /// <summary>
        /// Converts a GraphAPI-<see cref="DriveItem"/> to a Smintio-<see cref="AssetDataObject"/>.
        /// </summary>
        /// <param name="driveItem">The <see cref="DriveItem"/> that needs to be converted.</param>
        /// <returns>The converted <see cref="AssetDataObject"/> </returns>
        public async Task<AssetDataObject> GetAssetDataObjectAsync(DriveItem driveItem)
        {
            var fileName = driveItem.GetFileName();

            if (string.IsNullOrEmpty(fileName))
            {
                // happens if the file is just .extension or something like that

                fileName = driveItem.Name;
            }

            var fileExtension = driveItem.GetFileExtension();

            var contentType = driveItem.GetContentType(fileExtension);

            // Because of assets search, we want to reduce Sharepoint calls.
            var additionalDataByFieldNames = _fetchAdditionalData
                ? await _sharepointClient.GetDriveItemAdditionalDataByFieldNamesAsync(driveItem).ConfigureAwait(false)
                : null;

            // eTags as well as cTags in Sharepoint unfortunately are not reliable
            // cTags change even if content does not change
            // we need to build some eTag ourselves, approximating and eTag

            // if file name changes
            // if mime type changes (type of file)
            // or if length changes (content of file)
            // lets assume we have a new eTag

            var eTag = $"{fileName}-{driveItem.File?.MimeType}-{driveItem.Size}";

            var version = driveItem.Publication?.VersionId;

            if (string.IsNullOrEmpty(version))
            {
                version = "0";
            }

            var assetDataObject = new AssetDataObject(_dataAdapterContext)
            {
                Id = driveItem.GetAssetId(),
                ExternalId = driveItem.Id,
                Description = driveItem.Description.Localize(),
                Name = fileName.Localize(),
                ContentType = contentType,
                RawData = GetDataObjects(additionalDataByFieldNames),
                Version = version,
                ParentFolderIds = new[] { driveItem.GetParentAssetId() },
                CreatedAt = driveItem.CreatedDateTime,
                ModifiedAt = driveItem.LastModifiedDateTime,
                CreatedBy = driveItem.CreatedBy?.User?.DisplayName,
                ModifiedBy = driveItem.LastModifiedBy?.User?.DisplayName,
                ETag = eTag
            };

            SetFileMetaData(driveItem, assetDataObject, fileName, fileExtension);

            SetThumbnails(assetDataObject, driveItem);

            if (_contentTypeMetadataActionDict.TryGetValue(contentType.Id, out var metaDataAction))
            {
                metaDataAction(assetDataObject, driveItem, _fetchAdditionalData);
            }

            SetPhotoMetaData(assetDataObject, driveItem);

            SetPermissionUuids(assetDataObject);

            return assetDataObject;
        }

        /// <summary>
        /// Converts a GraphAPI-<see cref="DriveItem"/> to a Smintio-<see cref="FolderDataObject"/>.
        /// </summary>
        /// <param name="driveItem">The <see cref="DriveItem"/> that needs to be converted.</param>
        /// <returns>The converted <see cref="FolderDataObject"/> </returns>
        public async Task<FolderDataObject> GetFolderDataObjectAsync(DriveItem driveItem)
        {
            var assetDataObject = await GetAssetDataObjectAsync(driveItem).ConfigureAwait(false);

            var folderDataObject = new FolderDataObject(_dataAdapterContext)
            {
                Id = assetDataObject.Id,
                ExternalId = assetDataObject.ExternalId,
                Name = assetDataObject.Name,
                ParentFolderIds = assetDataObject.ParentFolderIds,
                Version = assetDataObject.Version,
                RawData = assetDataObject.RawData,
                PermissionUuids = assetDataObject.PermissionUuids,
                CreatedAt = assetDataObject.CreatedAt,
                ModifiedAt = assetDataObject.ModifiedAt,
                CreatedBy = assetDataObject.CreatedBy,
                ModifiedBy = assetDataObject.ModifiedBy,
                ETag = null // no content
            };

            return folderDataObject;
        }

        private static void SetFileMetaData(DriveItem driveItem, AssetDataObject assetDataObject, string name, string fileExtension)
        {
            assetDataObject.FileMetadata = new FileMetadataDataObject
            {
                Name = name.Localize(),
                Description = driveItem.Description.Localize(),
                MediaType = driveItem.File?.MimeType,
                Sha1Hash = driveItem.File?.Hashes?.Sha1Hash,
                FileName = driveItem.Name,
                FileExtension = fileExtension,
                FileSizeInBytes = driveItem.Size,
                Creator = driveItem.CreatedBy?.User?.DisplayName,
                Title = name
            };
        }

        private static void SetImageMetadata(AssetDataObject assetDataObject, DriveItem driveItem, bool fetchAdditionalData)
        {
            var isSvg = driveItem.File?.MimeType?.StartsWith("image/svg") ?? false;

            if (isSvg)
            {
                SetVectorMetadata(assetDataObject);

                return;
            }

            assetDataObject.ImageMetadata = new ImageMetadataDataObject();

            if (driveItem.Image == null)
            {
                return;
            }

            assetDataObject.ImageMetadata.Height = driveItem.Image.Height;
            assetDataObject.ImageMetadata.Width = driveItem.Image.Width;

            assetDataObject.ThumbnailAspectRatio = ComputeAspectRatio(driveItem.Image.Width, driveItem.Image.Height);
        }

        private static void SetAudioMetadata(AssetDataObject assetDataObject, DriveItem driveItem, bool fetchAdditionalData)
        {
            assetDataObject.AudioMetadata = new AudioMetadataDataObject();

            if (driveItem.Audio == null)
            {
                return;
            }

            assetDataObject.AudioMetadata.AudioStreams = new AudioStreamDataObject[]
            {
                new AudioStreamDataObject
                {
                    BitRate = driveItem.Audio.Bitrate?.ToString() ?? null,
                    DurationInSeconds = driveItem.Audio.Duration
                }
            };

            assetDataObject.AudioMetadata.Album = driveItem.Audio.Album;
            assetDataObject.AudioMetadata.AlbumArtist = driveItem.Audio.AlbumArtist;
            assetDataObject.AudioMetadata.Artist = driveItem.Audio.Artist;
            assetDataObject.AudioMetadata.Composers = driveItem.Audio.Composers;
            assetDataObject.AudioMetadata.Copyright = driveItem.Audio.Copyright;
            assetDataObject.AudioMetadata.DiscNumber = driveItem.Audio.Disc;
            assetDataObject.AudioMetadata.DiscCount = driveItem.Audio.DiscCount;
            assetDataObject.AudioMetadata.Genre = driveItem.Audio.Genre;
            assetDataObject.AudioMetadata.Drm = driveItem.Audio.HasDrm;
            assetDataObject.AudioMetadata.VariableBitrateEncoding = driveItem.Audio.IsVariableBitrate;
            assetDataObject.AudioMetadata.Title = driveItem.Audio.Title;
            assetDataObject.AudioMetadata.TrackNumber = driveItem.Audio.Track;
            assetDataObject.AudioMetadata.TrackCount = driveItem.Audio.TrackCount;
            assetDataObject.AudioMetadata.Year = driveItem.Audio.Year;
        }

        private static void SetVideoMetadata(AssetDataObject assetDataObject, DriveItem driveItem, bool fetchAdditionalData)
        {
            assetDataObject.VideoMetadata = new VideoMetadataDataObject();

            if (driveItem.Video == null)
            {
                return;
            }

            assetDataObject.VideoMetadata.Width = driveItem.Video.Width;
            assetDataObject.VideoMetadata.Height = driveItem.Video.Height;

            assetDataObject.ThumbnailAspectRatio = ComputeAspectRatio(driveItem.Video.Width, driveItem.Video.Height);

            SetVideoMetadataStreams(assetDataObject, driveItem);
        }

        private static void SetVectorMetadata(AssetDataObject assetDataObject)
        {
            // Sharepoint doesn't return any EpsMetadata.
            assetDataObject.VectorMetadata = new VectorMetadataDataObject();
        }

        private static void SetDocumentMetadata(AssetDataObject assetDataObject, DriveItem driveItem, bool fetchAdditionalData)
        {
            // Sharepoint doesn't return anything that we can map.
            assetDataObject.DocumentMetadata = new DocumentMetadataDataObject();
        }

        private static void SetPhotoMetaData(AssetDataObject assetDataObject, DriveItem driveItem)
        {
            if (driveItem.Photo == null)
            {
                return;
            }

            if (assetDataObject.ImageMetadata == null)
            {
                assetDataObject.ImageMetadata = new ImageMetadataDataObject();
            }

            assetDataObject.ImageMetadata.CameraMake = driveItem.Photo.CameraMake;
            assetDataObject.ImageMetadata.CameraModel = driveItem.Photo.CameraModel;
            assetDataObject.ImageMetadata.ExposureDenominator = (decimal?)driveItem.Photo.ExposureDenominator;
            assetDataObject.ImageMetadata.ExposureNumerator = (decimal?)driveItem.Photo.ExposureNumerator;
            assetDataObject.ImageMetadata.FNumber = (decimal?)driveItem.Photo.FNumber;
            assetDataObject.ImageMetadata.FocalLength = (decimal?)driveItem.Photo.FocalLength;
            assetDataObject.ImageMetadata.Iso = driveItem.Photo.Iso;
            assetDataObject.ImageMetadata.Orientation = driveItem.Photo.Orientation;
            assetDataObject.ImageMetadata.CaptureDate = driveItem.Photo.TakenDateTime;
        }

        private void SetThumbnails(AssetDataObject assetDataObject, DriveItem driveItem)
        {
            if (driveItem.IsFolder())
            {
                return;
            }

            assetDataObject.InternalMetadata = new DataObjectInternalMetadata();

            if (assetDataObject.ContentType.Id == ContentTypeEnumDataObject.Image.Id)
            {
                var thumbnailSet = driveItem.Thumbnails?.FirstOrDefault();

                assetDataObject.IsThumbnailSmallAvailable = !string.IsNullOrEmpty(thumbnailSet?.Small?.Url);
                assetDataObject.IsThumbnailMediumAvailable = !string.IsNullOrEmpty(thumbnailSet?.Medium?.Url);
                assetDataObject.IsThumbnailLargeAvailable = !string.IsNullOrEmpty(thumbnailSet?.Large?.Url);
                assetDataObject.IsThumbnailPreviewAvailable = assetDataObject.IsThumbnailLargeAvailable;
            }
            else if (assetDataObject.ContentType.Id == ContentTypeEnumDataObject.Video.Id)
            {
                var thumbnailSet = driveItem.Thumbnails?.FirstOrDefault();

                assetDataObject.IsThumbnailSmallAvailable = !string.IsNullOrEmpty(thumbnailSet?.Small?.Url);
                assetDataObject.IsThumbnailMediumAvailable = !string.IsNullOrEmpty(thumbnailSet?.Medium?.Url);
                assetDataObject.IsThumbnailLargeAvailable = !string.IsNullOrEmpty(thumbnailSet?.Large?.Url);
                assetDataObject.IsThumbnailPreviewAvailable = assetDataObject.IsThumbnailLargeAvailable;

                assetDataObject.IsPlaybackLargeAvailable = true;
                assetDataObject.IsPlaybackStreamingAvailable = true;
            }
            else if (assetDataObject.ContentType.Id == ContentTypeEnumDataObject.Audio.Id)
            {
                assetDataObject.IsPlaybackLargeAvailable = true;
            }
            else
            {
                var thumbnailSet = driveItem.Thumbnails?.FirstOrDefault();

                assetDataObject.IsThumbnailSmallAvailable = !string.IsNullOrEmpty(thumbnailSet?.Small?.Url);
                assetDataObject.IsThumbnailMediumAvailable = !string.IsNullOrEmpty(thumbnailSet?.Medium?.Url);
                assetDataObject.IsThumbnailLargeAvailable = !string.IsNullOrEmpty(thumbnailSet?.Large?.Url);
                assetDataObject.IsThumbnailPreviewAvailable = assetDataObject.IsThumbnailLargeAvailable;

                var mimeType = driveItem.File?.MimeType;

                if (!string.IsNullOrEmpty(mimeType))
                {
                    if (string.Equals(mimeType, "text/csv") ||
                        mimeType.StartsWith("application/msword") ||
                        mimeType.StartsWith("application/vnd.ms-word") ||
                        mimeType.StartsWith("application/mspowerpoint") ||
                        mimeType.StartsWith("application/ms-powerpoint") ||
                        mimeType.StartsWith("application/vnd.ms-powerpoint") ||
                        mimeType.StartsWith("application/msexcel") ||
                        mimeType.StartsWith("application/ms-excel") ||
                        mimeType.StartsWith("application/vnd.ms-excel") ||
                        string.Equals(mimeType, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") ||
                        string.Equals(mimeType, "application/vnd.openxmlformats-officedocument.spreadsheetml.template") ||
                        string.Equals(mimeType, "application/vnd.openxmlformats-officedocument.presentationml.slideshow") ||
                        string.Equals(mimeType, "application/vnd.openxmlformats-officedocument.presentationml.presentation") ||
                        string.Equals(mimeType, "application/vnd.openxmlformats-officedocument.wordprocessingml.document") ||
                        string.Equals(mimeType, "application/vnd.openxmlformats-officedocument.wordprocessingml.template") ||
                        string.Equals(mimeType, "application/vnd.oasis.opendocument.presentation") ||
                        string.Equals(mimeType, "application/vnd.oasis.opendocument.spreadsheet") ||
                        string.Equals(mimeType, "application/vnd.oasis.opendocument.text") ||
                        string.Equals(mimeType, "application/rtf"))
                    {
                        // see https://docs.microsoft.com/en-us/graph/api/driveitem-get-content-format?view=graph-rest-1.0&tabs=csharp

                        assetDataObject.IsPdfPreviewAvailable = true;

                        assetDataObject.InternalMetadata.PdfPreviewSpec = ConvertThumbnailSpec;
                    }
                    else if (string.Equals(mimeType, "application/pdf"))
                    {
                        assetDataObject.IsPdfPreviewAvailable = true;
                    }
                }
                else
                {
                    Logger.LogWarning($"Mime type is empty for file {driveItem.Name}");
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

        private static void SetVideoMetadataStreams(AssetDataObject assetDataObject, DriveItem driveItem)
        {
            assetDataObject.VideoMetadata.VideoStreams = new VideoStreamDataObject[]
            {
                new VideoStreamDataObject
                {
                    BitRate = driveItem.Video.Bitrate?.ToString() ?? null,
                    FrameRate = (decimal?)driveItem.Video.FrameRate,
                    DurationInSeconds = driveItem.Video.Duration,
                    Format = driveItem.Video.FourCC
                }
            };

            assetDataObject.VideoMetadata.AudioStreams = new AudioStreamDataObject[]
            {
                new AudioStreamDataObject
                {
                    Format = driveItem.Video.AudioFormat,
                    SamplingRate = driveItem.Video.AudioSamplesPerSecond,
                    Channels = driveItem.Video.AudioChannels?.ToString() ?? null,
                    BitRate = driveItem.Video.AudioBitsPerSample?.ToString() ?? null
                }
            };
        }

        protected override string GetPropertyKey(JsonProperty property)
        {
            // Sharepoint property keys are returned in an inconsistent way
            return property.Name.ConvertToPascalCase();
        }

        protected override IDictionary<string, object>[] GetEnumObjects(string propertyKey, object value, string semanticHint)
        {
            var lookupModel = GetTypedValue<ICollection<LookupModel>>(propertyKey, value, logWarning: false);

            if (lookupModel != null && lookupModel.All(lookup => lookup.LookupId > 0))
            {
                return lookupModel
                    .Select(lookup =>
                    {
                        var enumDataObject = new Dictionary<string, object>
                        {
                            {  EntityModel.PropName_Id, lookup.LookupId.ToString() }
                        };

                        if (!string.IsNullOrEmpty(lookup.LookupValue))
                        {
                            enumDataObject.Add(EntityModel.PropName_ListDisplayName, lookup.LookupValue);
                        }

                        if (!string.IsNullOrEmpty(lookup.Email))
                        {
                            enumDataObject.Add(nameof(LookupModel.Email), lookup.Email);
                        }

                        return enumDataObject;
                    })
                    .ToArray();
            }

            return base.GetEnumObjects(propertyKey, value, semanticHint);
        }

        protected override IDictionary<string, object> GetEnumObject(string propertyKey, object value, string semanticHint)
        {
            var enumValue = GetTypedValue<string>(propertyKey, value, logWarning: false);

            if (string.IsNullOrEmpty(enumValue))
            {
                return GetObject(propertyKey, value, semanticHint);
            }

            var enumObject = new Dictionary<string, object>
            {
                {  EntityModel.PropName_ListDisplayName, enumValue }
            };

            return enumObject;
        }

        protected override CurrencyModel GetCurrencyModelDataType(string propertyKey, object value, string semanticHint)
        {
            var currencyModelValue = GetDecimalDataType(propertyKey, value, semanticHint);

            var currencyModel = new CurrencyModel
            {
                Currency = semanticHint,
                Value = currencyModelValue
            };

            return currencyModel;
        }

        private DataObject[] GetDataObjects(IDictionary<string, object> additionalDataByFieldNames)
        {
            if (additionalDataByFieldNames == null)
            {
                return null;
            }

            var dataObject = GetDataObject(SharepointMetamodelBuilder.RootEntityKey, additionalDataByFieldNames);

            return new[] { dataObject };
        }
    }
}
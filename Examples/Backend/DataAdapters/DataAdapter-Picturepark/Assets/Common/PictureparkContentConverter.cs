using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Picturepark.SDK.V1.Contract;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Converters;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Permissions;
using SmintIo.Portals.SDK.Core.Extensions;
using SmintIo.Portals.SDK.Core.Models.Context;
using SmintIo.Portals.SDK.Core.Models.Metamodel;
using SmintIo.Portals.SDK.Core.Models.Metamodel.Data;
using SmintIo.Portals.SDK.Core.Models.Metamodel.Data.ContentTypes;
using SmintIo.Portals.SDK.Core.Models.Metamodel.Model;
using SmintIo.Portals.SDK.Core.Models.Strings;
using static SmintIo.Portals.DataAdapter.Picturepark.Assets.PictureparkAssetsDataAdapterConfiguration;

namespace SmintIo.Portals.DataAdapter.Picturepark.Assets.Common
{
    public class PictureparkContentConverter : JsonObjectConverter
    {
        private static readonly ICollection<DataType> _translationPropertyDataTypes = new HashSet<DataType>
        {
            DataType.Enum,
            DataType.DataObject,
            DataType.LocalizedStringsModel,
            DataType.LocalizedStringsArrayModel
        };

        private readonly IDataAdapterContextModel _dataAdapterContext;
        private readonly IEntityModelProvider _entityModelProvider;

        private readonly string _listNameAttribute;
        private readonly string _listNameAttribute2;
        private readonly string[] _resolveListDataAttributes;

        public PictureparkContentConverter(
            ILogger logger,
            IDataAdapterContextModel dataAdapterContext,
            IEntityModelProvider entityModelProvider,
            string listNameAttribute,
            string listNameAttribute2,
            string[] resolveListDataAttributes)
            : this(logger, dataAdapterContext, entityModelProvider, listNameAttribute, listNameAttribute2, resolveListDataAttributes, postProcessObjectConverter: null)
        {
        }

        public PictureparkContentConverter(
            ILogger logger,
            IDataAdapterContextModel dataAdapterContext,
            IEntityModelProvider entityModelProvider,
            string listNameAttribute,
            string listNameAttribute2,
            string[] resolveListDataAttributes,
            IPostProcessObjectConverter postProcessObjectConverter)
            : base(logger, entityModelProvider, postProcessObjectConverter)
        {
            _dataAdapterContext = dataAdapterContext;
            _entityModelProvider = entityModelProvider;

            _listNameAttribute = listNameAttribute;
            _listNameAttribute2 = listNameAttribute2;
            _resolveListDataAttributes = resolveListDataAttributes;
        }

        protected override ICollection<DataType> SupportedTranslationPropertyDataTypes
        {
            get
            {
                return _translationPropertyDataTypes;
            }
        }

        public static AssetDataObject Convert(IDataAdapterContextModel dataAdapterContext, ContentDetail contentDetail)
        {
            var asset = new AssetDataObject(dataAdapterContext)
            {
                Id = contentDetail.Id
            };

            // Display values have to be loaded

            var dictionary = new Dictionary<string, string>()
            {
                { LocalizedStringsModel.DefaultCulture, ProcessLiquid(contentDetail.DisplayValues[nameof(DisplayPatternType.Name).ToLowerCamelCase()]) }
            };

            dictionary = FixDictionary(dictionary);

            if (dictionary != null)
            {
                var name = new LocalizedStringsModel(dictionary);

                asset.ListDisplayName = name;

                asset.Name = name;
            }

            asset.ContentType = Convert(contentDetail.ContentType);

            ProcessOutputs(asset, contentDetail);
            ProcessDisplayContentOutputs(asset, contentDetail);

            return asset;
        }

        public AssetDataObject Convert(ContentDetail contentDetail, PictureparkDisplayPattern? titleDisplayPattern)
        {
            var asset = new AssetDataObject(_dataAdapterContext)
            {
                Id = contentDetail.Id
            };

            if (contentDetail.Audit != null)
            {
                if (contentDetail.Audit.CreationDate == DateTime.MinValue)
                {
                    asset.CreatedAt = null;
                }
                else if (contentDetail.Audit.CreationDate == DateTime.MaxValue)
                {
                    asset.CreatedAt = DateTimeOffset.MaxValue;
                }
                else
                {
                    asset.CreatedAt = new DateTimeOffset(contentDetail.Audit.CreationDate);
                }

                if (contentDetail.Audit.ModificationDate == DateTime.MinValue)
                {
                    asset.ModifiedAt = null;
                }
                else if (contentDetail.Audit.ModificationDate == DateTime.MaxValue)
                {
                    asset.ModifiedAt = DateTimeOffset.MaxValue;
                }
                else
                {
                    asset.ModifiedAt = new DateTimeOffset(contentDetail.Audit.CreationDate);
                }

                if (contentDetail.Audit.CreatedByUser != null &&
                    !string.IsNullOrEmpty(contentDetail.Audit.CreatedByUser.FirstName) &&
                    !string.IsNullOrEmpty(contentDetail.Audit.CreatedByUser.LastName))
                {
                    asset.CreatedBy = $"{contentDetail.Audit.CreatedByUser.FirstName} {contentDetail.Audit.CreatedByUser.LastName}";
                }

                if (contentDetail.Audit.ModifiedByUser != null &&
                    !string.IsNullOrEmpty(contentDetail.Audit.ModifiedByUser.FirstName) &&
                    !string.IsNullOrEmpty(contentDetail.Audit.ModifiedByUser.LastName))
                {
                    asset.ModifiedBy = $"{contentDetail.Audit.ModifiedByUser.FirstName} {contentDetail.Audit.ModifiedByUser.LastName}";
                }
            }

            // Display values have to be loaded

            string titleDisplayPatternType;
            bool fallback = true;

            if (titleDisplayPattern != null)
            {
                switch (titleDisplayPattern)
                {
                    case PictureparkDisplayPattern.List:
                        titleDisplayPatternType = nameof(DisplayPatternType.List).ToLowerCamelCase();
                        break;
                    case PictureparkDisplayPattern.Name:
                    default:
                        titleDisplayPatternType = nameof(DisplayPatternType.Name).ToLowerCamelCase();

                        fallback = false;

                        break;
                }
            }
            else
            {
                titleDisplayPatternType = nameof(DisplayPatternType.Name).ToLowerCamelCase();

                fallback = false;
            }

            var fallbackRequired = false;

            if (contentDetail.DisplayValues.ContainsKey(titleDisplayPatternType))
            {
                var dictionary = new Dictionary<string, string>()
                {
                    { LocalizedStringsModel.DefaultCulture, ProcessLiquid(contentDetail.DisplayValues[titleDisplayPatternType]) }
                };

                dictionary = FixDictionary(dictionary);

                if (dictionary != null)
                {
                    var name = new LocalizedStringsModel(dictionary);

                    asset.ListDisplayName = name;

                    asset.Name = name;
                }
                else
                {
                    fallbackRequired = true;
                }
            }

            if (fallbackRequired && fallback)
            {
                // fallback, if we did not try already with "name"

                var dictionary = new Dictionary<string, string>()
                {
                    { LocalizedStringsModel.DefaultCulture, ProcessLiquid(contentDetail.DisplayValues[nameof(DisplayPatternType.Name).ToLowerCamelCase()]) }
                };

                dictionary = FixDictionary(dictionary);

                if (dictionary != null)
                {
                    var name = new LocalizedStringsModel(dictionary);

                    asset.ListDisplayName = name;

                    asset.Name = name;
                }
            }

            asset.ContentType = Convert(contentDetail.ContentType);

            ProcessOutputs(asset, contentDetail);
            ProcessDisplayContentOutputs(asset, contentDetail);

            if (contentDetail.ContentType != ContentType.Virtual)
            {
                FileMetadata fileMetadata = contentDetail.GetFileMetadata();

                if (!(fileMetadata is null))
                {
                    if (fileMetadata is ImageMetadata imageMetadata)
                    {
                        var (imageMetadataDataObject, fileMetadataDataObject) = Convert(imageMetadata);

                        asset.ImageMetadata = imageMetadataDataObject;
                        asset.FileMetadata = fileMetadataDataObject;
                    }
                    else if (fileMetadata is VideoMetadata videoMetadata)
                    {
                        var (videoMetadataDataObject, fileMetadataDataObject) = Convert(videoMetadata);

                        asset.VideoMetadata = videoMetadataDataObject;
                        asset.FileMetadata = fileMetadataDataObject;
                    }
                    else if (fileMetadata is AudioMetadata audioMetadata)
                    {
                        if (asset.ThumbnailAspectRatio == null)
                        {
                            asset.ThumbnailAspectRatio = 1;
                        }

                        var (audioMetadataDataObject, fileMetadataDataObject) = Convert(audioMetadata);

                        asset.AudioMetadata = audioMetadataDataObject;
                        asset.FileMetadata = fileMetadataDataObject;
                    }
                    else if (fileMetadata is DocumentMetadata documentMetadata)
                    {
                        var (documentMetadataDataObject, fileMetadataDataObject) = Convert(documentMetadata);

                        asset.DocumentMetadata = documentMetadataDataObject;
                        asset.FileMetadata = fileMetadataDataObject;
                    }
                    else if (fileMetadata is VectorMetadata vectorMetadata)
                    {
                        var (vectorMetadataDataObject, fileMetadataDataObject) = Convert(vectorMetadata);

                        asset.VectorMetadata = vectorMetadataDataObject;
                        asset.FileMetadata = fileMetadataDataObject;
                    }
                }
            }

            if (_entityModelProvider != null)
            {
                asset.RawData = Convert(contentDetail);
            }

            var permissionUuids = new HashSet<string>();

            if (contentDetail.ContentRights != null && contentDetail.ContentRights.Count > 0)
            {
                foreach (var contentRight in contentDetail.ContentRights)
                {
                    switch (contentRight)
                    {
                        case ContentRight.View:
                            permissionUuids.Add(DataAdapterPermission.ReadAssetDetailsPermissionUuid);
                            permissionUuids.Add(DataAdapterPermission.DownloadAssetLayoutFilesPermissionUuid);
                            break;
                        case ContentRight.AccessOriginal:
                            permissionUuids.Add(DataAdapterPermission.DownloadAssetsHiResPermissionUuid);
                            break;
                        default:
                            // not relevant to us
                            break;
                    }
                }
            }

            asset.PermissionUuids = permissionUuids.ToArray();

            return asset;
        }

        private static void ProcessOutputs(AssetDataObject asset, ContentDetail contentDetail)
        {
            var outputs = contentDetail.Outputs;

            if (outputs == null || !outputs.Any())
            {
                asset.Version = "0";

                return;
            }

            var hasDisplayContentId = !string.IsNullOrEmpty(contentDetail.DisplayContentId);
                
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

                var outputDetail = output.Detail;

                if (outputDetail == null)
                {
                    // no output detail

                    continue;
                }

                var outputFormatId = output.OutputFormatId;

                switch (outputFormatId)
                {
                    case "ThumbnailExtraLarge":
                    case "ThumbnailPortal":
                    case "Preview":
                        if (!hasDisplayContentId)
                        {
                            asset.IsThumbnailPreviewAvailable = true;
                        }

                        break;

                    case "ThumbnailLarge":
                        if (!hasDisplayContentId)
                        {
                            asset.IsThumbnailLargeAvailable = true;
                        }

                        break;

                    case "ThumbnailMedium":
                        if (!hasDisplayContentId)
                        {
                            asset.IsThumbnailMediumAvailable = true;
                        }

                        break;

                    case "ThumbnailSmall":
                        if (!hasDisplayContentId)
                        {
                            asset.IsThumbnailSmallAvailable = true;
                        }

                        break;

                    case "VideoLarge":
                        if (!hasDisplayContentId)
                        {
                            asset.IsPlaybackLargeAvailable = true;
                        }

                        break;

                    case "VideoSmall":
                        if (!hasDisplayContentId)
                        {
                            asset.IsPlaybackSmallAvailable = true;
                        }

                        break;

                    case "AudioSmall":
                        if (!hasDisplayContentId)
                        {
                            asset.IsPlaybackSmallAvailable = true;
                        }

                        break;

                    case "Pdf":
                        if (!hasDisplayContentId)
                        {
                            asset.IsPdfPreviewAvailable = true;
                        }

                        break;

                    case "Svg":
                        if (!hasDisplayContentId)
                        {
                            asset.IsThumbnailPreviewAvailable = true;
                            asset.IsThumbnailLargeAvailable = true;
                        }

                        break;

                    case "Original":
                        asset.Version = $"{output.FileVersion}";
                        break;

                    case "VideoKeyframes":
                    default:
                        continue;
                }

                if (!hasDisplayContentId &&
                    asset.ThumbnailAspectRatio == null)
                {
                    int? width = null;
                    int? height = null;

                    if (outputDetail is OutputDataImage outputDataImage)
                    {
                        width = outputDataImage.Width;
                        height = outputDataImage.Height;
                    }
                    else if (outputDetail is OutputDataVideo outputDataVideo)
                    {
                        width = outputDataVideo.Width;
                        height = outputDataVideo.Height;
                    }

                    if (width != null && height != null && height > 0)
                    {
                        asset.ThumbnailAspectRatio = (decimal)width / (decimal)height;
                    }
                }
            }

            if (string.IsNullOrEmpty(asset.Version))
            {
                asset.Version = "0";
            }
        }

        private static void ProcessDisplayContentOutputs(AssetDataObject asset, ContentDetail contentDetail)
        {
            var displayContentId = contentDetail.DisplayContentId;

            if (string.IsNullOrEmpty(displayContentId))
            {
                return;
            }

            var displayContentOutputs = contentDetail.DisplayContentOutputs;

            if (displayContentOutputs == null || !displayContentOutputs.Any())
            {
                return;
            }

            if (asset.InternalMetadata == null)
            {
                asset.InternalMetadata = new DataObjectInternalMetadata();
            }

            foreach (var displayContentOutput in displayContentOutputs)
            {
                if (displayContentOutput.RenderingState == OutputRenderingState.Failed ||
                    displayContentOutput.RenderingState == OutputRenderingState.NoLicense)
                {
                    // ignore likely unrecoverable ones...

                    continue;
                }

                if (displayContentOutput.RenderingState == OutputRenderingState.Skipped &&
                    displayContentOutput.DynamicRendering != true)
                {
                    // This is no dynamic render, but skipped
                    // so we skip as well

                    continue;
                }

                var displayContentOutputDetail = displayContentOutput.Detail;

                if (displayContentOutputDetail == null)
                {
                    // no output detail

                    continue;
                }

                var displayContentOutputFormatId = displayContentOutput.OutputFormatId;

                switch (displayContentOutputFormatId)
                {
                    case "ThumbnailExtraLarge":
                    case "ThumbnailPortal":
                    case "Preview":
                        asset.IsThumbnailPreviewAvailable = true;

                        asset.InternalMetadata.PreviewThumbnailSpec = displayContentId;

                        break;

                    case "ThumbnailLarge":
                        asset.IsThumbnailLargeAvailable = true;

                        asset.InternalMetadata.LargeThumbnailSpec = displayContentId;

                        break;

                    case "ThumbnailMedium":
                        asset.IsThumbnailMediumAvailable = true;

                        asset.InternalMetadata.MediumThumbnailSpec = displayContentId;

                        break;

                    case "ThumbnailSmall":
                        asset.IsThumbnailSmallAvailable = true;

                        asset.InternalMetadata.SmallThumbnailSpec = displayContentId;

                        break;

                    case "VideoLarge":
                        asset.IsPlaybackLargeAvailable = true;

                        asset.InternalMetadata.PlaybackLargeSpec = displayContentId;

                        break;

                    case "VideoSmall":
                        asset.IsPlaybackSmallAvailable = true;

                        asset.InternalMetadata.PlaybackSmallSpec = displayContentId;

                        break;

                    case "AudioSmall":
                        asset.IsPlaybackSmallAvailable = true;

                        asset.InternalMetadata.PlaybackSmallSpec = displayContentId;

                        break;

                    case "Pdf":
                        asset.IsPdfPreviewAvailable = true;

                        asset.InternalMetadata.PdfPreviewSpec = displayContentId;

                        break;

                    case "Svg":
                        asset.IsThumbnailPreviewAvailable = true;
                        asset.IsThumbnailLargeAvailable = true;

                        asset.InternalMetadata.PreviewThumbnailSpec = displayContentId;
                        asset.InternalMetadata.LargeThumbnailSpec = displayContentId;

                        break;

                    case "Original":
                        // not here

                        break;

                    case "VideoKeyframes":
                    default:
                        continue;
                }

                if (asset.ThumbnailAspectRatio == null)
                {
                    int? width = null;
                    int? height = null;

                    if (displayContentOutputDetail is OutputDataImage outputDataImage)
                    {
                        width = outputDataImage.Width;
                        height = outputDataImage.Height;
                    }
                    else if (displayContentOutputDetail is OutputDataVideo outputDataVideo)
                    {
                        width = outputDataVideo.Width;
                        height = outputDataVideo.Height;
                    }

                    if (width != null && height != null && height > 0)
                    {
                        asset.ThumbnailAspectRatio = (decimal)width / (decimal)height;
                    }
                }
            }
        }

        public static ContentTypeEnumDataObject Convert(ContentType? contentType)
        {
            if (contentType == null)
                return ContentTypeEnumDataObject.Unknown;

            switch (contentType)
            {
                case ContentType.Bitmap:
                case ContentType.VectorGraphic:
                case ContentType.RawImage:
                    return ContentTypeEnumDataObject.Image;

                case ContentType.Video:
                    return ContentTypeEnumDataObject.Video;

                case ContentType.Audio:
                    return ContentTypeEnumDataObject.Audio;

                case ContentType.InterchangeDocument:
                case ContentType.WordProcessingDocument:
                case ContentType.TextDocument:
                case ContentType.DesktopPublishingDocument:
                case ContentType.Presentation:
                case ContentType.Spreadsheet:
                    return ContentTypeEnumDataObject.Document;

                /* case ContentType.Archive:
                case ContentType.Font:
                case ContentType.Multimedia:
                case ContentType.Application:
                case ContentType.SourceCode:
                case ContentType.Database:
                case ContentType.Cad:
                case ContentType.Model3d:
                case ContentType.Virtual: */

                default:
                    return ContentTypeEnumDataObject.Other;
            }
        }

        public static (ImageMetadataDataObject, FileMetadataDataObject) Convert(ImageMetadata source)
        {
            return Copy(source, new ImageMetadataDataObject(), new FileMetadataDataObject());
        }

        private static (VectorMetadataDataObject, FileMetadataDataObject) Convert(VectorMetadata source)
        {
            return Copy(source, new VectorMetadataDataObject(), new FileMetadataDataObject());
        }

        public static (VideoMetadataDataObject, FileMetadataDataObject) Convert(VideoMetadata source)
        {
            return Copy(source, new VideoMetadataDataObject(), new FileMetadataDataObject());
        }

        public static (AudioMetadataDataObject, FileMetadataDataObject) Convert(AudioMetadata source)
        {
            return Copy(source, new AudioMetadataDataObject(), new FileMetadataDataObject());
        }

        private static (DocumentMetadataDataObject, FileMetadataDataObject) Convert(DocumentMetadata source)
        {
            return Copy(source, new DocumentMetadataDataObject(), new FileMetadataDataObject());
        }

        private static EpsMetadataDataObject Convert(EpsMetadata source)
        {
            return Copy(source, new EpsMetadataDataObject());
        }

        private static (ImageMetadataDataObject, FileMetadataDataObject) Copy(ImageMetadata source, ImageMetadataDataObject target, FileMetadataDataObject fileMetadataTarget)
        {
            Copy(source, fileMetadataTarget);

            target.Channels = source.Channels;
            target.HasAdobeResourceData = source.HasAdobeResourceData;
            target.HasIptcData = source.HasIptcData;
            target.HasExifData = source.HasExifData;
            target.TotalUnspecifiedTiffExtraChannels = source.TotalUnspecifiedTiffExtraChannels;
            target.TotalFrames = source.TotalFrames;
            target.VerticalResolution = (decimal)source.VerticalResolution;
            target.HorizontalResolution = (decimal)source.HorizontalResolution;
            target.IsExtended = source.IsExtended;
            target.IsIndexed = source.IsIndexed;
            target.HasAlpha = source.HasAlpha;
            target.PixelFormat = source.PixelFormat;
            target.UncompressedSizeInBytes = source.UncompressedSizeInBytes;
            target.BitsPerChannel = source.BitsPerChannel;
            target.BitsPerPixel = source.BitsPerPixel;
            target.ColorProfile = source.ColorProfile;
            target.ColorSpace = source.ColorSpace;
            target.HeightInCm = (decimal)source.HeightInCm;
            target.WidthInCm = (decimal)source.WidthInCm;
            target.HeightInInch = (decimal)source.HeightInInch;
            target.WidthInInch = (decimal)source.WidthInInch;
            target.Height = source.Height;
            target.Width = source.Width;
            target.HasXmpData = source.HasXmpData;

            return (target, fileMetadataTarget);
        }

        private static (VideoMetadataDataObject, FileMetadataDataObject) Copy(VideoMetadata source, VideoMetadataDataObject target, FileMetadataDataObject fileMetadataTarget)
        {
            Copy(source, fileMetadataTarget);

            target.Width = source.Width;
            target.Height = source.Height;
            target.DurationInSeconds = (decimal)source.DurationInSeconds;
            target.Format = source.Format;
            target.Codec = source.Codec;
            target.OverallBitrate = source.OverallBitrate;

            target.AudioStreams = Convert(source.AudioStreams);
            target.VideoStreams = Convert(source.VideoStreams);

            return (target, fileMetadataTarget);
        }

        private static (AudioMetadataDataObject, FileMetadataDataObject) Copy(AudioMetadata source, AudioMetadataDataObject target, FileMetadataDataObject fileMetadataTarget)
        {
            Copy(source, fileMetadataTarget);

            var audioStreams = source.AudioStreams;

            if (audioStreams != null)
            {
                target.AudioStreams = Convert(audioStreams).ToArray();
            }

            return (target, fileMetadataTarget);
        }

        private static (DocumentMetadataDataObject, FileMetadataDataObject) Copy(DocumentMetadata source, DocumentMetadataDataObject target, FileMetadataDataObject fileMetadataTarget)
        {
            Copy(source, fileMetadataTarget);

            fileMetadataTarget.Title = source.DocumentTitle;
            fileMetadataTarget.Company = source.Company;
            fileMetadataTarget.Publisher = source.Publisher;
            fileMetadataTarget.Creator = source.Creator;
            fileMetadataTarget.Author = source.Author;

            target.Titles = source.Titles?.ToArray();
            target.RevisionNumber = source.RevisionNumber;
            target.ParagraphCount = source.ParagraphCount;
            target.SlideCount = source.SlideCount;
            target.PageCount = source.PageCount;
            target.LineCount = source.LineCount;
            target.CharacterCountWithSpaces = source.CharacterCountWithSpaces;
            target.CharacterCount = source.CharacterCount;
            target.ApplicationVersion = source.ApplicationVersion;
            target.ApplicationName = source.ApplicationName;
            target.ImageTitles = source.ImageTitles?.ToArray();

            target.EpsMetadata = Convert(source.EpsInfo);

            return (target, fileMetadataTarget);
        }

        private static (VectorMetadataDataObject, FileMetadataDataObject) Copy(VectorMetadata source, VectorMetadataDataObject target, FileMetadataDataObject fileMetadataTarget)
        {
            Copy(source, fileMetadataTarget);

            fileMetadataTarget.Author = source.Author;
            fileMetadataTarget.Creator = source.Creator;
            fileMetadataTarget.Publisher = source.Publisher;
            fileMetadataTarget.Company = source.Company;
            fileMetadataTarget.Title = source.Title;
            target.PageCount = source.PageCount;

            target.EpsMetadata = Convert(source.EpsInfo);

            return (target, fileMetadataTarget);
        }

        private static EpsMetadataDataObject Copy(EpsMetadata source, EpsMetadataDataObject target)
        {
            if (source is null)
            {
                return null;
            }

            target.IsRasterized = source.IsRasterized;

            target.WidthInPoints = (decimal?)source.WidthInPoints;
            target.HeightInPoints = (decimal?)source.HeightInPoints;

            return target;
        }

        private static AudioStreamDataObject[] Convert(ICollection<AudioStream> source)
        {
            if (source is null)
            {
                return null;
            }

            return source.Select(s => Convert(s)).ToArray();
        }

        private static VideoStreamDataObject[] Convert(ICollection<VideoStream> source)
        {
            if (source is null)
            {
                return null;
            }
            return source.Select(s => Convert(s)).ToArray();
        }

        public static VideoStreamDataObject Convert(VideoStream source)
        {
            VideoStreamDataObject target = new VideoStreamDataObject();

            target.FrameCount = source.FrameCount;
            target.StreamSize = source.StreamSize;
            target.Resolution = source.Resolution;
            target.PixelAspectRatio = source.PixelAspectRatio.HasValue ? (decimal?)source.PixelAspectRatio.Value : null;
            target.Language = source.Language;
            target.Height = source.Height;
            target.FrameRate = source.FrameRate.HasValue ? (decimal?)source.FrameRate.Value : null;
            target.Rotation = source.Rotation.HasValue ? (decimal?)source.Rotation.Value : null;
            target.Format = source.Format;
            target.DurationInSeconds = (decimal)source.DurationInSeconds;
            target.DisplayAspectRatio = source.DisplayAspectRatio;
            target.Codec = source.Codec;
            target.BitRate = source.BitRate;
            target.Width = source.Width;

            return target;
        }

        public static AudioStreamDataObject Convert(AudioStream source)
        {
            AudioStreamDataObject target = new AudioStreamDataObject();

            target.BitRate = source.BitRate;
            target.BitRateMode = source.BitRateMode;
            target.Channels = source.Channels;
            target.ChannelPositions = source.ChannelPositions;
            target.Codec = source.Codec;
            target.DurationInSeconds = source.DurationInSeconds.HasValue ? (decimal?)source.DurationInSeconds.Value : null;
            target.Format = source.Format;
            target.Language = source.Language;
            target.Resolution = source.Resolution;
            target.SamplingRate = source.SamplingRate;
            target.StreamSize = source.StreamSize;

            return target;
        }

        private static T Copy<T>(FileMetadata source, T target) where T : FileMetadataDataObject
        {
            if (!(source.Names is null))
            {
                var dictionary = FixDictionary(source.Names);

                if (dictionary != null)
                {
                    var name = new LocalizedStringsModel(dictionary);

                    target.ListDisplayName = name;
                    target.DetailDisplayName = name;

                    target.Name = name;
                }
            }

            if (!(source.Descriptions is null))
            {
                var dictionary = FixDictionary(source.Descriptions);

                if (dictionary != null)
                {
                    target.Description = new LocalizedStringsModel(dictionary);
                }
            }

            var fileExtension = source.FileExtension;

            if (!string.IsNullOrEmpty(fileExtension))
            {
                if (fileExtension.StartsWith("."))
                {
                    // Picturepark usually delivers with ".", we expect without "."

                    fileExtension = fileExtension[1..];
                }
            }

            target.FileExtension = fileExtension;
            target.FileName = source.FileName;
            target.FilePath = source.FilePath;
            target.FileSizeInBytes = source.FileSizeInBytes;
            target.Sha1Hash = source.Sha1Hash;
            target.Language = source.Language;

            return target;
        }

        private DataObject[] Convert(ContentDetail contentDetail)
        {
            var layerDataObjects = new List<DataObject>();

            if (contentDetail.Content != null)
            {
                var contentSchema = contentDetail.Content;
                var contentSchemaId = contentDetail.ContentSchemaId;

                if (contentSchema != null && contentSchema is JObject jContentSchema && !string.IsNullOrEmpty(contentSchemaId))
                {
                    if (string.IsNullOrEmpty(contentSchemaId))
                        throw new Exception($"Content schema ID is empty for content schema {contentSchema}");

                    var rawDataObject = GetDataObject(contentSchemaId, jContentSchema);

                    layerDataObjects.Add(rawDataObject);
                }
            }

            if (contentDetail.LayerSchemaIds != null && contentDetail.LayerSchemaIds.Count > 0)
            {
                foreach (var layerSchemaId in contentDetail.LayerSchemaIds)
                {
                    var layerSchema = contentDetail.Layer(layerSchemaId);

                    if (layerSchema == null || !(layerSchema is JObject jLayerSchema))
                        continue;

                    if (string.IsNullOrEmpty(layerSchemaId))
                        throw new Exception($"Layer schema ID is empty for layer schema {layerSchema}");

                    var rawDataObject = GetDataObject(layerSchemaId, jLayerSchema);

                    layerDataObjects.Add(rawDataObject);
                }
            }

            return layerDataObjects.ToArray();
        }

        protected override DateTimeOffset? GetDateTimeDataType(JToken value, string semanticHint)
        {
            if (value.Type == JTokenType.Null)
            {
                return null;
            }

            if (value is JProperty jProperty)
            {
                value = jProperty.Value;
            }

            var v = value as JValue;
            var objectValue = v.Value;

            if (objectValue == null)
            {
                return null;
            }

            if (objectValue is DateTimeOffset dateTimeOffset)
            {
                return dateTimeOffset;
            }
            else if (objectValue is DateTime dateTime)
            {
                if (dateTime == DateTime.MinValue)
                {
                    return DateTimeOffset.MinValue;
                }
                else if (dateTime == DateTime.MaxValue)
                {
                    return DateTimeOffset.MaxValue;
                }
                else
                {
                    // cannot do this conversion at min and max value

                    return new DateTimeOffset(dateTime);
                }
            }
            else if (objectValue is string stringValue && DateTimeOffset.TryParse(stringValue, out dateTimeOffset))
            {
                return dateTimeOffset;
            }

            return base.GetDateTimeDataType(value, semanticHint);
        }

        protected override decimal? GetDecimalDataType(JToken value, string semanticHint)
        {
            var decimalValue = GetTypedValue<decimal?>(value, logWarning: false);

            if (decimalValue.HasValue)
            {
                return decimalValue.Value;
            }

            var longValue = GetTypedValue<long?>(value, logWarning: false);

            if (longValue.HasValue)
            {
                return longValue.Value;
            }

            var intValue = GetTypedValue<int?>(value, logWarning: false);

            if (intValue.HasValue)
            {
                return intValue.Value;
            }

            return base.GetDecimalDataType(value, semanticHint);
        }

        protected override EnumDataObject GetEnumDataObject(string metamodelEntityKey, string targetMetamodelEntityKey, JObject _object)
        {
            var enumDataObject = base.GetEnumDataObject(metamodelEntityKey, targetMetamodelEntityKey, _object);

            SetEnumData(enumDataObject, _object);

            return enumDataObject;
        }

        protected override EnumDataObject[] GetEnumDataObjects(string metamodelEntityKey, string targetMetamodelEntityKey, JObject[] _objects)
        {
            if (_objects.Count() <= 1)
            {
                // single ref is OK

                return base.GetEnumDataObjects(metamodelEntityKey, targetMetamodelEntityKey, _objects);
            }

            bool isExpensive = true;

            // we either whitelist the parent object itself

            var entityModelKeySplit = metamodelEntityKey.Split("___", StringSplitOptions.RemoveEmptyEntries);
            var entityModelKey = entityModelKeySplit.Length > 1
                ? string.Join("___", entityModelKeySplit[1..])
                : entityModelKeySplit[0];

            if (_resolveListDataAttributes != null && _resolveListDataAttributes.Any(resolveListDataAttribute => string.Equals(resolveListDataAttribute, entityModelKey, StringComparison.OrdinalIgnoreCase)))
            {
                isExpensive = false;
            }
            else
            {
                // or we whitelist the enum type

                var targetEntityModelKeySplit = targetMetamodelEntityKey.Split("___", StringSplitOptions.RemoveEmptyEntries);

                var targetEntityModelKey = targetEntityModelKeySplit.Length > 1
                    ? string.Join("___", targetEntityModelKeySplit[1..])
                    : targetEntityModelKeySplit[0];

                if (_resolveListDataAttributes != null && _resolveListDataAttributes.Any(resolveListDataAttribute => string.Equals(resolveListDataAttribute, targetEntityModelKey, StringComparison.OrdinalIgnoreCase)))
                {
                    isExpensive = false;
                }
            }

            if (!isExpensive)
            {
                return base.GetEnumDataObjects(metamodelEntityKey, targetMetamodelEntityKey, _objects);
            }

            var enumDataObjects = _objects
                .Select(o =>
                {
                    var enumDataObject = new EnumDataObject(targetMetamodelEntityKey);

                    SetEnumData(enumDataObject, o);

                    return enumDataObject;
                })
                .ToArray();

            return enumDataObjects;
        }

        private void SetEnumData(EnumDataObject enumDataObject, JObject jObjectValue)
        {
            if (jObjectValue == null)
            {
                return;
            }

            var contentSchemaProperties = jObjectValue.Properties().ToList();

            if (contentSchemaProperties == null || contentSchemaProperties.Count <= 0)
            {
                return;
            }

            var idContentSchemaProperty = contentSchemaProperties.Where(contentSchemaProperty => string.Equals(contentSchemaProperty.Name, "_refId")).FirstOrDefault();

            if (idContentSchemaProperty == null)
            {
                throw new Exception($"Ref ID enum content schema property not found: {idContentSchemaProperty}");
            }

            var idContentSchemaValue = idContentSchemaProperty.Value;

            if (idContentSchemaValue == null || idContentSchemaValue.Type != JTokenType.String)
            {
                throw new Exception($"Ref ID enum content schema value not found or not of type string: {idContentSchemaProperty}");
            }

            var refId = idContentSchemaValue.Value<string>();

            if (string.IsNullOrEmpty(refId))
            {
                throw new Exception($"Ref ID enum content schema value is empty: {idContentSchemaProperty}");
            }

            LocalizedStringsModel name = null;

            if (!string.IsNullOrEmpty(_listNameAttribute))
            {
                // try priority 1

                name = GetContentSchemaNameProperty(contentSchemaProperties, _listNameAttribute);
            }

            if (name == null && !string.IsNullOrEmpty(_listNameAttribute2))
            {
                // try priority 2

                name = GetContentSchemaNameProperty(contentSchemaProperties, _listNameAttribute2);
            }

            if (name == null)
            {
                // try "name"

                name = GetContentSchemaNameProperty(contentSchemaProperties, "name");
            }

            if (name == null)
            {
                // try "label"

                name = GetContentSchemaNameProperty(contentSchemaProperties, "label");
            }

            if (name == null)
            {
                // try to get name from matching property types

                name = GetContentSchemaNamePropertyTranslatableString(contentSchemaProperties);
            }

            if (name == null)
            {
                // try to get name from matching property types

                name = GetContentSchemaNamePropertyString(contentSchemaProperties);
            }

            if (name == null)
            {
                name = refId.Localize();
            }

            enumDataObject.Id = refId;
            enumDataObject.ListDisplayName = name;
        }

        private static LocalizedStringsModel GetContentSchemaNameProperty(List<JProperty> contentSchemaProperties, string propertyName)
        {
            var nameContentSchemaProperty = contentSchemaProperties.Where(contentSchemaProperty => string.Equals(contentSchemaProperty.Name, propertyName)).FirstOrDefault();

            if (nameContentSchemaProperty != null)
            {
                var nameContentSchemaValue = nameContentSchemaProperty.Value;

                if (nameContentSchemaValue != null)
                {
                    if (nameContentSchemaValue.Type == JTokenType.Object)
                    {
                        var jObjectValue = nameContentSchemaValue.Value<JObject>();

                        if (jObjectValue != null)
                        {
                            var dictionary = jObjectValue.ToObject<Dictionary<string, string>>();

                            dictionary = FixDictionary(dictionary);

                            if (dictionary != null)
                            {
                                return new LocalizedStringsModel(dictionary);
                            }
                        }
                    }
                    else if (nameContentSchemaValue.Type == JTokenType.String)
                    {
                        var stringValue = nameContentSchemaValue.Value<string>();

                        if (!string.IsNullOrEmpty(stringValue))
                        {
                            return new LocalizedStringsModel() {
                                { LocalizedStringsModel.DefaultCulture, stringValue }
                            };
                        }
                    }
                }
            }

            return null;
        }

        private static LocalizedStringsModel GetContentSchemaNamePropertyTranslatableString(List<JProperty> contentSchemaProperties)
        {
            foreach (var contentSchemaProperty in contentSchemaProperties)
            {
                var contentSchemaPropertyValue = contentSchemaProperty.Value;

                if (contentSchemaPropertyValue.Type == JTokenType.Object)
                {
                    // seems to be something we can use

                    try
                    {
                        var jObjectValue = contentSchemaPropertyValue.Value<JObject>();

                        if (jObjectValue != null)
                        {
                            var dictionary = jObjectValue.ToObject<Dictionary<string, string>>();

                            dictionary = FixDictionary(dictionary);

                            if (dictionary != null && dictionary.ContainsKey("x-default"))
                            {
                                // use it

                                return new LocalizedStringsModel(dictionary);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // something is wrong, ignore...
                    }
                }
            }

            return null;
        }

        private static LocalizedStringsModel GetContentSchemaNamePropertyString(List<JProperty> contentSchemaProperties)
        {
            foreach (var contentSchemaProperty in contentSchemaProperties)
            {
                var contentSchemaPropertyValue = contentSchemaProperty.Value;

                if (contentSchemaPropertyValue.Type == JTokenType.String)
                {
                    // seems to be something we can use

                    try
                    {
                        var stringValue = contentSchemaPropertyValue.Value<string>();

                        if (!string.IsNullOrEmpty(stringValue))
                        {
                            // use it

                            return new LocalizedStringsModel(new Dictionary<string, string>() {
                                        { LocalizedStringsModel.DefaultCulture, stringValue }
                                    });
                        }
                    }
                    catch (Exception)
                    {
                        // something is wrong, ignore...
                    }
                }
            }

            return null;
        }

        private static Dictionary<string, string> FixDictionary(Dictionary<string, string> dictionary)
        {
            if (dictionary is null || dictionary.Count == 0)
                return null;

            dictionary = dictionary
                .Where(entry => !string.IsNullOrEmpty(entry.Value))
                .ToDictionary(entry => entry.Key, entry => entry.Value);

            if (dictionary.Count == 0)
                return null;

            return dictionary;
        }

        private static string ProcessLiquid(string liquidTemplate)
        {
            return liquidTemplate;

            /* TODO: HTML safety, ...

            if (string.IsNullOrEmpty(liquidTemplate))
                return liquidTemplate;

            try
            {
                var template = DotLiquid.Template.Parse(liquidTemplate);

                return template.Render();
            } 
            catch (Exception)
            {
                // cannot parse

                return liquidTemplate;
            }

            */
        }
    }
}
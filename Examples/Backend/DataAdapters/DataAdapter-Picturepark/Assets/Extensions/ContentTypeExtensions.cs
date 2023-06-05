using System.Collections.Generic;
using Picturepark.SDK.V1.Contract;

namespace SmintIo.Portals.DataAdapter.Picturepark.Assets.Extensions
{
    public static class ContentTypeExtensions
    {
        private static readonly List<string> _imageContentTypes = new()
        {
            ContentType.Bitmap.ToString(),
            ContentType.VectorGraphic.ToString(),
            ContentType.RawImage.ToString()
        };

        private static readonly List<string> _videoContentTypes = new()
        {
            ContentType.Video.ToString()
        };

        private static readonly List<string> _audioContentTypes = new()
        {
            ContentType.Audio.ToString()
        };

        private static readonly List<string> _documentContentTypes = new()
        {
            ContentType.InterchangeDocument.ToString(),
            ContentType.WordProcessingDocument.ToString(),
            ContentType.TextDocument.ToString(),
            ContentType.DesktopPublishingDocument.ToString(),
            ContentType.Presentation.ToString(),
            ContentType.Spreadsheet.ToString()
        };

        public static List<string> GetPictureparkContentTypes(this DataAdapterSDK.DataAdapters.Interfaces.Assets.Models.ContentType? contentType)
        {
            return contentType switch
            {
                DataAdapterSDK.DataAdapters.Interfaces.Assets.Models.ContentType.Image => _imageContentTypes,
                DataAdapterSDK.DataAdapters.Interfaces.Assets.Models.ContentType.Video => _videoContentTypes,
                DataAdapterSDK.DataAdapters.Interfaces.Assets.Models.ContentType.Audio => _audioContentTypes,
                DataAdapterSDK.DataAdapters.Interfaces.Assets.Models.ContentType.Document => _documentContentTypes,
                _ => _imageContentTypes,
            };
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;
using SmintIo.Portals.DataAdapter.Picturepark.Assets.Common;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Impl;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets.Parameters;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets.Results;
using SmintIo.Portals.SDK.Core.Models.Metamodel.Data;

namespace SmintIo.Portals.DataAdapter.Picturepark.Assets
{
    public partial class PictureparkAssetsDataAdapter : AssetsDataAdapterBaseImpl
    {
        private static List<string> ImageContentTypes = new List<string>() { ContentType.Bitmap.ToString() };
        private static List<string> VideoContentTypes = new List<string>() { ContentType.Video.ToString() };
        private static List<string> AudioContentTypes = new List<string>() { ContentType.Audio.ToString() };
        private static List<string> DocumentContentTypes = new List<string>() { 
            ContentType.InterchangeDocument.ToString(), 
            ContentType.WordProcessingDocument.ToString(), 
            ContentType.TextDocument.ToString(), 
            ContentType.DesktopPublishingDocument.ToString(),  
            ContentType.Presentation.ToString(), 
            ContentType.Spreadsheet.ToString() 
        };

        public override async Task<GetRandomAssetsResult> GetRandomAssetsAsync(GetRandomAssetsParameters parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            int limit = parameters?.Max ?? 1;
            if (limit <= 0)
                limit = 1;

            List<string> pictureparkContentTypes;

            switch (parameters.ContentType)
            {
                case DataAdapterSDK.DataAdapters.Interfaces.Assets.Models.ContentType.Image:
                    pictureparkContentTypes = ImageContentTypes;
                    break;
                case DataAdapterSDK.DataAdapters.Interfaces.Assets.Models.ContentType.Video:
                    pictureparkContentTypes = VideoContentTypes;
                    break;
                case DataAdapterSDK.DataAdapters.Interfaces.Assets.Models.ContentType.Audio:
                    pictureparkContentTypes = AudioContentTypes;
                    break;
                case DataAdapterSDK.DataAdapters.Interfaces.Assets.Models.ContentType.Document:
                    pictureparkContentTypes = DocumentContentTypes;
                    break;

                default:
                    pictureparkContentTypes = ImageContentTypes;
                    break;
            }

            var queryAmount = limit * 5;
            if (queryAmount < 50)
                queryAmount = 50;

            if (queryAmount > 100)
                queryAmount = 100;

            var (response, contentDetails) = await _client.SearchContentAsync(
                searchString: null,
                aggregators: null,
                new TermsFilter()
                {
                    Field = nameof(ContentDetail.ContentType).ToLowerCamelCase(),
                    Terms = pictureparkContentTypes
                },
                aggregationFilters: null,
                pageToken: null,
                pageSize: queryAmount,
                sortInfos: null).ConfigureAwait(false);

            var random = new Random();

            if (contentDetails == null)
            {
                return new GetRandomAssetsResult
                {
                    AssetDataObjects = new AssetDataObject[0]
                };
            }

            var assetDataObjects =
                contentDetails
                    .OrderBy(x => random.Next())
                    .Take(limit)
                    .Select(contentDetail => PictureparkContentConverter.Convert(Context, contentDetail))
                    .ToArray();

            return new GetRandomAssetsResult()
            {
                AssetDataObjects = assetDataObjects
            };
        }
    }
}
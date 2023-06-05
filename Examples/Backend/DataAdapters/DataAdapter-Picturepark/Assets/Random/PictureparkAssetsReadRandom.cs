using System;
using System.Linq;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;
using SmintIo.Portals.DataAdapter.Picturepark.Assets.Common;
using SmintIo.Portals.DataAdapter.Picturepark.Assets.Extensions;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Impl;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets.Parameters;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets.Results;
using SmintIo.Portals.SDK.Core.Models.Metamodel.Data;

namespace SmintIo.Portals.DataAdapter.Picturepark.Assets
{
    public partial class PictureparkAssetsDataAdapter : AssetsDataAdapterBaseImpl
    {
        public override async Task<GetRandomAssetsResult> GetRandomAssetsAsync(GetRandomAssetsParameters parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            int limit = parameters?.Max ?? 1;
            if (limit <= 0)
                limit = 1;

            var pictureparkContentTypes = parameters.ContentType.GetPictureparkContentTypes();

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
using System;
using System.Linq;
using System.Threading.Tasks;
using SmintIo.Portals.Connector.HelloWorld.Models.Common;
using SmintIo.Portals.Connector.HelloWorld.Models.Requests;
using SmintIo.Portals.DataAdapter.HelloWorld.Assets.Common;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Impl;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets.Models;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets.Parameters;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets.Results;
using SmintIo.Portals.SDK.Core.Models.Metamodel.Data;

namespace SmintIo.Portals.DataAdapter.HelloWorld.Assets
{
    public partial class HelloWorldAssetsDataAdapter : AssetsDataAdapterBaseImpl
    {
        public override async Task<GetRandomAssetsResult> GetRandomAssetsAsync(GetRandomAssetsParameters parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            var limit = parameters?.Max ?? 1;

            if (limit <= 0)
            {
                limit = 1;
            }

            var queryAmount = limit * 5;

            if (queryAmount < 50)
            {
                queryAmount = 50;
            }

            if (queryAmount > 100)
            {
                queryAmount = 100;
            }

            var contentType = GetHelloWorldContentType(parameters.ContentType);

            var searchAssetsRequest = new HelloWorldSearchAssetsRequest
            {
                Limit = queryAmount,
                ContentType = contentType
            };

            var searchAssetsResponse = await _helloWorldClient.SearchAssetsAsync(searchAssetsRequest).ConfigureAwait(false);

            if (searchAssetsResponse == null || searchAssetsResponse.Assets == null)
            {
                return new GetRandomAssetsResult
                {
                    AssetDataObjects = Array.Empty<AssetDataObject>()
                };
            }

            var converter = new HelloWorldContentConverter(_logger, Context, entityModelProvider: null, customFieldById: null);

            var random = new Random();

            var assetDataObjects = searchAssetsResponse.Assets
                .OrderBy(i => random.Next())
                .Take(limit)
                .Select(m => converter.GetAssetDataObject(m))
                .ToArray();

            return new GetRandomAssetsResult
            {
                AssetDataObjects = assetDataObjects
            };
        }

        private static HelloWorldContentType GetHelloWorldContentType(ContentType? contentType)
        {
            return contentType switch
            {
                ContentType.Image => HelloWorldContentType.Image,
                ContentType.Document => HelloWorldContentType.Document,
                ContentType.Video => HelloWorldContentType.Video,
                ContentType.Audio => HelloWorldContentType.Audio,
                _ => throw new NotSupportedException()
            };
        }
    }
}
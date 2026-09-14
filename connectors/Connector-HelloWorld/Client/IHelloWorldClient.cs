using System.Collections.Generic;
using System.Threading.Tasks;
using SmintIo.Portals.Connector.HelloWorld.Models.Requests;
using SmintIo.Portals.Connector.HelloWorld.Models.Responses;
using SmintIo.Portals.ConnectorSDK.Clients.Prefab;
using SmintIo.Portals.SDK.Core.Http.Prefab.Extensions;
using SmintIo.Portals.SDK.Core.Http.Prefab.Models;

namespace SmintIo.Portals.Connector.HelloWorld.Client
{
    /// <summary>
    /// Client interface of external systems with relevant functionality definitions
    /// </summary>
    public interface IHelloWorldClient : IClient
    {
        IRequestFailedHandler DefaultRequestFailedHandler { get; }

        Task EndpointTestAsync();

        Task<IDictionary<string, HelloWorldCustomFieldResponse>> GetCustomFieldByIdAsync(bool getFreshData);

        Task<HelloWorldSearchAssetsResponse> SearchAssetsAsync(HelloWorldSearchAssetsRequest searchRequest);

        Task<HelloWorldAssetResponse> GetAssetAsync(string assetId);

        Task<HelloWorldAssetResponses> GetAssetsAsync(ICollection<string> assetIds);

        Task<StreamResponse> GetStreamResponseWithoutBackoffAsync(string thumbnailUrl);
    }
}

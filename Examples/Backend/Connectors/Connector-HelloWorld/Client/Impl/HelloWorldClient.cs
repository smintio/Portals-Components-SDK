using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SmintIo.Portals.Connector.HelloWorld.Models.Requests;
using SmintIo.Portals.Connector.HelloWorld.Models.Responses;
using SmintIo.Portals.ConnectorSDK.Clients.Prefab;
using SmintIo.Portals.SDK.Core.Cache;
using SmintIo.Portals.SDK.Core.Http.Prefab.Extensions;
using SmintIo.Portals.SDK.Core.Http.Prefab.Models;
using SmintIo.Portals.SDK.Core.Models.Context;

namespace SmintIo.Portals.Connector.HelloWorld.Client.Impl
{
    public class HelloWorldClient : BaseRestSharpApiClient, IHelloWorldClient
    {
        private readonly ICache _cache;
        private readonly string _clientId;

        public HelloWorldClient(
            ILogger logger,
            ICache cache,
            IPortalsContextModel portalsContextModel,
            IHttpClientFactory httpClientFactory,
            string clientId,
            string baseUrl,
            Func<string> accessTokenFunc)
            : base(HelloWorldConnectorStartup.HelloWorldConnector, portalsContextModel, httpClientFactory, logger, baseUrl, accessTokenFunc)
        {
            _cache = cache;
            _clientId = clientId;
            DefaultRequestFailedHandler = new HelloWorldDefaultRequestFailedHandler();
        }

        public IRequestFailedHandler DefaultRequestFailedHandler { get; private set; }

        public Task EndpointTestAsync()
        {
            return Task.CompletedTask;
        }

        public async Task<IDictionary<string, HelloWorldCustomFieldResponse>> GetCustomFieldByIdAsync(bool getFreshData)
        {
            var cacheKey = $"custom_fields_{_clientId}";

            Dictionary<string, HelloWorldCustomFieldResponse> customFieldsById;

            if (!getFreshData)
            {
                customFieldsById = await _cache.GetAsync<Dictionary<string, HelloWorldCustomFieldResponse>>(cacheKey).ConfigureAwait(false);

                if (customFieldsById != null)
                {
                    return customFieldsById;
                }
            }

            customFieldsById = HelloWorldArtificialData.CustomFieldsById;

            await _cache.StoreAsync(cacheKey, customFieldsById, TimeSpan.FromDays(7)).ConfigureAwait(false);

            return customFieldsById;
        }

        public async Task<HelloWorldSearchAssetsResponse> SearchAssetsAsync(HelloWorldSearchAssetsRequest searchRequest)
        {
            if (searchRequest == null)
            {
                return null;
            }

            IEnumerable<HelloWorldAssetResponse> assets = HelloWorldArtificialData.HelloWorldAssets;

            if (!string.IsNullOrEmpty(searchRequest.SearchQuery))
            {
                assets = assets.Where(a => a.FileName.Contains(searchRequest.SearchQuery) || a.FileName.Replace("-", " ").Contains(searchRequest.SearchQuery));
            }

            if (searchRequest.FacetFilters != null)
            {
                foreach (var facetFilter in searchRequest.FacetFilters)
                {
                    if (string.IsNullOrEmpty(facetFilter.StringValue))
                    {
                        continue;
                    }

                    assets = assets.Where(a => a.CustomFieldValues
                        .Where(cfv => cfv.Value is string)
                        .Any(cfv => cfv.CustomFieldId == facetFilter.Id && (string)cfv.Value == facetFilter.StringValue));
                }
            }

            if (searchRequest.Ids != null)
            {
                assets = assets.Where(a => searchRequest.Ids.Contains(a.Id));
            }

            if (searchRequest.ContentType.HasValue)
            {
                assets = assets.Where(a => a.ContentType == searchRequest.ContentType);
            }

            if (searchRequest.Skip.HasValue)
            {
                assets = assets.Skip(searchRequest.Skip.Value);
            }

            if (searchRequest.Limit.HasValue)
            {
                assets = assets.Take(searchRequest.Limit.Value);
            }

            if (!string.IsNullOrEmpty(searchRequest.OrderBy))
            {
                assets = searchRequest.OrderBy.StartsWith("!")
                    ? assets.OrderByDescending(a => a.FileName)
                    : assets.OrderBy(a => a.FileName);
            }

            var assetsToReturn = assets.ToList();

            var searchFacetResponses = new List<HelloWorldSearchFacetResponse>();

            if (assetsToReturn.Any())
            {
                var customFieldById = await GetCustomFieldByIdAsync(getFreshData: false).ConfigureAwait(false);

                foreach (var asset in assetsToReturn)
                {
                    var customFieldValueGroups = asset.CustomFieldValues.GroupBy(cfv => cfv.CustomFieldId ?? string.Empty);

                    foreach (var customFieldValueGroup in customFieldValueGroups)
                    {
                        if (!customFieldById.TryGetValue(customFieldValueGroup.Key, out var customFieldResponse))
                        {
                            continue;
                        }

                        var groupCount = customFieldValueGroup.Count();

                        var searchFacetResponse = new HelloWorldSearchFacetResponse
                        {
                            Id = customFieldResponse.Id,
                            Label = customFieldResponse.Label,
                            SearchFacetValues = customFieldValueGroup
                                .Select(cfv => new HelloWorldSearchFacetValueResponse
                                {
                                    Label = cfv.Label,
                                    Value = cfv.Value as string,
                                    Count = groupCount
                                })
                                .ToList()
                        };

                        searchFacetResponses.Add(searchFacetResponse);
                    }
                }
            }

            var searchAssetsResponse = new HelloWorldSearchAssetsResponse
            {
                Assets = assetsToReturn,
                SearchFacets = searchFacetResponses,
                TotalCount = HelloWorldArtificialData.HelloWorldAssets.Count
            };

            return searchAssetsResponse;
        }

        public Task<HelloWorldAssetResponse> GetAssetAsync(string assetId)
        {
            var assetResponse = HelloWorldArtificialData.HelloWorldAssets.SingleOrDefault(a => a.Id == assetId);

            return Task.FromResult(assetResponse);
        }

        public Task<HelloWorldAssetResponses> GetAssetsAsync(ICollection<string> assetIds)
        {
            var assets = HelloWorldArtificialData.HelloWorldAssets
                .Where(a => assetIds.Contains(a.Id))
                .ToList();

            var assetResponses = new HelloWorldAssetResponses
            {
                Assets = assets,
                Count = assets.Count
            };

            return Task.FromResult(assetResponses);
        }

        public Task<StreamResponse> GetStreamResponseWithoutBackoffAsync(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return Task.FromResult<StreamResponse>(null);
            }

            if (url.Contains('?'))
            {
                url = url.Remove(url.IndexOf("?"));
            }

            var asset = HelloWorldArtificialData.HelloWorldAssets.FirstOrDefault(a => a.DownloadUrl == url || a.PreviewUrl == url);

            if (asset == null)
            {
                return Task.FromResult<StreamResponse>(null);
            }

            return Task.FromResult(new StreamResponse
            {
                FileName = asset.FileName,
                FileSizeInBytes = asset.FileSize,
                MediaType = asset.MimeType,
                Stream = new MemoryStream()
            });
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Picturepark.SDK.V1.Contract;
using SmintIo.Portals.Connector.Picturepark.Metamodel.Aggregations;
using SmintIo.Portals.ConnectorSDK.Clients.Prefab;
using SmintIo.Portals.SDK.Core.Cache;
using SmintIo.Portals.SDK.Core.Http.Prefab.Extensions;
using SmintIo.Portals.SDK.Core.Http.Prefab.Models;
using SmintIo.Portals.SDK.Core.Models.Context;
using SmintIo.Portals.SDK.Core.Rest.Prefab.Exceptions;

namespace SmintIo.Portals.Connector.Picturepark.Client.Impl
{
    public class DefaultPictureparkClient : BaseHttpClientApiClient, IPictureparkClient
    {
        protected HttpClient HttpClient { get; private set; }

        private string _debugAccessTokenCache;
        private IPictureparkService _service;

        private readonly string _channelId;
        private readonly ICache _cache;

        private readonly string _apiUrl;
        private readonly string _customerAlias;

        private readonly bool _thumbnailPortalPresent;
        private readonly bool _thumbnailExtraLargePresent;

        private readonly ILogger _logger;

        private readonly PictureparkConnector _pictureparkConnector;

        private readonly IPortalsContextModel _portalsContextModel;

        public IRequestFailedHandler DefaultRequestFailedHandler { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="channelId"></param>
        /// <param name="logger"></param>
        public DefaultPictureparkClient(
            PictureparkConnector pictureparkConnector, 
            IPortalsContextModel portalsContextModel, 
            string accessToken, 
            IPictureparkService service, 
            string channelId, 
            ICache cache, 
            IHttpClientFactory httpClientFactory,
            HttpClient httpClient, 
            string apiUrl, 
            string customerAlias, 
            bool thumbnailPortalPresent, 
            bool thumbnailExtraLargePresent, 
            ILogger logger)
            : base(PictureparkConnectorStartup.PictureparkConnector, portalsContextModel, httpClientFactory, logger)
        {
            _pictureparkConnector = pictureparkConnector;

            _portalsContextModel = portalsContextModel;

            _debugAccessTokenCache = accessToken;
            _service = service;

            _channelId = channelId;
            _apiUrl = apiUrl;
            _customerAlias = customerAlias;

            _thumbnailPortalPresent = thumbnailPortalPresent;
            _thumbnailExtraLargePresent = thumbnailExtraLargePresent;

            _logger = logger;

            _cache = cache;

            HttpClient = httpClient;

            DefaultRequestFailedHandler = new PictureparkDefaultRequestFailedHandler(this);
        }

        internal async Task<RequestFailedHandlerResult?> HandleUnauthorizedAsync()
        {
            // try to refresh the auth token, then backoff again

            if (_pictureparkConnector.PassThroughAuthenticationTokenProvider != null)
            {
                try
                {
                    var (newAccessToken, newService) = await _pictureparkConnector.RefreshPassThroughAuthorizationValuesAsync(log: true).ConfigureAwait(false);

                    _logger.LogInformation($"Authorization values refreshed successfully ({newAccessToken}, {newService}, {_portalsContextModel})");

                    _debugAccessTokenCache = newAccessToken;
                    _service = newService;

                    // if we succeed, backoff and try again

                    return RequestFailedHandlerResult.Retry;
                }
                catch (ExternalDependencyException e)
                when (e.ErrorCode == ExternalDependencyStatusEnum.AuthorizationValuesExpired)
                {
                    // if we fail because of expired grant, fail and report

                    _logger.LogError($"Picturepark pass through auth refresh failed because authorization values expired, failing and reporting: {e} ({_portalsContextModel})", PictureparkConnectorStartup.PictureparkConnector);

                    throw new ExternalDependencyException(ExternalDependencyStatusEnum.PassThroughAuthorizationValuesExpired, e.Message, e.Identifier);
                }
                catch (Exception e)
                {
                    // if we fail because of something else, backoff and try again

                    _logger.LogError($"Picturepark auth failed with exception, backing off and trying again: {e} ({_portalsContextModel})", PictureparkConnectorStartup.PictureparkConnector);

                    return null;
                }
            }

            return null;
        }

        public async Task InitializeChannelAggregatorsAsync()
        {
            var channelAggregatorsInitializedMarker = await _cache.GetAsync<ChannelAggregatorsInitializedMarker>("ChannelAggregatorsInitialized").ConfigureAwait(false);
            if (channelAggregatorsInitializedMarker != null)
                return;

            var channel = await ExecuteWithBackoffAsync(
                apiFunc: (_) =>
                    {
                        return _service.Channel.GetAsync(_channelId);
                    },
                requestFailedHandler: DefaultRequestFailedHandler,
                hint: "Initialize channel aggregators",
                isGet: false
            ).ConfigureAwait(false);

            if (channel != null)
            {
                AggregatorManager aggregatorManager = new AggregatorManager(channel.Aggregations, channel.Sort, channel.SortFields);

                await aggregatorManager.CacheAggregatorsAsync(_cache).ConfigureAwait(false);
            }

            await _cache.StoreAsync("ChannelAggregatorsInitialized", new ChannelAggregatorsInitializedMarker()).ConfigureAwait(false);
        }

        public async Task<AggregatorManager> GetAggregatorManagerAsync()
        {
            AggregatorManager aggregatorManager = new AggregatorManager();

            await aggregatorManager.InitializeFromCacheAsync(_cache).ConfigureAwait(false);

            return aggregatorManager;
        }

        /// <summary>
        /// Returns all available schemas, sorted by their ID.
        /// </summary>
        /// <returns></returns>
        public async Task<ICollection<Schema>> GetSchemasAsync()
        {
            string pageToken = null;
            List<Schema> schemas = new List<Schema>(256);

            do
            {
                SchemaSearchRequest request = new SchemaSearchRequest()
                {
                    PageToken = pageToken,
                    Limit = 5000
                };

                var schemaSearchResult = await ExecuteWithBackoffAsync(
                    apiFunc: (_) =>
                        {
                            return _service.Schema.SearchAsync(request);
                        },
                    requestFailedHandler: DefaultRequestFailedHandler,
                    hint: "Get schemas",
                    isGet: false
                ).ConfigureAwait(false);

                if (schemaSearchResult == null)
                {
                    break;
                }

                if (schemaSearchResult.Results != null)
                {
                    schemas.AddRange(schemaSearchResult.Results);
                }

                pageToken = schemaSearchResult.PageToken;
            } while (pageToken != null);
            //
            // The IDs are usually human-readable names, so we sort them alphabetically,
            // mainly for debugging purposes.
            //
            schemas.Sort((s1, s2) => s1.Id.CompareTo(s2.Id));
            return schemas;
        }

        /// <summary>
        /// Returns all available output formats.
        /// </summary>
        /// <returns></returns>
        public async Task<ICollection<OutputFormatInfo>> GetOutputFormatsAsync()
        {
            var customerInfo = await ExecuteWithBackoffAsync(
                apiFunc: (_) =>
                    {
                        return _service.Info.GetInfoAsync();
                    },
                requestFailedHandler: DefaultRequestFailedHandler,
                hint: "Get output formats",
                isGet: false
            ).ConfigureAwait(false);

            return customerInfo?.OutputFormats;
        }

        /// <summary>
        /// Returns schema details for the list of schemas specified via argument.
        /// Schema details contain fields specification used to populate entities.
        /// </summary>
        /// <param name="schemas"></param>
        /// <returns></returns>
        public async Task<ICollection<SchemaDetail>> GetSchemaDetailsAsync(ICollection<Schema> schemas)
        {
            const int MaxLimit = 100;
            List<SchemaDetail> result = new List<SchemaDetail>();

            for (int i = 0, to = schemas.Count; i < to; i += MaxLimit)
            {
                int count = Math.Min(to - i, MaxLimit);
                IEnumerable<string> schemaIds = schemas.Skip(i).Take(count).Select(s => s.Id);

                var schemaDetails = await ExecuteWithBackoffAsync(
                    apiFunc: (_) =>
                        {
                            return _service.Schema.GetManyAsync(schemaIds);
                        },
                    requestFailedHandler: DefaultRequestFailedHandler,
                    hint: "Get schema details",
                    isGet: false
                ).ConfigureAwait(false);

                if (schemaDetails != null)
                {
                    result.AddRange(schemaDetails);
                }
            }

            return result;
        }

        /// <summary>
        /// Returns list items for the specified schemas. The schemas are expected to be of type
        /// <see cref="SchemaType.List"/>.
        /// </summary>
        /// <param name="schemas"></param>
        /// <returns></returns>
        public async Task<ICollection<ListItem>> GetListItemsAsync(ICollection<Schema> schemas)
        {
            string pageToken = null;
            List<ListItem> listItems = new List<ListItem>(512);
            ICollection<string> schemaIds = schemas.Select(s => s.Id).ToList();

            do
            {
                ListItemSearchRequest listItemSearchRequest = new ListItemSearchRequest()
                {
                    SchemaIds = schemaIds,
                    ResolveBehaviors = new[] {
                        ListItemResolveBehavior.Content,
                        ListItemResolveBehavior.OuterDisplayValueName
                    },
                    Limit = 5000,
                    PageToken = pageToken
                };

                var listItemSearchResult = await ExecuteWithBackoffAsync(
                    apiFunc: (_) =>
                        {
                            return _service.ListItem.SearchAsync(listItemSearchRequest);
                        },
                    requestFailedHandler: DefaultRequestFailedHandler,
                    hint: "Get list items",
                    isGet: false
                ).ConfigureAwait(false);

                if (listItemSearchResult == null)
                {
                    break;
                }

                if (listItemSearchResult.Results != null)
                {
                    listItems.AddRange(listItemSearchResult.Results);
                }

                pageToken = listItemSearchResult.PageToken;

            } while (pageToken != null);

            return listItems;
        }

        public async Task<Channel> GetChannelAsync(string channelId)
        {
            var channel = await ExecuteWithBackoffAsync(
                apiFunc: (_) =>
                    {
                        return _service.Channel.GetAsync(channelId);
                    },
                requestFailedHandler: DefaultRequestFailedHandler,
                hint: $"Get channel ({channelId})",
                isGet: true
            ).ConfigureAwait(false);

            return channel;
        }

        public async Task<ICollection<Channel>> GetAllChannelsAsync()
        {
            var channels = await ExecuteWithBackoffAsync(
                apiFunc: (_) =>
                    {
                        return _service.Channel.GetAllAsync();
                    },
                requestFailedHandler: DefaultRequestFailedHandler,
                hint: "Get all channels",
                isGet: false
            ).ConfigureAwait(false);

            return channels;
        }

        public async Task<(ContentSearchResult, ICollection<ContentDetail>)> SearchContentAsync(string searchString, ICollection<AggregatorBase> aggregators,
            FilterBase filter, ICollection<AggregationFilter> aggregationFilters, string pageToken, int? pageSize, ICollection<SortInfo> sortInfos,
            SearchType? searchType = null, bool includeFullText = false, bool resolveMetadata = false)
        {
            SearchBehavior[] searchBehaviors = null;

            switch (searchType)
            {
                case SearchType.SimpleOr:
                    searchBehaviors = new SearchBehavior[]
                    {
                        SearchBehavior.SimplifiedSearchOr,
                        SearchBehavior.DropInvalidCharactersOnFailure,
                        SearchBehavior.WildcardOnSingleTerm
                    };

                    break;

                case SearchType.Advanced:
                    searchBehaviors = new SearchBehavior[]
                    {
                        SearchBehavior.DropInvalidCharactersOnFailure,
                        SearchBehavior.WildcardOnSingleTerm
                    };

                    break;

                case SearchType.SimpleAnd:
                default:
                    searchBehaviors = new SearchBehavior[]
                    {
                        SearchBehavior.SimplifiedSearch,
                        SearchBehavior.DropInvalidCharactersOnFailure,
                        SearchBehavior.WildcardOnSingleTerm
                    };

                    break;
            }

            ContentSearchRequest request = new ContentSearchRequest()
            {
                ChannelId = _channelId,
                Aggregators = aggregators,
                Filter = filter,
                AggregationFilters = aggregationFilters,
                SearchString = searchString,
                SearchBehaviors = searchBehaviors,
                PageToken = pageToken,
                Limit = pageSize.HasValue ? pageSize.Value : 30,
                BrokenDependenciesFilter = BrokenDependenciesFilter.All,
                SearchType = includeFullText ? ContentSearchType.MetadataAndFullText : ContentSearchType.Metadata,
                Sort = sortInfos
            };

            var contentSearchResult = await ExecuteWithBackoffAsync(
                apiFunc: (_) =>
                {
                    return _service.Content.SearchAsync(request);
                },
                requestFailedHandler: DefaultRequestFailedHandler,
                hint: "Search content",
                isGet: false
            ).ConfigureAwait(false);

            ICollection<ContentDetail> contentDetails;

            if (resolveMetadata)
            {
                contentDetails = await GetManyAsync(contentSearchResult?.Results?.Select(result => result.Id), new ContentResolveBehavior[] {
                    ContentResolveBehavior.OuterDisplayValueThumbnail,
                    ContentResolveBehavior.OuterDisplayValueList,
                    ContentResolveBehavior.OuterDisplayValueName,
                    ContentResolveBehavior.Metadata,
                    ContentResolveBehavior.Content,
                    ContentResolveBehavior.Outputs,
                    ContentResolveBehavior.Permissions
                }).ConfigureAwait(false);
            }
            else
            {
                contentDetails = await GetManyAsync(contentSearchResult?.Results?.Select(result => result.Id), new ContentResolveBehavior[] {
                    ContentResolveBehavior.OuterDisplayValueThumbnail,
                    ContentResolveBehavior.OuterDisplayValueList,
                    ContentResolveBehavior.OuterDisplayValueName,
                    ContentResolveBehavior.Content,
                    ContentResolveBehavior.Outputs,
                    ContentResolveBehavior.Permissions
                }).ConfigureAwait(false);
            }

            return (contentSearchResult, contentDetails);
        }

        public async Task<(ContentDetail ContentDetail, ContentType? OriginalContentType)> GetContentAsync(string id)
        {
            await CheckContentChannelAccessAsync(id).ConfigureAwait(false);

            var contentDetail = await ExecuteWithBackoffAsync(
               apiFunc: (_) =>
               {
                    return _service.Content.GetAsync(id, new ContentResolveBehavior[] {
                        ContentResolveBehavior.Metadata,
                        ContentResolveBehavior.Content,
                        ContentResolveBehavior.OuterDisplayValueThumbnail,
                        ContentResolveBehavior.OuterDisplayValueList,
                        ContentResolveBehavior.OuterDisplayValueName,
                        // ContentResolveBehavior.OuterDisplayValueDetail,
                        ContentResolveBehavior.LinkedListItems,
                        ContentResolveBehavior.Outputs,
                        ContentResolveBehavior.Permissions,
                        ContentResolveBehavior.DynamicViewFields
                    });
               },
               requestFailedHandler: DefaultRequestFailedHandler,
               hint: "Search content",
               isGet: false
           ).ConfigureAwait(false);

            if (contentDetail == null)
            {
                return (null, null);
            }

            var originalContentType = await HandleThumbnailReferenceFileTypeAsync(contentDetail).ConfigureAwait(false);

            return (contentDetail, originalContentType);
        }

        public async Task<ICollection<ContentDetail>> GetContentsAsync(ICollection<string> ids, bool skipNonAccessibleContents = false)
        {
            var accessibleIds = await CheckContentChannelAccessAsync(ids, skipNonAccessibleContents).ConfigureAwait(false);

            var contentDetails = await GetManyAsync(accessibleIds, new ContentResolveBehavior[] {
                ContentResolveBehavior.Metadata,
                ContentResolveBehavior.Content,
                ContentResolveBehavior.OuterDisplayValueThumbnail,
                ContentResolveBehavior.OuterDisplayValueList,
                ContentResolveBehavior.OuterDisplayValueName,
                // ContentResolveBehavior.OuterDisplayValueDetail,
                ContentResolveBehavior.LinkedListItems,
                ContentResolveBehavior.Outputs,
                ContentResolveBehavior.Permissions
            }).ConfigureAwait(false);

            await HandleThumbnailReferenceFileTypeAsync(contentDetails).ConfigureAwait(false);

            return contentDetails;
        }

        public async Task<ICollection<OutputFormatDetail>> GetOutputFormatsAsync(ICollection<string> outputFormatIds)
        {
            var outputFormats = await ExecuteWithBackoffAsync(
                apiFunc: (_) =>
                {
                    return _service.OutputFormat.GetManyAsync(outputFormatIds);
                },
                requestFailedHandler: DefaultRequestFailedHandler,
                hint: "Get output formats",
                isGet: false
            ).ConfigureAwait(false);

            return outputFormats;
        }

        public async Task<ICollection<ContentDetail>> GetContentPermissionsAsync(ICollection<string> ids)
        {
            var contentSearchResult = await GetContentChannelAccessIdsAsync(ids).ConfigureAwait(false);

            var contentDetails = await GetManyAsync(contentSearchResult.Results?.Select(result => result.Id), new ContentResolveBehavior[] {
                ContentResolveBehavior.Permissions
            }).ConfigureAwait(false);

            return contentDetails;
        }

        public async Task<ICollection<ContentDetail>> GetContentOutputsAsync(ICollection<string> ids)
        {
            var contentSearchResult = await GetContentChannelAccessIdsAsync(ids).ConfigureAwait(false);

            var contentDetails = await GetManyAsync(contentSearchResult.Results?.Select(result => result.Id), new ContentResolveBehavior[] {
                ContentResolveBehavior.Metadata,
                ContentResolveBehavior.Content,
                ContentResolveBehavior.OuterDisplayValueThumbnail,
                ContentResolveBehavior.OuterDisplayValueList,
                ContentResolveBehavior.OuterDisplayValueName,
                // ContentResolveBehavior.OuterDisplayValueDetail,
                ContentResolveBehavior.LinkedListItems,
                ContentResolveBehavior.Outputs,
                ContentResolveBehavior.Permissions,
                ContentResolveBehavior.DynamicViewFields
            }).ConfigureAwait(false);

            contentDetails = await HandleThumbnailReferenceAsync(contentDetails).ConfigureAwait(false);

            return contentDetails;
        }

        private async Task<ContentSearchResult> GetContentChannelAccessIdsAsync(ICollection<string> ids, int? limit = null)
        {
            ContentSearchRequest request = new ContentSearchRequest()
            {
                ChannelId = _channelId,
                Filter = new TermsFilter()
                {
                    Field = "id",
                    Terms = ids
                },
                AggregationFilters = null,
                SearchString = null,
                SearchBehaviors = new SearchBehavior[]
                {
                    SearchBehavior.SimplifiedSearch
                },
                PageToken = null,
                Limit = limit == null ? ids.Count : (int)limit,
                BrokenDependenciesFilter = BrokenDependenciesFilter.All,
                SearchType = ContentSearchType.Metadata,
                Sort = null
            };

            var contentSearchResult = await ExecuteWithBackoffAsync(
                apiFunc: (_) =>
                {
                    return _service.Content.SearchAsync(request);
                },
                requestFailedHandler: DefaultRequestFailedHandler,
                hint: "Get content channel access IDs",
                isGet: false
            ).ConfigureAwait(false);

            return contentSearchResult;
        }

        private async Task CheckContentChannelAccessAsync(string id)
        {
            await CheckContentChannelAccessAsync(new[] { id }, skipNonAccessibleContents: false).ConfigureAwait(false);
        }

        private async Task<ICollection<string>> CheckContentChannelAccessAsync(ICollection<string> ids, bool skipNonAccessibleContents)
        {
            if (ids == null || !ids.Any())
            {
                return Array.Empty<string>();
            }

            ids = ids.Distinct().ToList();

            if (!skipNonAccessibleContents)
            { 
                var contentSearchResult = await GetContentChannelAccessIdsAsync(ids, limit: 0);

                if (contentSearchResult.TotalResults < ids.Count)
                {
                    throw new ExternalDependencyException(ExternalDependencyStatusEnum.AssetNotFound, "The asset was not found", ids.First());
                }

                return ids;
            }
            else
            {
                var contentSearchResult = await GetContentChannelAccessIdsAsync(ids).ConfigureAwait(false);

                if (contentSearchResult.Results == null)
                {
                    return Array.Empty<string>();
                }
                        
                return contentSearchResult.Results.Select(result => result.Id).ToList();
            }
        }

        public async Task CreateContentsAsync(ICollection<ContentCreateRequest> contents)
        {
            if (contents == null || contents.Count == 0)
                return;

            var contentsCreateRequest = new ContentCreateManyRequest()
            {
                Items = contents
            };

            await ExecuteWithBackoffAsync(
               apiFunc: async (_) =>
               {
                   var result = await _service.Content.CreateManyAsync(contentsCreateRequest).ConfigureAwait(false);

                   await _service.BusinessProcess.WaitForCompletionAsync(result.BusinessProcessId);

                   // something

                   return true;
               },
               requestFailedHandler: DefaultRequestFailedHandler,
               hint: "Create contents",
               isGet: false
           ).ConfigureAwait(false);
        }

        public async Task UpdateContentsAsync(ICollection<ContentMetadataUpdateItem> contents)
        {
            if (contents == null || contents.Count == 0)
                return;

            await CheckContentChannelAccessAsync(contents.Select(content => content.Id).ToList(), skipNonAccessibleContents: false).ConfigureAwait(false);

            var contentsUpdateRequest = new ContentMetadataUpdateManyRequest()
            {
                Items = contents
            };

            await ExecuteWithBackoffAsync(
               apiFunc: async (_) =>
               {
                   var result = await _service.Content.UpdateMetadataManyAsync(contentsUpdateRequest).ConfigureAwait(false);

                   await _service.BusinessProcess.WaitForCompletionAsync(result.BusinessProcessId);

                   // something

                   return true;
               },
               requestFailedHandler: DefaultRequestFailedHandler,
               hint: "Update contents",
               isGet: false
           ).ConfigureAwait(false);
        }

        public async Task<StreamResponse> GetImageDownloadStreamAsync(string id, ThumbnailSize size, long? maxFileSizeBytes)
        {
            string downloadUri;

            if (size == ThumbnailSize.Large && _thumbnailExtraLargePresent)
            {
                var sizeString = "ThumbnailExtraLarge";

                downloadUri = $"{_apiUrl}v1/Contents/downloads/{Uri.EscapeDataString(id)}/{Uri.EscapeDataString(sizeString)}";
            }
            else if (size == ThumbnailSize.Large && _thumbnailPortalPresent)
            {
                var sizeString = "ThumbnailPortal";

                downloadUri = $"{_apiUrl}v1/Contents/downloads/{Uri.EscapeDataString(id)}/{Uri.EscapeDataString(sizeString)}";
            }
            else
            {
                var sizeString = size.ToPictureparkEnumMemberAttrValue();

                downloadUri = $"{_apiUrl}v1/Contents/thumbnails/{Uri.EscapeDataString(id)}/{Uri.EscapeDataString(sizeString)}";
            }

            var accessToken = _pictureparkConnector.GetAccessToken();

            var requestHeaders = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Picturepark-CustomerAlias", _customerAlias)
            };

            var streamResponse = await GetHttpClientStreamResponseWithBackoffAsync(downloadUri, requestFailedHandler: DefaultRequestFailedHandler, accessToken, hint: $"Get image download stream ({id}, {size})", requestHeaders: requestHeaders, cancelRequestDelay: TimeSpan.FromSeconds(5), maxFileSizeBytes);

            return streamResponse;
        }

        public async Task<StreamResponse> GetPlaybackDownloadStreamAsync(string id, string size, long? maxFileSizeBytes)
        {
            var downloadUri = $"{_apiUrl}v1/Contents/downloads/{Uri.EscapeDataString(id)}/{Uri.EscapeDataString(size)}";

            var accessToken = _pictureparkConnector.GetAccessToken();

            var requestHeaders = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Picturepark-CustomerAlias", _customerAlias)
            };

            var streamResponse = await GetHttpClientStreamResponseWithBackoffAsync(downloadUri, requestFailedHandler: DefaultRequestFailedHandler, accessToken, hint: $"Get playback download stream ({id}, {size})", requestHeaders: requestHeaders, cancelRequestDelay: TimeSpan.FromSeconds(5), maxFileSizeBytes);

            return streamResponse;
        }

        public async Task<StreamResponse> GetDownloadStreamForOutputFormatIdAsync(string id, string outputFormatId, long? maxFileSizeBytes)
        {
            id = await GetThumbnailReferenceIdAsync(id).ConfigureAwait(false);

            var downloadUri = $"{_apiUrl}v1/Contents/downloads/{Uri.EscapeDataString(id)}/{Uri.EscapeDataString(outputFormatId)}";

            var accessToken = _pictureparkConnector.GetAccessToken();

            var requestHeaders = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Picturepark-CustomerAlias", _customerAlias)
            };

            var streamResponse = await GetHttpClientStreamResponseWithBackoffAsync(downloadUri, requestFailedHandler: DefaultRequestFailedHandler, accessToken, hint: $"Get download stream for output format ({id}, {outputFormatId})", requestHeaders: requestHeaders, maxFileSizeBytes: maxFileSizeBytes);

            return streamResponse;
        }

        public async Task<ICollection<string>> GetProfileUserRoleIdsAsync()
        {
            if (_pictureparkConnector.PassThroughAuthenticationTokenProvider == null)
            {
                return null;
            }

            var profile = await ExecuteWithBackoffAsync(
               apiFunc: (_) =>
               {
                   try
                   {
                       return _service.Profile.GetAsync();
                   }
                   catch (PictureparkForbiddenException)
                   {
                       // lets immediately cancel

                       throw new ExternalDependencyException(ExternalDependencyStatusEnum.GetNotFound, "The Picturepark user was not found", PictureparkConnectorStartup.PictureparkConnector);
                   }
               },
               requestFailedHandler: DefaultRequestFailedHandler,
               hint: "Get profile user role IDs",
               isGet: false
           ).ConfigureAwait(false);

            if (profile == null)
            {
                return Array.Empty<string>();
            }

            var userRoles = profile.UserRoleIds?.Distinct().ToList();

            if (userRoles == null)
            {
                return Array.Empty<string>();
            }

            return userRoles;
        }

        private async Task<ICollection<ContentDetail>> GetManyAsync(IEnumerable<string> contentIds, ContentResolveBehavior[] contentResolveBehaviors)
        {
            List<ContentDetail> contentDetails = new List<ContentDetail>();

            if (contentIds != null && contentIds.Any())
            {
                const int MaxLimit = 100;

                for (int i = 0, to = contentIds.Count(); i < to; i += MaxLimit)
                {
                    int count = Math.Min(to - i, MaxLimit);
                    IEnumerable<string> partialContentIds = contentIds.Skip(i).Take(count);

                    var partialContentDetails = await ExecuteWithBackoffAsync(
                       apiFunc: (_) =>
                       {
                           return _service.Content.GetManyAsync(partialContentIds, contentResolveBehaviors);
                       },
                       requestFailedHandler: DefaultRequestFailedHandler,
                       hint: "Get many",
                       isGet: false
                   ).ConfigureAwait(false);

                    if (partialContentDetails == null)
                    {
                        continue;
                    }

                    contentDetails.AddRange(partialContentDetails);
                }
            }

            return contentDetails;
        }

        private async Task<ICollection<ContentDetail>> HandleThumbnailReferenceAsync(ICollection<ContentDetail> contentDetails)
        {
            if (contentDetails == null || !contentDetails.Any())
            {
                return contentDetails;
            }

            if (!contentDetails.Any(contentDetail => contentDetail.LayerSchemaIds.Any(layerSchemaId => string.Equals(layerSchemaId, "BrandContent"))))
            {
                return contentDetails;
            }

            var result = new List<ContentDetail>();

            foreach (var contentDetail in contentDetails)
            {
                var targetContentId = GetThumbnailReferenceTargetContentId(contentDetail);

                if (string.IsNullOrEmpty(targetContentId))
                {
                    result.Add(contentDetail);

                    continue;
                }

                var (targetContentDetail, _) = await GetContentAsync(targetContentId).ConfigureAwait(false);

                result.Add(targetContentDetail);
            }

            return result;
        }

        private async Task<ContentType?> HandleThumbnailReferenceFileTypeAsync(ContentDetail contentDetail)
        {
            var originalContentType = contentDetail.ContentType;

            await HandleThumbnailReferenceFileTypeAsync(new[] { contentDetail }).ConfigureAwait(false);

            if (contentDetail.ContentType != originalContentType)
            {
                return originalContentType;
            }

            return null;
        }

        private async Task HandleThumbnailReferenceFileTypeAsync(ICollection<ContentDetail> contentDetails)
        {
            if (contentDetails == null || !contentDetails.Any())
            {
                return;
            }

            if (!contentDetails.Any(contentDetail => contentDetail.LayerSchemaIds.Any(layerSchemaId => string.Equals(layerSchemaId, "BrandContent"))))
            {
                return;
            }

            foreach (var contentDetail in contentDetails)
            {
                var targetContentId = GetThumbnailReferenceTargetContentId(contentDetail);

                if (string.IsNullOrEmpty(targetContentId))
                {
                    continue;
                }

                var (targetContentDetail, _) = await GetContentAsync(targetContentId).ConfigureAwait(false);

                contentDetail.ContentType = targetContentDetail.ContentType;
            }
        }

        private async Task<string> GetThumbnailReferenceIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return id;
            }

            var (contentDetail, _) = await GetContentAsync(id).ConfigureAwait(false);

            var targetContentId = GetThumbnailReferenceTargetContentId(contentDetail);

            if (string.IsNullOrEmpty(targetContentId))
            {
                return id;
            }

            return targetContentId;
        }

        private string GetThumbnailReferenceTargetContentId(ContentDetail contentDetail)
        {
            var layerSchemaIds = contentDetail.LayerSchemaIds;

            if (layerSchemaIds == null ||
                !layerSchemaIds.Any() ||
                !layerSchemaIds.Any(layerSchemaId => string.Equals(layerSchemaId, "BrandContent")))
            {
                return null;
            }

            var brandContentLayer = contentDetail.Layer("BrandContent");

            if (brandContentLayer == null)
            {
                return null;
            }

            return GetRelationshipTargetContentId(brandContentLayer, "thumbnail2Original");
        }

        private string GetRelationshipTargetContentId(JObject contentSchema, string key)
        {
            if (!contentSchema.TryGetValue(key, out var relationshipToken))
            {
                return null;
            }

            if (relationshipToken.Type == JTokenType.Array)
            {
                // can be array as well - in this case, take first

                relationshipToken = relationshipToken.Children().FirstOrDefault();

                if (relationshipToken == null)
                {
                    return null;
                }
            }

            if (relationshipToken.Type != JTokenType.Object)
            {
                return null;
            }

            var relationshipObject = relationshipToken.Value<JObject>();

            if (relationshipObject == null ||
                !relationshipObject.TryGetValue("_targetId", out var targetIdToken) ||
                targetIdToken.Type != JTokenType.String ||
                !relationshipObject.TryGetValue("_targetDocType", out var targetDocTypeToken))
            {
                return null;
            }

            var targetDocType = targetDocTypeToken.Value<string>();

            if (!string.Equals(targetDocType, "Content"))
            {
                return null;
            }

            var targetId = targetIdToken.Value<string>();

            if (string.IsNullOrEmpty(targetId))
            {
                return null;
            }

            return targetId;
        }
    }
}
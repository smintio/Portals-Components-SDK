using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Picturepark.SDK.V1.Contract;
using Polly;
using Polly.Retry;
using SmintIo.Portals.Connector.Picturepark.Metamodel.Aggregations;
using SmintIo.Portals.SDK.Core.Cache;
using SmintIo.Portals.SDK.Core.Http.Prefab.Extensions;
using SmintIo.Portals.SDK.Core.Http.Prefab.Models;
using SmintIo.Portals.SDK.Core.Models.Context;
using SmintIo.Portals.SDK.Core.Rest.Prefab.Exceptions;

namespace SmintIo.Portals.Connector.Picturepark.Client.Impl
{
    public class DefaultPictureparkClient : IPictureparkClient
    {
        protected const int MaxRetryAttempts = 3;

        protected HttpClient HttpClient { get; private set; }
        protected AsyncRetryPolicy RetryPolicy { get; private set; }

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

        private readonly IPortalsContextModel _portalsContext;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="channelId"></param>
        /// <param name="logger"></param>
        public DefaultPictureparkClient(PictureparkConnector pictureparkConnector, IPortalsContextModel portalsContext, string accessToken, IPictureparkService service, string channelId, ICache cache, HttpClient httpClient, string apiUrl, string customerAlias, bool thumbnailPortalPresent, bool thumbnailExtraLargePresent, ILogger logger)
        {
            _pictureparkConnector = pictureparkConnector;

            _portalsContext = portalsContext;

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
            RetryPolicy = GetRetryStrategy();
        }

        private AsyncRetryPolicy GetRetryStrategy()
        {
            return Policy
                .Handle<ApiException>()
                .Or<Exception>()
                .WaitAndRetryAsync(
                    MaxRetryAttempts,
                    retryAttempt => TimeSpan.FromMilliseconds(Math.Pow(2, retryAttempt) * 100),
                    async (ex, timespan, context) =>
                    {
                        if (ex is ContentPermissionException)
                        {
                            // fail, because there is some low level permission error

                            throw ex;
                        }
                        if (ex is UserNotFoundException userNotFoundException)
                        {
                            // fail, because user was not found

                            throw new ExternalDependencyException(ExternalDependencyStatusEnum.GetNotFound, "The Picturepark user was not found", userNotFoundException.MissingUserId);
                        }
                        else if (ex is ApiException apiEx)
                        {
                            if (apiEx.StatusCode == (int)HttpStatusCode.TooManyRequests)
                            {
                                // too many requests, backoff and try again

                                return;
                            }
                            else if (apiEx.StatusCode == (int)HttpStatusCode.Unauthorized)
                            {
                                // try to refresh the auth token, then backoff again

                                if (_pictureparkConnector.PassThroughAuthenticationTokenProvider != null)
                                {
                                    try
                                    {
                                        var (newAccessToken, newService) = await _pictureparkConnector.RefreshPassThroughAuthorizationValuesAsync(log: true).ConfigureAwait(false);

                                        _logger.LogInformation($"Authorization values refreshed successfully ({newAccessToken}, {newService}, {_portalsContext})");

                                        _debugAccessTokenCache = newAccessToken;

                                        _service = newService;

                                        // if we succeed, backoff and try again

                                        return;
                                    }
                                    catch (ExternalDependencyException e)
                                    when (e.ErrorCode == ExternalDependencyStatusEnum.AuthorizationValuesExpired)
                                    {
                                        // if we fail because of expired grant, fail and report

                                        _logger.LogError($"PT auth refresh failed because authorization values expired, failing and reporting: {e} ({_portalsContext})");

                                        throw new ExternalDependencyException(ExternalDependencyStatusEnum.PassThroughAuthorizationValuesExpired, e.Message, e.Identifier);
                                    }
                                    catch (Exception e)
                                    {
                                        // if we fail because of something else, backoff and try again

                                        _logger.LogError($"PT auth failed with exception, backing off and trying again: {e} ({_portalsContext})");

                                        return;
                                    }
                                }
                            }

                            // expected error happened server side, most likely our problem, cancel

                            ExternalDependencyException.HandleStatusCode(apiEx.StatusCode, apiEx, "Picturepark request", PictureparkConnectorStartup.PictureparkConnector, targetGetUuid: null, _logger);
                        }

                        // some server side or communication issue, backoff and try again
                    });
        }

        public async Task InitializeChannelAggregatorsAsync()
        {
            var channelAggregatorsInitializedMarker = await _cache.GetAsync<ChannelAggregatorsInitializedMarker>("ChannelAggregatorsInitialized").ConfigureAwait(false);
            if (channelAggregatorsInitializedMarker != null)
                return;

            Channel channel = null;

            await RetryPolicy.ExecuteAsync(async () =>
            {
                _logger.LogInformation($"Calling Channel.GetAsync with {_debugAccessTokenCache} ({_portalsContext})");

                channel = await _service.Channel.GetAsync(_channelId).ConfigureAwait(false);
            }).ConfigureAwait(false);

            AggregatorManager aggregatorManager = new AggregatorManager(channel.Aggregations, channel.Sort, channel.SortFields);

            await aggregatorManager.CacheAggregatorsAsync(_cache).ConfigureAwait(false);

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

                SchemaSearchResult response = null;

                await RetryPolicy.ExecuteAsync(async () =>
                {
                    _logger.LogInformation($"Calling Schema.SearchAsync with {_debugAccessTokenCache} ({_portalsContext})");

                    response = await _service.Schema.SearchAsync(request).ConfigureAwait(false);
                }).ConfigureAwait(false);

                schemas.AddRange(response.Results);
                pageToken = response.PageToken;
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
            CustomerInfo response = null;

            await RetryPolicy.ExecuteAsync(async () =>
            {
                _logger.LogInformation($"Calling Info.GetInfoAsync() with {_debugAccessTokenCache} ({_portalsContext})");

                response = await _service.Info.GetInfoAsync().ConfigureAwait(false);
            }).ConfigureAwait(false);

            return response.OutputFormats;
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

                ICollection<SchemaDetail> schemaDetails = null;

                await RetryPolicy.ExecuteAsync(async () =>
                {
                    _logger.LogInformation($"Calling Schema.GetManyAsync with {_debugAccessTokenCache} ({_portalsContext})");

                    schemaDetails = await _service.Schema.GetManyAsync(schemaIds).ConfigureAwait(false);
                }).ConfigureAwait(false);

                result.AddRange(schemaDetails);
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
                ListItemSearchRequest request = new ListItemSearchRequest()
                {
                    SchemaIds = schemaIds,
                    ResolveBehaviors = new[] {
                        ListItemResolveBehavior.Content,
                        ListItemResolveBehavior.OuterDisplayValueName
                    },
                    Limit = 5000,
                    PageToken = pageToken
                };

                ListItemSearchResult response = null;

                await RetryPolicy.ExecuteAsync(async () =>
                {
                    _logger.LogInformation($"Calling ListItem.SearchAsync with {_debugAccessTokenCache} ({_portalsContext})");

                    response = await _service.ListItem.SearchAsync(request).ConfigureAwait(false);
                }).ConfigureAwait(false);

                listItems.AddRange(response.Results);
                pageToken = response.PageToken;

            } while (pageToken != null);

            return listItems;
        }

        public async Task<Channel> GetChannelAsync(string channelId)
        {
            Channel result = null;

            await RetryPolicy.ExecuteAsync(async () =>
            {
                _logger.LogInformation($"Calling Channel.GetAsync with {_debugAccessTokenCache} ({_portalsContext})");

                result = await _service.Channel.GetAsync(channelId).ConfigureAwait(false);
            }).ConfigureAwait(false);

            return result;
        }

        public async Task<ICollection<Channel>> GetAllChannelsAsync()
        {
            ICollection<Channel> result = null;

            await RetryPolicy.ExecuteAsync(async () =>
            {
                _logger.LogInformation($"Calling Channel.GetAllAsync with {_debugAccessTokenCache} ({_portalsContext})");

                result = await _service.Channel.GetAllAsync().ConfigureAwait(false);
            }).ConfigureAwait(false);

            return result;
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

            ContentSearchResult contentSearchResult = null;

            await RetryPolicy.ExecuteAsync(async () =>
            {
                _logger.LogInformation($"Calling Service.Content with {_debugAccessTokenCache} ({_portalsContext})");

                contentSearchResult = await _service.Content.SearchAsync(request).ConfigureAwait(false);
            }).ConfigureAwait(false);

            ICollection<ContentDetail> contentDetails;

            if (resolveMetadata)
            {
                contentDetails = await GetManyAsync(contentSearchResult.Results?.Select(result => result.Id), new ContentResolveBehavior[] {
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
                contentDetails = await GetManyAsync(contentSearchResult.Results?.Select(result => result.Id), new ContentResolveBehavior[] {
                    ContentResolveBehavior.OuterDisplayValueThumbnail,
                    ContentResolveBehavior.OuterDisplayValueList,
                    ContentResolveBehavior.OuterDisplayValueName,
                    ContentResolveBehavior.Content,
                    ContentResolveBehavior.Outputs,
                    ContentResolveBehavior.Permissions
                }).ConfigureAwait(false);
            }

            DEBUG($"SearchAsync() took {contentSearchResult.ElapsedMilliseconds} ms");

            return (contentSearchResult, contentDetails);
        }

        public async Task<ContentDetail> GetContentAsync(string id)
        {
            await CheckContentChannelAccessAsync(id).ConfigureAwait(false);

            ContentDetail contentDetail = null;

            await RetryPolicy.ExecuteAsync(async () =>
            {
                _logger.LogInformation($"Calling Content.GetAsync with {_debugAccessTokenCache} ({_portalsContext})");

                contentDetail = await _service.Content.GetAsync(id, new ContentResolveBehavior[] {
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
            }).ConfigureAwait(false);

            await HandleThumbnailReferenceFileTypeAsync(contentDetail).ConfigureAwait(false);

            return contentDetail;
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
            ICollection<OutputFormatDetail> result = null;

            await RetryPolicy.ExecuteAsync(async () =>
            {
                _logger.LogInformation($"OutputFormat.GetManyAsync with {_debugAccessTokenCache} ({_portalsContext})");

                result = await _service.OutputFormat.GetManyAsync(outputFormatIds).ConfigureAwait(false);
            }).ConfigureAwait(false);

            return result;
        }

        public async Task<ICollection<ContentDetail>> GetContentPermissionsAsync(ICollection<string> ids)
        {
            var contentSearchResult = await GetContentChannelAccessIdsAsync(ids).ConfigureAwait(false);

            var contentDetails = await GetManyAsync(contentSearchResult.Results?.Select(result => result.Id), new ContentResolveBehavior[] {
                ContentResolveBehavior.Permissions
            }).ConfigureAwait(false);

            DEBUG($"GetContentPermissionsAsync() took {contentSearchResult.ElapsedMilliseconds} ms");

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

            DEBUG($"GetContentOutputsAsync() took {contentSearchResult.ElapsedMilliseconds} ms");

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

            ContentSearchResult contentSearchResult = null;

            await RetryPolicy.ExecuteAsync(async () =>
            {
                _logger.LogInformation($"Calling Content.SearchAsync with {_debugAccessTokenCache} ({_portalsContext})");

                contentSearchResult = await _service.Content.SearchAsync(request).ConfigureAwait(false);
            }).ConfigureAwait(false);

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
                    throw new PictureparkNotFoundException();
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

            await RetryPolicy.ExecuteAsync(async () =>
            {
                _logger.LogInformation($"Creating Content.CreateAsync with {_debugAccessTokenCache} ({_portalsContext})");

                var contentsCreateRequest = new ContentCreateManyRequest()
                {
                    Items = contents
                };

                var result = await _service.Content.CreateManyAsync(contentsCreateRequest).ConfigureAwait(false);

                await _service.BusinessProcess.WaitForCompletionAsync(result.BusinessProcessId);
            }).ConfigureAwait(false);
        }

        public async Task UpdateContentsAsync(ICollection<ContentMetadataUpdateItem> contents)
        {
            if (contents == null || contents.Count == 0)
                return;

            await CheckContentChannelAccessAsync(contents.Select(content => content.Id).ToList(), skipNonAccessibleContents: false).ConfigureAwait(false);

            await RetryPolicy.ExecuteAsync(async () =>
            {
                _logger.LogInformation($"Creating Content.UpdateAsync with {_debugAccessTokenCache} ({_portalsContext})");

                var contentsUpdateRequest = new ContentMetadataUpdateManyRequest()
                {
                    Items = contents
                };

                var result = await _service.Content.UpdateMetadataManyAsync(contentsUpdateRequest).ConfigureAwait(false);

                await _service.BusinessProcess.WaitForCompletionAsync(result.BusinessProcessId);
            }).ConfigureAwait(false);
        }

        public async Task<StreamResponse> GetImageDownloadStreamAsync(string id, ThumbnailSize size)
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

            var streamResponse = await GetStreamResponseAsync(downloadUri, cancelRequestDelay: TimeSpan.FromSeconds(5));

            return streamResponse;
        }

        public async Task<StreamResponse> GetPlaybackDownloadStreamAsync(string id, string size)
        {
            var downloadUri = $"{_apiUrl}v1/Contents/downloads/{Uri.EscapeDataString(id)}/{Uri.EscapeDataString(size)}";

            var streamResponse = await GetStreamResponseAsync(downloadUri, cancelRequestDelay: TimeSpan.FromSeconds(5));

            return streamResponse;
        }

        public async Task<StreamResponse> GetDownloadStreamForOutputFormatIdAsync(string id, string outputFormatId)
        {
            id = await GetThumbnailReferenceIdAsync(id).ConfigureAwait(false);

            var downloadUri = $"{_apiUrl}v1/Contents/downloads/{Uri.EscapeDataString(id)}/{Uri.EscapeDataString(outputFormatId)}";

            var streamResponse = await GetStreamResponseAsync(downloadUri);

            return streamResponse;
        }

        public async Task<ICollection<string>> GetProfileUserRoleIdsAsync()
        {
            if (_pictureparkConnector.PassThroughAuthenticationTokenProvider == null)
            {
                return null;
            }

            var profile = await RetryPolicy.ExecuteAsync(async () =>
            {
                _logger.LogInformation($"Getting Profile.GetAsync with {_debugAccessTokenCache} ({_portalsContext})");

                try
                {
                    return await _service.Profile.GetAsync().ConfigureAwait(false);
                }
                catch (PictureparkForbiddenException)
                {
                    // happens... let's "transform" it :)

                    throw new UserNotFoundException();
                }
            });

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

        private async Task<StreamResponse> GetStreamResponseAsync(string downloadUri, TimeSpan cancelRequestDelay = default)
        {
            var accessToken = _pictureparkConnector.GetAccessToken();

            var requestHeaders = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Picturepark-CustomerAlias", _customerAlias)
            };

            var downloadStreamRequest = new DownloadStreamRequest(downloadUri, accessToken, requestHeaders, cancelRequestDelay);

            var streamResponse = await HttpClient.GetDownloadStreamAsync(_logger, _portalsContext, downloadStreamRequest);

            return streamResponse;
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

                    ICollection<ContentDetail> partialContentDetails = null;

                    await RetryPolicy.ExecuteAsync(async () =>
                    {
                        _logger.LogInformation($"Calling GetManyAsync with {_debugAccessTokenCache} ({_portalsContext})");

                        partialContentDetails = await _service.Content.GetManyAsync(partialContentIds, contentResolveBehaviors).ConfigureAwait(false);
                    }).ConfigureAwait(false);

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

                var targetContentDetail = await GetContentAsync(targetContentId).ConfigureAwait(false);

                result.Add(targetContentDetail);
            }

            return result;
        }

        private async Task HandleThumbnailReferenceFileTypeAsync(ContentDetail contentDetail)
        {
            await HandleThumbnailReferenceFileTypeAsync(new[] { contentDetail }).ConfigureAwait(false);
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

                var targetContentDetail = await GetContentAsync(targetContentId).ConfigureAwait(false);

                contentDetail.ContentType = targetContentDetail.ContentType;
            }
        }

        private async Task<string> GetThumbnailReferenceIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return id;
            }

            var contentDetail = await GetContentAsync(id).ConfigureAwait(false);

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

        protected void DEBUG(string message, params object[] args)
        {
            if (!(_logger is null))
            {
                _logger.LogDebug(message, args);
            }
        }
    }
}
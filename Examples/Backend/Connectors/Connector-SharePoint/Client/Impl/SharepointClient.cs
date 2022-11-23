using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Newtonsoft.Json.Linq;
using Polly;
using Polly.Retry;
using RestSharp;
using SmintIo.Portals.Connector.SharePoint.Client.Authenticators;
using SmintIo.Portals.Connector.SharePoint.Extensions;
using SmintIo.Portals.Connector.SharePoint.MicrosoftGraph;
using SmintIo.Portals.Connector.SharePoint.Models;
using SmintIo.Portals.ConnectorSDK.Models;
using SmintIo.Portals.SDK.Core.Cache;
using SmintIo.Portals.SDK.Core.Extensions;
using SmintIo.Portals.SDK.Core.Http.Prefab.Exceptions;
using SmintIo.Portals.SDK.Core.Http.Prefab.Extensions;
using SmintIo.Portals.SDK.Core.Http.Prefab.Models;
using SmintIo.Portals.SDK.Core.Models.Context;
using SmintIo.Portals.SDK.Core.Rest;
using SmintIo.Portals.SDK.Core.Rest.Prefab;
using SmintIo.Portals.SDK.Core.Rest.Prefab.Exceptions;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace SmintIo.Portals.Connector.SharePoint.Client.Impl
{
    public class SharepointClient : ConnectorSDK.Clients.Prefab.BaseClient, ISharepointClient, IResponseFailedHandler
    {
        private const string RootFolderId = "0";

        private static readonly string[] _driveItemFields = new[]
        {
            "id",
            "name",
            "createdBy",
            "parentReference",
            "audio",
            "video",
            "file",
            "fileSystemInfo",
            "folder",
            "image",
            "location",
            "package",
            "photo",
            "publication",
            "remoteItem",
            "root",
            "size",
            "speacialFolder",
            "webUrl"
        };

        private static readonly ICollection<string> _possibleDefaultShareDocumentLists = new HashSet<string>
        {
            "Shared Documents",
            "Freigegebene Dokumente",
            "Documentos Compartidos",
            "Documentos Compartilhados"
        };

        private readonly ILogger _logger;
        private readonly ICache _cache;
        private readonly IPortalsContextModel _portalsContext;
        private readonly Func<AuthorizationValuesModel> _getAuthorizationValuesFunc;
        private readonly string _sharepointUrl;
        private readonly IEnumerable<string> _siteFolderIds;

        private GraphServiceClient _graphServiceClient;
        private IRestSharpClient _restSharpClient;

        /// <summary>
        /// Constructs a new SharepointClient with a function to renew the OAuth2 Access Token.
        /// The <paramref name="getAuthorizationValuesFunc"/> function is called on every Graph API request to renew the token.
        /// </summary>
        /// <param name="logger">Used to perform general logging <see cref="ILogger"/></param>
        /// <param name="cache"><see cref="ICache"/></param>
        /// <param name="portalsContext">Used to perform general logging <see cref="IPortalsContextModel"/></param>
        /// <param name="getAuthorizationValuesFunc">A <see cref="Func{T}"/>that performs the refresh</param>
        /// <param name="sharepointUrl">the Url for the sharepoint site</param>
        /// <param name="siteId">the Site ID for the sharepoint site</param>
        public SharepointClient(
            ILogger logger,
            ICache cache,
            IPortalsContextModel portalsContext,
            IHttpClientFactory httpClientFactory,
            Func<AuthorizationValuesModel> getAuthorizationValuesFunc,
            string sharepointUrl,
            string siteId,
            IEnumerable<string> siteFolderIds)
            : base(SharepointConnectorStartup.SharepointConnector, portalsContext, httpClientFactory, logger)
        {
            _logger = logger;
            _cache = cache;
            _portalsContext = portalsContext;
            _getAuthorizationValuesFunc = getAuthorizationValuesFunc;
            _sharepointUrl = sharepointUrl;
            _siteFolderIds = siteFolderIds;

            SiteId = siteId;

            CreateGraphApiClient();
            CreateRestApiClient();
        }

        public string SiteId { get; }

        /// <summary>
        /// Creates GraphApi client with delayed bearer authentication via <see cref="_getAuthorizationValuesFunc"/>.
        /// </summary>
        private void CreateGraphApiClient()
        {
            var authenticationProvider = new DelegateAuthenticationProvider(request =>
            {
                var authorizationValues = _getAuthorizationValuesFunc();

                request.Headers.Authorization = new AuthenticationHeaderValue("bearer", authorizationValues.AccessToken);

                return Task.CompletedTask;
            });

            _graphServiceClient = new GraphServiceClient(authenticationProvider);
        }

        protected override async Task HandleDefaultRetryPolicyExceptionAsync(Exception ex, TimeSpan timespan, Context context, string requestId)
        {
            if (ex is ServiceException serviceException)
            {
                if (serviceException.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    // too many requests, backoff and try again

                    return;
                }

                if (serviceException.StatusCode == HttpStatusCode.Unauthorized)
                {
                    // Retry with a new client

                    CreateGraphApiClient();

                    return;
                }

                if (serviceException.StatusCode == HttpStatusCode.Gone)
                {
                    context[nameof(serviceException.StatusCode)] = serviceException.StatusCode;

                    return;
                }

                if (serviceException.StatusCode == HttpStatusCode.NotAcceptable)
                {
                    throw serviceException;
                }
            }
            else if (ex is HttpResponseException httpResponseException)
            {
                if (httpResponseException.StatusCode == (int)HttpStatusCode.NotAcceptable)
                {
                    // happens in some edge cases

                    throw httpResponseException;
                }
            }

            await base.HandleDefaultRetryPolicyExceptionAsync(ex, timespan, context, requestId).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates Rest Api client with delayed bearer authentication via <see cref="_getAuthorizationValuesFunc"/>.
        /// </summary>
        private void CreateRestApiClient()
        {
            if (string.IsNullOrEmpty(_sharepointUrl))
            {
                throw new ExternalDependencyException(
                    ExternalDependencyStatusEnum.NotSetup,
                    "The SharePoint Url is not setup yet");
            }

            _restSharpClient = new RestSharpClient(new Uri($"{_sharepointUrl}"));
            _restSharpClient.Client.Authenticator = new SharepointBearerAuthenticator(_getAuthorizationValuesFunc);
        }

        private AsyncRetryPolicy<IRestResponse> RestSharpRetryPolicy =>
            Policy
                .HandleResult<IRestResponse>((response) => response.StatusCode != HttpStatusCode.OK)
                .WaitAndRetryAsync(
                    DefaultMaxRetryAttempts,
                    retryCount => GetSleepDurationDuringRetry(retryCount),
                    (response, timespan, context) =>
                    {
                        if (response.Result.StatusCode == HttpStatusCode.TooManyRequests)
                        {
                            // too many requests, backoff and try again

                            return;
                        }

                        if (response.Result.StatusCode == HttpStatusCode.Forbidden)
                        {
                            // retry with a new client

                            CreateRestApiClient();

                            return;
                        }

                        if (response.Result.ErrorException is HttpResponseException httpResponseException)
                        {
                            ExternalDependencyException.HandleStatusCode(
                                statusCode: httpResponseException.StatusCode,
                                httpResponseException,
                                "SharePoint REST request",
                                SharepointConnectorStartup.SharepointConnector,
                                targetGetUuid: null,
                                _logger);

                            return;
                        }
                    });

        private Task<IRestResponse> ExecuteRestSharpRequestAsync(Func<Context, Task<IRestResponse>> apiFunc, string requestId = "", [CallerMemberName] string callerName = "")
        {
            return RestSharpRetryPolicy.ExecuteAsync(
               (context) =>
               {
                   if (_logger.IsEnabled(LogLevel.Information))
                   {
                       _logger.LogInformation($"Calling {callerName} with {_portalsContext} (with backoff)");
                   }

                   return apiFunc(context);
               },
               contextData: new Dictionary<string, object>() { { RequestIdKey, requestId } });
        }

        public async Task<ICollection<SiteResponse>> GetSitesAsync(string query = null)
        {
            var response = await ExecuteRestSharpRequestAsync(async (context) =>
            {
                var request = new RestRequest("_api/search/query?querytext='contentclass:sts_site'", Method.GET);

                SetResponseHeaderType(request);

                return await _restSharpClient.ExecuteTaskAsync<string>(request);
            }).ConfigureAwait(false);

            var host = new Uri(_sharepointUrl).Host;

            var siteResponses = JObject
                .Parse(response.Content)
                .SelectTokens("$.d.query.PrimaryQueryResult.RelevantResults.Table.Rows.results..Cells.results")
                .Select(t => new SiteResponse
                {
                    Host = host,
                    SiteId = GetTokenValueByProperty<string>(t, "Key", "SiteId"),
                    WebId = GetTokenValueByProperty<string>(t, "Key", "WebId"),
                    WebUrl = GetTokenValueByProperty<string>(t, "Key", "Path"),
                    DisplayName = GetTokenValueByProperty<string>(t, "Key", "Title")
                })
                .Where(sr =>
                    sr.WebUrl.ToLowerInvariant().StartsWith(_sharepointUrl.ToLowerInvariant())
                    && sr.WebUrl.ToLowerInvariant().Contains("/sites/")
                    && (string.IsNullOrEmpty(query) || sr.WebUrl.Contains(query, StringComparison.InvariantCultureIgnoreCase)))
                .ToList();

            return siteResponses;
        }

        public async Task<Site> GetSiteAsync(string siteId)
        {
            var sitesResult = await ExecuteAsync((context) =>
            {
                return _graphServiceClient
                    .Sites[siteId]
                    .Request()
                    .Expand(s => s.Lists)
                    .GetAsync();
            }).ConfigureAwait(false);

            return sitesResult;
        }

        public async Task<ICollection<ColumnDefinitionResponse>> GetSiteMetadataAsync(string siteId)
        {
            if (string.IsNullOrEmpty(SiteId))
            {
                return null;
            }

            if (string.IsNullOrEmpty(siteId) || !_siteFolderIds.Any())
            {
                return new List<ColumnDefinitionResponse>();
            }

            var site = await GetSiteAsync(siteId).ConfigureAwait(false);

            if (site == null)
            {
                throw new SharepointConnectorException($"The requested site was not found! SiteId: {siteId}");
            }

            var documentList = site.Lists.FirstOrDefault(l => _possibleDefaultShareDocumentLists.Contains(l.Name));

            if (documentList == null)
            {
                throw new SharepointConnectorException($"The requested site does match any of the default shared document lists! SiteId: {SiteId}");
            }

            var response = await ExecuteRestSharpRequestAsync(async (context) =>
            {
                var resource = $"/sites/{site.Name}/_api/lists(guid'{documentList.Id}')/fields";
                var request = new RestRequest(resource, Method.GET);

                SetResponseHeaderType(request);

                return await _restSharpClient.ExecuteTaskAsync<string>(request).ConfigureAwait(false);
            }).ConfigureAwait(false);

            var columnDefinitionResponses = JObject
                .Parse(response.Content)
                .SelectTokens("$.d.results..[*]") // $.d.results..[?(@.Hidden == 'false')] -- not working.
                .Select(jToken => new ColumnDefinitionResponse
                {
                    Id = GetTokenValue<string>(jToken, "Id"),
                    Name = GetTokenValue<string>(jToken, "EntityPropertyName"),
                    DisplayName = GetTokenValue<string>(jToken, "Title"),
                    IsHidden = GetTokenValue<bool>(jToken, "Hidden"),
                    FieldTypeKind = GetTokenValue<int>(jToken, "FieldTypeKind"),
                    TypeAsString = GetTokenValue<string>(jToken, "TypeAsString")
                })
                .Where(cd => !cd.IsHidden && !cd.Name.StartsWith("OData", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            var columnDefinitionNamesCacheKey = GetColumnDefinitionNamesCacheKey(siteId);
            var columnDefinitionNames = GetColumnDefinitionNames(columnDefinitionResponses);

            await _cache.StoreAsync(columnDefinitionNamesCacheKey, columnDefinitionNames, TimeSpan.FromDays(7)).ConfigureAwait(false);

            await SetCurrencyLocalesAsync(siteId, documentList.Id, columnDefinitionResponses).ConfigureAwait(false);

            return columnDefinitionResponses;
        }

        private async Task SetCurrencyLocalesAsync(string siteId, string documentListId, ColumnDefinitionResponse[] columnDefinitionResponses)
        {
            var currencyColumnDefinitionTasks = columnDefinitionResponses
                .Where(cdr => cdr.FieldType == SharepointFieldType.Currency)
                .Select(cdr => GetColumnDefinitionAsync(siteId, documentListId, cdr.Id))
                .ToArray();

            var currencyColumnDefinitions = await Task.WhenAll(currencyColumnDefinitionTasks).ConfigureAwait(false);

            foreach (var currencyColumnDefinition in currencyColumnDefinitions)
            {
                if (currencyColumnDefinition.Currency == null)
                {
                    continue;
                }

                var columnDefinitionResponse = columnDefinitionResponses.Single(cdr => cdr.Id == currencyColumnDefinition.Id);

                columnDefinitionResponse.CurrencyLocale = currencyColumnDefinition.Currency.Locale;
            }
        }

        private async Task<ColumnDefinition> GetColumnDefinitionAsync(string siteId, string documentListId, string columnId)
        {
            var columnDefinition = await ExecuteAsync((context) =>
            {
                return _graphServiceClient
                    .Sites[siteId]
                    .Lists[documentListId]
                    .Columns[columnId]
                    .Request()
                    .GetAsync();
            }).ConfigureAwait(false);

            return columnDefinition;
        }

        public async Task<DriveItemListModel> GetFolderDriveItemsAsync(string assetId, string skipToken, int? pageSize)
        {
            if (string.IsNullOrEmpty(SiteId) || !_siteFolderIds.Any())
            {
                return null;
            }

            // Sync selected asset
            if (!string.IsNullOrEmpty(assetId))
            {
                var folderDriveItemList = await GetChildrenDriveItemsAsync(assetId, skipToken, pageSize).ConfigureAwait(false);

                return folderDriveItemList;
            }

            // Sync root folder
            if (_siteFolderIds.Contains(RootFolderId))
            {
                var rootDriveItemList = await GetRootDriveItemListAsync(skipToken, pageSize).ConfigureAwait(false);

                return rootDriveItemList;
            }

            // Sync UI selected assets
            var assetIds = _siteFolderIds.ToList();

            var folderDriveItems = await GetDriveItemsBatchAsync(assetIds).ConfigureAwait(false);

            for (var i = folderDriveItems.Count - 1; i >= 0; i--)
            {
                var driveItem = folderDriveItems.ElementAt(i);

                var parentDriveItemAssetId = driveItem.GetParentAssetId();

                // We remove child sub folders, so we don't sync the content multiple times
                if (_siteFolderIds.Contains(parentDriveItemAssetId))
                {
                    folderDriveItems.Remove(driveItem);
                }
            }

            return new DriveItemListModel
            {
                DriveItems = folderDriveItems
            };
        }

        private async Task<bool> CanAccessDriveItemAsync(DriveItem driveItem)
        {
            var assetId = driveItem.GetAssetId();

            // Top Level - we can access everything
            if (_siteFolderIds.Contains(RootFolderId))
            {
                return true;
            }

            if (_siteFolderIds.Contains(assetId))
            {
                return true;
            }

            var parentDriveItemAssetId = driveItem.GetParentAssetId();

            while (true)
            {
                if (_siteFolderIds.Contains(parentDriveItemAssetId))
                {
                    return true;
                }

                // Going all they way to the root, in order to check whether the asset can be accessed

                var parentDriveItemModel = await GetDriveItemRelationshipAsync(parentDriveItemAssetId).ConfigureAwait(false);

                if (parentDriveItemModel == null)
                {
                    return false;
                }

                parentDriveItemAssetId = parentDriveItemModel.ParentAssetId;

                if (string.IsNullOrEmpty(parentDriveItemAssetId))
                {
                    // Root folder reached

                    return false;
                }
            }
        }

        private async Task<DriveItemRelationshipModel> GetDriveItemRelationshipAsync(string assetId)
        {
            var parentDriveItemModel = await _cache.GetAsync<DriveItemRelationshipModel>(assetId).ConfigureAwait(false);

            if (parentDriveItemModel == null)
            {
                var parentDriveItem = await GetDriveItemInternallyAsync(assetId).ConfigureAwait(false);

                if (parentDriveItem == null)
                {
                    return null;
                }

                var driveItemRelationshipModel = new DriveItemRelationshipModel
                {
                    ParentAssetId = parentDriveItem.GetParentAssetId()
                };

                await _cache.StoreAsync(assetId, driveItemRelationshipModel, expiresIn: TimeSpan.FromMinutes(5)).ConfigureAwait(false);

                parentDriveItemModel = driveItemRelationshipModel;
            }

            return parentDriveItemModel;
        }

        private async Task EnsureDriveItemsAccessAsync(ICollection<DriveItem> driveItems)
        {
            for (var i = driveItems.Count - 1; i >= 0; i--)
            {
                var driveItem = driveItems.ElementAt(i);

                var canAccess = await CanAccessDriveItemAsync(driveItem).ConfigureAwait(false);

                if (!canAccess)
                {
                    driveItems.Remove(driveItem);
                }
            }
        }

        private async Task<DriveItemListModel> GetChildrenDriveItemsAsync(string assetId, string skipToken, int? pageSize)
        {
            if (string.IsNullOrEmpty(SiteId) || !_siteFolderIds.Any())
            {
                return null;
            }

            var driveItem = await EnsureDriveItemAccessAsync(assetId).ConfigureAwait(false);

            if (driveItem == null)
            {
                return null;
            }

            if (!driveItem.IsFolder())
            {
                throw new Exception($"DriveItem is not a Folder!");
            }

            var driveItemListModel = await GetChildrenDriveItemsInternallyAsync(driveItem, skipToken, pageSize).ConfigureAwait(false);

            return driveItemListModel;
        }

        private async Task<DriveItemListModel> GetChildrenDriveItemsInternallyAsync(DriveItem driveItem, string skipToken, int? pageSize)
        {
            try
            {
                var childrenDriveItems = await ExecuteAsync((context) =>
                {
                    var driveItemChildrenCollectionRequest = _graphServiceClient
                        .Sites[SiteId]
                        .Drives[driveItem.ParentReference.DriveId]
                        .Items[driveItem.Id]
                        .Children
                        .Request()
                        .Expand(i => i.Thumbnails)
                        .Select(string.Join(",", _driveItemFields));

                    driveItemChildrenCollectionRequest.QueryOptions.SetSkipToken(skipToken);
                    driveItemChildrenCollectionRequest.QueryOptions.SetPageSize(pageSize);

                    return driveItemChildrenCollectionRequest.GetAsync();
                }).ConfigureAwait(false);

                var nextSkipToken = childrenDriveItems.NextPageRequest?.QueryOptions.GetNextSkipToken();

                return new DriveItemListModel
                {
                    DriveItems = childrenDriveItems,
                    ContinuationUuid = nextSkipToken
                };
            }
            catch (Exception)
            {
                _logger.LogError($"Error occured with drive ID {driveItem.ParentReference.DriveId} and item ID {driveItem.Id} and skip token {skipToken}");

                throw;
            }
        }

        public async Task<ICollection<DriveItem>> GetFoldersListAsync()
        {
            if (string.IsNullOrEmpty(SiteId))
            {
                return null;
            }

            var rootDriveItems = await GetRootDriveItemListAsync(skipToken: null, pageSize: null).ConfigureAwait(false);

            var folderDriveItems = await GetFoldersListInternallyAsync(rootDriveItems).ConfigureAwait(false);

            return folderDriveItems;
        }

        private async Task<ICollection<DriveItem>> GetFoldersListInternallyAsync(DriveItemListModel rootDriveItemList)
        {
            var folderDriveItems = new List<DriveItem>();

            if (rootDriveItemList == null || !rootDriveItemList.DriveItems.Any())
            {
                return folderDriveItems;
            }

            var rootFoldersDriveItems = rootDriveItemList.DriveItems
                .Where(di => di.IsFolder())
                .ToList();

            foreach (var rootFolderDriveItem in rootFoldersDriveItems)
            {
                folderDriveItems.Add(rootFolderDriveItem);

                await GetChildrenRecursivelyAsync(folderDriveItems, rootFolderDriveItem, rootFolderDriveItem.Name, foldersOnly: true).ConfigureAwait(false);
            }

            return folderDriveItems;
        }

        private async Task<ICollection<Drive>> GetDrivesAsync()
        {
            if (string.IsNullOrEmpty(SiteId))
            {
                return null;
            }

            var drives = await ExecuteAsync((context) =>
            {
                return _graphServiceClient
                    .Sites[SiteId]
                    .Drives
                    .Request()
                    .GetAsync();
            }).ConfigureAwait(false);

            return drives;
        }

        private async Task<DriveItemListModel> GetRootDriveItemListAsync(string skipToken, int? pageSize)
        {
            var drives = await GetDrivesAsync().ConfigureAwait(false);

            var drive = drives.FirstOrDefault();

            // One drive per site

            if (drive == null)
            {
                throw new InvalidOperationException();
            }

            var driveItems = await ExecuteAsync((context) =>
            {
                var driveItemChildrenCollectionRequest = _graphServiceClient
                    .Drives[drive.Id]
                    .Root
                    .Children
                    .Request()
                    .Expand(i => i.Thumbnails)
                    .Select(string.Join(",", _driveItemFields));

                driveItemChildrenCollectionRequest.QueryOptions.SetSkipToken(skipToken);
                driveItemChildrenCollectionRequest.QueryOptions.SetPageSize(pageSize);

                return driveItemChildrenCollectionRequest.GetAsync();
            }).ConfigureAwait(false);

            var nextSkipToken = driveItems.NextPageRequest?.QueryOptions.GetNextSkipToken();

            return new DriveItemListModel
            {
                DriveItems = driveItems,
                ContinuationUuid = nextSkipToken
            };
        }

        private async Task<DriveItem> GetChildrenRecursivelyAsync(ICollection<DriveItem> folderDriveItems, DriveItem driveItem, string parentFolderName, bool foldersOnly = false)
        {
            var driveItemList = await GetChildrenDriveItemsInternallyAsync(driveItem, skipToken: null, pageSize: null).ConfigureAwait(false);

            var childDriveItems = foldersOnly
                ? driveItemList.DriveItems
                    .Where(c => c.IsFolder())
                    .ToList()
                : driveItemList.DriveItems;

            foreach (var childDriveItem in childDriveItems)
            {
                if (!string.IsNullOrEmpty(parentFolderName))
                {
                    childDriveItem.Name = $"{parentFolderName} > {childDriveItem.Name}";
                }

                var child = await GetChildrenRecursivelyAsync(folderDriveItems, childDriveItem, childDriveItem.Name, foldersOnly).ConfigureAwait(false);

                folderDriveItems.Add(child);
            }

            return driveItem;
        }

        public async Task<DriveItem> GetFolderDriveItemAsync(string assetId)
        {
            if (string.IsNullOrEmpty(SiteId) || !_siteFolderIds.Any())
            {
                return null;
            }

            var driveItem = await GetDriveItemInternallyAsync(assetId).ConfigureAwait(false);

            if (driveItem == null)
            {
                return null;
            }

            if (!driveItem.IsFolder())
            {
                throw new Exception($"DriveItem is not a Folder!");
            }

            return driveItem;
        }

        public async Task<DriveItem> GetDriveItemAsync(string assetId)
        {
            if (string.IsNullOrEmpty(SiteId) || !_siteFolderIds.Any())
            {
                return null;
            }

            if (string.IsNullOrEmpty(assetId))
            {
                throw new ArgumentNullException(nameof(assetId));
            }

            var driveItem = await EnsureDriveItemAccessAsync(assetId).ConfigureAwait(false);

            return driveItem;
        }

        private async Task<DriveItem> EnsureDriveItemAccessAsync(string assetId)
        {
            var driveItem = await GetDriveItemInternallyAsync(assetId).ConfigureAwait(false);

            if (driveItem == null)
            {
                return null;
            }

            var canAccess = await CanAccessDriveItemAsync(driveItem).ConfigureAwait(false);

            if (!canAccess)
            {
                return null;
            }

            return driveItem;
        }

        private async Task<DriveItem> GetDriveItemInternallyAsync(string assetId)
        {
            (string driveId, string itemId) = assetId.Parse();

            try
            {
                var driveItem = await ExecuteAsync((context) =>
                {
                    return GetDriveItemRequest(assetId).GetAsync();
                }).ConfigureAwait(false);

                return driveItem;
            }
            catch (ServiceException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    // We will handle it gracefully because of the integration layer
                    return null;
                }

                throw ex;
            }
        }

        private IDriveItemRequest GetDriveItemRequest(string assetId)
        {
            (string driveId, string itemId) = assetId.Parse();

            var driveItemRequest = _graphServiceClient
                .Drives[driveId]
                .Items[itemId]
                .Request()
                .Expand(i => i.Thumbnails)
                .Select(string.Join(",", _driveItemFields));

            return driveItemRequest;
        }

        public async Task<IDictionary<string, object>> GetDriveItemAdditionalDataByFieldNamesAsync(DriveItem driveItem)
        {
            if (string.IsNullOrEmpty(SiteId) || !_siteFolderIds.Any())
            {
                return null;
            }

            if (driveItem == null)
            {
                throw new ArgumentNullException(nameof(driveItem));
            }

            var columnDefinitionNames = await GetCachedColumnDefinitionNamesAsync().ConfigureAwait(false);

            var selectValues = string.Join(", ", columnDefinitionNames);

            var listItemWithFields = await ExecuteAsync((context) =>
            {
                return _graphServiceClient
                    .Drives[driveItem.ParentReference.DriveId]
                    .Items[driveItem.Id]
                    .ListItem
                    .Fields
                    .Request()
                    .Select(selectValues)
                    .GetAsync();
            }).ConfigureAwait(false);

            return listItemWithFields.AdditionalData
                .ToDictionary(
                    kvp1 => kvp1.Key,
                    kvp1 => kvp1.Value);
        }

        private async Task<ICollection<string>> GetCachedColumnDefinitionNamesAsync()
        {
            var columnDefinitionNamesCacheKey = GetColumnDefinitionNamesCacheKey(SiteId);

            var columnDefinitionNames = await _cache.GetAsync<List<string>>(columnDefinitionNamesCacheKey).ConfigureAwait(false);

            if (columnDefinitionNames == null)
            {
                var columnDefinitionResponses = await GetSiteMetadataAsync(SiteId).ConfigureAwait(false);

                columnDefinitionNames = GetColumnDefinitionNames(columnDefinitionResponses);
            }

            return columnDefinitionNames;
        }

        private static List<string> GetColumnDefinitionNames(ICollection<ColumnDefinitionResponse> columnDefinitionResponses)
        {
            return columnDefinitionResponses
                .Select(cdr => cdr.Name)
                .ToList();
        }

        private static string GetColumnDefinitionNamesCacheKey(string value)
        {
            return $"columnDefinitions_{value}";
        }

        public async Task<ItemPreviewInfo> GetDriveItemPreviewInfoAsync(string assetId)
        {
            if (string.IsNullOrEmpty(SiteId) || !_siteFolderIds.Any())
            {
                return null;
            }

            var driveItem = await EnsureDriveItemAccessAsync(assetId).ConfigureAwait(false);

            if (driveItem == null)
            {
                return null;
            }

            var itemPreviewInfo = await ExecuteAsync((context) =>
            {
                var driveItemSearchRequest = _graphServiceClient
                    .Drives[driveItem.ParentReference.DriveId]
                    .Items[driveItem.Id]
                    .Preview()
                    .Request();

                return driveItemSearchRequest.PostAsync();
            }).ConfigureAwait(false);

            return itemPreviewInfo;
        }

        public async Task<StreamResponse> GetDriveItemContentAsync(string assetId)
        {
            if (string.IsNullOrEmpty(SiteId) || !_siteFolderIds.Any())
            {
                return null;
            }

            var driveItem = await EnsureDriveItemAccessAsync(assetId).ConfigureAwait(false);

            if (driveItem == null)
            {
                return null;
            }

            var size = driveItem.Size;

            if (size != null && size > int.MaxValue)
            {
                // this exceeds the max stream size

                _logger.LogWarning($"Found file with a file size larger than {int.MaxValue} bytes ({assetId}), skipping altogether (for now)");

                return null;
            }
            else if (size == null)
            {
                _logger.LogWarning($"Found drive item with NULL size ({assetId})");
            }

            var stream = await ExecuteAsync((context) =>
            {
                return _graphServiceClient
                   .Drives[driveItem.ParentReference.DriveId]
                   .Items[driveItem.Id]
                   .Content
                   .Request()
                   .GetAsync();
            }).ConfigureAwait(false);

            var streamResponse = new StreamResponse
            {
                FileName = driveItem.Name,
                FileSizeInBytes = stream.Length,
                MediaType = driveItem.File?.MimeType,
                Stream = stream
            };

            return streamResponse;
        }

        public async Task<StreamResponse> GetDriveItemThumbnailContentAsync(string assetId, string thumbnailSetId, string size)
        {
            if (string.IsNullOrEmpty(SiteId) || !_siteFolderIds.Any())
            {
                return null;
            }

            if (string.IsNullOrEmpty(thumbnailSetId) || string.IsNullOrEmpty(size))
            {
                return null;
            }

            var driveItem = await EnsureDriveItemAccessAsync(assetId).ConfigureAwait(false);

            if (driveItem == null)
            {
                return null;
            }

            GraphResponse responseMessage;

            try
            {
                responseMessage = await ExecuteAsync((context) =>
                {
                    return _graphServiceClient
                        .Drives[driveItem.ParentReference.DriveId]
                        .Items[driveItem.Id]
                        .Thumbnails[thumbnailSetId][size]
                        .Content
                        .Request()
                        .GetResponseAsync();
                }).ConfigureAwait(false);
            }
            catch (ServiceException se)
            {
                if (se.StatusCode == HttpStatusCode.NotAcceptable)
                {
                    // happens in edge cases

                    return null;
                }
                else
                {
                    throw;
                }
            }

            var streamResponse = new StreamResponse
            {
                FileName = responseMessage.Content?.Headers?.ContentDisposition?.FileName,
                FileSizeInBytes = responseMessage.Content?.Headers?.ContentLength,
                MediaType = responseMessage.Content?.Headers?.ContentType?.MediaType,
                Stream = await responseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false)
            };

            return streamResponse;
        }

        public async Task<StreamResponse> GetDriveItemPdfContentAsync(string assetId)
        {
            if (string.IsNullOrEmpty(SiteId) || !_siteFolderIds.Any())
            {
                return null;
            }

            var driveItem = await EnsureDriveItemAccessAsync(assetId).ConfigureAwait(false);

            if (driveItem == null)
            {
                return null;
            }

            // see https://docs.microsoft.com/en-us/graph/api/driveitem-get-content-format?view=graph-rest-1.0&tabs=csharp

            var queryOptions = new List<QueryOption>()
            {
                new QueryOption("format", "pdf")
            };

            try
            {
                var responseMessage = await ExecuteAsync((context) =>
                {
                    return _graphServiceClient
                        .Drives[driveItem.ParentReference.DriveId]
                        .Items[driveItem.Id]
                        .Content
                        .Request(queryOptions)
                        .GetResponseAsync();
                }).ConfigureAwait(false);

                var streamResponse = new StreamResponse
                {
                    FileName = responseMessage.Content?.Headers?.ContentDisposition?.FileName,
                    FileSizeInBytes = responseMessage.Content?.Headers?.ContentLength,
                    MediaType = responseMessage.Content?.Headers?.ContentType?.MediaType,
                    Stream = await responseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false)
                };

                return streamResponse;
            }
            catch (ServiceException se)
            {
                if (se.Error != null)
                {
                    if (string.Equals(se.Error.Code, "notSupported"))
                        return null;

                    if (string.Equals(se.Error.Code, "itemNotFound"))
                        return null;
                }

                throw;
            }
        }

        public async Task<StreamResponse> GetHttpStreamResponseWithSharepointErrorHandlingAsync(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return null;
            }

            var downloadStreamRequest = new DownloadStreamRequest(url, accessToken: null, requestHeaders: null, cancelRequestDelay: default);

            var streamResponse = await ExecuteHttpRequestAsync((context, httpClient) => httpClient.GetDownloadStreamAsync(Logger, PortalsContextModel, downloadStreamRequest, this), requestId: null);

            return streamResponse;
        }

        public Task<RequestFailedHandlerResult> HandleRequestFailedAsync(HttpResponseMessage responseMessage)
        {
            if (responseMessage == null ||
                responseMessage.Headers == null ||
                !responseMessage.Headers.TryGetValues("x-errorcode", out var values) ||
                values == null ||
                !values.Any())
            {
                return Task.FromResult(RequestFailedHandlerResult.Throw);
            }

            var errorCode = values.First();

            if (string.IsNullOrEmpty(errorCode))
            {
                return Task.FromResult(RequestFailedHandlerResult.Throw);
            }

            if (string.Equals(errorCode, "VideoProcessing_ByteStreamTypeNotSupported") ||
                string.Equals(errorCode, "SubStreamCached_GeneralFailure"))
            {
                // permanent error

                return Task.FromResult(RequestFailedHandlerResult.Ignore);
            }

            return Task.FromResult(RequestFailedHandlerResult.Throw);
        }

        public Task<RequestFailedHandlerResult> HandleMaxFileSizeExceededAsync(long? actualFileSize)
        {
            return Task.FromResult(RequestFailedHandlerResult.Throw);
        }

        public RequestFailedHandlerResult HandleJsonResponse(string content)
        {
            return RequestFailedHandlerResult.Throw;
        }

        public async Task<DriveItemChangesListModel> GetDriveItemChangesListAsync(string deltaLink)
        {
            if (string.IsNullOrEmpty(SiteId) || !_siteFolderIds.Any())
            {
                return null;
            }

            var driveItemsChangesList = await ExecuteAsync(
                async (context) =>
                {
                    var driveItemChangesListModel = new DriveItemChangesListModel();

                    if (context.TryGetValue(nameof(ServiceException.StatusCode), out var statusCode) && (int)statusCode == (int)HttpStatusCode.Gone)
                    {
                        // If a deltaLink is no longer valid, the service will respond with 410 Gone.
                        driveItemChangesListModel.ContinuationTooOld = true;

                        return driveItemChangesListModel;
                    }

                    var driveItemDeltaRequest = GetDriveItemDeltaRequest(deltaLink);

                    var driveItemDeltaCollectionPage = await driveItemDeltaRequest.GetAsync().ConfigureAwait(false);

                    // We don't track the root folder.

                    var driveItems = driveItemDeltaCollectionPage
                        .Where(driveItem => !string.Equals(driveItem.Name, "root"))
                        .ToList();

                    driveItemChangesListModel.DriveItems = driveItems;

                    // as MS does: https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/blob/6ec55dcc43b884cec3d61aeb3ceb69fe27852157/src/Microsoft.Graph.Core/Tasks/PageIterator.cs

                    if (driveItemDeltaCollectionPage.NextPageRequest != null)
                    {
                        driveItemChangesListModel.ContinuationUuid = driveItemDeltaCollectionPage.NextPageRequest.GetHttpRequestMessage().RequestUri.AbsoluteUri;
                    }
                    else if (driveItemDeltaCollectionPage.AdditionalData != null)
                    {
                        if (driveItemDeltaCollectionPage.AdditionalData.TryGetValue(Constants.OdataInstanceAnnotations.NextLink, out object nextLink))
                        {
                            driveItemChangesListModel.ContinuationUuid = nextLink?.ToString();
                        }
                        else if (driveItemDeltaCollectionPage.AdditionalData.TryGetValue(Constants.OdataInstanceAnnotations.DeltaLink, out object deltaLink))
                        {
                            driveItemChangesListModel.ContinuationUuid = deltaLink?.ToString();
                        }
                    }

                    return driveItemChangesListModel;
                }).ConfigureAwait(false);

            for (var i = driveItemsChangesList.DriveItems.Count - 1; i >= 0; i--)
            {
                var driveItem = driveItemsChangesList.DriveItems.ElementAt(i);

                if (driveItem.Deleted != null)
                {
                    continue;
                }

                var canAccess = await CanAccessDriveItemAsync(driveItem).ConfigureAwait(false);

                if (canAccess)
                {
                    continue;
                }

                // We are deleting assets that we no longer have can access to

                if (driveItem.IsFolder())
                {
                    // We will not receive notifications for the individual assets, so we will delete them recursively

                    driveItemsChangesList.DriveItems.Remove(driveItem);
                    driveItemsChangesList.FolderDriveItemsToDelete.Add(driveItem);
                }
                else
                {
                    // Single asset being moved and we cannot access it

                    driveItem.Deleted = new Deleted();
                }
            }

            return driveItemsChangesList;
        }

        private IDriveItemDeltaRequest GetDriveItemDeltaRequest(string deltaLink)
        {
            IDriveItemDeltaRequest driveItemDeltaRequest;

            if (!string.IsNullOrEmpty(deltaLink))
            {
                var driveItemDeltaCollectionPage = new DriveItemDeltaCollectionPage();

                driveItemDeltaCollectionPage.InitializeNextPageRequest(_graphServiceClient, deltaLink);

                driveItemDeltaRequest = driveItemDeltaCollectionPage.NextPageRequest;
            }
            else
            {
                var queryOptions = new List<QueryOption>()
                {
                    new QueryOption("token", "latest")
                };

                driveItemDeltaRequest = _graphServiceClient
                    .Sites[SiteId]
                    .Drive
                    .Root
                    .Delta()
                    .Request(queryOptions);
            }

            if (!driveItemDeltaRequest.QueryOptions.Any(queryOption => string.Equals(queryOption.Name, "$top")))
            {
                driveItemDeltaRequest.QueryOptions.SetPageSize(50);
            }

            return driveItemDeltaRequest;
        }

        public async Task<ICollection<DriveItem>> GetDriveItemsBatchAsync(List<string> assetIds)
        {
            if (string.IsNullOrEmpty(SiteId) || !_siteFolderIds.Any())
            {
                return null;
            }

            if (assetIds == null || !assetIds.Any())
            {
                return new List<DriveItem>();
            }

            var driveItems = new List<DriveItem>();

            var batchResponsesByAsset = await GetDriveItemsBatchResponsesAsync(assetIds).ConfigureAwait(false);

            foreach (var assetId in assetIds)
            {
                if (!batchResponsesByAsset.TryGetValue(assetId, out var message))
                {
                    continue;
                }

                var responseContent = await message.Content.ReadAsStreamAsync().ConfigureAwait(false);

                var driveItem = JsonSerializer.Deserialize<DriveItem>(responseContent);

                if (driveItem == null)
                {
                    continue;
                }

                driveItems.Add(driveItem);
            }

            await EnsureDriveItemsAccessAsync(driveItems).ConfigureAwait(false);

            return driveItems;
        }

        private async Task<IDictionary<string, HttpResponseMessage>> GetDriveItemsBatchResponsesAsync(List<string> assetIds)
        {
            var batchResponsesTasks = assetIds
                .Chunk(size: 20)
                .Select(assetIdsChunk =>
                {
                    var batchRequestSteps = assetIdsChunk
                        .Select(assetId =>
                        {
                            var driveItemRequest = GetDriveItemRequest(assetId);

                            return new BatchRequestStep(assetId, driveItemRequest.GetHttpRequestMessage());
                        })
                        .ToArray();

                    var batchRequestContent = new BatchRequestContent(batchRequestSteps);
                    var batchResponseContent = BatchAsync(batchRequestContent);

                    return batchResponseContent;
                })
                .ToList();

            var batchResponses = await Task.WhenAll(batchResponsesTasks).ConfigureAwait(false);

            var responses = await Task.WhenAll(batchResponses.Select(br => br.GetResponsesAsync()).ToArray()).ConfigureAwait(false);

            return responses
                .SelectMany(r => r)
                .ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        public async Task<BatchResponseContent> BatchAsync(BatchRequestContent batchRequestContent)
        {
            if (string.IsNullOrEmpty(SiteId) || !_siteFolderIds.Any())
            {
                return null;
            }

            if (batchRequestContent == null)
            {
                return null;
            }

            var batchResponseContent = await ExecuteAsync((context) =>
            {
                return _graphServiceClient
                    .Batch
                    .Request()
                    .PostAsync(batchRequestContent);
            }).ConfigureAwait(false);

            return batchResponseContent;
        }

        private static void SetResponseHeaderType(RestRequest request)
        {
            request.AddHeader("Accept", "application/json;odata=verbose");
        }

        private static T GetTokenValue<T>(JToken jToken, string property)
        {
            var rawValue = jToken.SelectToken(property);

            if (Equals(rawValue, default(T)))
            {
                return default;
            }

            return rawValue.Value<T>();
        }

        private static T GetTokenValueByProperty<T>(JToken jToken, string property, string propertyValue)
        {
            var rawValue = jToken.SelectToken($"$[?(@.{property} == '{propertyValue}')].Value");

            if (Equals(rawValue, default(T)))
            {
                return default;
            }

            return rawValue.Value<T>();
        }
    }
}
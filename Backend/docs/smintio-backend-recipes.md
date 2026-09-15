Smint.io Portals backend component recipes
==========================================

Current version of this document is: 1.1.0 (as of 15th of September, 2026)

Task-shaped answers to "how do I …?", each one complete enough to paste into a component and
adapt. The other backend documents describe *what exists*; this one shows *how it is used*.

Every recipe names the SDK types it uses, shows the whole of the interesting part, and ends with
the mistakes that recipe invites. Where a working version of the same thing exists in this
repository, the recipe links to it — reading the recipe and then the example is the fastest way
in.

1. [How do I validate credentials during setup?](#user-content-how-do-i-validate-credentials-during-setup) — basic
1. [How do I fill a configuration dropdown from the external system?](#user-content-how-do-i-fill-a-configuration-dropdown-from-the-external-system) — basic
1. [How do I make one dropdown depend on another?](#user-content-how-do-i-make-one-dropdown-depend-on-another) — basic
1. [How do I turn the external system's errors into something usable?](#user-content-how-do-i-turn-the-external-systems-errors-into-something-usable) — basic
1. [How do I survive rate limiting?](#user-content-how-do-i-survive-rate-limiting) — basic
1. [How do I cache an expensive lookup?](#user-content-how-do-i-cache-an-expensive-lookup) — basic
1. [How do I report that an asset does not exist?](#user-content-how-do-i-report-that-an-asset-does-not-exist) — basic
1. [How do I implement a paged search?](#user-content-how-do-i-implement-a-paged-search) — advanced
1. [How do I page a source that has no random access?](#user-content-how-do-i-page-a-source-that-has-no-random-access) — advanced
1. [How do I feed the Smint.io integration layer?](#user-content-how-do-i-feed-the-smintio-integration-layer) — advanced
1. [How do I keep state the external system cannot hold?](#user-content-how-do-i-keep-state-the-external-system-cannot-hold) — advanced
1. [How do I resolve a meta-model entity key at runtime?](#user-content-how-do-i-resolve-a-meta-model-entity-key-at-runtime) — advanced
1. [How do I publish my own public API interface?](#user-content-how-do-i-publish-my-own-public-api-interface) — advanced
1. [How do I declare a custom permission?](#user-content-how-do-i-declare-a-custom-permission) — advanced
1. [How do I verify a signed link?](#user-content-how-do-i-verify-a-signed-link) — advanced
1. [How do I add a second data adapter to an existing connector?](#user-content-how-do-i-add-a-second-data-adapter-to-an-existing-connector) — basic
1. [How do I write a data processor that renames a download?](#user-content-how-do-i-write-a-data-processor-that-renames-a-download) — basic
1. [How do I change stored state without breaking a rollout?](#user-content-how-do-i-change-stored-state-without-breaking-a-rollout) — advanced

## How do I validate credentials during setup?

*basic.* `PerformPostConfigurationChecksAsync` is the one moment where a wrong credential reaches
the person who can fix it. Call something cheap against the external system, and turn a failure
into an `ExternalDependencyException` with a reason that says what went wrong.

```C#
public async Task PerformPostConfigurationChecksAsync(AuthorizationValuesModel authorizationValuesModel)
{
    if (string.IsNullOrEmpty(_configuration.ServiceUrl))
    {
        throw new ExternalDependencyException(
            ExternalDependencyStatusEnum.EndpointUrlInvalid,
            "The service URL is missing",
            "my-service");
    }

    try
    {
        // Something cheap that proves the credentials work — "who am I", a tenant read,
        // a one-item search. Never a full listing.
        var me = await _client.GetCurrentUserAsync().ConfigureAwait(false);

        if (me == null)
        {
            throw new ExternalDependencyException(
                ExternalDependencyStatusEnum.EndpointTestFailed,
                "The service did not identify the configured user",
                "my-service");
        }

        // Anything the rest of the flow needs, discovered here, goes onto the model.
        authorizationValuesModel.KeyValueStore["apiUrl"] = me.TenantApiUrl;
    }
    catch (ExternalDependencyException)
    {
        throw;
    }
    catch (Exception e)
    {
        throw new ExternalDependencyException(
            ExternalDependencyStatusEnum.EndpointTestFailed,
            "The service could not be reached with the configured credentials",
            "my-service",
            e);
    }
}
```

Pick the reason that matches, because the administrator sees a different message for each:

| Reason | Means |
|---|---|
| `EndpointUrlInvalid` | the URL is missing or malformed |
| `EndpointDataInvalid` | the URL is right, what came back was not what we expect |
| `EndpointTestFailed` | the test call did not succeed |
| `Unauthorized`, `Forbidden` | the credentials were rejected, or they lack the rights |
| `ScopeNotSufficient` | the token is valid but was not granted what the connector needs |
| `NotSetup` | the connector has not been configured yet |
| `CannotReadMetamodel` | the schema could not be read |
| `CompatiblityIssue` | the external system's version is not supported |

Pitfalls:

- **Do not rethrow your own exception as a generic one.** Catch `ExternalDependencyException`
  first and let it through, as above, or the reason you chose is lost.
- **Never put the credential into the message.** The message is shown to the administrator and
  written to logs.
- **Values you write onto the model are not revalidated.** Whatever you put into
  `KeyValueStore` is used as-is from then on.

A working example: `Connector-HelloWorld/HelloWorldConnector.cs`, and
`Connector-Picturepark/PictureparkConnector.cs` for a connector that discovers its API URL here.

## How do I fill a configuration dropdown from the external system?

*basic.* Never ask an administrator to paste an identifier they have to look up. A dynamic
allowed values provider turns the field into a dropdown the component fills.

```C#
using Microsoft.Extensions.DependencyInjection;
using SmintIo.Portals.SDK.Core.Models.Paging;
using SmintIo.Portals.SDK.Core.Models.Strings;
using SmintIo.Portals.SDK.Core.Providers;
using SmintIo.Portals.SDK.Core.Rest.Prefab.Exceptions;

public class ChannelAllowedValuesProvider : IDynamicValueListProvider<string>
{
    private readonly IMyClient _client;

    public bool SupportsSearch => false;
    public bool SupportsPagination => false;

    public IDynamicAllowedValuesParametersProvider ParametersProvider => null;

    public ChannelAllowedValuesProvider(IServiceProvider serviceProvider)
    {
        // The provider runs while the administrator is still filling in the form, so the
        // connector may not be authorized yet. That is a normal state, not an error.
        try
        {
            _client = serviceProvider.GetService<IMyClient>();
        }
        catch (Exception e)
        when (e.InnerException is ExternalDependencyException || e.InnerException is ArgumentNullException)
        {
            _client = null;
        }
    }

    /// Resolves one already-selected value back into something displayable.
    public async Task<UiDetailsModel<string>> GetDynamicValueAsync(string channelId)
    {
        if (_client == null)
        {
            return new UiDetailsModel<string> { Value = channelId };
        }

        var channel = await _client.GetChannelAsync(channelId).ConfigureAwait(false);

        if (channel == null)
        {
            return null;
        }

        return new UiDetailsModel<string>
        {
            Value = channel.Id,
            Name = new LocalizedStringsModel { { LocalizedStringsModel.DefaultCulture, channel.Name } }
        };
    }

    /// Fills the dropdown.
    public async Task<PagingResult<UiDetailsModel<string>>> GetDynamicValueListAsync(
        string searchTerm, int? offset, int? limit, string parentValue)
    {
        if (_client == null)
        {
            return new PagingResult<UiDetailsModel<string>> { Result = Array.Empty<UiDetailsModel<string>>() };
        }

        var channels = await _client.GetChannelsAsync().ConfigureAwait(false);

        return new PagingResult<UiDetailsModel<string>>
        {
            TotalResults = channels.Count,
            Result = channels
                .Select(channel => new UiDetailsModel<string>
                {
                    Value = channel.Id,
                    Name = new LocalizedStringsModel { { LocalizedStringsModel.DefaultCulture, channel.Name } }
                })
                .ToArray()
        };
    }
}
```

Point the property at it:

```C#
[DisplayName("en", "Channel", IsDefault = true)]
[DynamicAllowedValuesProvider(typeof(ChannelAllowedValuesProvider))]
public string ChannelId { get; set; }
```

Pitfalls:

- **Handle "not authorized yet".** The provider is constructed while the form is open; throwing
  there means the administrator cannot finish the configuration at all.
- **`GetDynamicValueAsync` is performance critical** — it runs for every already-selected value
  every time the form opens. [Cache](#user-content-how-do-i-cache-an-expensive-lookup) it.
- **Return the key, show the name.** `Value` is what gets persisted; changing what you put there
  orphans every configuration that selected it.
- **Say what you support.** With `SupportsSearch => false`, `searchTerm` is always `null`; do not
  write code that depends on it.

A working example: `Connector-Picturepark/AllowedValues/ChannelAllowedValuesProvider.cs`, and
the SharePoint connector's `AllowedValues/` folder for providers that support search and paging.

## How do I make one dropdown depend on another?

*basic.* A site has lists, a list has folders. There are two different situations here, and they
are solved differently.

**The parent is another configuration property** — site, then the lists *of that site*. The
provider reads the parent from the client it was given, because the client is constructed from
the configuration the administrator is filling in:

```C#
[DisplayName("en", "Site", IsDefault = true)]
[DynamicAllowedValuesProvider(typeof(SiteIdProvider))]
public string SiteId { get; set; }

[DisplayName("en", "List", IsDefault = true)]
[DynamicAllowedValuesProvider(typeof(ListIdProvider))]
public string ListId { get; set; }
```

```C#
public class ListIdProvider : IDynamicValueListProvider<string>
{
    private readonly IMyClient _client;

    public bool SupportsSearch => true;
    public bool SupportsPagination => true;

    public IDynamicAllowedValuesParametersProvider ParametersProvider => null;

    public async Task<PagingResult<UiDetailsModel<string>>> GetDynamicValueListAsync(
        string searchTerm, int? offset, int? limit, string parentValue)
    {
        // No site chosen yet — offer nothing rather than everything.
        if (_client == null || string.IsNullOrEmpty(_client.SiteId))
        {
            return new PagingResult<UiDetailsModel<string>> { Result = Array.Empty<UiDetailsModel<string>>() };
        }

        var lists = await _client
            .GetListsAsync(siteId: _client.SiteId, search: searchTerm, offset: offset ?? 0, limit: limit ?? 50)
            .ConfigureAwait(false);

        return new PagingResult<UiDetailsModel<string>>
        {
            TotalResults = lists.TotalCount,
            Result = lists.Items.Select(ToUiDetails).ToArray()
        };
    }
}
```

**The parent is a value in the same list** — a folder inside a folder. That is what `parentValue`
is for: the administrator opens a node and the platform asks you for its children.

```C#
public async Task<PagingResult<UiDetailsModel<string>>> GetDynamicValueListAsync(
    string searchTerm, int? offset, int? limit, string parentValue)
{
    // parentValue is null at the root, and the opened folder's id below it.
    var folders = await _client
        .GetFoldersAsync(parentFolderId: parentValue, search: searchTerm)
        .ConfigureAwait(false);

    return new PagingResult<UiDetailsModel<string>>
    {
        TotalResults = folders.TotalCount,
        Result = folders.Items.Select(ToUiDetails).ToArray()
    };
}
```

Pitfalls:

- **An empty parent means "nothing yet", not "everything".** Returning the full list lets the
  administrator pick a list that does not belong to the site they chose, and the failure shows up
  much later.
- **Say what you support.** With `SupportsSearch => false` the `searchTerm` is always `null`, and
  with `SupportsPagination => false` so are `offset` and `limit` — a provider that ignores its own
  flags returns the first page forever.

A working example: the SharePoint connector's `AllowedValues/` folder, which chains site → drive →
list → folder this way.

## How do I turn the external system's errors into something usable?

*basic.* Without a request failed handler, a rate-limit response and a wrong password look the
same to everyone downstream. The handler is what tells the platform which of the two happened.

```C#
using System.Net;
using SmintIo.Portals.SDK.Core.Http.Prefab.Exceptions;
using SmintIo.Portals.SDK.Core.Http.Prefab.Extensions;
using SmintIo.Portals.SDK.Core.Http.Prefab.RequestFailedHandlers;

public class MyServiceRequestFailedHandler : DefaultRequestFailedHandler
{
    public override Task<RequestFailedHandlerResult> HandleHttpStatusExceptionAsync(
        HttpStatusException httpStatusException)
    {
        switch (httpStatusException.StatusCode)
        {
            case 429:                                   // rate limited — back off and try again
                return Task.FromResult(RequestFailedHandlerResult.Retry);

            case (int)HttpStatusCode.Processing:        // still working on it
                return Task.FromResult(RequestFailedHandlerResult.Retry);

            case (int)HttpStatusCode.NotFound:          // a missing object is not an error here
                return Task.FromResult(RequestFailedHandlerResult.Ignore);

            case (int)HttpStatusCode.BadRequest:
                // Some systems report a transient condition as a 400 with a body that says so.
                var body = httpStatusException.RestResponse?.Content;

                if (!string.IsNullOrEmpty(body) && body.Contains("temporarily_unavailable"))
                {
                    return Task.FromResult(RequestFailedHandlerResult.Retry);
                }

                break;
        }

        return base.HandleHttpStatusExceptionAsync(httpStatusException);
    }
}
```

| Result | The platform then |
|---|---|
| `Retry` | backs off and repeats the call |
| `Ignore` | treats the call as having produced nothing, without failing |
| `Default` | applies the standard handling for that status |

The other hooks are `HandleOtherExceptionAsync(requestUri, tryCount, portalsContextModel, exception)`
— useful for a transport-level failure, and the place to stop retrying after *n* tries —
`HandleHttpRequestTimeoutAsync()`, `HandleMaxFileSizeExceededAsync(actualFileSize)` and
`HandleFileExpectedButJsonReceivedAsync(responseBody)`, the last of which catches the common case
of an expired session returning an HTML or JSON login page where a file was expected.

Pitfall: **do not retry a non-idempotent call.** A `Retry` on something that creates or uploads
duplicates it.

A working example: `Connector-HelloWorld/Client/Impl/HelloWorldDefaultRequestFailedHandler.cs`.

## How do I survive rate limiting?

*basic.* Every call goes through the base client's execute helpers, which apply the backoff and
hand failures to your request failed handler. Getting this wrong is the most common cause of a
connector that works in testing and collapses under a real portal.

```C#
public async Task<Asset> GetAssetAsync(string assetId)
{
    var request = new RestRequest($"/assets/{assetId}", Method.Get);

    return await ExecuteRestSharpRequestWithBackoffAsync<Asset>(
        request,
        _requestFailedHandler,
        hint: "get asset",
        isGet: true).ConfigureAwait(false);
}

public async Task<CreateResult> CreateAssetAsync(CreateAssetRequest body)
{
    var request = new RestRequest("/assets", Method.Post).AddJsonBody(body);

    // A retry would create a second asset — no backoff.
    return await ExecuteRestSharpRequestWithoutBackoffAsync<CreateResult>(
        request,
        _requestFailedHandler,
        hint: "create asset").ConfigureAwait(false);
}
```

- **`…WithBackoffAsync` for reads and anything idempotent**, `…WithoutBackoffAsync` for anything
  a repeat would duplicate.
- **Always pass a `hint`.** It is what a support engineer sees in the log line; "get asset" costs
  nothing and saves an hour.
- The same pair exists on `BaseHttpClientApiClient` and `BaseDynamicApiClient<T>` for a plain
  `HttpClient` and for a vendor SDK respectively.

## How do I cache an expensive lookup?

*basic.* `ICache` is injected into a connector and into a client. Use it for anything stable that
you would otherwise fetch on every request — a channel list, a schema, a display name.

```C#
private readonly ICache _cache;

public async Task<Channel> GetChannelAsync(string channelId)
{
    var cacheKey = $"channel-{channelId}";

    var cached = await _cache.GetAsync<Channel>(cacheKey).ConfigureAwait(false);

    if (cached != null)
    {
        return cached;
    }

    var channel = await FetchChannelAsync(channelId).ConfigureAwait(false);

    if (channel != null)
    {
        await _cache.StoreAsync(cacheKey, channel, expiresIn: TimeSpan.FromMinutes(30)).ConfigureAwait(false);
    }

    return channel;
}
```

`WarmCachesAsync(bool forceWarming)` on the connector is called by the platform to fill the
cache before it is needed. Pre-fetch what is expensive *and* stable there; do not use it to load
content.

Pitfalls:

- **The cached type must be a `class` with a parameterless constructor.** Cache your own model,
  not a vendor SDK type that happens not to deserialize.
- **Include everything that varies in the key** — the channel, the tenant, the language. A key
  that is too coarse serves one administrator's data to another's portal.
- **Give everything an expiry.** A cache entry with no expiry is a bug with a long fuse, and it
  survives a deployment: see
  [changing stored state](#user-content-how-do-i-change-stored-state-without-breaking-a-rollout).

## How do I report that an asset does not exist?

*basic.* Not with `null`, and not with an empty result — both read as "the source has nothing to
say", and the portal cannot tell the visitor anything useful.

```C#
public override async Task<GetAssetResult> GetAssetAsync(GetAssetParameters parameters)
{
    var asset = await _client.GetAssetAsync(parameters.AssetId.Id).ConfigureAwait(false);

    if (asset == null)
    {
        throw new ExternalDependencyException(
            ExternalDependencyStatusEnum.AssetNotFound,
            $"The asset was not found",
            parameters.AssetId.Id);
    }

    return new GetAssetResult { AssetDataObject = _converter.Convert(asset) };
}
```

Use `AssetNotFound` for a missing asset, `GetNotFound` for anything else that was looked up and
is not there. The identifier argument is what lands in the log — pass the *unscoped* id you
actually looked up, never the whole request.

Pitfall: **a method you do not support throws `NotImplementedException`** and is not declared in
your feature support. That is a different thing from a missing object, and the platform treats
them differently.

## How do I implement a paged search?

*advanced.* A search takes a page and a page size, and reports the paging state back in
`Details`. Page numbers start at 0.

```C#
public async Task<SearchAssetsResult> SearchAssetsAsync(SearchAssetsParameters parameters)
{
    var page = parameters.Page ?? 0;
    var pageSize = parameters.PageSize ?? 30;

    var response = await _client.SearchAsync(
        query: parameters.QueryString,
        offset: page * pageSize,
        limit: pageSize,
        filters: BuildFilters(parameters.CurrentFilters),
        folderIds: parameters.ParentFolderIds?.Select(folderId => folderId.Id).ToArray())
        .ConfigureAwait(false);

    var assetDataObjects = response.Items
        .Select(item => _converter.Convert(item))
        .ToArray();

    return new SearchAssetsResult
    {
        AssetDataObjects = assetDataObjects,
        Details = new AssetSearchDetailsModel
        {
            CurrentPage = page,
            CurrentItemsPerPage = pageSize,
            TotalResults = response.TotalCount,
            MaxPages = (int)Math.Ceiling(response.TotalCount / (double)pageSize),
            HasMoreResults = (page + 1) * pageSize < response.TotalCount
        }
    };
}
```

- **`CurrentFilters` is a set of opaque tokens** that *you* minted, in the filter model you
  returned with an earlier search. Nothing else interprets them, so the format is yours — but it
  has to survive a round trip through the portal unchanged.
- **Report `TotalResults` honestly.** A portal renders "1–30 of 812" from it; an estimate that
  shrinks as the visitor pages looks like data loss.
- **Declare what you support** in `GetFeatureSupportAsync` / `GetAssetsReadFeatureSupportAsync`.
  Claiming folder navigation you have not implemented hides the failure until a visitor clicks.

A working example: `DataAdapter-HelloWorld/Assets/Search/`, which also builds the facet model the
portal renders as filters.

## How do I page a source that has no random access?

*advanced.* Many systems return a cursor instead of an offset. Put the cursor in
`SearchResultSetId`; the caller passes it back as `SearchAssetsParameters.SearchResultSetUuid`,
and you continue from there.

```C#
public async Task<SearchAssetsResult> SearchAssetsAsync(SearchAssetsParameters parameters)
{
    var pageSize = parameters.PageSize ?? 30;

    // First call: no cursor. Later calls: the cursor we handed out last time.
    var response = await _client.SearchAsync(
        query: parameters.QueryString,
        pageToken: parameters.SearchResultSetUuid,
        limit: pageSize).ConfigureAwait(false);

    return new SearchAssetsResult
    {
        AssetDataObjects = response.Items.Select(item => _converter.Convert(item)).ToArray(),
        Details = new AssetSearchDetailsModel
        {
            CurrentPage = parameters.Page ?? 0,
            CurrentItemsPerPage = pageSize,
            TotalResults = response.TotalCount,
            SearchResultSetId = response.PageToken,      // the cursor for the next page
            HasMoreResults = !string.IsNullOrEmpty(response.PageToken)
        }
    };
}
```

Pitfalls:

- **A cursor expires.** Decide what happens when the external system rejects it — the usual
  answer is to restart the search from the beginning rather than to fail.
- **Do not mix the two models.** If you serve a cursor, ignore `Page` for fetching; using both
  produces gaps and duplicates that are almost impossible to reproduce on demand.
- **Random access is a feature you declare.** A source that can only walk forward reports
  `IsRandomAccessSupported = false` on its meta-model, and the portal stops offering "jump to
  page 12".

A working example: `DataAdapter-Picturepark/Assets/Search/PictureparkAssetsSearch.cs`.

## How do I feed the Smint.io integration layer?

*advanced.* In indexed mode Smint.io walks your source once, then asks repeatedly for what
changed. Three methods do it, and the contract is `IAssetsIntegrationLayerApiProvider`.

```C#
public class MyAssetsIntegrationLayer : IAssetsIntegrationLayerApiProvider
{
    /// Has the administrator finished configuring this adapter? The platform will not index
    /// anything while this is false.
    public bool IsSetup => _client.IsSetup;

    /// The initial walk: hand back the contents of one folder.
    public async Task<GetFolderContentsResult> GetFolderContentsForIntegrationLayerAsync(
        GetFolderContentsParameters parameters)
    {
        var contents = await _client
            .ListFolderAsync(parameters.FolderId?.Id, parameters.Page ?? 0, parameters.PageSize ?? 100)
            .ConfigureAwait(false);

        return new GetFolderContentsResult
        {
            AssetDataObjects = contents.Files.Select(file => _converter.Convert(file)).ToArray(),
            FolderDataObjects = contents.Folders.Select(folder => _converter.ConvertFolder(folder)).ToArray()
        };
    }

    /// Everything that changed since the last call. The platform gives back the continuation
    /// value you returned last time.
    public async Task<GetChangesResult> GetChangesAsync(GetChangesParameters parameters)
    {
        MyChangeList changes;

        try
        {
            changes = await _client.GetChangesAsync(deltaToken: parameters.LastContinuationId)
                .ConfigureAwait(false);
        }
        catch (ExternalDependencyException e)
        when (e.ErrorCode == ExternalDependencyStatusEnum.ContinuationUuidTooOld)
        {
            // The token is too old to be used — say so, and the platform re-indexes from scratch.
            return new GetChangesResult { ContinuationUuidTooOld = true };
        }

        return new GetChangesResult
        {
            Changes = changes.Items.Select(ToChangeModel).ToArray(),
            Details = new GetChangesDetailsModel
            {
                ContinuationUuid = changes.NextDeltaToken,
                HasMoreResults = changes.HasMore
            }
        };
    }

    /// One object, fetched on demand — used when a change carries only an identifier.
    public async Task<GetAssetChangeResult> GetAssetChangeAsync(GetAssetChangeParameters parameters)
    {
        var file = await _client.GetFileAsync(parameters.AssetId.Id).ConfigureAwait(false);

        return new GetAssetChangeResult
        {
            Change = new ChangeModel
            {
                UnscopedAssetIdentifier = parameters.AssetId.Id,
                Type = file == null ? ChangeType.AssetDeletion : ChangeType.AssetUpdate,
                AssetDataObject = file == null ? null : _converter.Convert(file)
            }
        };
    }

    private static ChangeModel ToChangeModel(MyChange change) =>
        new ChangeModel
        {
            UnscopedAssetIdentifier = change.Id,
            Type = change.IsDeleted ? ChangeType.AssetDeletion : ChangeType.AssetUpdate,
            AssetDataObject = change.IsDeleted ? null : _converter.Convert(change.File)
        };
}
```

| Piece | What to know |
|---|---|
| `ChangeType` | `AssetUpdate`, `AssetDeletion`, `FolderUpdate`, `FolderDeletion` |
| `UnscopedAssetIdentifier` | the source system's own id — **not** the scoped identifier the portal uses |
| `RecursionIsHandledByDataAdapter` | on a folder change: set it when you already reported the children, so the platform does not walk them again |
| `ContinuationUuid` | the token you want back next time |
| `ContinuationUuidTooOld` | the honest answer when your token has expired — much better than silently missing changes |

Pitfalls:

- **A deletion must carry the identifier and no data object.** Reporting a deletion as an update
  with an empty asset leaves the index holding an empty entry.
- **The continuation value is yours and it is persisted by the platform.** Changing its format
  between releases breaks the first sync after the upgrade unless you handle the old shape too.
- **`IsSetup` is not optional.** Indexing a half-configured adapter fills the index with
  material that has to be thrown away.

A working example: `DataAdapter-SharePoint/Assets/IntegrationLayer/SharepointAssetsIntegrationLayer.cs`.

## How do I keep state the external system cannot hold?

*advanced.* A flag saying that a one-time action already happened, a mapping between the source's
ids and someone else's, a cursor of your own. It does **not** go into a status or comment field
of the external system, where that system's own processes will overwrite it.

```C#
private readonly IIdPersistentStorage _idPersistentStorage;
private readonly ITemporalPersistentStorage _temporalPersistentStorage;

public MyAssetsDataAdapter(IServiceProvider serviceProvider, /* … */)
    : base(serviceProvider)
{
    _idPersistentStorage = serviceProvider.GetService<IIdPersistentStorage>();
    _temporalPersistentStorage = serviceProvider.GetService<ITemporalPersistentStorage>();
}

private async Task MarkAsAnnouncedAsync(string assetId)
{
    var record = JsonConvert.SerializeObject(new AnnouncementState
    {
        Version = 1,
        AnnouncedAt = DateTimeOffset.UtcNow
    });

    await _idPersistentStorage
        .AddOrUpdateAsync(uuid: $"announced-{assetId}", data: record, groupUuid: "announcements")
        .ConfigureAwait(false);
}

private async Task<bool> WasAnnouncedAsync(string assetId)
{
    var stored = await _idPersistentStorage.GetAsync($"announced-{assetId}").ConfigureAwait(false);

    return stored != null;
}
```

| Storage | Shape | Use it for |
|---|---|---|
| `IIdPersistentStorage` | keyed records, optionally grouped — `GetAsync`, `GetMultipleAsync`, `GetGroupAsync`, `AddOrUpdateAsync`, `AddOrUpdateMultipleAsync`, `RemoveAsync`, `RemoveGroupAsync`, `RemoveMultipleAsync` | state *about an object*: keyed by your own id |
| `ITemporalPersistentStorage` | an append-only sequence — `AddAsync(data, proprietaryValue)`, `GetAsync(id)`, `GetRangeAsync(lastKnownId, pageSize)` | a log you walk forward: events, an outbox, anything with "what happened since?" |

Pitfalls:

- **Both store strings.** Serialize deliberately, and put a version number in the payload —
  see [changing stored state](#user-content-how-do-i-change-stored-state-without-breaking-a-rollout).
- **Use the group** when a set of records belongs together; it is the only way to get or remove
  them in one call later.
- **Neither is a cache.** They persist; use [the cache](#user-content-how-do-i-cache-an-expensive-lookup)
  for anything you can recompute.

## How do I resolve a meta-model entity key at runtime?

*advanced.* Entity keys are rewritten to be unique per connector configuration, so the same
connector configured twice yields two different key sets. Anything that needs to find an entity
has to resolve it.

```C#
private readonly IEntityModelProvider _entityModelProvider;

public MyAssetsDataAdapter(IServiceProvider serviceProvider, /* … */)
    : base(serviceProvider)
{
    _entityModelProvider = serviceProvider.GetService<IEntityModelProvider>();
}

private string GetProductNumber(AssetDataObject assetDataObject, string jsonPathSelector)
{
    // The selector comes from the administrator's configuration — a metadata attribute picker —
    // never from a constant in your code.
    var path = _entityModelProvider.ParseEntityMetamodelEntityProperty(jsonPathSelector);

    if (path == null)
    {
        return null;
    }

    return _entityModelProvider.GetDataObjectStringValue(assetDataObject, path);
}
```

`GetEntityModel(entityModelKey)` resolves an entity, `ParseEntityMetamodelEntityProperty` turns
a selector into a path, and `GetDataObjectValue` / `GetDataObjectStringValue` /
`SetDataObjectValue` read and write through it.

Pitfall: **a hard-coded key works in your test tenant and fails in production**, because
production is a different connector configuration. If you find yourself writing a key literal,
take a `MetadataAttributeModel` configuration property instead and let the administrator pick.

## How do I publish my own public API interface?

*advanced.* A **custom** component publishes its own interface for one UI component to consume.
The interface *is* the data model — there is no meta-model and no `AssetDataObject`.

```C#
// 1. The interface.
public interface IMyProductCatalog : IDataAdapterInterface
{
    Task<GetFeaturedProductsResult> GetFeaturedProductsAsync(GetFeaturedProductsParameters parameters);
}

// 2. The parameter object.
public class GetFeaturedProductsParameters : IDataAdapterParameterObject
{
    public int? MaxProducts { get; set; }

    public AssetIdentifier RelatedToAssetId { get; set; }

    public void UnscopeIdentifiers(IIdentifierUnscopeHelper identifierUnscopeHelper)
    {
        // Incoming identifiers are scoped to the portal — strip the scope before you use them.
        RelatedToAssetId = identifierUnscopeHelper.UnscopeAssetIdentifier(RelatedToAssetId);
    }
}

// 3. The result object.
public class GetFeaturedProductsResult : IDataAdapterResult
{
    public ProductModel[] Products { get; set; }

    public void ScopeIdentifiers(IIdentifierScopeHelper identifierScopeHelper)
    {
        // Outgoing identifiers must be scoped, or the portal cannot resolve them.
        foreach (var product in Products ?? Array.Empty<ProductModel>())
        {
            product.AssetId = identifierScopeHelper.ScopeAssetIdentifier(product.AssetId);
        }
    }
}

// 4. Implement it, and declare it.
public class MyProductCatalogDataAdapter : DataAdapterBaseImpl, IMyProductCatalog
{
    public async Task<GetFeaturedProductsResult> GetFeaturedProductsAsync(
        GetFeaturedProductsParameters parameters)
    {
        var products = await _client.GetFeaturedAsync(parameters.MaxProducts ?? 12).ConfigureAwait(false);

        return new GetFeaturedProductsResult { Products = products.Select(Convert).ToArray() };
    }
}

// On the startup:
public Type[] PublicApiInterfaces => new[] { typeof(IMyProductCatalog) };
```

Then generate the TypeScript definition with the
[Data Adapter Exporter CLI](../../Tools/Portals-DataAdapter-SDK-DataAdapterExporter-CLI/Release/)
and consume it from your UI component — see
[consuming your own data adapter interface](../../Frontend/Legacy/docs/smintio-frontend-recipes.md#user-content-how-do-i-consume-my-own-data-adapter-interface).

Pitfalls:

- **`PublicApiInterfaces` is the boundary.** A method not declared on a listed interface is
  unreachable from a portal, with no error anywhere.
- **Implement the scoping both ways**, or derive your parameter and result types from the
  `Prefab` envelopes that already do it. Leaving them empty is the most common cause of "the id
  the component gets back does not resolve".
- **Design it as a published API.** Renaming a field breaks every portal already configured
  against it, exactly as renaming a configuration property does.
- **Do not build the productized surface as well.** A custom adapter does not implement
  `IAssets` "in case".

## How do I declare a custom permission?

*advanced.* Three coordinated steps. Missing the third makes the method permanently denied, and
nothing reports it.

```C#
// 1. Declare the permission.
public static class MyPermissions
{
    public const string ViewWholesalePricesPermissionUuid = "8e2f…";   // ask Smint.io for the uuid

    public static readonly DataAdapterPermission ViewWholesalePrices = new DataAdapterPermission
    {
        Uuid = ViewWholesalePricesPermissionUuid,
        Name = new ResourceLocalizedStringsModel(nameof(ConfigurationMessages.da_perm_wholesale_name)),
        Description = new ResourceLocalizedStringsModel(nameof(ConfigurationMessages.da_perm_wholesale_description))
    };
}

// 2. Require it on the method.
[RequiredPermissions(MyPermissions.ViewWholesalePricesPermissionUuid)]
Task<GetWholesalePricesResult> GetWholesalePricesAsync(GetWholesalePricesParameters parameters);

// 3. Publish it from the startup — this is the step that is forgotten.
public DataAdapterPermission[] Permissions => new[] { MyPermissions.ViewWholesalePrices };
```

Per-object permissions are separate and equally easy to miss: set `PermissionUuids` on every
asset and folder your converter produces. The interface-level check passing is not enough — an
asset with no permissions has its actions hidden in the portal, silently.

The UI component asks with `hasAssetPermission(asset, permissionUuid)` or
`hasSomewherePermission(permissionUuid)` — take the uuid as a configuration property there
rather than hard-coding it, so the component is not tied to one data adapter.

## How do I verify a signed link?

*advanced.* When your component is reached through a link handed to someone *outside* the
portal — a tokenised URL granting one person one action for a limited time — validating that
link is the data adapter's job, and nothing else does it for you.

```C#
public async Task<GetSharedDocumentResult> GetSharedDocumentAsync(GetSharedDocumentParameters parameters)
{
    // 1. Verify the signature BEFORE looking at anything else the link carries.
    if (!IsSignatureValid(parameters.DocumentId, parameters.ExpiresAtUtc, parameters.Signature))
    {
        throw new ExternalDependencyException(
            ExternalDependencyStatusEnum.AccessDenied,
            "The link signature is not valid",
            "shared-document");
    }

    // 2. Only then check expiry…
    if (parameters.ExpiresAtUtc < DateTimeOffset.UtcNow)
    {
        throw new ExternalDependencyException(
            ExternalDependencyStatusEnum.AccessDenied,
            "The link has expired",
            "shared-document");
    }

    // 3. …and only then act on the identifier it carries.
    var document = await _client.GetDocumentAsync(parameters.DocumentId).ConfigureAwait(false);

    return new GetSharedDocumentResult { Document = Convert(document) };
}

private bool IsSignatureValid(string documentId, DateTimeOffset expiresAt, string signature)
{
    // The signing key is a property of THIS data adapter's configuration. Name it so that it
    // is recognised as a secret — see the annotations document.
    var key = Encoding.UTF8.GetBytes(_configuration.LinkSigningKey);

    var payload = Encoding.UTF8.GetBytes($"{documentId}|{expiresAt.ToUnixTimeSeconds()}");

    using var hmac = new HMACSHA256(key);

    var expected = hmac.ComputeHash(payload);
    var actual = Convert.FromBase64String(signature);

    // Constant-time comparison — a length check and a loop that returns early leaks the answer.
    return CryptographicOperations.FixedTimeEquals(expected, actual);
}
```

Pitfalls:

- **Verify before you read.** Never fetch an object named in a link before the signature over it
  has checked out; that is the whole attack.
- **Keep signing and verification together**, even when only the verifier ships. You need the
  signer to test the verifier, and a test that mints its links through the same code is the only
  way to know the two halves agree.
- **The key belongs in the data adapter's own configuration**, which is stored encrypted at rest
  and never displayed back — see
  [which component's configuration a setting belongs in](smintio-backend-annotations.md#user-content-which-components-configuration-a-setting-belongs-in).
- **Do not roll your own comparison.** `FixedTimeEquals` exists for this.

## How do I add a second data adapter to an existing connector?

*basic.* The cheapest useful thing in this whole SDK: a different view of a system that is
already integrated — upload, collections, products, a customer-specific variant. The connector is
reused **unchanged**.

1. Add a new folder with its own startup, configuration and adapter class.
2. Point the startup's `ConnectorKey` at the **existing** connector's key.
3. Give it its own `ConfigurationMessages.resx`, with its own `da_<key>_…` resource prefix.
4. List it in the connector startup's default data adapter keys if it should be created
   automatically when the connector is configured.
5. Take the connector's client as a constructor parameter — it is already registered, from
   `ConfigureServicesForDataAdapter`.

```C#
public class MyUploadDataAdapterStartup : IDataAdapterStartup
{
    public const string MyUploadDataAdapter = "mypartner-upload";

    public string Key => MyUploadDataAdapter;

    public string ConnectorKey => MyConnectorStartup.MyConnector;      // the existing connector

    public Type ConfigurationImplementation => typeof(MyUploadDataAdapterConfiguration);
    public Type ComponentImplementation => typeof(MyUploadDataAdapter);
    public Type ConfigurationMessages => typeof(Resources.UploadConfigurationMessages);

    public Type[] PublicApiInterfaces => new[] { typeof(IAssetsUpload) };

    public DataAdapterPermission[] Permissions => null;
}
```

Pitfalls:

- **Each adapter has its own configuration**, and that is the point — the settings that differ
  between the two belong there rather than on the shared connector.
- **Its own resource file, its own prefix.** Two adapters sharing a `.resx` collide the first
  time both need a key called `da_assets_name`.
- **Do not fork the connector.** If the second adapter needs something the client does not
  expose, add the operation to the client interface.

A working example: the Picturepark data adapter project ships an assets adapter and an external
users adapter on one connector.

## How do I write a data processor that renames a download?

*basic.* A data processor hooks into a data adapter's operations without the data adapter
knowing. Renaming a download is the smallest useful one: a `Post` phase, changing the response.

```C#
public class DownloadNamingDataProcessor : DataProcessorBaseImpl,
    IPostGetAssetDownloadStreamDataProcessorLifecyclePhase
{
    private readonly DownloadNamingDataProcessorConfiguration _configuration;
    private readonly IEntityModelProvider _entityModelProvider;

    public DownloadNamingDataProcessor(
        IServiceProvider serviceProvider,
        DownloadNamingDataProcessorConfiguration configuration)
        : base(serviceProvider)
    {
        _configuration = configuration;
        _entityModelProvider = serviceProvider.GetService<IEntityModelProvider>();
    }

    public Task<AssetDownloadStreamModel> ProcessAsync(
        IDataAdapterContextModel dataAdapterContextModel,
        Type publicApiInterface,
        string methodName,
        AssetDataObject assetDataObject,
        string folderName,
        AssetDownloadStreamModel assetDownloadStreamModel,
        string outputFormatId)
    {
        if (assetDownloadStreamModel == null || assetDataObject == null)
        {
            return Task.FromResult(assetDownloadStreamModel);
        }

        var extension = Path.GetExtension(assetDownloadStreamModel.FileName);

        var prefix = _configuration.FileNamePrefix;

        // A value out of the asset's own metadata, chosen by the administrator.
        var path = _entityModelProvider.ParseEntityMetamodelEntityProperty(_configuration.NameAttribute);
        var namePart = _entityModelProvider.GetDataObjectStringValue(assetDataObject, path);

        if (string.IsNullOrWhiteSpace(namePart))
        {
            // Nothing to rename with — hand back what you were given, unchanged.
            return Task.FromResult(assetDownloadStreamModel);
        }

        // AssetDownloadStreamModel is immutable: its FileName, FileSizeInBytes, MediaType and
        // Stream are all get-only, so a rename means returning a new model over the same stream.
        var renamed = new AssetDownloadStreamModel(
            $"{prefix}{Sanitize(namePart)}{extension}",
            assetDownloadStreamModel.FileSizeInBytes,
            assetDownloadStreamModel.MediaType,
            assetDownloadStreamModel.Stream);

        return Task.FromResult(renamed);
    }

    private static string Sanitize(string value) =>
        string.Join("_", value.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
}
```

Pitfalls:

- **`Prepare` phases change the request, `Post` phases change the response.** Renaming is a
  `Post`; narrowing a search is a `Prepare`.
- **A `Post` phase does not reach the search index.** If the change has to be *searchable*, it
  belongs in the integration-layer ingestion phase instead.
- **Always return a model, never `null`.** The phase returns the download that carries on down
  the chain; returning nothing when you decide not to act drops the download.
- **Leave it alone when you cannot improve it.** A processor that produces `_.pdf` because the
  metadata was empty is worse than one that does nothing.
- **Sanitize.** The value came from an external system and ends up as a file name on someone's
  machine.

## How do I change stored state without breaking a rollout?

*advanced.* During a rollout both versions of your component run at once, against the same
stored state: cache entries, persistent storage records, a continuation token. Whatever you
store will be read by the *other* version.

```C#
private sealed class AnnouncementState
{
    public int Version { get; set; }             // always present, from day one

    public DateTimeOffset AnnouncedAt { get; set; }

    public string Channel { get; set; }          // added in version 2
}

private static AnnouncementState Read(string json)
{
    var state = JsonConvert.DeserializeObject<AnnouncementState>(json);

    return state.Version switch
    {
        1 => Upgrade(state),                     // fill in what version 2 expects
        2 => state,
        _ => null                                // written by a newer version — ignore it
    };
}
```

The rules that follow:

- **Version every payload from the first release.** Adding a version field later means guessing
  at what the unversioned records were.
- **Add fields, never repurpose them.** The old version is still writing the old meaning.
- **Tolerate the future.** A record written by the newer component must not crash the older one —
  ignoring it is usually right.
- **Change the cache key rather than the cached shape.** A new key means both versions simply
  miss once; a changed shape under the same key means one of them deserializes nonsense.
- **A continuation token is stored state too.** If its format changes, accept the old one for at
  least one release, or report it as too old and let the platform re-index.

## Questions

Please do not hesitate to contact us at [support@smint.io](mailto:support@smint.io) if a recipe
you need is missing, or if one of these does not work as described.

Contributors
============

- Reinhard Holzner, Smint.io GmbH

Smint.io Portals backend component recipes
==========================================

Current version of this document is: 2.4.0 (as of 15th of September, 2026)

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
1. [How do I offer downloads?](#user-content-how-do-i-offer-downloads) — advanced
1. [How do I accept uploads?](#user-content-how-do-i-accept-uploads) — advanced
1. [How do I feed the Smint.io integration layer?](#user-content-how-do-i-feed-the-smintio-integration-layer) — advanced
1. [How do I keep state the external system cannot hold?](#user-content-how-do-i-keep-state-the-external-system-cannot-hold) — advanced
1. [How do I stop two requests from colliding?](#user-content-how-do-i-stop-two-requests-from-colliding) — advanced
1. [How do I resolve a meta-model entity key at runtime?](#user-content-how-do-i-resolve-a-meta-model-entity-key-at-runtime) — advanced
1. [How do I publish my own public API interface?](#user-content-how-do-i-publish-my-own-public-api-interface) — advanced
1. [How do I declare a custom permission?](#user-content-how-do-i-declare-a-custom-permission) — advanced
1. [How do I accept a callback from the external system?](#user-content-how-do-i-accept-a-callback-from-the-external-system) — advanced
1. [How do I add a second data adapter to an existing connector?](#user-content-how-do-i-add-a-second-data-adapter-to-an-existing-connector) — basic
1. [How do I write a data processor that renames a download?](#user-content-how-do-i-write-a-data-processor-that-renames-a-download) — basic
1. [How do I write an identity provider?](#user-content-how-do-i-write-an-identity-provider) — advanced
1. [How do I ship a resource?](#user-content-how-do-i-ship-a-resource) — basic
1. [How do I write a task handler?](#user-content-how-do-i-write-a-task-handler) — advanced
1. [How do I build a portal template?](#user-content-how-do-i-build-a-portal-template) — basic
1. [How do I change stored state without breaking a rollout?](#user-content-how-do-i-change-stored-state-without-breaking-a-rollout) — advanced

## How do I validate credentials during setup?

*Basic.* `PerformPostConfigurationChecksAsync` is the one moment where a wrong credential reaches
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

*Basic.* Never ask an administrator to paste an identifier they have to look up. A dynamic
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
- **A name of your own belongs in a resource file.** The names above come from the external
  system, so they are built with `LocalizedStringsModel` directly. For a label your component
  supplies — "All channels", "None", a grouping header — put it in `ConfigurationMessages` and
  resolve it in one call:

  ```C#
  Name = ConfigurationMessages.ResourceManager
      .FullyResolveToLocalizedStringsModel(nameof(ConfigurationMessages.c_myconnector_all_channels))
  ```

  That returns a model carrying every language you ship. Resolve it once into a `static readonly`
  field rather than per row — see
  [three ways to turn a resource key into a localized string](smintio-backend-annotations.md#user-content-three-ways-to-turn-a-resource-key-into-a-localized-string).
- **Say what you support.** With `SupportsSearch => false`, `searchTerm` is always `null`; do not
  write code that depends on it.

A working example: `Connector-Picturepark/AllowedValues/ChannelAllowedValuesProvider.cs`, and
the SharePoint connector's `AllowedValues/` folder for providers that support search and paging.

## How do I make one dropdown depend on another?

*Basic.* A site has lists, a list has folders. There are two different situations here, and they
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

*Basic.* Without a request failed handler, a rate-limit response and a wrong password look the
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

*Basic.* Every call goes through the base client's execute helpers, which apply the backoff and
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

*Basic.* `ICache` is a constructor parameter on the connector, which forwards it into the client.
Use it for anything stable that you would otherwise fetch on every request — a schema, a field
list, a channel list, a display name.

```C#
private readonly ICache _cache;

/// The `getFreshData` flag is the convention: every cached read offers a way past the cache,
/// because something eventually has to be able to force a refresh.
public async Task<Dictionary<string, FieldDefinition>> GetFieldDefinitionsAsync(bool getFreshData)
{
    // The key only has to be unique WITHIN this component: the platform already scopes every
    // entry to the tenant and to this configured component. It only needs to say what the
    // value is, and to carry anything that varies inside your own component — a language, say.
    var cacheKey = "field_definitions";

    if (!getFreshData)
    {
        var cached = await _cache
            .GetAsync<Dictionary<string, FieldDefinition>>(cacheKey)
            .ConfigureAwait(false);

        if (cached != null)
        {
            return cached;
        }
    }

    var response = await FetchFieldDefinitionsAsync().ConfigureAwait(false);

    if (response == null || response.Count == 0)
    {
        // Do not cache an empty answer — it is usually a transient failure, and caching it
        // makes a five-second problem last for a day.
        return new Dictionary<string, FieldDefinition>();
    }

    var fieldDefinitions = response.ToDictionary(field => field.Id, field => field);

    await _cache.StoreAsync(cacheKey, fieldDefinitions, TimeSpan.FromDays(7)).ConfigureAwait(false);

    return fieldDefinitions;
}
```

The interface is four methods: `GetAsync<T>(key)`, `GetAsync<T>(keys)` for a batch,
`StoreAsync<T>(key, value, expiresIn)` and `InvalidateAsync(key)`. **`T` must be a class with a
parameterless constructor**, so a `string`, a struct or an array cannot be cached directly — wrap
it in a list, a dictionary or a small model of your own.

How long: schema-shaped things that change when an administrator edits them are cached for days;
anything that reflects content rather than structure gets minutes. When you know *when* a value
becomes wrong, prefer `InvalidateAsync` at that moment over a short expiry everywhere.

`WarmCachesAsync(bool forceWarming)` on the connector is called by the platform to fill the
cache before it is needed. Pre-fetch what is expensive *and* stable there; do not use it to load
content.

Pitfalls:

- **The cached type must be a `class` with a parameterless constructor.** Cache your own model,
  not a vendor SDK type that happens not to deserialize.
- **You do not scope the key yourself.** The platform already separates entries by tenant and by
  configured component, so no tenant id, portal id, connector key or configuration id belongs in
  it. What is still yours is uniqueness *inside* your own component: two different lookups that
  both key on a bare external id serve each other's values.
- **Do not cache an empty or failed answer**, and do not cache an access token — tokens live in
  the authorization values, which the platform refreshes.
- **A cached value outlives a deployment.** If its shape changes, change the key too: see
  [changing stored state](#user-content-how-do-i-change-stored-state-without-breaking-a-rollout).

## How do I report that an asset does not exist?

*Basic.* Not with `null`, and not with an empty result — both read as "the source has nothing to
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

*Advanced.* A search takes a page and a page size, and reports the paging state back in
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

*Advanced.* Many systems return a cursor instead of an offset. Put the cursor in
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

## How do I offer downloads?

*Advanced.* **Not by implementing `IAssetsDownload`.** That interface is the portal-facing
two-step flow — collect the options across everything the visitor selected, then produce one
download — and Smint.io owns it, because it spans data adapters, applies the permission gates and
delivers the file. Your data adapter contributes the *options* for its own assets, and serves the
bytes when one is chosen.

The extension point is `GetCustomAssetDownloadItemMappingsAsync` on `AssetsDataAdapterBaseImpl`.

The Picturepark example in this repository overrides the whole `GetAssetsDownloadItemMappingsAsync`
instead — it has to, because it builds its options from the vendor SDK rather than from the base
class's built-ins. That is the exception; unless you are in the same position, override the
custom hook and let the base class keep contributing the original, the PDF preview and the
playback renditions.

The base class already offers the original, the PDF preview, the playback rendition and the
preview thumbnail where the asset has them; you add whatever your source system has beyond that
— named renditions, transformations, derivations.

```C#
protected override async Task<ICollection<AssetDownloadItemMappingModel>>
    GetCustomAssetDownloadItemMappingsAsync(AssetDataObject assetDataObject)
{
    var renditions = await _client.GetRenditionsAsync(assetDataObject.Id).ConfigureAwait(false);

    if (renditions == null || renditions.Count == 0)
    {
        // Nothing of our own to add — let the base class decide.
        return await base.GetCustomAssetDownloadItemMappingsAsync(assetDataObject).ConfigureAwait(false);
    }

    return renditions
        .Select(rendition => new AssetDownloadItemMappingModel
        {
            // The ItemId comes back to you verbatim when the visitor picks this option —
            // it must be enough to resolve the rendition on its own.
            ItemId = rendition.Id,

            // Mandatory, and it is the ASSET's version, not the rendition's.
            Version = assetDataObject.Version,

            GroupId = assetDataObject.ContentType.Id,
            GroupName = assetDataObject.ContentType.ListDisplayName,

            Description = rendition.DisplayName.Localize(),

            // A suggestion. An administrator's high-resolution list overrides it wholesale.
            RequiresHiResDownloadPermission = rendition.IsFullResolution
        })
        .ToList();
}
```

Then serve the bytes. The same `ItemId` arrives as the output format on the download stream call,
which is where you turn it back into a request against the source system:

```C#
public override async Task<AssetDownloadStreamModel> GetAssetDownloadStreamAsync(
    AssetIdentifier assetId, AssetDownloadItemMappingModel assetDownloadItemMapping, ...)
{
    var renditionId = assetDownloadItemMapping.ItemId;

    var download = await _client.GetRenditionStreamAsync(assetId.Id, renditionId).ConfigureAwait(false);

    return new AssetDownloadStreamModel(
        download.FileName, download.FileSizeInBytes, download.MediaType, download.Stream);
}
```

**What the administrator's allow-lists do.** Implementing `IOutputFormatDataAdapterConfiguration`
gives them twelve lists — one global and one per content type, for the offered formats and again
for the ones needing the high-resolution permission. The rules are worth knowing before you
debug a missing download option:

- The **global list is an AND gate**: a non-empty `OutputFormatIdAllowList` that does not contain
  your `ItemId` removes the option, whatever the per-content-type list says.
- **No configuration at all means everything is allowed** — the lists only restrict.
- A content type outside image, video, audio, document and "other" **falls through to nothing
  allowed**. If a whole content type produces no downloads, check this first.
- When the administrator has filled any high-resolution list, your
  `RequiresHiResDownloadPermission` is **replaced** by their answer. When they have not, the
  original is high-resolution and everything else is not.

Pitfalls:

- **Do not implement `IAssetsDownload` in a connector's data adapter.** You would be reimplementing
  the cross-source flow, and the portal will not call it.
- **`Version` and `ItemId` are mandatory** on every mapping, and a missing one fails the whole
  call rather than dropping the option.
- **Never set `SelectedByDefault`.** It is reset and re-decided — first high-resolution option,
  otherwise the first option — so anything you set is discarded and only confuses the reader.
- **Return exactly one mapping set per asset you were asked about.** Silently skipping an asset
  that has been deleted at the source fails the whole batch, so return an empty mapping list for
  it instead.
- **Make the `ItemId` self-sufficient and stable.** It round-trips through the portal and comes
  back later; if it is an index into a list you rebuilt in the meantime, the visitor downloads the
  wrong rendition. Prefix your own ids so they cannot collide with the built-in ones, and remove
  a source rendition that duplicates "original" rather than offering it twice.

## How do I accept uploads?

*Advanced.* An upload data adapter is a **second data adapter** on an existing connector, and it
implements one method. The settings the upload form obeys — how many files, which types, which
size limits — come from your configuration class, and the base class publishes them for you.

```C#
public class MyAssetsUploadDataAdapterConfiguration : IAssetsUploadDataAdapterConfiguration
{
    [DisplayName("en", "Maximum number of files", IsDefault = true)]
    [DefaultValue(10)]
    public int? MaxFiles { get; set; }

    [DisplayName("en", "Maximum file size in bytes", IsDefault = true)]
    [MaxValue(50000000000)]
    public long? MaxFileSize { get; set; }

    // … ContentTypes, AllowedFormats, the per-content-type size limits,
    //   the image dimension limits, and CustomFormId
}
```

**Do not override `GetAssetUploadSettingsAsync`.** The base class projects your configuration
into it, and the same configuration drives the check that runs when the files actually arrive —
override the settings method and the two drift apart, so the form accepts what the check rejects.

The one method you write receives the uploaded files *by reference*, not as streams:

```C#
private readonly IFilePersistentStorage _filePersistentStorage;   // off the service provider
private readonly IPortalsContextModel _portalsContextModel;       // likewise

public override async Task HandleAssetUploadsAsync(
    long taskUuid,
    ICollection<AssetUploadModel> assetUploadModels,
    FormFieldValuesModel customFormFieldValuesModel,
    IProgressMonitor progressMonitor = null)
{
    if (string.IsNullOrEmpty(_configuration.TargetFolderId))
    {
        throw new ExternalDependencyException(
            ExternalDependencyStatusEnum.NotSetup,
            "The upload target folder is not configured",
            MyAssetsUploadDataAdapterStartup.MyAssetsUploadDataAdapter);
    }

    // Integer division: guard the count, or with many files each share rounds to zero.
    var count = Math.Max(assetUploadModels.Count, 1);
    var transferShare = Math.Max(80 / count, 1);

    foreach (var assetUploadModel in assetUploadModels)
    {
        var fileMonitor = progressMonitor?.CreateSubmonitor(transferShare);

        // The bytes live in IFilePersistentStorage until you fetch them.
        var uploadedFile = await _filePersistentStorage
            .GetUploadedFileAsync(_portalsContextModel, assetUploadModel.FileUuid)
            .ConfigureAwait(false);

        if (uploadedFile == null)
        {
            throw new ExternalDependencyException(
                ExternalDependencyStatusEnum.UploadedFileNoLongerAvailable,
                "The uploaded file is no longer available",
                assetUploadModel.OriginalFileName);
        }

        try
        {
            await _client.UploadAsync(
                _configuration.TargetFolderId,
                assetUploadModel.OriginalFileName,
                assetUploadModel.MediaType,
                uploadedFile.Stream,
                BuildMetadata(customFormFieldValuesModel, taskUuid)).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            throw new ExternalDependencyException(
                ExternalDependencyStatusEnum.UploadFailed,
                "The file could not be uploaded",
                assetUploadModel.OriginalFileName,
                e);
        }
        finally
        {
            uploadedFile.Stream?.Dispose();
        }

        if (fileMonitor != null)
        {
            await fileMonitor.FinishedAsync(null).ConfigureAwait(false);
        }
    }
}
```

When the source system would rather fetch the file itself, ask `IFilePersistentStorage` for a
**time-limited URL** instead of a stream, and hand that over — that is the one place in the
upload path where a signed URL is produced.

Pitfalls:

- **`AssetUploadModel` carries no bytes.** It is a reference — the file id, the original file
  name, the size and the media type. Fetch the content from `IFilePersistentStorage`, and
  dispose what you fetched.
- **Custom form values are namespaced.** Each value's id is the connector key, a separator and the
  remote field id; strip the prefix before you use it, and apply your own allow-list — a form
  value is administrator input, not a trusted field name.
- **Fail with `ExternalDependencyException` and the matching reason** — not set up, upload failed,
  file no longer available, media type not supported, too many files, file too large, type not
  allowed. The third argument is what the visitor sees, so pass the file name.
- **Null-check every progress monitor**, including the one you create per file. A `null` monitor
  is normal.
- **The upload permission is consumed, not declared.** The interface method carries the
  requirement; your startup declares no permissions of its own.
- **Stamp the task identifier onto what you create** where the source system allows it. It is how
  an upload is traced back to the request that approved it.

## How do I feed the Smint.io integration layer?

*Advanced.* In indexed mode Smint.io walks your source once, then asks repeatedly for what
changed. Three methods do it, and the contract is `IAssetsIntegrationLayerApiProvider`.

This is also **where a folder structure is served from**. `IAssetsFolderNavigation` is the
portal-facing interface and Smint.io implements it across data adapters; a connector exposes its
folders by returning them from `GetFolderContentsForIntegrationLayerAsync` and by producing
`FolderDataObject`s in its converter, and declares folder navigation in its feature support.

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

*Advanced.* A flag saying that a one-time action already happened, a mapping between the source's
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

    // The uuid is scoped to this configured component for you — it only has to be unique
    // within your own component, so the external system's own id is usually the whole key.
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
| `ITemporalPersistentStorage` | an append-only sequence — `AddAsync(data, proprietaryValue)`, `GetAsync(id)`, `GetRangeAsync(lastKnownId, pageSize)`, `UpdateAsync`, `RemoveAsync`, `RemoveMultipleAsync`, `RemoveRangeUntilIdAsync(id, deleteIdRecord)` | a log you walk forward: queued callbacks, an outbox, anything with "what happened since?" |

The temporal one has a shape worth knowing, because it is how a
[webhook queue](#user-content-how-do-i-accept-a-callback-from-the-external-system) is drained:

```C#
var lastKnownId = long.TryParse(parameters.LastContinuationId, out var parsed) ? parsed : 0;

// Delete what the previous call already reported — at the START of this call, once the
// platform has come back with the continuation value, never inside the loop.
if (lastKnownId > 0)
{
    await _temporalPersistentStorage
        .RemoveRangeUntilIdAsync(lastKnownId, deleteIdRecord: true)
        .ConfigureAwait(false);
}

// GetRangeAsync is EXCLUSIVE of lastKnownId.
var records = await _temporalPersistentStorage.GetRangeAsync(lastKnownId, pageSize: 100)
    .ConfigureAwait(false);

var changes = new List<ChangeModel>();

foreach (var record in records)
{
    // Advance on EVERY branch, including the ones that skip — otherwise the record that
    // cannot be parsed is replayed forever.
    lastKnownId = record.Id;

    var change = TryConvert(record);

    if (change == null)
    {
        continue;
    }

    changes.Add(change);
}
```

Pitfalls:

- **Both store strings.** Serialize deliberately, and put a version number in the payload —
  see [changing stored state](#user-content-how-do-i-change-stored-state-without-breaking-a-rollout).
  Never let a deserialization failure throw: log it, return nothing for that record, and move on.
- **Do not scope the uuid yourself.** The platform separates records by tenant and by configured
  component, so a tenant id, a portal id or the connector key in the uuid buys nothing and makes
  the record harder to find. Uniqueness within your own component is what you owe.
- **Use the group** when a set of records belongs together. It is the only way to get or remove
  them in one call later, and removing the group is how a full re-index starts from clean.
- **Both services can be `null`.** They come off the injected service provider with `GetService`;
  guard every use.
- **Neither is a cache.** They persist; use [the cache](#user-content-how-do-i-cache-an-expensive-lookup)
  for anything you can recompute.
- **Neither serialises anything.** Two requests can read the same record, both decide to act, and
  both write. When that matters, take a lock — see
  [stopping two requests from colliding](#user-content-how-do-i-stop-two-requests-from-colliding).

## How do I stop two requests from colliding?

*Advanced.* Two visitors act on the same objects at the same moment, and both writes reach the
external system interleaved — one overwrites the other's status, or both pass a "is it still
free?" check that only one of them should have passed. A `lock` statement does nothing about
this: your component is short-lived, there are many instances of it, and they do not share a
process. `IStorageBackedLock` is a mutual exclusion that holds **across every server running your
component**.

Like the storage services, the lock is **scoped to your configured component for you**: the uuid
you pass is only the last part of the real key. Two portals, or two configurations of the same
data adapter, therefore never block each other — which is almost always what you want, since they
usually point at different content. When they point at the *same* content and must be serialised
across configurations, that is not something the uuid can express; raise it with Smint.io.

```C#
private readonly IStorageBackedLock _storageBackedLock;

/// One name per thing being protected. Everything that must not run concurrently uses the
/// same one, and nothing else uses it.
private const string LockUuid = nameof(MyReservationDataAdapter);

public MyReservationDataAdapter(IServiceProvider serviceProvider, /* … */)
    : base(serviceProvider)
{
    _storageBackedLock = serviceProvider.GetService<IStorageBackedLock>();
}

public async Task<ReserveAssetsResult> ReserveAssetsAsync(ReserveAssetsParameters parameters)
{
    if (_storageBackedLock == null)
    {
        throw new ExternalDependencyException(
            ExternalDependencyStatusEnum.NotAvailable,
            "The storage backed lock service is not available",
            MyReservationDataAdapterStartup.MyReservationDataAdapter);
    }

    // The key is yours, it is secret, and it is what proves later calls are the same holder.
    var lockKey = Guid.NewGuid().ToString();

    // Keep the duration short — it is the time the lock survives if your component dies
    // holding it. Prolong instead of asking for a long one up front.
    var lockAcquired = await _storageBackedLock
        .LockAsync(LockUuid, TimeSpan.FromMinutes(1), lockKey)
        .ConfigureAwait(false);

    if (!lockAcquired)
    {
        // It does not wait. Tell the visitor to come back, in their own language.
        throw new ExternalDependencyException(
            ExternalDependencyStatusEnum.CustomError,
            "Someone else is currently changing these assets. Please try again in a moment",
            "reservation_in_progress");
    }

    try
    {
        // Everything between here and the finally is protected: read the current state,
        // decide, and write it back.
        var current = await _client.GetReservationStateAsync(parameters.AssetIds).ConfigureAwait(false);

        if (current.Any(state => state.IsReserved))
        {
            throw new ExternalDependencyException(
                ExternalDependencyStatusEnum.CustomError,
                "One of the assets has just been reserved by someone else",
                "already_reserved");
        }

        await _client.SetReservedAsync(parameters.AssetIds, parameters.RequesterName).ConfigureAwait(false);

        return new ReserveAssetsResult { ReservedAssetIds = parameters.AssetIds };
    }
    finally
    {
        // Always, on every path. The expiry is the safety net, not the release mechanism.
        await _storageBackedLock.ClearLockAsync(LockUuid, lockKey).ConfigureAwait(false);
    }
}
```

**Prolonging a lock you already hold.** When the protected work is a loop over many objects, ask
for a short duration and extend it as you go, rather than asking for the ten minutes the worst
case might need. Calling `LockAsync` again with the **same uuid and the same lock key** extends
the expiry; the lock key is what proves you are the holder, so a caller without it gets `false`.

```C#
var processed = 0;

foreach (var chunk in assetIds.Chunk(10))
{
    await _client.UpdateAsync(chunk).ConfigureAwait(false);

    processed += chunk.Length;

    // Roughly every minute of work, buy another minute.
    if (processed % 100 == 0)
    {
        await _storageBackedLock
            .LockAsync(LockUuid, TimeSpan.FromMinutes(1), lockKey)
            .ConfigureAwait(false);
    }
}
```

Pitfalls:

- **It does not queue.** `LockAsync` returns `false` immediately when someone else holds the
  lock — it never waits. Decide what that means for the caller: usually "try again shortly",
  occasionally "we have queued your request". Do not loop on it in a tight retry.
- **Release in a `finally`.** The duration is there for the case where your component is killed
  mid-operation; relying on it for ordinary release means every other caller waits out the full
  expiry for nothing.
- **Keep the duration short and prolong.** A ten-minute lock taken by a request that dies after
  one second blocks the feature for ten minutes.
- **Keep the lock key private and unique per acquisition.** A fresh `Guid` per call is right. It
  is the only thing that distinguishes the holder from everyone else, so never derive it from
  something a caller supplies, and never reuse one.
- **Lock around the decision *and* the write.** Reading the state before the lock and writing
  after it protects nothing — the check and the change have to be inside.
- **One uuid per protected resource, and do not scope it yourself.** Naming it after the component
  (`nameof(MyDataAdapter)`) serialises every protected operation of that configured adapter
  against each other, which is the safe default; a finer uuid — per folder, per collection — gets
  more throughput, and is only correct when operations on different ones genuinely cannot
  interfere. Putting the tenant or the configuration into the uuid adds nothing: it is already
  there.
- **The service can be `null`.** Like the other storage services it comes off the service
  provider with `GetService`, so decide deliberately whether "no lock available" means fail or
  proceed — for anything that can double-book, it means fail.
- **Do not hold it across a call that can hang.** An external system with no timeout inside a
  lock turns one slow request into an outage for the feature; set a timeout on the client and
  let the lock expire rather than pinning it.

## How do I resolve a meta-model entity key at runtime?

*Advanced.* Entity keys are rewritten to be unique per connector configuration, so the same
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

*Advanced.* A **custom** component publishes its own interface for one UI component to consume.
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

*Advanced.* Three coordinated steps. Missing the third makes the method permanently denied, and
nothing reports it.

```C#
// 1. Declare the permission.
public static class MyPermissions
{
    // A readable, stable slug of your own. It is persisted with every grant an administrator
    // makes, so it cannot change after release — and it must not collide with a built-in
    // (search_assets, read_asset_details, download_assets_hi_res, upload_assets, …) or with
    // another component's.
    public const string ViewWholesalePricesPermissionUuid = "view_wholesale_prices";

    // Resolve the labels once, here, into a model carrying every language you ship.
    public static readonly DataAdapterPermission ViewWholesalePrices = new DataAdapterPermission
    {
        Uuid = ViewWholesalePricesPermissionUuid,
        Name = ConfigurationMessages.ResourceManager
            .FullyResolveToLocalizedStringsModel(nameof(ConfigurationMessages.da_myadapter_perm_wholesale_name)),
        Description = ConfigurationMessages.ResourceManager
            .FullyResolveToLocalizedStringsModel(nameof(ConfigurationMessages.da_myadapter_perm_wholesale_description))
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

## How do I accept a callback from the external system?

*Advanced.* A source system that can tell you what changed is worth far more than one you have to
poll. The contract is `IWebhooksApiProvider`, it has two methods, and the split between them is
the whole design: **validate cheaply, then queue; never do the work in the callback.**

```C#
public partial class MyAssetsDataAdapter : IWebhooksApiProvider
{
    private const string SignatureHeader = "X-My-Signature";
    private const string TimestampHeader = "X-My-Timestamp";

    public Task<ValidateWebhookResultModel> ValidateWebhookAsync(
        ValidateWebhookContextModel validateWebhookContextModel,
        IDictionary<string, IEnumerable<string>> headersByKey,
        string data)
    {
        // Optional but cheap: refuse anything not coming from where it should.
        if (_configuration.AllowedIpAddresses?.Length > 0 &&
            !_configuration.AllowedIpAddresses.Contains(validateWebhookContextModel?.RemoteIpAddress))
        {
            _logger.LogWarning("Webhook rejected: remote address is not allowed");

            return Task.FromResult(new ValidateWebhookResultModel(isSuccess: false));
        }

        var timestamp = GetHeader(headersByKey, TimestampHeader);
        var signature = GetHeader(headersByKey, SignatureHeader);

        if (string.IsNullOrEmpty(timestamp) || string.IsNullOrEmpty(signature))
        {
            _logger.LogWarning("Webhook rejected: signature headers are missing");

            return Task.FromResult(new ValidateWebhookResultModel(isSuccess: false));
        }

        // Reject a replayed message before doing anything else with it.
        if (!IsWithinReplayWindow(timestamp, TimeSpan.FromMinutes(5)))
        {
            _logger.LogWarning("Webhook rejected: timestamp is outside the accepted window");

            return Task.FromResult(new ValidateWebhookResultModel(isSuccess: false));
        }

        // The signature is over the timestamp AND the body, so neither can be swapped.
        var isValid = IsSignatureValid($"{timestamp}.{data}", signature);

        if (!isValid)
        {
            _logger.LogWarning("Webhook rejected: signature did not validate");
        }

        return Task.FromResult(new ValidateWebhookResultModel(isSuccess: isValid));
    }

    private bool IsSignatureValid(string signedContent, string presentedSignature)
    {
        // The signing secret is a property of THIS data adapter's configuration.
        if (string.IsNullOrEmpty(_configuration.WebhookSigningSecret))
        {
            return false;
        }

        using var hmac = new HMACSHA256(Convert.FromBase64String(_configuration.WebhookSigningSecret));

        var expected = hmac.ComputeHash(Encoding.UTF8.GetBytes(signedContent));

        byte[] presented;

        try
        {
            presented = Convert.FromBase64String(presentedSignature);
        }
        catch (FormatException)
        {
            return false;
        }

        // Constant-time comparison: a length check and an early return leak the answer.
        return CryptographicOperations.FixedTimeEquals(expected, presented);
    }

    public async Task ProcessWebhookAsync(string data, string type)
    {
        if (string.IsNullOrEmpty(data))
        {
            throw new ArgumentNullException(nameof(data));
        }

        // Queue it. Do not fetch, convert or index here — the caller is waiting, and it
        // will retry the whole delivery if you are slow.
        await _temporalPersistentStorage.AddAsync(data, type).ConfigureAwait(false);
    }
}
```

**The loop this belongs to.** A validated callback is queued, and the integration layer drains the
queue the next time Smint.io asks what changed — so a webhook-driven source and a polled source
end up in the same place:

```
external system → ValidateWebhookAsync → ProcessWebhookAsync → temporal storage
                                                                     ↓
                                       GetChangesAsync drains the queue → ChangeModel[]
```

`GetChangesAsync` reads forward from the continuation value it handed out last time, converts the
queued payloads into changes, and returns a new continuation value —
see [feeding the integration layer](#user-content-how-do-i-feed-the-smintio-integration-layer)
and [keeping state](#user-content-how-do-i-keep-state-the-external-system-cannot-hold).

Registering the callback URL is a separate step. The URL is not yours to invent: ask for it
through the data adapter context's reflection provider, and expect the registration itself to be
arranged with Smint.io rather than performed automatically from your component.

Pitfalls:

- **`IWebhooksApiProvider` goes into `PublicApiInterfaces`** even though it is not part of the
  portal-facing surface. Leaving it out means the callback never reaches you.
- **Never throw to reject a message.** Return `new ValidateWebhookResultModel(isSuccess: false)`
  and log why. An exception is for a malformed request, not an unauthorized one.
- **Validation must be cheap.** No calls to the external system, no database work — several
  source systems time out in seconds and then redeliver, which turns one event into many.
- **Never lose an event.** If the payload does not deserialize, still queue the raw body under a
  "failed" marker rather than dropping it; you cannot ask the source system to send it again.
- **Some systems begin with a handshake.** A first request may carry a validation code that has to
  be echoed back — that is what the result's response string is for, so check for it before you
  start validating signatures.
- **Put the signing secret on the data adapter**, and name it so it is treated as a secret. Leave
  it optional — there is no safe default for a key, and requiredness cannot be made conditional on
  another setting — and give its absence a defined behaviour: an unconfigured secret must mean
  "reject everything", never "accept everything".

### Signing a URL you hand out

The other direction — a link your component gives to a browser or to the external system — is the
same discipline in reverse: sign a canonical string, include an expiry, and check both the upper
and lower bound when it comes back.

- **Put an expiry in the signed data**, and reject a timestamp that is too old *or* too far in the
  future; clock skew in the other direction is just as much a forgery signal.
- **Re-derive, never trust.** When a caller proposes the parameters to sign, rebuild the allowed
  set yourself and refuse anything that does not match, rather than signing what you were handed.
- **The secret stays in the backend.** A public identifier may travel with the request; the
  signing key never leaves your component.
- **Keep signing and verification together** even when only one half ships, so a test can mint a
  link through the same code that checks it.

## How do I add a second data adapter to an existing connector?

*Basic.* The cheapest useful thing in this whole SDK: a different view of a system that is
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

*Basic.* A data processor hooks into a data adapter's operations without the data adapter
knowing. Renaming a download is the smallest useful one: a `Post` phase, changing the response.

A processor is four files — startup, configuration, the processor, and a resource file — plus a
model class when it renders a template.

**The startup.** `Type` is not free text: it is one of three constants, and it decides where the
processor is offered in the administration user interface.

```C#
public class DownloadNamingDataProcessorStartup : IDataProcessorStartup
{
    public const string DownloadNamingDataProcessor = "mypartner-download-naming";

    public string Type => DataProcessorType.MetadataTransformation;   // or .Scripting, or .Templates
    public string Key => DownloadNamingDataProcessor;

    public LocalizedStringsModel Name { get; } =
        new ResourceLocalizedStringsModel(nameof(ConfigurationMessages.dp_mypartner_download_naming_name));
    public LocalizedStringsModel Description { get; } =
        new ResourceLocalizedStringsModel(nameof(ConfigurationMessages.dp_mypartner_download_naming_description));

    public string LogoUrl => null;
    public string IconUrl => null;
    public string IllustrationUrl => null;
    public string MdiIcon => "mdi-form-textbox";

    public bool IntegrationLayerOnly => false;

    public void ConfigureServices(IServiceCollection services) { }

    public Type ComponentImplementation => typeof(DownloadNamingDataProcessor);
    public Type ConfigurationImplementation => typeof(DownloadNamingDataProcessorConfiguration);
    public Type ConfigurationMessages => typeof(ConfigurationMessages);
    public Type MetamodelMessages => null;
}
```

**The configuration.** The administrator should not be writing C#, so the naming pattern is a
Liquid template and the metadata it can read is a picker:

```C#
[DisplayName(translationKey: nameof(ConfigurationMessages.dp_mypartner_download_naming_attributes_display_name))]
[DynamicAllowedValuesProvider(typeof(MetadataAttributeAllowedValuesProvider))]
[FormGroup("naming")]
public MetadataAttributeModel[] NameAttributes { get; set; }

[DisplayName(translationKey: nameof(ConfigurationMessages.dp_mypartner_download_naming_template_display_name))]
[IsLiquid]
[DefaultValue("{{ assetDataObject.firstAvailableMetadataAttribute }}")]
[FormItemVisibility(Visibility = FormItemVisibilityEnum.Advanced)]
[FormGroup("naming")]
public string LiquidTemplate { get; set; }
```

**The processor.** The phase returns the download that carries on down the chain, so it returns a
model rather than mutating one — `AssetDownloadStreamModel` is immutable.

```C#
public class DownloadNamingDataProcessor : DataProcessorBaseImpl,
    IPostGetAssetDownloadStreamDataProcessorLifecyclePhase
{
    private readonly DownloadNamingDataProcessorConfiguration _configuration;
    private readonly Lazy<ILiquidRenderer> _liquidRenderer;
    private readonly IEntityModelProvider _entityModelProvider;
    private readonly ICache _cache;

    public DownloadNamingDataProcessor(
        DownloadNamingDataProcessorConfiguration configuration,
        ILogger logger,
        IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        _configuration = configuration;

        // The renderer is required; the other two are optional and every use must null-guard.
        _liquidRenderer = new Lazy<ILiquidRenderer>(() => serviceProvider.GetRequiredService<ILiquidRenderer>());
        _entityModelProvider = serviceProvider.GetService<IEntityModelProvider>();
        _cache = serviceProvider.GetService<ICache>();
    }

    public async Task<AssetDownloadStreamModel> ProcessAsync(
        IDataAdapterContextModel dataAdapterContextModel,
        Type publicApiInterface,
        string methodName,
        AssetDataObject assetDataObject,
        string folderName,
        AssetDownloadStreamModel assetDownloadStreamModel,
        string outputFormatId)
    {
        if (assetDataObject == null || assetDownloadStreamModel == null)
        {
            return assetDownloadStreamModel;
        }

        var newFileName = await GetModifiedFileNameAsync(assetDataObject, assetDownloadStreamModel, outputFormatId)
            .ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(newFileName))
        {
            // Nothing to rename with — hand back what you were given, unchanged.
            return assetDownloadStreamModel;
        }

        return new AssetDownloadStreamModel(
            newFileName,
            assetDownloadStreamModel.FileSizeInBytes,
            assetDownloadStreamModel.MediaType,
            assetDownloadStreamModel.Stream);
    }

    private async Task<string> GetModifiedFileNameAsync(
        AssetDataObject assetDataObject,
        AssetDownloadStreamModel assetDownloadStreamModel,
        string outputFormatId)
    {
        var extension = Path.GetExtension(assetDownloadStreamModel.FileName);

        var values = await GetConfiguredAttributeValuesAsync(assetDataObject).ConfigureAwait(false);

        if (values == null || values.Count == 0)
        {
            return null;
        }

        var model = new DownloadNamingLiquidModel(assetDataObject)
        {
            MetadataAttributes = values,
            DownloadRenditionId = outputFormatId
        };

        var rendered = _liquidRenderer.Value.RenderLiquid(Context, _configuration.LiquidTemplate, model);

        if (string.IsNullOrWhiteSpace(rendered))
        {
            return null;
        }

        return $"{Sanitize(rendered)}{extension}";
    }

    private static string Sanitize(string value) =>
        string.Join("_", value.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
}
```

**The Liquid model** is a class implementing `ILiquidModel`; every public member of it is a name
the administrator may write in their template, so treat those names as published API:

```C#
public class DownloadNamingLiquidModel : LiquidDataObjectModel, ILiquidModel
{
    public DownloadNamingLiquidModel(AssetDataObject assetDataObject) : base(assetDataObject) { }

    public List<string> MetadataAttributes { get; set; }

    public string FirstAvailableMetadataAttribute =>
        MetadataAttributes?.FirstOrDefault(value => !string.IsNullOrEmpty(value));

    public string DownloadRenditionId { get; set; }
}
```

### Changing the request instead of the response

A `Prepare` phase receives the **mutable parameters object** and changes it in place. It returns
`Task`, not a result — narrowing a search is the typical use:

```C#
public class TenantScopingDataProcessor : DataProcessorBaseImpl,
    IPrepareAssetSearchDataProcessorLifecyclePhase
{
    public Task ProcessAsync(
        IPortalsContextModel portalsContextModel,
        IDataAdapterContextModel dataAdapterContextModel,
        Type publicApiInterface,
        string methodName,
        SearchAssetsParameters input)
    {
        // Mutate the parameters the data adapter is about to be called with.
        input.QueryString = string.IsNullOrEmpty(input.QueryString)
            ? _configuration.MandatoryQuery
            : $"({input.QueryString}) AND ({_configuration.MandatoryQuery})";

        // A result that depends on who is asking must not be cached across users.
        portalsContextModel?.ResultCacheControl?.EnforceUserScopedResultCaching();

        return Task.CompletedTask;
    }
}
```

To *refuse* a request, throw `ExternalDependencyException` with `AccessDenied`. A `Prepare` phase
and its matching `Post` phase run on the **same, per-request processor instance**, so state they
share is an ordinary instance field — never a static one.

### Making the change searchable

A `Post` phase changes what this one call returns; it never reaches the search index. When the
change has to be searchable, implement the integration-layer ingestion phase as well. Its
signature is deliberately smaller — no portals context, no method name, no result — because it
runs while the asset is being indexed:

```C#
public Task ProcessAsync(IDataAdapterContextModel context, AssetDataObject assetDataObject)
{
    // Mutate the asset that is about to be indexed.
    ApplyNaming(assetDataObject);

    return Task.CompletedTask;
}
```

Ship a configuration flag for the two modes and guard the runtime phases with it, so an
administrator can choose between "change it as it is served" and "change it as it is indexed"
rather than getting both.

Pitfalls:

- **`Prepare` changes the request, `Post` changes the response**, and neither reaches the index —
  that is the ingestion phase.
- **Always return a model, never `null`**, from a phase that returns one; returning nothing when
  you decide not to act drops the download.
- **Localize per culture, not once.** A localized value has one string per language: render your
  template for *each* culture present in the source attributes, and give up — returning the
  original untouched — unless the result carries the default culture. Rendering once against the
  current culture produces a file name in whichever language happened to be active.
- **Cache parsed metadata paths against the configuration version.** Parsing a meta-model path is
  expensive and the result is only valid for one configuration *version*, so the cache key is your
  own prefix plus `Context.ConfigurationVersion` — and give each distinct set of paths its own
  prefix, or one lookup serves the other's result. This is not scoping: the platform already
  separates entries per configured component. The version is there so that an administrator's
  edit invalidates what you parsed from the previous one.
- **Both `IEntityModelProvider` and `ICache` can be `null`.** They are resolved with
  `GetService`, not `GetRequiredService`; guard every use.
- **Sanitize.** The value came from an external system and ends up as a file name on someone's
  machine.
- **Every public member of the Liquid model is API.** Administrators write those names into
  templates that are then saved in portals; renaming one breaks them silently.

## How do I write an identity provider?

*Advanced.* Less work than it sounds, because **you never write the login flow**. The platform
does the OpenID Connect or SAML dance; your component's job is to validate the tenant's identity
server during configuration and write what the platform needs onto the authorization values.

```C#
public class MyOidcIdentityProvider : OidcIdentityProvider          // or SamlIdentityProvider
{
    private readonly MyOidcIdentityProviderConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public MyOidcIdentityProvider(
        MyOidcIdentityProviderConfiguration configuration,
        IHttpClientFactory httpClientFactory)
        : base(configuration)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    public override async Task PerformPostConfigurationChecksAsync(
        AuthorizationValuesModel authorizationValuesModel)
    {
        var ssoUrl = _configuration.SsoUrl?.TrimEnd('/');

        try
        {
            // Prove the URL really is an identity server before anyone tries to log in.
            using var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromMinutes(1);

            using var response = await httpClient
                .GetAsync($"{ssoUrl}/.well-known/openid-configuration")
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Discovery returned {response.StatusCode}");
            }
        }
        catch (Exception)
        {
            throw new ExternalDependencyException(
                ExternalDependencyStatusEnum.EndpointUrlInvalid,
                "The identity server URL did not deliver valid discovery information",
                MyOidcIdentityProviderStartup.MyOidcIdentityProvider);
        }

        // Everything the platform needs to run the flow goes onto the model.
        authorizationValuesModel.IdentityServerUrl = ssoUrl;
        authorizationValuesModel.RequiredScopes = new[] { "openid", "profile", "email" };
        authorizationValuesModel.UsePkce = true;
        authorizationValuesModel.UsePassThroughAuthentication = false;

        // Incoming claim name → the claim name Smint.io expects.
        authorizationValuesModel.ClaimMappings = new Dictionary<string, string>
        {
            { "preferred_username", "email" }
        };
    }
}
```

The configuration class opts into behaviour with marker interfaces, exactly as a data adapter's
does:

| Interface | What it adds |
|---|---|
| `IOidcIdentityProviderConfiguration` | the OIDC basics — the SSO URL, the client id |
| `IOidcClientSecretIdentityProviderConfiguration` | the above plus `ClientSecret`, for a confidential client |
| `ISamlIdentityProviderConfiguration` | `MetadataUrl` — SAML is configured from the metadata document |
| `IPassThroughIdentityProviderConfiguration` | the visitor's own token is passed through to the connector |
| `IExternalUserGroupsIdentityProviderConfiguration` | group membership is resolved from an external system |

For SAML the shape is the same: fetch the metadata document, validate it, read the entity id out
of it, and set `IdentityServerUrl` and `PeerEntityId`.

### Resolving group membership from the source system

`IExternalUserGroupsIdentityProviderConfiguration` needs three members, and they are **not** form
fields — the platform fills them in, so declare them bare, with no annotations:

```C#
public bool ExternalUserGroupResolutionEnabled { get; set; }
public IExternalUsersRead ExternalUsersRead { get; set; }
public int ExternalUserLookupRetries { get; set; }
```

The other half is an ordinary data adapter on your connector that publishes `IExternalUsersRead`:

```C#
public async Task<GetUserGroupMembershipResult> GetUserGroupMembershipAsync(
    GetUserGroupMembershipParameters parameters)
{
    var groupIds = await _client.GetGroupsForCurrentUserAsync().ConfigureAwait(false);

    if (groupIds == null)
    {
        return null;
    }

    return new GetUserGroupMembershipResult
    {
        ExternalUserGroups = groupIds
            .Select(groupId => new ExternalUserGroupModel(Context) { Id = groupId })
            .ToList()
    };
}
```

Pitfalls:

- **`AuthorizationValuesModel` is two different classes.** The identity provider SDK has one and
  the connector SDK has another, with the same name and different members —
  `ClaimMappings`, `RequiredScopes`, `UsePkce` and `PeerEntityId` exist only on the identity
  provider one. Importing the wrong namespace compiles and then makes no sense.
- **Do not implement group resolution yourself.** The base class does it, including a retry for
  the case where a just-provisioned single sign-on user is not readable yet. You only supply the
  data adapter that answers the lookup.
- **A scope can be removed as well as added.** Prefixing an entry of `RequiredScopes` with `-`
  tells the platform to leave that scope out — the way to suppress a scope it would otherwise
  request.
- **`SetupDocumentationUrl` on the startup is a localized strings model**, not a string: a
  documentation URL per language, out of your resource file.
- **Return `null` from a group lookup to mean "no opinion"** and an empty list to mean "this user
  is in no groups". They are different: the first leaves existing group assignments alone.

## How do I ship a resource?

*Basic.* A resource is a typed piece of portal content — a string, an image, a menu item, an
email — that an administrator maintains once and components point at. The plain kind has **no
code at all**: a startup that declares it and a configuration class that *is* the content.

```C#
public class ProductNoteResourceStartup : IResourceStartup
{
    public const string ProductNoteResource = "mypartner-product-note";

    public string Key => ProductNoteResource;

    public LocalizedStringsModel Name { get; } =
        new ResourceLocalizedStringsModel(nameof(ConfigurationMessages.r_mypartner_product_note_name));
    public LocalizedStringsModel Description { get; } =
        new ResourceLocalizedStringsModel(nameof(ConfigurationMessages.r_mypartner_product_note_description));

    public string LogoUrl => null;
    public string IconUrl => null;
    public string MdiIcon => "mdi-text-short";

    public string[] AllowedPortalTypes => null;      // or e.g. [PortalType.PressPortal]
    public bool IsResourceAsset => false;

    public void ConfigureServices(IServiceCollection services) { }

    public Type ComponentImplementation => null;     // a plain resource has no implementation
    public Type ConfigurationImplementation => typeof(ProductNoteResourceConfiguration);
    public Type ConfigurationMessages => typeof(ConfigurationMessages);
}
```

```C#
[Serializable]
public class ProductNoteResourceConfiguration : IComponentConfiguration
{
    [DisplayName(translationKey: nameof(ConfigurationMessages.r_mypartner_product_note_text_display_name))]
    [Required]
    [MaxLength(255)]
    public LocalizedStringsModel Text { get; set; }
}
```

### A resource asset — a resource that behaves like an asset

When the resource should be searchable and renderable like an asset — a press release, a
campaign, a profile — derive the startup from `ResourceAssetStartup` instead. It fixes
`IsResourceAsset` and `ComponentImplementation` for you and adds two members: a meta-model, and
the conversion into an `AssetDataObject`.

```C#
public class CampaignResourceStartup : ResourceAssetStartup
{
    public const string CampaignResource = "mypartner-campaign";

    public override string Key => CampaignResource;
    public override string[] AllowedPortalTypes { get; } = new[] { PortalType.PressPortal };
    public override Type MetamodelMessages => typeof(MetamodelMessages);

    public override ResourceAssetMetamodel GetResourceAssetMetamodel()
    {
        // Return null when you need no properties of your own — the built-in composite
        // metadata is then the whole story.
        var metamodel = new ResourceAssetMetamodel(this, version: 1);

        var entityModel = metamodel.AddEntity(
            CampaignResource,
            EntityType.TopLevelObject,
            parentEntityModelKey: null,
            new ResourceLocalizedStringsModel(nameof(MetamodelMessages.r_mypartner_campaign_name)));

        entityModel.AddProperty(
            nameof(CampaignResourceConfiguration.Headline),
            DataType.LocalizedStringsModel,
            new ResourceLocalizedStringsModel(nameof(MetamodelMessages.r_mypartner_campaign_headline)));

        return metamodel;
    }

    public override Task FillAssetDataObjectAsync(
        AssetDataObject assetDataObject,
        IComponentConfiguration componentConfiguration,
        IPageReferenceResolver pageReferenceResolver,
        IEntityModelProvider entityModelProvider)
    {
        if (componentConfiguration is not CampaignResourceConfiguration configuration)
        {
            return Task.CompletedTask;
        }

        assetDataObject.Name = configuration.Name;
        assetDataObject.PathForUrl = configuration.PathForUrl;

        // Related assets are how a resource asset points at real assets.
        if (configuration.HeroImageAssetId != null &&
            !string.IsNullOrEmpty(configuration.HeroImageAssetId.UnscopedId))
        {
            assetDataObject.RelatedAssets = new[]
            {
                new RelatedAssetsDataObject
                {
                    Type = RelationshipTypeEnumDataObject.HeroRepresentation,
                    AssetIds = new[] { configuration.HeroImageAssetId.ToString() }
                }
            };
        }

        // Your own properties go into raw data, keyed by the entity you declared.
        var entityModel = entityModelProvider.GetEntityModel(CampaignResource);

        var dataObject = new DataObject(entityModel.Key);

        if (configuration.Headline != null)
        {
            dataObject.SetValue(nameof(CampaignResourceConfiguration.Headline), configuration.Headline);
        }

        assetDataObject.RawData = new[] { dataObject };

        return Task.CompletedTask;
    }
}
```

Pitfalls:

- **`version` is part of the identity, not a display number.** It is concatenated into the
  meta-model key, so incrementing it mints a new meta-model — a breaking change, not a cosmetic
  one.
- **Property keys are `nameof(<Configuration>.<Property>)` on both sides.** The declaration and
  the `SetValue` have to move together; renaming the configuration property alone breaks the
  binding with no error.
- **Serialize an asset identifier with `ToString()`**, not with its unscoped id — the unscoped id
  is for emptiness checks only.
- **Give dates a value even when the administrator left them empty.** A "publish from" of the
  minimum date and a "publish until" of the maximum date keep the resource findable in the index;
  leaving them null does not.
- **`AllowedPortalTypes` has to agree with the portal template's `Type`**, or the resource cannot
  be placed in the portal it was written for.

## How do I write a task handler?

*Advanced.* A task handler is a **state machine** for the task framework — an approval, a
request, a review. It does not store the tasks; a data adapter publishing the task management
interfaces does that, and the handler names it.

```C#
public class RequestAccessApprovalTaskHandler : IRequestAccessTaskHandler
{
    // The states this task can be in, and the actions it offers. Static, because the startup
    // reads them for task lists without constructing a handler.
    public readonly static List<ITaskStateDescriptorModel> TaskStateDescriptors = new()
    {
        new TaskStateNewStateDescriptorModel(),
        new TaskStateApprovedStateDescriptorModel(),
        new TaskStateDeclinedStateDescriptorModel(),
        new TaskStateIgnoredStateDescriptorModel()
    };

    public readonly static List<ITaskActionDescriptorModel> TaskActionDescriptors = new()
    {
        new TaskActionApproveActionDescriptorModel(),
        new TaskActionDeclineActionDescriptorModel(),
        new TaskActionIgnoreActionDescriptorModel()
    };

    private readonly static List<ITaskActionDescriptorModel> NewStateActionDescriptors = TaskActionDescriptors;

    /// The transition table. `null` means "terminal state — nothing can be done here".
    public static List<ITaskActionDescriptorModel> GetTaskActionDescriptors(string taskState)
    {
        if (string.IsNullOrEmpty(taskState))
        {
            throw new ArgumentException("State is empty", nameof(taskState));
        }

        return taskState switch
        {
            TaskStateNewStateDescriptorModel.TaskStateNew => NewStateActionDescriptors,
            _ => null
        };
    }

    public Task<List<ITaskActionDescriptorModel>> GetTaskActionDescriptorsAsync(string taskState) =>
        Task.FromResult(GetTaskActionDescriptors(taskState));

    public Task<ITaskStateDescriptorModel> PerformTaskActionAsync(
        ITaskContextModel taskContextModel,
        string oldTaskState,
        string taskAction,
        FormFieldValuesModel oldFormFieldValuesModel,
        FormFieldValuesModel formFieldValuesModel,
        IProgressMonitor progressMonitor = null)
    {
        return oldTaskState switch
        {
            TaskStateNewStateDescriptorModel.TaskStateNew =>
                PerformNewStateActionAsync(taskAction, oldFormFieldValuesModel, formFieldValuesModel),
            _ => throw new NotImplementedException()
        };
    }

    private Task<ITaskStateDescriptorModel> PerformNewStateActionAsync(
        string taskAction,
        FormFieldValuesModel oldFormFieldValuesModel,
        FormFieldValuesModel formFieldValuesModel)
    {
        switch (taskAction)
        {
            case TaskActionApproveActionDescriptorModel.TaskActionApprove:
                // The values captured when the task was created.
                Reference = oldFormFieldValuesModel.Values
                    .First(value => string.Equals(value.Id, TaskInitialConfiguration.FormFieldReference,
                        StringComparison.InvariantCulture))
                    .StringValue;

                Done = true;

                return Task.FromResult<ITaskStateDescriptorModel>(new TaskStateApprovedStateDescriptorModel());

            case TaskActionDeclineActionDescriptorModel.TaskActionDecline:
                // The values of the action's own form.
                Reason = formFieldValuesModel.Values
                    .FirstOrDefault(value => string.Equals(value.Id, nameof(TaskDeclineConfiguration.Reason),
                        StringComparison.InvariantCulture))
                    ?.StringValue;

                Declined = true;

                return Task.FromResult<ITaskStateDescriptorModel>(new TaskStateDeclinedStateDescriptorModel());

            case TaskActionIgnoreActionDescriptorModel.TaskActionIgnore:
                return Task.FromResult<ITaskStateDescriptorModel>(new TaskStateIgnoredStateDescriptorModel());

            default:
                throw new NotImplementedException();
        }
    }

    // Usually nothing to do — a pre-action can add links, a post-action can send an email.
    public Task<TaskPreActionResultModel> PerformTaskPreActionAsync(
        ITaskContextModel taskContextModel, string taskState,
        FormFieldValuesModel formFieldValuesModel, bool isNew) =>
        Task.FromResult<TaskPreActionResultModel>(null);

    public Task<TaskPostActionResultModel> PerformTaskPostActionAsync(ITaskContextModel taskContextModel) =>
        Task.FromResult<TaskPostActionResultModel>(null);
}
```

The startup binds the handler to the connector and the task management data adapter, and exposes
the same transition table synchronously:

```C#
public class RequestAccessApprovalTaskHandlerStartup : ITaskHandlerStartup
{
    public const string TaskHandlerKey = "mypartner-request-access-approval";

    public string Type => TaskType.RequestAccess;
    public string ConnectorKey => MyConnectorStartup.MyConnector;
    public string DataAdapterKey => MyTasksDataAdapterStartup.MyTasksDataAdapter;
    public string Key => TaskHandlerKey;

    public List<ITaskStateDescriptorModel> TaskStateDescriptors =>
        RequestAccessApprovalTaskHandler.TaskStateDescriptors;
    public List<ITaskActionDescriptorModel> TaskActionDescriptors =>
        RequestAccessApprovalTaskHandler.TaskActionDescriptors;

    /// The action the creation form binds to — separate from the actions above.
    public ITaskActionDescriptorModel CreateTaskActionDescriptor { get; } =
        new TaskActionSubmitActionDescriptorModel();

    public bool AllowsAnonymousUsers => true;
    public bool AllowsRegisteredUsers => true;

    public List<ITaskActionDescriptorModel> GetTaskActionDescriptors(string taskState) =>
        RequestAccessApprovalTaskHandler.GetTaskActionDescriptors(taskState);

    public Type ComponentImplementation => typeof(RequestAccessApprovalTaskHandler);
    public Type ConfigurationImplementation => typeof(TaskInitialConfiguration);
    public Type ConfigurationMessages => typeof(ConfigurationMessages);
}
```

Pitfalls:

- **Ship the transition table twice** — as a `static` method the startup calls, and as the
  instance `…Async` one. The startup's copy runs for every row of a task list and must do no
  input or output.
- **`null` means terminal.** An empty list means "there are actions, but none right now", which
  renders differently.
- **Two form field value models arrive, and they are not the same.** The *old* one carries what
  was captured when the task was created; the other carries the current action's own form. Both
  are already validated by the framework — look values up by their string id.
- **The outcome is returned through properties, not the return value.** The method returns the
  new state; `Done`, `Declined`, `Reason` and the rest are declared by the specialised task
  handler interface and read off the instance afterwards.
- **The descriptor lists belong to the handler as `static readonly` fields.** The startup is a
  long-lived singleton and the handler is short-lived and stateful; pointing the singleton at
  instance state is a bug waiting for load.

## How do I build a portal template?

*Basic.* Almost always: **build the portal you want, then have Smint.io turn it into a template.**
A portal template component is then nearly empty — it names a real portal per environment and the
platform clones it.

```C#
public class MyPortalTemplate : BaseLivePortalTemplate
{
    private readonly MyPortalTemplateConfiguration _configuration;

    public MyPortalTemplate(MyPortalTemplateConfiguration configuration)
    {
        _configuration = configuration;
    }
}
```

```C#
[FormGroupDeclaration(SetupReservedFormGroupId)]
[FormGroupDisplayName(SetupReservedFormGroupId,
    nameof(ConfigurationMessages.pot_mypartner_media_gallery_setup_form_group_display_name))]
public class MyPortalTemplateConfiguration : PortalTemplateConfiguration
{
}
```

Everything that matters is on the startup: the portal type, the screenshots the administrator
picks from, and the portal to clone — **one identifier per environment**, because the same portal
has a different identity in each.

```C#
public string Type => PortalType.MediaGallery;
public const string MyPortalTemplate = "mypartner-media-gallery";
public string Key => MyPortalTemplate;

public string[] ScreenshotUrls { get; } = new[] { "https://cdn.example.com/my-template-main.png" };

public bool IsSmintedUi => false;
public bool IsDeprecated => false;

public long? LivePortalUuidProduction => null;      // filled in with Smint.io, per environment
public long? LivePortalUuidStaging => null;
public long? LivePortalUuidDevelopment => null;
```

Pitfalls:

- **Subclass `PortalTemplateConfiguration`; never implement `IComponentConfiguration` directly.**
  The base class carries the branding, the transactional email settings and the analytics
  configuration — a portal created from a template that skipped it comes up without them.
- **The setup form group id is reserved.** Declare it and give it a display name; do not invent
  your own name for the setup step.
- **The three environment identifiers are different numbers.** Reusing one across environments
  clones the wrong portal, or nothing.
- **`Type` has to match the resources you expect to work.** A resource that restricts itself to
  one portal type is only available in a template of that type.
- **`IsDeprecated` retires a template without breaking portals** already created from it — that
  is the way to withdraw one.

## How do I change stored state without breaking a rollout?

*Advanced.* During a rollout both versions of your component run at once, against the same
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

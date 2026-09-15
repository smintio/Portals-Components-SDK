Smint.io Portals data adapter public API interfaces
===================================================

Current version of this document is: 1.4.0 (as of 15th of September, 2026)

Which public API interfaces exist, what each one publishes, how you declare the ones your data
adapter supports, and how to publish an interface of your own.

This is the catalogue. Its companion,
[the asset data model](smintio-asset-data-model.md), describes the *objects* these methods carry
and is the document to read while you are filling them in;
[the connector meta-model](smintio-connector-metamodel.md) describes the schema that gives those
objects meaning. If you are writing a *UI component* that consumes these interfaces rather than
a data adapter that implements them, you want
[the frontend data adapter reference](../../Frontend/Legacy/docs/smintio-data-adapter-reference.md),
which lists the same surface as TypeScript.

1. [What a public API interface is](#user-content-what-a-public-api-interface-is)
1. [Two kinds of data adapter](#user-content-two-kinds-of-data-adapter)
1. [Declaring the ones you support](#user-content-declaring-the-ones-you-support)
1. [The catalogue](#user-content-the-catalogue)
1. [Assets](#user-content-assets)
1. [Collections, shares and the rest](#user-content-collections-shares-and-the-rest)
1. [Base classes: what you must implement](#user-content-base-classes-what-you-must-implement)
1. [Parameters and results](#user-content-parameters-and-results)
1. [Short-running and long-running methods](#user-content-short-running-and-long-running-methods)
1. [Permissions](#user-content-permissions)
1. [The data adapter's own configuration](#user-content-the-data-adapters-own-configuration)
1. [Configuration marker interfaces](#user-content-configuration-marker-interfaces)
1. [Getting to the connector's client](#user-content-getting-to-the-connectors-client)
1. [Keeping state the external system does not keep](#user-content-keeping-state-the-external-system-does-not-keep)
1. [Two kinds of security, and which one is yours](#user-content-two-kinds-of-security-and-which-one-is-yours)
1. [Custom public API interfaces](#user-content-custom-public-api-interfaces)
1. [Things that are easy to get wrong](#user-content-things-that-are-easy-to-get-wrong)

## What a public API interface is

A data adapter does not expose REST endpoints. It implements C# interfaces, and Smint.io Portals
publishes their methods over its own generic execution endpoint, resolving parameters and
results by reflection. **The interfaces are the API contract** — there is no separate OpenAPI
document that describes what a search returns.

Every public API interface derives from `IDataAdapterInterface`
(`SmintIo.Portals.SDK.Core.Models.Interfaces`), which carries a single member, the data adapter
context:

```C#
public interface IDataAdapterInterface
{
    public IDataAdapterContextModel Context { get; }
}
```

They live in `SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces`, one namespace per family.

Two things consume them. A **Smint.io Portals frontend or backend component** declares a
configuration property of the interface type, and the portal administrator points it at one of
the configured data adapters that publishes it. And the **portal itself** calls a fixed set of
them for the standard portal experience — asset search, asset detail, collections, downloads.

## Two kinds of data adapter

Which interfaces you implement is not really a menu choice — it follows from which of two things
you are building. Settle this before anything else; see
[productized or custom](../README.md#user-content-productized-or-custom).

**A productized data adapter** implements the **standard** interfaces, above all `IAssets` — the
main asset interaction interface. Its consumer is the generic portal: search pages, asset detail,
folder navigation, download, collections. Because generic code has to interpret what it returns,
everything in this document about `AssetDataObject`, permissions, feature support and paging
applies, and so does the whole of
[the connector meta-model](smintio-connector-metamodel.md).

**A custom data adapter** implements **its own** interfaces, published for one custom UI
component to call. Its consumer knows the data model at compile time, from the TypeScript
declaration generated off your C# types. Nothing generic ever interprets the payload, so:

- **no connector meta-model is needed** — the interface *is* the data model;
- `AssetDataObject` and the asset data model do not apply unless you choose to use them;
- there is no integration-layer decision, no feature support to declare, no `rawData`;
- the [shared test suite](smintio-backend-component-delivery.md#user-content-which-tests-to-write)
  does not apply either — write your own tests against the test driver.

What stays the same for both: the startup and configuration contracts, dependency injection from
the connector, the parameter and result contracts, identifier scoping, permissions, and the fact
that `PublicApiInterfaces` is the boundary.

Do not build the productized surface "in case". An `IAssets` implementation that exists so that
the component looks complete is a large amount of code with no consumer, and every method of it
is something a reviewer has to read.

## Declaring the ones you support

Your `IDataAdapterStartup` lists them:

```C#
public Type[] PublicApiInterfaces => new[] { typeof(IAssets) };
```

**This list is the boundary, not your class's interface list.** A method is callable only if it
is declared on one of the interfaces named here. Implementing `IAssetsFolderNavigation` on the
class but listing only `IAssetsSearch` means the folder methods exist in your assembly and are
unreachable from a portal.

Declare the interface you actually support, not the widest one you can make compile.

## The catalogue

| Family | Interfaces |
|---|---|
| **Assets** | `IAssets`, `IAssetsRead`, `IAssetsSearch`, `IAssetsFolderNavigation`, `IAssetsReadRandom`, `IAssetsDownload`, `IAssetsUpload` |
| **Resource assets** | `IResourceAssets`, `IResourceAssetsRead`, `IResourceAssetsSearch`, `IResourceAssetsReadRandom` |
| **Product assets** | `IProductAssets`, `IProductAssetsRead`, `IProductAssetsSearch`, `IProductAssetsReadRandom`, `IProductAssetsSearchRelated` |
| **Collections** | `ICollections`, `ICollectionsCreate`, `ICollectionsRead`, `ICollectionsSearch`, `ICollectionsUpdate`, `ICollectionsDelete`, `ICollectionsAssets` (`…Read`, `…Search`, `…Modify`), `ICollectionsUsers` (`…Read`, `…Search`, `…Modify`), `ICollectionsAndAssets` |
| **Shares** | `IShares`, `ISharesCreate`, `ISharesRead`, `ISharesSearch`, `ISharesUpdate`, `ISharesDelete` |
| **Resources** | `IResources`, `IResourcesRead`, `IResourcesSearch` |
| **Task management** | `ITaskManagement`, `ITaskManagementCreate`, `ITaskManagementRead`, `ITaskManagementSearch`, `ITaskManagementUpdate`, `ITaskManagementDelete` |
| **Templates** | `ITemplates`, `ITemplatesRead` |
| **AI** | `IAi`, `IAiSemanticAnalysis`, `IAiEmbeddings`, `ISmintedAi` |
| **External users** | `IExternalUsersRead` |
| **Webhooks** | `IWebhooksApiProvider` |

The pattern is the same throughout: the bare family name — `IAssets`, `ICollections`, `IShares`
— is a **composition** of the finer-grained ones, and the finer-grained ones inherit from each
other where one genuinely needs another. `IAssetsSearch` extends `IAssetsRead`, because a search
result is useless without the ability to read an asset. `IAssetsFolderNavigation` extends
`IAssetsSearch`. A method that arrives by more than one route is of course implemented once.

Interfaces whose name ends in `InternalApiProvider` are not part of the callable surface. They
declare methods the platform calls internally — binary streams, download item mappings — and
you implement them without listing them in `PublicApiInterfaces`.
`IAssetsIntegrationLayerApiProvider` is the equivalent for adapters that feed the Smint.io
integration layer, and `IWebhooksApiProvider` for adapters that receive callbacks from the
external system; both *are* listed, because the platform resolves an adapter by them.

## Assets

The family nearly every data adapter implements. `IAssets` composes `IAssetsRead`,
`IAssetsReadRandom` and `IAssetsSearch`.

**`IAssetsRead`** — asset detail.

| Method | |
|---|---|
| `GetAssetsReadFeatureSupportAsync` | declares whether fast bulk reads are supported |
| `GetAssetAsync` | one asset, by identifier |
| `GetAssetsAsync` | several assets — long-running |
| `GetResourceAssetAsync`, `GetResourceAssetsAsync` | the same against portal resource assets |

**`IAssetsSearch`** (: `IAssetsRead`) — search and facets.

| Method | |
|---|---|
| `GetFeatureSupportAsync` | random access, full-text proposals, folder navigation |
| `SearchAssetsAsync` | the search itself |
| `GetFullTextSearchProposalsAsync` | search-as-you-type proposals |
| `GetFormItemDefinitionAllowedValuesAsync` | values for a facet flagged as dynamic |
| `SearchResourceAssetsAsync`, `GetFullTextSearchProposalsResourceAssetsAsync`, `GetFormItemDefinitionAllowedValuesResourceAssetsAsync` | the same against resource assets |

**`IAssetsFolderNavigation`** (: `IAssetsSearch`) — the containment tree.

| Method | |
|---|---|
| `GetFolderAsync`, `GetFoldersAsync` | one folder, several folders (the plural is long-running) |
| `SearchFoldersAsync` | search within folders |
| `GetFolderContentsAsync` | the assets in a folder |

Like the download interface, this is the **portal-facing** contract and Smint.io implements it
across data adapters. A connector serves its folders by returning them from
`GetFolderContentsForIntegrationLayerAsync`, by producing `FolderDataObject`s in its converter,
and by reporting folder navigation in its search feature support.

**`IAssetsReadRandom`** (: `IAssetsRead`) — `GetRandomAssetsAsync`, `GetRandomResourceAssetsAsync`.
What a "surprise me" or rotating-hero component calls.

**`IAssetsDownload`** — downloads, in two steps. All five methods are long-running.

| Method | |
|---|---|
| `GetAssetsDownloadsForAssetsAsync`, `GetAssetsDownloadsForCollectionAsync` | what can be downloaded, grouped |
| `InitiateAssetsDownloadForAssetsAsync`, `InitiateAssetsDownloadForCollectionAsync` | produce the delivery URL |
| `PrepareTemplateEditorAsync` | hand off to a template editor for an editable rendition |

The bytes never pass through the Smint.io API: the second step returns a signed delivery URL
that the client follows directly.

**You almost certainly do not implement this one.** A download normally spans several data
adapters — the visitor selected assets from more than one source — so Smint.io composes the two
steps itself, applies the permission gates and delivers the file. A connector's data adapter
contributes the *options* for its own assets through `GetAssetsDownloadItemMappingsAsync`, and
serves the bytes through `GetAssetDownloadStreamAsync`. See
[offering downloads](smintio-backend-recipes.md#user-content-how-do-i-offer-downloads).

**`IAssetsUpload`** — `GetAssetUploadSettingsAsync`. Requires the upload permission. The settings
come from your configuration class through the base implementation; the method you actually write
is `HandleAssetUploadsAsync`, which receives the uploaded files as references and puts them into
the source system. See [accepting uploads](smintio-backend-recipes.md#user-content-how-do-i-accept-uploads).

**`IAssetsInternalApiProvider`**, brought in by `IAssetsRead` and therefore implemented by every
asset adapter, is where the binaries are actually served:
`GetAssetThumbnailDownloadStreamAsync`, `GetAssetDownloadStreamAsync`,
`GetFolderThumbnailDownloadStreamAsync` and `GetAssetsDownloadItemMappingsAsync`.

**Resource assets and product assets** are the same methods against a different population.
`IResourceAssetsSearch` and `IProductAssetsSearch` both extend `IAssetsSearch` and add nothing
of their own except, for products, `IProductAssetsSearchRelated.SearchProductAssetsForAssetsAsync`
— the products belonging to a given set of assets.

## Collections, shares and the rest

| Interface | What it publishes |
|---|---|
| `ICollectionsRead` / `…Search` | read a collection and its comments; search collections, with filter and sort values |
| `ICollectionsCreate` / `…Update` / `…Delete` | the lifecycle of a collection, plus commenting on it |
| `ICollectionsAssetsRead` / `…Search` / `…Modify` | the assets inside a collection: read comments, search, add, remove, move, copy, flag, comment, notify |
| `ICollectionsUsersRead` / `…Search` / `…Modify` | who a collection is shared with: invitations, eligible users and user groups |
| `ISharesCreate` / `…Read` / `…Search` / `…Update` / `…Delete` | shares — a set of assets published to someone outside the portal |
| `IResourcesRead` / `…Search` | portal resources, and `IResourcesUrlProvider` for their download URL and stream |
| `ITaskManagementRead` / `…Search` / `…Create` / `…Update` / `…Delete` | the task framework's data side. Read [task handlers](smintio-task-handlers.md) for the state machines themselves |
| `ITemplatesRead` | template support for an asset, preparing a template editor, and the rendered download stream |
| `IAiSemanticAnalysis` | `GetFileAnalysisAsync`, `GetSemanticAnalysisAsync` |
| `IAiEmbeddings` | vector embeddings for semantic and natural-language search |
| `IExternalUsersRead` | `GetUserGroupMembershipAsync` — resolves a portal user's groups in the external system, which is how external group membership drives portal permissions |
| `IWebhooksApiProvider` | `ValidateWebhookAsync` — validates an inbound callback before the platform acts on it |

Search-shaped interfaces follow one convention throughout: a `Search…Async` paired with
`Get…FilterValuesAsync` and `Get…SortValuesAsync`, so the client can build the filter and sort
controls from the adapter rather than from hard-coded assumptions.

## Base classes: what you must implement

`SmintIo.Portals.DataAdapterSDK.DataAdapters.Impl` gives you three starting points:

| Base class | Use it for |
|---|---|
| `DataAdapterBaseImpl` | any data adapter. Implements `IDataAdapter`, resolves the context |
| `AssetsDataAdapterBaseImpl` (: `DataAdapterBaseImpl`, `IAssets`) | an asset adapter |
| `AssetsUploadDataAdapterBaseImpl` | an upload adapter |

`AssetsDataAdapterBaseImpl` is where most of the work is. It leaves **eleven abstract members**
for you, and these are exactly the list of "what a new asset data adapter has to write":

```C#
GetFeatureSupportAsync              GetAssetsReadFeatureSupportAsync
GetAssetAsync                       GetAssetsAsync
GetRandomAssetsAsync                SearchAssetsAsync
GetFullTextSearchProposalsAsync     GetFormItemDefinitionAllowedValuesAsync
GetAssetDownloadStreamAsync         GetAssetThumbnailDownloadStreamAsync
GetFolderThumbnailDownloadStreamAsync
```

Everything else it supplies with a working default, and every one of those defaults is `virtual`
so you can take it over. The ones worth knowing about:

- `GetAssetsDownloadItemMappingsAsync` builds the download options for an asset out of the
  output formats your configuration allows, using the `Has…OutputFormat` and `OutputFormatApplies`
  helpers. Override `GetCustomAssetDownloadItemMappingsAsync` to add your own rather than
  replacing the whole method.
- `IsHiRes` / `HiResIsConfigured` decide which downloads need the high-resolution permission.
- The `…ResourceAssets…` methods throw by default. Override them only if your adapter serves
  resource assets.

**Split the implementation across partial class files.** It is not required and it is what every
shipped adapter does: one file per capability, in folders named `Search`, `Read`, `Random`,
`Download` and `IntegrationLayer`, with the constructor and the feature-support methods in the
file named after the class. Converters and other helpers go in `Common`.

```
Assets/
  MyAssetsDataAdapter.cs              partial class — constructor, feature support
  MyAssetsDataAdapterStartup.cs
  MyAssetsDataAdapterConfiguration.cs
  Search/MyAssetsSearch.cs            partial class — SearchAssetsAsync, proposals, facets
  Read/MyAssetsRead.cs                partial class — GetAssetAsync, streams
  Random/MyAssetsReadRandom.cs        partial class — GetRandomAssetsAsync
  Common/MyContentConverter.cs        source payload -> AssetDataObject
Resources/
  ConfigurationMessages.resx
```

Every file that continues the partial class has to repeat the **same** class signature,
interface list included. Adding an interface in one file and not the others will not compile —
which is the good case. The bad case is adding it to the class and forgetting
`PublicApiInterfaces`.

## Parameters and results

Every method takes exactly one parameter object and returns exactly one result object.

```C#
public interface IDataAdapterParameterObject
{
    void UnscopeIdentifiers(IIdentifierUnscopeHelper identifierUnscopeHelper);
}

public interface IDataAdapterResult
{
    void ScopeIdentifiers(IIdentifierScopeHelper identifierScopeHelper);
    Task PrepareWireFormatAsync(IWireFormatPreparationHelper wireFormatPreparationHelper);
}
```

Those three members are how identifiers are scoped on the way out and unscoped on the way in.
**Your adapter never sees a scoped identifier and must never produce one** — it works purely in
the external system's own identifiers. See
[identifiers are scoped for you](smintio-asset-data-model.md#user-content-identifiers-are-scoped-for-you).

Ready-made envelopes for the common shapes are in
`SmintIo.Portals.DataAdapterSDK.DataAdapters.Prefab`: `AssetIdParameters`, `AssetIdsParameters`,
`AssetResult`, `AssetsResult`, and the folder and resource equivalents. Derive from these rather
than implementing the three members yourself — they get the scoping right.

## Short-running and long-running methods

A method that additionally takes an `IProgressMonitor` is **long-running**. The platform queues
it, returns immediately, and the caller polls for the result. A method without one is executed
inline and its payload comes back with the first response.

```C#
Task<GetAssetsResult> GetAssetsAsync(GetAssetsParameters parameters, IProgressMonitor progressMonitor);
```

Report progress as you go — `ReportProgressIncrementAsync`, `ReportProgressAbsoluteAsync`,
`FinishedAsync`, each taking a `LocalizedStringsModel` to display. A long-running method that
reports nothing looks identical to one that has hung.

You do not choose which of your methods are long-running: the signature on the interface decides,
and the standard interfaces have already decided. `GetAssetsAsync`,
`GetAssetsDownloadsForAssetsAsync` and `InitiateAssetsDownloadForAssetsAsync` are long-running;
`SearchAssetsAsync` and `GetAssetAsync` are not.

**A method you do not support should throw `NotImplementedException`.** The platform turns that
into a clean "method not supported" answer. Returning an empty result instead is worse for
everyone: the portal cannot tell "nothing found" from "not available here", and neither can you
when a customer reports it.

## Permissions

The standard interfaces already carry `[RequiredPermissions(...)]` on their methods, naming
constants from `SmintIo.Portals.DataAdapterSDK.DataAdapters.Permissions.DataAdapterPermission`:

| Constant | |
|---|---|
| `SearchAssetsPermissionUuid` | search assets |
| `ReadAssetDetailsPermissionUuid` | read asset details |
| `DownloadAssetLayoutFilesPermissionUuid` | download layout files |
| `DownloadAssetsHiResPermissionUuid` | download high-resolution assets |
| `FlagAssetsPermissionUuid`, `CommentAssetsPermissionUuid` | flag, comment |
| `ShareAssetsPermissionUuid` and its four refinements | share, share with anonymous users, flag, comment, remove assets from a share |
| `CollectionBrowseUserGroupsPermissionUuid`, `CollectionBrowseUsersPermissionUuid` | browse users and groups when sharing a collection |
| `UploadAssetsPermissionUuid` | upload |

So an adapter that implements only standard interfaces declares **no permissions of its own**:

```C#
public DataAdapterPermission[] Permissions => null;
```

That is correct and it is what nearly every shipped adapter does. Do not re-declare the standard
permission UUIDs — the platform already knows them.

Declare permissions only when you publish a **custom** public API interface with operations the
standard set does not describe. Then it is three steps, all of which are required:

```C#
// 1. the permission
public class MyDataAdapterPermission
{
    public const string ReadProductDataPermissionUuid = "read_product_data";

    public static readonly DataAdapterPermission ReadProductData = new()
    {
        Uuid = ReadProductDataPermissionUuid,
        Name = new ResourceLocalizedStringsModel(nameof(ConfigurationMessages.da_myadapter_read_product_data))
    };
}

// 2. on the interface method
[RequiredPermissions(MyDataAdapterPermission.ReadProductDataPermissionUuid)]
Task<ReadProductResult> ReadProductAsync(ReadProductParameters parameters);

// 3. on the startup, so the administrator can grant it
public DataAdapterPermission[] Permissions => new[] { MyDataAdapterPermission.ReadProductData };
```

Miss step 3 and the permission exists but cannot be granted, so the method is permanently
denied.

Permissions are also carried per object: set `PermissionUuids` on each asset and folder your
converter produces. The interface-level check passing is not enough — an asset with no
permissions on it has its actions hidden in the portal, silently.

## The data adapter's own configuration

**A data adapter has its own configuration class, exactly as a connector does** — a plain class
implementing `IComponentConfiguration`, pointed at by `ConfigurationImplementation` on your
`IDataAdapterStartup`, rendered as its own form when an administrator configures the adapter and
injected into the constructor. Two kinds of thing go into it: the marker interfaces below, and
**properties of your own**.

The properties of your own are the part most often left out, with everything pushed onto the
connector instead. Anything only this adapter interprets belongs here — a customer-specific
setting, a limit, a naming scheme, a behaviour one customer wants switched off, or the signing
key for [a signed link](#user-content-two-kinds-of-security-and-which-one-is-yours). The
HelloWorld adapter's `MultiSelectItemCount` is exactly that: an ordinary annotated property
sitting next to the marker interface members.

Why it matters which component carries the setting: one connector can serve several data
adapters, so a property on the connector forces the same value on all of them, while a property
on the adapter is set once per configured adapter. Configurations are stored encrypted at rest,
so a secret is a legitimate property here — see
[which component's configuration a setting belongs in](smintio-backend-annotations.md#user-content-which-components-configuration-a-setting-belongs-in)
and [configuration is stored encrypted](smintio-backend-annotations.md#user-content-configuration-is-stored-encrypted).

## Configuration marker interfaces

Your configuration class opts into platform behaviour by implementing marker interfaces from
`SmintIo.Portals.DataAdapterSDK.DataAdapters.Configurations`. They are mixins — implement as
many as apply, there is no base class.

| Interface | What it adds |
|---|---|
| `IOutputFormatDataAdapterConfiguration` | the output format allow-lists that drive download options and the high-resolution permission |
| `IPreserveMetadataDataAdapterConfiguration` | the metadata attributes an administrator marks to preserve. **Without this, raw metadata is dropped from search results entirely** — see [raw data](smintio-asset-data-model.md#user-content-raw-data-and-what-survives-a-search) |
| `IAlwaysOnIntegrationLayerDataAdapterConfiguration` | the adapter is always indexed through the Smint.io integration layer |
| `IOptionalIntegrationLayerDataAdapterConfiguration` | indexing through the integration layer is an administrator's choice |
| `IAssetsUploadDataAdapterConfiguration` | upload settings |
| `IAiDataAdapterConfiguration` | AI-backed search settings |

The two integration-layer interfaces are the switch between the two integration modes: a **live
connection**, where every request goes to the external system, and an **indexed** one, where
Smint.io analyses and indexes the content and serves searches from its own index. Choose the
live connection when the external system can do faceted search and translated metadata well;
choose the index when it cannot, or when it is too slow to sit in front of a portal.

## Getting to the connector's client

The connector registers the client; the data adapter receives it by constructor injection.

```C#
// in the connector
public override void ConfigureServicesForDataAdapter(ServiceCollection services)
{
    services.AddTransient(_ => EnsureMyClient());     // registers IMyClient
}

// in the data adapter
public MyAssetsDataAdapter(
    ILogger logger,
    IServiceProvider serviceProvider,
    IMyClient myClient,                               // resolved from the connector
    MyAssetsDataAdapterConfiguration configuration)
    : base(serviceProvider)
{
    _logger = logger;
    _myClient = myClient;
    _configuration = configuration;

    _entityModelProvider = serviceProvider.GetService<IEntityModelProvider>();
    _portalsContextModel = serviceProvider.GetService<IPortalsContextModel>();
}
```

The logger, the service provider and your own configuration instance are always available. The
platform's own services — the entity model provider, the portals context, the integration layer
provider, persistent storage — are pulled off the injected `IServiceProvider` rather than taken
as constructor parameters. That is the idiom throughout; follow it.

**The client is how you call the external system, and it must not expose any secrets.** Tokens,
refresh tokens, client secrets and API keys stay inside the connector and the client's own
implementation — your data adapter should never hold one. If you find yourself wanting the
client to hand you a token or an authorization header, add the operation you actually need to
the client interface instead. See
[the client must not expose secrets](smintio-connector-reference.md#user-content-the-client-must-not-expose-secrets).

**`ConfigureServices` on the data adapter startup is not the place for this.** It exists, and in
practice every shipped adapter leaves it empty. Registering your client there instead of in the
connector's `ConfigureServicesForDataAdapter` produces a dependency injection failure at runtime
with nothing to see at compile time.

## Keeping state the external system does not keep

Some interactions need to remember something the external system has no field for — that a
one-time action has already been performed, that a step was completed, what a caller was shown
last time. Two services off the injected `IServiceProvider` cover this:

| | |
|---|---|
| `IIdPersistentStorage` | keyed records. `GetAsync(uuid)`, `AddOrUpdateAsync(uuid, data, groupUuid)`, `GetGroupAsync`, `RemoveAsync` and their bulk forms, over an `IdPersistentStorageData { Uuid, GroupUuid, Data }` |
| `ITemporalPersistentStorage` | an append-ordered log. `AddAsync` returns the identifier, `GetRangeAsync(lastKnownId, pageSize)` reads forward from one |

Use the keyed store when you have an identifier from the external system to key on, and the
temporal one when you need to replay a sequence in order. Neither is a cache — the cache is for
that, and persistent storage is not the way to avoid a call you could simply make.

Do not keep this state in a field of the external system instead. A status text, a comment or a
description field belongs to that system's own processes: a workflow there reads it, a user edits
it, and your marker is gone without anything failing loudly.

## Two kinds of security, and which one is yours

The data adapter never authenticates to the external system — that is the connector's job, and
the client it hands you must not expose a credential. It is easy to read that as "security does
not belong in a data adapter". There is a second kind that does.

When your component is reached through a **link handed to someone outside the portal** — a
tokenised URL granting one person one action on one object for a limited time — validating that
link is the *data adapter's* responsibility, and nothing else validates it for you. Verify the
signature before anything else, reject an expired link, and treat every value carried in it as
untrusted until the signature has checked out. In particular, never read an object identifier out
of a link and fetch it before verifying the signature over it.

Keep signing and verification together even when only the verifier ships: you need the signer to
test the verifier, and a test that mints its links through the same code is the only way to know
the two halves agree.

**The signing key is a property of the data adapter's own configuration**, not of the
connector's — it is this adapter's secret, it has nothing to do with authenticating to the
external system, and it is often different per customer. Configurations are stored encrypted at
rest and a saved secret is never displayed back to the administrator, so putting it there is
safe — **provided you name the property so that it is recognised as a secret**, which a name
containing `key`, `secret` or `password` is. See
[configuration is stored encrypted](smintio-backend-annotations.md#user-content-configuration-is-stored-encrypted)
and [the data adapter's own configuration](#user-content-the-data-adapters-own-configuration).

## Custom public API interfaces

If the standard interfaces do not describe what your component needs, publish your own. The
platform treats it exactly like a standard one — including the permission check, the parameter
and result handling, and the generated TypeScript for a UI component to call it.

**Your interface is the data model.** The consumer is a UI component that deserializes against
the TypeScript declaration generated from these C# types, so the parameter and result classes
are a contract in both directions — and **no meta-model describes them, because none is needed**.
Design them as you would any published API: name the fields for the consumer rather than for the
external system, keep them stable, and treat renaming or retyping a field as the breaking change
it is.

You need four things:

```C#
// 1. the interface
public interface IMyProductData : IDataAdapterInterface
{
    [RequiredPermissions(MyDataAdapterPermission.ReadProductDataPermissionUuid)]
    Task<ReadProductResult> ReadProductAsync(ReadProductParameters parameters);
}

// 2. the parameter object
public class ReadProductParameters : IDataAdapterParameterObject
{
    public string ProductCode { get; set; }

    public void UnscopeIdentifiers(IIdentifierUnscopeHelper helper) { }
}

// 3. the result object
public class ReadProductResult : IDataAdapterResult
{
    public LocalizedStringsModel Title { get; set; }

    public void ScopeIdentifiers(IIdentifierScopeHelper helper) { }
    public Task PrepareWireFormatAsync(IWireFormatPreparationHelper helper) => Task.CompletedTask;
}

// 4. the declaration
public Type[] PublicApiInterfaces => new[] { typeof(IAssets), typeof(IMyProductData) };
```

Compose several small interfaces into one umbrella interface when the surface grows, the way
`IAssets` composes `IAssetsRead`, `IAssetsSearch` and `IAssetsReadRandom`. It lets a component
configuration ask for exactly the part it needs.

If your parameters or results carry asset, folder or resource identifiers, implement
`UnscopeIdentifiers` and `ScopeIdentifiers` rather than leaving them empty — or derive from the
`Prefab` envelopes, which do it for you.

To call the interface from a **UI component**, generate its TypeScript definition from the built
assembly with the
[Data Adapter Exporter CLI](../../Tools/Portals-DataAdapter-SDK-DataAdapterExporter-CLI/Release/):

```console
SmintIo.Portals.DataAdapterSDK.DataAdapterExporter.CLI.exe -s MyDataAdapter.dll -t .\IMyProductData.ts
```

Re-run it whenever the C# interface changes; the `.ts` file is generated output and is never
edited by hand. It can be published as an npm package if several components use it.

### Calling another interface from your own component

When a backend component holds a configuration property of a public API interface type, do not
invoke the method on it directly. Route it through the execution wrapper, which performs the
permission checks and any configured data processing:

```C#
var searchAssetsResult = await _portalsContext.PublicApiInterfaceExecutionWrapper
    .WrapPublicApiInterfaceExecutionAsync<SearchAssetsParameters, SearchAssetsResult, IAssetsSearch>(
        _configuration.SearchBarAutoCompletion,
        nameof(IAssetsSearch.SearchAssetsAsync),
        parameters)
    .ConfigureAwait(false);
```

From a **UI component** there is nothing to do: the wiring from the frontend to the backend is
generated for you, and you call the method on the injected interface.

## Things that are easy to get wrong

- **`PublicApiInterfaces` is the boundary.** An interface you implement but do not list is
  unreachable, with no error anywhere.
- **Do not widen an interface to make one method reachable.** Declaring `IAssets` when you only
  support search means the portal will call methods you have not written.
- **Do not implement the standard asset surface for a custom component.** If the only consumer is
  your own UI component, `IAssets` and the meta-model behind it have no purpose.
- **Never let a credential out through the client.** The data adapter calls the external system
  through the client; it does not authenticate to it.
- **But do validate a link your component was reached by.** Authenticating to the external system
  is the connector's job; verifying a signed, time-limited link handed to an outside party is
  yours, and nothing else does it — see
  [two kinds of security](#user-content-two-kinds-of-security-and-which-one-is-yours).
- **Do not stash your component's state in a field of the external system.** A status text or a
  comment field belongs to that system's processes and will be overwritten. Use
  [persistent storage](#user-content-keeping-state-the-external-system-does-not-keep).
- **Throw `NotImplementedException` for what you do not support**, and make the feature-support
  methods agree with it. A feature flagged `true` and then unimplemented is the worst of both.
- **`Permissions => null` is right for standard interfaces.** Only a custom interface needs
  custom permissions — and then all three steps.
- **Set `PermissionUuids` on every asset and folder** your converter produces, or the portal
  hides the actions.
- **Register the external client in the connector**, not in the data adapter's
  `ConfigureServices`.
- **Keep the partial class signature identical in every file.**
- **Ship a `ConfigurationMessages.resx`.** Your startup's `Name` and `Description` come from it.
  `MetamodelMessages` is the opposite — add it only when you actually ship one.
- **Well-known fields go on `AssetDataObject` directly; only source-specific metadata goes
  through the converter into `rawData`.** Pushing a name or a date through the converter is a
  common and confusing mistake.

## Questions

Please do not hesitate to contact us at [support@smint.io](mailto:support@smint.io) if you run
into any issues.

Contributors
============

- Reinhard Holzner, Smint.io GmbH

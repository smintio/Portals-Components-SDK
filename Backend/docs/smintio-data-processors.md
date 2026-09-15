Smint.io Portals data processors
===============================

Current version of this document is: 1.0.0 (as of 15th of September, 2026)

A data processor hooks into a data adapter's operations and changes what goes in or what comes
out. It is the component type to reach for when the data is right but its *shape*, its *naming*
or its *reachability* is not — and when changing the data adapter itself would be wrong, because
the adjustment belongs to one customer, one portal or one workflow rather than to the
integration.

Data processors are configured and attached by a Smint.io Portals administrator, not wired in
code. The same processor can be attached to several data adapter configurations, and a data
adapter can have several processors attached, each with its own configuration and priority.

1. [What a data processor is for](#user-content-what-a-data-processor-is-for)
1. [The contract](#user-content-the-contract)
1. [Data processor types](#user-content-data-processor-types)
1. [Lifecycle phases — the hook points](#user-content-lifecycle-phases--the-hook-points)
1. [Writing one](#user-content-writing-one)
1. [How a processor is attached](#user-content-how-a-processor-is-attached)
1. [Capability interfaces](#user-content-capability-interfaces)
1. [Privileged processors](#user-content-privileged-processors)
1. [Things that are easy to get wrong](#user-content-things-that-are-easy-to-get-wrong)

## What a data processor is for

The kinds of thing the shipped processors do, as a guide to what the type is good at:

- rename a downloaded file or folder from the asset's own metadata, or from a Liquid template an
  administrator writes;
- override the displayed name of an asset, or add an indicator to it, without touching the
  source system;
- inject or adjust metadata attributes after an asset is read;
- assemble several assets into one generated download, such as a presentation;
- add columns to a download-event report;
- narrow or redirect an asset search according to rules that are not expressible as a filter.

Two things they are *not* for. They are not a place to work around a data adapter that returns
the wrong data — fix the adapter. And they are not a substitute for the connector meta-model: a
field that was never declared has already been dropped before any processor runs.

## The contract

```C#
namespace SmintIo.Portals.DataProcessorSDK.DataProcessors
{
    public interface IDataProcessor : IDataProcessorComponent { }    // marker

    public interface IDataProcessorStartup : IComponentStartup
    {
        string Type { get; }                  // one of the DataProcessorType constants
        string IllustrationUrl { get; }
        string MdiIcon { get; }
        bool IntegrationLayerOnly { get; }    // only offered for indexed data sources
        Type MetamodelMessages { get; }
    }
}
```

`IDataProcessorComponent` (`SmintIo.Portals.SDK.Core.Components`) carries the processor's
context, `IDataProcessorContextModel`.

`IDataProcessor` itself declares **no methods**. A processor that implemented nothing else would
be a valid component that never runs. What makes it do something is the lifecycle phase
interfaces it implements — see below.

Derive your implementation from `DataProcessorBaseImpl`
(`SmintIo.Portals.DataProcessorSDK.DataProcessors.Impl`), which takes an `IServiceProvider` and
resolves the context for you.

Everything else about the component is the ordinary component shape: a `Key`, a `Name` and a
`Description` from your `ConfigurationMessages` resource file, an `IComponentConfiguration`
implementation carrying the settings the administrator fills in, and
[the annotations](smintio-backend-annotations.md) that describe them.

`IllustrationUrl` and `MdiIcon` are what the administrator sees when picking a processor from
the list. Supply at least the icon. Set `IntegrationLayerOnly` to `true` when your processor
only makes sense against a data source that Smint.io indexes, rather than one queried live.

## Data processor types

`Type` classifies the processor for the administrator. The values are constants on
`SmintIo.Portals.DataProcessorTypeSDK.DataProcessorTypes.Models.DataProcessorType`:

| Constant | Value | For |
|---|---|---|
| `MetadataTransformation` | `data-processor-type-metadata-transformation` | changing metadata, names, download shapes — most processors |
| `Scripting` | `data-processor-type-scripting` | processors driven by administrator-supplied rules |
| `Templates` | `data-processor-type-templates` | template editing and rendering |

The type is a classification only. It does not change which hooks you may implement, and there
is no runtime component behind it.

## Lifecycle phases — the hook points

A processor opts into a point in the pipeline by implementing the matching interface from
`SmintIo.Portals.DataProcessorSDK.DataProcessors.LifecyclePhases`. Each declares a single
`ProcessAsync` overload. Implement as many as apply.

The general pattern is a **`Prepare…` phase that receives the parameters** before the data
adapter method runs, and a **`Post…` phase that receives the parameters and the result**
afterwards. `Prepare` is where you change what is asked for; `Post` is where you change what
came back. Both mutate the object handed to them.

**The generic pair — any method**

| Interface | Receives |
|---|---|
| `IPrepareMethodDataProcessorLifecyclePhase` | the public API interface type, the method name, the parameter object |
| `IPostMethodDataProcessorLifecyclePhase` | the same, plus the result |

Use these when you want to act on many methods at once. Prefer a specific pair when you can —
they hand you typed objects rather than the base interfaces.

**Asset search and read**

| Prepare | Post |
|---|---|
| `IPrepareAssetSearchDataProcessorLifecyclePhase` | `IPostAssetSearchDataProcessorLifecyclePhase` |
| `IPrepareGetAssetDataProcessorLifecyclePhase` | `IPostGetAssetDataProcessorLifecyclePhase` |
| `IPrepareGetAssetsDataProcessorLifecyclePhase` | `IPostGetAssetsDataProcessorLifecyclePhase` |
| `IPrepareGetRandomAssetsDataProcessorLifecyclePhase` | `IPostGetRandomAssetsDataProcessorLifecyclePhase` |
| `IPrepareGetFullTextSearchProposalsDataProcessorLifecyclePhase` | `IPostGetFullTextSearchProposalsDataProcessorLifecyclePhase` |
| `IPrepareGetFormItemDefinitionAllowedValuesDataProcessorLifecyclePhase` | `IPostGetFormItemDefinitionAllowedValuesDataProcessorLifecyclePhase` |

**Downloads**

| Prepare | Post |
|---|---|
| `IPrepareGetAssetsDownloadItemMappingsDataProcessorLifecyclePhase` | `IPostGetAssetsDownloadItemMappingsDataProcessorLifecyclePhase` |
| `IPrepareGetAssetsDownloadsForAssetsDataProcessorLifecyclePhase` | `IPostGetAssetsDownloadsForAssetsDataProcessorLifecyclePhase` |
| `IPrepareGetAssetsDownloadsForCollectionDataProcessorLifecyclePhase` | `IPostGetAssetsDownloadsForCollectionDataProcessorLifecyclePhase` |

`IPostGetAssetDownloadStreamDataProcessorLifecyclePhase` is the one that returns a value rather
than mutating: it receives the asset, a folder name, the download stream and the output format
id, and returns the `AssetDownloadStreamModel` to use. **This is the hook for renaming a
download or substituting its content.**

`ICreateCompositeAssetDataProcessorLifecyclePhase` receives the download item mappings for a
whole set of assets and returns one stream — the hook for generating a single file out of many.

**Templates**

`IPrepareTemplateEditorDataProcessorLifecyclePhase` (two overloads, by download stream or by
template id) and `IRenderTemplateDataProcessorLifecyclePhase` (likewise), for a processor that
backs an editable rendition.

**Two phases outside the request path**

| Interface | When |
|---|---|
| `IPreIntegrationLayerAssetIngestionDataProcessorLifecyclePhase` | an asset is about to be indexed by the Smint.io integration layer. Receives the `AssetDataObject` |
| `IPostGetConnectorMetamodelDataProcessorLifecyclePhase` | a connector meta-model has just been built. Receives the `ConnectorMetamodel`, and may add or alter entities |

The ingestion phase is the one to use when a change must be reflected in the **search index**
rather than only in what a single request returns. A `Post` phase on a read does not reach the
index, so metadata you add there is not searchable.

**Reports**

`IPrepareDownloadEventCsvReportLifecyclePhase` receives an asset and the report result, for
adding columns to a download-event export.

## Writing one

```C#
public class MyDownloadNamingDataProcessor : DataProcessorBaseImpl,
    IPostGetAssetDownloadStreamDataProcessorLifecyclePhase
{
    private readonly MyDownloadNamingDataProcessorConfiguration _configuration;
    private readonly ILogger _logger;

    public MyDownloadNamingDataProcessor(
        ILogger logger,
        IServiceProvider serviceProvider,
        MyDownloadNamingDataProcessorConfiguration configuration)
        : base(serviceProvider)
    {
        _logger = logger;
        _configuration = configuration;
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
        // change assetDownloadStreamModel.FileName, or return it unchanged
        return Task.FromResult(assetDownloadStreamModel);
    }
}
```

The startup is the ordinary shape:

```C#
public class MyDownloadNamingDataProcessorStartup : IDataProcessorStartup
{
    public const string MyDownloadNamingDataProcessor = "my-download-naming";

    public string Key => MyDownloadNamingDataProcessor;
    public string Type => DataProcessorType.MetadataTransformation;

    public LocalizedStringsModel Name =>
        new ResourceLocalizedStringsModel(nameof(ConfigurationMessages.dp_my_download_naming_name));
    public LocalizedStringsModel Description =>
        new ResourceLocalizedStringsModel(nameof(ConfigurationMessages.dp_my_download_naming_description));

    public string LogoUrl => null;
    public string IconUrl => null;
    public string IllustrationUrl => null;
    public string MdiIcon => "mdiFileDocumentEdit";

    public bool IntegrationLayerOnly => false;

    public Type ConfigurationImplementation => typeof(MyDownloadNamingDataProcessorConfiguration);
    public Type ComponentImplementation => typeof(MyDownloadNamingDataProcessor);
    public Type ConfigurationMessages => typeof(Resources.ConfigurationMessages);
    public Type MetamodelMessages => null;

    public void ConfigureServices(IServiceCollection services) { }
}
```

A processor instance is **short-lived**: the platform constructs one per invocation, with your
configuration injected. Do not hold state between calls.

## How a processor is attached

Nothing in your code decides which data adapters your processor runs against. An administrator
creates a *data processor configuration* — an instance of your processor with its settings
filled in, a priority, and an enabled flag — and attaches it to one or more data adapter
configurations.

Consequences worth planning for:

- **Several processors can be attached to the same data adapter**, and they run in priority
  order. Your processor may receive a result another processor has already changed, and may have
  its own output changed after it.
- **The same processor can be attached twice with different settings.** Nothing about an
  instance is global.
- **Your processor may be attached to a data adapter you have never seen.** Write against the
  `AssetDataObject` contract, not against the conventions of one integration. Check for the
  absence of what you need and leave the object alone rather than throwing.

## Capability interfaces

Two interfaces in `SmintIo.Portals.DataProcessorSDK.DataProcessors.Interfaces.Templates` are not
lifecycle phases but declarations of what a processor can do, asked before it is used:

| Interface | Method |
|---|---|
| `ITemplatesDataProcessor` | `GetTemplateSupportAsync(AssetDataObject)` — can this asset be template-edited? |
| `ICompositeDownloadsDataProcessor` | `GetCompositeDownloadsSupportAsync(string groupId)` — can this download group be assembled into one file? |

## Privileged processors

`IPrivilegedDataProcessor` adds `SetPrivilegedServiceProvider(IServiceProvider)`, giving the
processor a service provider with access beyond the ordinary component sandbox. It exists for
processors that have to reach across the platform rather than only transform what they were
handed.

Do not implement it unless you have agreed the need with Smint.io. A processor that works
without it should not ask for it.

## Things that are easy to get wrong

- **A processor with no lifecycle phase interface never runs.** `IDataProcessor` alone declares
  nothing. This compiles, deploys, appears in the administrator's list, and does nothing.
- **`Prepare` changes the request, `Post` changes the response.** Trying to filter results in a
  `Prepare` phase, or to widen a search in a `Post` phase, is the most common structural mistake.
- **A `Post` phase does not reach the search index.** If the change has to be searchable, use
  `IPreIntegrationLayerAssetIngestionDataProcessorLifecyclePhase` instead — and note it applies
  only to indexed data sources.
- **Return the model from `IPostGetAssetDownloadStreamDataProcessorLifecyclePhase`**, including
  when you change nothing. Returning `null` loses the download.
- **Do not assume you are the only processor.** Read the value you are about to overwrite, and
  say in a comment what you assume about ordering.
- **Prefer a specific phase over the generic `…MethodDataProcessorLifecyclePhase` pair.** The
  generic one hands you base types and a method name string; the specific one hands you the
  typed parameters and result.
- **Handle the missing case quietly.** A processor attached to an unexpected data adapter should
  leave the object untouched, not throw — it is on the request path of a live portal.

## Questions

Please do not hesitate to contact us at [support@smint.io](mailto:support@smint.io) if you run
into any issues.

Contributors
============

- Reinhard Holzner, Smint.io GmbH

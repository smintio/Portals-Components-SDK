The Smint.io Portals asset data model
=====================================

Current version of this document is: 1.0.0 (as of 11th of September, 2026)

What a Smint.io Portals data adapter has to return, and what the frontend does with it.

The data adapter public API interfaces — `IAssetsRead`, `IAssetsSearch`, `IAssetsFolderNavigation`,
`IAssetsDownload` and their neighbours — *are* the contract for asset search, asset detail, folder
navigation and downloads. There is no separate REST specification that describes a search result:
the shape of the data is the shape of these C# types. This document describes that shape from the
authoring side, so that you know what to fill in, what you may leave out, and which of your choices
the portal will act on.

This document covers the objects and the interfaces. Its companion,
[the connector meta-model](smintio-connector-metamodel.md), covers the schema that gives those
objects meaning — which is what you need for anything under `rawData`.

If you are writing a *UI component* rather than a data adapter, you want the consuming side instead:
[the frontend data adapter reference](../../Frontend/docs/smintio-data-adapter-reference.md), which
lists every interface and model as TypeScript.

1. [An asset is a bag of values, not a class](#user-content-an-asset-is-a-bag-of-values-not-a-class)
1. [There is only one asset type](#user-content-there-is-only-one-asset-type)
1. [Binaries: thumbnails, playback and previews](#user-content-binaries-thumbnails-playback-and-previews)
1. [Identifiers are scoped for you](#user-content-identifiers-are-scoped-for-you)
1. [Folders](#user-content-folders)
1. [References between assets](#user-content-references-between-assets)
1. [Composite assets](#user-content-composite-assets)
1. [Raw data, and what survives a search](#user-content-raw-data-and-what-survives-a-search)
1. [Say what you support](#user-content-say-what-you-support)
1. [Paging](#user-content-paging)
1. [Filters are opaque tokens](#user-content-filters-are-opaque-tokens)
1. [Permissions](#user-content-permissions)
1. [Long-running methods](#user-content-long-running-methods)
1. [A checklist for a new data adapter](#user-content-a-checklist-for-a-new-data-adapter)

## An asset is a bag of values, not a class

`AssetDataObject` looks like an ordinary C# class with a few dozen properties, and it is not. It
extends `DataObject`, which is a dictionary of property keys to values. Every property you see on it
is a typed accessor over that dictionary, marked `[JsonIgnore]` — the dictionary is what gets
serialized, not the properties.

Two consequences, and they are the ones that catch people out:

- **Anything you do not set is absent**, not null, not empty, not defaulted. It does not appear in
  the payload at all, and the frontend cannot tell the difference between "the external system has
  no value for this" and "the data adapter never got round to it".
- **The meaning of a key comes from the meta-model**, not from the C# type. For the properties
  Smint.io defines, the meta-model is built in. For everything under `rawData`, the meaning comes
  from the meta-model your *connector* built — which is why the connector and the data adapter have
  to agree, and why a raw data value whose property was never declared is silently dropped.

So the job of a data adapter is: take whatever the external system returned, and set as many of the
properties below as you can honestly fill.

## There is only one asset type

There is no separate type for a product, a press release, a document or a video. Everything the
platform treats as an asset is an `AssetDataObject`. What it *is* comes from two enum properties:

| Property | Values |
|---|---|
| `contentType` | `unknown`, `image`, `document`, `video`, `audio`, `other`, `composite` |
| `compositeAssetType` | `press_release`, `product` — only meaningful when `contentType` is `composite` |
| `assetCategories` | an **open enum**: it ships with no values at all, and the values are whatever your connector supplies |

`contentType` is not decoration. It selects which per-content-type metadata object applies, it
becomes part of the asset's identifier, and the frontend branches on it when deciding how to render
a result. Set it on every asset; `other` is a better answer than leaving it empty.

The detail that belongs to one kind of content hangs off its own property, each one a data object of
its own: `fileMetadata`, `imageMetadata`, `audioMetadata`, `videoMetadata`, `documentMetadata`,
`vectorMetadata`. Set the one that applies and leave the rest absent — `fileMetadata` generally
applies to everything that has a file behind it, the others alongside it.

Composite assets additionally have `pressReleaseMetadata` and `productMetadata`. The first carries
real fields (headline, summary, date, location, text blocks, a quote, contact details, links). The
second declares no fields of its own: a product's substance lives in its raw data — the customer's
own product schema — and in its references to other assets. Do not go looking for product attributes
on `productMetadata`.

## Binaries: thumbnails, playback and previews

**You do not produce the rendition URLs. You declare which renditions exist, and Smint.io calls you
back for the bytes.**

This is the part of the model most often misunderstood, so it is worth being precise about the
sequence:

1. Your data adapter sets an **availability flag** for each rendition it can serve —
   `isThumbnailLargeAvailable`, `isPlaybackSmallAvailable`, `isPdfPreviewAvailable` and so on. It
   does *not* set `largeThumbnailUrl` and its siblings.
2. For every flag that is `true`, the Smint.io backend generates a URL pointing at the Smint.io
   CDN, carrying the asset identifier, the asset `version`, the rendition size, and a spec (below).
   It writes that URL into the corresponding `*Url` property. A rendition whose flag is not `true`
   gets no URL at all.
3. The browser requests that CDN URL, and the request **loops back into your data adapter**:
   Smint.io calls `GetAssetThumbnailDownloadStreamAsync` on
   `IAssetsInternalApiProvider` — which `IAssetsRead` already inherits, so every data adapter that
   can read assets has it — handing you the asset identifier, the content type, the requested
   `AssetThumbnailSize`, the spec, and a maximum file size. You return the stream.

So a rendition is served by *your* code, on demand, for as long as the portal asks for it. That is
why the flags matter: a flag set to `true` for a rendition you cannot actually produce becomes a
broken image in the portal, not a missing one.

One method serves every rendition. `AssetThumbnailSize` covers `Preview`, `Large`, `Medium`,
`Small`, `PlaybackLarge`, `PlaybackSmall`, `PlaybackStreaming`, `PdfPreview` and `Custom` — despite
the name, playback and PDF previews come through the same call. Switch on `size`.

| Availability flag | Fills | Loop-back arrives as |
|---|---|---|
| `isThumbnailLargeAvailable` | `largeThumbnailUrl` | `AssetThumbnailSize.Large` |
| `isThumbnailMediumAvailable` | `mediumThumbnailUrl` | `Medium` |
| `isThumbnailSmallAvailable` | `smallThumbnailUrl` | `Small` |
| `isThumbnailPreviewAvailable` | `previewThumbnailUrl` | `Preview` |
| `isPlaybackLargeAvailable` | `playbackLargeUrl` | `PlaybackLarge` |
| `isPlaybackSmallAvailable` | `playbackSmallUrl` | `PlaybackSmall` |
| `isPlaybackStreamingAvailable` | `playbackStreamingUrl` | `PlaybackStreaming` |
| `isPdfPreviewAvailable` | `pdfPreviewUrl` | `PdfPreview` |

`playbackStreamingMediaType` goes alongside the streaming flag. `thumbnailAspectRatio` and
`thumbnailAlignment` tell the frontend how to lay the thumbnail out before it has loaded, which is
what keeps a result grid from jumping around — fill the aspect ratio whenever the external system
knows it.

### `InternalMetadata` — the escape hatch

`AssetDataObject.InternalMetadata` (a `DataObjectInternalMetadata`) is **not part of the
metamodel**: it never reaches the frontend, and it is not a place to put business data. It is where
you influence the rendition mechanism above. Two of its fields matter most.

**Set a URL there and it is used verbatim.** If `InternalMetadata.LargeThumbnailUrl` is set, the
backend takes it as-is instead of generating a CDN URL — no loop-back, no call to your data adapter
for those bytes. Use this when the external system already publishes a directly usable URL, and the
portal may link straight to it. There is one such field per rendition, named after it
(`LargeThumbnailUrl`, `PreviewThumbnailUrl`, `PlaybackSmallUrl`, `PdfPreviewUrl`, …).

The availability flag still governs. A URL in `InternalMetadata` with the matching flag unset
produces nothing at all.

**Set a spec there and you get it back.** `InternalMetadata.LargeThumbnailSpec` and its siblings are
**opaque to Smint.io**: whatever you put in is carried through the generated URL and handed back to
`GetAssetThumbnailDownloadStreamAsync` as its `thumbnailSpec` argument. It is the way to remember,
per asset and per rendition, what your data adapter will need in order to produce that rendition —
a rendition key from the source system, a format name, a derivative id — without looking it up
again on every request.

Two constraints on a spec, both of which fail silently:

- It is sanitized before use. It must consist only of word characters, whitespace, and
  `. @ _ - + = ( ) : / & , ' |`. Anything else and the spec is dropped — your loop-back gets `null`.
- A spec that is empty, or exactly `-`, means "no spec".

So keep specs short and plain. If you need to carry something structured, keep the structure out of
the spec — use a short key and resolve it on your side.

The Hello World data adapter shows the whole round trip in about thirty lines. `SetThumbnails` in
`Assets/Common/HelloWorldContentConverter.cs` sets four availability flags and four specs — the
pixel widths `2048`, `1024`, `640` and `320` — and no URLs at all:

```C#
assetDataObject.IsThumbnailPreviewAvailable = true;
assetDataObject.IsThumbnailLargeAvailable = true;
// ...
assetDataObject.InternalMetadata = new DataObjectInternalMetadata
{
    PreviewThumbnailSpec = "2048",
    LargeThumbnailSpec = "1024",
    // ...
};
```

`GetAssetThumbnailDownloadStreamAsync` in `Assets/Read/HelloWorldAssetsRead.cs` is the other half:
it switches on `assetThumbnailSize` and, for the four thumbnail sizes, passes the spec it gets back
into the source system's own resizing URL. Read the two together before writing your own.

The remaining fields are for data adapters that feed the Smint.io index rather than serving a live
connection: a `*ETag` per rendition, which lets the indexer tell whether a rendition has changed and
needs regenerating, and a `DoNotGenerate*` flag per rendition, which tells it not to generate that
one at all.

### Downloads are a different path

None of the above is how an asset is *downloaded*. Downloads are a separate, three-step flow on
`IAssetsDownload`: the frontend asks which download options exist for a set of assets, the user
picks, the data adapter initiates the download and returns a URL, and the client follows that URL.
Those bytes never travel through the portal API at all.

### Folders work the same way, with less

`FolderDataObject` carries the same availability flags and gets the same treatment: Smint.io
generates the URL and calls `GetFolderThumbnailDownloadStreamAsync` for the bytes. But a folder has
no `InternalMetadata`, so there is no way to supply a URL directly, and the spec it is called with
is always `null`.

## Identifiers are scoped for you

Inside your data adapter you work with the external system's own IDs, unchanged. On the way out to
the frontend, Smint.io wraps each one so that it also identifies the data adapter instance it came
from, and unwraps it again on the way back in.

| Identifier | Scoped form |
|---|---|
| asset | `{dataAdapterInstanceKey}:{contentType}:{externalId}` |
| folder, resource, other identifiables | `{dataAdapterInstanceKey}:{externalId}` |

**This happens automatically, and you should neither produce nor parse a scoped ID.** Return the
external system's ID; expect the external system's ID in your parameters.

Two rules the scoping imposes on you. The content type may not contain a colon, and an asset whose
`contentType` you left empty is scoped with a reserved placeholder — one more reason to always set
it. The external ID itself *may* contain colons, because everything after the second separator is
treated as the ID; what it may not contain is an empty segment, so an ID with two colons in a row
will not survive the round trip.

The single exception is the [relationship semantic type](#user-content-references-between-assets),
where you deliberately construct an asset identifier from a foreign key held in raw metadata.

## Folders

`FolderDataObject` is the containment tree. It is navigated downward from the folder and upward from
the asset:

- folder → children: `childFolderIds`, `childFolders`, `childFolderCount`
- folder → parents: `parentFolderIds`, `parentFolderPaths`
- asset → folders: the same `parentFolderIds` and `parentFolderPaths`, on the asset

Note what is missing: **a folder does not list the assets it contains.** To get them, either search
with `SearchAssetsParameters.ParentFolderIds`, or implement `GetFolderContentsAsync`. Both are only
offered by the portal when you report folder navigation as supported.

Folders carry their own thumbnails, tags, raw data and timestamps, in the same style as assets.

## References between assets

There are two independent mechanisms. Most data adapters end up using both.

### `relatedAssets` — explicit groups

`AssetDataObject.relatedAssets` is an array of `RelatedAssetsDataObject`, conventionally one entry
per kind of relationship:

| Property | Meaning |
|---|---|
| `type` | what the group *means* — one of the relationship types below |
| `assetIds` | the referenced assets, listed directly |
| `searchAssetsSpecJson` | a serialized search specification — the *query* form, for a set too large or too dynamic to enumerate |
| `assetCaptions` | a caption per referenced asset |
| `assetCaptionJsonPathSelector` | where to read a caption from the referenced asset's own metadata instead |
| `caption`, `languageIsoCode` | a caption for the group as a whole |

A group is therefore either **by ID** or **by search spec** — the two ways anything in Smint.io
points at a set of assets. The search spec form is worth reaching for: a UI component can hand it
straight to a search page, so the filter lives in your data rather than being rebuilt in a component.

The relationship types are: `hero_representation`, `representation`, `download`, `asset_area_1`
through `asset_area_6`, `product`, `upstream`, `downstream`, `information`, `bom`, `certificate`,
`association`, `function`, `relation`, `variation`, `translation`, `reference`, `press_release`,
`campaign`, `article`, `version`, `other`.

They are conventions, not behaviour — with two exceptions, both described under
[composite assets](#user-content-composite-assets): `hero_representation` and `download`. A UI
component picks a group by matching its configured relationship type against `type`, so pick the one
that describes the relation and stay consistent across your assets.

### The relationship semantic type — implicit references

Beyond `relatedAssets`, **any raw metadata property can become a reference.** Declare the semantic
type *relationship* on the property in the connector's meta-model, and have the data adapter's
converter turn the raw foreign key into an asset identifier as it converts the value. The frontend
then receives an ID it can feed straight back into a "get asset" call, without knowing anything
about the external system.

The two halves have to be added together. A property declared as a relationship whose converter does
not do the rewrite emits a bare external ID that nothing in the portal can resolve, and nothing
reports an error — the link simply does nothing.

## Composite assets

An asset with `contentType = composite` is one that **references other assets**, and
`compositeAssetType` says what kind of composition it is. A product is normally a composite asset.

A composite **may carry a binary of its own, but usually does not.** The common pattern is that it
has a `hero_representation` reference, and Smint.io serves *that* asset's binary as the composite's
own: the availability flags, the streaming media type and the thumbnail aspect ratio are copied
across from the hero asset, and `InternalMetadata.ThumbnailAssetIdentifier` and `ThumbnailContentType`
are stamped on the composite so that the rendition loop-back is answered for the hero asset instead.

Three things follow:

- A composite with no `hero_representation` group and no thumbnails of its own simply has no
  thumbnail. That is a legitimate design — some product catalogues work this way — but it is also
  exactly what a forgotten hero reference looks like.
- A composite is free to declare its own availability flags instead and answer the loop-back itself,
  exactly like any other asset.
- **Downloading a composite is separate from displaying it.** When a composite is downloaded, it is
  replaced by the assets in its `download` relationship group, and the composite itself drops out of
  the request. A composite with no `download` group is not downloadable, however good its thumbnail.

## Raw data, and what survives a search

`AssetDataObject.rawData` is where the external system's own metadata goes — the custom fields, the
metadata layers, the schema that is specific to that customer. Each entry is a data object whose
entity is one your connector declared in
[its meta-model](smintio-connector-metamodel.md) — and anything you emit that the meta-model does
not declare is dropped on conversion.

**Search results and detail results differ in exactly one respect: raw data.** The property keys and
values are otherwise identical. A detail call returns the raw data as you produced it. A search
result returns only the parts that the portal administrator marked to be preserved.

That whitelist comes from the data adapter's own configuration. Implement
`IPreserveMetadataDataAdapterConfiguration` on your configuration class:

```C#
public class MyAssetsDataAdapterConfiguration : IPreserveMetadataDataAdapterConfiguration
{
    public MetadataAttributeModel[] SmintIoPreserveMetadataAttributes { get; set; }
}
```

and the "preserve metadata" form group appears in the data adapter configuration UI, where an
administrator picks the metadata attributes that search results should carry. Configure nothing and
search results carry no raw data at all.

This matters because the difference in size is large — a detail payload for an asset with several
metadata layers is routinely several times the size of its search result — and because the way to
change what a search returns is to change that configuration, **not** to change what your data
adapter puts into `RawData`. Keep producing the full raw data; let the configuration decide what is
worth carrying through a result list.

One thing the whitelist does *not* do: it does not deliver the meta-model that describes the
preserved data. Which parts of a connector meta-model reach the browser is decided separately, by
what the portal's components actually reference. Raw data can therefore arrive in the frontend
without a description — which is a reason to keep the set of preserved attributes tight and
deliberate.

## Say what you support

Two feature-support methods exist so the portal can ask instead of assume, and every data adapter
should answer them honestly:

| Method | Reports |
|---|---|
| `IAssetsSearch.GetFeatureSupportAsync` | `IsRandomAccessSupported` (can the user jump to page *n*?), `IsFullTextSearchProposalsSupported`, `IsFolderNavigationSupported` |
| `IAssetsRead.GetAssetsReadFeatureSupportAsync` | `IsFastGetAssetsSupported` |

Reporting `false` is not a failure; it makes the portal stop offering the feature. Reporting `true`
for something the external system cannot really do produces a control that misbehaves under the
user's hands.

A method you do not support should throw `NotImplementedException` — the framework turns that into
a clean "method not supported" answer rather than an error.

## Paging

A search result carries an `AssetSearchDetailsModel` alongside the assets, with `CurrentPage`,
`CurrentItemsPerPage`, `MaxPages`, `TotalResults`, `HasMoreResults`, the data adapter instance key,
the current folder when navigating folders, and `SearchResultSetId`.

`SearchResultSetId` is a **cursor**, and it is how paging works when the external system cannot do
random access: you return it, the caller passes it back as
`SearchAssetsParameters.SearchResultSetUuid`, and you continue from where you left off. Fill
`MaxPages` and `TotalResults` when the external system can tell you; leave them out rather than
guessing, and use `HasMoreResults` to say that there is more.

## Filters are opaque tokens

Which facets exist is declared by the
[connector meta-model](smintio-connector-metamodel.md#user-content-form-groups--declaring-search-facets),
as form groups and form items. What a
search returns is the live instance of those: each form item definition comes back with its allowed
values, each value carrying a display name, a result count, and a **string value that is a filter
token**.

That token is yours to define and yours alone to interpret. The caller — the portal frontend — hands
it back verbatim in `SearchAssetsParameters.CurrentFilters` when the user selects the facet. Nothing
outside your data adapter constructs one, and nothing should have to parse one. Keep them stable
across calls for the same selection, and keep them self-contained.

For facets whose values are too numerous or too dynamic to ship with every search, mark the form
item as having dynamic allowed values and serve them from
`GetFormItemDefinitionAllowedValuesAsync`.

## Permissions

Every method on the asset interfaces declares the permissions it requires with
`[RequiredPermissions(...)]`, checked against the frontend user before your code runs. The built-in
ones include `search_assets`, `read_asset_details`, `download_assets_hi_res`,
`download_asset_layout_files`, `flag_assets`, `comment_assets`, `share_assets`, `upload_assets` and
several more.

A data adapter can also **declare custom permissions of its own**, which then appear to the portal
administrator alongside the built-in ones and let access be managed at whatever granularity the
external system needs. In addition, an individual asset can carry `permissionUuids`, which is how a
data adapter says that this particular asset is only for some users.

## Long-running methods

A method that takes an `IProgressMonitor` is treated as long-running: it is queued rather than
executed inline, and the caller is handed a background task to poll. Methods without one are
executed inline and answer immediately.

That is a fixed property of the interface, not something you choose per call, so the signature tells
you which kind you are implementing. Report progress through the monitor in the long-running ones —
it is what the portal shows the user during a large download preparation.

## A checklist for a new data adapter

Before you consider a data adapter finished, check that:

- [ ] every asset has a `contentType`, even if it is `other`
- [ ] the matching per-content-type metadata object is filled, and the others are absent
- [ ] you set the rendition **availability flags**, not the `*Url` properties, and
      `GetAssetThumbnailDownloadStreamAsync` can actually serve every size you flagged
- [ ] the thumbnail aspect ratio is set where the external system knows it
- [ ] you return the external system's own IDs, never a scoped one
- [ ] both feature-support methods answer truthfully, and unsupported methods throw
      `NotImplementedException`
- [ ] paging returns a cursor when the external system cannot do random access
- [ ] your configuration implements `IPreserveMetadataDataAdapterConfiguration` if the assets carry
      raw metadata worth searching on
- [ ] relationships use `relatedAssets` with a type that describes them, and composites either have
      a `hero_representation` or declare their own renditions
- [ ] a composite that should be downloadable has a `download` relationship group
- [ ] every property you emit in `rawData` is declared in the connector's meta-model — anything else
      is dropped

The [Hello World data adapter](../DataAdapters/DataAdapter-HelloWorld/) implements the shape of all
of this against artificial data, and is the place to start. The Picturepark and SharePoint data
adapters show the same against a live system and against an indexed one respectively.

## Questions

Please do not hesitate to contact us at [support@smint.io](mailto:support@smint.io) if you run into
any issues.

Contributors
============

- Reinhard Holzner, Smint.io GmbH

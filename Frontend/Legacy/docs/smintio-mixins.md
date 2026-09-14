Smint.io Portals frontend component mixins
==========================================

We have prepared several Vue.js mixins that help you to perform common tasks more quickly in your Smint.io Portals frontend components.

They are all exported from `@smintio/portals-components`.

* [Download asset(s)](#download-assets)
* [Remember (collect) asset(s)](#remember-collect-assets)
* [Share asset(s)](#share-assets)
* [Other useful mixins](#other-useful-mixins)

Current version of this document is: 2.2.0 (as of 14th of September, 2026)

## How the mixins are built

Each of the three asset action mixins comes as a pair, already combined for you:

* an `S...Props` mixin, which contributes the **configuration properties** (all the dialog
  texts, as localized strings), their form group and their default values, and
* a dialog mixin, which contributes the **behaviour** (the methods you call and the
  `...DialogProps` getter you bind to the dialog).

You only mix in the `S...Props` one — it already mixes in the behaviour.

The dialog component comes with the mixin. Vue merges a mixin's `components` option into your
own, so mixing in `SDownloadProps`, `SShareProps` or `SRememberProps` registers
`SDownloadDialog`, `SShareDialog` or `SRememberDialog` for you. Use the kebab-case tag directly
in your template; do **not** import or register it yourself.

This applies to these three dialogs only. Every other shared component — `SCreateCollectionDialog`
and `SCollectionUsersDialog` included — you import and list under `components` as usual.

> **Note:** in earlier SDK versions you had to add the dialog texts by hand to
> `resources/definition.ts` via `setFormFieldValues`. That is no longer necessary — the
> `S...Props` mixins carry the correct string resource ids as `@DefaultValue`.

---

## Download asset(s)

### 1. Use the mixin in the Smint.io frontend component

```ts
import { Mixins } from "vue-property-decorator";
import { SDownloadProps } from "@smintio/portals-components";

export default class PortalsUiComponentImplementation extends Mixins(SDownloadProps) {
    ...
}
```

### 2. Use the dialog in the template

```vue
<s-download-dialog v-model="showDownloadDialog" v-bind="downloadDialogProps" />
```

### 3. Usage

```ts
public downloadAssets(assetIds: IAssetIdentifier[]): void;
public downloadCollection(collectionId: string): void;

// for assets and collections reached through a share link
public downloadSharedAssets(assetIds: IAssetIdentifier[], shareId: string, shareSecret: string): void;
public downloadSharedCollection(collectionId: string, shareId: string, shareSecret: string): void;
```

Call any of them to open the download dialog.

Before you offer a download button, check the permission:

```ts
import { AssetPermissionsMixin } from "@smintio/portals-components";
// ...
this.hasDownloadPermission(asset)
```

---

## Remember (collect) asset(s)

"Remember" is the collect-into-a-collection flow. The dialog lets the user pick an existing
collection or create a new one.

### 1. Use the mixin in the Smint.io frontend component

```ts
import { SRememberProps } from "@smintio/portals-components";

export default class PortalsUiComponentImplementation extends Mixins(SRememberProps) {
    ...
}
```

### 2. Use the dialog in the template

```vue
<s-remember-dialog
    v-model="showRememberDialog"
    v-bind="rememberDialogProps"
    @collection-created="onCollectionCreated"
    @collection-selected="onCollectionSelected"
    @collection-create:start="isCollecting = true"
    @collection-create:end="isCollecting = false"
/>
```

### 3. Usage

```ts
// open the dialog; pass asset ids to collect them as soon as a collection is chosen or created
public openRememberDialog(assetIds: IAssetIdentifier[] = []): void;

// add assets to an existing collection without the dialog
public async addAssetsToCollection(collectionId: string, assetIds: IAssetIdentifier[]): Promise<void>;

// show the "added to collection" confirmation
public showCollectionDoneNotification(count = 0): void;
```

A typical wiring:

```vue
<s-remember-dialog
    v-model="showRememberDialog"
    v-bind="rememberDialogProps"
    @collection-created="showCollectionDoneNotification(1)"
    @collection-selected="addAssetsToCollection($event.collectionId, [assetIdentifier])"
    @collection-create:start="isCollecting = true"
    @collection-create:end="isCollecting = false"
/>
```

---

## Share asset(s)

### 1. Use the mixin in the Smint.io frontend component

```ts
import { SShareProps } from "@smintio/portals-components";

export default class PortalsUiComponentImplementation extends Mixins(SShareProps) {
    ...
}
```

### 2. Use the dialog in the template

```vue
<s-share-dialog v-model="showShareDialog" v-bind="shareDialogProps" />
```

### 3. Usage

```ts
public shareAssets(assetIds: IAssetIdentifier[]): void;
public shareCollection(collectionId: string): void;
public editShare(shareId: string): void;
```

Sharing is not available in every portal. Guard the UI with `AuthMixin`:

```ts
import { AuthMixin } from "@smintio/portals-components";
// ...
v-if="shareAvailable"
```

---

## All three at once

The three mixins compose. This is what a component offering download, share and collect
looks like:

```ts
import { Mixins } from "vue-property-decorator";
import {
    AssetPermissionsMixin,
    AuthMixin,
    SDownloadProps,
    SRememberProps,
    SShareProps,
} from "@smintio/portals-components";

export default class PortalsUiComponentImplementation extends Mixins(
    Mixins<AssetPermissionsMixin, AuthMixin>(AssetPermissionsMixin, AuthMixin),
    Mixins<SDownloadProps, SRememberProps, SShareProps>(SDownloadProps, SRememberProps, SShareProps),
) {
    ...
}
```

```vue
<s-download-dialog v-model="showDownloadDialog" v-bind="downloadDialogProps" />
<s-share-dialog v-model="showShareDialog" v-bind="shareDialogProps" />
<s-remember-dialog
    v-model="showRememberDialog"
    v-bind="rememberDialogProps"
    @collection-created="onCollectionCreated"
    @collection-selected="onCollectionSelected"
    @collection-create:start="isCollecting = true"
    @collection-create:end="isCollecting = false"
/>
```

---

## Other useful mixins

| Mixin | What it gives you |
|---|---|
| `AuthMixin` | `isLoggedIn`, `user`, `loginAvailable`, `shareAvailable`, `ratingAvailable`, `commentingAvailable` and the related feature flags |
| `AssetPermissionsMixin` | `hasViewPermission`, `hasHiResPermission`, `hasLoResPermission`, `hasDownloadPermission` |
| `RoutingMixin` | `generateRouterLocation()`, `generateRouterLocationForAssetDataObject()` |
| `AssetsReferenceMixin` | resolves an `IAssetsReferenceModel` configuration property into actual assets — `getAssets()`, `getAssetByIds()`, `getAssetsByFolderId()`, `getAssetsBySearch()`, `getAssetsReferenceByRelatedAssets()`, `getAssetsReferenceByMetadataAttributes()`, plus `getAssetReferenceByRelationshipType(asset, "<relationship type>")`, which pulls the search or asset reference an asset carries in its own `relatedAssets` |
| `MetadataMixin` | `getLocalizedTags()`, `getLocalizedAttributeValue()`, `getLocalizedAttributeValues()`, `getLinks()` |
| `GalleryAssetConverterMixin` | converts assets into gallery items for the gallery components — `convertToItemAsset()`, `convertToImageAsset()`, `convertToVideoAsset()`, `convertToAudioAsset()`, `isVideoAsset()`, `isAudioAsset()` |
| `AssetDetailsPageNavigationMixin` | previous/next navigation through search results on an asset details page, plus `assetId` and `buttonBackToSearchTarget` |
| `MenuItemMixin` | builds effective menu items and their image URLs |
| `QuickViewModalMixin` | quick view modal wiring |
| `SPageMixin` | **page templates only**: registers the common slot renderers — `SHeaderSlot`, `SFooterSlot`, `SGenericSlot`, `SGenericMultiSlot` — and mixes in `SCssProps`. `SSideSlot`, `SSideMenuSlot` and `SBannerSlot` are not covered; register those yourself |
| `SLocalizedStringsModelProvider` / `SLocalizedStringsModelInjector` | provide and consume localized strings down a tree of ordinary Vue sub-components |

The `S...Props` configuration mixins (text, links, images, colors, layout gaps, content
width, custom CSS class and anchor) are documented alongside the UI components — mix them in
rather than declaring the same properties again.

Contributors
============

- Reinhard Holzner, Smint.io GmbH

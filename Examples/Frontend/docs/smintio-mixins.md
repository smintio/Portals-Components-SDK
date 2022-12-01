Smint.io Portals frontend component mixins
==========================================

We have prepared several Vue.js mixins that help you to perform common tasks more quickly in your Smint.io Portals frontend components.

* [Download asset(s)](https://github.com/smintio/Portals-UIComponents-Overview/blob/main/docs/smintio-mixins.md#download-assets)
* [Collect asset(s)](https://github.com/smintio/Portals-UIComponents-Overview/blob/main/docs/smintio-mixins.md#collect-assets)
* [Remember asset(s)](https://github.com/smintio/Portals-UIComponents-Overview/blob/main/docs/smintio-mixins.md#remember-assets)

Current version of this document is: 1.0.0 (as of 3rd of March, 2022)

## Download asset(s)

When you want the user to be able to open the download asset(s) dialog in your top level Smint.io component you need to make sure that the following steps are implemented:

### 1. Use the mixin in the Smint.io frontend component

```js
import { SDownloadProps } from "@smintio/portals-components";

export default class PortalsUiComponentImplementation extends Mixins(SDownloadProps) {
    ...
}
```

### 2. Use the dialog in the template

```vue
<s-download-dialog v-model="showDownloadDialog" v-bind="downloadDialogProps" />
```

### 3. Add the following resource definitions to `resources/definition.ts`

```js
{
    "id": "buttonDownloadText",
    "dataType": "resource_id",
    "resourceIdValue": "button_download",
},
{
    "id": "downloadSelectDownloadFormatsText",
    "dataType": "resource_id",
    "resourceIdValue": "text_select_download_formats",
},
{
    "id": "downloadNoDownloadFormatsAvailableText",
    "dataType": "resource_id",
    "resourceIdValue": "text_no_download_formats_available",
},
{
    "id": "downloadButtonDownloadImmediatelyText",
    "dataType": "resource_id",
    "resourceIdValue": "button_download_immediately",
},
{
    "id": "downloadButtonSendDownloadLinkText",
    "dataType": "resource_id",
    "resourceIdValue": "button_send_download_link",
},
{
    "id": "downloadDownloadImmediatelyDoneText",
    "dataType": "resource_id",
    "resourceIdValue": "text_download_immediately_done",
},
{
    "id": "downloadSendDownloadLinkDoneText",
    "dataType": "resource_id",
    "resourceIdValue": "text_download_send_download_link_done",
},
{
    "id": "downloadButtonCancelText",
    "dataType": "resource_id",
    "resourceIdValue": "button_cancel",
},
```

### 4. Usage:

There are 2 functions in the mixin that you can use in order to download an assets or collections:

```ts
public downloadAssets(assetIds: IAssetIdentifier[]): void {
  ...
}

public downloadCollection(collectionId: string): void {
  ...
}
```

You can call one, or the other in order to fire the download dialog.

---

## Collect asset(s)


When you want the user to be able to open the collect asset(s) dialog in your top level Smint.io component need to make sure that the following steps are implemented:

### 1. Use the mixin in the Smint.io component

```js
import { SSelectOrCreateCollectionProps } from "@smintio/portals-components";

export default class PortalsUiComponentImplementation extends Mixins(SSelectOrCreateCollectionProps) {
    ...
}
```

### 2. Use the dialog in the template

```vue
<s-select-or-create-collection-dialog
    v-model="showSelectOrCreateCollectionDialog"
    v-bind="selectOrCreateCollectionDialogProps"
    :create-new="true"
    @collection-created=""
    @collection-selected=""
    @collection-create:start=""
    @collection-create:end=""
/>
```

### 3. Add the following resource definitions to `resources/definition.ts`

```js
{
    "id": "buttonRememberText",
    "dataType": "resource_id",
    "resourceIdValue": "button_remember"
},
{
    "id": "rememberSelectCollectionText",
    "dataType": "resource_id",
    "resourceIdValue": "text_select_collection"
},
{
    "id": "rememberCreateCollectionText",
    "dataType": "resource_id",
    "resourceIdValue": "text_crete_collection"
},
{
    "id": "rememberNewCollectionText",
    "dataType": "resource_id",
    "resourceIdValue": "text_new_collection"
},
{
    "id": "rememberButtonSaveText",
    "dataType": "resource_id",
    "resourceIdValue": "button_save"
},
{
    "id": "rememberDoneText",
    "dataType": "resource_id",
    "resourceIdValue": "text_remember_done"
},
{
    "id": "rememberButtonCancelText",
    "dataType": "resource_id",
    "resourceIdValue": "button_cancel"
},
{
    "id": "rememberButtonCreateCollectionText",
    "dataType": "resource_id",
    "resourceIdValue": "button_create_collection"
},
```

### 4. Usage:

In order to open the select or create collection dialog you should call `openSelectOrCreateCollectionDialog`  function. 

```ts
public openSelectOrCreateCollectionDialog(assetIds: IAssetIdentifier[] = []): void {
  ...
}
```
 
If you want to create the collection and collect the items at the same time you should pass `assetIds` param.

The mixin provides the `addAssetsToCollection` function that handles the adding assets to existing collection.

```ts
public async addAssetsToCollection(collectionId: string, assetIds: IAssetIdentifier[]): Promise<void> {
  ...
}
```

Typical usage: 
```vue
<s-select-or-create-collection-dialog
    v-model="showSelectOrCreateCollectionDialog"
    v-bind="selectOrCreateCollectionDialogProps"
    @collection-created="showCollectionDoneNotification(1)"
    @collection-selected="addAssetsToCollection($event.collectionId, [assetIdentifier])"
    @collection-create:start="isCollecting = true"
    @collection-create:end="isCollecting = false"
/>
```

If you want ONLY to create new collection pass the `:create-new='true'` prop.

---

## Remember asset(s)

When you want the user to be able to open the remember asset(s) dialog in your top level Smint.io component you need to make sure that the following steps are implemented:

### 1. Use the mixin in the Smint.io component

```js
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

### 3. Add the following resource definitions to `resources/definition.ts`

```js
{
    "id": "buttonRememberText",
    "dataType": "resource_id",
    "resourceIdValue": "button_remember",
},
{
    "id": "rememberCreateCollectionText",
    "dataType": "resource_id",
    "resourceIdValue": "button_create_collection",
},
{
    "id": "rememberSelectCollectionText",
    "dataType": "resource_id",
    "resourceIdValue": "text_select_collection",
},
{
    "id": "rememberCollectionNameText",
    "dataType": "resource_id",
    "resourceIdValue": "text_collection_name",
},
{
    "id": "rememberButtonSaveText",
    "dataType": "resource_id",
    "resourceIdValue": "button_save",
},
{
    "id": "rememberDoneText",
    "dataType": "resource_id",
    "resourceIdValue": "text_collect_assets_done",
},
{
    "id": "rememberButtonCancelText",
    "dataType": "resource_id",
    "resourceIdValue": "button_cancel",
},
```

### 4. Usage:

There is one function in the mixin that you can use in order to add assets to the collection:

```ts
public async addAssetsToCollection(
  collectionId: string, 
  assetIds: IAssetIdentifier[]
): Promise<void> {
  ...
}
```

Contributors
============

- Yanko Belov, Smint.io. GmbH
- Reinhard Holzner, Smint.io GmbH

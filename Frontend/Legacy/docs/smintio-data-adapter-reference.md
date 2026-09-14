Smint.io Portals frontend data adapter reference
=================================================

Current version of this document is: 1.0.0 (as of 10th of September, 2026)

Everything a UI component can reach through a data adapter public API interface,
generated from `@smintio/portals-component-sdk`.

A UI component does not call a data adapter directly. It declares a property typed as a
public API interface, the portal editor picks which configured data source fulfils it, and
the runtime wires the call through to the backend:

```javascript
@DisplayName("en", "Data source", true)
@Implements("IAssetsSearch")
@ComponentProperty({ name: "dataSource" })
@FormGroup("my-group")
public readonly dataSource!: IAssetsSearch;
```

Every method is asynchronous and takes a single parameter object. Every parameter object
extends `IDataAdapterParameterObject` and every result extends `IDataAdapterResult`.

Please remember that the property can be `undefined` when the portal editor has not
configured a data source for it, so guard for that before calling.

## Contents

- [Assets](#assets)
- [Resource assets](#resource-assets)
- [Product assets](#product-assets)
- [Collections](#collections)
- [Shares](#shares)
- [File upload](#file-upload)
- [Task management](#task-management)
- [Search specs](#search-specs)
- [Prefabricated models](#prefabricated-models)
- [Progress](#progress)
- [Asset data objects](#asset-data-objects)
- [Base types](#base-types)
- [Localized string types](#localized-string-types)
- [Metadata metamodel](#metadata-metamodel)
- [Folders](#folders)
- [Page references](#page-references)
- [Menu items](#menu-items)
- [Resource types](#resource-types)

---

## Assets

### Public API interfaces

These are the names you pass to `@Implements(...)`.

#### `IAssets`

Also offers everything of `IAssetsRead`, `IAssetsReadRandom`, `IAssetsSearch`.

#### `IAssetsDownload`

| Method | Parameters | Returns |
|---|---|---|
| `getAssetsDownloadsForAssetsAsync` | `parameters: IGetAssetsDownloadsForAssetsParameters, progressMonitor: IProgressMonitor` | `Promise<IGetAssetsDownloadsResult>` |
| `getAssetsDownloadsForCollectionAsync` | `parameters: IGetAssetsDownloadsForCollectionParameters, progressMonitor: IProgressMonitor` | `Promise<IGetAssetsDownloadsResult>` |
| `prepareTemplateEditorAsync` | `parameters: IPrepareTemplateEditorParameters` | `Promise<IPrepareTemplateEditorResult>` |
| `initiateAssetsDownloadForAssetsAsync` | `parameters: IInitiateAssetsDownloadForAssetsParameters, progressMonitor: IProgressMonitor` | `Promise<IInitiateAssetsDownloadResult>` |
| `initiateAssetsDownloadForCollectionAsync` | `parameters: IInitiateAssetsDownloadForCollectionParameters, progressMonitor: IProgressMonitor` | `Promise<IInitiateAssetsDownloadResult>` |

#### `IAssetsFolderNavigation`

Also offers everything of `IAssetsSearch`, `IAssetsRead`.

| Method | Parameters | Returns |
|---|---|---|
| `getFolderAsync` | `parameters: IGetFolderParameters` | `Promise<IGetFolderResult>` |
| `getFoldersAsync` | `parameters: IGetFoldersParameters, progressMonitor: IProgressMonitor` | `Promise<IGetFoldersResult>` |
| `searchFoldersAsync` | `parameters: ISearchFoldersParameters` | `Promise<ISearchFoldersResult>` |

#### `IAssetsRead`

| Method | Parameters | Returns |
|---|---|---|
| `getAssetsReadFeatureSupportAsync` | `parameters: IGetAssetsReadFeatureSupportParameters` | `Promise<IGetAssetsReadFeatureSupportResult>` |
| `getAssetAsync` | `parameters: IGetAssetParameters` | `Promise<IGetAssetResult>` |
| `getAssetsAsync` | `parameters: IGetAssetsParameters, progressMonitor: IProgressMonitor` | `Promise<IGetAssetsResult>` |

#### `IAssetsReadRandom`

Also offers everything of `IAssetsRead`.

| Method | Parameters | Returns |
|---|---|---|
| `getRandomAssetsAsync` | `parameters: IGetRandomAssetsParameters` | `Promise<IGetRandomAssetsResult>` |

#### `IAssetsSearch`

Also offers everything of `IAssetsRead`.

| Method | Parameters | Returns |
|---|---|---|
| `getFeatureSupportAsync` | `parameters: IGetAssetsSearchFeatureSupportParameters` | `Promise<IGetAssetsSearchFeatureSupportResult>` |
| `getFormItemDefinitionAllowedValuesAsync` | `parameters: IGetFormItemDefinitionAllowedValuesParameters` | `Promise<IGetFormItemDefinitionAllowedValuesResult>` |
| `searchAssetsAsync` | `parameters: ISearchAssetsParameters` | `Promise<ISearchAssetsResult>` |
| `getFullTextSearchProposalsAsync` | `parameters: IGetFullTextSearchProposalsParameters` | `Promise<IGetFullTextSearchProposalsResult>` |

### Models

#### `IGetAssetResult`

Extends `IDataAdapterResult`, `IAssetDataObjectResult`.

Marker interface, no members of its own.

#### `IGetAssetsDownloadsResult`

Extends `IDataAdapterResult`.

| Field | Type | Optional |
|---|---|---|
| `assetDownloadGroups` | `IAssetDownloadGroupModel[]` | yes |

#### `IGetAssetsReadFeatureSupportResult`

Extends `IDataAdapterResult`.

| Field | Type | Optional |
|---|---|---|
| `isFastGetAssetsSupported` | `boolean` | yes |

#### `IGetAssetsResult`

Extends `IDataAdapterResult`, `IAssetDataObjectsResult`.

Marker interface, no members of its own.

#### `IGetAssetsSearchFeatureSupportResult`

Extends `IDataAdapterResult`.

| Field | Type | Optional |
|---|---|---|
| `isRandomAccessSupported` | `boolean` | yes |
| `isFullTextSearchProposalsSupported` | `boolean` | yes |
| `isFolderNavigationSupported` | `boolean` | yes |

#### `IGetFolderResult`

Extends `IDataAdapterResult`, `IFolderDataObjectResult`.

Marker interface, no members of its own.

#### `IGetFoldersResult`

Extends `IDataAdapterResult`, `IFolderDataObjectsResult`.

Marker interface, no members of its own.

#### `IGetFormItemDefinitionAllowedValuesResult`

Extends `IDataAdapterResult`.

| Field | Type | Optional |
|---|---|---|
| `allowedValues` | `IValueForJsonUiDetailsModel[]` | yes |
| `allowedValuesTotalCount` | `number` | yes |

#### `IGetFullTextSearchProposalsResult`

Extends `IDataAdapterResult`.

| Field | Type | Optional |
|---|---|---|
| `fullTextProposals` | `string[]` | yes |

#### `IGetRandomAssetsResult`

Extends `IDataAdapterResult`, `IAssetDataObjectsResult`.

Marker interface, no members of its own.

#### `IInitiateAssetsDownloadResult`

Extends `IDataAdapterResult`.

| Field | Type | Optional |
|---|---|---|
| `assetDownloadUrl` | `IAssetDownloadUrlModel` | yes |

#### `ISearchAssetsResult`

Extends `IDataAdapterResult`, `IAssetDataObjectsResult`.

| Field | Type | Optional |
|---|---|---|
| `details` | `IAssetSearchDetailsModel` | yes |
| `filterModel` | `IFormGroupsDefinitionModel` | yes |

#### `ISearchFoldersResult`

Extends `IDataAdapterResult`, `IFolderDataObjectsResult`.

| Field | Type | Optional |
|---|---|---|
| `details` | `IFolderSearchDetailsModel` | yes |

#### `IGetAssetParameters`

Extends `IDataAdapterParameterObject`, `IAssetIdParameters`.

| Field | Type | Optional |
|---|---|---|
| `shareId` | `string` | yes |
| `shareSecret` | `string` | yes |
| `isPageEvent` | `boolean` | yes |

#### `IGetAssetsDownloadsForAssetsParameters`

Extends `IDataAdapterParameterObject`, `IAssetIdsParameters`.

| Field | Type | Optional |
|---|---|---|
| `shareId` | `string` | yes |
| `shareSecret` | `string` | yes |
| `ignoreMissingAssets` | `boolean` | yes |

#### `IGetAssetsDownloadsForCollectionParameters`

Extends `IDataAdapterParameterObject`.

| Field | Type | Optional |
|---|---|---|
| `collectionId` | `string` | yes |
| `shareId` | `string` | yes |
| `shareSecret` | `string` | yes |
| `ignoreMissingAssets` | `boolean` | yes |

#### `IGetAssetsParameters`

Extends `IDataAdapterParameterObject`, `IAssetIdsParameters`.

| Field | Type | Optional |
|---|---|---|
| `shareId` | `string` | yes |
| `shareSecret` | `string` | yes |

#### `IGetAssetsReadFeatureSupportParameters`

Extends `IDataAdapterParameterObject`.

Marker interface, no members of its own.

#### `IGetAssetsSearchFeatureSupportParameters`

Extends `IDataAdapterParameterObject`.

Marker interface, no members of its own.

#### `IGetFolderParameters`

Extends `IDataAdapterParameterObject`, `IFolderIdParameters`.

Marker interface, no members of its own.

#### `IGetFoldersParameters`

Extends `IDataAdapterParameterObject`, `IFolderIdsParameters`.

Marker interface, no members of its own.

#### `IGetFormItemDefinitionAllowedValuesParameters`

Extends `IDataAdapterParameterObject`.

| Field | Type | Optional |
|---|---|---|
| `parentFolderIds` | `IFolderIdentifier[]` | yes |
| `searchQueryString` | `string` | yes |
| `queryString` | `string` | yes |
| `currentFilters` | `IFormFieldValuesModel` | yes |
| `contentType` | `ContentType` | yes |
| `resourceAssetMode` | `ResourceAssetMode` | yes |
| `resourceAssetType` | `string` | yes |
| `assetCategory` | `string` | yes |
| `searchResultSetUuid` | `string` | yes |
| `formItemDefinitionId` | `string` | yes |
| `formItemDefinitionQueryString` | `string` | yes |
| `maxResultCount` | `number` | yes |

#### `IGetFullTextSearchProposalsParameters`

Extends `IDataAdapterParameterObject`.

| Field | Type | Optional |
|---|---|---|
| `searchQueryString` | `string` | yes |
| `currentFilters` | `IFormFieldValuesModel` | yes |
| `contentType` | `ContentType` | yes |
| `resourceAssetMode` | `ResourceAssetMode` | yes |
| `resourceAssetType` | `string` | yes |
| `assetCategory` | `string` | yes |
| `maxResultCount` | `number` | yes |

#### `IGetRandomAssetsParameters`

Extends `IDataAdapterParameterObject`.

| Field | Type | Optional |
|---|---|---|
| `contentType` | `ContentType` | yes |
| `resourceAssetMode` | `ResourceAssetMode` | yes |
| `resourceAssetType` | `string` | yes |
| `assetCategory` | `string` | yes |
| `max` | `number` | no |

#### `IInitiateAssetsDownloadForAssetsParameters`

Extends `IDataAdapterParameterObject`, `IAssetIdsParameters`.

| Field | Type | Optional |
|---|---|---|
| `assetDownloadItems` | `IAssetDownloadItemModel[]` | yes |
| `immediatelyDownload` | `boolean` | no |
| `shareId` | `string` | yes |
| `shareSecret` | `string` | yes |

#### `IInitiateAssetsDownloadForCollectionParameters`

Extends `IDataAdapterParameterObject`.

| Field | Type | Optional |
|---|---|---|
| `collectionId` | `string` | yes |
| `assetDownloadItems` | `IAssetDownloadItemModel[]` | yes |
| `immediatelyDownload` | `boolean` | no |
| `shareId` | `string` | yes |
| `shareSecret` | `string` | yes |

#### `ISearchAssetsParameters`

Extends `IDataAdapterParameterObject`.

| Field | Type | Optional |
|---|---|---|
| `parentFolderIds` | `IFolderIdentifier[]` | yes |
| `queryString` | `string` | yes |
| `currentFilters` | `IFormFieldValuesModel` | yes |
| `contentType` | `ContentType` | yes |
| `resourceAssetMode` | `ResourceAssetMode` | yes |
| `resourceAssetType` | `string` | yes |
| `assetCategory` | `string` | yes |
| `searchResultSetUuid` | `string` | yes |
| `page` | `number` | yes |
| `pageSize` | `number` | yes |

#### `ISearchFoldersParameters`

Extends `IDataAdapterParameterObject`.

| Field | Type | Optional |
|---|---|---|
| `parentFolderIds` | `IFolderIdentifier[]` | yes |
| `includeParentFolders` | `boolean` | yes |
| `includeLevels` | `number` | yes |
| `queryStringFilter` | `IQueryStringFilterModel` | yes |
| `page` | `number` | yes |
| `pageSize` | `number` | yes |

#### `IAssetDownloadGroupModel`

| Field | Type | Optional |
|---|---|---|
| `groupId` | `string` | yes |
| `groupName` | `string` | yes |
| `assetDownloadItems` | `IAssetDownloadItemModel[]` | yes |

#### `IAssetDownloadItemModel`

| Field | Type | Optional |
|---|---|---|
| `groupId` | `string` | yes |
| `itemId` | `string` | yes |
| `description` | `string` | yes |
| `count` | `number` | no |
| `fileSizeInBytes` | `number` | yes |
| `selectedByDefault` | `boolean` | yes |
| `uiComponentType` | `string` | yes |
| `uiComponent` | `string` | yes |
| `externalUrl` | `string` | yes |
| `maxAssetCount` | `number` | yes |
| `maxAssetCountExceeded` | `boolean` | yes |
| `warningToBeAccepted` | `string` | yes |
| `assetIds` | `string[]` | yes |
| `formFieldValuesModel` | `IFormFieldValuesModel` | yes |
| `formGroupsDefinitionModel` | `IFormGroupsDefinitionModel` | yes |

#### `IAssetDownloadUrlModel`

Extends `IDataAdapterResult`, `IAssetDownloadInfoModel`.

| Field | Type | Optional |
|---|---|---|
| `url` | `string` | yes |

#### `IAssetSearchDetailsModel`

| Field | Type | Optional |
|---|---|---|
| `searchResultSetId` | `string` | yes |
| `currentPage` | `number` | yes |
| `currentItemsPerPage` | `number` | yes |
| `maxPages` | `number` | yes |
| `totalResults` | `number` | yes |
| `hasMoreResults` | `boolean` | yes |
| `dataAdapterInstanceKey` | `string` | yes |
| `folderNavigationEnabled` | `boolean` | yes |
| `parentFolderId` | `IFolderIdentifier` | yes |
| `parentFolderName` | `ILocalizedStringsModel` | yes |
| `errors` | `IErrorModel[]` | yes |

#### `IFolderDownloadStreamModel`

Extends `IDataAdapterResult`, `IFolderDownloadInfoModel`.

| Field | Type | Optional |
|---|---|---|
| `stream` | `unknown` | yes |

#### `IFolderDownloadUrlModel`

Extends `IDataAdapterResult`, `IFolderDownloadInfoModel`.

| Field | Type | Optional |
|---|---|---|
| `url` | `string` | yes |

#### `IFolderSearchDetailsModel`

| Field | Type | Optional |
|---|---|---|
| `currentPage` | `number` | yes |
| `currentItemsPerPage` | `number` | yes |
| `maxPages` | `number` | yes |
| `totalResults` | `number` | yes |
| `hasMoreResults` | `boolean` | yes |

#### `IQueryStringFilterModel`

| Field | Type | Optional |
|---|---|---|
| `queryString` | `string` | yes |
| `operator` | `QueryStringOperator` | yes |
| `ignoreCase` | `boolean` | yes |
| `applyToChildFolders` | `boolean` | yes |

#### `IAssetDownloadInfoModel`

Extends `IDataAdapterResult`.

| Field | Type | Optional |
|---|---|---|
| `fileName` | `string` | yes |
| `fileSizeInBytes` | `number` | yes |
| `mediaType` | `string` | yes |

#### `IFolderDownloadInfoModel`

Extends `IDataAdapterResult`.

| Field | Type | Optional |
|---|---|---|
| `fileName` | `string` | yes |
| `fileSizeInBytes` | `number` | yes |
| `mediaType` | `string` | yes |

### Enumerations

`AssetThumbnailSize`: `0`, `1`, `2`, `3`, `4`, `5`, `6`, `7`, `8`

`ContentType`: `0`, `1`, `2`, `3`

`FolderThumbnailSize`: `0`, `1`, `2`, `3`

`QueryStringOperator`: `0`, `1`, `2`

`ResourceAssetMode`: `0`, `1`, `2`

---

## Resource assets

### Public API interfaces

These are the names you pass to `@Implements(...)`.

#### `IResourceAssets`

Also offers everything of `IResourceAssetsRead`, `IAssetsRead`, `IResourceAssetsReadRandom`, `IAssetsReadRandom`, `IResourceAssetsSearch`, `IAssetsSearch`.

#### `IResourceAssetsRead`

Also offers everything of `IAssetsRead`.

#### `IResourceAssetsReadRandom`

Also offers everything of `IResourceAssetsRead`, `IAssetsRead`, `IAssetsReadRandom`.

#### `IResourceAssetsSearch`

Also offers everything of `IResourceAssetsRead`, `IAssetsRead`, `IAssetsSearch`.

---

## Product assets

### Public API interfaces

These are the names you pass to `@Implements(...)`.

#### `IProductAssets`

Also offers everything of `IProductAssetsRead`, `IAssetsRead`, `IProductAssetsReadRandom`, `IAssetsReadRandom`, `IProductAssetsSearch`, `IAssetsSearch`, `IProductAssetsSearchRelated`.

#### `IProductAssetsRead`

Also offers everything of `IAssetsRead`.

#### `IProductAssetsReadRandom`

Also offers everything of `IProductAssetsRead`, `IAssetsRead`, `IAssetsReadRandom`.

#### `IProductAssetsSearch`

Also offers everything of `IProductAssetsRead`, `IAssetsRead`, `IAssetsSearch`.

#### `IProductAssetsSearchRelated`

Also offers everything of `IProductAssetsRead`, `IAssetsRead`.

| Method | Parameters | Returns |
|---|---|---|
| `searchProductAssetsForAssetsAsync` | `parameters: ISearchProductAssetsForAssetsParameters` | `Promise<ISearchAssetsResult>` |

### Models

#### `ISearchProductAssetsForAssetsParameters`

Extends `IDataAdapterParameterObject`.

| Field | Type | Optional |
|---|---|---|
| `assetIds` | `IAssetIdentifier[]` | yes |
| `searchResultSetUuid` | `string` | yes |
| `page` | `number` | yes |
| `pageSize` | `number` | yes |

---

## Collections

### Public API interfaces

These are the names you pass to `@Implements(...)`.

#### `ICollections`

Also offers everything of `ICollectionsCreate`, `ICollectionsRead`, `ICollectionsSearch`, `ICollectionsUpdate`, `ICollectionsDelete`.

#### `ICollectionsAndAssets`

Also offers everything of `ICollections`, `ICollectionsCreate`, `ICollectionsRead`, `ICollectionsSearch`, `ICollectionsUpdate`, `ICollectionsDelete`, `ICollectionsAssets`, `ICollectionsAssetsSearch`, `ICollectionsAssetsRead`, `ICollectionsAssetsModify`.

#### `ICollectionsAssets`

Also offers everything of `ICollectionsAssetsSearch`, `ICollectionsAssetsRead`, `ICollectionsAssetsModify`.

#### `ICollectionsAssetsModify`

| Method | Parameters | Returns |
|---|---|---|
| `addAssetsToCollectionAsync` | `parameters: IAddAssetsToCollectionParameters, progressMonitor: IProgressMonitor` | `Promise<IAddAssetsToCollectionResult>` |
| `removeAssetsFromCollectionAsync` | `parameters: IRemoveAssetsFromCollectionParameters, progressMonitor: IProgressMonitor` | `Promise<IRemoveAssetsFromCollectionResult>` |
| `moveAssetsToCollectionAsync` | `parameters: IMoveAssetsToCollectionParameters, progressMonitor: IProgressMonitor` | `Promise<IMoveAssetsToCollectionResult>` |
| `copyAssetsToCollectionAsync` | `parameters: ICopyAssetsToCollectionParameters, progressMonitor: IProgressMonitor` | `Promise<ICopyAssetsToCollectionResult>` |
| `flagCollectionAssetsAsync` | `parameters: IFlagCollectionAssetsParameters, progressMonitor: IProgressMonitor` | `Promise<IFlagCollectionAssetsResult>` |
| `commentCollectionAssetAsync` | `parameters: ICommentCollectionAssetParameters` | `Promise<ICommentCollectionAssetResult>` |
| `updateCommentCollectionAssetAsync` | `parameters: IUpdateCommentCollectionAssetParameters` | `Promise<IUpdateCommentCollectionAssetResult>` |
| `deleteCommentCollectionAssetAsync` | `parameters: IDeleteCommentCollectionAssetParameters` | `Promise<IDeleteCommentCollectionAssetResult>` |

#### `ICollectionsAssetsRead`

| Method | Parameters | Returns |
|---|---|---|
| `getCollectionAssetCommentsAsync` | `parameters: IGetCollectionAssetCommentsParameters` | `Promise<IGetCollectionAssetCommentsResult>` |

#### `ICollectionsAssetsSearch`

| Method | Parameters | Returns |
|---|---|---|
| `searchCollectionAssetsAsync` | `parameters: ISearchCollectionAssetsParameters` | `Promise<ISearchCollectionAssetsResult>` |
| `getCollectionAssetFilterValuesAsync` | `parameters: IGetCollectionAssetsFilterValuesParameters` | `Promise<IGetCollectionAssetsFilterValuesResult>` |
| `getCollectionAssetSortValuesAsync` | `parameters: IGetCollectionAssetsSortValuesParameters` | `Promise<IGetCollectionAssetsSortValuesResult>` |

#### `ICollectionsCreate`

| Method | Parameters | Returns |
|---|---|---|
| `createCollectionAsync` | `parameters: ICreateCollectionParameters, progressMonitor: IProgressMonitor` | `Promise<ICreateCollectionResult>` |

#### `ICollectionsDelete`

Also offers everything of `ICollectionsRead`.

| Method | Parameters | Returns |
|---|---|---|
| `deleteCollectionAsync` | `parameters: IDeleteCollectionParameters, progressMonitor: IProgressMonitor` | `Promise<IDeleteCollectionResult>` |

#### `ICollectionsRead`

| Method | Parameters | Returns |
|---|---|---|
| `getCollectionAsync` | `parameters: IGetCollectionParameters` | `Promise<IGetCollectionResult>` |
| `getCollectionCommentsAsync` | `parameters: IGetCollectionCommentsParameters` | `Promise<IGetCollectionCommentsResult>` |

#### `ICollectionsSearch`

Also offers everything of `ICollectionsRead`.

| Method | Parameters | Returns |
|---|---|---|
| `searchCollectionsAsync` | `parameters: ISearchCollectionsParameters` | `Promise<ISearchCollectionsResult>` |
| `getCollectionsFilterValuesAsync` | `parameters: IGetCollectionsFilterValuesParameters` | `Promise<IGetCollectionsFilterValuesResult>` |
| `getCollectionsSortValuesAsync` | `parameters: IGetCollectionsSortValuesParameters` | `Promise<IGetCollectionsSortValuesResult>` |

#### `ICollectionsUpdate`

Also offers everything of `ICollectionsRead`.

| Method | Parameters | Returns |
|---|---|---|
| `updateCollectionAsync` | `parameters: IUpdateCollectionParameters` | `Promise<IUpdateCollectionResult>` |
| `commentCollectionAsync` | `parameters: ICommentCollectionParameters` | `Promise<ICommentCollectionResult>` |
| `updateCommentCollectionAsync` | `parameters: IUpdateCommentCollectionParameters` | `Promise<IUpdateCommentCollectionResult>` |
| `deleteCommentCollectionAsync` | `parameters: IDeleteCommentCollectionParameters` | `Promise<IDeleteCommentCollectionResult>` |

#### `ICollectionsUsers`

Also offers everything of `ICollectionsUsersSearch`, `ICollectionsUsersRead`, `ICollectionsUsersModify`.

#### `ICollectionsUsersModify`

| Method | Parameters | Returns |
|---|---|---|
| `updateCollectionUsersAsync` | `parameters: IUpdateCollectionUsersParameters, progressMonitor: IProgressMonitor` | `Promise<IUpdateCollectionUsersResult>` |
| `acceptCollectionUserInvitationAsync` | `parameters: IAcceptCollectionUserInvitationParameters` | `Promise<IAcceptCollectionUserInvitationResult>` |
| `declineCollectionUserInvitationAsync` | `parameters: IDeclineCollectionUserInvitationParameters` | `Promise<IDeclineCollectionUserInvitationResult>` |

#### `ICollectionsUsersRead`

| Method | Parameters | Returns |
|---|---|---|
| `getCollectionUserInvitationAsync` | `parameters: IGetCollectionUserInvitationParameters` | `Promise<IGetCollectionUserInvitationResult>` |

#### `ICollectionsUsersSearch`

| Method | Parameters | Returns |
|---|---|---|
| `searchCollectionUsersAsync` | `parameters: ISearchCollectionUsersParameters` | `Promise<ISearchCollectionUsersResult>` |
| `getCollectionUserFilterValuesAsync` | `parameters: IGetCollectionUsersFilterValuesParameters` | `Promise<IGetCollectionUsersFilterValuesResult>` |
| `getCollectionUserSortValuesAsync` | `parameters: IGetCollectionUsersSortValuesParameters` | `Promise<IGetCollectionUsersSortValuesResult>` |
| `searchCollectionEligibleUserGroupsAsync` | `parameters: ISearchCollectionEligibleUserGroupsParameters` | `Promise<ISearchCollectionEligibleUserGroupsResult>` |
| `getCollectionEligibleUserGroupFilterValuesAsync` | `parameters: IGetCollectionEligibleUserGroupsFilterValuesParameters` | `Promise<IGetCollectionEligibleUserGroupsFilterValuesResult>` |
| `getCollectionEligibleUserGroupSortValuesAsync` | `parameters: IGetCollectionEligibleUserGroupsSortValuesParameters` | `Promise<IGetCollectionEligibleUserGroupsSortValuesResult>` |
| `searchCollectionEligibleUsersAsync` | `parameters: ISearchCollectionEligibleUsersParameters` | `Promise<ISearchCollectionEligibleUsersResult>` |
| `getCollectionEligibleUserFilterValuesAsync` | `parameters: IGetCollectionEligibleUsersFilterValuesParameters` | `Promise<IGetCollectionEligibleUsersFilterValuesResult>` |
| `getCollectionEligibleUserSortValuesAsync` | `parameters: IGetCollectionEligibleUsersSortValuesParameters` | `Promise<IGetCollectionEligibleUsersSortValuesResult>` |

### Models

#### `ICollectionSpec`

| Field | Type | Optional |
|---|---|---|
| `name` | `string` | yes |
| `isStarred` | `boolean` | yes |
| `isOptIn` | `boolean` | yes |

#### `ICollectionUserSpec`

| Field | Type | Optional |
|---|---|---|
| `collectionUserId` | `string` | yes |
| `collectionUserGroupId` | `string` | yes |
| `emailAddress` | `string` | yes |
| `permissionLevel` | `PermissionLevelEnum` | no |

#### `IAcceptCollectionUserInvitationResult`

Extends `IDataAdapterResult`.

Marker interface, no members of its own.

#### `IAddAssetsToCollectionResult`

Extends `IDataAdapterResult`.

Marker interface, no members of its own.

#### `ICommentCollectionAssetResult`

Extends `IDataAdapterResult`.

| Field | Type | Optional |
|---|---|---|
| `comment` | `ICommentModel` | yes |

#### `ICommentCollectionResult`

Extends `IDataAdapterResult`.

| Field | Type | Optional |
|---|---|---|
| `comment` | `ICommentModel` | yes |

#### `ICopyAssetsToCollectionResult`

Extends `IDataAdapterResult`.

Marker interface, no members of its own.

#### `ICreateCollectionResult`

Extends `IDataAdapterResult`.

| Field | Type | Optional |
|---|---|---|
| `collection` | `ICollectionModel` | yes |

#### `IDeclineCollectionUserInvitationResult`

Extends `IDataAdapterResult`.

Marker interface, no members of its own.

#### `IDeleteCollectionResult`

Extends `IDataAdapterResult`.

Marker interface, no members of its own.

#### `IDeleteCommentCollectionAssetResult`

Extends `IDataAdapterResult`.

Marker interface, no members of its own.

#### `IDeleteCommentCollectionResult`

Extends `IDataAdapterResult`.

Marker interface, no members of its own.

#### `IFlagCollectionAssetsResult`

Extends `IDataAdapterResult`.

Marker interface, no members of its own.

#### `IGetCollectionAssetCommentsResult`

Extends `IDataAdapterResult`.

| Field | Type | Optional |
|---|---|---|
| `comments` | `ICommentModel[]` | yes |
| `details` | `ICollectionAssetGetCommentsDetailsModel` | yes |

#### `IGetCollectionAssetsFilterValuesResult`

Extends `IDataAdapterResult`.

| Field | Type | Optional |
|---|---|---|
| `filterSpecs` | `IFilterSpec[]` | yes |

#### `IGetCollectionAssetsSortValuesResult`

Extends `IDataAdapterResult`.

| Field | Type | Optional |
|---|---|---|
| `sortSpecs` | `ISortSpec[]` | yes |

#### `IGetCollectionCommentsResult`

Extends `IDataAdapterResult`.

| Field | Type | Optional |
|---|---|---|
| `comments` | `ICommentModel[]` | yes |
| `details` | `ICollectionGetCommentsDetailsModel` | yes |

#### `IGetCollectionResult`

Extends `IDataAdapterResult`.

| Field | Type | Optional |
|---|---|---|
| `collection` | `ICollectionModel` | yes |

#### `IGetCollectionsFilterValuesResult`

Extends `IDataAdapterResult`.

| Field | Type | Optional |
|---|---|---|
| `filterSpecs` | `IFilterSpec[]` | yes |

#### `IGetCollectionsSortValuesResult`

Extends `IDataAdapterResult`.

| Field | Type | Optional |
|---|---|---|
| `sortSpecs` | `ISortSpec[]` | yes |

#### `IGetCollectionUserInvitationResult`

Extends `IDataAdapterResult`.

| Field | Type | Optional |
|---|---|---|
| `inviterName` | `string` | yes |
| `collectionId` | `string` | yes |
| `collectionName` | `string` | yes |
| `userState` | `UserStateEnum` | yes |

#### `IGetCollectionUsersFilterValuesResult`

Extends `IDataAdapterResult`.

| Field | Type | Optional |
|---|---|---|
| `filterSpecs` | `IFilterSpec[]` | yes |

#### `IGetCollectionUsersSortValuesResult`

Extends `IDataAdapterResult`.

| Field | Type | Optional |
|---|---|---|
| `sortSpecs` | `ISortSpec[]` | yes |

#### `IGetCollectionEligibleUserGroupsFilterValuesResult`

Extends `IDataAdapterResult`.

| Field | Type | Optional |
|---|---|---|
| `filterSpecs` | `IFilterSpec[]` | yes |

#### `IGetCollectionEligibleUserGroupsSortValuesResult`

Extends `IDataAdapterResult`.

| Field | Type | Optional |
|---|---|---|
| `sortSpecs` | `ISortSpec[]` | yes |

#### `IGetCollectionEligibleUsersFilterValuesResult`

Extends `IDataAdapterResult`.

| Field | Type | Optional |
|---|---|---|
| `filterSpecs` | `IFilterSpec[]` | yes |

#### `IGetCollectionEligibleUsersSortValuesResult`

Extends `IDataAdapterResult`.

| Field | Type | Optional |
|---|---|---|
| `sortSpecs` | `ISortSpec[]` | yes |

#### `IMoveAssetsToCollectionResult`

Extends `IDataAdapterResult`.

Marker interface, no members of its own.

#### `IRemoveAssetsFromCollectionResult`

Extends `IDataAdapterResult`.

Marker interface, no members of its own.

#### `ISearchCollectionAssetsResult`

Extends `IDataAdapterResult`, `IAssetDataObjectsResult`.

| Field | Type | Optional |
|---|---|---|
| `details` | `ICollectionAssetSearchDetailsModel` | yes |

#### `ISearchCollectionsResult`

Extends `IDataAdapterResult`.

| Field | Type | Optional |
|---|---|---|
| `collections` | `ICollectionModel[]` | yes |
| `details` | `ICollectionSearchDetailsModel` | yes |

#### `ISearchCollectionUsersResult`

Extends `IDataAdapterResult`.

| Field | Type | Optional |
|---|---|---|
| `collectionUsers` | `ICollectionUserModel[]` | yes |
| `details` | `ICollectionUserSearchDetailsModel` | yes |

#### `ISearchCollectionEligibleUserGroupsResult`

Extends `IDataAdapterResult`.

| Field | Type | Optional |
|---|---|---|
| `collectionEligibleUserGroups` | `ICollectionEligibleUserGroupModel[]` | yes |
| `details` | `ICollectionEligibleUserGroupSearchDetailsModel` | yes |

#### `ISearchCollectionEligibleUsersResult`

Extends `IDataAdapterResult`.

| Field | Type | Optional |
|---|---|---|
| `collectionEligibleUsers` | `ICollectionEligibleUserModel[]` | yes |
| `details` | `ICollectionEligibleUserSearchDetailsModel` | yes |

#### `ISendFlagNotificationsResult`

Extends `IDataAdapterResult`.

Marker interface, no members of its own.

#### `IUpdateCollectionResult`

Extends `IDataAdapterResult`.

| Field | Type | Optional |
|---|---|---|
| `collection` | `ICollectionModel` | yes |

#### `IUpdateCollectionUsersResult`

Extends `IDataAdapterResult`.

Marker interface, no members of its own.

#### `IUpdateCommentCollectionAssetResult`

Extends `IDataAdapterResult`.

| Field | Type | Optional |
|---|---|---|
| `comment` | `ICommentModel` | yes |

#### `IUpdateCommentCollectionResult`

Extends `IDataAdapterResult`.

| Field | Type | Optional |
|---|---|---|
| `comment` | `ICommentModel` | yes |

#### `IAcceptCollectionUserInvitationParameters`

Extends `IDataAdapterParameterObject`.

| Field | Type | Optional |
|---|---|---|
| `invitationId` | `string` | yes |
| `invitationSecret` | `string` | yes |
| `message` | `string` | yes |

#### `IAddAssetsToCollectionParameters`

Extends `IDataAdapterParameterObject`, `IAssetIdsParameters`.

| Field | Type | Optional |
|---|---|---|
| `collectionId` | `string` | yes |

#### `ICommentCollectionAssetParameters`

Extends `IDataAdapterParameterObject`, `IAssetIdParameters`.

| Field | Type | Optional |
|---|---|---|
| `collectionId` | `string` | yes |
| `shareId` | `string` | yes |
| `shareSecret` | `string` | yes |
| `comment` | `string` | yes |
| `anonymousUserName` | `string` | yes |
| `uniqueId` | `string` | yes |

#### `ICommentCollectionParameters`

Extends `IDataAdapterParameterObject`.

| Field | Type | Optional |
|---|---|---|
| `collectionId` | `string` | yes |
| `shareId` | `string` | yes |
| `shareSecret` | `string` | yes |
| `comment` | `string` | yes |
| `anonymousUserName` | `string` | yes |
| `uniqueId` | `string` | yes |

#### `ICopyAssetsToCollectionParameters`

Extends `IDataAdapterParameterObject`, `IAssetIdsParameters`.

| Field | Type | Optional |
|---|---|---|
| `sourceCollectionId` | `string` | yes |
| `targetCollectionId` | `string` | yes |

#### `ICreateCollectionParameters`

Extends `IDataAdapterParameterObject`, `IAssetIdsParameters`.

| Field | Type | Optional |
|---|---|---|
| `definition` | `ICollectionSpec` | yes |
| `searchAssetsParameters` | `ISearchAssetsParameters` | yes |

#### `IDeclineCollectionUserInvitationParameters`

Extends `IDataAdapterParameterObject`.

| Field | Type | Optional |
|---|---|---|
| `invitationId` | `string` | yes |
| `invitationSecret` | `string` | yes |
| `message` | `string` | yes |

#### `IDeleteCollectionParameters`

Extends `IDataAdapterParameterObject`.

| Field | Type | Optional |
|---|---|---|
| `collectionId` | `string` | yes |

#### `IDeleteCommentCollectionAssetParameters`

Extends `IDataAdapterParameterObject`, `IAssetIdParameters`.

| Field | Type | Optional |
|---|---|---|
| `collectionId` | `string` | yes |
| `shareId` | `string` | yes |
| `shareSecret` | `string` | yes |
| `commentId` | `string` | yes |

#### `IDeleteCommentCollectionParameters`

Extends `IDataAdapterParameterObject`.

| Field | Type | Optional |
|---|---|---|
| `collectionId` | `string` | yes |
| `shareId` | `string` | yes |
| `shareSecret` | `string` | yes |
| `commentId` | `string` | yes |

#### `IFlagCollectionAssetsParameters`

Extends `IDataAdapterParameterObject`, `IAssetIdsParameters`.

| Field | Type | Optional |
|---|---|---|
| `collectionId` | `string` | yes |
| `shareId` | `string` | yes |
| `shareSecret` | `string` | yes |
| `flagId` | `string` | yes |

#### `IGetCollectionAssetCommentsParameters`

Extends `IDataAdapterParameterObject`, `IAssetIdParameters`.

| Field | Type | Optional |
|---|---|---|
| `collectionId` | `string` | yes |
| `shareId` | `string` | yes |
| `shareSecret` | `string` | yes |
| `page` | `number` | yes |
| `pageSize` | `number` | yes |

#### `IGetCollectionAssetsFilterValuesParameters`

Extends `IDataAdapterParameterObject`.

Marker interface, no members of its own.

#### `IGetCollectionAssetsSortValuesParameters`

Extends `IDataAdapterParameterObject`.

Marker interface, no members of its own.

#### `IGetCollectionCommentsParameters`

Extends `IDataAdapterParameterObject`.

| Field | Type | Optional |
|---|---|---|
| `collectionId` | `string` | yes |
| `shareId` | `string` | yes |
| `shareSecret` | `string` | yes |
| `page` | `number` | yes |
| `pageSize` | `number` | yes |

#### `IGetCollectionParameters`

Extends `IDataAdapterParameterObject`.

| Field | Type | Optional |
|---|---|---|
| `collectionId` | `string` | yes |
| `shareId` | `string` | yes |
| `shareSecret` | `string` | yes |
| `sharePassword` | `string` | yes |
| `isPageEvent` | `boolean` | yes |

#### `IGetCollectionsFilterValuesParameters`

Extends `IDataAdapterParameterObject`.

Marker interface, no members of its own.

#### `IGetCollectionsSortValuesParameters`

Extends `IDataAdapterParameterObject`.

Marker interface, no members of its own.

#### `IGetCollectionUserInvitationParameters`

Extends `IDataAdapterParameterObject`.

| Field | Type | Optional |
|---|---|---|
| `invitationId` | `string` | yes |
| `invitationSecret` | `string` | yes |

#### `IGetCollectionUsersFilterValuesParameters`

Extends `IDataAdapterParameterObject`.

Marker interface, no members of its own.

#### `IGetCollectionUsersSortValuesParameters`

Extends `IDataAdapterParameterObject`.

Marker interface, no members of its own.

#### `IGetCollectionEligibleUserGroupsFilterValuesParameters`

Extends `IDataAdapterParameterObject`.

Marker interface, no members of its own.

#### `IGetCollectionEligibleUserGroupsSortValuesParameters`

Extends `IDataAdapterParameterObject`.

Marker interface, no members of its own.

#### `IGetCollectionEligibleUsersFilterValuesParameters`

Extends `IDataAdapterParameterObject`.

Marker interface, no members of its own.

#### `IGetCollectionEligibleUsersSortValuesParameters`

Extends `IDataAdapterParameterObject`.

Marker interface, no members of its own.

#### `IMoveAssetsToCollectionParameters`

Extends `IDataAdapterParameterObject`, `IAssetIdsParameters`.

| Field | Type | Optional |
|---|---|---|
| `sourceCollectionId` | `string` | yes |
| `targetCollectionId` | `string` | yes |

#### `IRemoveAssetsFromCollectionParameters`

Extends `IDataAdapterParameterObject`, `IAssetIdsParameters`.

| Field | Type | Optional |
|---|---|---|
| `collectionId` | `string` | yes |
| `shareId` | `string` | yes |
| `shareSecret` | `string` | yes |

#### `ISearchCollectionAssetsParameters`

Extends `IDataAdapterParameterObject`.

| Field | Type | Optional |
|---|---|---|
| `collectionId` | `string` | yes |
| `shareId` | `string` | yes |
| `shareSecret` | `string` | yes |
| `queryString` | `string` | yes |
| `filters` | `IFilterSpec[]` | yes |
| `sortBy` | `string` | yes |
| `sortDirection` | `SortDirection` | yes |
| `page` | `number` | yes |
| `pageSize` | `number` | yes |

#### `ISearchCollectionsParameters`

Extends `IDataAdapterParameterObject`.

| Field | Type | Optional |
|---|---|---|
| `queryString` | `string` | yes |
| `filters` | `IFilterSpec[]` | yes |
| `sortBy` | `string` | yes |
| `sortDirection` | `SortDirection` | yes |
| `page` | `number` | yes |
| `pageSize` | `number` | yes |

#### `ISearchCollectionUsersParameters`

Extends `IDataAdapterParameterObject`.

| Field | Type | Optional |
|---|---|---|
| `collectionId` | `string` | yes |
| `queryString` | `string` | yes |
| `filters` | `IFilterSpec[]` | yes |
| `sortBy` | `string` | yes |
| `sortDirection` | `SortDirection` | yes |
| `page` | `number` | yes |
| `pageSize` | `number` | yes |

#### `ISearchCollectionEligibleUserGroupsParameters`

Extends `IDataAdapterParameterObject`.

| Field | Type | Optional |
|---|---|---|
| `queryString` | `string` | yes |
| `filters` | `IFilterSpec[]` | yes |
| `sortBy` | `string` | yes |
| `sortDirection` | `SortDirection` | yes |
| `page` | `number` | yes |
| `pageSize` | `number` | yes |

#### `ISearchCollectionEligibleUsersParameters`

Extends `IDataAdapterParameterObject`.

| Field | Type | Optional |
|---|---|---|
| `queryString` | `string` | yes |
| `filters` | `IFilterSpec[]` | yes |
| `sortBy` | `string` | yes |
| `sortDirection` | `SortDirection` | yes |
| `page` | `number` | yes |
| `pageSize` | `number` | yes |

#### `ISendFlagNotificationsParameters`

Extends `IDataAdapterParameterObject`.

| Field | Type | Optional |
|---|---|---|
| `collectionId` | `string` | yes |
| `shareId` | `string` | yes |
| `shareSecret` | `string` | yes |
| `anonymousUserName` | `string` | yes |
| `uniqueId` | `string` | yes |

#### `IUpdateCollectionParameters`

Extends `IDataAdapterParameterObject`.

| Field | Type | Optional |
|---|---|---|
| `collectionId` | `string` | yes |
| `shareId` | `string` | yes |
| `shareSecret` | `string` | yes |
| `definition` | `ICollectionSpec` | yes |

#### `IUpdateCollectionUsersParameters`

Extends `IDataAdapterParameterObject`.

| Field | Type | Optional |
|---|---|---|
| `collectionId` | `string` | yes |
| `sendEmailInvite` | `boolean` | yes |
| `invitationMessage` | `string` | yes |
| `collectionUserSpecs` | `ICollectionUserSpec[]` | yes |

#### `IUpdateCommentCollectionAssetParameters`

Extends `IDataAdapterParameterObject`, `IAssetIdParameters`.

| Field | Type | Optional |
|---|---|---|
| `collectionId` | `string` | yes |
| `shareId` | `string` | yes |
| `shareSecret` | `string` | yes |
| `commentId` | `string` | yes |
| `comment` | `string` | yes |
| `anonymousUserName` | `string` | yes |

#### `IUpdateCommentCollectionParameters`

Extends `IDataAdapterParameterObject`.

| Field | Type | Optional |
|---|---|---|
| `collectionId` | `string` | yes |
| `shareId` | `string` | yes |
| `shareSecret` | `string` | yes |
| `commentId` | `string` | yes |
| `comment` | `string` | yes |
| `anonymousUserName` | `string` | yes |

#### `ICollectionAssetGetCommentsDetailsModel`

| Field | Type | Optional |
|---|---|---|
| `currentPage` | `number` | yes |
| `currentItemsPerPage` | `number` | yes |
| `maxPages` | `number` | yes |
| `totalResults` | `number` | yes |
| `hasMoreResults` | `boolean` | yes |

#### `ICollectionAssetSearchDetailsModel`

| Field | Type | Optional |
|---|---|---|
| `currentPage` | `number` | yes |
| `currentItemsPerPage` | `number` | yes |
| `maxPages` | `number` | yes |
| `totalResults` | `number` | yes |
| `hasMoreResults` | `boolean` | yes |

#### `ICollectionGetCommentsDetailsModel`

| Field | Type | Optional |
|---|---|---|
| `currentPage` | `number` | yes |
| `currentItemsPerPage` | `number` | yes |
| `maxPages` | `number` | yes |
| `totalResults` | `number` | yes |
| `hasMoreResults` | `boolean` | yes |

#### `ICollectionModel`

| Field | Type | Optional |
|---|---|---|
| `collectionId` | `string` | yes |
| `name` | `string` | yes |
| `previews` | `IPreviewsModel` | yes |
| `permissionLevel` | `PermissionLevelEnum` | yes |
| `isShare` | `boolean` | yes |
| `isSearchBased` | `boolean` | yes |
| `shareConsumer` | `IShareConsumerModel` | yes |
| `totalCommentCount` | `number` | yes |

#### `ICollectionUserSearchDetailsModel`

| Field | Type | Optional |
|---|---|---|
| `currentPage` | `number` | yes |
| `currentItemsPerPage` | `number` | yes |
| `maxPages` | `number` | yes |
| `totalResults` | `number` | yes |
| `hasMoreResults` | `boolean` | yes |

#### `ICollectionEligibleUserGroupSearchDetailsModel`

| Field | Type | Optional |
|---|---|---|
| `currentPage` | `number` | yes |
| `currentItemsPerPage` | `number` | yes |
| `maxPages` | `number` | yes |
| `totalResults` | `number` | yes |
| `hasMoreResults` | `boolean` | yes |

#### `ICollectionEligibleUserSearchDetailsModel`

| Field | Type | Optional |
|---|---|---|
| `currentPage` | `number` | yes |
| `currentItemsPerPage` | `number` | yes |
| `maxPages` | `number` | yes |
| `totalResults` | `number` | yes |
| `hasMoreResults` | `boolean` | yes |

#### `ICollectionEligibleUserGroupModel`

| Field | Type | Optional |
|---|---|---|
| `collectionUserGroupId` | `string` | yes |
| `userGroupName` | `string` | yes |

#### `ICollectionEligibleUserModel`

| Field | Type | Optional |
|---|---|---|
| `collectionUserId` | `string` | yes |
| `emailAddress` | `string` | yes |
| `firstName` | `string` | yes |
| `lastName` | `string` | yes |
| `displayName` | `string` | yes |

#### `ICollectionUserModel`

| Field | Type | Optional |
|---|---|---|
| `collectionUserId` | `string` | yes |
| `collectionUserGroupId` | `string` | yes |
| `emailAddress` | `string` | yes |
| `userGroupName` | `string` | yes |
| `firstName` | `string` | yes |
| `lastName` | `string` | yes |
| `displayName` | `string` | yes |
| `userState` | `UserStateEnum` | yes |
| `permissionLevel` | `PermissionLevelEnum` | yes |

#### `ICollectionSearchDetailsModel`

| Field | Type | Optional |
|---|---|---|
| `currentPage` | `number` | yes |
| `currentItemsPerPage` | `number` | yes |
| `maxPages` | `number` | yes |
| `totalResults` | `number` | yes |
| `hasMoreResults` | `boolean` | yes |

#### `ICommentModel`

| Field | Type | Optional |
|---|---|---|
| `commentId` | `string` | yes |
| `sentByUserUuid` | `string` | yes |
| `sentByUserIsAnonymous` | `boolean` | yes |
| `sentByUserName` | `string` | yes |
| `createdAt` | `string` | no |
| `lastUpdatedAt` | `string` | yes |
| `comment` | `string` | yes |

#### `IPreviewModel`

| Field | Type | Optional |
|---|---|---|
| `smallThumbnailUrl` | `string` | yes |
| `mediumThumbnailUrl` | `string` | yes |
| `largeThumbnailUrl` | `string` | yes |
| `previewThumbnailUrl` | `string` | yes |

#### `IPreviewsModel`

| Field | Type | Optional |
|---|---|---|
| `preview` | `IPreviewModel[]` | yes |

### Enumerations

`PermissionLevelEnum`: `0`, `1`, `2`, `3`

`UserStateEnum`: `0`, `1`, `2`

---

## Shares

### Public API interfaces

These are the names you pass to `@Implements(...)`.

#### `IShares`

Also offers everything of `ISharesCreate`, `ISharesRead`, `ISharesSearch`, `ISharesUpdate`, `ISharesDelete`.

#### `ISharesCreate`

| Method | Parameters | Returns |
|---|---|---|
| `createShareAsync` | `parameters: ICreateShareParameters, progressMonitor: IProgressMonitor` | `Promise<ICreateShareResult>` |

#### `ISharesDelete`

Also offers everything of `ISharesRead`.

| Method | Parameters | Returns |
|---|---|---|
| `deleteShareAsync` | `parameters: IDeleteShareParameters, progressMonitor: IProgressMonitor` | `Promise<IDeleteShareResult>` |

#### `ISharesRead`

| Method | Parameters | Returns |
|---|---|---|
| `getShareAsync` | `parameters: IGetShareParameters` | `Promise<IGetShareResult>` |

#### `ISharesSearch`

Also offers everything of `ISharesRead`.

| Method | Parameters | Returns |
|---|---|---|
| `searchSharesAsync` | `parameters: ISearchSharesParameters` | `Promise<ISearchSharesResult>` |
| `getSharesFilterValuesAsync` | `parameters: IGetSharesFilterValuesParameters` | `Promise<IGetSharesFilterValuesResult>` |
| `getSharesSortValuesAsync` | `parameters: IGetSharesSortValuesParameters` | `Promise<IGetSharesSortValuesResult>` |

#### `ISharesUpdate`

Also offers everything of `ISharesRead`.

| Method | Parameters | Returns |
|---|---|---|
| `updateShareAsync` | `parameters: IUpdateShareParameters` | `Promise<IUpdateShareResult>` |

### Models

#### `IShareSpec`

Extends `ICollectionSpec`.

| Field | Type | Optional |
|---|---|---|
| `password` | `string` | yes |
| `expirationDate` | `string` | yes |
| `emailAddresses` | `string[]` | yes |
| `mustBeRegisteredUsers` | `boolean` | yes |
| `canModifyCollection` | `boolean` | yes |
| `enableCommenting` | `boolean` | yes |
| `enableRating` | `boolean` | yes |
| `enableHighResDownload` | `boolean` | yes |
| `displayMessage` | `string` | yes |
| `invitationMessage` | `string` | yes |
| `sendEmailInvite` | `boolean` | yes |

#### `ICreateShareResult`

Extends `IDataAdapterResult`.

| Field | Type | Optional |
|---|---|---|
| `share` | `IShareModel` | yes |

#### `IDeleteShareResult`

Extends `IDataAdapterResult`.

Marker interface, no members of its own.

#### `IGetShareResult`

Extends `IDataAdapterResult`.

| Field | Type | Optional |
|---|---|---|
| `share` | `IShareModel` | yes |

#### `IGetSharesFilterValuesResult`

Extends `IDataAdapterResult`.

| Field | Type | Optional |
|---|---|---|
| `filterSpecs` | `IFilterSpec[]` | yes |

#### `IGetSharesSortValuesResult`

Extends `IDataAdapterResult`.

| Field | Type | Optional |
|---|---|---|
| `sortSpecs` | `ISortSpec[]` | yes |

#### `ISearchSharesResult`

Extends `IDataAdapterResult`.

| Field | Type | Optional |
|---|---|---|
| `shares` | `IShareModel[]` | yes |
| `details` | `IShareSearchDetailsModel` | yes |

#### `IUpdateShareResult`

Extends `IDataAdapterResult`.

| Field | Type | Optional |
|---|---|---|
| `share` | `IShareModel` | yes |

#### `ICreateShareParameters`

Extends `IDataAdapterParameterObject`, `IAssetIdsParameters`.

| Field | Type | Optional |
|---|---|---|
| `definition` | `IShareSpec` | yes |
| `collectionId` | `string` | yes |
| `searchAssetsParameters` | `ISearchAssetsParameters` | yes |

#### `IDeleteShareParameters`

Extends `IDataAdapterParameterObject`.

| Field | Type | Optional |
|---|---|---|
| `shareId` | `string` | yes |

#### `IGetShareParameters`

Extends `IDataAdapterParameterObject`.

| Field | Type | Optional |
|---|---|---|
| `shareId` | `string` | yes |
| `isPageEvent` | `boolean` | yes |

#### `IGetSharesFilterValuesParameters`

Extends `IDataAdapterParameterObject`.

Marker interface, no members of its own.

#### `IGetSharesSortValuesParameters`

Extends `IDataAdapterParameterObject`.

Marker interface, no members of its own.

#### `ISearchSharesParameters`

Extends `IDataAdapterParameterObject`.

| Field | Type | Optional |
|---|---|---|
| `queryString` | `string` | yes |
| `filters` | `IFilterSpec[]` | yes |
| `sortBy` | `string` | yes |
| `sortDirection` | `SortDirection` | yes |
| `page` | `number` | yes |
| `pageSize` | `number` | yes |

#### `IUpdateShareParameters`

Extends `IDataAdapterParameterObject`.

| Field | Type | Optional |
|---|---|---|
| `shareId` | `string` | yes |
| `definition` | `IShareSpec` | yes |

#### `IShareConsumerModel`

| Field | Type | Optional |
|---|---|---|
| `shareId` | `string` | yes |
| `sharedByName` | `string` | yes |
| `commentingEnabled` | `boolean` | yes |
| `ratingEnabled` | `boolean` | yes |
| `displayMessage` | `string` | yes |
| `isCollection` | `boolean` | yes |
| `isEditable` | `boolean` | yes |
| `shareSecret` | `string` | yes |

#### `IShareModel`

Extends `ICollectionModel`.

| Field | Type | Optional |
|---|---|---|
| `shareId` | `string` | yes |
| `password` | `string` | yes |
| `expirationDate` | `string` | yes |
| `emailAddresses` | `string[]` | yes |
| `mustBeRegisteredUsers` | `boolean` | yes |
| `canModifyCollection` | `boolean` | yes |
| `enableCommenting` | `boolean` | yes |
| `enableRating` | `boolean` | yes |
| `enableHighResDownload` | `boolean` | yes |
| `displayMessage` | `string` | yes |
| `creationDate` | `string` | no |
| `shareLink` | `string` | yes |
| `isCollection` | `boolean` | yes |

#### `IShareSearchDetailsModel`

| Field | Type | Optional |
|---|---|---|
| `currentPage` | `number` | yes |
| `currentItemsPerPage` | `number` | yes |
| `maxPages` | `number` | yes |
| `totalResults` | `number` | yes |
| `hasMoreResults` | `boolean` | yes |

---

## File upload

### Models

#### `IFileUpload`

| Method | Parameters | Returns |
|---|---|---|
| `uploadFileAsync` | `file: Blob, progressMonitor?: IProgressMonitor, cancelable?: ICancelable` | `Promise<IUploadFileResult>` |

#### `IUploadFileResult`

| Field | Type | Optional |
|---|---|---|
| `fileUuid` | `string` | no |

---

## Task management

### Public API interfaces

These are the names you pass to `@Implements(...)`.

#### `ITaskManagement`

Also offers everything of `ITaskManagementCreate`, `ITaskManagementRead`.

#### `ITaskManagementCreate`

| Method | Parameters | Returns |
|---|---|---|
| `createTaskAsync` | `parameters: ICreateTaskParameters, progressMonitor: IProgressMonitor` | `Promise<ICreateTaskResult>` |
| `validateCreateTaskFormFieldValuesAsync` | `parameters: IValidateCreateTaskFormFieldValuesParameters` | `Promise<IValidateCreateTaskFormFieldValuesResult>` |

#### `ITaskManagementRead`

| Method | Parameters | Returns |
|---|---|---|
| `getTaskHandlerTaskCreationFormAsync` | `parameters: IGetTaskHandlerTaskCreationFormParameters` | `Promise<IGetTaskHandlerTaskCreationFormResult>` |

### Models

#### `ICreateTaskResult`

Extends `IDataAdapterResult`.

| Field | Type | Optional |
|---|---|---|
| `succeeded` | `boolean` | no |
| `openExternalUrl` | `string` | yes |
| `isNonInteractive` | `boolean` | no |
| `formGroupsDefinition` | `IFormGroupsDefinitionModel` | yes |

#### `IGetTaskHandlerTaskCreationFormResult`

Extends `IDataAdapterResult`.

| Field | Type | Optional |
|---|---|---|
| `taskActionDescriptor` | `ITaskActionDescriptorModel` | yes |

#### `IValidateCreateTaskFormFieldValuesResult`

Extends `IDataAdapterResult`.

| Field | Type | Optional |
|---|---|---|
| `formGroupsDefinition` | `IFormGroupsDefinitionModel` | yes |

#### `ICreateTaskParameters`

Extends `IDataAdapterParameterObject`.

| Field | Type | Optional |
|---|---|---|
| `taskHandlerId` | `string` | yes |
| `formFieldValues` | `IFormFieldValuesModel` | yes |
| `shareId` | `string` | yes |
| `shareSecret` | `string` | yes |
| `overrideFormItemDefinitions` | `IFormFieldItemDefinitionModel[],` | yes |
| `customFormId` | `string` | yes |

#### `IGetTaskHandlerTaskCreationFormParameters`

Extends `IDataAdapterParameterObject`.

| Field | Type | Optional |
|---|---|---|
| `taskHandlerId` | `string` | yes |
| `shareId` | `string` | yes |
| `shareSecret` | `string` | yes |
| `overrideFormItemDefinitions` | `IFormFieldItemDefinitionModel[],` | yes |
| `customFormId` | `string` | yes |

#### `IValidateCreateTaskFormFieldValuesParameters`

Extends `IDataAdapterParameterObject`.

| Field | Type | Optional |
|---|---|---|
| `taskHandlerId` | `string` | yes |
| `formFieldValues` | `IFormFieldValuesModel` | yes |
| `shareId` | `string` | yes |
| `shareSecret` | `string` | yes |
| `overrideFormItemDefinitions` | `IFormFieldItemDefinitionModel[],` | yes |
| `customFormId` | `string` | yes |

#### `ITaskActionDescriptorModel`

Extends `IUiDetailsModel`.

| Field | Type | Optional |
|---|---|---|
| `action` | `string` | yes |
| `openExternalUrl` | `string` | yes |
| `name` | `ILocalizedStringsModel` | yes |
| `internalName` | `ILocalizedStringsModel` | yes |
| `description` | `ILocalizedStringsModel` | yes |
| `mdiIcon` | `string` | yes |
| `formGroupsDefinition` | `IFormGroupsDefinitionModel` | yes |

---

## Search specs

### Models

#### `IFilterSpec`

| Field | Type | Optional |
|---|---|---|
| `value` | `string` | yes |
| `labels` | `ILocalizedStringsModel` | yes |

#### `ISortInfo`

| Field | Type | Optional |
|---|---|---|
| `fieldSpecification` | `ISortSpec` | yes |
| `direction` | `SortDirection` | yes |

#### `ISortSpec`

| Field | Type | Optional |
|---|---|---|
| `value` | `string` | yes |
| `labels` | `ILocalizedStringsModel` | yes |

### Enumerations

`SortDirection`: `0`, `1`

---

## Prefabricated models

### Models

#### `IAssetIdParameters`

Extends `IDataAdapterParameterObject`.

| Field | Type | Optional |
|---|---|---|
| `assetId` | `IAssetIdentifier` | yes |

#### `IAssetIdsParameters`

Extends `IDataAdapterParameterObject`.

| Field | Type | Optional |
|---|---|---|
| `assetIds` | `IAssetIdentifier[]` | yes |

#### `IAssetDataObjectResult`

Extends `IDataAdapterResult`.

| Field | Type | Optional |
|---|---|---|
| `assetDataObject` | `IAssetDataObject` | yes |
| `dataObject` | `any` | yes |

#### `IAssetDataObjectsResult`

Extends `IDataAdapterResult`.

| Field | Type | Optional |
|---|---|---|
| `assetDataObjects` | `IAssetDataObject[]` | yes |
| `dataObjects` | `any[]` | yes |

#### `IFolderIdParameters`

Extends `IDataAdapterParameterObject`.

| Field | Type | Optional |
|---|---|---|
| `folderId` | `IFolderIdentifier` | yes |

#### `IFolderIdsParameters`

Extends `IDataAdapterParameterObject`.

| Field | Type | Optional |
|---|---|---|
| `folderIds` | `IFolderIdentifier[]` | yes |

#### `IFolderDataObjectResult`

Extends `IDataAdapterResult`.

| Field | Type | Optional |
|---|---|---|
| `folderDataObject` | `IFolderDataObject` | yes |
| `dataObject` | `any` | yes |

#### `IFolderDataObjectsResult`

Extends `IDataAdapterResult`.

| Field | Type | Optional |
|---|---|---|
| `folderDataObjects` | `IFolderDataObject[]` | yes |
| `dataObjects` | `any[]` | yes |

#### `IResourceIdParameters`

Extends `IDataAdapterParameterObject`.

| Field | Type | Optional |
|---|---|---|
| `resourceId` | `IResourceIdentifier` | yes |

#### `IResourceIdsParameters`

Extends `IDataAdapterParameterObject`.

| Field | Type | Optional |
|---|---|---|
| `resourceIds` | `IResourceIdentifier[]` | yes |

#### `IResourceDataObjectResult`

Extends `IDataAdapterResult`.

| Field | Type | Optional |
|---|---|---|
| `resourceDataObject` | `IResourceDataObject` | yes |
| `dataObject` | `any` | yes |

#### `IResourceDataObjectsResult`

Extends `IDataAdapterResult`.

| Field | Type | Optional |
|---|---|---|
| `resourceDataObjects` | `IResourceDataObject[]` | yes |
| `dataObjects` | `any[]` | yes |

---

## Progress

### Models

#### `IProgressMonitor`

| Field | Type | Optional |
|---|---|---|
| `currentValue` | `number` | no |
| `maximum` | `number` | no |

| Method | Parameters | Returns |
|---|---|---|
| `reportProgressAsync` | `units: number, displayText: ILocalizedStringsModel` | `Promise<void>` |
| `finishedAsync` | `displayText: ILocalizedStringsModel` | `Promise<void>` |

---

## Asset data objects

### Models

#### `IAssetDataObject`

Extends `IDataObject`.

| Field | Type | Optional |
|---|---|---|
| `name` | `ILocalizedStringsModel` | yes |
| `description` | `ILocalizedStringsModel` | yes |
| `contentType` | `IEnumDataObject` | yes |
| `compositeAssetType` | `IEnumDataObject` | yes |
| `assetCategories` | `IEnumDataObject[]` | yes |
| `fileMetadata` | `IFileMetadataDataObject` | yes |
| `imageMetadata` | `IImageMetadataDataObject` | yes |
| `audioMetadata` | `IAudioMetadataDataObject` | yes |
| `videoMetadata` | `IVideoMetadataDataObject` | yes |
| `documentMetadata` | `IDocumentMetadataDataObject` | yes |
| `vectorMetadata` | `IVectorMetadataDataObject` | yes |
| `pressReleaseMetadata` | `ICompositeAssetPressReleaseMetadataDataObject` | yes |
| `productMetadata` | `ICompositeAssetProductMetadataDataObject` | yes |
| `largeThumbnailUrl` | `string` | yes |
| `mediumThumbnailUrl` | `string` | yes |
| `smallThumbnailUrl` | `string` | yes |
| `previewThumbnailUrl` | `string` | yes |
| `playbackLargeUrl` | `string` | yes |
| `playbackSmallUrl` | `string` | yes |
| `playbackStreamingUrl` | `string` | yes |
| `pdfPreviewUrl` | `string` | yes |
| `thumbnailAspectRatio` | `number` | yes |
| `thumbnailAlignment` | `IEnumDataObject` | yes |
| `isThumbnailLargeAvailable` | `boolean` | no |
| `isThumbnailMediumAvailable` | `boolean` | no |
| `isThumbnailSmallAvailable` | `boolean` | no |
| `isThumbnailPreviewAvailable` | `boolean` | no |
| `isPlaybackLargeAvailable` | `boolean` | no |
| `isPlaybackSmallAvailable` | `boolean` | no |
| `isPlaybackStreamingAvailable` | `boolean` | no |
| `playbackStreamingMediaType` | `string` | yes |
| `isPdfPreviewAvailable` | `boolean` | no |
| `indicator` | `IIndicatorDataObject` | yes |
| `localizedTags` | `ILocalizedStringsArrayModel` | no |
| `parentFolderIds` | `string[]` | yes |
| `relatedAssets` | `IRelatedAssetsDataObject[]` | yes |
| `dataAdapterInstanceKey` | `string` | yes |
| `version` | `string` | yes |
| `rawData` | `IRawDataObject[]` | yes |
| `rawDataHierarchy` | `IHierarchyEntryDataObject[]` | yes |
| `permissionUuids` | `string[]` | yes |
| `createdAt` | `string` | yes |
| `createdBy` | `string` | yes |
| `modifiedAt` | `string` | yes |
| `modifiedBy` | `string` | yes |
| `eTag` | `string` | yes |
| `metadataLayers` | `IMetadataLayerDataObject[]` | yes |
| `flag` | `IEnumDataObject` | yes |
| `totalCommentCount` | `number` | yes |
| `pathForUrl` | `ILocalizedStringsModel` | yes |
| `publishFrom` | `string` | yes |
| `publishUntil` | `string` | yes |
| `isDownloadable` | `boolean` | yes |
| `isCollectible` | `boolean` | yes |
| `externalId` | `string` | yes |
| `assetDetailPageInstanceKey` | `string` | yes |

#### `IFileMetadataDataObject`

Extends `IDataObject`.

| Field | Type | Optional |
|---|---|---|
| `name` | `ILocalizedStringsModel` | yes |
| `description` | `ILocalizedStringsModel` | yes |
| `fileExtension` | `string` | yes |
| `fileName` | `string` | yes |
| `filePath` | `string` | yes |
| `fileSizeInBytes` | `number` | yes |
| `mediaType` | `string` | yes |
| `sha1Hash` | `string` | yes |
| `language` | `string` | yes |
| `author` | `string` | yes |
| `creator` | `string` | yes |
| `publisher` | `string` | yes |
| `company` | `string` | yes |
| `title` | `string` | yes |
| `originalCreatedAt` | `string` | yes |
| `digitizedAt` | `string` | yes |
| `copyright` | `ILocalizedStringsModel,` | yes |
| `credit` | `ILocalizedStringsModel,` | yes |
| `location` | `ILocalizedStringsModel,` | yes |
| `source` | `ILocalizedStringsModel,` | yes |
| `captionWriter` | `string,` | yes |
| `category` | `ILocalizedStringsModel,` | yes |
| `supplementalCategories` | `ILocalizedStringsArrayModel,` | yes |
| `transmissionReference` | `ILocalizedStringsModel,` | yes |
| `city` | `string,` | yes |
| `state` | `string,` | yes |
| `country` | `ILocalizedStringsModel,` | yes |
| `countryCode` | `string,` | yes |
| `instructions` | `ILocalizedStringsModel,` | yes |
| `rawData` | `IRawDataObject[]` | yes |

#### `IImageMetadataDataObject`

Extends `IDataObject`.

| Field | Type | Optional |
|---|---|---|
| `width` | `number` | yes |
| `height` | `number` | yes |
| `widthInInch` | `number` | yes |
| `heightInInch` | `number` | yes |
| `widthInCm` | `number` | yes |
| `heightInCm` | `number` | yes |
| `colorSpace` | `string` | yes |
| `colorProfile` | `string` | yes |
| `bitsPerPixel` | `number` | yes |
| `bitsPerChannel` | `number` | yes |
| `channels` | `string` | yes |
| `pixelFormat` | `string` | yes |
| `hasAlpha` | `boolean` | yes |
| `isIndexed` | `boolean` | yes |
| `isExtended` | `boolean` | yes |
| `horizontalResolution` | `number` | yes |
| `verticalResolution` | `number` | yes |
| `resolution` | `number` | yes |
| `totalFrames` | `number` | yes |
| `totalUnspecifiedTiffExtraChannels` | `number` | yes |
| `hasExifData` | `boolean` | yes |
| `hasIptcData` | `boolean` | yes |
| `hasAdobeResourceData` | `boolean` | yes |
| `hasXmpData` | `boolean` | yes |
| `uncompressedSizeInBytes` | `number` | yes |
| `cameraMake` | `string` | yes |
| `cameraModel` | `string` | yes |
| `exposureDenominator` | `number` | yes |
| `exposureNumerator` | `number` | yes |
| `fNumber` | `number` | yes |
| `focalLength` | `number` | yes |
| `iso` | `number` | yes |
| `orientation` | `number` | yes |
| `takenDateTime` | `string` | yes |
| `quality` | `number` | yes |
| `dominantColor` | `string` | yes |
| `transparency` | `boolean` | yes |
| `aperture` | `number` | yes |
| `maxAperture` | `number` | yes |
| `meteringMode` | `string` | yes |
| `exposureBias` | `number` | yes |
| `exposureMode` | `number` | yes |
| `exposureTime` | `number` | yes |
| `flashUsed` | `boolean` | yes |
| `lens` | `string` | yes |
| `whiteBalance` | `string` | yes |

#### `IImageAsset`

Extends `IAssetDataObject`.

| Field | Type | Optional |
|---|---|---|
| `contentType` | `{` | no |
| `smintIoId` | `"image",` | no |
| `imageMetadata` | `IImageMetadataDataObject` | no |

#### `IAudioMetadataDataObject`

Extends `IDataObject`.

| Field | Type | Optional |
|---|---|---|
| `audioStreams` | `IAudioStreamDataObject[]` | yes |
| `album` | `string` | yes |
| `albumArtist` | `string` | yes |
| `artist` | `string` | yes |
| `composers` | `string` | yes |
| `copyright` | `string` | yes |
| `discNumber` | `number` | yes |
| `discCount` | `number` | yes |
| `genre` | `string` | yes |
| `drm` | `boolean` | yes |
| `variableBitrateEncoding` | `boolean` | yes |
| `title` | `string` | yes |
| `trackNumber` | `number` | yes |
| `trackCount` | `number` | yes |
| `year` | `number` | yes |

#### `IAudioAsset`

Extends `IAssetDataObject`.

| Field | Type | Optional |
|---|---|---|
| `contentType` | `{` | no |
| `smintIoId` | `"audio",` | no |
| `audioMetadata` | `IAudioMetadataDataObject` | no |

#### `IAudioStreamDataObject`

Extends `IDataObject`.

| Field | Type | Optional |
|---|---|---|
| `bitRate` | `string` | yes |
| `bitRateMode` | `string` | yes |
| `channels` | `string` | yes |
| `channelPositions` | `string` | yes |
| `codec` | `string` | yes |
| `durationInSeconds` | `number` | yes |
| `format` | `string` | yes |
| `language` | `string` | yes |
| `resolution` | `number` | yes |
| `samplingRate` | `number` | yes |
| `streamSize` | `number` | yes |
| `rawData` | `IRawDataObject[]` | yes |

#### `IVideoMetadataDataObject`

Extends `IDataObject`.

| Field | Type | Optional |
|---|---|---|
| `width` | `number` | yes |
| `height` | `number` | yes |
| `durationInSeconds` | `number` | yes |
| `format` | `string` | yes |
| `codec` | `string` | yes |
| `overallBitRate` | `number` | yes |
| `videoStreams` | `IVideoStreamDataObject[]` | yes |
| `audioStreams` | `IAudioStreamDataObject[]` | yes |

#### `IVideoAsset`

Extends `IAssetDataObject`.

| Field | Type | Optional |
|---|---|---|
| `contentType` | `{` | no |
| `smintIoId` | `"video",` | no |
| `videoMetadata` | `IVideoMetadataDataObject` | no |

#### `IVideoStreamDataObject`

Extends `IDataObject`.

| Field | Type | Optional |
|---|---|---|
| `bitRate` | `string` | yes |
| `codec` | `string` | yes |
| `displayAspectRatio` | `string` | yes |
| `durationInSeconds` | `number` | yes |
| `format` | `string` | yes |
| `frameCount` | `number` | yes |
| `frameRate` | `number` | yes |
| `width` | `number` | yes |
| `height` | `number` | yes |
| `language` | `string` | yes |
| `pixelAspectRatio` | `number` | yes |
| `resolution` | `number` | yes |
| `streamSize` | `number` | yes |
| `rotation` | `number` | yes |
| `scanType` | `string` | yes |
| `rawData` | `IRawDataObject[]` | yes |

#### `IDocumentMetadataDataObject`

Extends `IDataObject`.

| Field | Type | Optional |
|---|---|---|
| `applicationName` | `string` | yes |
| `applicationVersion` | `string` | yes |
| `characterCount` | `number` | yes |
| `characterCountWithSpaces` | `number` | yes |
| `lineCount` | `number` | yes |
| `pageCount` | `number` | yes |
| `slideCount` | `number` | yes |
| `paragraphCount` | `number` | yes |
| `revisionNumber` | `number` | yes |
| `titles` | `string[]` | yes |
| `imageTitles` | `string[]` | yes |
| `epsMetadata` | `IEpsMetadataDataObject` | yes |

#### `IEpsMetadataDataObject`

Extends `IDataObject`.

| Field | Type | Optional |
|---|---|---|
| `isRasterized` | `boolean` | yes |
| `widthInPoints` | `number` | yes |
| `heightInPoints` | `number` | yes |

#### `IDocumentAsset`

Extends `IAssetDataObject`.

| Field | Type | Optional |
|---|---|---|
| `contentType` | `{` | no |
| `smintIoId` | `"document",` | no |
| `documentMetadata` | `IDocumentMetadataDataObject` | no |

#### `IVectorMetadataDataObject`

Extends `IDataObject`.

| Field | Type | Optional |
|---|---|---|
| `pageCount` | `number` | yes |

#### `IVectorAsset`

Extends `IAssetDataObject`.

| Field | Type | Optional |
|---|---|---|
| `contentType` | `{` | no |
| `smintIoId` | `"vector",` | no |
| `vectorMetadata` | `IVectorMetadataDataObject` | no |

#### `IIndicatorDataObject`

Extends `IDataObject`.

| Field | Type | Optional |
|---|---|---|
| `mdiIcon` | `string` | yes |
| `color` | `string` | yes |
| `text` | `ILocalizedStringsModel` | yes |

#### `ICompositeAssetPressReleaseMetadataDataObject`

Extends `IDataObject`.

| Field | Type | Optional |
|---|---|---|
| `headline` | `ILocalizedStringsModel` | yes |
| `summary` | `ILocalizedStringsModel` | yes |
| `date` | `string` | yes |
| `time` | `string` | yes |
| `location` | `ILocalizedStringsModel` | yes |
| `textBlock1` | `ILocalizedStringsModel` | yes |
| `textBlock2` | `ILocalizedStringsModel` | yes |
| `quoteText` | `ILocalizedStringsModel` | yes |
| `quoteOrigin` | `ILocalizedStringsModel` | yes |
| `about` | `ILocalizedStringsModel` | yes |
| `contactName` | `string` | yes |
| `contactAddress` | `string` | yes |
| `contactEmailAddress` | `string` | yes |
| `contactPhoneNumber` | `string` | yes |
| `link1` | `ILinkDataObject` | yes |
| `link2` | `ILinkDataObject` | yes |
| `link3` | `ILinkDataObject` | yes |

#### `ICompositeAssetProductMetadataDataObject`

Extends `IDataObject`.

Marker interface, no members of its own.

#### `IHierarchyEntryDataObject`

Extends `IDataObject`.

| Field | Type | Optional |
|---|---|---|
| `targetMetamodelEntityKey` | `string` | yes |
| `rawDataTopLevelObjectId` | `string` | yes |
| `rawDataPropertyKey` | `string` | yes |
| `childDataObjects` | `IHierarchyEntryDataObject[]` | yes |

#### `IMetadataLayerDataObject`

Extends `IDataObject`.

Marker interface, no members of its own.

#### `ILinkDataObject`

Extends `IDataObject`, `IPageReference`.

| Field | Type | Optional |
|---|---|---|
| `linkText` | `ILocalizedStringsModel` | yes |
| `urls` | `ILocalizedStringsModel` | yes |
| `effectiveUrl` | `string` | yes |
| `effectiveUrlIsPageReference` | `boolean` | no |
| `effectiveUrlIsAbsoluteUrl` | `boolean` | no |
| `effectiveUrlIsExternal` | `boolean` | no |

#### `IPressReleaseMetadataDataObjectCompositeAsset`

Extends `IAssetDataObject`.

| Field | Type | Optional |
|---|---|---|
| `contentType` | `{` | no |
| `smintIoId` | `"composite",` | no |
| `compositeAssetType` | `{` | no |
| `smintIoId` | `"press_release",` | no |
| `pressReleaseMetadata` | `ICompositeAssetPressReleaseMetadataDataObject` | yes |

#### `IProductMetadataDataObjectCompositeAsset`

Extends `IAssetDataObject`.

| Field | Type | Optional |
|---|---|---|
| `contentType` | `{` | no |
| `smintIoId` | `"composite",` | no |
| `compositeAssetType` | `{` | no |
| `smintIoId` | `"product",` | no |
| `productMetadata` | `ICompositeAssetProductMetadataDataObject` | yes |

#### `IRelatedAssetsDataObject`

Extends `IDataObject`.

| Field | Type | Optional |
|---|---|---|
| `type` | `IEnumDataObject` | yes |
| `assetIds` | `string[]` | yes |
| `assetCaptionJsonPathSelector` | `string` | yes |
| `assetCaptions` | `IAssetCaptionDataObject[]` | yes |
| `searchAssetsSpecJson` | `string` | yes |
| `caption` | `ILocalizedStringsModel` | yes |

#### `IAssetCaptionDataObject`

Extends `IDataObject`.

Marker interface, no members of its own.

#### `IDynamicAllowedValuesParametersModel`

Marker interface, no members of its own.

#### `IAssetParametersModel`

Extends `IDynamicAllowedValuesParametersModel`.

| Field | Type | Optional |
|---|---|---|
| `contentType` | `ContentType` | yes |
| `resourceAssetMode` | `ResourceAssetMode` | yes |
| `assetCategory` | `string` | yes |
| `allowRelatedAssets` | `boolean` | yes |

#### `IResourceAssetParametersModel`

Extends `IDynamicAllowedValuesParametersModel`.

| Field | Type | Optional |
|---|---|---|
| `contentType` | `ContentType` | yes |
| `assetCategory` | `string` | yes |
| `resourceAssetType` | `string` | yes |
| `allowRelatedAssets` | `boolean` | yes |

---

## Base types

### Models

#### `IDataAdapterInterface`

Marker interface, no members of its own.

#### `IUiDetailsModel`

| Field | Type | Optional |
|---|---|---|
| `value` | `IValueForJson` | yes |
| `name` | `ILocalizedStringsModel` | yes |
| `description` | `ILocalizedStringsModel` | yes |
| `count` | `number` | yes |

#### `IValueForJsonUiDetailsModel`

| Field | Type | Optional |
|---|---|---|
| `value` | `IValueForJson` | yes |
| `name` | `ILocalizedStringsModel` | yes |
| `description` | `ILocalizedStringsModel` | yes |
| `count` | `number` | yes |

#### `IValueForJson`

| Field | Type | Optional |
|---|---|---|
| `dataType` | `ValueType` | no |
| `stringValue` | `string` | yes |
| `stringArrayValue` | `string[]` | yes |
| `localizedStringsModelValue` | `ILocalizedStringsModel` | yes |
| `localizedStringsArrayModelValue` | `ILocalizedStringsArrayModel` | yes |
| `int32Value` | `number` | yes |
| `int32ArrayValue` | `number[]` | yes |
| `int64Value` | `number` | yes |
| `int64ArrayValue` | `number[]` | yes |
| `decimalValue` | `number` | yes |
| `booleanValue` | `boolean` | yes |
| `dateTimeValue` | `Date` | yes |
| `dataObjectValue` | `IDataObject` | yes |
| `dataObjectArrayValue` | `IDataObject[]` | yes |
| `dataAdapterInstanceKeyValue` | `string` | yes |
| `dataAdapterInstanceKeyArrayValue` | `string[]` | yes |
| `pageInstanceKeyValue` | `string` | yes |
| `resourceIdValue` | `string` | yes |
| `resourceIdArrayValue` | `string[]` | yes |
| `assetIdValue` | `string` | yes |
| `assetIdArrayValue` | `string[]` | yes |
| `folderIdValue` | `string` | yes |
| `folderIdArrayValue` | `string[]` | yes |
| `assetsReferenceModelValue` | `IAssetsReferenceModel` | yes |
| `searchAssetsSpecModelValue` | `ISearchAssetsSpecModel` | yes |
| `metadataAttributeModelValue` | `IMetadataAttributeModel` | yes |
| `metadataAttributeModelArrayValue` | `IMetadataAttributeModel[]` | yes |
| `enumerationValue` | `IEnumDataObject` | yes |
| `enumerationArrayValue` | `IEnumDataObject[]` | yes |
| `localizedEnumsArrayModelValue` | `ILocalizedEnumsArrayModel` | yes |
| `currencyModelValue` | `ICurrencyModel` | yes |
| `geoLocationModelValue` | `IGeoLocationModel` | yes |

#### `ICurrencyModel`

| Field | Type | Optional |
|---|---|---|
| `currency` | `string` | yes |
| `value` | `number` | yes |

#### `IGeoLocationModel`

| Field | Type | Optional |
|---|---|---|
| `longitude` | `number` | yes |
| `latitude` | `number` | yes |

#### `IFormGroupsDefinitionModel`

| Field | Type | Optional |
|---|---|---|
| `formGroupDefinitions` | `IFormGroupDefinitionModel[]` | yes |
| `validationErrors` | `IFormItemValidationErrorModel[]` | yes |

#### `IFormGroupDefinitionModel`

Extends `IUiDetailsModel`.

| Field | Type | Optional |
|---|---|---|
| `id` | `string` | no |
| `isDefaultGroup` | `boolean` | yes |
| `isModified` | `boolean` | yes |
| `formItemDefinitions` | `IFormFieldItemDefinitionModel[]` | yes |

#### `IFormItemValidationErrorModel`

| Field | Type | Optional |
|---|---|---|
| `formItemId` | `string` | yes |
| `errorCode` | `string` | yes |

#### `IFormFieldItemDefinitionModel`

Extends `IUiDetailsModel`.

| Field | Type | Optional |
|---|---|---|
| `id` | `string` | no |
| `itemVisibility` | `FormItemVisibilityEnum` | yes |
| `columnCountDesktop` | `number` | yes |
| `columnCountMobile` | `number` | yes |
| `lineFeedBefore` | `boolean` | yes |
| `lineFeedAfter` | `boolean` | yes |
| `isModified` | `boolean` | yes |
| `isInherited` | `boolean` | yes |
| `isOverridden` | `boolean` | yes |
| `isAutoDiscovered` | `boolean` | yes |
| `isRequired` | `boolean` | yes |
| `sortOrder` | `number` | yes |
| `dataType` | `ValueType` | no |
| `visibleIfs` | `IFormItemVisibleIfModel[]` | yes |
| `currentValue` | `IValueForJson` | yes |
| `defaultValue` | `IValueForJson` | yes |
| `allowedValues` | `IFormItemAllowedValue[]` | yes |
| `allowedValuesTotalCount` | `number` | yes |
| `allowedValuesPageSize` | `number` | yes |
| `isDynamicAllowedValues` | `boolean` | yes |
| `minValue` | `IValueForJson` | yes |
| `maxValue` | `IValueForJson` | yes |
| `stringMinLength` | `number` | yes |
| `stringMaxLength` | `number` | yes |
| `stringValidationRegex` | `string` | yes |
| `stringIsEmail` | `boolean` | yes |
| `stringIsUri` | `boolean` | yes |
| `stringIsPhoneNumber` | `boolean` | yes |
| `stringIsJson` | `boolean` | yes |
| `stringEnforceHttpsOnUri` | `boolean` | yes |
| `stringIsColor` | `boolean` | yes |
| `stringIsRichText` | `boolean` | yes |
| `stringIsFileUpload` | `boolean` | yes |
| `fileUploadAllowedMediaTypes` | `string[]` | yes |
| `fileUploadFileUrl` | `string` | yes |
| `fileDownloadFileUrl` | `string` | yes |
| `fileDownloadFileRichTextMessage` | `string` | yes |
| `localizedStringsModelIsUri` | `boolean` | yes |
| `localizedStringsModelIsRichText` | `boolean` | yes |
| `htmlTagWhitelist` | `string[]` | yes |
| `minValueDateTimeBefore` | `Date` | yes |
| `minValueDateTimeBeforeOrEqual` | `Date` | yes |
| `maxValueDateTimeAfter` | `Date` | yes |
| `maxValueDateTimeAfterOrEqual` | `Date` | yes |
| `requiresServerSideValidation` | `boolean` | yes |
| `uiDetails` | `IUiDetailsModel[]` | yes |

#### `IFormItemAllowedValue`

Extends `IUiDetailsModel`.

| Field | Type | Optional |
|---|---|---|
| `value` | `IValueForJson` | no |

#### `IFormItemVisibleIfModel`

| Field | Type | Optional |
|---|---|---|
| `property` | `string` | no |
| `operator` | `VisibleIfOperatorEnum` | no |
| `value` | `IValueForJson` | no |

#### `IFormFieldValuesModel`

| Field | Type | Optional |
|---|---|---|
| `values` | `IFormFieldValueModel[]` | yes |

#### `IFormFieldValueModel`

Extends `IValueForJson`.

| Field | Type | Optional |
|---|---|---|
| `id` | `string` | no |
| `isAutoDiscovered` | `boolean` | yes |
| `subComponentFormFieldValues` | `{` | yes |

#### `ISearchAssetsSpecModel`

| Field | Type | Optional |
|---|---|---|
| `parentFolderIds` | `IFolderIdentifier[]` | yes |
| `queryString` | `string` | yes |
| `currentFilters` | `IFormFieldValuesModel` | yes |

#### `IRelatedAssetsModel`

| Field | Type | Optional |
|---|---|---|
| `mode` | `RelatedAssetsMode` | no |
| `relationshipType` | `string` | yes |
| `metadataAttributeJsonPathSelectors` | `string[]` | yes |

#### `IAssetsReferenceModel`

| Field | Type | Optional |
|---|---|---|
| `type` | `AssetsReferenceType` | no |
| `assetIds` | `IAssetIdentifier[]` | yes |
| `folderId` | `IFolderIdentifier` | yes |
| `searchAssetsSpecModel` | `ISearchAssetsSpecModel` | yes |
| `relatedAssetsModel` | `IRelatedAssetsModel` | yes |

#### `IMetadataAttributeModel`

| Field | Type | Optional |
|---|---|---|
| `jsonPathSelector` | `string` | no |
| `labels` | `ILocalizedStringsModel` | yes |
| `displayType` | `MetadataAttributeDisplayType` | yes |
| `columnCountDesktop` | `number` | yes |
| `columnCountMobile` | `number` | yes |
| `indexType` | `MetadataAttributeIndexType` | yes |
| `includeValues` | `string[]` | yes |
| `excludeValues` | `string[]` | yes |
| `numberOfValuesToDisplay` | `number` | yes |
| `defaultSort` | `MetadataAttributeDefaultSort` | yes |
| `valuesSort` | `MetadataAttributeValuesSort` | yes |
| `isDefaultSortMetadataAttribute` | `boolean` | yes |
| `isQueryDefaultSortMetadataAttribute` | `boolean` | yes |

#### `IDataObject`

| Field | Type | Optional |
|---|---|---|
| `smintIoId` | `string` | yes |
| `smintIoListDisplayName` | `ILocalizedStringsModel` | yes |
| `smintIoDetailDisplayName` | `ILocalizedStringsModel` | yes |

#### `IEnumDataObject`

Extends `IDataObject`.

Marker interface, no members of its own.

#### `IProxyDataObject`

| Field | Type | Optional |
|---|---|---|
| `method` | `string` | no |
| `parameter` | `string` | yes |
| `isQueryStringSupported` | `boolean` | yes |
| `isRandomAccessSupported` | `boolean` | yes |

#### `IRawDataObject`

Extends `IDataObject`.

Marker interface, no members of its own.

#### `ILocalizedEnumModel`

| Field | Type | Optional |
|---|---|---|
| `id` | `string` | no |
| `listDisplayName` | `string` | yes |

#### `ILocalizedEnumsArrayModel`

Marker interface, no members of its own.

#### `IErrorModel`

| Field | Type | Optional |
|---|---|---|
| `errorCode` | `string` | yes |
| `errorMessage` | `string` | yes |
| `errorDetails` | `IErrorDetailsModel` | yes |

#### `IErrorDetailsModel`

| Field | Type | Optional |
|---|---|---|
| `uuid` | `string` | yes |
| `name` | `string` | yes |

### Enumerations

`VisibleIfOperatorEnum`: `equal`, `not_equal`, `greater_than`, `greater_than_or_equal`, `less_than`, `less_than_or_equal`, `one_of`

`AssetsReferenceType`: `asset_ids`, `folder_id`, `search_assets_spec`, `related_assets`

`RelatedAssetsMode`: `relationship_type`, `metadata_attributes`

`MetadataAttributeDisplayType`: `default`, `date_short`, `date_medium`, `date_long`, `date_time_short`, `date_time_medium`, `date_time_long`, `number_no_digits`, `chip_always`, `chip_never`

`MetadataAttributeIndexType`: `default`, `full_text`, `fragment`, `none`

`MetadataAttributeDefaultSort`: `default`, `asc`, `desc`

`MetadataAttributeValuesSort`: `default`, `count_desc`, `count_asc`, `natural_desc`, `natural_asc`

`ButtonStyle`: `textOnly`, `solidButton`, `outlinedButton`

---

## Localized string types

### Models

#### `ILocalizedStringsModel`

Marker interface, no members of its own.

#### `ILocalizedStringsArrayModel`

Marker interface, no members of its own.

---

## Metadata metamodel

### Models

#### `IAssetIdentifier`

| Field | Type | Optional |
|---|---|---|
| `id` | `string` | no |

#### `IFolderIdentifier`

| Field | Type | Optional |
|---|---|---|
| `id` | `string` | no |

#### `IResourceIdentifier`

| Field | Type | Optional |
|---|---|---|
| `id` | `string` | no |

#### `IEntityModel`

| Field | Type | Optional |
|---|---|---|
| `key` | `string` | no |
| `type` | `EntityTypeEnum` | no |
| `label` | `ILocalizedStringsModel` | yes |
| `properties` | `IPropertyModel[]` | yes |
| `parentEntityModelKey` | `string` | yes |

#### `IPropertyModel`

| Field | Type | Optional |
|---|---|---|
| `key` | `string` | no |
| `label` | `ILocalizedStringsModel` | yes |
| `dataType` | `ValueType` | no |
| `semanticType` | `SemanticTypeEnum` | yes |
| `targetEntityModelKey` | `string` | yes |

### Enumerations

`EntityTypeEnum`: `top_level_object`, `metadata_layer`, `fieldset`, `enum`, `other`, `unknown`

`SemanticTypeEnum`: `url`, `email`, `phone_number`, `date_only`, `date_time`, `time_only`, `html`, `relationship`

---

## Folders

### Models

#### `IFolderDataObject`

Extends `IDataObject`.

| Field | Type | Optional |
|---|---|---|
| `version` | `string` | yes |
| `name` | `ILocalizedStringsModel` | yes |
| `description` | `ILocalizedStringsModel` | yes |
| `smallThumbnailUrl` | `string` | yes |
| `mediumThumbnailUrl` | `string` | yes |
| `largeThumbnailUrl` | `string` | yes |
| `iconUrl` | `string` | yes |
| `thumbnailAspectRatio` | `number` | yes |
| `isThumbnailLargeAvailable` | `boolean` | no |
| `isThumbnailMediumAvailable` | `boolean` | no |
| `isThumbnailSmallAvailable` | `boolean` | no |
| `isIconAvailable` | `boolean` | no |
| `localizedTags` | `ILocalizedStringsArrayModel` | no |
| `parentFolderIds` | `string[]` | yes |
| `childFolderCount` | `number` | yes |
| `childFolderIds` | `string[]` | yes |
| `childFolders` | `IFolderDataObject[]` | yes |
| `rawData` | `IRawDataObject[]` | yes |
| `permissionUuids` | `string[]` | yes |

---

## Page references

### Models

#### `VueRouterLocation`

| Field | Type | Optional |
|---|---|---|
| `name` | `string` | yes |
| `path` | `string` | yes |
| `hash` | `string` | yes |
| `query` | `VueRouterDictionary<string \| (string \| null)[] \| null \| undefined>` | yes |
| `params` | `VueRouterDictionary<string>` | yes |
| `append` | `boolean` | yes |
| `replace` | `boolean` | yes |

#### `IPageReference`

Extends `IDataObject`.

| Field | Type | Optional |
|---|---|---|
| `to` | `VueRouterRawLocation` | no |

---

## Menu items

### Models

#### `IMenuItem`

Extends `IDataObject`, `IPageReference`.

| Field | Type | Optional |
|---|---|---|
| `title` | `ILocalizedStringsModel` | yes |
| `image` | `IImage` | yes |
| `imageForTransparentHeader` | `IImage` | yes |
| `icon` | `string` | yes |
| `graphicShowText` | `boolean` | yes |
| `backgroundImage` | `IImage` | yes |
| `backgroundImageSize` | `IEnumDataObject` | yes |
| `backgroundImageColor` | `string` | yes |
| `backgroundImageViewport` | `IEnumDataObject` | yes |
| `backgroundImageOnHover` | `IImage` | yes |
| `backgroundImageOnHoverSize` | `IEnumDataObject` | yes |
| `backgroundImageOnHoverColor` | `string` | yes |
| `backgroundImageOnHoverViewport` | `IEnumDataObject` | yes |
| `headerButtonStyle` | `ButtonStyle` | yes |
| `urls` | `ILocalizedStringsModel` | yes |
| `effectiveUrl` | `string` | yes |
| `effectiveUrlIsPageReference` | `boolean` | no |
| `effectiveUrlIsAbsoluteUrl` | `boolean` | no |
| `effectiveUrlIsExternal` | `boolean` | no |
| `isOnlySubmenuContainer` | `boolean` | no |
| `subMenuItems` | `IMenuItem[]` | yes |
| `disabled` | `boolean` | yes |

---

## Resource types

### Models

#### `IResourceDataObject`

Extends `IDataObject`.

| Field | Type | Optional |
|---|---|---|
| `id` | `string` | yes |
| `version` | `string` | yes |
| `name` | `ILocalizedStringsModel` | yes |
| `description` | `ILocalizedStringsModel` | yes |
| `resourceType` | `string` | yes |
| `localizedResourceAssets` | `ILocalizedResourceAssetsModel` | yes |
| `subResources` | `IResourceDataObject[]` | yes |
| `stringMetadata` | `IStringMetadataDataObject` | yes |
| `textMetadata` | `ITextMetadataDataObject` | yes |
| `menuItemMetadata` | `IMenuItem` | yes |
| `imageMetadata` | `IResourceImageMetadataDataObject` | yes |
| `videoMetadata` | `IResourceVideoMetadataDataObject` | yes |
| `emailMetadata` | `IResourceEmailMetadataDataObject` | yes |
| `categoryMetadata` | `IResourceCategoryMetadataDataObject` | yes |

#### `ILocalizedResourceAssetsModel`

Marker interface, no members of its own.

#### `ILocalizedResourceAssetDataObject`

Extends `IDataObject`.

| Field | Type | Optional |
|---|---|---|
| `version` | `string` | yes |
| `mediaType` | `string` | yes |
| `highResUrl` | `string` | yes |
| `smallThumbnailUrl` | `string` | yes |
| `largeThumbnailUrl` | `string` | yes |
| `playbackSmallUrl` | `string` | yes |
| `playbackLargeUrl` | `string` | yes |
| `originalWidth` | `number` | yes |
| `originalHeight` | `number` | yes |
| `isHighResAvailable` | `boolean` | yes |
| `isThumbnailLargeAvailable` | `boolean` | yes |
| `isThumbnailSmallAvailable` | `boolean` | yes |
| `isPlaybackLargeAvailable` | `boolean` | yes |
| `isPlaybackSmallAvailable` | `boolean` | yes |

#### `IStringMetadataDataObject`

Extends `IDataObject`.

| Field | Type | Optional |
|---|---|---|
| `text` | `ILocalizedStringsModel` | yes |

#### `IString`

Extends `IResourceDataObject`.

| Field | Type | Optional |
|---|---|---|
| `resourceType` | `"string",` | no |
| `stringMetadata` | `IStringMetadataDataObject` | no |

#### `ITextMetadataDataObject`

Extends `IDataObject`.

| Field | Type | Optional |
|---|---|---|
| `text` | `ILocalizedStringsModel` | yes |

#### `IText`

Extends `IResourceDataObject`.

| Field | Type | Optional |
|---|---|---|
| `resourceType` | `"text",` | no |
| `textMetadata` | `ITextMetadataDataObject` | no |

#### `IResourceImageMetadataDataObject`

Extends `IDataObject`.

| Field | Type | Optional |
|---|---|---|
| `altText` | `ILocalizedStringsModel` | yes |
| `caption` | `ILocalizedStringsModel` | yes |

#### `IImage`

Extends `IResourceDataObject`.

| Field | Type | Optional |
|---|---|---|
| `resourceType` | `"image",` | no |
| `imageMetadata` | `IResourceImageMetadataDataObject` | no |

#### `IResourceVideoMetadataDataObject`

Extends `IDataObject`.

| Field | Type | Optional |
|---|---|---|
| `altText` | `ILocalizedStringsModel` | yes |
| `caption` | `ILocalizedStringsModel` | yes |

#### `IVideo`

Extends `IResourceDataObject`.

| Field | Type | Optional |
|---|---|---|
| `resourceType` | `"video",` | no |
| `videoMetadata` | `IResourceVideoMetadataDataObject` | no |

#### `IResourceEmailMetadataDataObject`

Extends `IDataObject`.

| Field | Type | Optional |
|---|---|---|
| `subject` | `ILocalizedStringsModel` | yes |
| `title` | `ILocalizedStringsModel` | yes |
| `header` | `ILocalizedStringsModel` | yes |
| `textBeforeCallToAction` | `ILocalizedStringsModel` | yes |
| `callToAction` | `ILocalizedStringsModel` | yes |
| `textAfterCallToAction` | `ILocalizedStringsModel` | yes |
| `footer` | `ILocalizedStringsModel` | yes |

#### `IEmail`

Extends `IResourceDataObject`.

| Field | Type | Optional |
|---|---|---|
| `resourceType` | `"email",` | no |
| `emailMetadata` | `IResourceEmailMetadataDataObject` | no |

#### `IResourceCategoryMetadataDataObject`

Extends `IDataObject`, `IPageReference`.

| Field | Type | Optional |
|---|---|---|
| `image` | `IImage` | yes |
| `imageSize` | `IEnumDataObject` | yes |
| `imageColor` | `string` | yes |
| `imageViewport` | `IEnumDataObject` | yes |
| `header` | `ILocalizedStringsModel` | yes |
| `subHeader` | `ILocalizedStringsModel` | yes |
| `continuousText` | `ILocalizedStringsModel` | yes |
| `linkText` | `ILocalizedStringsModel` | yes |
| `urls` | `ILocalizedStringsModel` | yes |
| `effectiveUrl` | `string` | yes |
| `effectiveUrlIsPageReference` | `boolean` | no |
| `effectiveUrlIsAbsoluteUrl` | `boolean` | no |
| `effectiveUrlIsExternal` | `boolean` | no |
| `disabled` | `boolean` | yes |

#### `ICategory`

Extends `IResourceDataObject`.

| Field | Type | Optional |
|---|---|---|
| `resourceType` | `"category",` | no |
| `categoryMetadata` | `IResourceCategoryMetadataDataObject` | no |

---

Contributors
============

- Reinhard Holzner, Smint.io GmbH
- Yosif Velev, Smint.io GmbH

Page type contracts
===================

What a UI component receives from the page that hosts it, per page type — generated from
the `ui-slot-data` bindings in every page template in this repository.

A UI component whose `type` belongs to a page (a search result, a search bar, an asset
details viewer) is handed data by its page template as ordinary Vue props, and reports back
by emitting events. **There is no separate declaration of this contract** — the page
template's bindings are the contract, which is why this file is generated from them.

Declare what you consume:

```ts
@Prop()
public readonly currentSearch!: IFormFieldValuesModel;
```

and emit what you produce:

```ts
this.$emit("query-string-changed", this.searchQuery);
```

Names given in `camelCase` in the tables arrive as camelCase props; Vue also accepts the
kebab-case spelling. Event names are always kebab-case.

If two page templates implement the same page type, they should offer the same contract. A
difference between them in the tables below is worth treating as a bug in one of them.

---

## `page-type-account-accept-terms`

### Accept terms

`smintio-page-generic-account-accept-terms-1`

| Slot | Accepts | Props passed in | Events listened for |
|---|---|---|---|
| `header` (0–1) | `ui-type-header` | — | — |
| `top` | any | — | — |
| `form` (1–1) | `ui-type-account-accept-terms-form` | — | — |
| `bottom` | any | — | — |
| `footer` (0–1) | `ui-type-footer` | — | — |

---

## `page-type-account-access-denied`

### Access denied

`smintio-page-generic-account-access-denied-1`

| Slot | Accepts | Props passed in | Events listened for |
|---|---|---|---|
| `header` (0–1) | `ui-type-header` | — | — |
| `top` | any | — | — |
| `form` (1–1) | `ui-type-account-access-denied-form` | — | — |
| `bottom` | any | — | — |
| `footer` (0–1) | `ui-type-footer` | — | — |

---

## `page-type-account-confirm-email`

### Confirm email address

`smintio-page-generic-account-confirm-email-1`

| Slot | Accepts | Props passed in | Events listened for |
|---|---|---|---|
| `header` (0–1) | `ui-type-header` | — | — |
| `top` | any | — | — |
| `form` (1–1) | `ui-type-account-confirm-email-form` | — | — |
| `bottom` | any | — | — |
| `footer` (0–1) | `ui-type-footer` | — | — |

---

## `page-type-account-cookie-consent`

### Cookie consent banner

`smintio-page-generic-account-cookie-consent-1`

| Slot | Accepts | Props passed in | Events listened for |
|---|---|---|---|
| `banner` (1–1) | `ui-type-account-cookie-consent` | `dialogData` | — |

---

## `page-type-account-email-confirmation-sent`

### Email confirmation message sent

`smintio-page-generic-account-email-confirmation-sent-1`

| Slot | Accepts | Props passed in | Events listened for |
|---|---|---|---|
| `header` (0–1) | `ui-type-header` | — | — |
| `top` | any | — | — |
| `form` (1–1) | `ui-type-account-email-confirmation-sent-form` | — | — |
| `bottom` | any | — | — |
| `footer` (0–1) | `ui-type-footer` | — | — |

---

## `page-type-account-email-not-confirmed`

### Email not yet confirmed

`smintio-page-generic-account-email-not-confirmed-1`

| Slot | Accepts | Props passed in | Events listened for |
|---|---|---|---|
| `header` (0–1) | `ui-type-header` | — | — |
| `top` | any | — | — |
| `form` (1–1) | `ui-type-account-email-not-confirmed-form` | — | — |
| `bottom` | any | — | — |
| `footer` (0–1) | `ui-type-footer` | — | — |

---

## `page-type-account-forgot-password`

### Forgot password

`smintio-page-generic-account-forgot-password-1`

| Slot | Accepts | Props passed in | Events listened for |
|---|---|---|---|
| `header` (0–1) | `ui-type-header` | — | — |
| `top` | any | — | — |
| `form` (1–1) | `ui-type-account-forgot-password-form` | — | — |
| `bottom` | any | — | — |
| `footer` (0–1) | `ui-type-footer` | — | — |

---

## `page-type-account-forgot-password-confirmation`

### Password reset message sent

`smintio-page-generic-account-forgot-password-confirmation-1`

| Slot | Accepts | Props passed in | Events listened for |
|---|---|---|---|
| `header` (0–1) | `ui-type-header` | — | — |
| `top` | any | — | — |
| `form` (1–1) | `ui-type-account-forgot-password-confirmation-form` | — | — |
| `bottom` | any | — | — |
| `footer` (0–1) | `ui-type-footer` | — | — |

---

## `page-type-account-invitation-collection`

### Accept or decline invitation to collection

`smintio-page-generic-account-invitation-collection-1`

| Slot | Accepts | Props passed in | Events listened for |
|---|---|---|---|
| `header` (0–1) | `ui-type-header` | — | — |
| `top` | any | — | — |
| `form` (1–1) | `ui-type-account-invitation-collection-form` | — | — |
| `bottom` | any | — | — |
| `footer` (0–1) | `ui-type-footer` | — | — |

---

## `page-type-account-login`

### Login

`smintio-page-generic-account-login-1`

| Slot | Accepts | Props passed in | Events listened for |
|---|---|---|---|
| `header` (0–1) | `ui-type-header` | — | — |
| `top` | any | — | — |
| `form` (1–1) | `ui-type-account-login-form` | — | — |
| `bottom` | any | — | — |
| `footer` (0–1) | `ui-type-footer` | — | — |

---

## `page-type-account-logout`

### Logout

`smintio-page-generic-account-logout-1`

| Slot | Accepts | Props passed in | Events listened for |
|---|---|---|---|
| `header` (0–1) | `ui-type-header` | — | — |
| `top` | any | — | — |
| `form` (1–1) | `ui-type-account-logout-form` | — | — |
| `bottom` | any | — | — |
| `footer` (0–1) | `ui-type-footer` | — | — |

---

## `page-type-account-manage`

### Manage account

`smintio-page-generic-account-manage-1`

| Slot | Accepts | Props passed in | Events listened for |
|---|---|---|---|
| `header` (0–1) | `ui-type-header` | — | — |
| `top` | any | — | — |
| `form` (1–1) | `ui-type-account-manage-form` | — | — |
| `bottom` | any | — | — |
| `footer` (0–1) | `ui-type-footer` | — | — |

---

## `page-type-account-manage-change-password`

### Change password

`smintio-page-generic-account-manage-change-password-1`

| Slot | Accepts | Props passed in | Events listened for |
|---|---|---|---|
| `header` (0–1) | `ui-type-header` | — | — |
| `top` | any | — | — |
| `form` (1–1) | `ui-type-account-manage-change-password-form` | — | — |
| `bottom` | any | — | — |
| `footer` (0–1) | `ui-type-footer` | — | — |

---

## `page-type-account-register`

### Register

`smintio-page-generic-account-register-1`

| Slot | Accepts | Props passed in | Events listened for |
|---|---|---|---|
| `header` (0–1) | `ui-type-header` | — | — |
| `top` | any | — | — |
| `form` (1–1) | `ui-type-account-register-form` | — | — |
| `bottom` | any | — | — |
| `footer` (0–1) | `ui-type-footer` | — | — |

---

## `page-type-account-reset-password`

### Reset password

`smintio-page-generic-account-reset-password-1`

| Slot | Accepts | Props passed in | Events listened for |
|---|---|---|---|
| `header` (0–1) | `ui-type-header` | — | — |
| `top` | any | — | — |
| `form` (1–1) | `ui-type-account-reset-password-form` | — | — |
| `bottom` | any | — | — |
| `footer` (0–1) | `ui-type-footer` | — | — |

---

## `page-type-account-reset-password-confirmation`

### Password has been reset

`smintio-page-generic-account-reset-password-confirmation-1`

| Slot | Accepts | Props passed in | Events listened for |
|---|---|---|---|
| `header` (0–1) | `ui-type-header` | — | — |
| `top` | any | — | — |
| `form` (1–1) | `ui-type-account-reset-password-confirmation-form` | — | — |
| `bottom` | any | — | — |
| `footer` (0–1) | `ui-type-footer` | — | — |

---

## `page-type-asset-details`

### Asset details with side flaps

`smintio-page-media-gallery-asset-details-1`

| Slot | Accepts | Props passed in | Events listened for |
|---|---|---|---|
| `header` (0–1) | `ui-type-header` | — | — |
| `left` | any | `asset`, `assetId` | — |
| `content` (1–n) | any | `asset`, `assetId`, `buttonBackToSearchTarget`, `canNavigateNextResultItem`, `canNavigatePreviousResultItem`, `nextIsLoading`, `previousIsLoading`, `shareId`, `shareSecret` | `next-item`, `previous-item` |
| `right` | any | `asset`, `assetId` | — |
| `footer` (0–1) | `ui-type-footer` | — | — |

### Asset details with banner

`smintio-press-portal-asset-details-1`

| Slot | Accepts | Props passed in | Events listened for |
|---|---|---|---|
| `header` (0–1) | `ui-type-header` | `transparent`, `transparentHeaderLogo` | — |
| `left` | any | `asset`, `assetId` | — |
| `banner` (0–1) | `ui-type-asset-details-banner` | `asset`, `assetId`, `transparent-header` | — |
| `content` | any | `asset`, `assetId`, `buttonBackToSearchTarget`, `canNavigateNextResultItem`, `canNavigatePreviousResultItem`, `nextIsLoading`, `previousIsLoading`, `shareId`, `shareSecret`, `transparent-header` | `next-item`, `previous-item` |
| `right` | any | `asset`, `assetId` | — |
| `footer` (0–1) | `ui-type-footer` | — | — |

---

## `page-type-assets-search`

### Search assets (Media center - Imagination)

`smintio-page-media-gallery-assets-search-1`

| Slot | Accepts | Props passed in | Events listened for |
|---|---|---|---|
| `header` (0–1) | `ui-type-header` | `is-loading` | — |
| `left` (0–1) | `ui-type-search-form` | `currentItemsPerPage`, `currentPage`, `currentQueryString`, `currentSearch`, `errors`, `folderNavigationEnabled`, `form-groups-definition`, `hasMoreResults`, `initialLoading`, `is-loading-results`, `isSearching`, `maxPages`, `parentFolderId`, `parentFolderName`, `results`, `searchUuid`, `totalResults` | `clear-query`, `remove-search-value`, `search-changed` |
| `search-bar` (0–1) | `ui-type-search-bar` | `currentSearch` | `query-string-changed` |
| `search-result` (1–1) | `ui-type-search-result` | `currentItemsPerPage`, `currentPage`, `currentQueryString`, `currentSearch`, `errors`, `folderNavigationEnabled`, `form-groups-definition-model`, `hasMoreResults`, `initialLoading`, `is-loading-results`, `isSearching`, `parentFolderId`, `parentFolderName`, `results`, `searchUuid`, `totalResults` | `clear-query`, `collection-created`, `collection-selected`, `next-search-page`, `remove-search-value` |
| `right` (0–1) | `ui-type-collections-quickview` | — | `collection-created`, `collection-deleted`, `collection-selected` |
| `footer` (0–1) | `ui-type-footer` | — | — |

---

## `page-type-collection-details`

### Collection details

`smintio-page-generic-collection-details-1`

| Slot | Accepts | Props passed in | Events listened for |
|---|---|---|---|
| `header` (0–1) | `ui-type-header` | — | — |
| `top` | any | `collection`, `collectionId` | — |
| `right` (0–1) | `ui-type-collection-comments` | `asset`, `collection`, `isOpen` | `asset-comment-deleted`, `asset-comment-posted`, `close`, `collection-comment-deleted`, `collection-comment-posted` |
| `details` (1–1) | `ui-type-collection-details` | `collection`, `collectionId` | `collection-updated`, `open-comments-for-asset`, `toggle-comments` |
| `bottom` | any | `collection`, `collectionId` | — |
| `footer` (0–1) | `ui-type-footer` | — | — |

---

## `page-type-collections-overview`

### Collections overview

`smintio-page-generic-collections-overview-1`

| Slot | Accepts | Props passed in | Events listened for |
|---|---|---|---|
| `header` (0–1) | `ui-type-header` | `is-loading` | — |
| `top` | any | — | — |
| `overview` (1–1) | `ui-type-collections-overview` | — | — |
| `bottom` | any | — | — |
| `footer` (0–1) | `ui-type-footer` | — | — |

---

## `page-type-error`

### Error

`smintio-page-generic-error-1`

| Slot | Accepts | Props passed in | Events listened for |
|---|---|---|---|
| `header` (0–1) | `ui-type-header` | `hard-load-links` | — |
| `top` | any | — | — |
| `form` (1–1) | `ui-type-error-form` | — | — |
| `bottom` | any | — | — |
| `footer` (0–1) | `ui-type-footer` | — | — |

---

## `page-type-generic`

### Content page

`smintio-page-generic-1`

| Slot | Accepts | Props passed in | Events listened for |
|---|---|---|---|
| `header` (0–1) | `ui-type-header` | — | — |
| `left` | any | — | — |
| `content` (1–n) | any | — | — |
| `right` | any | — | — |
| `footer` (0–1) | `ui-type-footer` | — | — |

---

## `page-type-generic-dialog`

### Dialog

`smintio-page-generic-dialog-1`

| Slot | Accepts | Props passed in | Events listened for |
|---|---|---|---|
| `top` | any | — | — |
| `content` | any | `dialogData` | — |
| `bottom` | any | — | — |

---

## `page-type-imprint`

### Imprint page

`smintio-page-generic-imprint-1`

| Slot | Accepts | Props passed in | Events listened for |
|---|---|---|---|
| `header` (0–1) | `ui-type-header` | — | — |
| `left` | any | — | — |
| `content` (1–n) | any | — | — |
| `right` | any | — | — |
| `footer` (0–1) | `ui-type-footer` | — | — |

---

## `page-type-main`

### Cover page

`smintio-page-media-gallery-main-1`

| Slot | Accepts | Props passed in | Events listened for |
|---|---|---|---|
| `header` (0–1) | `ui-type-header` | `transparent`, `transparentHeaderLogo` | — |
| `left` | any | `transparent-header` | — |
| `banner` | `ui-type-banner`, `ui-type-section` | `transparent-header` | — |
| `content` | any | — | — |
| `right` | any | `transparent-header` | — |
| `footer` (0–1) | `ui-type-footer` | — | — |

---

## `page-type-mpa`

### System page

`smintio-page-generic-mpa-1`

| Slot | Accepts | Props passed in | Events listened for |
|---|---|---|---|
| `header` (0–1) | `ui-type-header` | — | — |
| `form` (1–n) | `ui-type-mpa-form` | — | — |
| `content` | any | — | — |
| `footer` (0–1) | `ui-type-footer` | — | — |

---

## `page-type-privacy-policy`

### Privacy policy page

`smintio-page-generic-privacy-policy-1`

| Slot | Accepts | Props passed in | Events listened for |
|---|---|---|---|
| `header` (0–1) | `ui-type-header` | — | — |
| `left` | any | — | — |
| `content` (1–n) | any | — | — |
| `right` | any | — | — |
| `footer` (0–1) | `ui-type-footer` | — | — |

---

## `page-type-request-access`

### Request access

`smintio-page-generic-request-access-1`

| Slot | Accepts | Props passed in | Events listened for |
|---|---|---|---|
| `header` (0–1) | `ui-type-header` | — | — |
| `top` | any | — | — |
| `form` (1–1) | `ui-type-request-access-form` | — | — |
| `bottom` | any | — | — |
| `footer` (0–1) | `ui-type-footer` | — | — |

---

## `page-type-request-download`

### Request download

`smintio-page-generic-request-download-1`

| Slot | Accepts | Props passed in | Events listened for |
|---|---|---|---|
| `top` | any | — | — |
| `form` (1–1) | `ui-type-request-download-form` | `dialogData` | — |
| `bottom` | any | — | — |

---

## `page-type-request-permission`

### Request permission

`smintio-page-generic-request-permission-1`

| Slot | Accepts | Props passed in | Events listened for |
|---|---|---|---|
| `header` (0–1) | `ui-type-header` | — | — |
| `top` | any | — | — |
| `form` (1–1) | `ui-type-request-permission-form` | — | — |
| `bottom` | any | — | — |
| `footer` (0–1) | `ui-type-footer` | — | — |

---

## `page-type-share-details`

### Share link details

`smintio-page-generic-share-details-1`

| Slot | Accepts | Props passed in | Events listened for |
|---|---|---|---|
| `header` (0–1) | `ui-type-header` | — | — |
| `top` | any | `collection`, `collectionId`, `shareConsumer`, `shareId`, `shareSecret` | — |
| `right` (0–1) | `ui-type-collection-comments` | `asset`, `collection`, `isOpen` | `asset-comment-deleted`, `asset-comment-posted`, `close`, `collection-comment-deleted`, `collection-comment-posted` |
| `details` (1–1) | `ui-type-share-details` | `collection`, `collectionId`, `shareConsumer`, `shareId`, `shareSecret` | `collection-updated`, `open-comments-for-asset`, `toggle-comments` |
| `bottom` | any | `collection`, `collectionId`, `shareConsumer`, `shareId`, `shareSecret` | — |
| `footer` (0–1) | `ui-type-footer` | — | — |

---

## `page-type-shared-links`

### Share links overview

`smintio-page-generic-shared-links-1`

| Slot | Accepts | Props passed in | Events listened for |
|---|---|---|---|
| `header` (0–1) | `ui-type-header` | `is-loading` | — |
| `top` | any | — | — |
| `links` (1–1) | `ui-type-shared-links` | — | `update:is-loading-results` |
| `bottom` | any | — | — |
| `footer` (0–1) | `ui-type-footer` | — | — |

---

## `page-type-terms-and-conditions`

### Terms and conditions page

`smintio-page-generic-terms-and-conditions-1`

| Slot | Accepts | Props passed in | Events listened for |
|---|---|---|---|
| `header` (0–1) | `ui-type-header` | — | — |
| `left` | any | — | — |
| `content` (1–n) | any | — | — |
| `right` | any | — | — |
| `footer` (0–1) | `ui-type-footer` | — | — |

---

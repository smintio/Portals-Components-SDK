Smint.io Portals frontend reference
==================================

Current version of this document is: 1.0.0 (as of 10th of September, 2026)

Lookup tables for building Smint.io Portals frontend components: the services you can inject,
the template filters and CSS classes the runtime provides, the shared property mixins you
should reuse instead of re-declaring properties, and the type and provider names you will
need when writing annotations.

For the annotations themselves see [smintio-annotations.md](smintio-annotations.md); for the
component and page types see
[smintio-frontend-component-types.md](smintio-frontend-component-types.md); for the asset
action mixins see [smintio-mixins.md](smintio-mixins.md).

- [Global services](#global-services)
- [Global filters](#global-filters)
- [Configuration property mixins](#configuration-property-mixins)
- [Shared components](#shared-components)
- [Dynamic allowed values providers](#dynamic-allowed-values-providers)
- [`Implements` type names](#implements-type-names)
- [Data types](#data-types)
- [Global CSS classes](#global-css-classes)

---

## Global services

Inject these into your component with `@PortalsInject(PortalsGlobalServices.X)`. The page
context is normally injected with Vue's `@InjectReactive` instead, because it changes as the
visitor navigates.

```javascript
import type { IPortalsContext, IErrorHandler, ITranslator } from "@smintio/portals-component-sdk";
import { PortalsGlobalServices, PortalsInject } from "@smintio/portals-component-sdk";
import { InjectReactive } from "vue-property-decorator";

@PortalsInject(PortalsGlobalServices.PortalsContext)
public readonly portalsContext!: IPortalsContext;

@PortalsInject(PortalsGlobalServices.ErrorHandler)
public readonly errorHandler!: IErrorHandler;

@InjectReactive(PortalsGlobalServices.PageContext)
public readonly pageContext!: IPortalsPageContext;
```

| Service | Interface | What it gives you |
|---|---|---|
| `PortalsContext` | `IPortalsContext` | the portal: `currentPortalUuid`, `portalsName`, `locale`, `tenant`, `user`, `storage`, `services`, `backendWebUrl`, `recaptchaSiteKey`, `notificationStatus` |
| `PageContext` | `IPortalsPageContext` | the current page: `pageType`, `title`, `description`, `path`, `canonicalUrls`, `uiDesignSpecification`, `getUIComponentsForSlot(slotId)` |
| `Translator` | `ITranslator` | `localizeStrings()`, `localizeResourceAsset()`, `getVueI18N()` |
| `ErrorHandler` | `IErrorHandler` | `captureExceptionAndDisplayMessage()` — route every caught failure here |
| `NotificationDialog` | | show a notification dialog |
| `EventBus` | `IEventBus` | events between components |
| `AnalyticsProvider` | `IAnalyticsProvider` | tracking |
| `SearchCacheProvider` | | the search result cache, used for asset details navigation |
| `DataObjectParser` | | parse data object payloads |
| `DataObjectMetadataFormatter` | | format metadata values for display |
| `PageReferenceResolver` | | resolve an `IPageReference` to a route |
| `ConsentProvider` | | cookie and consent state |
| `PortalsStorage` | | portal scoped storage |
| `PortalsInjector` | | the injector itself, for anything not listed here |

`portalsContext.services` groups the data adapter backed services the portal has configured:
`assets` (`read` / `search` / `folderNavigation` / `download`), `resourceAssets`,
`productAssets`, `collections`, `collectionsAndAssets`, `collectionsAndUsers`, `shares` and
`permissions`.

`portalsContext.services.permissions` answers the questions you should ask before offering an
action to the visitor:

```javascript
hasReadAssetDetailsPermission(asset)
hasDownloadAssetHiResPermission(asset)
hasDownloadAssetLayoutFilePermission(asset)
hasDownloadPermission(asset)
hasAssetPermission(asset, permissionUuid)
```

Please note that permissions are always enforced on the backend as well. Checking them in the
frontend is for the user experience, not for security.

---

## Global filters

The Smint.io Portals runtime registers these Vue filters globally. You do not import or
declare them.

| Filter | Use |
|---|---|
| `resolve_localized` | resolve an `ILocalizedStringsModel` or `ILocalizedResourceAssetsModel` for the visitor's language. Supports pluralization choices and `{placeholder}` substitution |
| `translate` | translate a key |
| `number` | locale formatted number |
| `yes_no` | boolean to a localized Yes / No |
| `disabled` | boolean to a localized Enabled / Disabled |
| `emptify` | the value, or `-` if empty |
| `naify` | the value, or a localized "n/a" if empty |
| `value_check` | the value, or `-` if falsy |
| `date_time`, `date`, `time`, `time_ago` | localized date and time formatting |

```vue
<h1 class="s-section-title">{{ headerText | resolve_localized }}</h1>
```

To use a filter outside an interpolation — which you need for rich text — call it through
`$options.filters`:

```vue
<p v-html="$options.filters.resolve_localized(continuousText)"></p>
```

The runtime also provides `this.$_` (lodash) alongside the usual `this.$router` and
`this.$route`.

---

## Configuration property mixins

`@smintio/portals-components` ships ready made configuration mixins. Mixing one in gives you
its properties, their translations, their form group and, where applicable, the matching
behaviour — in a single line. Please prefer them over declaring the same properties again, so
that portal editors see the same fields with the same names across all components.

Each mixin exposes its form group id as a static, so you can put your own properties into the
same group:

```javascript
@FormGroup(SBottomGapProps.FormGroupId)
```

### Mixins that contribute properties

| Mixin | Form group | Contributes |
|---|---|---|
| `STextProps` | `s-text-props` | `headerText`, `subHeaderText`, `continuousText` (rich text), `alignment` |
| `SLinkButtonProps` | `s-link-button-props` | `linkType`, `linkText`, `page`, `queryParameters`, `url` |
| `SSecondaryLinkButtonProps` | `s-secondary-link-button-props` | the same five, prefixed `secondary` |
| `SUiComponentColorsProps` | `s-colors-props` | 7 per component color overrides (`uiComponentBackgroundColor`, `uiComponentPrimaryColor`, …) |
| `SPageTemplateColorsProps` | `s-colors-props` | 15 page level colors (header, footer, primary, secondary, background, input, text on …) |
| `SBottomGapProps` | `s-layout-props` | `bottomGap` |
| `SAssetDetailBottomGapProps` | `s-layout-props` | `bottomGap` for asset detail components |
| `SContentWidthProps` | `s-layout-props` | `slotWidthPixelsDesktop`, `slotWidthPixelsMobile`, `contentWidthDesktop`, `contentWidthMobile` — for page templates |
| `SHtmlProps` | `s-html-props` | `className`, `anchorName` |
| `SCssProps` | `s-html-props` | `className` only |
| `SDownloadProps` | `s-download-props` | 15 download dialog texts, plus the download behaviour |
| `SRememberProps` | `s-remember-props` | 7 collect dialog texts, plus the collect behaviour |
| `SShareProps` | `s-share-props` | 27 share dialog texts, plus the share behaviour |
| `SQuickViewProps` | `s-quick-view-props` | large file warning texts and the size limit |
| `SCiHubProps` | `s-ci-hub-props` | CI Hub publishing settings |

> `SCssProps` and `SHtmlProps` share a form group id and both declare `className`. Mix in
> exactly one of them: `SHtmlProps` if your component should support in-page anchors,
> `SCssProps` if it should not.

### Mixins that only declare a form group

These contribute no properties. They exist so that several components can put their own
properties into one consistently named group. Mix one in when you add properties to that
group:

`SAssetProps`, `SAssetsProps`, `SBackgroundProps`, `SCategoriesProps`, `SCategoryProps`,
`SDataSourceProps`, `SDataSourcesProps`, `SEffectsProps`, `SImageProps`, `SLayoutProps`,
`SLinkProps`, `SLinksProps`, `SMenuItemProps`, `SMenuItemsProps`, `SPressReleaseProps`,
`SPressReleasesProps`, `STextsProps`, `SVideoProps`.

---

## Shared components

Also from `@smintio/portals-components`. The dialogs are registered globally by the runtime,
so use their kebab-case tags directly without importing them. The rest you import and list
under `components` in your component metadata.

| Group | Components |
|---|---|
| Slots — for page templates | `SHeaderSlot`, `SFooterSlot`, `SSideSlot`, `SSideMenuSlot`, `SBannerSlot`, `SGenericSlot`, `SGenericMultiSlot` |
| Dialogs | `SDownloadDialog`, `SRememberDialog`, `SCreateCollectionDialog`, `SCollectionUsersDialog`, `SDialogWrapper` |
| Text | `SText`, `SImageWithText` |
| Asset preview | `SImagePreview`, `SVideoPreview`, `SVideoPlayer`, `VideoPlayer`, `SAudioPreview`, `SAudioPlayer`, `SPdfPreview`, `SQuickViewPdfModal` |
| Assets | `STags` |
| Collections | `SCollectionCard`, `SCollectionGallery`, `SCollectionControls`, `SCollectionSelector`, `SCollectionEditableName`, `SCollectionAssetPreviewControls`, `SCollectionQuickViewControls`, `SCollectionItems`, `SCollectionItemPreview`, `SRate`, `SRateFilter` |
| Search and gallery | `SSearchInput`, `SSearchResult`, `SActiveFilters`, `SActionBar`, `SCheckbox`, `SCheckboxPlaceholder`, `SMobilePreviewIndicator`, `SearchFilter`, `SearchFilterSkeleton` |
| Forms | `SForm`, `SFormErrorDisplay` |
| Other | `SCategory`, `SColor`, `SImageCross`, `SSideMenu`, `STaskActionDescriptor` |

### Slot renderer props

`SGenericSlot`, `SHeaderSlot`, `SFooterSlot`, `SSideSlot` and `SBannerSlot` all take
`ui-slot` (the `IUIComponentInfo[]` from your `@UIComponentSlot` field) and `ui-slot-data`
(a Vue render data object with `props`, `on`, `attrs`, `class`, `style`), plus these layout
controls:

`container-fluid`, `content-width-desktop`, `content-width-mobile`,
`default-content-width-desktop`, `default-content-width-mobile`, `max-width-desktop`,
`max-width-mobile`, `default-max-width-desktop`, `default-max-width-mobile`,
`default-padding-desktop`, `default-padding-mobile`, `content-gap`, `apply-ui-component-gap`,
`apply-top-content-gap`, `apply-bottom-content-gap`, `slot-tag`.

`SGenericMultiSlot` renders three stacked areas in one go: `top-ui-slot`, `center-ui-slot`
and `bottom-ui-slot`, each with its own `*-ui-slot-data`.

`SSideMenuSlot` adds `v-model`, `right`, `width`, `activator-text`, `activator`,
`collapse-on-outside-click`, `force-on-desktop` and `wrapper-tag`.

---

## Dynamic allowed values providers

The second argument to `@DynamicAllowedValuesProvider`. The first argument is almost always
`PortalsGlobalServices.PortalsContext.toString()`. Each provider makes the portal editor
choose from something that exists in their portal, rather than typing a value.

```javascript
@DynamicAllowedValuesProvider(
    PortalsGlobalServices.PortalsContext.toString(),
    "StringResourceAllowedValuesProvider"
)
```

| Provider | Offers the portal editor |
|---|---|
| `StringResourceAllowedValuesProvider` | the portal's string resources — plain texts: labels, button captions, headings |
| `TextResourceAllowedValuesProvider` | the portal's text resources — rich texts: long form body copy carrying markup |
| `ImageResourceAllowedValuesProvider` | image resources |
| `VideoResourceAllowedValuesProvider` | video resources |
| `CategoryResourceAllowedValuesProvider` | category resources |
| `MenuItemResourceAllowedValuesProvider` | menu item resources |
| `MetadataAttributeAllowedValuesProvider` | metadata attributes of the data source |
| `TextMetadataAttributeAllowedValuesProvider` | text typed metadata attributes |
| `LinkMetadataAttributeAllowedValuesProvider` | link typed metadata attributes |
| `DataObjectArrayMetadataAttributeAllowedValuesProvider` | data object array metadata attributes |
| `AssetIdAllowedValuesProvider` | a specific asset |
| `AssetsReferenceAllowedValuesProvider` | an assets reference (individual assets, a folder, or a search) |
| `ResourceAssetsReferenceAllowedValuesProvider` | a resource assets reference |
| `SearchGroupAllowedValuesProvider` | search groups |
| `FilterFragmentAllowedValuesProvider` | search filter fragments |
| `CustomFormAllowedValuesProvider` | custom forms configured in the portal |
| `PortalUserGroupAllowedValuesProvider` | portal user groups |
| `RequestAccessTaskHandlerAllowedValuesProvider` | access request handlers |
| `RequestPermissionTaskHandlerAllowedValuesProvider` | permission request handlers |
| `RequestDownloadTaskHandlerAllowedValuesProvider` | download request handlers |
| `RequestGenericTaskHandlerAllowedValuesProvider` | generic request handlers |
| `UploadAssetsTaskHandlerAllowedValuesProvider` | asset upload handlers |

---

## `Implements` type names

The string you pass to `@Implements(...)` is the name of the type on the backend. These are
the ones used across the Smint.io components:

| Type name | The property becomes |
|---|---|
| `ILocalizedStringsModel` | a localized text field — by far the most common |
| `IPageReference` | a page picker |
| `IImage`, `IVideo` | a resource picker |
| `IAssetIdentifier` | an asset picker |
| `IAssetsReferenceModel` | an assets reference: individual assets, a folder, or a configured search |
| `IMetadataAttributeModel` | a metadata attribute picker |
| `IAssetsSearch`, `IAssetsRead`, `IAssetsReadRandom`, `IAssetsUpload` | a data source picker, restricted to data adapters offering that capability |
| `ITaskManagement` | a task handler picker |
| *your own interface* | a data source implementing your own custom data adapter public API interface |

---

## Data types

The `ValueType` values, used with `@ImplementsPrimitiveType` and as the `dataType` in
`setFormFieldValues` in `resources/definition.ts`:

`unknown`, `string`, `string_array`, `localized_strings_model`,
`localized_strings_array_model`, `int32`, `int32_array`, `int64`, `int64_array`, `decimal`,
`boolean`, `date_time`, `data_object`, `data_object_array`, `data_adapter_instance_key`,
`data_adapter_instance_key_array`, `page_instance_key`, `resource_id`, `resource_id_array`,
`asset_id`, `asset_id_array`, `folder_id`, `folder_id_array`, `assets_reference_model`,
`search_assets_spec_model`, `metadata_attribute_model`, `metadata_attribute_model_array`,
`enumeration`, `enumeration_array`, `currency_model`, `geo_location_model`,
`localized_enums_array_model`.

The resource types are `image`, `video`, `audio` and `document` (file based — place the files
in your component's `resources` folder, where `loadFileResources` picks them up), and `string`
and `text` (not file based — declare these in `resources/definition.ts` with
`addEmbeddedResource`, see
[Shipping your own string resources](../README.md#shipping-your-own-string-resources)).

`string` is a **plain text** — a label, a button caption, a heading. `text` is a **rich text**,
long form body copy carrying markup, the kind paired with `IsRichText` and rendered with
`v-html`. Each is offered to the editor by its own provider, so match the property's provider
to the kind of resource you ship.

An `ILocalizedStringsModel` property is a resource reference **only** when it carries
`StringResourceAllowedValuesProvider` or `TextResourceAllowedValuesProvider`. Without one it is
an ordinary localized text field the editor types into, which is what you want for text used
only by your own component. Resources are for text that has to be **reusable across
components**. A property points at one either with `DefaultValue("<id>")` (nothing is written
to the database, so it can be changed later) or through `setFormFieldValues` in
`resources/definition.ts` (a real database write at instantiation, which later changes do not
reach).

---

## Global CSS classes

The portal application defines these classes and drives them from the portal's own design
definition — its fonts, colors and spacing scale. Please use them instead of hard coding
typography or spacing, so that your component follows the design of whatever portal it ends
up in.

| Class | For |
|---|---|
| `s-page-title` | page titles |
| `s-section-title` | section headings — also styles an `h1` inside it |
| `s-standard-text` | body copy |
| `s-header-text` | header bar text |
| `s-footer-text` | footer text |

For the standard gaps between components, in the scale `none`, `small`, `medium`, `large`,
`xlarge`, `xxlarge`, plus an unsuffixed default:

`s-mt-space-between-components[-size]`, `s-mb-space-between-components[-size]`,
`s-my-space-between-components[-size]`, `s-pt-space-between-components[-size]`,
`s-pb-space-between-components[-size]`, `s-py-space-between-components[-size]`

Everything else is Vuetify 2: its grid (`v-row`, `v-col`, `no-gutters`), its spacing
utilities (`ma-1`, `pa-3`, `mb-4`, …) and `$vuetify.breakpoint` for responsive behaviour.

Please remember the layout rule: your UI component fills the layout box it is given, and adds
no white space around its own content. Spacing between components is the page template's job.

Contributors
============

- Reinhard Holzner, Smint.io GmbH
- Yosif Velev, Smint.io GmbH

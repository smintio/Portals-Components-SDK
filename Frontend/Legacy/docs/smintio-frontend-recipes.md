Smint.io Portals frontend component recipes
===========================================

Current version of this document is: 2.0.1 (as of 15th of September, 2026)

Task-shaped answers to "how do I …?", each one complete enough to paste into a component and
adapt. The other frontend documents describe *what exists*; this one shows *how it is used*.

Every recipe names the symbols it uses, shows the whole of the interesting part — imports,
class, template, error handling — and ends with the mistakes that recipe invites. They assume
the component skeleton from
[developing frontend components](../README.md#user-content-how-to-develop-your-own-frontend-component);
only the parts that matter for the task are repeated here.

Two imports appear throughout:

| Package | What comes from it |
|---|---|
| `@smintio/portals-component-sdk` | the annotations, the injectable services, and every data adapter type (`IAssetsSearch`, `IAssetDataObject`, …) |
| `@smintio/portals-components` | the Vue mixins and the shared components (`SDownloadProps`, `SGenericSlot`, …) |

1. [How do I call a data adapter method and render the result?](#user-content-how-do-i-call-a-data-adapter-method-and-render-the-result) — basic
1. [How do I use the portal's own data sources instead of asking for one?](#user-content-how-do-i-use-the-portals-own-data-sources-instead-of-asking-for-one) — basic
1. [How do I turn an assets reference setting into real assets?](#user-content-how-do-i-turn-an-assets-reference-setting-into-real-assets) — basic
1. [How do I page through a search result?](#user-content-how-do-i-page-through-a-search-result) — advanced
1. [How do I show progress for a long-running call?](#user-content-how-do-i-show-progress-for-a-long-running-call) — advanced
1. [How do I report an error?](#user-content-how-do-i-report-an-error) — basic
1. [How do I localize text from script, with placeholders and plurals?](#user-content-how-do-i-localize-text-from-script-with-placeholders-and-plurals) — basic
1. [How do I make one setting depend on another?](#user-content-how-do-i-make-one-setting-depend-on-another) — basic
1. [How do I restrict my component to certain portal types?](#user-content-how-do-i-restrict-my-component-to-certain-portal-types) — basic
1. [How do I render nothing without leaving the editor guessing?](#user-content-how-do-i-render-nothing-without-leaving-the-editor-guessing) — basic
1. [How do I hide my component completely, leaving no gap behind?](#user-content-how-do-i-hide-my-component-completely-leaving-no-gap-behind) — basic
1. [How do I check a permission before offering an action?](#user-content-how-do-i-check-a-permission-before-offering-an-action) — basic
1. [How do I react to the signed-in user?](#user-content-how-do-i-react-to-the-signed-in-user) — basic
1. [How do I offer a download?](#user-content-how-do-i-offer-a-download) — basic
1. [How do I talk to another component on the same page?](#user-content-how-do-i-talk-to-another-component-on-the-same-page) — advanced
1. [How do I write a component for the search page?](#user-content-how-do-i-write-a-component-for-the-search-page) — advanced
1. [How do I write a component for the asset details page?](#user-content-how-do-i-write-a-component-for-the-asset-details-page) — advanced
1. [How do I consume my own data adapter interface?](#user-content-how-do-i-consume-my-own-data-adapter-interface) — advanced
1. [How do I wrap other components in a section?](#user-content-how-do-i-wrap-other-components-in-a-section) — advanced
1. [How do I let visitors upload files?](#user-content-how-do-i-let-visitors-upload-files) — advanced
1. [How do I work with collections?](#user-content-how-do-i-work-with-collections) — advanced
1. [How do I submit a request or an approval?](#user-content-how-do-i-submit-a-request-or-an-approval) — advanced
1. [How do I build a search form with facets?](#user-content-how-do-i-build-a-search-form-with-facets) — advanced
1. [How do I ship and render an image or a video?](#user-content-how-do-i-ship-and-render-an-image-or-a-video) — basic
1. [How do I read the payload of a dialog page?](#user-content-how-do-i-read-the-payload-of-a-dialog-page) — basic
1. [How do I track an event, and respect the visitor's consent?](#user-content-how-do-i-track-an-event-and-respect-the-visitors-consent) — advanced
1. [How do I render an asset preview?](#user-content-how-do-i-render-an-asset-preview) — advanced
1. [How do I react to navigation and to live editor changes?](#user-content-how-do-i-react-to-navigation-and-to-live-editor-changes) — advanced
1. [How do I behave properly on a phone?](#user-content-how-do-i-behave-properly-on-a-phone) — basic

## How do I call a data adapter method and render the result?

*basic.* The single most common thing a component does, and the one thing no other document
shows end to end.

A component does not open connections. It declares a configuration property of a **public API
interface** type, the portal administrator points that property at one of the configured data
adapters, and the runtime hands you an object whose methods are that adapter's methods.

```vue
<template>
    <div>
        <v-progress-circular v-if="isLoading" indeterminate />

        <div v-else-if="loadFailed">
            {{ errorText | resolve_localized }}
        </div>

        <div v-else-if="!assets.length">
            {{ emptyText | resolve_localized }}
        </div>

        <ul v-else>
            <li v-for="asset in assets" :key="asset.smintIoId">
                <img v-if="asset.isThumbnailSmallAvailable" :src="asset.smallThumbnailUrl" alt="" />
                {{ asset.name | resolve_localized }}
            </li>
        </ul>
    </div>
</template>

<script lang="ts">
import { Mixins } from "vue-property-decorator";
import type {
    IAssetDataObject,
    IAssetsSearch,
    IErrorHandler,
    ILocalizedStringsModel,
} from "@smintio/portals-component-sdk";
import {
    ComponentProperty,
    DefaultCulture,
    Description,
    DisplayName,
    FormGroup,
    FormGroupDeclaration,
    FormGroupDisplayName,
    Implements,
    IsNumber,
    MaxValue,
    MinValue,
    DefaultValue,
    PortalsGlobalServices,
    PortalsInject,
    PortalsUiComponent,
} from "@smintio/portals-component-sdk";
import { SCssProps } from "@smintio/portals-components";

@PortalsUiComponent({
    type: "ui-type-assets-preview",
    key: "mypartner-ui-latest-assets-1",
    displayName: { [DefaultCulture]: "Latest assets" },
    description: { [DefaultCulture]: "Shows the newest assets of a data source." },
})
@FormGroupDeclaration("latest-assets")
@FormGroupDisplayName("latest-assets", "en", "Latest assets", true)
export default class PortalsUiComponentImplementation extends Mixins(SCssProps) {
    @PortalsInject(PortalsGlobalServices.ErrorHandler)
    public readonly errorHandler!: IErrorHandler;

    @DisplayName("en", "Data source", true)
    @Description("en", "The data source the assets are read from.", true)
    @Implements("IAssetsSearch")
    @ComponentProperty({ name: "dataSource" })
    @FormGroup("latest-assets")
    public readonly dataSource!: IAssetsSearch;

    @DisplayName("en", "Number of assets", true)
    @IsNumber()
    @MinValue(1)
    @MaxValue(50)
    @DefaultValue(12)
    @ComponentProperty({ name: "maxAssets" })
    @FormGroup("latest-assets")
    public readonly maxAssets!: number;

    public assets: IAssetDataObject[] = [];
    public isLoading = false;
    public loadFailed = false;

    public errorText: ILocalizedStringsModel = { [DefaultCulture]: "The assets could not be loaded." };
    public emptyText: ILocalizedStringsModel = { [DefaultCulture]: "Nothing to show yet." };

    public async created(): Promise<void> {
        await this.loadAssets();
    }

    public async loadAssets(): Promise<void> {
        // The administrator may not have picked a data source yet — in the page composer this
        // is the normal state while the component is being set up.
        if (!this.dataSource) {
            console.info("mypartner-ui-latest-assets-1: no data source configured, rendering nothing");

            return;
        }

        this.isLoading = true;
        this.loadFailed = false;

        try {
            const result = await this.dataSource.searchAssetsAsync({
                page: 0,
                pageSize: this.maxAssets,
            });

            this.assets = result?.assetDataObjects ?? [];
        } catch (error) {
            this.loadFailed = true;

            this.errorHandler.captureExceptionAndDisplayMessage(
                error as Error,
                "The assets could not be loaded.",
                { origin: "mypartner-ui-latest-assets-1" }
            );
        } finally {
            this.isLoading = false;
        }
    }
}
</script>
```

What the pieces are:

| | |
|---|---|
| `@Implements("IAssetsSearch")` | makes the property a **data source picker** in the editor. The string is the interface name from [the data adapter reference](smintio-data-adapter-reference.md) |
| `searchAssetsAsync(parameters)` | a data adapter method takes one parameter object and returns a promise. A few — the long-running ones and the collection mutations — take a second positional argument for progress or options; the generated TypeScript shows which |
| `result.assetDataObjects` | the assets. Results are shaped `I…Result`; the assets always arrive under `assetDataObjects` |
| `asset.smintIoId` | the asset's id, inherited from `IDataObject`. Use it as the `:key` and wherever an `IAssetIdentifier` is asked for: `{ id: asset.smintIoId }` |

Pitfalls:

- **The property can be `undefined`.** It is `undefined` until the administrator picks a data
  adapter, and a component that throws in that state is unusable in the page composer. Guard
  every use.
- **Do not call in `mounted()` if the result decides whether you render at all** — `created()`
  runs before the first render, so the loading state is shown instead of an empty flash.
- **Never swallow the failure.** `console.log` is not error handling; route it to the
  [error handler](#user-content-how-do-i-report-an-error) so it reaches the portal's monitoring.
- **Do not assume a method exists.** A data adapter declares which interfaces it publishes, and
  the administrator can point your property at any adapter that publishes the one you asked
  for — but an *optional* capability (search proposals, folder navigation) can still be
  unsupported at runtime. Catch, turn the feature off for the session, and carry on rendering.

## How do I use the portal's own data sources instead of asking for one?

*basic.* Sometimes the component should read whatever the *portal* reads, without the
administrator picking anything. The portals context carries the portal's own configured
adapters.

```typescript
import type { IPortalsContext } from "@smintio/portals-component-sdk";
import { PortalsGlobalServices, PortalsInject } from "@smintio/portals-component-sdk";

@PortalsInject(PortalsGlobalServices.PortalsContext)
public readonly portalsContext!: IPortalsContext;

public async loadAssets(): Promise<void> {
    const search = this.portalsContext?.services?.assets?.search;

    if (!search) {
        console.info("no asset search service configured for this portal, rendering nothing");

        return;
    }

    const result = await search.searchAssetsAsync({ page: 0, pageSize: 12 });

    this.assets = result?.assetDataObjects ?? [];
}
```

`portalsContext.services` groups the portal's adapters by family, and **every member is
optional** — a portal that has no collections has no `services.collections`:

| Group | Members |
|---|---|
| `assets` | `read`, `search`, `folderNavigation`, `download` |
| `resourceAssets`, `productAssets` | `read`, `search` |
| `collections` | `create`, `read`, `search`, `update`, `delete` |
| `collectionsAndAssets`, `collectionsAndUsers` | `read`, `search`, `modify` |
| `shares` | `create`, `read`, `search`, `update`, `delete` |
| `permissions` | see [checking a permission](#user-content-how-do-i-check-a-permission-before-offering-an-action) |

Which to use: take a **configuration property** when the administrator should decide where the
data comes from — that is almost always the right answer for a component that shows content.
Take the **portals context service** when your component is part of the portal's own experience
and has to agree with the rest of the page, such as an action bar acting on the asset the page
is already showing.

## How do I turn an assets reference setting into real assets?

*basic.* An *assets reference* lets the administrator choose assets in the way that suits them
— by picking assets, by folder, by a saved search, or by a relationship to the asset the page
is showing — and it is one property. `AssetsReferenceMixin` resolves whichever of those they
chose.

```vue
<script lang="ts">
import { Mixins, Prop, Watch } from "vue-property-decorator";
import type { IAssetDataObject, IAssetsReferenceModel } from "@smintio/portals-component-sdk";
import {
    ComponentProperty,
    DisplayName,
    DynamicAllowedValuesProvider,
    FormGroup,
    Implements,
    PortalsGlobalServices,
    PortalsUiComponent,
} from "@smintio/portals-component-sdk";
import { AssetsReferenceMixin, SCssProps } from "@smintio/portals-components";

export default class PortalsUiComponentImplementation extends Mixins(AssetsReferenceMixin, SCssProps) {
    @DisplayName("en", "Assets", true)
    @Implements("IAssetsReferenceModel")
    @ComponentProperty({ name: "assetsReference" })
    @DynamicAllowedValuesProvider(
        PortalsGlobalServices.PortalsContext.toString(),
        "AssetsReferenceAllowedValuesProvider"
    )
    @FormGroup("my-group")
    public readonly assetsReference!: IAssetsReferenceModel;

    /** Set by the page when the component sits on an asset details page. */
    @Prop()
    public readonly asset!: IAssetDataObject;

    public assets: IAssetDataObject[] = [];
    public isLoading = false;

    public async created(): Promise<void> {
        await this.loadAssets();
    }

    @Watch("asset")
    public async onAssetChanged(): Promise<void> {
        await this.loadAssets();
    }

    public async loadAssets(): Promise<void> {
        if (!this.assetsReference) {
            return;
        }

        this.isLoading = true;

        try {
            this.assets = await this.getAssets(
                this.assetsReference,
                this.asset,          // needed only for a "related assets" reference
                12                   // maximum number of assets
            );
        } finally {
            this.isLoading = false;
        }
    }
}
</script>
```

`getAssets()` dispatches on `assetsReference.type` and does the fetching for you. The finer
methods are there when you need them — `getAssetByIds()`, `getAssetsByFolderId()`,
`getAssetsBySearch()` — and `getAssetsReferenceByRelatedAssets()` /
`getAssetReferenceByRelationshipType()` resolve a relationship into a plain reference you can
hand to the others.

Pitfalls:

- **A "related assets" reference needs the current asset.** Without the `asset` prop
  `getAssets()` returns an empty array, which looks like "the administrator configured nothing".
  On an asset details page the asset arrives *after* the first render — watch it and reload, as
  above.
- **`maxNumberAssets` truncates, it does not page.** For a long list, use
  [paging](#user-content-how-do-i-page-through-a-search-result) instead.
- **Offer the picker.** Without `AssetsReferenceAllowedValuesProvider` the administrator gets a
  raw field instead of the asset chooser.

## How do I page through a search result?

*advanced.* A search takes `page` and `pageSize`, and the result carries the paging state in
`details`. Two rules matter: page numbers start at **0**, and every page after the first must
repeat the **search result set id** so the platform serves a stable result set rather than
re-running the query.

```typescript
import type { IAssetDataObject, IAssetsSearch } from "@smintio/portals-component-sdk";

public assets: IAssetDataObject[] = [];
public isLoading = false;
public hasMoreResults = false;
public totalResults = 0;

private currentPage = 0;
private searchResultSetUuid: string | undefined = undefined;

public async loadFirstPage(): Promise<void> {
    this.assets = [];
    this.currentPage = 0;
    this.searchResultSetUuid = undefined;

    await this.loadPage();
}

public async loadMore(): Promise<void> {
    if (!this.hasMoreResults || this.isLoading) {
        return;
    }

    this.currentPage += 1;

    await this.loadPage();
}

private async loadPage(): Promise<void> {
    if (!this.dataSource) {
        return;
    }

    this.isLoading = true;

    try {
        const result = await this.dataSource.searchAssetsAsync({
            queryString: this.queryString,
            page: this.currentPage,
            pageSize: 24,
            searchResultSetUuid: this.searchResultSetUuid,
        });

        // Append — never replace — or "load more" throws away what the visitor already sees.
        this.assets = [...this.assets, ...(result?.assetDataObjects ?? [])];

        this.hasMoreResults = result?.details?.hasMoreResults ?? false;
        this.totalResults = result?.details?.totalResults ?? this.assets.length;
        this.searchResultSetUuid = result?.details?.searchResultSetId;
    } finally {
        this.isLoading = false;
    }
}
```

`details` also carries `currentPage`, `currentItemsPerPage`, `maxPages`, `folderNavigationEnabled`,
`parentFolderId` and `parentFolderName`, plus `errors` — a **partial** failure, where some
results came back and something else went wrong, is reported there rather than by a rejected
promise. Show what arrived and surface the errors; do not treat a populated `errors` array as a
total failure.

Pitfalls:

- **Do not re-run the query for page 2** — a new query without `searchResultSetUuid` can
  reorder, and the visitor sees duplicates.
- **Guard against overlapping loads.** A visitor scrolling fast triggers `loadMore()` twice; the
  `isLoading` check above is not decoration.
- **Reset everything when the query changes**, including the result set id. Keeping the old one
  pages through the old search.

## How do I show progress for a long-running call?

*advanced.* Several methods — downloads, uploads, collection operations — take an
`IProgressMonitor` as a required argument. The notification dialog service makes one that shows
progress to the visitor, and lets them cancel.

```typescript
import type { INotificationDialog, IProgressMonitor } from "@smintio/portals-component-sdk";
import { DefaultCulture, PortalsGlobalServices, PortalsInject } from "@smintio/portals-component-sdk";

@PortalsInject(PortalsGlobalServices.NotificationDialog)
public readonly notificationDialog!: INotificationDialog;

public async prepareDownload(assetIds: { id: string }[]): Promise<void> {
    const progress = this.notificationDialog.showProgress({
        title: { [DefaultCulture]: "Preparing your download" },
        message: { [DefaultCulture]: "Preparing… {progressPercentage}%" },
    });

    try {
        const result = await this.dataSource.initiateAssetsDownloadForAssetsAsync(
            { assetIds },
            progress as IProgressMonitor
        );

        await progress.finishedAsync({ [DefaultCulture]: "Your download is ready." });

        return result;
    } catch (error) {
        this.errorHandler.captureExceptionAndDisplayMessage(error as Error, "The download failed.");
    }
}
```

`showProgress()` returns an `INotificationProgress`, which *is* an `IProgressMonitor`: the
platform calls `reportProgressAsync(progressPercentage, displayText)` on it as the work
proceeds, and the `{progressPercentage}` placeholder in your message is filled in for you. If
the visitor cancels, `reportProgressAsync` rejects and the operation is abandoned — so do not
catch and ignore inside the monitor.

For a plain message with no progress, `displayNotification()` shows a snackbar; add `buttons`
and it becomes a dialog:

```typescript
this.notificationDialog.displayNotification({
    message: { [DefaultCulture]: "Added to your collection." },
    category: DialogCategoryEnum.Info,
});
```

## How do I report an error?

*basic.* Every caught failure goes to the error handler, which reports it to the portal's
monitoring. Whether the *visitor* is told is a separate decision, and it is the difference
between the two methods.

```typescript
import type { IErrorHandler } from "@smintio/portals-component-sdk";
import { PortalsGlobalServices, PortalsInject } from "@smintio/portals-component-sdk";

@PortalsInject(PortalsGlobalServices.ErrorHandler)
public readonly errorHandler!: IErrorHandler;

// The visitor asked for something and it failed — tell them, and report it.
try {
    await this.dataSource.getAssetAsync({ assetId: { id } });
} catch (error) {
    this.errorHandler.captureExceptionAndDisplayMessage(
        error as Error,
        "The asset could not be loaded.",
        { origin: "mypartner-ui-asset-detail-1" }
    );
}

// Something failed that the visitor did not ask for — report it, stay silent, degrade.
try {
    this.searchProposals = (await this.dataSource.getFullTextSearchProposalsAsync({ searchQueryString })).fullTextProposals ?? [];
} catch (error) {
    this.searchProposalsSupported = false;

    this.errorHandler.captureException(error as Error, { origin: "mypartner-ui-search-bar-1" });
}
```

Both also exist in a promise-chain form, where the method is called with the *message* and
returns a `catch` handler:

```typescript
this.dataSource
    .searchAssetsAsync({ page: 0, pageSize: 12 })
    .catch(this.errorHandler.captureExceptionAndDisplayMessage("The assets could not be loaded."))
    .then((result) => { /* … */ });
```

The message is a **translation key or a plain string**, not a localized strings model — prefer a
key that exists in your resources, so the visitor sees their own language.

Pitfalls:

- **Do not report the same failure twice.** An error that has already been handled arrives with
  `isAlreadyHandled` set; re-reporting it shows the visitor two messages for one problem.
- **Do not put values into the message.** An identifier, a URL or anything out of the asset
  belongs in the tags or extra data, not in what the visitor reads.

## How do I localize text from script, with placeholders and plurals?

*basic.* In a template, the `resolve_localized` filter is all you need:

```vue
{{ headerText | resolve_localized }}
<div v-html="$options.filters.resolve_localized(continuousText)"></div>
```

In script, inject the translator:

```typescript
import type { ILocalizedStringsModel, ITranslator } from "@smintio/portals-component-sdk";
import { PortalsGlobalServices, PortalsInject } from "@smintio/portals-component-sdk";

@PortalsInject(PortalsGlobalServices.Translator)
public readonly translator!: ITranslator;

public get title(): string {
    return this.translator.localizeStrings(this.headerText) ?? "";
}
```

| Member | What it is for |
|---|---|
| `localizeStrings(model)` | pick the current language out of an `ILocalizedStringsModel` |
| `localizeStringsArray(model)` | the same for an array of strings, such as tags |
| `localizeResourceAsset(model)` | pick the current language's *file* out of a localized resource asset — the image or document to render |
| `getVueI18N()` | the underlying `vue-i18n` instance, for placeholders and plurals |
| `locale`, `currency` | the visitor's current language and currency, kept up to date |

Placeholders and plural forms go through `vue-i18n`, with `{placeholder}` in the text and a
`count` parameter choosing between the `|`-separated plural forms:

```typescript
public get resultCountText(): string {
    return this.translator.getVueI18N().tc("text_result_count", this.totalResults, {
        count: this.totalResults,
    });
}
```

```
text_result_count = "no results | one result | {count} results"
```

Pitfalls:

- **Never concatenate a sentence out of translated fragments.** Word order differs per language;
  use one text with a placeholder.
- **`localizeStrings` can return `undefined`** — for a model the administrator left empty. Decide
  what an empty setting means rather than rendering "undefined".
- **A *string* resource is a text you ship; a *file* resource is an image, video, audio file or
  document.** They are localized by different methods — `localizeStrings` against
  `localizeResourceAsset` — and mixing them up silently renders nothing.

## How do I make one setting depend on another?

*basic.* Three annotations shape the form the administrator sees. They are what turns twelve
settings into a form that looks like three.

```typescript
import {
    FormGroupVisibleIf,
    FormItemVisibility,
    FormItemVisibilityEnum,
    IsBoolean,
    SortPosition,
    VisibleIf,
    VisibleIfOperator,
} from "@smintio/portals-component-sdk";

@DisplayName("en", "Show a caption", true)
@IsBoolean()
@DefaultValue(false)
@ComponentProperty({ name: "showCaption" })
@FormGroup("my-group")
@SortPosition(10)
public readonly showCaption!: boolean;

// Only shown when the checkbox above is ticked.
@DisplayName("en", "Caption", true)
@Implements("ILocalizedStringsModel")
@ComponentProperty({ name: "caption" })
@VisibleIf("showCaption", VisibleIfOperator.Equal, true)
@FormGroup("my-group")
@SortPosition(20)
public readonly caption!: ILocalizedStringsModel;

// Rarely needed — hidden behind "advanced".
@DisplayName("en", "Caption line height", true)
@IsNumber()
@FormItemVisibility(FormItemVisibilityEnum.Advanced)
@VisibleIf("showCaption", VisibleIfOperator.Equal, true)
@ComponentProperty({ name: "captionLineHeight" })
@FormGroup("my-group")
@SortPosition(30)
public readonly captionLineHeight!: number;
```

`VisibleIfOperator` is `Equal`, `NotEqual`, `GreaterThan`, `GreaterThanOrEqual`, `LessThan`,
`LessThanOrEqual` or `OneOf` — `OneOf` takes an array as its value. `FormGroupVisibleIf` does
the same for a whole group, and takes the group id as its first argument:

```typescript
@FormGroupVisibleIf("caption-group", "showCaption", VisibleIfOperator.Equal, true)
public readonly caption!: ILocalizedStringsModel;
```

`FormItemVisibility` takes `Basic` (the default), `Advanced`, `Expert` or `Hidden`. Put the
settings that decide what the component *is* in `Basic`, and everything that fine-tunes it in
`Advanced`. `Hidden` is for a value that is written by something else and never typed.

Pitfall: **the condition names the *property*, not the persisted name.** If the two differ —
`@ComponentProperty({ name: "show_caption" })` on a property called `showCaption` — the
condition takes the property name, and a typo simply means the field is always visible.

## How do I restrict my component to certain portal types?

*basic.* Not every component makes sense in every portal. `allowedPortalTypes` keeps yours out
of the composer where it does not belong, and `mdiIcon` and `illustrationUrls` are how it
presents itself there.

```typescript
@PortalsUiComponent({
    type: "ui-type-custom",
    key: "mypartner-ui-press-contact-1",
    displayName: { [DefaultCulture]: "Press contact", de: "Pressekontakt" },
    description: { [DefaultCulture]: "Shows the press contact for the current release." },
    allowedPortalTypes: ["pressPortal"],
    mdiIcon: "mdi-account-box",
    illustrationUrls: {
        pressPortal: "https://cdn.example.com/press-contact-illustration.png",
    },
})
```

Leave `allowedPortalTypes` out and the component is offered everywhere, which is right for
anything generic. Set it when the component depends on something only one kind of portal has.

## How do I render nothing without leaving the editor guessing?

*basic.* A component that renders nothing is normal — no data source picked yet, an empty
result, a visitor without permission. A component that renders nothing *silently* is a support
ticket.

```typescript
public get shouldRender(): boolean {
    if (!this.dataSource) {
        console.info(`${MY_KEY}: no data source configured — rendering nothing`);

        return false;
    }

    if (!this.assets.length) {
        console.info(`${MY_KEY}: search returned no assets — rendering nothing`);

        return false;
    }

    return true;
}
```

Three more rules that go with it:

- **Fill your layout box and add no white space of your own.** The page decides the spacing; a
  component with its own outer margin cannot be laid out.
- **Render the empty state when there is one to render.** "Nothing to show yet" with the
  administrator's own text beats an invisible component — take the text as a localized strings
  property so they can word it.
- **Rendering an empty element is not the same as being absent.** The page still lays out a slot
  element for it, gap and all. When the component should disappear entirely, say so — see the
  next recipe.

## How do I hide my component completely, leaving no gap behind?

*basic.* Returning an empty template is not enough. The page has already laid out a slot element
for your component — a column, its content gaps, possibly a background band — so an empty
component leaves a visible hole in the page.

**Emit `component:hide`, passing your component instance.** The slot renderer takes your
component out of the slot altogether, and nothing is laid out for it at all.

```vue
<template>
    <div v-if="hasSomethingToShow" class="my-quote">
        <blockquote>{{ quoteText }}</blockquote>
    </div>
</template>

<script lang="ts">
import { Mixins, Prop } from "vue-property-decorator";
import type { IAssetDataObject, IMetadataAttributeModel } from "@smintio/portals-component-sdk";
import {
    ComponentProperty,
    DefaultCulture,
    DisplayName,
    DynamicAllowedValuesProvider,
    FormGroup,
    Implements,
    PortalsGlobalServices,
    PortalsUiComponent,
} from "@smintio/portals-component-sdk";
import { MetadataMixin, SCssProps } from "@smintio/portals-components";

const MY_KEY = "mypartner-ui-asset-quote-1";

@PortalsUiComponent({
    type: "ui-type-asset-details-text",
    key: MY_KEY,
    displayName: { [DefaultCulture]: "Asset quote" },
})
export default class PortalsUiComponentImplementation extends Mixins(MetadataMixin, SCssProps) {
    @Prop()
    public readonly asset?: IAssetDataObject;

    @DisplayName("en", "Quote attribute", true)
    @Implements("IMetadataAttributeModel")
    @ComponentProperty({ name: "quoteAttributes" })
    @DynamicAllowedValuesProvider(
        PortalsGlobalServices.PortalsContext.toString(),
        "TextMetadataAttributeAllowedValuesProvider"
    )
    @FormGroup("my-quote")
    public readonly quoteAttributes!: IMetadataAttributeModel[];

    public get quoteText(): string {
        if (!this.asset || !this.quoteAttributes?.length) {
            return "";
        }

        return this.getLocalizedAttributeValue(this.asset, this.quoteAttributes, false, true);
    }

    public get hasSomethingToShow(): boolean {
        // Still waiting for the asset — do not hide yet, this is a temporary state.
        if (!this.asset) {
            return false;
        }

        if (!this.quoteText) {
            console.info(`${MY_KEY}: this asset carries no quote — hiding the component`);

            // Take me out of the page entirely: no column, no content gap, no background band.
            this.$emit("component:hide", this);

            return false;
        }

        return true;
    }
}
</script>
```

The payload is **the component instance itself** (`this`) — the page identifies which slot
element to drop from it, so `this.$emit("component:hide")` with no argument does nothing.

Where to emit it from: the natural place is the getter that already decides whether there is
anything to render, as above — that is how the shipped components do it. Emitting from a method
you call once loading has finished works just as well. What matters is *when*: emit only once you
know the answer is final.

**On a section start component it hides the whole section.** Emitting `component:hide` from a
`ui-type-section` component removes the section start, everything the visitor placed inside it,
and its matching section end — nested sections included. That is how a conditional section works:
the component evaluates its condition and, if it does not hold, takes its whole block out of the
page.

Pitfalls:

- **Hiding is one-way for that page render.** There is no "unhide" event: once the page has
  dropped your component it stays dropped until the page renders again. Never emit while data is
  still loading, or a slow response hides the component permanently — guard on "loading finished"
  first, as the `!this.asset` branch above does.
- **Emit it, and still render nothing.** The emit takes effect on the page's next render pass;
  your own template must already be guarded, or the component flashes before it disappears.
- **Do not use it for an error.** A failure the visitor caused should be
  [reported](#user-content-how-do-i-report-an-error); silently vanishing makes a broken data
  source indistinguishable from an empty one.
- **Log why, first.** A hidden component is invisible in the page *and* in the editor; the
  console line is all a support engineer has.
- **It is a page-layout mechanism, not a permission check.** Hide the component *and* do not
  fetch what the visitor may not see.

## How do I check a permission before offering an action?

*basic.* Permissions are per asset, and a permission check in the frontend is about the
*experience*: the platform enforces the real thing server-side. Never treat a frontend check as
security — but do use it, because an action that fails after the visitor clicks it is worse than
one that was never offered.

```vue
<template>
    <div>
        <v-btn v-if="canDownload" @click="onDownload">
            {{ downloadButtonText | resolve_localized }}
        </v-btn>

        <span v-else>{{ noDownloadPermissionText | resolve_localized }}</span>
    </div>
</template>

<script lang="ts">
import { Mixins } from "vue-property-decorator";
import { AssetPermissionsMixin } from "@smintio/portals-components";

export default class PortalsUiComponentImplementation extends Mixins(AssetPermissionsMixin) {
    public get canDownload(): boolean {
        return !!this.asset && this.hasDownloadPermission(this.asset);
    }
}
</script>
```

The checks, all of them on `portalsContext.services.permissions` and surfaced by the mixin:

| Check | Answers |
|---|---|
| `hasReadAssetDetailsPermission(asset)` | may the visitor open the detail page? |
| `hasDownloadPermission(asset)` | may they download it at all? |
| `hasDownloadAssetHiResPermission(asset)` | may they download the high-resolution version? |
| `hasDownloadAssetLayoutFilePermission(asset)` | may they download the layout file? |
| `hasAssetPermission(asset, permissionUuid)` | a custom permission, on this asset |
| `hasSomewherePermission(permissionUuid)` | a custom permission, anywhere in the portal |

The last two are how a **custom permission declared by a data adapter** reaches the UI. Pass the
permission's uuid; a component that hard-codes one is tied to a single data adapter, so take it
as a configuration property.

Pitfall: **an asset with no permissions on it has its actions hidden**, and that looks like a
bug in your component. If actions never appear, check what the data adapter puts on the asset
before you debug the template.

## How do I react to the signed-in user?

*basic.* `AuthMixin` answers the questions a component actually asks, and the portals context
carries the user.

```vue
<template>
    <div>
        <span v-if="isLoggedIn">{{ greeting }}</span>
        <v-btn v-else-if="loginAvailable" :to="loginTarget">{{ loginText | resolve_localized }}</v-btn>

        <v-btn v-if="shareAvailable" @click="onShare">{{ shareText | resolve_localized }}</v-btn>
    </div>
</template>

<script lang="ts">
import { Mixins } from "vue-property-decorator";
import { AuthMixin } from "@smintio/portals-components";

export default class PortalsUiComponentImplementation extends Mixins(AuthMixin) {
    public get greeting(): string {
        const user = this.portalsContext?.user;

        if (!user || user.isAnonymous) {
            return "";
        }

        return this.translator
            .getVueI18N()
            .t("text_greeting", { name: user.displayName ?? user.firstName ?? "" })
            .toString();
    }
}
</script>
```

`AuthMixin` is a set of getters over the portals context. `isLoggedIn` is "there is a user and
they are not anonymous", `user` is that user, and `loginAvailable` comes from the tenant. The
rest — `shareAvailable`, `ratingAvailable`, `commentingAvailable`, and the finer
`…SettingAvailable` ones for what a share may offer — are **portal-wide permission checks**,
each one a `hasSomewherePermission(…)` against the portal's permission service. So they answer
"may anyone here do this?", not "does this portal have the feature compiled in", and like every
frontend permission check they are about the experience, not security.

`portalsContext.user` carries `uuid`, `isAnonymous`,
`emailAddress`, `isEmailAddressConfirmed`, `firstName`, `lastName`, `displayName`,
`notificationCulture`, `currency` and the custom form field values collected at registration.

Pitfalls:

- **A visitor is always there; they may be anonymous.** Check `isAnonymous` rather than testing
  for a missing user.
- **Never key business logic on the email address.** Use `uuid`.
- **Do not render personal data into a page that is cached or shared** — a share link is opened
  by someone who is not the person the link was made for.

## How do I offer a download?

*basic.* Mix in `SDownloadProps` and you get the configuration properties (all the dialog
texts), the dialog component, and the methods. You bind the dialog once and call a method.

```vue
<template>
    <div>
        <v-btn v-if="canDownload" @click="onDownload">
            {{ downloadButtonText | resolve_localized }}
        </v-btn>

        <s-download-dialog v-model="showDownloadDialog" v-bind="downloadDialogProps" />
    </div>
</template>

<script lang="ts">
import { Mixins } from "vue-property-decorator";
import type { IAssetDataObject } from "@smintio/portals-component-sdk";
import { AssetPermissionsMixin, SDownloadProps } from "@smintio/portals-components";

export default class PortalsUiComponentImplementation extends Mixins(SDownloadProps, AssetPermissionsMixin) {
    public showDownloadDialog = false;

    public get canDownload(): boolean {
        return !!this.asset && this.hasDownloadPermission(this.asset);
    }

    public onDownload(): void {
        // IAssetIdentifier is { id }, and the id is the asset's smintIoId.
        this.downloadAssets([{ id: this.asset.smintIoId }]);
    }
}
</script>
```

`SDownloadProps` itself contributes only the configuration properties — every dialog text, as a
localized string. The methods, the `downloadDialogProps` getter and the registration of
`<s-download-dialog>` come from the download dialog mixin that it mixes in for you, which is why
one `Mixins(SDownloadProps)` is the whole setup. The four methods:

| Method | For |
|---|---|
| `downloadAssets(assetIds)` | assets the visitor is looking at |
| `downloadCollection(collectionId)` | a whole collection |
| `downloadSharedAssets(assetIds, shareId, shareSecret)` | assets reached through a share link |
| `downloadSharedCollection(collectionId, shareId, shareSecret)` | a collection reached through a share link |

On a page reached by a share link, `shareId` and `shareSecret` arrive as props from the page —
pass them through, or the download is refused.

The dialog handles output formats, the high-resolution choice and the licence confirmation. Do
not build your own: `IAssetsDownload` (`getAssetsDownloadsForAssetsAsync`,
`initiateAssetsDownloadForAssetsAsync`) is there for the case where you genuinely need to drive
the download yourself, and then you also own everything the dialog was doing for you.

Pitfall: **mix in `SDownloadProps`, not the dialog mixin.** The props mixin already mixes in the
behaviour *and* registers `<s-download-dialog>`; registering the dialog yourself gives you two
of them.

## How do I talk to another component on the same page?

*advanced.* Normally components talk through props and events, with the page in the middle. When
two components sit in different slots and one has to know what the other did, the event bus is
the sanctioned way.

```typescript
import type { IAssetsCollectedEventListener, IEventBus } from "@smintio/portals-component-sdk";
import { PortalsGlobalServices, PortalsInject } from "@smintio/portals-component-sdk";

export default class PortalsUiComponentImplementation
    extends Mixins(SCssProps)
    implements IAssetsCollectedEventListener
{
    @PortalsInject(PortalsGlobalServices.EventBus)
    public readonly eventBus!: IEventBus;

    public created(): void {
        this.eventBus.registerAssetsCollectedEventListener(this);
    }

    public beforeDestroy(): void {
        this.eventBus.unregisterAssetsCollectedEventListener(this);
    }

    public onAssetsCollected(collectionId: string, assetIds: string[]): void {
        this.collectedCount += assetIds.length;
    }
}
```

The bus publishes the events the portal itself raises — assets collected and removed,
collections created, updated, deleted and selected, comments posted, updated and deleted — each
with a typed `register…`/`unregister…` pair and a matching `emit…`. For anything of your own,
`$on` / `$off` / `$emit` take a plain event name; prefix it with your component key so it cannot
collide with another partner's.

Pitfalls:

- **Always unregister in `beforeDestroy`.** A listener on a destroyed component keeps it alive
  and fires into a dead render tree — this is the single most common leak here.
- **Do not use the bus for parent-child communication.** Props down, events up; the bus is for
  components that cannot see each other.

## How do I write a component for the search page?

*advanced.* A search page is a page template that owns the search and hands the state to whichever
components sit in its slots. Your component declares the matching `ui-type-…`, takes the state as
**props**, and asks for changes by **emitting events**. It does not search.

Nor, in fact, does the page template call a data adapter directly. It registers the search with
the search cache provider, gets a **cursor** back, and pages through that — which is what lets the
visitor open an asset, come back, and find the same result set at the same scroll position. The
filters live in the **URL query**: changing a filter pushes a new route, and the route change
drives the next search.

```vue
<template>
    <div>
        <div v-if="initialLoading">…</div>

        <div v-else>
            <div v-for="asset in results" :key="asset.smintIoId">
                {{ asset.name | resolve_localized }}
            </div>

            <v-btn v-if="hasMoreResults" :loading="isLoadingResults" @click="$emit('next-search-page')">
                {{ loadMoreText | resolve_localized }}
            </v-btn>
        </div>
    </div>
</template>

<script lang="ts">
import { Mixins, Prop } from "vue-property-decorator";
import type {
    IAssetDataObject,
    IFolderIdentifier,
    IFormFieldValuesModel,
    IFormGroupsDefinitionModel,
    ILocalizedStringsModel,
} from "@smintio/portals-component-sdk";
import { DefaultCulture, PortalsUiComponent } from "@smintio/portals-component-sdk";
import { SCssProps } from "@smintio/portals-components";

@PortalsUiComponent({
    type: "ui-type-search-result",
    key: "mypartner-ui-search-result-1",
    displayName: { [DefaultCulture]: "Search result" },
})
export default class PortalsUiComponentImplementation extends Mixins(SCssProps) {
    @Prop({ default: () => [] })
    public readonly results!: IAssetDataObject[];

    // The result set
    @Prop() public readonly totalResults!: number;
    @Prop() public readonly currentPage!: number;
    @Prop() public readonly currentItemsPerPage!: number;
    @Prop({ default: false }) public readonly hasMoreResults!: boolean;
    @Prop({ default: "" }) public readonly searchUuid!: string;

    // The three loading states, and they are not interchangeable
    @Prop({ default: false }) public readonly initialLoading!: boolean;
    @Prop({ default: false }) public readonly isSearching!: boolean;
    @Prop({ default: false }) public readonly isLoadingResults!: boolean;

    // What was searched for
    @Prop({ default: "" }) public readonly currentQueryString!: string;
    @Prop() public readonly currentSearch!: IFormFieldValuesModel;
    @Prop() public readonly formGroupsDefinitionModel!: IFormGroupsDefinitionModel;

    // Folder navigation, when the data source offers it
    @Prop({ default: false }) public readonly folderNavigationEnabled!: boolean;
    @Prop({ default: null }) public readonly parentFolderId!: IFolderIdentifier | null;
    @Prop({ default: null }) public readonly parentFolderName!: ILocalizedStringsModel | null;
}
</script>
```

The contract is **positional in name only** — there is no interface to implement and nothing
fails at compile time if you get a prop name wrong; the prop simply stays `undefined`. The names
for each page type are listed in
[the page type contracts](smintio-page-type-contracts.md). The events go the other way:

| Event | Emitted by | Means |
|---|---|---|
| `next-search-page` | the search **result** | the visitor wants the next page |
| `remove-search-value` | the search **result** or **form** | the visitor removed one active filter |
| `clear-query` | the search **result** or **form** | the visitor cleared the search |
| `search-changed` | the search **form** | the filter selection changed |
| `query-string-changed` | the search **bar** | the visitor typed a new query |

The three loading flags mean different things and a good component uses all of them:
`initialLoading` is the very first load, when there is nothing to show yet; `isSearching` is a new
search replacing the results, which is when the list should be cleared and scrolled to the top;
`isLoadingResults` is another page arriving under the results already on screen.

Pitfalls:

- **Do not run your own search on a search page.** Two searches on one page disagree, and the
  visitor sees the filter chips of one and the results of the other. It also throws away the
  cursor, so returning from an asset's detail page restarts the search.
- **The filter definition prop has two names.** A search *result* component receives
  `formGroupsDefinitionModel`; a search *form* component receives `form-groups-definition`. They
  are the same data and the wrong name silently yields `undefined`.
- **`currentSearch` is the *current filter state*, not a query string.** Pass it on to anything
  that needs to stay in sync — a search bar asking for proposals takes it as `currentFilters`.
- **Declare the type that matches the slot.** A `ui-type-custom` component cannot be placed in
  the search result slot, and a `ui-type-search-result` one gets props nowhere else.

## How do I write a component for the asset details page?

*advanced.* The asset details **page template** owns the asset and the navigation through the
search result — that is what `AssetDetailsPageNavigationMixin` is for, and it belongs in the page,
not in your component. Your component receives the state as props and asks for a move by emitting
an event, exactly as on the search page.

```vue
<template>
    <div v-if="asset">
        <h1>{{ asset.name | resolve_localized }}</h1>

        <img v-if="asset.isThumbnailLargeAvailable" :src="asset.largeThumbnailUrl" alt="" />

        <v-btn :to="buttonBackToSearchTarget" exact>{{ backText | resolve_localized }}</v-btn>

        <v-btn :disabled="!canNavigatePreviousResultItem" @click="$emit('previous-item')">‹</v-btn>
        <v-btn :disabled="!canNavigateNextResultItem" @click="$emit('next-item')">›</v-btn>
    </div>
</template>

<script lang="ts">
import { Mixins, Prop } from "vue-property-decorator";
import type { IAssetDataObject, IMetadataAttributeModel } from "@smintio/portals-component-sdk";
import {
    ComponentProperty,
    DisplayName,
    DynamicAllowedValuesProvider,
    FormGroup,
    Implements,
    PortalsGlobalServices,
} from "@smintio/portals-component-sdk";
import { MetadataMixin } from "@smintio/portals-components";

export default class PortalsUiComponentImplementation extends Mixins(MetadataMixin) {
    @Prop({ required: true })
    public readonly asset!: IAssetDataObject;

    // Navigation state, all of it from the page.
    @Prop({ default: false }) public readonly canNavigatePreviousResultItem!: boolean;
    @Prop({ default: false }) public readonly canNavigateNextResultItem!: boolean;
    @Prop({ default: false }) public readonly previousIsLoading!: boolean;
    @Prop({ default: false }) public readonly nextIsLoading!: boolean;
    @Prop({ default: "" }) public readonly buttonBackToSearchTarget!: VueRouterLocation;

    // Present when the page was opened through a share link — pass them on to anything
    // that acts on the asset, or the action is refused.
    @Prop({ default: "" }) public readonly shareId!: string;
    @Prop({ default: "" }) public readonly shareSecret!: string;

    @DisplayName("en", "Tag attributes", true)
    @Implements("IMetadataAttributeModel")
    @ComponentProperty({ name: "tagAttributes" })
    @DynamicAllowedValuesProvider(
        PortalsGlobalServices.PortalsContext.toString(),
        "MetadataAttributeAllowedValuesProvider"
    )
    @FormGroup("my-details")
    public readonly tagAttributes!: IMetadataAttributeModel[];

    // The attributes to read come from a configuration property with a metadata attribute
    // picker — never from a hard-coded key. `true` splits multi-value attributes into
    // separate tags.
    public get tags(): string[] {
        if (!this.asset || !this.tagAttributes?.length) {
            return [];
        }

        return this.getLocalizedTags(this.asset, this.tagAttributes, true) ?? [];
    }
}
</script>
```

What the asset carries, and which of it to render:

| | |
|---|---|
| `name`, `description` | localized strings — always through `resolve_localized` |
| `smallThumbnailUrl`, `mediumThumbnailUrl`, `largeThumbnailUrl`, `previewThumbnailUrl` | preview images, each with an `isThumbnail…Available` flag to check first |
| `playbackSmallUrl`, `playbackLargeUrl`, `playbackStreamingUrl`, `pdfPreviewUrl` | video, audio and document playback, likewise flagged |
| `contentType`, `assetCategories`, `compositeAssetType` | what kind of thing it is |
| `fileMetadata`, `imageMetadata`, `videoMetadata`, `audioMetadata`, `documentMetadata` | the typed metadata blocks |
| `localizedTags` | tags, already localized — `MetadataMixin.getLocalizedTags()` unwraps them |
| `rawData` | the source system's own metadata, when the data adapter preserved it — reach it through `MetadataMixin.getLocalizedAttributeValue()` rather than by index |
| `permissionUuids` | what the visitor may do with it |
| `relatedAssets` | other assets in a declared relationship |

Pitfalls:

- **The asset arrives after the first render.** Guard the template with `v-if="asset"` and watch
  the prop rather than reading it in `created()`.
- **Do not mix in the page navigation mixin.** It provides the asset, the cursor and the
  navigation for the *page template*; in a UI component it would run a second, competing
  navigation. Take the props.
- **Carry the share id and secret through.** On a page opened from a share link, every action on
  the asset needs them, and forgetting them turns into "works for me, forbidden for the customer".
- **Check the availability flag, not the URL.** A URL can be present and the rendition not ready.
- **Never hard-code a raw metadata key.** Keys are unique per connector configuration, so the
  same source configured twice produces different keys — take the attribute as a configuration
  property with a metadata attribute picker.

## How do I consume my own data adapter interface?

*advanced.* A **custom** component pairs a data adapter that publishes its own interface with a
UI component that consumes it. The contract between them is a TypeScript file generated from the
data adapter's own types.

1. Generate the definition from the built data adapter assembly with the
   [Data Adapter Exporter CLI](../../../Tools/Portals-DataAdapter-SDK-DataAdapterExporter-CLI/Release/).
2. Put the generated file into your component package — `src/data-adapter/IMyDataAdapter.ts` is
   a good home — and commit it.
3. Declare the property with `@Implements` and the interface's own name.

```typescript
import type { IMyProductCatalog, IProductModel } from "./data-adapter/IMyDataAdapter";

@DisplayName("en", "Product catalog", true)
@Implements("IMyProductCatalog")
@ComponentProperty({ name: "catalog" })
@FormGroup("my-group")
public readonly catalog!: IMyProductCatalog;

public products: IProductModel[] = [];

public async created(): Promise<void> {
    if (!this.catalog) {
        return;
    }

    const result = await this.catalog.getFeaturedProductsAsync({ maxProducts: 12 });

    this.products = result?.products ?? [];
}
```

The string in `@Implements` is the **interface name as the exporter wrote it**, and it is what
the editor matches against the data adapters that publish it. A mismatch is not a compile error:
the picker simply offers nothing.

Pitfalls:

- **Regenerate whenever the data adapter's types change**, and treat the generated file as part
  of the release. A stale definition compiles and fails at runtime.
- **Name the fields for the consumer.** The generated types are your published API — renaming a
  field later breaks every portal already configured.
- **A custom interface takes one parameter object and returns one result object.** Do not design
  it around positional parameters — the only positional argument the platform adds is the
  progress monitor on a long-running method.

## How do I wrap other components in a section?

*advanced.* A *section* is the one sanctioned way for a component to contain other components.
The editor adds your section start, then any number of components, then the generic section end;
the runtime collects everything between them and hands it to you.

```vue
<template>
    <v-expansion-panels v-if="subUiComponentInfos.length">
        <v-expansion-panel>
            <v-expansion-panel-header>{{ title | resolve_localized }}</v-expansion-panel-header>

            <v-expansion-panel-content>
                <s-generic-slot
                    :ui-slot="subUiComponentInfos"
                    :ui-slot-data="{ attrs: $attrs, on: $listeners }"
                />
            </v-expansion-panel-content>
        </v-expansion-panel>
    </v-expansion-panels>
</template>

<script lang="ts">
import { Mixins, Prop } from "vue-property-decorator";
import type { ILocalizedStringsModel, IUIComponentInfo } from "@smintio/portals-component-sdk";
import { DefaultCulture, PortalsUiComponent } from "@smintio/portals-component-sdk";
import { SGenericSlot, SHtmlProps } from "@smintio/portals-components";

@PortalsUiComponent({
    type: "ui-type-section",
    key: "mypartner-ui-section-start-expansion-1",
    displayName: { [DefaultCulture]: "Start expandable section" },
    components: { SGenericSlot },
    inheritAttrs: false,
})
export default class PortalsUiComponentImplementation extends Mixins(SHtmlProps) {
    @Prop({ default: () => [] })
    public readonly subUiComponentInfos!: IUIComponentInfo[];

    @DisplayName("en", "Section title", true)
    @Implements("ILocalizedStringsModel")
    @ComponentProperty({ name: "title" })
    @FormGroup("my-section")
    public readonly title!: ILocalizedStringsModel;
}
</script>
```

Pitfalls:

- **Pass `$attrs` and `$listeners` through.** Everything the page handed to the section — the
  current asset, the search state — reaches the components inside only through that binding.
- **`inheritAttrs: false`**, or Vue also writes those attributes onto your root element.
- **Sections nest.** The runtime matches each section start to its own section end, so do not
  assume your section is the outermost one and do not build your own end marker.
- **A section is still a component**: it fills its box and adds no white space of its own.

## How do I let visitors upload files?

*advanced.* Uploading is two halves that meet in a form: the **files** go to Smint.io's file
storage one by one and come back as identifiers, and the **request** — the metadata, the
requester, the approval — is a task carrying those identifiers. Your component drives the first
half and hands the identifiers to the second.

```typescript
@Implements("IAssetsUpload")
@ComponentProperty({ name: "assetsUpload" })
@FormGroup("my-upload")
public readonly assetsUpload?: IAssetsUpload;

public uploadSettings: IGetAssetUploadSettingsResult | null = null;

public async created(): Promise<void> {
    if (!this.assetsUpload) {
        return;
    }

    // The limits the visitor's browser should enforce before anything is sent.
    this.uploadSettings = await this.assetsUpload.getAssetUploadSettingsAsync({});
}
```

The settings carry `maxFiles`, `contentTypes`, `allowedFormats`, a general `maxFileSize` and one
per content type, the image dimension bounds, and the id of a custom metadata form. **Check them
in the browser before uploading** — the server checks again, but a file rejected after a
two-minute upload is a bad experience.

The upload itself goes through the portal's file service, one file at a time:

```typescript
public async uploadOne(file: File): Promise<string | undefined> {
    const upload = this.portalsContext?.services?.files?.upload;

    if (!upload) {
        return undefined;
    }

    try {
        const result = await upload.uploadFileAsync(
            file,
            // A progress monitor: the platform calls this as the bytes go out.
            { reportProgressAsync: async (percentage: number) => { this.progress = percentage; } },
            // A cancel callback: return true and the upload is abandoned.
            { cancel: async () => this.cancelled }
        );

        return result.fileUuid;
    } catch (error) {
        // One file failing must not stop the queue — mark it and carry on.
        this.errors.push({ fileName: file.name, message: this.uploadFailedText });

        return undefined;
    }
}

public destroyed(): void {
    // An in-flight upload otherwise outlives the component.
    this.cancelled = true;
}
```

Collect the identifiers as they arrive and submit them with the task:

```typescript
public fileUuidsFormFieldValues: IFormFieldValuesModel = {
    values: [{ id: "FileUuids", dataType: ValueType.StringArray, subComponentFormFieldValues: {}, stringArrayValue: [] }]
};

public onUploaded(fileUuid: string): void {
    this.fileUuidsFormFieldValues.values![0].stringArrayValue!.push(fileUuid);
}
```

Pitfalls:

- **Upload strictly one at a time.** Several at once compete for bandwidth, make progress
  meaningless, and multiply the failure modes.
- **The third argument is a cancel *callback*, not a token.** The platform asks it repeatedly;
  return `true` to stop.
- **Set the cancel flag when the component is destroyed**, or a visitor who navigates away keeps
  uploading.
- **A file is not an asset yet.** The identifiers are meaningless until the task that carries them
  is submitted and approved, so keep the submit button disabled until at least one file is in.
- **Anonymous visitors need a captcha.** The portals context carries the site key; use it when the
  visitor is not signed in.

## How do I work with collections?

*advanced.* Collections are the visitor's own groupings. Everything goes through the portal's
collection services, and **every change is announced twice** — once to your parent slot, once on
the event bus for components elsewhere on the page.

```typescript
public async searchCollections(): Promise<void> {
    const search = this.portalsContext?.services?.collections?.search;

    if (!search) {
        return;
    }

    const result = await search.searchCollectionsAsync({
        queryString: this.queryString,
        page: this.page,           // collections page from 1, not 0
        pageSize: 16,
        sortBy: "createdAt",
        sortDirection: SortDirection.Desc,
    });

    this.collections = result?.collections ?? [];

    // maxPages is not always populated — compute the fallback.
    this.maxPages = result?.details?.maxPages
        ?? Math.ceil((result?.details?.totalResults ?? 1) / 16);
}

public async createCollection(name: string): Promise<void> {
    const result = await this.portalsContext!.services!.collections!.create!
        .createCollectionAsync({ definition: { name } }, {});

    this.$emit("collection-created", result.collection);
    this.eventBus.emitCollectionCreated(result.collection!);
}

public async addToCollection(collectionId: string, assetIds: IAssetIdentifier[]): Promise<void> {
    await this.portalsContext!.services!.collectionsAndAssets!.modify!
        .addAssetsToCollectionAsync({ collectionId, assetIds }, {});

    this.$emit("assets-collected", { collectionId, assetIds: assetIds.map((a) => a.id) });
    this.eventBus.emitAssetsCollected(collectionId, assetIds.map((a) => a.id));
}
```

**The "collect without asking" path.** The portals context remembers which collection the visitor
last worked with; when there is one, add straight to it, and only open the collection chooser when
there is not:

```typescript
public onCollectClicked(asset: IAssetDataObject): void {
    const assetIds = [{ id: asset.smintIoId! }];
    const currentCollectionId = this.portalsContext?.storage?.currentSelectedCollectionId;

    if (!currentCollectionId) {
        this.openRememberDialog(assetIds);      // from SRememberProps

        return;
    }

    this.addAssetsToCollection(currentCollectionId, assetIds);
}
```

Pitfalls:

- **Announce every change on both channels.** A parent slot listens to your event; a quick-view
  panel in another slot listens to the bus. Emitting only one leaves half the page stale.
- **The mutation methods take a second, empty options argument.** It is positional, and leaving it
  out is a runtime error, not a compile error.
- **`currentSelectedCollectionId` is read-only and not reactive.** Read it at the moment you need
  it; do not cache it in a computed property.
- **Replace array items with `$set`.** Assigning by index does not trigger a re-render.
- **Collections page from 1.** Asset searches page from 0. They really are different.

## How do I submit a request or an approval?

*advanced.* Access requests, download requests, upload reviews — all of them are **tasks**, and a
component does not build one by hand. It points at a task management data adapter, names the task
handler, and renders the shared task action component, which builds the form from the handler's
own definition.

```typescript
@Implements("ITaskManagement")
@ComponentProperty({ name: "taskManagement" })
@FormGroup("my-request")
public readonly taskManagement!: ITaskManagement;

/// The list of handlers comes from the data source above — note the first argument is the
/// name of another property, not a global service.
@ComponentProperty({ name: "requestTaskHandlerId" })
@DynamicAllowedValuesProvider("taskManagement", "RequestDownloadTaskHandlerAllowedValuesProvider")
@FormGroup("my-request")
public readonly requestTaskHandlerId!: string;

/// An optional portal-defined metadata form to collect alongside the request.
@ComponentProperty({ name: "requestCustomFormId" })
@DynamicAllowedValuesProvider(PortalsGlobalServices.PortalsContext.toString(), "CustomFormAllowedValuesProvider")
@FormItemVisibility(FormItemVisibilityEnum.Expert)
@FormGroup("my-request")
public readonly requestCustomFormId!: string;
```

```vue
<s-task-action-descriptor
    :task-management="taskManagement"
    :task-handler-id="requestTaskHandlerId"
    :custom-form-id="requestCustomFormId"
    :task-id="downloadId"
    :pre-set-form-values="preSetFormValues"
    :button-cancel-text="buttonCancelText"
    @click:cancel="closeDialog"
/>
```

Values your component already knows are handed in as pre-set form values rather than asked for
again:

```typescript
public get preSetFormValues(): IFormFieldValuesModel {
    return {
        values: [{
            id: "DownloadUuid",
            dataType: ValueType.String,
            subComponentFormFieldValues: {},
            stringValue: this.dialogData?.urlData?.downloadUuid as string,
        }],
    };
}
```

Pitfalls:

- **The handler id is configuration, never a constant.** Different portals have different task
  handlers configured; a hard-coded id works in your tenant only.
- **Do not build the form yourself.** The fields come from the task handler's definition, so a
  hand-built form breaks the moment the handler changes.
- **The form field id has to match what the handler reads.** It is a string contract on both
  sides, and a typo simply yields an empty value at approval time.

## How do I build a search form with facets?

*advanced.* The page hands your component the **filter definition** — the groups, the items, their
data types and current values — and expects one event back when the visitor changes something.
Your component renders the definition; it does not invent filters.

```typescript
@Prop() public readonly formGroupsDefinition!: IFormGroupsDefinitionModel | undefined;
@Prop({ default: false }) public readonly initialLoading!: boolean;
@Prop() public readonly isSearching?: boolean;
@Prop({ default: "" }) public readonly searchUuid!: string;
@Prop({ default: null }) public readonly errors!: IErrorModel[] | null;

public get formGroupDefinitions(): IFormGroupDefinitionModel[] {
    return this.formGroupsDefinition?.formGroupDefinitions ?? [];
}

public onSearchChanged(newSearch?: IFormFieldValuesModel): void {
    this.$emit("search-changed", newSearch);
}
```

**The one rule that decides whether this works**: a form field value does not have a generic
`value` — the property to read and write is derived from the item's data type, in camel case with
`Value` appended. `string` becomes `stringValue`, `string_array` becomes `stringArrayValue`,
`date_time` becomes `dateTimeValue`:

```typescript
private getValueKey(dataType: ValueType): string {
    const camelCased = dataType.replace(/([-_][a-z])/g, (group) =>
        group.toUpperCase().replace("-", "").replace("_", ""));

    return `${camelCased}Value`;
}

public buildSearch(changed: IFormFieldValueModel): IFormFieldValuesModel {
    const values: IFormFieldValueModel[] = [];

    for (const group of this.formGroupDefinitions) {
        for (const item of group.formItemDefinitions ?? []) {
            const value = { id: item.id, dataType: item.dataType } as IFormFieldValueModel;
            const valueKey = this.getValueKey(item.dataType!);

            // The changed item takes the new value; every other item keeps its current one.
            const source = item.id === changed.id ? changed : item.currentValue;

            if (source?.[valueKey] !== undefined && source?.[valueKey] !== null) {
                this.$set(value, valueKey, source[valueKey]);
                values.push(value);
            }
        }
    }

    return { values };
}
```

Pitfalls:

- **Send the whole filter state, not the delta.** The page replaces its filters with what you
  emit, so an event carrying only the changed facet clears everything else.
- **Key the rendered filter on the search id.** When a new search starts, the definition is
  replaced; without a key on `searchUuid` Vue reuses the old inputs and the visitor sees stale
  selections.
- **`errors` is a partial failure, not a total one.** Some facets may be unavailable while the
  rest work; show them rather than blanking the form.
- **Debounce the change event.** A visitor ticking four boxes should cause one search, not four.

## How do I ship and render an image or a video?

*basic.* Text you ship as an **embedded** resource in `resources/definition.ts`. Images, videos,
audio and documents are **file** resources: they live in your package's resources folder and are
loaded from there.

```typescript
// resources/definition.ts
export default (builder: IResourceDefinitionBuilder): Promise<void> =>
    builder.buildUIComponentResourceDefinition(async (uiComponentBuilder) => {
        uiComponentBuilder.setDefaultSettings(uiComponentBuilder.createDefaultSettings());

        await uiComponentBuilder.defineResources(async (resourceBuilder) => {
            await resourceBuilder.loadFileResources(async (fileResources) => {
                await fileResources.verifyLoadedResources(async (resource) => resource);
            });
        });
    });
```

The property is typed by what it holds, and gets the matching picker:

```typescript
@DisplayName("en", "Image to display", true)
@Implements("IImage")
@ComponentProperty({ name: "image" })
@DynamicAllowedValuesProvider(
    PortalsGlobalServices.PortalsContext.toString(),
    "ImageResourceAllowedValuesProvider"
)
@FormGroup(SImageProps.FormGroupId)
public readonly image!: IImage;
```

Rendering means picking the visitor's language out of the resource and reading a URL off it:

```typescript
public imageSrc = "";

@Watch("image", { deep: true, immediate: true })
public setImageSrc(): void {
    if (!this.image) {
        return;
    }

    // Always pass the localized resource assets, never the resource itself.
    const localized = this.translator.localizeResourceAsset(this.image.localizedResourceAssets);

    this.imageSrc = localized?.highResUrl ?? "";
}
```

| Field on the localized resource | Use |
|---|---|
| `highResUrl` | the image itself, and the video source |
| `largeThumbnailUrl`, `smallThumbnailUrl` | previews, and a video poster when none was configured |
| `playbackLargeUrl`, `playbackSmallUrl` | playback renditions |
| `originalWidth`, `originalHeight` | aspect ratio, before the file has loaded |
| the `is…Available` flags | whether the rendition exists at all |

Pitfalls:

- **Watch deeply and immediately.** A resource is a deep object that arrives after the first
  render; a plain computed property misses the update.
- **`localizeResourceAsset` returns `undefined`** when the resource has nothing for this language.
  Fall back rather than rendering an empty `src`.
- **Offer a placeholder.** A component configured with no image yet should show something in the
  page composer, not collapse.
- **Pair the type with its provider.** `IImage` with the image provider, `IVideo` with the video
  one — mismatched, the administrator gets a picker that offers the wrong resources.

## How do I read the payload of a dialog page?

*basic.* A dialog page has no URL of its own, so everything a full page would read from the query
string arrives in a **prop**. Its presence is also how you know you are in a dialog at all.

```typescript
@Prop()
public readonly dialogData!: IPageDialogData;

public get isDialog(): boolean {
    return !!this.dialogData;
}

public get downloadId(): string {
    // What would have been $route.query.downloadUuid on a full page.
    return this.dialogData?.urlData?.downloadUuid as string;
}

public closeDialog(): void {
    this.eventBus.emitClosePageDialog();
}
```

```vue
<template>
    <v-card v-if="isDialog">
        <!-- dialog chrome: title bar, close button -->
        <slot />
    </v-card>

    <div v-else>
        <slot />
    </div>
</template>
```

`dialogData` carries `url` (what opened the dialog), `urlData` (the query parameters), `anyData`
(a free-form payload when something opened the dialog programmatically), and
`setMaxDialogWidth()` so a component can size its own host.

Pitfalls:

- **Everything in `urlData` is a string or an array of strings.** Cast deliberately; there is no
  typing to protect you.
- **Close through the event bus.** The payload also exposes a close method, but the bus is what
  every shipped form uses, and it works from anywhere in the dialog's component tree.
- **The same component often has to work both ways.** A form used as a dialog *and* as a page
  should switch its wrapper on `isDialog` rather than being written twice.

## How do I track an event, and respect the visitor's consent?

*advanced.* Analytics are optional at runtime — a visitor who declined tracking has no analytics
provider — so the injection is optional and every call is guarded:

```typescript
@PortalsInject(PortalsGlobalServices.AnalyticsProvider)
public readonly analyticsProvider?: IAnalyticsProvider;

public onPlaybackStarted(): void {
    if (this.trackedPlay) {
        return;           // once per asset, not once per render
    }

    this.trackedPlay = true;

    this.analyticsProvider?.sendAnalyticsEvent("Video", "Playback started", this.asset.smintIoId);
}

@Watch("asset", { deep: true, immediate: true })
public resetTracking(): void {
    this.trackedPlay = false;
    this.tracked25Percent = false;
    // …
}
```

`sendAnalyticsEvent(category, action, label?, value?)` is the whole surface: a category, what
happened, usually the asset id, and an optional number for a milestone.

**Consent** is its own service, and the important thing about it is that **it is not reactive**.
Register for the two events and copy what you need into your own state:

```typescript
@PortalsInject(PortalsGlobalServices.ConsentProvider)
public readonly consentProvider!: IConsentProvider;

public consentGiven = false;

public mounted(): void {
    this.consentProvider.registerConsentAcceptedEventListener(this);
    this.consentProvider.registerConsentDeclinedEventListener(this);

    this.refresh();
}

public beforeDestroy(): void {
    this.consentProvider.unregisterConsentAcceptedEventListener(this);
    this.consentProvider.unregisterConsentDeclinedEventListener(this);
}

// Called by the provider — the names are the contract.
public consentAccepted(consentVersion: number): void { this.refresh(); }
public consentDeclined(): void { this.refresh(); }

public refresh(): void {
    const version = this.consentProvider.getConsentVersion();
    const required = this.consentProvider.getRequiredConsentVersion() ?? 1;

    // undefined or null: not decided yet. -1: explicitly declined.
    this.consentGiven = version !== undefined && version !== null && version >= required;
}

public get consentNecessary(): boolean {
    // Only one tracking mode collects personal information; below that there is nothing to ask.
    return this.consentProvider.getTrackingMode() === 3;
}
```

Pitfalls:

- **Never gate rendering on an un-refreshed consent value.** Without the listeners your component
  shows the consent state from the moment it mounted, forever.
- **"Not decided" and "declined" are different.** A missing version means you have not asked yet;
  a version below the required one means the visitor declined or the terms changed.
- **Do not track before consent.** Embeds, pixels and video players that start reporting before
  the answer is in are exactly what the consent step exists to prevent.
- **Guard every analytics call with `?.`** — the provider is legitimately absent.

## How do I render an asset preview?

*advanced.* Do not build a media player. The shared library ships a preview component per media
kind; your component's job is to pick the right one and pass the asset through.

```vue
<template>
    <component
        :is="previewComponentName"
        :asset="asset"
        :can-navigate-previous-result-item="canNavigatePreviousResultItem"
        :can-navigate-next-result-item="canNavigateNextResultItem"
        :previous-is-loading="previousIsLoading"
        :next-is-loading="nextIsLoading"
        :is-quick-view-active.sync="isQuickViewActive"
        @previous-item="$emit('previous-item')"
        @next-item="$emit('next-item')"
    />
</template>

<script lang="ts">
import { Mixins, Prop } from "vue-property-decorator";
import type { IAssetDataObject } from "@smintio/portals-component-sdk";
import { PortalsUiComponent } from "@smintio/portals-component-sdk";
import {
    SAudioPreview,
    SCssProps,
    SImagePreview,
    SPdfPreview,
    SQuickViewProps,
    SVideoPreview,
} from "@smintio/portals-components";

@PortalsUiComponent({
    type: "ui-type-asset-details-preview",
    key: "mypartner-ui-asset-preview-1",
    displayName: { [DefaultCulture]: "Asset preview" },
    components: { SImagePreview, SAudioPreview, SVideoPreview, SPdfPreview },
})
export default class PortalsUiComponentImplementation extends Mixins(SQuickViewProps, SCssProps) {
    @Prop() public readonly asset!: IAssetDataObject;

    public isQuickViewActive = false;

    public get previewComponentName(): string {
        switch (this.asset?.contentType?.smintIoId) {
            case "video": return "SVideoPreview";
            case "audio": return "SAudioPreview";
            case "document": return "SPdfPreview";
            default: return "SImagePreview";
        }
    }

    public mounted(): void {
        window.onkeyup = (event: KeyboardEvent) => {
            if (event.key === "ArrowLeft" && this.canNavigatePreviousResultItem) {
                this.$emit("previous-item");
            }

            if (event.key === "ArrowRight" && this.canNavigateNextResultItem) {
                this.$emit("next-item");
            }
        };
    }
}
</script>
```

`SQuickViewProps` contributes the quick-view settings — the large-file warning texts and the size
limit above which a preview is not opened automatically. `GalleryAssetConverterMixin` converts an
asset into the shape the gallery components expect, with `convertToItemAsset()`,
`convertToImageAsset()`, `convertToVideoAsset()` and the `isVideoAsset()` / `isAudioAsset()`
tests.

Pitfalls:

- **Dispatch on the content type, not the file extension.** A source system's extension is
  unreliable; the content type is what the data adapter determined.
- **Check the availability flag before offering a rendition.** A playback URL can be present while
  the rendition is still being generated.
- **Release the keyboard handler.** Assigning `window.onkeyup` replaces whatever else was there —
  clear it when the component is destroyed, or the next page inherits your shortcuts.
- **Quick view is synced, not owned.** Bind it with `.sync` so the preview component and your
  component agree on whether it is open.

## How do I react to navigation and to live editor changes?

*advanced.* Two things change under a running component: the visitor navigates, and — in the
page composer — the administrator edits the settings and expects to see the result.

The page context is injected **reactively**, because it changes as the visitor moves:

```typescript
import { InjectReactive, Watch } from "vue-property-decorator";
import type { IPortalsPageContext } from "@smintio/portals-component-sdk";
import { PortalsGlobalServices } from "@smintio/portals-component-sdk";

@InjectReactive(PortalsGlobalServices.PageContext)
public readonly pageContext!: IPortalsPageContext;

public get isOnSearchPage(): boolean {
    return this.pageContext?.pageType === "page-type-assets-search";
}

// The route changes without the component being re-created.
@Watch("$route")
public async onRouteChanged(): Promise<void> {
    await this.loadFirstPage();
}

// A configuration property changed in the editor — reload what depends on it.
@Watch("dataSource")
@Watch("maxAssets")
public async onConfigurationChanged(): Promise<void> {
    await this.loadFirstPage();
}
```

Three rules that follow from this:

- **Anything fetched in `created()` must also be re-fetched on change.** A component that only
  loads once looks broken in the page composer, where nothing is ever re-created.
- **Use `@PortalsInject` for the services that do not change** (the portals context, the error
  handler, the translator) and `@InjectReactive` for the page context.
- **Do not cache derived values in `data`** when a computed property would do; a getter
  recomputes, a data field does not.

## How do I behave properly on a phone?

*basic.* Portals are used on phones at least as much as on desktops, and the page cannot fix a
component that ignores the viewport.

```vue
<template>
    <v-row>
        <v-col v-for="asset in assets" :key="asset.smintIoId" :cols="columns">
            <!-- … -->
        </v-col>
    </v-row>
</template>

<script lang="ts">
public get columns(): number {
    return this.$vuetify.breakpoint.smAndDown ? 12 : 3;
}

public get effectiveWidth(): number {
    // The convention the shipped components follow for their own width settings: the mobile
    // value -1 is shown as "Default" and means "use the desktop value", so an administrator
    // fills it in only when it differs. Each component resolves it itself — nothing central
    // does it for you.
    return this.assetsWidthMobile !== -1 ? this.assetsWidthMobile : this.assetsWidth;
}
</script>
```

Declare the pair like this, so the administrator sees "Default" rather than a magic number:

```typescript
@DisplayName("en", "Width (mobile)", true)
@ComponentProperty({ name: "assetsWidthMobile" })
@AllowedValues([-1, 50, 66, 75, 100, 120])
@AllowedValueDisplayName(-1, "en", "Default", true)
@IsInt32()
@DefaultValue(-1)
@FormGroup(SBottomGapProps.FormGroupId)
public readonly assetsWidthMobile!: number;
```

- `$vuetify.breakpoint` is available in every component: `xsOnly`, `smAndDown`, `mdAndUp` and
  the rest.
- Where a setting genuinely differs between phone and desktop, ship **two settings** and follow
  the `-1` convention above, rather than guessing from the viewport alone.
- **The page template's content width is a different convention.** There the "Default" value is
  `0`, and it falls back to the default the slot renderer was given — not to the desktop value.
  Do not carry the `-1` idiom across to it.
- Test at 360 px wide. A component that needs a horizontal scrollbar there will be reported as
  broken.

## Questions

Please do not hesitate to contact us at [support@smint.io](mailto:support@smint.io) if a recipe
you need is missing, or if one of these does not work as described.

Contributors
============

- Reinhard Holzner, Smint.io GmbH

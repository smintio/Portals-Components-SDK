Building a Smint.io Portals page template
=========================================

Current version of this document is: 1.1.1 (as of 15th of September, 2026)

A **page template** is the second level of structure in a Smint.io Portal. It defines the
*slots* of a page — header, left, content, right, footer — decides which UI component types each
slot accepts, and renders the components the runtime injects into them.

Please read [the frontend component guide](../README.md) first. Almost everything there — the
package layout, the annotations, the configuration properties, the build and publish loop —
applies identically here. This document covers only what differs.

> Note: *slot* here means a **Smint.io Portals slot**, not a Vue slot. They are unrelated
> concepts that happen to share a word.

1. [What differs from a UI component](#user-content-what-differs-from-a-ui-component)
1. [Skeleton](#user-content-skeleton)
1. [Declaring slots](#user-content-declaring-slots)
1. [Rendering slots](#user-content-rendering-slots)
1. [Page types](#user-content-page-types)
1. [Configuration properties](#user-content-configuration-properties)
1. [Resources](#user-content-resources)
1. [Build, run, publish](#user-content-build-run-publish)
1. [Checklist](#user-content-checklist)

## What differs from a UI component

| | UI component | Page template |
|---|---|---|
| Directory name prefix | `ui-` | `page-` |
| Entry file | `src/PortalsUiComponent.vue` | `src/PortalsPage.vue` |
| Class decorator | `PortalsUiComponent` | `PortalsPageTemplateComponent` |
| Class name by convention | `PortalsUiComponentImplementation` | `PortalsPage` |
| `type` | a UI component type | a **page type** |
| Shared rollup config imported by `rollup.config.js` | `../../config/rollup/rollup-config.ts` | `../../config/rollup/rollup-config-page.ts` |
| `main` in `package.json` | `./lib/portals-ui-component.umd.min.js` | `./lib/portals-page.umd.js` |
| Resource builder call in `resources/definition.ts` | `buildUIComponentResourceDefinition` | `buildPageTemplateResourceDefinition` |
| Generated resource file | `portals-ui-component.json` | `portals-page-template.json` |
| Padding and margins | must add none of its own | **owns them** |

The last row is the important one. A UI component fills the layout box it is given and adds no
white space around its content; the page template is responsible for all spacing between the
components it hosts and around the page.

## Skeleton

```vue
<template>
    <div>
        <s-header-slot :ui-slot="headerSlot" />
        <s-side-slot :ui-slot="leftSlot" />

        <v-main>
            <s-generic-slot
                :ui-slot="contentSlot"
                :max-width-desktop="slotWidthPixelsDesktop"
                :max-width-mobile="slotWidthPixelsMobile"
                :content-width-desktop="contentWidthDesktop"
                :content-width-mobile="contentWidthMobile"
                :default-content-width-desktop="8"
                :default-content-width-mobile="10"
                :apply-top-content-gap="true"
                :apply-bottom-content-gap="true"
                :content-gap="contentGap"
            />
        </v-main>

        <s-side-slot :ui-slot="rightSlot" />
        <s-footer-slot :ui-slot="footerSlot" />
    </div>
</template>

<script lang="ts">
import type { IUIComponentInfo } from "@smintio/portals-component-sdk";
import {
    AllowedValueDisplayName,
    AllowedValues,
    ComponentProperty,
    DefaultCulture,
    DefaultValue,
    DisplayName,
    FormGroup,
    FormGroupDeclaration,
    FormGroupDisplayName,
    IsString,
    PortalsPageTemplateComponent,
    UIComponentSlot,
} from "@smintio/portals-component-sdk";
import { Mixins } from "vue-property-decorator";
import {
    SContentWidthProps,
    SPageMixin,
    SPageTemplateColorsProps,
    SSideSlot,
} from "@smintio/portals-components";

@FormGroupDeclaration("s-content")
@FormGroupDisplayName("s-content", "en", "Content", true)
@PortalsPageTemplateComponent({
    type: "page-type-generic",
    key: "smintio-page-generic-1",
    displayName: {
        [DefaultCulture]: "Content page",
        de: "Inhaltsseite",
    },
    description: {
        [DefaultCulture]: "A content page with header, footer, left and right areas and a content area.",
    },
    components: {
        SSideSlot,
    },
})
export default class PortalsPage extends Mixins(
    SPageTemplateColorsProps,
    SContentWidthProps,
    SPageMixin
) {
    @UIComponentSlot({
        slotId: "header",
        minimumItems: 0,
        maximumItems: 1,
        allowedUiComponentTypes: ["ui-type-header"],
    })
    public headerSlot: IUIComponentInfo[] = [];

    @UIComponentSlot({ slotId: "left" })
    public leftSlot: IUIComponentInfo[] = [];

    @UIComponentSlot({ slotId: "content", minimumItems: 1 })
    public contentSlot: IUIComponentInfo[] = [];

    @UIComponentSlot({ slotId: "right" })
    public rightSlot: IUIComponentInfo[] = [];

    @UIComponentSlot({
        slotId: "footer",
        minimumItems: 0,
        maximumItems: 1,
        allowedUiComponentTypes: ["ui-type-footer"],
    })
    public footerSlot: IUIComponentInfo[] = [];

    @DisplayName("en", "Top and bottom gap", true)
    @ComponentProperty({ name: "contentGap" })
    @IsString()
    @AllowedValues(["none", "small", "medium", "large", "xlarge", "xxlarge", "default"])
    @AllowedValueDisplayName("none", "en", "No gap", true)
    // … one AllowedValueDisplayName per value per language
    @FormGroup("s-content")
    @DefaultValue("default")
    public readonly contentGap!: string;
}
</script>
```

Smint.io's own `page-generic-1` page template is that file almost verbatim — a good place to
start if you have the [reference sources](../Reference/README.md).

Note that the template binds `contentGap`, so the property has to be declared: a page template
whose markup reads a setting it never declares renders with `undefined` and no error.

## Declaring slots

```javascript
@UIComponentSlot({
    slotId: "header",
    minimumItems: 0,
    maximumItems: 1,
    allowedUiComponentTypes: ["ui-type-header"],
    deniedUiComponentTypes: [],
})
public headerSlot: IUIComponentInfo[] = [];
```

| Option | Meaning |
|---|---|
| `slotId` | the slot's identity — persisted with every page built from the template, so never rename it after release |
| `minimumItems` | the page composer refuses to save a page with fewer |
| `maximumItems` | the page composer refuses to add more |
| `allowedUiComponentTypes` | whitelist of UI component types |
| `deniedUiComponentTypes` | blacklist |

Omitting both type lists means the slot accepts any UI component, which is right for a generic
content area and wrong for a header.

`UIComponentSlot` does **not** create an ordinary data property. It replaces the field with a
computed that reads the slot's components from the page context, and declares the page context
prop the runtime fills in. Write the field with an `= []` initializer so it types correctly, and
never assign to it.

## Rendering slots

The slot renderers come from `@smintio/portals-components`. **They have to be registered before
you can use their tags.** `SPageMixin` registers `SHeaderSlot`, `SFooterSlot`, `SGenericSlot` and
`SGenericMultiSlot` for you — and brings `SCssProps` along. `SSideSlot`, `SSideMenuSlot` and
`SBannerSlot` are not covered by it: import those and list them under `components` in the
`PortalsPageTemplateComponent` metadata when you use them. Without the registration the tag
renders nothing and Vue warns about an unknown custom element.

| Component | Use for |
|---|---|
| `<s-header-slot>` | the header area |
| `<s-footer-slot>` | the footer area |
| `<s-side-slot>` | a plain left or right column |
| `<s-side-menu-slot>` | a collapsible left or right drawer with an activator — takes `v-model`, `width`, `right`, `activator-text` |
| `<s-banner-slot>` | a banner area |
| `<s-generic-slot>` | a single content area — the workhorse |
| `<s-generic-multi-slot>` | three stacked areas in one renderer: `top-ui-slot`, `center-ui-slot`, `bottom-ui-slot`, each with its own `*-ui-slot-data` |

The renderers also implement **`component:hide`**: a hosted UI component that emits it is dropped
from the slot entirely — no column and no content gap are laid out for it — and a section start
that emits it takes its whole section with it. Your page template gets this by using the
renderers; a page that places components itself, without them, does not. See
[the frontend reference](smintio-frontend-reference.md#events-a-component-can-emit).

### Layout props on the slot renderers

`s-generic-slot` and its siblings carry the spacing controls. The useful ones:

| Prop | Effect |
|---|---|
| `max-width-desktop` / `max-width-mobile` | hard pixel cap on the slot |
| `content-width-desktop` / `content-width-mobile` | content columns out of 12 (`0` means "use the default") |
| `default-content-width-desktop` / `-mobile` | what to use when the portal editor leaves the setting at its default |
| `content-gap` | the gap size token — `none`, `small`, `medium`, `large`, `xlarge`, `xxlarge`, `default` |
| `apply-top-content-gap` / `apply-bottom-content-gap` | whether to add the gap above and below the whole slot |
| `default-padding-desktop` / `-mobile` | slot padding |
| `container-fluid`, `slot-tag` | container behaviour and the wrapper element |

Wire the editor-facing versions of these through `SContentWidthProps`, which contributes
`slotWidthPixelsDesktop`, `slotWidthPixelsMobile`, `contentWidthDesktop` and
`contentWidthMobile` as configuration properties. A page template that declares those properties
but does not pass them to its slot renderers gives the editor settings that do nothing.

### Passing data and events into a slot

`ui-slot-data` is a Vue render data object — `props`, `on`, `attrs`, `class`, `style`. This is
the mechanism by which a page fulfils the **page type contract** with the UI components it
hosts.

```vue
<s-generic-multi-slot
    :top-ui-slot="searchBarSlot"
    :top-ui-slot-data="{
        props: { currentSearch },
        on: { 'query-string-changed': onQueryStringChanged },
    }"
    :center-ui-slot="searchResultSlot"
    :center-ui-slot-data="{
        props: {
            searchUuid, currentPage, currentQueryString, currentSearch,
            results: currentResults, totalResults, isSearching, hasMoreResults, errors,
        },
        on: {
            'next-search-page': onNextSearchPage,
            'clear-query': clearQuery,
            'remove-search-value': removeSearchValue,
        },
    }"
/>
```

`page-media-gallery-assets-search-1` is the fullest example.

**This binding block *is* the contract** — there is no separate declaration of it anywhere. If
you are writing a page template for an existing page type, copy the prop and event names from
the existing template for that type exactly, or components built against that page type will
silently receive nothing.

[The page type contracts](smintio-page-type-contracts.md) tabulate what every Smint.io page
template passes into every slot, generated from these bindings. Check a new template against the
row for its page type before you publish: two templates of the same page type offering different
contracts is a bug in one of them.

## Page types

`type` must be one of the page types listed in
[the frontend component types](smintio-frontend-component-types.md). The list is maintained by
Smint.io — please get in touch at [support@smint.io](mailto:support@smint.io) if you need one
that does not exist.

The page type is what makes a page contract meaningful: a UI component declaring
`type: "ui-type-search-result"` may rely on being hosted by a `page-type-assets-search` page, and
can check it at runtime:

```javascript
this.hostedInAssetsSearchPage = this.pageContext?.pageType === "page-type-assets-search";
```

### Dialog pages

`page-type-generic-dialog` renders inside a dialog rather than as a full page. Such a template
receives the dialog payload as a plain prop and forwards it to its content slot:

```javascript
@Prop()
public readonly dialogData!: IPageDialogData;
```

```vue
<s-generic-multi-slot
    :top-ui-slot="topSlot"
    :center-ui-slot="contentSlot"
    :center-ui-slot-data="{ props: { dialogData } }"
    :bottom-ui-slot="bottomSlot"
/>
```

`PortalsPageTemplateComponent` also accepts `isDialog` and `dialogAlignment` (`center`, `left`,
`right`, `top` or `bottom`) for finer control.
`page-generic-dialog-1` is the working example.

## Configuration properties

Identical to UI components — the same annotations, the same rules, the same
[reference tables](smintio-frontend-reference.md). The page specific mixins are:

| Mixin | Contributes |
|---|---|
| `SPageMixin` | registers the common slot renderers, and mixes in `SCssProps` |
| `SPageTemplateColorsProps` | the page level colour settings |
| `SContentWidthProps` | the slot width and content width settings |

Beyond those, page templates commonly add their own gap setting — `contentGap` in the skeleton
above. Please keep that gap vocabulary: `none`, `small`, `medium`, `large`, `xlarge`, `xxlarge`,
`default`. It matches the global `s-*-space-between-components-*` CSS classes and the equivalent
setting on UI components, so a portal's spacing stays consistent. Every Smint.io page template
that offers the setting uses exactly those seven values.

## Resources

`resources/definition.ts` uses the page template builder rather than the UI component one:

```javascript
import type { IResourceDefinitionBuilder } from "@smintio/portals-resource-builder-cli";

export default (builder: IResourceDefinitionBuilder): Promise<void> =>
    builder.buildPageTemplateResourceDefinition(async (pageBuilder) => {
        pageBuilder.setDefaultSettings(pageBuilder.createDefaultSettings());

        await pageBuilder.defineResources(async (resourceBuilder) => {
            await resourceBuilder.loadFileResources(async (fileResources) => {
                await fileResources.verifyLoadedResources(async (resource) => resource);
            });
        });
    });
```

This writes `portals-page-template.json`. Everything else about resources works exactly as it
does for UI components, including
[shipping your own string resources](../README.md#user-content-shipping-your-own-string-resources)
with `addEmbeddedResource`.

## Build, run, publish

The same as for UI components, including the
[publish boundary](../README.md#user-content-what-the-dev-server-covers-and-what-still-needs-publishing):
markup and behaviour are served from your working folder, but anything that changes the settings
surface must be published before the page editor sees it. **For a page template that includes the
slot declarations** — adding a slot, renaming a `slotId`, or changing `minimumItems`,
`maximumItems` or the accepted types is a publish, not a reload.

Two details differ:

- The dev server mapping points at the page bundle rather than the UI component bundle:

```json
"ComponentMappings": {
    "page-example-hello-world-1": "page-example-hello-world-1/lib/portals-page.umd.js"
}
```

  Make the mapping match `main` in your `package.json`. Rollup takes the output filename from
  `main` and never renames it, so whatever you put there is what the dev server has to find —
  note that page templates are conventionally **not** suffixed `.min`, unlike UI components,
  even though both are minified for a production build.

- The dev server reads `appsettings.json` only when it starts, so restart it after adding the
  mapping.

The [two portal-side switches](../README.md#user-content-two-switches-decide-whether-your-local-build-is-asked-for-at-all)
apply unchanged: the portal needs *Basic settings > Development mode* enabled, and a user must be
logged in whom Smint.io has cleared as a developer. Without both, the page never asks for your
local bundle — the dev server's request log stays empty and the mapping is irrelevant.

## Checklist

- [ ] the entry file is `src/PortalsPage.vue`
- [ ] `rollup.config.js` imports `rollup-config-page.ts`, and `main` points at the page bundle
- [ ] `type` is a real page type, and `key` is your partner id plus the directory name
- [ ] every slot has a stable `slotId`, and type restrictions wherever the slot is not generic
- [ ] header and footer slots capped at `maximumItems: 1` and restricted by type
- [ ] the content slot has `minimumItems: 1` if an empty page makes no sense
- [ ] all the spacing between components is handled here, not in the components
- [ ] if implementing an existing page type: prop and event names copied exactly from the
      existing template for that type
- [ ] `SContentWidthProps` wired through to the slot renderers, so the editor's width settings
      actually apply
- [ ] `resources/definition.ts` uses `buildPageTemplateResourceDefinition`, and
      `portals-page-template.json` is regenerated rather than hand-edited
- [ ] `npm run build` and `npm run lint` clean, and `version` bumped
- [ ] dev server mapping added, the dev server restarted, and the template verified in a real
      portal

Contributors
============

- Reinhard Holzner, Smint.io GmbH

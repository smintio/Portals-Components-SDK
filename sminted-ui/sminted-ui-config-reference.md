Configuration reference
=======================

Everything an administrator can change about your component, you declare in one file. This is the
reference for that file.

Current version of this document is: 1.0.0 (as of 14th of September, 2026)

Everything here is imported from `@smintio/sminted-ui-sdk/configs` — note the plural.

1. [The shape of a configuration](#user-content-the-shape-of-a-configuration)
1. [Metadata](#user-content-metadata)
1. [Declaring a setting](#user-content-declaring-a-setting)
1. [Setting types](#user-content-setting-types)
1. [Portal resources and assets](#user-content-portal-resources-and-assets)
1. [Organising the form](#user-content-organising-the-form)
1. [Showing a setting only when it applies](#user-content-showing-a-setting-only-when-it-applies)
1. [Defaults, requirements and limits](#user-content-defaults-requirements-and-limits)
1. [Slots](#user-content-slots)
1. [Shipping values with your component](#user-content-shipping-values-with-your-component)
1. [Reusing a group of settings](#user-content-reusing-a-group-of-settings)
1. [Things to get right](#user-content-things-to-get-right)

## The shape of a configuration

```ts
import type { InferProps, InferSlots } from '@smintio/sminted-ui-sdk/configs';
import { config, exp, metadata, propOption, resource } from '@smintio/sminted-ui-sdk/configs';

const contentGroup = 'banner-content';
const styleGroup = 'banner-style';

const Config = () =>
  config()
    .uiComponent('ui-type-banner')
    .metadata(
      metadata()
        .id('banner')
        .title('title')
        .description('description')
        .icon('mdi-image-text')
    )
    .group(contentGroup, (group) => group.title('groupContent'))
    .group(styleGroup, (group) => group.title('groupStyle'))
    .prop('headline', resource().string(), (prop) =>
      prop.display('headline').required().group(contentGroup)
    )
    .prop('shape', (prop) =>
      prop
        .string<'pill' | 'square'>()
        .display('shape')
        .defaultValue('pill')
        .group(styleGroup)
        .options([propOption('pill').display('shapePill'), propOption('square').display('shapeSquare')])
    )
    .slot('default')
    .build();

export default Config;

export type Props = InferProps<ReturnType<typeof Config>>;
export type Slots = InferSlots<ReturnType<typeof Config>>;
```

`InferProps` derives your component's prop types from the declaration, so the two can never drift
apart. Use it rather than writing the type by hand.

Start with `.uiComponent(type)` for a UI component, or `.pageTemplate(type)` for a page template.

## Metadata

`metadata()` describes the component itself — what the administrator sees when choosing one.

| Call | Purpose |
|---|---|
| `.id('banner')` | Identifies the component. Permanent once released. |
| `.title(key)` | The name shown in the component picker. |
| `.description(key)` | One line explaining what it is for. |
| `.icon('mdi-image')` | An icon for the picker. |
| `.illustration(portalType, url)` | A preview image, per portal type. |
| `.allowedPortalTypes(...)` | Restrict to `brand_portal`, `media_gallery` or `press_portal`. |
| `.dialog()` / `.dialogAlignment(...)` | Page templates only — render as a dialog, and from which edge. |

Set `allowedPortalTypes` only when the component genuinely makes no sense elsewhere. Every
restriction is a component an administrator cannot find.

## Declaring a setting

A setting is a name, a type, and some description of how it behaves:

```ts
.prop('maxItems', (prop) => prop.int().min(1).max(50).display('maxItems'))
```

For a setting backed by a portal resource or asset, the source goes in the middle:

```ts
.prop('logo', asset().image(), (prop) => prop.display('logo'))
```

**The name is permanent.** It is the key the administrator's choice is stored under. Renaming it
loses every portal's saved value for that setting.

## Setting types

Pick the most specific type available. The type is what decides which editor the administrator
gets — a colour picker, a page chooser, a rich text box — so reaching for a plain string where a
real type exists gives them a worse tool for no reason.

**Text and numbers**

| Call | Gives the administrator |
|---|---|
| `string()`, `stringArray()` | plain text |
| `int()`, `long()`, `decimal()` (and `…Array()`) | a number field |
| `boolean()` | a switch |
| `date()` | a date picker |
| `localizedString()`, `localizedStringArray()` | text per language |

**Text with a known meaning**

| Call | Gives the administrator |
|---|---|
| `color()` | a colour picker — see the note below |
| `email()`, `phoneNumber()`, `uri(opts)` | a validated field |
| `richText(opts)`, `richTextLocalized(opts)` | a formatted text editor |
| `json()`, `liquid()`, `javascript()` | a code editor |

`uri()` accepts `{ enforceHttps, removePathAndQueryString }`; `richText()` accepts an allowed HTML
tag list.

**A word on `color()`.** Think twice before adding a colour setting. Your component's colours
should come from the portal's own style through design tokens, and an administrator who wants a
different colour overrides the token — no setting needed. A colour setting is for the rare case
where a specific placement genuinely needs its own value. See
[tokens and theming](sminted-ui-tokens-and-theming.md).

**References into the portal**

| Call | Gives the administrator |
|---|---|
| `page()` | a page of this portal to link to |
| `assetsReference(args)` | a way to choose assets — by id, by folder, by search, or related to the current asset |
| `assetsSearch()` | a data source that can be searched |
| `metadataAttributes()` | which metadata fields to display |
| `image()`, `video()` | a media value |

See [data binding](sminted-ui-data-binding.md) for what these give you at runtime.

## Portal resources and assets

A *resource* is content managed centrally in the portal and reused across components — a text, a
menu, a category. Pointing a setting at a resource lets an administrator change the wording in one
place and have it update everywhere.

```ts
.prop('headline',  resource().string(),    (prop) => prop.display('headline'))
.prop('body',      resource().text(),      (prop) => prop.display('body'))
.prop('menuItems', resource().menuItems(), (prop) => prop.display('menuItems').required())
.prop('logo',      asset().image(),        (prop) => prop.display('logo'))
```

`resource()` offers `string()`, `text()` (rich text), `menuItem()`, `menuItems()`, `category()` and
`style()`. `asset()` offers `image()` and `video()`.

Use `resource().string()` for any text an administrator might reasonably want to share or
translate — which is nearly all visible copy. A bare `string()` is for a value that is genuinely
local to one instance, such as a keyboard shortcut.

## Organising the form

Long flat forms are hard to use. Two levels of structure are available:

- A **group** is a section of the form. Declare it, then assign settings to it.
- A **fieldset** is a labelled cluster of related settings inside a group.

```ts
.group(styleGroup, (group) => group.title('groupStyle'))
.fieldset('basket', (fieldset) => fieldset.title('basket'))

.prop('accent',   (prop) => prop.color().display('accent').group(styleGroup))
.prop('useBasket',(prop) => prop.boolean().display('useBasket').group(actionsGroup).fieldset('basket'))
```

Use `.order(n)` where the natural reading order matters. Every fieldset you reference must be
declared, or the configuration will not build.

## Showing a setting only when it applies

Settings that only matter in one mode should appear only in that mode:

```ts
const whenSearchEnabled = exp('showSearch').boolean().eq(true);

.prop('showSearch',  (prop) => prop.boolean().defaultValue(true).display('showSearch'))
.prop('searchHotkey',(prop) => prop.string().display('hotkey').visibleIf(whenSearchEnabled))
```

`exp(name)` builds the condition; the comparisons available are `eq`, `ne`, `gt`, `gte`, `lt`,
`lte` and `oneIf`.

There is also `.visibleOnView('expert')` for settings that should stay out of the way until
someone goes looking for them.

## Defaults, requirements and limits

| Call | Effect |
|---|---|
| `.defaultValue(v)` | The value used until the administrator changes it |
| `.required()` | The form will not save without it |
| `.min(n)` / `.max(n)` | Numeric bounds |
| `.options([...])` | A fixed choice list, built with `propOption(value).display(key)` |
| `.disableTrimming()` | Keep leading and trailing whitespace |

Give a sensible `defaultValue` to everything you can. A component that looks right the moment it
is dropped onto a page is far more likely to be used than one that must be configured first.

There is no built-in text length or pattern validation yet. If a setting has a format requirement,
validate it in your component and say so in the setting's description.

## Slots

Page templates declare slots; UI components can declare them too, for content a page supplies.

```ts
.slot('header', (slot) => slot.allowed('ui-type-header', 'ui-type-banner'))
.slot('content', (slot) => slot.minItems(1))
.slot('footer', (slot) => slot.maxItems(1))
```

`minItems`, `maxItems`, `allowed(...)` and `denied(...)` are the four controls. See
[component types](sminted-ui-component-types.md) for the full story.

## Shipping values with your component

`.setup(...)` supplies the values your component should start with — a default menu, a placeholder
image bundled with the package:

```ts
.setup((setup) =>
  setup
    .resource('headline', (r) => r.name('Default headline').value('en', 'Welcome'))
    .asset('placeholder', (a) => a.file('placeholder.png'))
)
```

## Reusing a group of settings

When several components need the same cluster of settings — a link, a placeholder, a set of
labels — declare it once and merge it in, rather than copying it:

```ts
import { createSmtLink } from '@smintio/sminted-ui-core-components/config';

.props(createSmtLink({ id: 'ctaLink', fieldsetId: 'cta', fieldsetLabel: loc().localizationKey('cta') }))
```

You can also build your own reusable bundle with `propsConfig()` and merge it with `.props(...)`.

This matters more than it looks. Consistent setting names and wording across components are what
make a portal feel like one product rather than a collection of plugins.

## Things to get right

- **Every `display`, `description` and `title` is a translation key**, not a literal. It must exist
  in `lang/en.json` and in every other language file you ship. See
  [localization](sminted-ui-localization.md).
- **Do not declare a setting that does nothing.** An administrator will try it and lose trust in
  the rest of your form.
- **Do not mirror your design tokens as settings.** Expose the few meaningful choices and derive
  the rest — see [tokens and theming](sminted-ui-tokens-and-theming.md).
- **A change to this file needs a publish** before it reaches the portal editor. Code changes do
  not — see [publishing](sminted-ui-publishing.md).

Contributors
============

- Reinhard Holzner, Smint.io GmbH

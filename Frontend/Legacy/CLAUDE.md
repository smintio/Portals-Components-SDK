# Building Smint.io Portals frontend components

This directory is everything you need to build a **frontend component** — a UI component or a
page template — for Smint.io Portals. It is written for an agent doing the work, and for the
developer reading over its shoulder.

| | |
|---|---|
| `README.md` | the guide: concepts, the questions to settle, getting started, building, publishing, the local development loop, common problems |
| `docs/` | the reference tables — annotations, mixins, services, filters, component and page types, the data adapter public API, the page type contracts |
| `Example/ui-example-hello-world-1/` | the minimal component, meant to be copied |
| `Reference/` | where the sources of the components Smint.io ships go — 64 UI components, 34 page templates, the shared `portals-components` library and the gallery view. **They are not in this repository**: they are available to Smint.io Certified Partners on request and copied in by the partner, so assume they are absent unless you have looked |
| `config/` | the shared rollup and TypeScript config a component extends |

The stack is **TypeScript, Vue.js 2 and Vuetify 2**, built with rollup on **Node 12.22.10**.
Newer Node breaks the toolchain; see *Build your custom frontend component* in `README.md`.

## Read before writing component code

Do not infer the conventions from a single file — they are written down.

| Document | Read it when |
|---|---|
| `README.md` | always, first — it is the full guide |
| `docs/smintio-frontend-recipes.md` | you know *what* to build and need *how*: worked, copy-able answers to the common tasks — fetching and rendering data, paging, permissions, downloads, page contracts, sections |
| `docs/smintio-annotations.md` | you are declaring a configuration property or component metadata |
| `docs/smintio-frontend-reference.md` | you need services, filters, `S*Props` mixins, shared components, allowed-values providers, `@Implements` names, data types or the global CSS classes |
| `docs/smintio-mixins.md` | the component downloads, shares or collects assets, or needs routing, permissions, metadata or asset references |
| `docs/smintio-frontend-component-types.md` | you are choosing the component's `type` |
| `docs/smintio-building-page-templates.md` | you are building a **page template** rather than a UI component |
| `docs/smintio-data-adapter-reference.md` | the component reads assets, collections, shares or search results |
| `docs/smintio-page-type-contracts.md` | the component targets a page type — the exact props and events each page hands into each slot |
| `docs/smintio-ui-components.md`, `docs/smintio-page-templates.md` | you are looking for the nearest existing component to start from |

The last two are generated from the components themselves, so they answer most questions without
the source. If the reference sources have been copied into `Reference/` — see
[`Reference/README.md`](Reference/README.md) — read the component itself when a table falls
short. If they have not, say so rather than guessing at what a component does.

## The short version — what is most often got wrong

- **Entry filenames are fixed**: `src/PortalsUiComponent.vue` for a UI component,
  `src/PortalsPage.vue` for a page template. Rollup hard-codes them.
- **`portals-ui-component.json` / `portals-page-template.json` are generated** from
  `resources/definition.ts` by `npm run build:resources`. Never hand-edit them — and note that
  `npm run watch` does **not** regenerate them.
- **Every configuration property needs a data type** (`@Is*()` or `@Implements("…")`) or the
  component throws *"Some properties lack type definition"* at registration, and a pinned
  `@ComponentProperty({ name })` or a later rename orphans every portal's saved configuration.
- **Reuse the `S*Props` mixins** from `@smintio/portals-components` instead of re-declaring
  text, link, colour or layout properties. `SCssProps` and `SHtmlProps` collide — pick one.
- **Mix in the layout props even when the template never reads them.** `SBottomGapProps` is the
  one that gets forgotten: it declares `bottomGap` — the editor's *Gap bottom* — and the
  component applies nothing itself. The page slot reads the configured value and puts the
  matching `s-mb-space-between-components-*` class on the wrapper. So do not go looking for
  where to use it, and do not conclude it is unused and drop it. Leaving it out is invisible
  while you build and shows up only when a portal editor misses the spacing control that every
  other component on the page has. Adding it later costs another publish, because a new
  property is an annotation-level change.
- **Bump `version` in `package.json` with every functional change.** Publishing the same
  version again fails.
- **Nothing in a component's code decides who gets it.** The tenant comes from the publish
  CLI's `appsettings.<Env>.json`, so a publish registers the component for one tenant. Never
  read or quote values out of those files — they hold an OAuth client secret and an npm
  authorization header.
- **A new package needs its own `.npmrc`**, or the registration half of a publish fails while
  `npm publish` has already succeeded. See *The two steps fail independently* in `README.md`.
- **Copy a component only when it is a different component.** If yours is "the generic one plus
  something", extend it instead — see below.

## A new component starts with an interview

Never start coding one from a one-line request. The things that cannot be corrected later — the
persisted `key`, the `type`, the pinned `@ComponentProperty` names, the tenant it is published
for — have to be settled before the first file is written.

Collect them as an **interview**: structured multiple-choice prompts, at most four questions per
prompt, your recommendation first, never a paragraph of prose questions. Ask only for what
cannot be derived — state the derived values instead of asking for them (the `key` is your
partner id plus the folder name; the package name is your npm scope plus the folder name;
`main`, `typings`, `files`, `publishConfig` and the dependency versions come from the component
you copied).

The same questions, written for a human rather than as an asking protocol, are in
`README.md` under *Before you start: the questions to answer*. Keep the two in step.

**Ask all of the questions. Do not skip any.**

### Round 1 — identity (blocks everything else)

| Ask | Offer | Why it cannot wait |
|---|---|---|
| What does it do, in one sentence, and what is the folder called? | propose `ui-<scope>-<what-it-does>-1`, where `<scope>` is `generic` unless the component is bound to one portal type | the folder name *is* the key, and the key is immutable after release |
| New component, or a variant of an existing one? | a new `-1` / a `-2` variant of an existing package / change the existing one in place | you never renumber a released component — portals in the field are configured against the old key |
| Which UI component type? | recommend one `ui-type-*` from `docs/smintio-frontend-component-types.md`; `ui-type-custom` as the fallback | the type decides which page-template slots accept it and which page contract it may rely on. A type that does not exist cannot be invented — ask Smint.io |

### Round 2 — where it sits and where its content comes from

| Ask | Offer | Why |
|---|---|---|
| Which portal types, and which page does it go on? | `brandPortal` / `mediaGallery` / `pressPortal` / any | sets `allowedPortalTypes`, and picks the contract in `docs/smintio-page-type-contracts.md` |
| Where does the content come from? | editor configuration only / the standard data adapter public API interfaces / a data adapter interface of its own / an external API | its own data adapter means a local `I<Name>DataAdapter.ts`, generated by the Data Adapter Exporter CLI |
| Which existing component should it start from, and is this that component **plus something** or something new? | recommend the nearest match from `docs/smintio-ui-components.md` — and from the reference sources, if they are present — and say which of the two it is | it fixes half the structure — and decides copy versus extend. "Plus something" means extend; only a genuinely new component gets copied. Recommend, do not ask twice: if the answer is obvious from the one-sentence description, state it and move on |
| Which settings must the portal editor be able to change? | list them, and mark which are advanced | each becomes a pinned configuration property. Start with few — you can add, you cannot remove. Text, link, colour and layout settings come from the shared `S*Props` mixins, so ask only for what those do not already cover |

### Round 3 — content and language

| Ask | Offer | Why |
|---|---|---|
| Which languages? | English only / English plus one more / more | the `@DisplayName` / `@Description` sets, and whether editor-facing text is `ILocalizedStringsModel` |
| Anything pre-filled or shipped with the component? | default form field values on drop (`setFormFieldValues`) / string resources / images or files in `resources/` / an `mdiIcon` or illustration for the page composer | a component that lands blank in the composer looks broken |
| What should it look like? | a design or mockup / a screenshot / an existing portal page to match / free hand within the design system | there is no visual spec anywhere else |

### Round 4 — delivery

| Ask | Offer | Why |
|---|---|---|
| **Which tenant is it published for?** | ask for the tenant by name; offer "not decided yet" as an explicit option | **nothing in the component's code decides this.** The tenant is whatever `SmintIo.ApiUrl` the publish CLI's `appsettings.<Env>.json` points at, so a publish registers the component for that one tenant — and the wrong one registers it in the wrong tenant's page editor. Record the answer and repeat it back before any publish |
| Which environment? | `development` / `staging` / `production` | picks which `appsettings.<Env>.json` applies, i.e. which `npm run smint-io-pc*` script. Each environment has its own tenant URL, so confirm the tenant per environment |
| Is there a ticket or issue id? | the id, or none | the commit message and the pull request |

Making a component available to more than one tenant is arranged with Smint.io — it is not an
option to offer in the interview. And never read or quote values out of the
`appsettings.<Env>.json` files; ask for the tenant by name.

**Never publish unprompted.** Ask before publishing, every time, and state the tenant and the
environment in the question.

### Decide these yourself — do not ask

Start `version` at whatever you are currently on, or `1.0.0` for a first component. Regenerate
`licenses.json` rather than inheriting the one from the component you copied. Generate
`portals-ui-component.json` from `resources/definition.ts`. Add the dev server's
`ComponentMappings` entry in the same change, and restart the dev server.

If something stays unanswered, pick the sensible default, build the component, and say in the
final report which assumption you made and where it is easy to change.

## Extend a generic component — do not copy it

Starting from the closest existing component is good advice, and it splits into two very
different moves. Copy when the component is genuinely new. **Extend** when yours is an existing
component plus an addition — then take the published package as a dependency.

A fork is large and it silently opts out of every later fix to the base. A header variant that
was nothing more than the generic header plus dynamically resolved menu items was first written
as a copy — 1,349 lines plus six sub-component files — and had to be thrown away and rewritten
as a 425-line extension with no template at all.

```ts
// eslint-disable-next-line @typescript-eslint/ban-ts-comment
// @ts-ignore
import SmintIoPortalsUiComponentImplementation from "@smintio/ui-generic-header-1";

export default class PortalsUiComponentImplementation extends Mixins(
    SmintIoPortalsUiComponentImplementation,
    Mixins<AssetsReferenceMixin, RoutingMixin>(AssetsReferenceMixin, RoutingMixin)
) {
```

Add the base to `dependencies`, declare your own `key` and `type`, and override only what
differs. Points that are not obvious until they bite:

- **Write the file with no `<template>` block at all** when you are not changing the markup.
  Vue's mixin merge then keeps the base's compiled render function, its `components`, its props
  and all of its configuration properties. A `.vue` file with only a `<script>` block is valid.
- **Override the smallest thing that expresses the change** — usually one computed. `super` is
  not available, because vue-class-component moves getters into the component options, so
  re-call the base's own helpers to reproduce its result before adding to it.
- **The base ships typings, so do not re-declare inherited members.** Declaring an inherited
  getter as a property fails the build with `TS2610: … is defined as an accessor … but is
  overridden here as an instance property`. Just use the member.
- **Lifecycle hooks merge, they do not replace.** Your `created()` runs alongside the base's.
- **The base's annotations come with it**, in every language it ships. A language decision
  applies only to the properties your own component declares.
- **Base behaviour keeps applying.** If the base limits or filters something, anything you
  append has to opt out of that deliberately — and say so in a comment.
- **Keep the package's own `node_modules` empty** apart from the link to the base. A full
  `npm i` inside the package shadows the hoisted tree, `rollup-plugin-typescript2` stops
  transpiling, and the build dies on the first decorator with `Unexpected character '@'`.

## Shipping string resources with a component

**Not every `ILocalizedStringsModel` property is a resource.** What makes one a resource is a
provider — `StringResourceAllowedValuesProvider` or `TextResourceAllowedValuesProvider`.
Without one it is an ordinary localized text field the editor types into.

| Property has | Editor gets | Right when |
|---|---|---|
| no provider | a plain localized text field | the text is used only by this component |
| a string or text resource provider | a dropdown of the portal's resources | the text must be **reusable across several components** |

`STextProps` is the first kind — `headerText`, `subHeaderText` and `continuousText` carry no
provider. `SDownloadProps` is the second kind throughout. Reach for a resource when the text is
shared, not by default.

**Check what a portal-side resource actually renders before defaulting a property to it.** The
id reads like a label, but the wording is whatever that portal defined, and it was usually
written for the component that introduced it. A generic "browse assets" resource that renders
*"Browse all related assets"* was simply wrong on a card whose sibling button said *"Browse
products"*, and the two stayed inconsistent until someone noticed it on the live portal. Ship
your own unless the text genuinely has to match other components.

The full mechanics — `addEmbeddedResource`, `"string"` versus `"text"`, matching the property's
provider, and `@DefaultValue` versus `setFormFieldValues` — are in `README.md` under *Shipping
your own string resources*. Two things to keep in mind here:

- `@DefaultValue("<id>")` writes nothing to the database and can be changed in a later version.
  `setFormFieldValues` is a **real database write** at instantiation and does not reach
  instances that already exist. Default to `@DefaultValue`.
- A new or changed resource is an **annotation-level change**: it does not reach the page
  editor until the package is published.

After changing `resources/definition.ts`, run `npm run build:resources` (or the full
`npm run build`). `npm run watch` does not.

## Linking to a search page — do not build the filter yourself

A button that opens a pre-filtered search looks like string work: take an id, put it in a query
parameter, done. It is not, and hand-building it is the wrong first move.

**Look for a search spec that already exists.** An asset carries its own searches in
`relatedAssets`, one entry per relationship type, each holding a complete search spec — the
filters, the data adapter instance, the query string. Pull it with
`getAssetReferenceByRelationshipType(asset, "<relationship type>")` on `AssetsReferenceMixin`
and hand it on. Nothing about the filter is then encoded in the component, so a change on the
data side needs no release.

**The encoding already exists too.** A search page restores a search from the query string as
one parameter per filter, named after the search field, plus the same name prefixed with `dt-`
carrying the field's data type, and the free text as `query`. The canonical implementation is in
the `ui-generic-assets-preview-1` component — search it for `"dt-" + id`, a typed switch over
the `ValueType` cases — in the reference sources, if you have them. Copy that, note in a comment
where you copied it from so a
new `ValueType` case can be traced back, and **take the data type from the spec, never from a
sample URL** — a sample once showed `string_array` where the spec said `string`.

This was learned the expensive way. A first implementation invented two configuration
properties for the field paths and guessed the data type; the assets had carried the correct,
complete specs all along. The properties had to be deleted again — free only because nothing
had been published yet. **Pinned property names cannot be removed after a release**, so an
unnecessary property is not a harmless extra.

**A page reference resolves to a *named* route, which has no path.**
`generateRouterLocation(page, { query })` returns `{ name: "pgt:<page id>", query, hash }` — no
`path`, no `fullPath`. Bind that object to `:to` and it works. But some renderings want a URL
*string*, and building one from `location.path` yields `undefined`. Let the router do it:

```ts
return this.$router.resolve(location).href;
```

A header extension once shipped with the hand-built version. The failure was invisible: no URL
meant no sub-items, which meant the parent had nothing under it, which meant a "leave out an
item with nothing under it" rule dropped it — an empty menu, no console output, on a component
that had already been published. **When your component decides to render nothing, log why.** A
silent empty result is indistinguishable from a broken registration, a stale bundle or a wrong
configuration, and you will pay for the ambiguity in browser sessions.

**Several cards linking to the same search page need `exact` on the link.** Vue-router's
default active matching compares the path and ignores the query, so every card's button points
at the "current" route as soon as one of them is followed and the whole grid highlights at once.
`exact` makes the comparison include the query. It looks like a cosmetic prop and it is not; say
so in a comment, or someone tidies it away.

## Verifying your work

**Build.** It is the only automatic check that exists — the component packages carry no tests.
`npm run build` runs both halves (`build:dist` and `build:resources`); `npm run lint` is the
other one worth running before you report done.

**Then look at it in a real portal.** The dev server reroutes a development portal's component
requests to your working tree, so your local build runs against real data. The full loop is in
`README.md` under *Local development*. Two things about it decide most of the confusion:

- **What needs a publish and what does not.** Markup and behaviour: rebuild and reload.
  Anything that changes the settings surface — a new or renamed property, a changed
  `@DisplayName`, `@Description`, `@DefaultValue`, `@AllowedValues`, `@VisibleIf`,
  `@FormGroup*`, the component metadata, a page template's slots — needs a publish, because the
  configuration form lives on the Smint.io server and not in your bundle.
- **Two portal-side switches gate the rerouting**, and neither is in your control: the portal
  needs *Basic settings > Development mode* enabled, and a user must be logged in whom Smint.io
  has cleared as a developer (the clearance sits on the frontend user, so an anonymous session
  never qualifies). Without both, your machine is never queried at all — the portal renders
  normally and the dev server log stays empty. Diagnose an empty log as one of these two, not as
  a wrong mapping.

**The portal shows your working tree, not the published package.** A green page proves your
local build works; it says nothing about what is registered. The component's script URL carries
the registered version, so read it out of that request to check whether a publish landed. That
request is also the quickest way to find out whether the page uses your component at all.

### Driving a browser against a running component

Reach the live instance through its root element and work on the component object itself, which
beats reading rendered markup:

```js
const vm = document.querySelector(".<your-root-class>").__vue__;
vm.assets[0];                       // the resolved data, exactly as the component sees it
vm.someButtonLocation(vm.assets[0]) // a computed router location, before it becomes a URL
vm.$router.push(vm.someButtonLocation(vm.assets[0]));   // follow a link
await vm.$nextTick();               // then inspect classes, counts, whatever changed
```

Four things that cost time in practice:

- **Clicking a `v-btn` with a browser automation tool may not navigate**, even when the anchor
  is correct. `$router.push` is the reliable way to follow a link; use clicks for what only a
  real click can show.
- **Browser automation may refuse scripts that read hrefs or query strings.** Read
  `location.pathname` and parameter *counts* instead, or push the location and let the resulting
  tab URL show you the query.
- **Match an existing design by measuring it, not by eye.** Put your component and the one you
  are matching side by side in the same DOM and compare `getComputedStyle` — radius, box-shadow,
  font tokens, the element box. That turns "looks about right" into a diff you can close.
- **Confirm a hypothesis with a count, not a screenshot.** "All buttons go grey" became
  `23 of 23 carry v-btn--active`, and `1` after the fix. Cheaper than catching a transient state
  in an image.

## Publishing is two steps, and they fail independently

`npm run smint-io-pc*` runs `npm publish` and then the registration CLI. If the npm half
succeeds and registration fails, **do not republish** — that version number is already consumed
and the retry would fail on the version alone. Rerun just the registration:

```
npm info --json | %SMINT_IO_SDK_HOME%\SmintIo.Portals.SDK.PublishComponent.CLI.exe -env <environment>
```

`No permissions to publish the component` has been seen from a version the feed had not made
visible yet, with an identical retry a minute later succeeding. Give that error one retry before
diagnosing it as a permissions problem.

A **brand new package needs its own `.npmrc`**, because `npm info` resolves the registry from
the package folder and not from `publishConfig`. Without it the registration half answers
`E404 '…' is not in the npm registry` and the CLI stops with `info: Missing component name`,
after `npm publish` has already succeeded. Copy the file from the example component when you
scaffold — it holds no credentials.

## Working in this repository itself

If you are changing the **documentation** here rather than building a component, read
`CLAUDE.md` at the repository root first. Everything in this repository is read by people
outside Smint.io: no customer or tenant names, no internal repository or path references, no
credentials or configuration values, and only supported, documented behaviour.

`Reference/` holds nothing but its own README in a fresh checkout — the component sources go
there only once a Smint.io Certified Partner has been granted access and copied them in, and a
`.gitignore` keeps them out of this repository. So **never link into `Reference/<folder>` from a
document**: the path does not exist for most readers. Name the package instead. And treat a
copied tree as read-only — it is refreshed from the reference repository, not edited in place.

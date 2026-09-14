Smint.io Portals frontend component reference sources
=====================================================

This directory is where the **sources of the frontend components Smint.io builds and ships**
belong: the UI components, the page templates, the shared component library they all build on,
and the gallery view library. They let you see how we solved a problem before you solve it
yourself, and give you a realistic component to copy rather than only the minimal example.

**The sources are not part of this repository.** In a fresh checkout this directory contains
nothing but the document you are reading. They are made available separately, and only to
Smint.io Certified Partners — see [Access](#user-content-access) below.

1. [Access](#user-content-access)
1. [Putting the code in place](#user-content-putting-the-code-in-place)
1. [What you get](#user-content-what-you-get)
1. [How to read it](#user-content-how-to-read-it)
1. [The shape of a component package](#user-content-the-shape-of-a-component-package)
1. [Where to start for a given task](#user-content-where-to-start-for-a-given-task)
1. [Building it](#user-content-building-it)
1. [Keeping it current](#user-content-keeping-it-current)

Current version of this document is: 1.0.0 (as of 14th of September, 2026)

## Access

**The reference sources are available to Smint.io Certified Partners only.** They are the
production source code of the components we ship, so access to them is granted individually
rather than published with this repository.

If you are a Smint.io Certified Partner, please get in touch at
[support@smint.io](mailto:support@smint.io) and we will grant you access to the reference
repository. If you are not, everything else in
[Frontend/Legacy](../) still applies: the [guide](../README.md), the
[reference documentation](../docs), and the
[`ui-example-hello-world-1`](../Example/ui-example-hello-world-1) starter are complete on their
own, and the generated overviews of
[every UI component](../docs/smintio-ui-components.md) and
[every page template](../docs/smintio-page-templates.md) describe what each one does, what it is
keyed and typed as, and which settings it offers — without the source.

## Putting the code in place

Once access has been granted, **copy the contents of the reference repository into this
directory**. Copy what is inside it, not the repository folder itself, so that the component
folders land directly here:

```
Frontend/Legacy/Reference/
├── README.md                  <- this file, already here
├── ui-components/             <- from the reference repository
├── page-templates/
├── portals-components/
├── gallery-view/
├── package.json               <- the monorepo root files: this is what makes the tooling work
├── package-lock.json
├── lerna.json
├── bootstrap.bat
└── .npmrc
```

The root files matter as much as the component folders: they are what lets you bootstrap the
tree and build any component in it with the same toolchain we use. Copy all of them.

This repository ignores everything you put here apart from this README, so the copied code will
not show up as a change in your checkout of the SDK and cannot be committed to it by accident.

## What you get

| Folder | What it holds |
|---|---|
| `ui-components/` | the UI components, one npm package per folder, each directly under `ui-components/` |
| `page-templates/` | the page templates, same layout |
| `ui-components/config/`, `page-templates/config/` | the shared rollup and TypeScript config every package in that folder extends |
| `portals-components/` | the shared component library published as `@smintio/portals-components` — the `S*Props` configuration mixins, the behaviour mixins, the slot renderers, the dialogs and the shared Vue components |
| `gallery-view/` | the gallery view library used by the search result and collection components |
| the repository root files | `package.json`, `package-lock.json`, `lerna.json`, `licenses.json`, `bootstrap.bat`, `.npmrc`, `.eslintrc.js`, `.prettierrc`, `.editorconfig` — so the tree can be bootstrapped and built as it stands |

The UI components divide into `ui-generic-*` (usable in a portal of any type),
`ui-media-gallery-*` and `ui-press-portal-*`. The page templates divide the same way:
`page-generic-*`, `page-media-gallery-*` and `page-press-portal-*`.

For what each one *is* — its key, its type, its configuration properties and the mixins it uses
— read the generated overviews rather than the source:

- [Overview of Smint.io UI components](../docs/smintio-ui-components.md)
- [Overview of Smint.io page templates](../docs/smintio-page-templates.md) — including the slots
  each page template declares and the UI component types they accept

## How to read it

Every package is self-contained and follows the same layout, so once you have read one you can
navigate all of them. The two files that matter are:

- `src/PortalsUiComponent.vue` (or `src/PortalsPage.vue`) — the whole component: template,
  TypeScript class with its annotated configuration properties, and scoped styles.
- `resources/definition.ts` — the resources the component ships and the form field values it
  pre-fills.

`portals-ui-component.json` and `portals-page-template.json` are **generated** from
`resources/definition.ts`. They are committed, but they are build output — read them if you want
to see the result, never edit them.

The component `key` is conventionally the `smintio-` prefix plus the directory name, and that
holds for all but two of the packages. Those two are worth knowing about before you assume you
can derive one from the other: `ui-generic-account-cookie-consent` has the key
`smintio-ui-generic-account-cookie-consent-1` although the directory carries no `-1`, and
`page-press-portal-asset-details-1` has the key `smintio-press-portal-asset-details-1`, without
the `page-` segment. A key is permanent once a portal has been configured against it, which is
why neither was ever corrected — follow the convention for anything new.

## The shape of a component package

```
ui-generic-text-1/
├── .npmrc                       # points the @smintio scope at the SDK npm feed
├── package.json                 # name, version, scripts, dependencies
├── tsconfig.json                # extends ../config/tsconfig.json
├── rollup.config.js             # loads ../config/rollup/rollup-config.ts
├── licenses.json                # third-party license roll-up for this package
├── resources/definition.ts      # resource and default-settings definition (input)
├── portals-ui-component.json    # GENERATED from definition.ts — do not hand-edit
└── src/PortalsUiComponent.vue   # the component itself — the filename is fixed
```

`src/PortalsUiComponent.vue` is the rollup entry point and its name is **hard-coded** in the
shared rollup config, as is `src/PortalsPage.vue` for page templates. Ordinary Vue
sub-components go in `src/components/` and are imported normally — they are invisible to the
page composer.

The build produces a **UMD bundle** with the SDK, the runtime, Vue, Vuetify and
`@smintio/portals-components` marked *external*. Those are provided by the portal application at
runtime rather than bundled, which is why the dependency versions have to stay aligned.

The `licenses.json` in each package is a roll-up of the licences in that package's production
dependency tree. No build script reads it and it is not published. When you copy a package as a
starting point, regenerate it rather than keeping the one you copied — it describes someone
else's dependencies otherwise.

> **If you copy a package out of this directory, fix the config paths.** A package here sits one
> level below its shared config, so its `rollup.config.js` and `tsconfig.json` point at
> `../config/…`. The standalone layout of
> [`../Example/ui-example-hello-world-1`](../Example/ui-example-hello-world-1) sits one level
> deeper and points at `../../config/…`. A mismatch fails the build with
> `Could not resolve '../config/rollup/rollup-config.ts'`. Note also that
> [`../config/`](../config) carries a separate `rollup-config-page.ts` for page templates, where
> the page templates here use their own `page-templates/config/rollup/rollup-config.ts`.

## Where to start for a given task

| You are building | Read |
|---|---|
| a simple display component with layout settings | `ui-components/ui-generic-text-1` |
| a component that calls a data adapter and routes | `ui-components/ui-generic-search-bar-1` |
| a component that participates in the search page contract | `ui-components/ui-media-gallery-search-result-1` |
| a component that lists assets and links to a pre-filtered search | `ui-components/ui-generic-assets-preview-1` |
| a component offering download, share and collect | `ui-components/ui-generic-video-1` |
| a component shipping its own string resources | `ui-components/ui-generic-color-1`, `ui-components/ui-generic-upload-assets-form-1` |
| a section component that wraps other components | `ui-components/ui-generic-section-start-table-1`, and the expansion panel, tab panel and conditional variants next to it |
| a form based component | `ui-components/ui-generic-request-generic-form-1` |
| a straightforward page template | `page-templates/page-generic-1` |
| a page template that fulfils a page type contract | `page-templates/page-media-gallery-assets-search-1` |
| a dialog page template | `page-templates/page-generic-dialog-1` |

For the minimal skeleton — the smallest thing that is still a working component — use
[`../Example/ui-example-hello-world-1`](../Example/ui-example-hello-world-1) instead. It is the
one meant to be copied for a new component.

## Building it

The copied tree carries the monorepo root files, so it can be bootstrapped and built where it
stands. You need **Node 12.22.10**, and npm authorized against the Smint.io SDK feed exactly as
described in [*Getting started*](../README.md#user-content-getting-started).

```
npm i
bootstrap.bat            # npx lerna bootstrap --hoist, after clearing package node_modules
npx lerna run build
```

Or build one package on its own, from its folder:

```
npm run build            # rollup, then the resource builder
npm run watch            # rollup only, incremental
```

`portals-components/` and `gallery-view/` are **not** part of the lerna workspace, so
`lerna run build` does not touch them. They are published packages that the components consume
from the npm feed, and the sources are there to read. Build one only if you want to, and then on
its own:

```
cd portals-components && npm i && npm run build
```

Two things to know:

- **`lerna.json` globs `ui-components/*` and `page-templates/*` only.** A package nested any
  deeper is silently skipped — no warning, no build.
- **Nothing in a package's `node_modules` but the hoisted tree.** Running `npm i` inside a
  package shadows it, `rollup-plugin-typescript2` stops transpiling, and rollup then fails on
  the first decorator with `Unexpected character '@'`. `bootstrap.bat` clears those folders for
  exactly this reason.

If `npm run build` picks up a newer Node from your `PATH`, see
[*If you also have a newer node installed*](../README.md#user-content-if-you-also-have-a-newer-node-installed).

## Keeping it current

Treat the copied code as **read-only**. We update the components constantly, so refresh your
copy from the reference repository from time to time — and to build something of your own, copy
a package out of here, or start from
[`../Example/ui-example-hello-world-1`](../Example/ui-example-hello-world-1), and work on the
copy.

The components are also published to the Smint.io component feed, so you can depend on one
directly and [extend it](../README.md#user-content-is-your-component-an-existing-one-plus-something-extend-it-do-not-copy-it)
rather than copying it. That is the better move whenever your component is an existing one plus
an addition.

Contributors
============

- Reinhard Holzner, Smint.io GmbH

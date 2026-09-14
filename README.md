Smint.io Portals Components SDK
===============================

Examples, reference documentation and tools for building your own components for the
Smint.io Portals solution.

Current version of this document is: 2.0.0 (as of 14th of September, 2026)

New here? Read [What are Smint.io Portals components?](OVERVIEW.md) first — it explains how a
portal is assembled from components, and which component type does what.

## What do you want to build?

| I want to… | Go to |
|---|---|
| Build a **frontend component** — a UI component or a page template | [sminted-ui/](sminted-ui/) *(current, Vue 3)* |
| Maintain a frontend component written on the **previous generation** | [smintio-components/](smintio-components/) *(Vue 2)* |
| Connect Smint.io Portals to an **external system** | [connectors/](connectors/) |
| Expose data from a connected system to components | [data-adapters/](data-adapters/) |
| Build a data processor, task handler, portal template or identity provider | [BACKEND-COMPONENTS.md](BACKEND-COMPONENTS.md) |

## The repository

```text
.
├── OVERVIEW.md              what Smint.io Portals components are, and how they fit together
├── TOOLS.md                 the command line tools you will need
├── BACKEND-COMPONENTS.md    the guide to all backend component types
│
├── sminted-ui/              frontend components — Sminted UI (Vue 3), current
├── smintio-components/      frontend components — Smint.io components (Vue 2), previous generation
├── connectors/              connector guide, meta-model reference and three worked examples
├── data-adapters/           data adapter guide, asset data model reference and three worked examples
└── tools/                   released command line tools
```

Every folder holds its own guide in `README.md`, its reference documents next to it, and its
worked examples as sub-folders.

## Two generations of frontend components

Smint.io Portals is moving its frontend from **Smint.io components** (Vue 2) to
**Sminted UI** (Vue 3).

|  | Smint.io components | Sminted UI |
|---|---|---|
| Technology | Vue 2, TypeScript decorators, rollup | Vue 3, Composition API, Vite |
| Settings declared with | annotations on a class | a `*.smintio.config.ts` configuration file |
| Status | supported, maintenance | **current — build new components here** |

The two generations describe the *same system*: the same component type ids, the same page type
contracts, the same data adapter model, the same asset shapes. Only the authoring technology
differs. That is why the Vue 2 reference documentation remains useful even when you are writing a
Vue 3 component — it is the most complete description of what a component of a given type is
expected to do.

## Getting access

Access to the Smint.io Portals SDKs is restricted. Get in contact with
[Smint.io](https://www.smint.io) to request it. Access is granted to Smint.io Solution Partners
and to Smint.io Portals Enterprise plan customers.

You will need an account with Microsoft Visual Studio cloud offerings (Azure DevOps), as the SDKs
are hosted there.

## Questions

Please do not hesitate to contact us at [support@smint.io](mailto:support@smint.io) if you run
into any issues.

Contributors
============

- Yosif Velev, Smint.io GmbH
- Reinhard Holzner, Smint.io GmbH

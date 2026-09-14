Smint.io Portals component development tools
============================================

The command line tools you need when developing Smint.io Portals components. Each is shipped as a
release archive under [tools/](tools/), with its own README describing installation and usage.

Current version of this document is: 1.0.0 (as of 14th of September, 2026)

1. [Which tool do I need?](#user-content-which-tool-do-i-need)
1. [Portals-SDK-PublishComponent-CLI](#user-content-portals-sdk-publishcomponent-cli)
1. [Portals-DevServer](#user-content-portals-devserver)
1. [Data Adapter Exporter CLI](#user-content-data-adapter-exporter-cli)
1. [Sminted UI components use a different toolchain](#user-content-sminted-ui-components-use-a-different-toolchain)

## Which tool do I need?

| I am… | Tool |
|---|---|
| Publishing a Smint.io components (Vue 2) frontend component | [Portals-SDK-PublishComponent-CLI](tools/Portals-SDK-PublishComponent-CLI/Release/) |
| Testing a frontend component against a portal, without publishing | [Portals-DevServer](tools/Portals-DevServer/Release/) |
| Consuming a custom data adapter interface from a frontend component | [Data Adapter Exporter CLI](tools/Portals-DataAdapter-SDK-DataAdapterExporter-CLI/Release/) |
| Building a Sminted UI (Vue 3) frontend component | the `smintedui` CLI — see [sminted-ui/](sminted-ui/) |

## Portals-SDK-PublishComponent-CLI

Packages a frontend component, publishes it to the component registry and registers it with your
Smint.io Portals tenant so that it appears in the portal editor.

Available for Windows, macOS and Linux.

[Tool README](tools/Portals-SDK-PublishComponent-CLI/Release/)

## Portals-DevServer

Serves your local component build to a development portal. Requests the portal would normally make
for the published component bundle are rerouted to the files on your machine, so you can edit,
rebuild and reload without publishing anything.

Two things must be arranged by Smint.io before it can work: the portal must have *development
mode* enabled, and your user must be cleared as a developer. If the dev server shows no incoming
requests at all, it is almost always one of those two — not a configuration mistake on your side.

Available for Windows, macOS (Intel and Apple silicon) and Linux.

[Tool README](tools/Portals-DevServer/Release/)

## Data Adapter Exporter CLI

Generates TypeScript definitions for the public API interfaces of a data adapter, so a frontend
component can call a custom data adapter interface with full type safety.

**Windows only**, unlike the other two tools.

[Tool README](tools/Portals-DataAdapter-SDK-DataAdapterExporter-CLI/Release/)

## Sminted UI components use a different toolchain

Sminted UI (Vue 3) frontend components do not use the Publish CLI. They are built with Vite and
published with the `smintedui` command line tool, which is distributed as part of the Sminted UI
developer tooling package rather than as a release archive here.

The Portals-DevServer still applies — the local development loop is the same for both generations.

See [sminted-ui/sminted-ui-publishing.md](sminted-ui/sminted-ui-publishing.md).

Contributors
============

- Reinhard Holzner, Smint.io GmbH

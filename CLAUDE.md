> **Read the workspace root config first.** This repo is one of ~46 that build together.
> `.claude/CLAUDE.md` in the Claude Code config repo checked out at the **workspace root**
> (the folder containing `HCore/` and `Smint.io/`) carries two things you need: the
> **workspace map** — what each repo is, how they depend on each other, and the OpenAPI
> code-generation workflow — and the **working agreements** that govern how changes are
> made here. Read it before making cross-repo changes.
>
> Note: cross-repo dependencies are **relative `ProjectReference` paths**, so every
> repo must be cloned side by side in the standard layout or this repo will not build.

# Portals-Components-SDK

**This repository is public.** It is published at `github.com/smintio/Portals-Components-SDK` and is
the front door for Smint.io Solution Partners and Enterprise-plan customers writing their own
components. That governs everything written here — see "Writing for this repo" below before editing
a single file. It also governs this file: keep it free of internal repository names and paths.

Two things live here, and the second is now the larger:

| | |
|---|---|
| **Reference implementations** (~158 `.cs`) | `Examples/Backend/` — paired connectors and data adapters: `Connector-HelloWorld` + `DataAdapter-HelloWorld` as the minimal skeleton, plus `Picturepark` (live connection) and `SharePoint` (indexed through the integration layer) as realistic ones, each with a test driver. `Examples/Frontend/ui-example-hello-world-1` is the UI component starter. |
| **Partner-facing documentation** (~500 KB of Markdown) | `Overview/`, `Examples/Backend/README.md` + `Examples/Backend/docs/`, `Examples/Frontend/README.md` + `Examples/Frontend/docs/` (eight reference documents), and a README per tool under `Tools/`. |

`Tools/` ships three tools as release zips: `Portals-SDK-PublishComponent-CLI` (publish and register
a component), `Portals-DevServer` (serve local frontend component builds to a development portal),
and `Portals-DataAdapter-SDK-DataAdapterExporter-CLI` (generate TypeScript definitions from a data
adapter assembly — **win-x64 only**, unlike the other two).

- Consumes `SmintIo.Portals.ConnectorSDK`, `SmintIo.Portals.DataAdapterSDK`,
  `SmintIo.Portals.DataAdapterSDK.TestDriver`, `SmintIo.Portals.SDK.Core`, and the
  `Connector.Test` and `DataAdapter.Test` packages. All examples are net8.0.

Start from `HelloWorld` for the shape of a component, then read `Picturepark` or `SharePoint` for
how a real source system is handled. The components Smint.io ships in production are maintained
separately and are not part of this repository.

Because this is documentation-by-example, keep it building: an example that no longer compiles
against the current SDK is worse than no example at all. The examples are also *copied* by partners
as their starting point, so a bug in one propagates — treat example code with the care of published
source, not of a sketch.

## Writing for this repo

Everything here is read by people outside Smint.io. Before committing anything:

- **No customer or tenant names**, no internal repository or path references, no internal class
  names from the platform's own source, and no captured payloads from a real portal.
- **No credentials or configuration values** — in particular nothing out of an
  `appsettings.<Env>.json`, which hold OAuth secrets and npm authorization headers.
- **Document only what is supported.** Internal or undocumented parameters and behaviours do not
  belong in a partner-facing document even when they work.
- Explain a mechanism in terms of the SDK types a partner writes against, not in terms of how the
  platform implements it. "Smint.io prefixes entity keys per connector configuration, so never
  hard-code one" is right; naming the literal prefix and the class that applies it is not.

Conventions every document here follows — match them:

- Setext-style title (`=====` underline), then a `Current version of this document is: X.Y.Z (as of
  <date>)` line, and a `Contributors` block at the end. **Bump the version line when you change a
  document**, and make sure there is only one of it — `Examples/Backend/README.md` carried two
  contradictory stamps for years.
- A numbered link list as the table of contents, with `#user-content-`-prefixed anchors for
  in-document links (GitHub's rendering of README files inside a directory listing needs the
  prefix).
- Relative links, resolved from the file's own directory. This has been got wrong before: every
  link in `Examples/Backend/Connectors/README.md` was once written as if from `Examples/Backend/`,
  and an anchor in the SharePoint connector README pointed at a heading that lives two levels up.
  Check links after editing.

## The documentation here mirrors documentation kept internally

`Examples/Frontend/docs/smintio-*.md` are partner-facing counterparts of internal documents. Some
are byte-identical; others are the internal document with a version line, a Contributors block, and
internal phrasing rewritten. **A change to one side usually belongs on the other.**

Three pairs are deliberately kept in step and drift the fastest:

| Here | Internal counterpart |
|---|---|
| `Examples/Frontend/README.md` — "Before you start: the questions to answer" | `building-ui-components.md` §3.1, and the interview protocol in the UI component library's own CLAUDE.md |
| `Examples/Frontend/docs/smintio-data-adapter-reference.md` | `data-adapter-reference.md` |
| `Examples/Frontend/docs/smintio-page-type-contracts.md` | `page-type-contracts.md` |

Where the two disagree, the internal documents are the working detail and take precedence — but
that is a signal to fix the partner-facing copy, not to leave it wrong.

The backend material has internal authorities too: the connector SDK's and data adapter SDK's own
CLAUDE.md files describe the metamodel and the asset data model in full, and the SDK core types
under `Models/Metamodel/Data` are the last word on what an asset actually carries.
`Examples/Backend/docs/smintio-asset-data-model.md` is the partner-facing rendering of that
material. When the SDK changes, update it from the source types rather than from memory — and keep
it free of the internal detail the internal documents carry (wire encodings, the execute routes,
the platform's own resolver classes).

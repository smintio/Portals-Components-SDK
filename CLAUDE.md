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

**Reference implementations** — the worked examples to copy from when writing a new
component (~158 `.cs`).

`Examples/` holds paired connectors and data adapters: `Connector-HelloWorld` with
`DataAdapter-HelloWorld` as the minimal skeleton, plus `Picturepark` and `SharePoint` as
realistic ones. `Overview/`, `Tools/` and `README.md` carry the accompanying docs.

- Consumes `SmintIo.Portals.ConnectorSDK`, `SmintIo.Portals.DataAdapterSDK`,
  `SmintIo.Portals.DataAdapterSDK.TestDriver`, `SmintIo.Portals.SDK.Core`, and the
  `Connector.Test` and `DataAdapter.Test` packages.

Start from `HelloWorld` for the shape of a component, then read `Picturepark` or
`SharePoint` for how a real source system is handled. Production components live in
`Portals-SmintIo-Components`.

Because this is documentation-by-example, keep it building: an example that no longer
compiles against the current SDK is worse than no example at all.

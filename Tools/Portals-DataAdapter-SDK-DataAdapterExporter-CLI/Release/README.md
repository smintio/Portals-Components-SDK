The Portals-DataAdapter-SDK-DataAdapterExporter-CLI tool
========================================================

1. [Description](#description)
1. [Download](#download)
1. [Parameters](#parameters)
1. [Usage](#usage)

Current version of this document is: 1.0.0 (as of 11th of September, 2026)

## Description

A Smint.io Portals data adapter can publish **custom public API interfaces** of its own, on top of the
standard ones such as `IAssetsSearch`. A UI component can then declare a property typed as one of your
interfaces, and the portal wires the call through to your data adapter — the same way it does for the
standard interfaces.

For that to work on the frontend side, the UI component needs the interface as a TypeScript definition.
This tool produces it: point it at your compiled data adapter assembly, and it writes out the matching
TypeScript interface definitions, including the parameter and result types they refer to.

The concept, and a worked example of the generated output, is described under
[custom public API interfaces](../../../Examples/Backend/#user-content-custom-public-api-interfaces).

## Download

Download the ZIP archive to a convenient location and extract it.

*Please note that this tool is currently published for Windows (x64) only,* unlike the
[Portals-DevServer](../../Portals-DevServer/Release/) and the
[Portals-SDK-PublishComponent-CLI](../../Portals-SDK-PublishComponent-CLI/Release/), which also ship
Linux and macOS builds. If you need it on another platform, please get in touch with
[support@smint.io](mailto:support@smint.io).

## Parameters

| Parameter | Meaning |
|---|---|
| `-s` | the compiled data adapter assembly (DLL) to read the interfaces from |
| `-t` | the TypeScript file to write |

## Usage

```console
SmintIo.Portals.DataAdapterSDK.DataAdapterExporter.CLI.exe -s [Data-Adapter-Assembly-DLL] -t [Output-Filename]
```

For example:

```console
SmintIo.Portals.DataAdapterSDK.DataAdapterExporter.CLI.exe -s SmintIo.Portals.DataAdapter.Picturepark.MyCustomPictureparkInterfaces.dll -t .\IMyCustomPictureparkInterfaces.ts
```

Use the resulting `.ts` file directly in your UI component, or publish it as an npm package if several
components share it.

Run the tool again whenever you change the C# interfaces — the generated file is output, so edit the
interfaces and regenerate rather than editing the TypeScript by hand.

Please do not hesitate to contact us at [support@smint.io](mailto:support@smint.io) for any questions
regarding this tool.

Contributors
============

- Reinhard Holzner, Smint.io GmbH

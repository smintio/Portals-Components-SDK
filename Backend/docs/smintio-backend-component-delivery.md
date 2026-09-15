Building, testing and publishing a backend component
===================================================

Current version of this document is: 1.0.0 (as of 15th of September, 2026)

How a Smint.io Portals backend component gets from a C# project to something running in a
portal: the project setup, the test drivers that let you run a connector and a data adapter
without the platform, the shared test suite you inherit, and the publish and versioning rules.

1. [The project](#user-content-the-project)
1. [Getting the SDK packages](#user-content-getting-the-sdk-packages)
1. [The test driver — running a component outside the platform](#user-content-the-test-driver--running-a-component-outside-the-platform)
1. [The shared test suite](#user-content-the-shared-test-suite)
1. [Generating TypeScript for a custom interface](#user-content-generating-typescript-for-a-custom-interface)
1. [Publishing](#user-content-publishing)
1. [Versioning and upgrades](#user-content-versioning-and-upgrades)
1. [A checklist before you publish](#user-content-a-checklist-before-you-publish)
1. [Things that are easy to get wrong](#user-content-things-that-are-easy-to-get-wrong)

## The project

A backend component is an ordinary .NET class library. **Target `net8.0`.**

A connector project:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <AssemblyName>MyCompany.Portals.Connector.MyService</AssemblyName>
    <Version>1.0.0</Version>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="SmintIo.Portals.ConnectorSDK" Version="…" />
  </ItemGroup>
</Project>
```

A data adapter project references the data adapter SDK, plus the connector project it belongs
to — but **not at runtime**: the connector is already loaded by the platform, so the reference
is compile-time only.

```xml
<ItemGroup>
  <PackageReference Include="SmintIo.Portals.DataAdapterSDK" Version="…" />
  <ProjectReference Include="..\..\Connectors\Connector-MyService\Connector-MyService.csproj"
                    Private="false" ExcludeAssets="runtime" />
</ItemGroup>
```

`Private="false"` and `ExcludeAssets="runtime"` are what keep the connector assembly out of the
data adapter's own output. Copy them.

`AssemblyName` and `Version` are read straight out of the project file when you publish, so keep
them accurate. The `AssemblyName` identifies the component for the rest of its life.

A test driver project references the data adapter project and the three test packages — see
below.

Keep all of your projects in one solution. The examples in this repository are laid out as
`Connectors/Connector-<Name>`, `DataAdapters/DataAdapter-<Name>` and
`DataAdapters/Portals-<Name>-TestDriver` under one solution file, and that layout is worth
copying.

## Getting the SDK packages

The SDK packages are **not public**. Access is granted to Smint.io Solution Partners and to
Smint.io Portals Enterprise plan customers — get in touch at
[support@smint.io](mailto:support@smint.io) to request it. You will need an account with
Microsoft Visual Studio cloud offerings (Azure DevOps), as the packages are hosted there.

| Package | For |
|---|---|
| `SmintIo.Portals.ConnectorSDK` | a connector |
| `SmintIo.Portals.DataAdapterSDK` | a data adapter |
| `SmintIo.Portals.SDK.Core` | pulled in by both — the annotations, the data objects, the meta-model |
| `SmintIo.Portals.ConnectorSDK.TestDriver`, `SmintIo.Portals.DataAdapterSDK.TestDriver` | running a component outside the platform |
| `SmintIo.Portals.Connector.Test`, `SmintIo.Portals.DataAdapter.Test` | the shared test suite |

Once you have access, add a `NuGet.Config` next to your solution pointing at the feed. Two of
the example projects in this repository carry one; there are no credentials in it — those belong
in your user-level NuGet configuration.

**Keep the SDK versions aligned across your projects.** A data adapter built against a different
SDK version than its connector is the kind of problem that only shows up at load time.

## The test driver — running a component outside the platform

The test drivers construct your connector and data adapter with in-memory stand-ins for
everything the platform normally provides — cache, persistent storage, HTTP client factory,
logging, portals context — so you can exercise the real component against the real external
system with no server, no database and no portal.

This is the development loop for a backend component. Use it.

```C#
// 1. the connector
var connectorTestDriver = new SetupTestDriver();

await connectorTestDriver.InstantiateConnectorAsync(
    typeof(MyConnectorStartup),
    new MyConnectorConfiguration
    {
        ServiceUrl   = ConfigurationOptions.ServiceUrl,
        ClientId     = ConfigurationOptions.ClientId,
        ClientSecret = ConfigurationOptions.ClientSecret
    });

var connector = (MyConnector)connectorTestDriver.Connector;
var metamodel = connectorTestDriver.ConnectorMetamodel;

// 2. the data adapter, on top of it
var dataAdapterTestDriver = new DataAdapterTestDriver(connector, metamodel);

await dataAdapterTestDriver.InstantiateDataAdapterAsync(
    typeof(MyAssetsDataAdapterStartup),
    new MyAssetsDataAdapterConfiguration { … });

var dataAdapter = (MyAssetsDataAdapter)dataAdapterTestDriver.DataAdapter;
```

Pick the connector test driver that matches your setup method:

| Test driver | For |
|---|---|
| `SetupTestDriver` | `ConnectorSetupMethod.Setup` — credentials from the configuration |
| `OAuth2ConnectorTestDriver` | an OAuth2 connector, constructed with the redirect URI and optionally a refresh token |
| `OAuth2AuthenticationCodeFlowWithPKCETestDriver` | the PKCE flow, same constructor |

Each runs the real lifecycle — `PerformPostConfigurationChecksAsync`, the authorization step,
`PerformPostAuthorizationChecksAsync`, `GetConnectorMetamodelAsync` — so a connector that fails
in the test driver would have failed in the platform.

Two things to know about the redirect flows. The driver opens a **real browser** for the consent
screen, which works most reliably with Google Chrome, and it listens on the redirect URI you
pass — so that URI has to be registered with the external system as an allowed redirect. Pass a
refresh token on later runs to skip the browser step.

`DataAdapterTestDriver` additionally implements `IEntityModelProvider`, which is what lets your
converter resolve meta-model entities without the platform. It is deliberately partial: methods
outside what a data adapter needs throw. That is a harness, not a simulator.

The test driver does **not** validate that your component would be accepted for publishing, and
it does not exercise a portal. Use it to get the integration right, then publish to a
development environment and look at a real portal.

## The shared test suite

Do not write the basic tests yourself. `SmintIo.Portals.Connector.Test` and
`SmintIo.Portals.DataAdapter.Test` ship abstract xUnit test classes that already assert what a
correct component does; you inherit one and supply the sample data.

| Base class | Asserts |
|---|---|
| `ConnectorStartupTests` | the startup is complete and consistent |
| `ConnectorTests` | the connector's lifecycle behaves |
| `OAuth2ConnectorTests` | the OAuth2 flow behaves |
| `ConnectorMetamodelTests` | the meta-model is well formed |
| `DataAdapterStartupTests` | the data adapter startup is complete |
| `DataAdapterAssetsTests` and its `…ReadTests`, `…SearchTests`, `…ReadRandomTests`, `…SharedTests` parts | asset read, search and random access behave as the platform expects |
| `DataAdapterAssetsLiveConnectionTests` | the same against a live connection |
| `DataAdapterIntegrationLayerTests`, `DataAdapterIntegrationLayerStartupTests`, `DataAdapterAssetsIntegrationLayerTests` | an indexed adapter |

The harness is an xUnit fixture deriving from
`BaseDataAdapterFixture<TConfigurationOptions, TConnector, TDataAdapterConfiguration, TDataAdapter>`,
which reads an `appsettings.json` next to the test assembly, binds your configuration options
out of it, and exposes `Connector`, `Metamodel`, `DataAdapter`, `DataAdapterConfiguration`,
`ServiceProvider` and `AssetOptions`.

```C#
public class MyServiceFixture : BaseDataAdapterFixture<
    MyServiceOptions, MyConnector, MyAssetsDataAdapterConfiguration, MyAssetsDataAdapter>
{
    protected override void BindSections(IConfiguration configuration)
    {
        base.BindSections(configuration);
        configuration.GetSection(MyServiceOptions.Name).Bind(ConfigurationOptions);
    }

    protected override async Task CreateConnectorAsync() { … }   // as above
    public override async Task InitializeAsync() { … }           // connector, then data adapter
}

[CollectionDefinition(nameof(MyServiceFixtureCollection))]
public class MyServiceFixtureCollection : ICollectionFixture<MyServiceFixture> { }

[Collection(nameof(MyServiceFixtureCollection))]
public class MyServiceDataAdapterAssetsTests : DataAdapterAssetsTests
{
    private readonly MyServiceFixture _fixture;

    public MyServiceDataAdapterAssetsTests(MyServiceFixture fixture) => _fixture = fixture;

    protected override bool SupportAssetVersions => false;
    protected override string SampleFormGroupDefinitionId => _fixture.AssetOptions.FormGroupDefinitionId;
    // … the rest of the sample data the suite needs
}
```

The collection fixture is what makes the connector bootstrap once per run rather than once per
test class — which matters when bootstrapping means an OAuth handshake.

Put the sample identifiers, names and query strings in `appsettings.json` rather than in the
test code, so that the same suite can be pointed at a different tenant. Structure the test
project as `Harness/` and `Integration/`, as all three examples in this repository do.

**Do not commit real credentials.** Use user secrets or environment variables for anything
sensitive, and keep the committed `appsettings.json` to placeholders — the Hello World test
driver's is a good model, because the Hello World connector is backed by artificial data and
needs no real credentials at all. That is also what makes it the one example whose tests run
anywhere, immediately.

## Generating TypeScript for a custom interface

If your data adapter publishes a [custom public API
interface](smintio-data-adapter-interfaces.md#user-content-custom-public-api-interfaces) and a UI
component has to call it, generate the TypeScript definition from the built assembly with the
[Data Adapter Exporter CLI](../../Tools/Portals-DataAdapter-SDK-DataAdapterExporter-CLI/Release/):

```console
SmintIo.Portals.DataAdapterSDK.DataAdapterExporter.CLI.exe -s MyDataAdapter.dll -t .\IMyProductData.ts
```

The tool is **win-x64 only**. Re-run it whenever the interface changes; the output is generated
and never hand-edited. Publish it as an npm package if more than one component uses it.

## Publishing

The [Portals-SDK-PublishComponent-CLI](../../Tools/Portals-SDK-PublishComponent-CLI/Release/)
compiles, packages and deploys a backend component. Its README carries the full setup — the
`SMINT_IO_SDK_HOME` environment variable, the per-environment `appsettings` files, and
registering it as a .NET global tool.

From the component folder:

```console
%SMINT_IO_SDK_HOME%\SmintIo.Portals.SDK.PublishComponent.CLI.exe -env development
```

or, with the tool installed globally:

```console
smint-io-pc -env development
```

It finds the `*.csproj`, restores, builds, packages the build output and uploads it. The
`Development` environment builds Debug; every other environment builds Release.

Three things follow from *how* it packages:

- **The package contains the whole build output**, your project references and external package
  references included. A dependency you added is shipped with your component.
- **Which environment and which Smint.io instance you publish to is decided entirely by the
  `appsettings.<Env>.json` you point the tool at** — not by anything in your code. Confirm it
  before you publish, every time. Those files carry OAuth credentials: never commit them and
  never quote values out of them.
- Publishing authorizes you interactively through a browser, which by default listens on port
  `43450` on your machine. If that port is unavailable you can change the redirect URL in the
  settings, but Smint.io has to whitelist the new one first — get in touch.

**The component key and the first registration come from Smint.io.** Component keys are globally
unique and issued, much as port numbers are, and a brand new backend component has to be
enabled on the Smint.io side before the CLI will accept its first upload. Publishing new
*versions* of a component that has been onboarded is then self-service. Get in touch at
[support@smint.io](mailto:support@smint.io) when you start a new component, not when you are
ready to ship it.

Once uploaded, the component is loaded by the running Smint.io Portals instances without a
platform deployment. There is nothing to schedule and nothing to restart.

## Versioning and upgrades

The `<Version>` in your project file is the component's version. The publish is rejected if that
version has already been published for that assembly name — so **bump it for every publish**, not
only for every functional change.

On upgrade, the new version is loaded alongside the old one and the platform switches over, then
releases the old one. Two consequences:

- **Both versions exist briefly.** Anything your component holds outside itself — a cache entry,
  a file, a row — may be seen by both. Version such things or make them tolerant.
- **There is no downgrade.** Publish a higher version with the fix rather than trying to go back.

Use ordinary semantic versioning and treat these as breaking, because they are:

- renaming or removing a configuration property — it orphans every saved configuration;
- changing the component `Key`;
- removing a public API interface from `PublicApiInterfaces`, or a method from a custom
  interface;
- changing what a meta-model entity key means.

Adding a configuration property, adding a method to a custom interface, and adding an interface
to `PublicApiInterfaces` are all safe.

**A connector meta-model is a snapshot taken when the connector configuration is set up.** A new
version that changes the meta-model does not change anything for existing configurations until
they are set up again. Say so when you hand over the release.

## A checklist before you publish

- [ ] the component `Key` is the one Smint.io issued, and unchanged from the released version
- [ ] `<Version>` bumped
- [ ] `TargetFramework` is `net8.0`, and the SDK package versions match across your projects
- [ ] the data adapter's project reference to the connector carries `Private="false"` and
      `ExcludeAssets="runtime"`
- [ ] `ConfigurationMessages` is declared and every configuration property has a `DisplayName`
      with exactly one default culture
- [ ] `MetamodelMessages` is declared if you build a meta-model with translatable labels
- [ ] `PublicApiInterfaces` lists every interface you want reachable — and nothing you have not
      implemented
- [ ] both feature-support methods answer truthfully, and unsupported methods throw
      `NotImplementedException`
- [ ] the meta-model identifier varies with the configuration
- [ ] the test driver runs green against the real external system
- [ ] the inherited shared test suite runs green
- [ ] no credentials in anything you are committing, and nothing read out of an `appsettings`
      file
- [ ] you know which environment and which Smint.io instance the `appsettings.<Env>.json` you
      are about to use points at

## Things that are easy to get wrong

- **Publishing to the wrong environment.** Nothing in your code decides it. Read the `-env`
  argument back before you press enter.
- **Forgetting to bump the version.** The publish is rejected on the version alone, after the
  build has run.
- **Committing an `appsettings.<Env>.json`.** They hold OAuth credentials.
- **Shipping a dependency you did not mean to.** The package is the whole build output.
- **Expecting a meta-model change to reach existing configurations.** It does not, until they
  are set up again.
- **Treating a configuration property rename as cosmetic.** It is a breaking change, silently.
- **Skipping the shared test suite.** It asserts things about a component that are easy to get
  wrong and hard to notice, and inheriting it costs a class.

## Questions

Please do not hesitate to contact us at [support@smint.io](mailto:support@smint.io) if you run
into any issues.

Contributors
============

- Reinhard Holzner, Smint.io GmbH

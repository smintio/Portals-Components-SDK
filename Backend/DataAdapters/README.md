How-to implement the `DataAdapter`
==================================
 
Current version of this document is: 1.4.0 (as of 15th of September, 2026)

This is the short orientation. **The full catalogue of public API interfaces, the base classes
and what they leave abstract, parameters and results, long-running methods, permissions, the
configuration marker interfaces and how to publish an interface of your own are in
[the data adapter public API interfaces](../docs/smintio-data-adapter-interfaces.md).**

## `DataAdapter` basics

The `DataAdapter` can be understood as a _facade_ for the external system. It is used to read, search (and potentially write) data from (and to) an external system.
It uses the `Connector` to establish a connection to the external system such as Sharepoint. The connecting point between those two is the `ConfigureServicesForDataAdapter` method:
```c#
//in SharepointConnector.cs
public override void ConfigureServicesForDataAdapter(ServiceCollection services)
{
    services.AddTransient(_ => CreateSharepointClient()); //returns an ISharepointClient
}
```
this enables the `DataAdapter` to inject the `ISharepointClient`, which is a wrapper around the Microsoft Graph API.

**That client is how the data adapter calls the external system, and it must expose no secrets** —
no token, no key, no prepared authorization header. The rules, and why they matter, are in
[the client must not expose secrets](../docs/smintio-connector-reference.md#user-content-the-client-must-not-expose-secrets).

## Compartmentalized features
There are several interfaces, each of which provides the front end with a set of method calls. In most cases you'll need `IAssetsRead` and `IAssetsSearch`.

Even though using `partial` classes is not required to separate the different implementations, it is a good way to structure your implementation. You could then have one `partial class`
for every interface and separate the implementation concerns accordingly.

Note that these interfaces inherit from one another, so implementing one brings in the methods of another: `IAssetsSearch`
extends `IAssetsRead`, which in turn extends `IAssetsInternalApiProvider` — the interface that declares
`GetAssetsDownloadItemMappingsAsync`. A method that arrives by more than one route is of course only implemented once.
It is, however, still advisable to declare every interface you support in the `DataAdapter`'s signature, because the
component framework discovers them by reflection.

Which methods you have to fill, what the objects you return have to look like, and which of them you may leave out, is
described in [the asset data model](../docs/smintio-asset-data-model.md).

Deriving from `AssetsDataAdapterBaseImpl` gives you the whole of `IAssets` with eleven members
left abstract — that list is the work. Everything else the base class supplies is `virtual`, so
override the smallest thing that expresses your difference rather than reimplementing a method
wholesale.

All of this — the standard interfaces, the asset data model, the integration modes below — applies
to a **productized** data adapter, one whose consumer is the standard portal experience. A
**custom** data adapter publishes its own interfaces for one custom UI component to call, and
needs none of it: the interface is the data model, and there is no meta-model, no integration
mode and no `AssetDataObject` mapping. See
[productized or custom](../README.md#user-content-productized-or-custom) before you start.

Smint.io offers two different integration modes — a **live connection**, where every request goes
to the external system, and an **internal index**, where Smint.io analyzes and indexes the content
and serves searches from its own index. Which one is right depends on what the external system can
do, and the choice shows up in the marker interfaces your configuration class implements. It is
described in
[live connection or internal index](../README.md#user-content-live-connection-or-internal-index).

Contributors
============

- Reinhard Holzner, Smint.io GmbH
- Yosif Velev, Smint.io GmbH
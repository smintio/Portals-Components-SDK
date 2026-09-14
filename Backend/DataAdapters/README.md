How-to implement the `DataAdapter`
==================================
 
Current version of this document is: 1.1.0 (as of 11th of September, 2026)

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

Smint.io offers two different integration modes. Based on the functionality supported by the external system.

- Live connection

The data is fetched on demand. This will mean that the external system must support feature-rich functionality (e.g. fully translatable metamodel, faceted search and etc)

- Internal index

Selected data is analyzed and metadata is captured. Thumbnails, video, audio and document renditions are generated and stored by Smint.io  for offline usage.
Further data synchronization is required via tokens, webhooks or time based intervals.
Please note that Smint.io does not store original assets.

Contributors
============

- Reinhard Holzner, Smint.io GmbH
- Yosif Velev, Smint.io GmbH
How-to implement the `DataAdapter`
==================================
 
Current version of this document is: 1.0.0 (as of 2th of March, 2023)

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
There are several interfaces, each of which provides the front end with a set of method calls. In most cases you'll ned `IAssetsRead`, `IAssetsSearch` and `IAssetsInternalApiProvider`.

Even though using `partial` classes is not required to separate the different implementations, it is a good way to structure your implementation. You could then have one `partial class`
for every interface and separate the implementation concerns accordingly.

Note: some of the interfaces have overlapping signatures, for example both the `IAssetsRead` and `IAssetsInternalProvider` both define the `GetAssetsDownloadItemMappingsAsync` method, 
and of course it only needs to be implemented once. It is, however, still advisable to declare all interfaces in the `DataAdapter`'s signature due to Reflection reasons.

Contributors
============

- Reinhard Holzner, Smint.io GmbH
- Yosif Velev, Smint.io GmbH
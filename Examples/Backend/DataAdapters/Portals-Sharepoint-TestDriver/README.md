# How-to implement the `DataAdapter`

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

This injects an instance of the `ISharepointClient` to the data adapter. The `ISharepointClient` is a wrapper around the Microsoft Graph API. 

*Please note that only the connector must have access to access tokens and the like. The data adapter should not have any methods to access credentials or security relevant information!*

## Compartmentalized features

There are several interfaces, each of which provides the front end with a set of method calls. In most cases you'll just need `IAssets` (which in turn combines several other interfaces
like `IAssetsRead`, `IAssetsSearch` and `IAssetsInternalApiProvider`).

The more generic the interface is, the more user interface components will be able to use it. Imagine a user interface component that requires the full `IAssets` functionality set. It can consume any 
data adapter that implements `IAssets`, and interfaces deriving from `IAssets`. If you would just implement `IAssetsRead`, the user interface component cannot consume your
service as you are not providing all required functionality.

You can also define your own interfaces, if this is required. More documentation on this will follow. Please get in touch for more info if you want to leverage that functionality.

Data adapter interfaces can be subject to permissions, even your custom ones. More documentation on this will follow. Please get in touch for more info if you want to leverage that functionality.

Documentation on task handlers and state machines will follow. Please get in touch if you want to leverage that functionality.

## Code style

Even though using `partial` classes is not required to separate the different implementations, it is a good way to structure your implementation. You could then have one `partial class`
for every interface and separate the implementation concerns accordingly.


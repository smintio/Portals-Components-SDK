Developing Smint.io Portals backend components
==============================================

This repository contains examples for Smint.io Portals backend components, which is connectors, data adapters, task handlers, portal templates and identity providers.

Documentation on portal templates, task handlers and identity providers will follow. Please get in touch if you want to leverage that functionality.

Please note that at any time you can build your own connector or data adapter components based
on our *Smint.io Portals SDKs*. Access to the SDKs is restricted. Get in contact with [Smint.io](https://www.smint.io)
and request access. Access will be granted to either Smint.io Solution Partners or to all our Smint.io Portals
Enterprise plan customers.

You will need an account with Microsoft Visual Studio cloud offerings (Azure DevOps), as the SDKs are hosted there.

1. [Examples](#examples)
1. [Connector description & flow](#user-content-connector-description--flow)
1. [Data adapter public API interfaces](#user-content-data-adapter-public-api-interfaces)
1. [Custom public API interfaces](#user-content-custom-public-api-interfaces)
1. [Overview of Smint.io annotations](../Frontend/docs/smintio-annotations.md)

Current version of this document is: 2.2.2 (as of 8th of March, 2023)

## Examples

#### Hello World Connector

- [Root directory](Connectors/Connector-HelloWorld/)

#### Hello World Data Adapter

This data adapter implements the *IAssets* data adapter interface.

- [Root directory](DataAdapters/DataAdapter-HelloWorld/)
- [Test driver README.md](DataAdapters/Portals-HelloWorld-TestDriver)

#### Microsoft SharePoint Connector

- [Root directory](Connectors/Connector-SharePoint/)
- [Connector README.md](Connectors/Connector-SharePoint/README.md)

#### Microsoft SharePoint Data Adapter

This data adapter implements the *IAssets* data adapter interface.

- [Root directory](DataAdapters/DataAdapter-SharePoint/)
- [Data adapter README.md](DataAdapters/DataAdapter-SharePoint/README.md)
- [Test driver README.md](DataAdapters/Portals-Sharepoint-TestDriver/README.md)

#### Picturepark Connector

- [Root directory](Connectors/Connector-Picturepark/)

#### Picturepark Data Adapter

- [Root directory](DataAdapters/DataAdapter-Picturepark/)
- [Test driver](DataAdapters/Portals-Picturepark-TestDriver/)

## Connector description & flow

The purpose of a connector is to provide a way to communicate with an external provider via a client, expose user interface configurations, authentication processing, verify settings and build a translatable meta-model.

Each connector is system dedicated. See [SharePoint](Connectors/Connector-SharePoint/) or [Picturepark](Connectors/Connector-Picturepark/). 

Please note that in the case of `Picturepark`, the connector uses a live connection (data is requested on the fly).

For `SharePoint` this is done using our own index. We call it the integration layer.

When developing a new connector, [HelloWorld](Connectors/Connector-HelloWorld/) serves as a good starting point.

The `SmintIo.Portals.ConnectorSDK` NuGet Smint.io package makes it possible to build a connector.

`IConnectorStartup` is the main interface to be implemented, so that at a later stage Smint.io knows how to use the newly built connector.

This interface describes the process of starting the connector, includes information such as its name, description, logo/icon, setup, documentation and more.
Points to configuration settings used by the user interface and which component is responsible for bootstrapping the connector itself.

When implementing `IComponentConfiguration`, the added properties must be the minimum settings for the connector configuration.
```C#
[DisplayName("en", "Your Picturepark URL", IsDefault = true)]
[DisplayName("de", "Ihre Picturepark URL")]
...
[Required]
[IsUri(EnforceHttps = true, RemovePathAndQueryString = true)]
public string PictureparkUrl { get; set; }

[DisplayName("en", "Your access token", IsDefault = true)]
[DisplayName("de", "Ihr Zugriffs-Token")]
[MaxLength(100)]
[Required]
public string AccessToken { get; set; }
```
Smint.io Portals understands the definition of annotations that allow to change the look and feel of the user interface.
Please note that multiple cultures are supported.

The full list of supported Smint.io Portals annotations can be found [here](../Frontend/docs/smintio-annotations.md).

The `IConnector` implementation should specify the authentication mechanism. Smint.io supports `OAuth2Connector` and `OAuth2AuthenticationCodeFlowWithPKCEConnector`, which should serve as a starting point in the form of a base class.
However, it is also possible to implement your own authentication mechanism at any time.

Several things need to be done depending on the authentication flow for instantiating a client.

### In case of connector setup method - `Setup`
#### Perform post configuration checks
Set the necessary configuration values to the authorizationValuesModel
```C#
public override async Task PerformPostConfigurationChecksAsync(AuthorizationValuesModel authorizationValuesModel)
{
    ...
    authorizationValuesModel.KeyValueStore.Remove(ApiUrlKey);
    authorizationValuesModel.KeyValueStore.Add(ApiUrlKey, apiUrl);

    authorizationValuesModel.IdentityServerUrl = identityServerUrl;
    ...
}
```
#### Initialize authorization
Sets authorization value. 
Please note that network call is possible as well.
```C#
public override Task<AuthorizationValuesModel> InitializeAuthorizationValuesAsync(string originalSecret, string secret, AuthorizationValuesModel bootstrapAuthorizationValuesModel)
{
    bootstrapAuthorizationValuesModel.AccessToken = _configuration.AccessToken;

    return Task.FromResult(bootstrapAuthorizationValuesModel);
}
```
#### Post authorization check
Confirms whether we can communicate successfully with the third party.
```C#
public override async Task PerformPostAuthorizationChecksAsync(FormFieldValuesModel formFieldValuesModel)
{
    var channelFormFieldValueModel = formFieldValuesModel.Values.FirstOrDefault(FormFieldValueModel => string.Equals(FormFieldValueModel.Id, nameof(PictureparkConnectorConfiguration.Channel)));

    string configuredChannelId = null;

    if (channelFormFieldValueModel != null)
    {
        configuredChannelId = channelFormFieldValueModel.StringValue;
    }            
    ...
	    
    if (!string.IsNullOrEmpty(configuredChannelId))
    {
	var channelId = await ValidateChannelIdAsync(configuredChannelId).ConfigureAwait(false);

	if (!string.Equals(channelId, configuredChannelId))
	{
	    channelFormFieldValueModel.StringValue = channelId;
	}
     }	
     ...
}
```
#### Refresh authorization
Time-controlled self refreshing authorization.
```C#
public override async Task<AuthorizationValuesModel> RefreshAuthorizationValuesAsync(AuthorizationValuesModel authorizationValuesModel)
{
	authorizationValuesModel.AccessToken = _configuration.AccessToken;
	...
	...
	return authorizationValuesModel;
}
```
#### Wire-up dependencies
```C#
public override void ConfigureServicesForDataAdapter(ServiceCollection services)
{
    services.AddSingleton((serviceProvider) =>
    {
        return CreatePictureparkClient();
    });
}
```
#### Build meta-model
```C#
public override async Task<ConnectorMetamodel> GetConnectorMetamodelAsync()
{
    var pictureparkClient = CreatePictureparkClient();

    var pictureparkMetamodelBuilder = new PictureparkMetamodelBuilder(pictureparkClient);

    var pictureparkMetamodel = await pictureparkMetamodelBuilder.BuildAsync();

    return pictureparkMetamodel;
}
```
### In case of connector setup method - `Redirect`
A similar flow to `Setup` with an additional step after `PerformPostConfigurationChecksAsync()`.
##### Compute redirect url
```C#
public override Task<string> GetRedirectUrlAsync(string targetRedirectUri, string secret, AuthorizationValuesModel authorizationValuesModel, CultureInfo currentCulture)
{
    ...
    var authorizeEndpoint = $"{idSrvUrl}/{encodedTenant}/oauth2/v2.0/authorize?client_id={encodedClientId}&response_type=code&redirect_uri={encodedRedirectUri}&response_mode=query&scope={encodedScopes}&state={HttpUtility.UrlEncode(secret)}";

    authorizationValuesModel.OriginalRedirectUrl = targetRedirectUri;

    return Task.FromResult(authorizeEndpoint);
}
```
***Between steps, the connector can create a provider's client that will later be used by the `Data Adapter` instance.***

### Provider's client
Acts as a facade in front of provider's concrete SDK as a means of communication.

Smint.io has a set of retry strategy helpers that live under SmintIo.Portals.ConnectorSDK.Clients.Prefab

A client can inherit from `BaseHttpClientApiClient`, `BaseDynamicApiClient<TDynamicApiClient>` or `BaseRestSharpApiClient`

See `SharepointClient.cs` for reference.

### Meta-model
Generally speaking the meta-model describes what types of *objects* exist in the external provider and what properties they have. In other words: the meta-model describes the _custom data_ that is delivered.

This meta-model is then used throughout Smint.io Portals to interpret the external metadata delivered by the external provider (e.g. also custom metadata).

Each type of object is represented by one `EntityModel`. 
As a resulting there is only one `EntityModel`. Columns (i.e. custom fields) affect all files equally. For example, if we were to add a custom choice field "Mood", that indicates the mood prevalent in an image, it would also be possible for `*.docx` file to have a "Mood" field. 

The `EntityModel` acts as a schema definition that allows the user interface to know in what format the data will came from the data adapter. 

- [Example](Connectors/Connector-SharePoint/README.md#meta-model-structure)

Naturally EntityModel is completely translatable, as are its properties.

## Data adapter public API interfaces

Each Smint.io Portals backend or UI component can tie itself to public API interfaces published by Smint.io Portals data adapters.

This can be done by requesting a data adapter public API interface through the configuration of the Smint.io Portals backend or 
UI component:

#### .NET example (for Smint.io Portals backend components)
```C#
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets;

...

[DisplayName("en", "Data source for auto completion", IsDefault = true)]
[DisplayName("de", "Daten-Quelle für die Auto-Vervollständigung")]
[Description("en", "The data source to query the search bar auto completion suggestions from.", IsDefault = true)]
[Description("de", "Die Daten-Quelle, aus der die Vorschläge für die Auto-Vervollständigung für die Such-Eingabeleiste geladen werden.")]
[FormGroup("s-search-bar")]
public IAssetsSearch SearchBarAutoCompletion { get; set; }

```
Once the Smint.io Portals UI component is instanciated, you can easily call methods of that public API interface.
```C#
var searchAssetsResult = await _portalsContext.PublicApiInterfaceExecutionWrapper
	.WrapPublicApiInterfaceExecutionAsync<SearchAssetsParameters, SearchAssetsResult, IAssetsSearch>(
		_configuration.SearchBarAutoCompletion, 
		nameof(IAssetsSearch.SearchAssetsAsync), 
		parameters)
	.ConfigureAwait(false);
```

*Side note: you could also invoke the method of the targets public API interface directly. However, we ask you to use this way of calling other public API interfaces, as
this method performs permission checks, script executions and other potentially required operations. In the future we will introduce some facet-based approach to avoid
this issue.*

### Typescript example (for Smint.io Portals UI components)
```typescript
import type {
    IAssetsSearch,
} from "@smintio/portals-component-sdk";

...

@DisplayName("en", "Data source for auto completion", true)
@DisplayName("de", "Daten-Quelle für die Auto-Vervollständigung")
@Description("en", "The data source to query the search bar auto completion suggestions from.", true)
@Description("de", "Die Daten-Quelle, aus der die Vorschläge für die Auto-Vervollständigung für die Such-Eingabeleiste geladen werden.")
@Implements("IAssetsSearch")
@ComponentProperty({ name: "searchBarAutoCompletion" })
@FormGroup("s-search-bar")
public readonly searchBarAutoCompletion!: IAssetsSearch;
```
Once the Smint.io Portals UI component is instanciated, you can easily call methods of that public API interface.
```typescript
this.searchBarAutoCompletion.getFullTextSearchProposalsAsync({ queryString: this.searchQuery })
	.catch((e) => {
		...
	})
	.then((searchProposals?: IGetFullTextSearchProposalsResult) => {
		...
	})
	.finally(() => {
		...
	});
```
*All the wiring from frontend to backend is done for you, without any further work involved.*

## Custom public API interfaces

In the above example, the `IAssetsSearch` is a standard public API interface provided by Smint.io Portals. 

*The great thing is*: if you develop your own Smint.io Portals data adapter, you can easily publish your own custom 
public API interfaces as well. This enables you to easily develop any custom functionality required using the Smint.io 
Portals component framework and runtime.

However, for use of your custom public API interfaces in a Smint.io Portals UI component you'll need its Typescript 
public API interface definition.

Use the [Smint.io Portals Data Adapter Exporter CLI tool](https://github.com/smintio/Portals-Components-SDK/tree/main/Tools/Portals-DataAdapter-SDK-DataAdapterExporter-CLI/Release) 
to generate the Typescript public API interface definition directly from your Smint.io Portals data adapter assembly. 

You can then simple use that Typescript public API interface definition file in your Smint.io Portals UI component.

Invoke the tool as follows:
```console
SmintIo.Portals.DataAdapterSDK.DataAdapterExporter.CLI.exe -s [Data-Adapter-Assembly-DLL] -t [Output-Filename]
```
This is an example on how to invoke the tool:
```console
SmintIo.Portals.DataAdapterSDK.DataAdapterExporter.CLI.exe -s SmintIo.Portals.DataAdapter.Picturepark.MyCustomPictureparkInterfaces.dll -t .\IMyCustomPictureparkInterfaces.ts
```
This is how the result might look like:
```typescript
import type { IDataAdapterInterface } from '@smintio/portals-component-sdk';
import type { IAssetIdsParameters } from '@smintio/portals-component-sdk';
import type { IDataAdapterParameterObject } from '@smintio/portals-component-sdk';
import type { IAssetIdentifier } from '@smintio/portals-component-sdk';
import type { IDataAdapterResult } from '@smintio/portals-component-sdk';

export interface IMyCustomPictureparkInterfaces extends IDataAdapterInterface
{
    markAssetsAsDeletedAsync(parameters: IMarkAssetsAsDeletedParameters) : Promise<IMarkAssetsAsDeletedResult>;
}
export interface IMarkAssetsAsDeletedParameters extends IDataAdapterParameterObject, IAssetIdsParameters
{
}
export interface IMarkAssetsAsDeletedResult extends IDataAdapterResult
{
}
```
*You see that this is just a Typescript interface definition file (in this case even a very simple one) that you can directly use in your Smint.io Portals UI component.
You may also publish the interface as a NPM package for further comfort.*

TODOs
=====

- Documentation for portal templates
- Documentation for task handlers
- Documentation for identity providers

Contributors
============

- Reinhard Holzner, Smint.io GmbH
- Yosif Velev, Smint.io GmbH

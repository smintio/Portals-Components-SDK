The Smint.io Portals connector contract
=======================================

Current version of this document is: 1.3.0 (as of 15th of September, 2026)

Every member a connector declares, the order in which the platform calls them, the
authentication flows you can start from, and how the API client underneath is built.

A connector has exactly two jobs: **establish and maintain a trust context** to the external
system, and **describe that system's schema**. It does not read assets — that is the data
adapter's job, and the two are separate components for a reason: several data adapters can share
one connector, and one connector's credentials.

The second job applies only to a **productized** connector. A **custom** connector — one whose
data adapter publishes its own interfaces for one custom UI component to consume — does not need
a meta-model at all, because nothing generic ever has to interpret its payload. Settle which of
the two you are building before you start; see
[productized or custom](../README.md#user-content-productized-or-custom).

Its companion documents: [the connector meta-model](smintio-connector-metamodel.md) for the
second job in full, [the data adapter public API interfaces](smintio-data-adapter-interfaces.md)
for what sits on top, and [the annotations](smintio-backend-annotations.md) for the configuration
form.

1. [The two classes you write](#user-content-the-two-classes-you-write)
1. [`IConnectorStartup` — every member](#user-content-iconnectorstartup--every-member)
1. [Setup methods](#user-content-setup-methods)
1. [`IConnector` — every member](#user-content-iconnector--every-member)
1. [The call order](#user-content-the-call-order)
1. [Choosing an authentication flow](#user-content-choosing-an-authentication-flow)
1. [The API client](#user-content-the-api-client)
1. [Handing the client to the data adapter](#user-content-handing-the-client-to-the-data-adapter)
1. [The meta-model identifier](#user-content-the-meta-model-identifier)
1. [Project layout](#user-content-project-layout)
1. [Things that are easy to get wrong](#user-content-things-that-are-easy-to-get-wrong)

## The two classes you write

| Class | Lifetime | What it is |
|---|---|---|
| your `IConnectorStartup` | a long-lived singleton | stateless metadata: the key, the names, the icons, which configuration class to use, which component class to instantiate |
| your `IConnector` | short-lived, per use | the stateful thing: holds the authorization values, talks to the external system, builds the meta-model |

Plus a third, your `IComponentConfiguration` implementation, which is a plain settings class —
see [the annotations](smintio-backend-annotations.md).

**Put into it only what the connector needs.** The credentials, the service endpoint, and the
tenant, channel or site that decides which content the meta-model describes — those are the
connector's. A setting only one data adapter interprets goes into *that adapter's* configuration
class, because a data adapter has one too: several adapters can share one connector, so a
connector property forces the same value on all of them. Customer-specific settings and an
adapter's own secret, such as a link signing key, belong on the data adapter. All component
configurations are stored encrypted at rest. See
[which component's configuration a setting belongs in](smintio-backend-annotations.md#user-content-which-components-configuration-a-setting-belongs-in).

The split exists because the platform publishes connector methods by reflection. Everything that
is *management* rather than *operation* is on the startup, so the platform can read it without
constructing anything.

## `IConnectorStartup` — every member

`IConnectorStartup : IComponentStartup`. The inherited half first:

| Member | Set it to |
|---|---|
| `string Key` | your connector's key, from a `public const string` on the class. **Keys are globally unique and issued by Smint.io** — much as port numbers are issued by IANA. Ask for yours rather than inventing one, and do not change it after release |
| `LocalizedStringsModel Name` | `new ResourceLocalizedStringsModel(nameof(ConfigurationMessages.c_mykey_name))`. Around 150 characters at most, one line |
| `LocalizedStringsModel Description` | the same, from your resource file. A little markup (`em`, `strong`, `a href`) survives; full HTML does not |
| `string LogoUrl` | an SVG or PNG, aspect ratio roughly 2:1 |
| `string IconUrl` | a square SVG or PNG |
| `Type ConfigurationImplementation` | `typeof(MyConnectorConfiguration)`. It must implement `IComponentConfiguration` and have a parameterless constructor — the platform creates an empty instance and fills it |
| `Type ComponentImplementation` | `typeof(MyConnector)` |
| `Type ConfigurationMessages` | `typeof(Resources.ConfigurationMessages)` |
| `void ConfigureServices(IServiceCollection services)` | **leave it empty.** Every connector Smint.io ships has an empty body here. This is not where you register your API client — see [handing the client to the data adapter](#user-content-handing-the-client-to-the-data-adapter) |

And the connector-specific half:

| Member | Set it to |
|---|---|
| `string[] DefaultDataAdapterKeys` | the data adapters to create automatically when this connector is configured, by their keys — typically `new[] { "assets" }`. An unknown key is ignored rather than failing. `null` for none |
| `ConnectorSetupMethod SetupMethod` | `None`, `Setup` or `Redirect` — see below |
| `bool IsAdvanced` | `true` when configuring this connector needs an IT administrator rather than an end user, because something has to be set up on the other side first — an OAuth application, an API user, a scope grant. Most connectors are `true` |
| `LocalizedStringsModel SetupDocumentationUrl` | localized links to your setup instructions. **Optional** — `null` is accepted, and several shipped connectors return `null`. Supply it if configuring your connector needs any preparation at all |
| `Type MetamodelMessages` | the resource file holding your meta-model's entity and property labels. **Optional** — `null` when you have no separate meta-model builder. If you *do* build a meta-model with translatable labels, this must point at the right type or those translations silently fail to resolve |

## Setup methods

`ConnectorSetupMethod` decides what the administrator goes through when configuring your
connector.

| Value | Meaning | Use it when |
|---|---|---|
| `None` | no setup step at all | the connector needs no credentials |
| `Setup` | a valid configuration is enough | credentials are typed into the form — an API key, a client id and secret, a personal access token. **This is what most connectors use, including OAuth2 ones using the client-credentials grant** |
| `Redirect` | the browser must be sent to the external system | the user has to consent in the external system's own UI, i.e. a real OAuth2 authorization-code flow. `GetRedirectUrlAsync` must then return the URL |

**`Setup` is the common case, including for OAuth2.** An OAuth2 connector where the credentials
are a client id and secret typed into the configuration form is a `Setup` connector — nothing is
redirected, the connector fetches its own token. Only reach for `Redirect` when a human must
approve something in a browser.

## `IConnector` — every member

| Member | What you do |
|---|---|
| `string Key` | the same key as the startup |
| `PerformPostConfigurationChecksAsync(AuthorizationValuesModel)` | validate the configuration **against the external system**, and put whatever the rest of the flow needs onto the model — the identity server URL, an API URL discovered from the tenant, values in `KeyValueStore`. Throw `ExternalDependencyException` when the configuration is wrong. You may also correct values in the form field values model; nothing revalidates them afterwards, so make sure what you write is valid |
| `GetRedirectUrlAsync(targetRedirectUri, secret, AuthorizationValuesModel, CultureInfo)` | build the authorization URL. Only called when `SetupMethod` is `Redirect`; throw otherwise |
| `InitializeAuthorizationValuesAsync(originalSecret, secret, bootstrapAuthorizationValuesModel)` | produce the authorization values used from now on — exchange the code for tokens, or fetch a token from the credentials, or simply copy the configured token onto the model |
| `PerformPostAuthorizationChecksAsync(FormFieldValuesModel)` | now that you can actually call the external system, confirm it works, and correct configured values that turn out to need it. Frequently a `Task.CompletedTask` |
| `RefreshAuthorizationValuesAsync(AuthorizationValuesModel)` | refresh the token. Called on a timer before expiry |
| `ConfigureServicesForDataAdapter(ServiceCollection)` | register the API client so data adapters can take it by constructor injection |
| `GetConnectorMetamodelAsync()` | build and return the [meta-model](smintio-connector-metamodel.md). A custom connector has no schema to describe |
| `WarmCachesAsync(bool forceWarming = false)` | called on server restart and every fifteen minutes. Pre-fetch what is expensive and stable. A no-op is fine |

`AuthorizationValuesModel` is the connector's own scratch space. The platform stores and returns
it but never interprets it: `AccessToken`, `RefreshToken`, `ExpiresAt`, `IdentityServerUrl`,
`OriginalRedirectUrl`, and a free-form `KeyValueStore` for anything else your connector needs to
remember between calls.

**Do not hold a network connection open.** A connector instance is short-lived and there will be
many of them. Hold credentials, not sockets.

## The call order

When an administrator configures your connector:

```
                  SetupMethod.Setup                 SetupMethod.Redirect
                  ─────────────────                 ────────────────────
  save form   →   PerformPostConfigurationChecksAsync
                                                →   GetRedirectUrlAsync
                                                    (browser goes to the external system,
                                                     comes back with a code)
              →   InitializeAuthorizationValuesAsync
              →   PerformPostAuthorizationChecksAsync
              →   GetConnectorMetamodelAsync
                  (then: data processors, translation, scoping, persistence)
```

Afterwards, in steady state: `RefreshAuthorizationValuesAsync` before the token expires,
`WarmCachesAsync` on restart and every fifteen minutes, and
`ConfigureServicesForDataAdapter` each time a data adapter instance is built.

**`GetConnectorMetamodelAsync` runs when the configuration is set up, regularly thereafter, and
on an administrator action — not per request.** So your meta-model keeps up with the external
system on its own: a new field
or a renamed label is picked up by a later refresh, and whatever that implies for an indexed
source is scheduled automatically. And because it is not on the request path, building it is
allowed to be slow. Read the external system's schema properly rather than cutting corners for
speed.

## Choosing an authentication flow

Four shapes cover everything shipped today. Pick the one that matches the external system, not
the one that sounds most thorough.

### API key or static credentials — implement `IConnector` directly

The most common shape by a wide margin. `SetupMethod.Setup`, credentials in the configuration
form, and the connector turns them into whatever the API expects.

```C#
public override Task<AuthorizationValuesModel> InitializeAuthorizationValuesAsync(
    string originalSecret, string secret, AuthorizationValuesModel bootstrapAuthorizationValuesModel)
{
    bootstrapAuthorizationValuesModel.AccessToken = _configuration.AccessToken;

    return Task.FromResult(bootstrapAuthorizationValuesModel);
}
```

`GetRedirectUrlAsync` is never called, so throw from it.

The [Hello World connector](../Connectors/Connector-HelloWorld/) is this shape, deliberately
minimal and heavily commented. Start there.

### OAuth2 with the client-credentials grant — derive from `OAuth2Connector`

Still `SetupMethod.Setup`: the administrator supplies a client id and secret, and your connector
fetches and refreshes tokens itself. `OAuth2Connector` gives you the plumbing —
`GetClientIdAndSecret`, `GetIdentityServerUrl`, `GetRefreshToken`, `GetAccessToken`,
`HandleInitializationError` — and leaves the flow-specific methods abstract.

Validate the credentials in `PerformPostConfigurationChecksAsync` by calling something cheap and
read-only, and wrap a failure as an `ExternalDependencyException` with the endpoint-test failure
reason. That turns an invalid secret into a message the administrator can act on instead of a
portal that is quietly empty.

### OAuth2 authorization code — derive from `OAuth2Connector`, `SetupMethod.Redirect`

Use this only when a human must consent in the external system. `GetRedirectUrlAsync` builds the
authorize URL and records the redirect it was given:

```C#
public override Task<string> GetRedirectUrlAsync(
    string targetRedirectUri, string secret, AuthorizationValuesModel authorizationValuesModel,
    CultureInfo currentCulture)
{
    var authorizeEndpoint =
        $"{idSrvUrl}/oauth2/authorize?client_id={encodedClientId}&response_type=code" +
        $"&redirect_uri={encodedRedirectUri}&scope={encodedScopes}" +
        $"&state={HttpUtility.UrlEncode(secret)}";

    authorizationValuesModel.OriginalRedirectUrl = targetRedirectUri;

    return Task.FromResult(authorizeEndpoint);
}
```

`InitializeAuthorizationValuesAsync` then exchanges the code for tokens, and
`RefreshAuthorizationValuesAsync` runs the refresh grant.

Carry `secret` through as the `state` parameter, unmodified. It is how the platform matches the
callback to the configuration being set up.

### OAuth2 authorization code with PKCE — derive from `OAuth2AuthenticationCodeFlowWithPKCEConnector`

`OAuth2AuthenticationCodeFlowWithPKCEConnector` extends `OAuth2Connector` and implements
`GetRedirectUrlAsync`, `InitializeAuthorizationValuesAsync` and
`RefreshAuthorizationValuesAsync` for you, adding the code verifier and challenge. Use it when
the external system requires PKCE.

Both the Picturepark and the SharePoint reference connectors in this repository derive from it —
read [the SharePoint connector README](../Connectors/Connector-SharePoint/README.md#authentication-process)
for the flow described step by step against a real system.

### Something else

If the external system authenticates in a way none of these describes, implement `IConnector`
directly and do it yourself. The platform does not require any particular scheme; it requires
that you can produce, refresh and hand over whatever the API needs.

## The API client

Put the external system's API behind a small interface of your own — `IMyClient` — and an
implementation deriving from one of the prefab base classes in
`SmintIo.Portals.ConnectorSDK.Clients.Prefab`. This is not ceremony: it is what lets the data
adapter take a dependency on something testable, and it is where the retry and error handling
live.

### The client must not expose secrets

The client is **the data adapter's way of calling the external system** — and that is all it is.
Access tokens, refresh tokens, client secrets and API keys belong inside the connector and
inside the client's own implementation, and nowhere else.

Concretely, on the client interface:

- **no property or method that returns a token, a secret or a key**, however convenient;
- **no method that hands back a prepared authorization header** for the caller to attach;
- **no "give me the raw HTTP client" escape hatch** that carries credentials with it.

Design the interface in terms of what the data adapter wants — `GetAssetAsync`,
`SearchAsync`, `GetDownloadStreamAsync` — never in terms of how you authenticate. The data
adapter should be able to do its whole job without ever holding a credential.

The reason is blunt: a value the data adapter holds can end up in a log line, in an exception
message, in a serialized result, or in a payload a data processor appends to something. A value
it never holds cannot. This is one of the things a
[code review](smintio-backend-component-delivery.md#user-content-what-the-review-looks-at) checks
for, and it is much cheaper to get right in the first version of the interface than to unpick
later.

The same applies inside the client: log the *operation* and the outcome, never the header you
sent.

| Base class | Use it for |
|---|---|
| `BaseRestSharpApiClient` | a REST/JSON API you call yourself. **The mainstream choice** |
| `BaseDynamicApiClient<TDynamicApiClient>` | a generated or vendor-supplied typed client that you wrap |
| `BaseHttpClientApiClient` | direct `HttpClient` use, when neither of the above fits |

All three derive from `BaseHttpClientApiClient` and implement `IClient`, whose four methods give
you streamed responses with and without backoff — which is what you use to serve binaries.

The pattern is the same whichever you pick: wrap every call to the external system in an
`Execute…Async` helper, handing it a `IRequestFailedHandler` and a short `hint` naming the
operation:

```C#
public Task<AssetResponse> GetAssetAsync(string assetId)
{
    return ExecuteRestSharpRequestWithBackoffAsync(
        async (client, _) =>
        {
            var request = new RestRequest($"assets/{assetId}");
            return await client.GetAsync<AssetResponse>(request).ConfigureAwait(false);
        },
        _requestFailedHandler,
        hint: "get asset",
        isGet: true);
}
```

`WithBackoff` retries transient failures; `WithoutBackoff` does not. Use backoff for reads and
for anything idempotent, and skip it where a retry would duplicate an effect.

**Write a `IRequestFailedHandler` implementation.** It is what turns the external system's error
responses into `ExternalDependencyException`s carrying a reason the platform understands, and
without one a rate-limit response and a wrong password look the same to everyone downstream. The
convention is one class per connector, named `MyDefaultRequestFailedHandler`, next to the client.

If your external system has a well-maintained official SDK, wrapping it in
`BaseDynamicApiClient<T>` is usually better than reimplementing its API surface.

## Handing the client to the data adapter

```C#
public override void ConfigureServicesForDataAdapter(ServiceCollection services)
{
    services.AddTransient(_ => EnsureMyClient());
}
```

The data adapter then takes `IMyClient` as a constructor parameter and the platform resolves it.
That is the whole connection between the two components.

Note which method this is. `IConnectorStartup.ConfigureServices` is a *different* method, it is
empty in every shipped connector, and registering your client there instead produces a
dependency injection failure at runtime with nothing visible at compile time.

## The meta-model identifier

`ConnectorMetamodel`'s first constructor argument is an identifier, and it is more consequential
than it looks. **It identifies the indexed content, and changing it triggers a full re-index.**

```C#
var identifier = $"{MyConnectorStartup.MyConnector}-{_configuration.ClientId}-{_configuration.Channel}";
```

Ordinary schema evolution does **not** go through it. A new field, a renamed label, a new enum
value: the meta-model is refreshed regularly, the difference is detected, and the index work it
implies happens on its own. The identifier is for the case that cannot be handled incrementally —
when what is already indexed is *wrong*, not merely incomplete.

So it has to satisfy two rules at once:

- **Stable while the source is the same.** Never derive it from anything that moves on its own —
  a timestamp, a token, a build number, your connector's version. An identifier that churns
  re-indexes the customer's entire source every time it changes, for nothing.
- **Different when the content is different.** The connector key alone is not enough: two
  configurations of the same connector pointing at different tenants, channels, saved searches or
  folder scopes are indexing different content and should not share an index identity. Compose it
  from the connector key plus the configured values that decide *what* is read.

**Changing it deliberately is the lever for "rebuild everything from scratch"** — the right move
when a meta-model change makes previously indexed data uninterpretable. It is also expensive: a
full re-index of a large customer source costs real time and capacity, so agree it with Smint.io
before shipping a version that changes the identifier on a production source, and say so plainly
in the release notes.

## Project layout

The layout every shipped connector follows. Keeping to it makes a connector legible to anyone
who has read another one:

```
Connector-MyService/
  MyServiceConnectorStartup.cs          IConnectorStartup
  MyServiceConnector.cs                 IConnector
  MyServiceConnectorConfiguration.cs    IComponentConfiguration
  Client/
    IMyServiceClient.cs
    Impl/
      MyServiceClient.cs                the prefab base class
      MyServiceDefaultRequestFailedHandler.cs
  Metamodel/
    MyServiceMetamodelBuilder.cs        IMetamodelBuilder
    MyServiceFormGroupsModelBuilder.cs  the search facets, when you offer any
    MyServiceTranslationLinker.cs       when the external system carries its own translations
  Models/                               request and response DTOs
  Extensions/                           mapping helpers
  AllowedValues/                        IDynamicValueListProvider for configuration dropdowns
  Resources/
    ConfigurationMessages.resx  (+ .de.resx, …)
    MetamodelMessages.resx      (+ .de.resx, …)
```

`AllowedValues/` is worth the effort: a configuration where the administrator picks the channel,
the site or the list from a dropdown your connector filled is a different product from one where
they paste an id.

## Things that are easy to get wrong

- **The key is issued, not chosen.** It is globally unique across all of Smint.io Portals and
  cannot change after release. Ask for it.
- **Never expose a credential on the client interface.** Not a token, not a key, not a prepared
  authorization header. The data adapter calls the external system through the client; it does
  not authenticate to it.
- **Do not build a meta-model you have no consumer for.** A custom connector paired with a
  custom UI component needs none.
- **`IConnectorStartup.ConfigureServices` is not
  `IConnector.ConfigureServicesForDataAdapter`.** The first is empty everywhere; the second is
  the only way a data adapter reaches your client.
- **`OAuth2Connector` does not imply `SetupMethod.Redirect`.** Most OAuth2 connectors use
  `Setup`. Choose the setup method from what the *user* has to do, not from the grant type.
- **Validate against the external system in `PerformPostConfigurationChecksAsync`**, and throw
  `ExternalDependencyException` when it fails. Configuration that is syntactically valid and
  functionally wrong is the single most common support case, and this is the one moment where it
  can be caught in front of the person who can fix it.
- **Whatever you correct in a form field values model is not revalidated.** Write valid data.
- **The meta-model identifier is the index identity, not a cache key.** Keep it stable while the
  source is the same — never derive it from a timestamp, a token or a version — and make it
  differ when the configuration points at different content. Changing it triggers a full
  re-index.
- **Set `MetamodelMessages` when you have translatable meta-model labels.** Leaving it `null`
  makes those translations fail silently — which looks exactly like a translation you forgot to
  write.
- **Write the request failed handler.** Retry behaviour and comprehensible errors both come from
  it.
- **Do not keep connections alive, and do not cache in instance state.** The connector is
  short-lived. `WarmCachesAsync` is where warming belongs.
- **Declare the feature flags honestly.** `isFolderNavigationSupported`,
  `isRandomAccessSupported` and `isFullTextSearchProposalsSupported` are promises the data
  adapter has to keep — see
  [say what you support](smintio-asset-data-model.md#user-content-say-what-you-support).

## Questions

Please do not hesitate to contact us at [support@smint.io](mailto:support@smint.io) if you run
into any issues.

Contributors
============

- Reinhard Holzner, Smint.io GmbH

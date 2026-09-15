How-to implement the `Connector`
================================

Current version of this document is: 1.3.0 (as of 15th of September, 2026)

This is the short orientation. **The full contract — every member of `IConnectorStartup` and
`IConnector`, the setup methods, the call order, the four authentication flows with worked code,
the API client base classes and the project layout — is in
[the connector contract](../docs/smintio-connector-reference.md).**

## `Connector` Basics

A `Connector` in the context of Smint.io Portals is an object that contains information about the external
system's metamodel and handles authorization.

One of the `Connector's` main tasks is to establish and maintain a trust context to the external system, e.g. by
obtaining access and refresh tokens. It should not keep any network connections alive. The connector also should provide a
means to refresh the access token, if OAuth2 is used.

Also, the `Connector` is tasked with establishing a meta-model of whatever is stored in the external system — **but only
when it is a productized connector**. A custom connector, whose data adapter publishes its own interfaces for one custom
UI component to consume, needs no meta-model at all: the interface is the data model. See
[productized or custom](../README.md#user-content-productized-or-custom). The meta-model
should describe the data structure that is being delivered by the external system. This meta-model is then used throughout 
Smint.io Portals to interpret the external metadata delivered by the external system (e.g. also custom metadata).

For example, if the external System is a simple file storage, the meta-model will likely
consist of all properties that a file can have. You can think of it in a similar way to file properties in a local file
system. While a text document has a `ModifiedDate`, a `Creator` and so on, an image might have a `Width` and a
`Height` property etc.

Regarding authorization a connector class can inherit from one the prefabricated flows that we support `OAuth2AuthenticationCodeFlowWithPKCEConnector` and `OAuth2Connector`.
An alternative to that would be to implement `IConnector`. This is in case the external system is using another flow or custom authorization — which is the common case, not the exception.

Note that deriving from `OAuth2Connector` does **not** imply the `Redirect` setup method. Most
OAuth2 connectors use `Setup`, because the credentials are typed into the configuration form and
the connector fetches its own tokens. Choose the setup method from what the *user* has to do, not
from the grant type — see
[choosing an authentication flow](../docs/smintio-connector-reference.md#user-content-choosing-an-authentication-flow).

For `OAuth2AuthenticationCodeFlowWithPKCEConnector` examples see [here](Connector-Picturepark/PictureparkConnector.cs) and [here](Connector-SharePoint/SharepointConnector.cs).
A more detailed description of the OAuth2 authentication code flow with PKCE (pixy) can be found [here](Connector-SharePoint/README.md#authentication-process).

For custom `IConnector` implementation click [here](Connector-HelloWorld/HelloWorldConnector.cs).

## The connector meta-model

Building the meta-model is the `Connector`'s second job, alongside authorization. It describes what types of *objects*
exist in the external system and what properties they have, so that Smint.io Portals can interpret the data your data
adapter delivers.

This is documented in full in **[the connector meta-model](../docs/smintio-connector-metamodel.md)** — the data types,
entity and property members, enum entities, indexing, semantic types, form groups, the converter that applies it, and
the lifecycle.

For a worked example against a real system, see the
[SharePoint meta-model walkthrough](Connector-SharePoint/README.md#meta-model-structure).

Contributors
============

- Reinhard Holzner, Smint.io GmbH
- Yosif Velev, Smint.io GmbH
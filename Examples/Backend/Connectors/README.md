How-to implement the `Connector`
================================

Current version of this document is: 1.0.0 (as of 8th of March, 2023)

## `Connector` Basics

A `Connector` in the context of Smint.io Portals is an object that contains information about the external
system's metamodel and handles authorization.

One of the `Connector's` main tasks is to establish and maintain a trust context to the external system, e.g. by
obtaining access and refresh tokens. It should not keep any network connections alive. The connector also should provide a
means to refresh the access token, if OAuth2 is used.

Also, the `Connector` is tasked with establishing a meta-model of whatever is stored in the external system. The meta-model
should describe the data structure that is being delivered by the external system. This meta-model is then used throughout 
Smint.io Portals to interpret the external metadata delivered by the external system (e.g. also custom metadata).

For example, if the external System is a simple file storage, the meta-model will likely
consist of all properties that a file can have. You can think of it in a similar way to file properties in a local file
system. While a text document has a `ModifiedDate`, a `Creator` and so on, an image might have a `Width` and a
`Height` property etc.

Regarding authorization a connector class can inherit from one the prefabricated flows that we support `OAuth2AuthenticationCodeFlowWithPKCEConnector` and `OAuth2Connector`.
An alternative to that would be to implement `IConnector`. This is in case the external system is using another flow or custom authorization.

For `OAuth2AuthenticationCodeFlowWithPKCEConnector` examples see [here](Connectors/Connector-Picturepark/PictureparkConnector.cs) and [here](Connectors/Connector-SharePoint/SharepointConnector.cs).
A more detailed description of the OAuth2 authentication code flow with PKCE (pixy) can be found [here](Connectors/Connector-SharePoint/README.md#authentication-process).

For custom `IConnector` implementation click [here](Connectors/Connector-HelloWorld/HelloWorldConnector.cs).

## The connector meta-model

Generally speaking the meta-model describes what types of *objects* exist in the external system and what properties they have. In other words: the meta-model describes the _data_ that is delivered. 

This meta-model is then used throughout Smint.io Portals to interpret the external metadata delivered by the external system (e.g. also custom metadata).

A more detailed explanation of the connector meta-model is available [here](Connectors/Connector-SharePoint/README.md#meta-model-structure)

Contributors
============

- Reinhard Holzner, Smint.io GmbH
- Yosif Velev, Smint.io GmbH
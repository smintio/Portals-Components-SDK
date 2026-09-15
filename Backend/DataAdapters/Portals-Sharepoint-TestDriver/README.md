The SharePoint test driver
==========================

Current version of this document is: 1.1.2 (as of 15th of September, 2026)

## Basics

The `DataAdapter` can be understood as a _facade_ for the external system. It is used to read, search (and potentially write) data from (and to) an external system.

It uses the `Connector` to establish a connection to the external system such as SharePoint.

To check that both the data adapter and the connector work against a real tenant, Smint.io ships a set of shared
integration tests. This document walks through how this example wires them up; the full contract — the fixture base
class, the whole set of shared test classes and what each expects — is in
[building, testing and publishing](../../docs/smintio-backend-component-delivery.md).

## Folder Structure

We have two folders in the test suite, `Harness` and `Integration`. Here's a brief description of each:

### Harness

The files in this folder are used to configure the data adapter fixture. In our case, the `SharepointFixture` first creates a connector test driver instance using `OAuth2AuthenticationCodeFlowWithPKCETestDriver` which we call `connectorTestDriver`. The test driver accepts `typeof(SharepointConnectorStartup)` and the startup settings. Then the connector test driver know how to create a valid `Connector` instance and obtain access and refresh tokens.

Next is the initialization of the data adapter instance using a `DataAdapterTestDriver`. This test driver instantiates an in-memory corresponding representation for the given startup type parameter `typeof(SharepointAssetsDataAdapterStartup)` for the given configurations.

Having both the connector and data adapter initialized, we can proceed and test them.

Please note that we use the term "fixture" for the harness that builds the connector and the data adapter together and
hands both to the tests.

### Integration

The integration folder contains the tests for SharePoint. They run against a real tenant, so they are integration tests
rather than unit tests. Smint.io ships a set of shared test classes, and this example inherits the ones that apply:

* ConnectorMetamodelTests - will test the connector's meta-model
* ConnectorTests - will test the connector itself
* DataAdapterIntegrationLayerStartupTests - will test basic data adapter integration layer startup integrity
* DataAdapterIntegrationLayerTests - will test the data adapter using an in-memory representation of the integration layer

Each set of tests requires configuration by overriding some of its properties.
Example would be for a valid connector and data adapter instance based on the configured `SharepointFixture`.

```C#
protected override ConnectorMetamodel GetConnectorMetamodel() => _fixture.Metamodel;

protected override AssetsDataAdapterBaseImpl GetDataAdapter(Type componentImplementation) => _fixture.DataAdapter;
```

Another override would be the definition of a different sample assets so that the connector can test with existing data.

```C#
        protected override AssetItemOption SampleImageAsset => _fixture.AssetOptions.ImageAsset;

        protected override AssetItemOption SampleVideoAsset => _fixture.AssetOptions.VideoAsset;

        protected override AssetItemOption SampleAudioAsset => _fixture.AssetOptions.AudioAsset;

        protected override AssetItemOption SampleDocumentAsset => _fixture.AssetOptions.DocumentAsset;
```

The behavior of the tests can be altered by overriding methods that check the integrity of the data.

```C#
//in SharePointDataAdapterIntegrationLayerTests.cs
protected override void AssertContentMetadata(AssetDataObject assetDataObject)
{
    if (assetDataObject.ImageMetadata != null)
    {
        assetDataObject.ImageMetadata.Height.Should().NotBeNull();
        assetDataObject.ImageMetadata.Width.Should().NotBeNull();
        assetDataObject.ThumbnailAspectRatio.Should().NotBeNull();
    }
    else if (assetDataObject.VideoMetadata != null)
    {
        assetDataObject.VideoMetadata.Height.Should().NotBeNull();
        assetDataObject.VideoMetadata.Width.Should().NotBeNull();
        assetDataObject.ThumbnailAspectRatio.Should().NotBeNull();
    }
}
```

This way each individual metadata can be asserted based on what we expect from SharePoint.

Please note that Smint.io tests are constantly being updated and new tests added, so everything is subject to future change.

Contributors
============

- Reinhard Holzner, Smint.io GmbH
- Yosif Velev, Smint.io GmbH
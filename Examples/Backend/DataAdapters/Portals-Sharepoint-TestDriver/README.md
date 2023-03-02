# `DataAdapter` Unit Tests

## Basics

The `DataAdapter` can be understood as a _facade_ for the external system. It is used to read, search (and potentially write) data from (and to) an external system.

It uses the `Connector` to establish a connection to the external system such as Sharepoint. 

In order to ensure that both the DataAdapter and Connector function correctly, `Smint.io` created a set of unit tests.
This documentation briefly explains how does the `Smint.io` test framework works.

## Folder Structure

We have two folders in the test suite, `Harness` and `Integration` Here's a brief description of each:

### Harness

The files in this folder are used to configure the data adapter fixture. In our case, the `SharePointFixture` first creates a Connector test drive instance using `OAuth2AuthenticationCodeFlowWithPKCETestDriver` which we call `connectorTestDriver`. The test driver accepts `typeof(SharepointConnectorStartup)` and the startup settings. Then the connector test driver know how to create a valid `Connector` instance and obtain access and refresh tokens.

Next is the initialization of the data adapter instance using a `DataAdapterTestDriver`. This test drive instantiates an in-memory corresponding representation for the given startup type parameter `typeof(SharepointAssetsDataAdapterStartup)` for the given configurations.

Having both the connector and data adapter initialized, we can proceed and test them.

Please note that we use the term "fixture" to describe the configuration of a DataAdapter.

### Integration

The integration folder contains all the unit tests for SharePoint. Since `Smint.io` has a set of generic unit tests, we can leverage them by inheriting some of the base classes. 

* ConnectorMetamodelTests - will test connector's metamodel
* ConnectorTests - will test the connector itself
* DataAdapterIntegrationLayerStartupTests - will test basic data adapter integration layer startup integrity
* DataAdapterIntegrationLayerTests - will test the data adapter using an in-memory representation of the integration layer

Each set of tests requires configuration by overriding some of its properties.
Example would be for a valid connector and data apdater instance based on the configured `SharePointFixture`.

```c#
protected override ConnectorMetamodel GetConnectorMetamodel() => _fixture.Metamodel;

protected override AssetsDataAdapterBaseImpl GetDataAdapter(Type componentImplementation) => _fixture.DataAdapter;
```

Another override would be the definition of a different sample assets so that the connector can test with existing data.

The behavior of the unit tests can be altered by overriding methods that check the integrity of the data.

```c#
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

Please note that `Smint.io` tests are constantly being updated and new tests added, so everything is subject to future change.
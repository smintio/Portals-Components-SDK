using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using SmintIo.Portals.Connector.Picturepark;
using SmintIo.Portals.Connector.Test.Tests;
using SmintIo.Portals.ConnectorSDK.Metamodel;
using SmintIo.Portals.ConnectorSDK.TestDriver.Picturepark.Test.Harness;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Impl;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets.Parameters;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Progress;
using SmintIo.Portals.SDK.Core.Models.Metamodel.Data;
using SmintIo.Portals.SDK.Core.Models.Metamodel.Model;
using SmintIo.Portals.SDK.Core.Models.Strings;
using Xunit;
using Xunit.Abstractions;

namespace SmintIo.Portals.ConnectorSDK.TestDriver.Picturepark.Test.Integration
{
    [Collection(nameof(PictureparkFixtureCollection))]
    public class PictureparkConnectorTest : ConnectorTests<PictureparkConnector>
    {
        private readonly PictureparkFixture _fixture;
        private readonly ITestOutputHelper _helper;

        public PictureparkConnectorTest(PictureparkFixture fixture, ITestOutputHelper helper)
        {
            _fixture = fixture;
            _helper = helper;
        }

        protected override PictureparkConnector GetConnector(Type componentType) => _fixture.Connector;

        protected ConnectorMetamodel Metamodel => _fixture.Metamodel;

        protected AssetsDataAdapterBaseImpl DataAdapter => _fixture.DataAdapter;

        [Fact]
        public async Task TestPictureparkConnectorAsync()
        {
            PrintMetamodel(Metamodel);

            // do something with the data adapter e.g...

            var assets = await DataAdapter.SearchAssetsAsync(new SearchAssetsParameters()
            {
                Page = 1,
                PageSize = 5
            }).ConfigureAwait(false);

            foreach (var assetSearch in assets.AssetDataObjects)
            {
                Console.WriteLine($"Found asset with ID {assetSearch.Id}");

                var assetGet = await DataAdapter.GetAssetAsync(new GetAssetParameters()
                {
                    AssetId = new AssetIdentifier(assetSearch.Id)
                });

                Console.WriteLine($"Asset has name {assetGet.AssetDataObject.Name?.ResolveLocalizedString(CultureInfo.CurrentCulture)}");

                // note, that here the connector metamodel (should have / has) already been used to
                // resolve the raw metadata of the external data source
            }

            //verify that the download mapping is correct
            var param = new GetAssetsDownloadItemMappingsParameters
            {
                AssetIds = assets.AssetDataObjects.Select(ado => new AssetIdentifier(ado.Id)).ToArray()
            };
            var result = await DataAdapter.GetAssetsDownloadItemMappingsAsync(param, new DummyProgressMonitor());
            Assert.Equal(5, result.AssetDownloadItemMappings.Count);
        }

        private void PrintMetamodel(ConnectorMetamodel connectorMetamodel)
        {
            foreach (var entity in connectorMetamodel.Entities)
            {
                var childStr = entity.ParentMetamodelEntityKey != null
                    ? $" (is child of \"{entity.ParentMetamodelEntityKey}\") and "
                    : "";

                var typeStr = entity is EnumEntityModel ? $" (type = {nameof(EnumEntityModel)}) " : "";
                _helper.WriteLine($"Entity \"{entity.Key}\" {typeStr}{childStr} has {entity.PropertyCount} properties:");


                foreach (var prop in entity.Properties)
                {
                    _helper.WriteLine($"  - {prop.Key} : {prop.DataType}");
                }

                if (entity is EnumEntityModel eem)
                {

                    _helper.WriteLine($"  - Enum Values: {string.Join(", ", eem.EnumValues.Select(e => e.Id))}");
                }
                _helper.WriteLine("");
            }
        }
    }

    public class DummyProgressMonitor : IProgressMonitor
    {
        public Task ReportProgressIncrementAsync(double units, LocalizedStringsModel displayText)
        {
            return Task.CompletedTask;
        }

        public Task ReportProgressAbsoluteAsync(double units, LocalizedStringsModel displayText)
        {
            return Task.CompletedTask;
        }

        public Task FinishedAsync(LocalizedStringsModel displayText)
        {
            return Task.CompletedTask;
        }

        public double CurrentValue { get; }

        public double Maximum { get; set; }
    }
}

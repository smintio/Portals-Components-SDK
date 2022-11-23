using SmintIo.Portals.Connector.Picturepark;
using SmintIo.Portals.DataAdapter.Picturepark.Assets;
using SmintIo.Portals.DataAdapter.Picturepark.Assets.Search;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets.Parameters;
using SmintIo.Portals.DataAdapterSDK.TestDriver;
using SmintIo.Portals.SDK.Core.Models.Metamodel.Data;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using SmintIo.Portals.ConnectorSDK.Metamodel;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Progress;
using SmintIo.Portals.SDK.Core.Models.Metamodel.Model;
using SmintIo.Portals.SDK.Core.Models.Strings;
using Xunit;
using Xunit.Abstractions;

namespace SmintIo.Portals.ConnectorSDK.TestDriver.Picturepark.Test
{
    public class PictureparkConnectorTest
    {
        private readonly ITestOutputHelper _helper;

        public PictureparkConnectorTest(ITestOutputHelper helper)
        {
            _helper = helper;
        }

        [Fact]
        public async Task TestPictureparkConnectorAsync()
        {
            // we also need to fill the defaults, as those are not pre-filled by the test driver

            var pictureparkConnectorConfiguration = new PictureparkConnectorConfiguration()
            {
                PictureparkUrl = "https://smint.poc-picturepark.com/ContentBrowser/",
                AccessToken = "4c797abdeb017c850dd7a8abd96ed646da9ca2425db291c0e5377e6e414b9e10",
                Channel = "rootChannel"
            };

            var connectorTestDriver = new SetupTestDriver();

            await connectorTestDriver.InstantiateConnectorAsync(
                typeof(PictureparkConnectorStartup),
                pictureparkConnectorConfiguration).ConfigureAwait(false);

            var connector = connectorTestDriver.Connector;
            var connectorMetamodel = connectorTestDriver.ConnectorMetamodel;

            PrintMetamodel(connectorMetamodel);

            // do something, e.g. look at what's there...
            // note that usually, data adapters do NOT have access to this data directly

            // you can then configure a data adapter as well using the data adapter test driver

            var pictureparkAssetsDataAdapterConfiguration = new PictureparkAssetsDataAdapterConfiguration()
            {

            };

            var dataAdapterTestDriver = new DataAdapterTestDriver(connector, connectorMetamodel);

            await dataAdapterTestDriver.InstantiateDataAdapterAsync(
                typeof(PictureparkAssetsDataAdapterStartup),
                pictureparkAssetsDataAdapterConfiguration);

            var dataAdapter = (PictureparkAssetsDataAdapter)dataAdapterTestDriver.DataAdapter;

            // do something with the data adapter e.g...

            var assets = await dataAdapter.SearchAssetsAsync(new SearchAssetsParameters()
            {
                Page = 1,
                PageSize = 5
            }).ConfigureAwait(false);

            foreach (var assetSearch in assets.AssetDataObjects)
            {
                Console.WriteLine($"Found asset with ID {assetSearch.Id}");

                var assetGet = await dataAdapter.GetAssetAsync(new GetAssetParameters()
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
            var result= await dataAdapter.GetAssetsDownloadItemMappingsAsync(param, new DummyProgressMonitor());
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
        public async Task ReportProgressAsync(double units, LocalizedStringsModel displayText)
        {
        }

        public async Task FinishedAsync(LocalizedStringsModel displayText)
        {
        }

        public double CurrentValue { get; }
        public double Maximum { get; set; }
    }
}

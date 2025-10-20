using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmintIo.Portals.Connector.Picturepark;
using SmintIo.Portals.Connector.Picturepark.Client;
using SmintIo.Portals.DataAdapter.Picturepark.Assets;
using SmintIo.Portals.DataAdapter.Picturepark.Assets.Search;
using SmintIo.Portals.DataAdapter.Picturepark.ExternalUsers;
using SmintIo.Portals.DataAdapterSDK.TestDriver;
using SmintIo.Portals.DataAdapterSDK.TestDriver.Harness;

namespace SmintIo.Portals.ConnectorSDK.TestDriver.Picturepark.Test.Harness
{
    public class PictureparkFixture : BaseDataAdapterFixture<PictureparkConfigurationOptions, PictureparkConnector, PictureparkAssetsDataAdapter>
    {
        public PictureparkExternalUsersDataAdapter ExternalUsersDataAdapter { get; protected set; }

        protected override void BindSections(IConfiguration configuration)
        {
            base.BindSections(configuration);

            configuration.GetSection(PictureparkConfigurationOptions.Name).Bind(ConfigurationOptions);
        }

        public async override Task InitializeAsync()
        {
            await CreateConnectorAsync().ConfigureAwait(false);

            var services = new ServiceCollection();

            services.AddTransient(s =>
            {
                var props = Connector
                    .GetType()
                    .GetField("_pictureparkClient", BindingFlags.NonPublic | BindingFlags.Instance);

                var pictureparkClient = (IPictureparkClient)props?.GetValue(Connector);

                return pictureparkClient;
            });

            ServiceProvider = services.BuildServiceProvider();

            DataAdapter = await CreateDataAdapterAsync().ConfigureAwait(false);
            ExternalUsersDataAdapter = await CreateExternalUsersDataAdapterAsync().ConfigureAwait(false);
        }

        private async Task<PictureparkAssetsDataAdapter> CreateDataAdapterAsync()
        {
            if (Connector == null)
            {
                throw new ArgumentNullException(nameof(Connector));
            }

            var dataAdapterConfiguration = new PictureparkAssetsDataAdapterConfiguration
            {
                DefaultPageSize = 50,
                MultiSelectItemCount = 15
            };

            var dataAdapterTestDriver = new DataAdapterTestDriver(Connector, Metamodel);

            await dataAdapterTestDriver.InstantiateDataAdapterAsync(typeof(PictureparkAssetsDataAdapterStartup), dataAdapterConfiguration);

            var dataAdapter = dataAdapterTestDriver.DataAdapter;

            return (PictureparkAssetsDataAdapter)dataAdapter;
        }

        private async Task<PictureparkExternalUsersDataAdapter> CreateExternalUsersDataAdapterAsync()
        {
            if (Connector == null)
            {
                throw new ArgumentNullException(nameof(Connector));
            }

            var dataAdapterConfiguration = new PictureparkExternalUsersDataAdapterConfiguration();

            var dataAdapterTestDriver = new DataAdapterTestDriver(Connector, Metamodel);

            await dataAdapterTestDriver.InstantiateDataAdapterAsync(typeof(PictureparkExternalUsersDataAdapterStartup), dataAdapterConfiguration);

            var dataAdapter = dataAdapterTestDriver.DataAdapter;

            return (PictureparkExternalUsersDataAdapter)dataAdapter;
        }

        protected override async Task CreateConnectorAsync()
        {
            var connectorConfiguration = new PictureparkConnectorConfiguration()
            {
                PictureparkUrl = ConfigurationOptions.PictureparkUrl,
                AccessToken = ConfigurationOptions.AccessToken,
                Channel = ConfigurationOptions.Channel
            };

            var connectorTestDriver = new SetupTestDriver();

            await connectorTestDriver.InstantiateConnectorAsync(typeof(PictureparkConnectorStartup), connectorConfiguration);

            Metamodel = connectorTestDriver.ConnectorMetamodel;
            Connector = (PictureparkConnector)connectorTestDriver.Connector;
        }
    }
}

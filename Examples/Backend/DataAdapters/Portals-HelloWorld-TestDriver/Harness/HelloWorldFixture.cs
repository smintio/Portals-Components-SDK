using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmintIo.Portals.Connector.HelloWorld;
using SmintIo.Portals.Connector.HelloWorld.Client;
using SmintIo.Portals.DataAdapter.HelloWorld.Assets;
using SmintIo.Portals.DataAdapterSDK.TestDriver;
using SmintIo.Portals.DataAdapterSDK.TestDriver.Harness;

namespace SmintIo.Portals.ConnectorSDK.TestDriver.HelloWorld.Test.Harness
{
    public class HelloWorldFixture : BaseDataAdapterFixture<OAuthOptions, HelloWorldConnector>
    {
        protected override void BindSections(IConfiguration configuration)
        {
            base.BindSections(configuration);

            configuration.GetSection(OAuthOptions.Name).Bind(ConfigurationOptions);
        }

        public async override Task InitializeAsync()
        {
            await CreateConnectorAsync().ConfigureAwait(false);

            var services = new ServiceCollection();

            services.AddTransient(s =>
            {
                var props = Connector
                    .GetType()
                    .GetField("_helloWorldClient", BindingFlags.NonPublic | BindingFlags.Instance);

                var helloWorldClient = (IHelloWorldClient)props?.GetValue(Connector);

                return helloWorldClient;
            });

            ServiceProvider = services.BuildServiceProvider();

            DataAdapter = await CreateDataAdapterAsync().ConfigureAwait(false);
        }

        private async Task<HelloWorldAssetsDataAdapter> CreateDataAdapterAsync()
        {
            if (Connector == null)
            {
                throw new ArgumentNullException(nameof(Connector));
            }

            var config = new HelloWorldAssetsDataAdapterConfiguration()
            {
                MultiSelectItemCount = 15
            };

            var dataAdapterTestDriver = new DataAdapterTestDriver(Connector, Metamodel);

            await dataAdapterTestDriver.InstantiateDataAdapterAsync(typeof(HelloWorldAssetsDataAdapterStartup), config);

            var dataAdapter = dataAdapterTestDriver.DataAdapter;

            return (HelloWorldAssetsDataAdapter)dataAdapter;
        }

        protected override async Task CreateConnectorAsync()
        {
            var connectorConfiguration = new HelloWorldConnectorConfiguration
            {
                SiteUrl = ConfigurationOptions.SiteUrl,
                ClientId = ConfigurationOptions.ClientId,
                ClientSecret = ConfigurationOptions.ClientSecret
            };

            var connectorTestDriver = new SetupTestDriver();

            await connectorTestDriver.InstantiateConnectorAsync(typeof(HelloWorldConnectorStartup), connectorConfiguration);

            Metamodel = connectorTestDriver.ConnectorMetamodel;
            Connector = (HelloWorldConnector)connectorTestDriver.Connector;
        }
    }
}
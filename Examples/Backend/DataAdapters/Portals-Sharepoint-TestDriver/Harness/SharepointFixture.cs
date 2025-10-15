using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmintIo.Portals.Connector.SharePoint;
using SmintIo.Portals.Connector.SharePoint.Client;
using SmintIo.Portals.DataAdapter.SharePoint.Assets;
using SmintIo.Portals.DataAdapterSDK.TestDriver;
using SmintIo.Portals.DataAdapterSDK.TestDriver.Harness;

namespace SmintIo.Portals.ConnectorSDK.TestDriver.Sharepoint.Test.Harness
{
    public class SharepointFixture : BaseDataAdapterFixture<OAuthOptions, SharepointConnector, SharepointAssetsDataAdapter>
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
                    .GetField("_sharepointClient", BindingFlags.NonPublic | BindingFlags.Instance);

                var sharepointClient = (ISharepointClient)props?.GetValue(Connector);

                return sharepointClient;
            });

            ServiceProvider = services.BuildServiceProvider();

            DataAdapter = await CreateDataAdapterAsync().ConfigureAwait(false);
        }

        private async Task<SharepointAssetsDataAdapter> CreateDataAdapterAsync()
        {
            if (Connector == null)
            {
                throw new ArgumentNullException(nameof(Connector));
            }

            var config = new SharepointAssetsDataAdapterConfiguration();

            var dataAdapterTestDriver = new DataAdapterTestDriver(Connector, Metamodel);

            await dataAdapterTestDriver.InstantiateDataAdapterAsync(typeof(SharepointAssetsDataAdapterStartup), config);

            var dataAdapter = dataAdapterTestDriver.DataAdapter;

            return (SharepointAssetsDataAdapter)dataAdapter;
        }

        protected override async Task CreateConnectorAsync()
        {
            var sharepointSettings = new SharepointConnectorConfiguration
            {
                SiteId = ConfigurationOptions.SiteId,
                SharepointUrl = ConfigurationOptions.SharepointUrl,
                SiteFolderIds = AssetOptions.Folders.Select(f => f.Id).ToArray()
            };

            var connectorTestDriver = new OAuth2AuthenticationCodeFlowWithPKCETestDriver("http://localhost:7587", RefreshToken);

            await connectorTestDriver.InstantiateConnectorAsync(typeof(SharepointConnectorStartup), sharepointSettings)
                .ConfigureAwait(false);

            Metamodel = connectorTestDriver.ConnectorMetamodel;

            var connector = (SharepointConnector)connectorTestDriver.Connector;

            RefreshToken = connector.GetAuthorizationValues().RefreshToken;

            Connector = connector;
        }
    }
}
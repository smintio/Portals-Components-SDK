using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmintIo.Portals.Connector.SharePoint;
using SmintIo.Portals.Connector.SharePoint.Client;
using SmintIo.Portals.ConnectorSDK.Metamodel;
using SmintIo.Portals.DataAdapter.SharePoint.Assets;
using SmintIo.Portals.DataAdapterSDK.TestDriver;
using Xunit;

namespace SmintIo.Portals.ConnectorSDK.TestDriver.Sharepoint.Test.Harness
{
    public class SharepointFixture : IAsyncLifetime
    {
        private const string RefreshTokenFilename = "refreshtoken.txt";

        private string _refreshToken;

        public SharepointFixture()
        {
            if (File.Exists(RefreshTokenFilename))
            {
                _refreshToken = File.ReadAllText(RefreshTokenFilename);

                if (string.IsNullOrWhiteSpace(_refreshToken))
                {
                    _refreshToken = null;
                }
            }
            else
            {
                File.Create(RefreshTokenFilename);
            }
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false)
                .AddJsonFile("appsettings.local.json", true)
                .AddEnvironmentVariables()
                .Build();

            OauthConfig = new OauthOptions();
            configuration.GetSection(OauthOptions.Name).Bind(OauthConfig);

            SharepointOptions = new SharepointOptions();
            configuration.GetSection(SharepointOptions.Name).Bind(SharepointOptions);
        }

        public OauthOptions OauthConfig { get; }

        public SharepointOptions SharepointOptions { get; }

        public SharepointConnector Connector { get; set; }

        public ConnectorMetamodel Metamodel { get; set; }

        public SharepointAssetsDataAdapter DataAdapter { get; private set; }

        public IServiceProvider ServiceProvider { get; private set; }

        public async Task InitializeAsync()
        {
            var (metamodel, connector) = await CreateConnectorAsync().ConfigureAwait(false);

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

            DataAdapter = await CreateDataAdapterAsync(metamodel, connector).ConfigureAwait(false);
        }

        public async Task DisposeAsync()
        {
            await File.WriteAllTextAsync(RefreshTokenFilename, _refreshToken);
        }

        private async Task<SharepointAssetsDataAdapter> CreateDataAdapterAsync(ConnectorMetamodel metamodel, SharepointConnector connector)
        {
            Metamodel = metamodel;
            Connector = connector;

            var config = new SharepointAssetsDataAdapterConfiguration();

            var dataAdapterTestDriver = new DataAdapterTestDriver(connector, metamodel);

            await dataAdapterTestDriver.InstantiateDataAdapterAsync(typeof(SharepointAssetsDataAdapterStartup), config);

            var dataAdapter = dataAdapterTestDriver.DataAdapter;

            return (SharepointAssetsDataAdapter)dataAdapter;
        }

        private async Task<(ConnectorMetamodel, SharepointConnector)> CreateConnectorAsync()
        {
            var sharepointSettings = new SharepointConnectorConfiguration
            {
                SiteId = SharepointOptions.SiteId,
                SharepointUrl = OauthConfig.SharepointUrl
            };

            var connectorTestDriver = new OAuth2AuthenticationCodeFlowWithPKCETestDriver("http://localhost:7587", _refreshToken);

            await connectorTestDriver.InstantiateConnectorAsync(typeof(SharepointConnectorStartup), sharepointSettings)
                .ConfigureAwait(false);

            var connector = (SharepointConnector)connectorTestDriver.Connector;
            
            _refreshToken = connector.GetAuthorizationValues().RefreshToken;

            return (connectorTestDriver.ConnectorMetamodel, connector);
        }
    }
}
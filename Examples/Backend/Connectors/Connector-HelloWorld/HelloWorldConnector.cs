using System;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmintIo.Portals.Connector.HelloWorld.Client;
using SmintIo.Portals.Connector.HelloWorld.Client.Impl;
using SmintIo.Portals.Connector.HelloWorld.Metamodel;
using SmintIo.Portals.ConnectorSDK.Connectors;
using SmintIo.Portals.ConnectorSDK.Metamodel;
using SmintIo.Portals.ConnectorSDK.Models;
using SmintIo.Portals.ConnectorSDK.Models.Prefab;
using SmintIo.Portals.SDK.Core.Cache;
using SmintIo.Portals.SDK.Core.Configuration;
using SmintIo.Portals.SDK.Core.Models.Context;
using SmintIo.Portals.SDK.Core.Rest.Prefab.Exceptions;

namespace SmintIo.Portals.Connector.HelloWorld
{
    public class HelloWorldConnector : IConnector
    {
        private readonly ILogger _logger;
        private readonly ICache _cache;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IPortalsContextModel _portalsContext;

        private readonly HelloWorldConnectorConfiguration _connectorConfiguration;

        private IHelloWorldClient _helloWorldClient;

        public HelloWorldConnector(
            IServiceProvider serviceProvider,
            ILogger logger,
            ICache cache,
            IHttpClientFactory httpClientFactory,
            HelloWorldConnectorConfiguration configuration)
        {
            _logger = logger;
            _cache = cache;
            _httpClientFactory = httpClientFactory;

            _portalsContext = serviceProvider.GetService<IPortalsContextModel>();

            _connectorConfiguration = configuration;
        }

        public string Key => HelloWorldConnectorStartup.HelloWorldConnector;
        
        public AuthorizationValuesModel AuthorizationValuesModel { get; private set; }

        public Task PerformPostConfigurationChecksAsync(AuthorizationValuesModel authorizationValuesModel)
        {
            authorizationValuesModel.IdentityServerUrl = _connectorConfiguration.SiteUrl;

            authorizationValuesModel.ClientId = _connectorConfiguration.ClientId;
            authorizationValuesModel.ClientSecret = _connectorConfiguration.ClientSecret;

            return Task.CompletedTask;
        }

        public async Task<AuthorizationValuesModel> InitializeAuthorizationValuesAsync(string originalSecret, string secret, AuthorizationValuesModel bootstrapAuthorizationValuesModel)
        {
            var oAuth2GetAccessTokenResponse = await GetAccessTokenAsync(bootstrapAuthorizationValuesModel).ConfigureAwait(false);

            bootstrapAuthorizationValuesModel.AccessToken = oAuth2GetAccessTokenResponse.AccessToken;
            bootstrapAuthorizationValuesModel.RefreshToken = oAuth2GetAccessTokenResponse.RefreshToken;
            bootstrapAuthorizationValuesModel.ExpiresAt = oAuth2GetAccessTokenResponse.ExpiresAt;

            return bootstrapAuthorizationValuesModel;
        }

        private static Task<OAuth2GetAccessTokenResponse> GetAccessTokenAsync(AuthorizationValuesModel authorizationValuesModel)
        {
            // Implement get access token request

            return Task.FromResult(new OAuth2GetAccessTokenResponse
            {
                AccessToken = Guid.NewGuid().ToString(),
                RefreshToken = Guid.NewGuid().ToString(),
                ExpiresAt = DateTime.UtcNow.AddHours(1),
            });
        }

        public async Task PerformPostAuthorizationChecksAsync(FormFieldValuesModel formFieldValuesModel)
        {
            var helloWorldClient = EnsureHelloWorldClient();

            try
            {
                await helloWorldClient.EndpointTestAsync().ConfigureAwait(false);
            }
            catch (Exception)
            {
                throw new ExternalDependencyException(ExternalDependencyStatusEnum.EndpointTestFailed, "The HelloWold endpoint test failed", HelloWorldConnectorStartup.HelloWorldConnector);
            }
        }

        public Task<AuthorizationValuesModel> RefreshAuthorizationValuesAsync(AuthorizationValuesModel authorizationValuesModel)
        {
            throw new NotSupportedException();
        }

        public void SetAuthorizationValuesModel(AuthorizationValuesModel authorizationValuesModel)
        {
            if (authorizationValuesModel is null)
            {
                throw new ArgumentNullException(nameof(authorizationValuesModel));
            }

            AuthorizationValuesModel = authorizationValuesModel;
        }

        public void ConfigureServicesForDataAdapter(ServiceCollection services)
        {
            services.AddTransient(_ => EnsureHelloWorldClient());
        }

        public Task<ConnectorMetamodel> GetConnectorMetamodelAsync()
        {
            var helloWorldClient = EnsureHelloWorldClient();

            var helloWorldMetamodelBuilder = new HelloWorldMetamodelBuilder(_logger, helloWorldClient, _connectorConfiguration);

            return helloWorldMetamodelBuilder.BuildAsync();
        }

        public Task<string> GetRedirectUrlAsync(string targetRedirectUri, string secret, AuthorizationValuesModel authorizationValuesModel, CultureInfo currentCulture)
        {
            throw new NotSupportedException();
        }

        private IHelloWorldClient EnsureHelloWorldClient()
        {
            return _helloWorldClient ??= new HelloWorldClient(
                _logger,
                _cache,
                _portalsContext,
                _httpClientFactory,
                _connectorConfiguration.ClientId,
                _connectorConfiguration.SiteUrl,
                () => AuthorizationValuesModel?.AccessToken);
        }

        public Task WarmCachesAsync(bool forceWarming = false)
        {
            return Task.CompletedTask;
        }
    }
}
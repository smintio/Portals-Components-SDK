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
    /// <summary>
    /// The Hello world connector itself
    /// </summary>
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

        /// <summary>
        /// Validates user input from the connector's UI page
        /// </summary>
        public Task PerformPostConfigurationChecksAsync(AuthorizationValuesModel authorizationValuesModel)
        {
            authorizationValuesModel.IdentityServerUrl = _connectorConfiguration.SiteUrl;

            authorizationValuesModel.ClientId = _connectorConfiguration.ClientId;
            authorizationValuesModel.ClientSecret = _connectorConfiguration.ClientSecret;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Initializes an authorization flow with the external system
        /// </summary>
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

        /// <summary>
        /// Ensures that after post-authorization the external system is accessible
        /// </summary>
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

        /// <summary>
        /// This method is called periodically by the Smint.Io infrastructure to obtain new tokens when an external system requires it
        /// </summary>
        public Task<AuthorizationValuesModel> RefreshAuthorizationValuesAsync(AuthorizationValuesModel authorizationValuesModel)
        {
            if (string.IsNullOrEmpty(authorizationValuesModel.AccessToken) || string.IsNullOrEmpty(authorizationValuesModel.RefreshToken))
            {
                return InitializeAuthorizationValuesAsync(originalSecret: null, secret: null, authorizationValuesModel);
            }

            // Implement get access token request

            return Task.FromResult(new AuthorizationValuesModel
            {
                AccessToken = Guid.NewGuid().ToString(),
                RefreshToken = Guid.NewGuid().ToString(),
                ExpiresAt = DateTime.UtcNow.AddHours(1),
            });
        }

        /// <summary>
        /// Final modification of authorization values before storing them in the Smint.Io database
        /// </summary>
        public void SetAuthorizationValuesModel(AuthorizationValuesModel authorizationValuesModel)
        {
            if (authorizationValuesModel is null)
            {
                throw new ArgumentNullException(nameof(authorizationValuesModel));
            }

            AuthorizationValuesModel = authorizationValuesModel;
        }

        /// <summary>
        /// <see cref="IServiceCollection"/> injection hook used by the data adapter
        /// </summary>
        public void ConfigureServicesForDataAdapter(ServiceCollection services)
        {
            services.AddTransient(_ => EnsureHelloWorldClient());
        }

        /// <summary>
        /// Representation of the external system's metadata into Smint.Io as <see cref="ConnectorMetamodel"/>
        /// </summary>
        public Task<ConnectorMetamodel> GetConnectorMetamodelAsync()
        {
            var helloWorldClient = EnsureHelloWorldClient();

            var helloWorldMetamodelBuilder = new HelloWorldMetamodelBuilder(_logger, helloWorldClient, _connectorConfiguration);

            return helloWorldMetamodelBuilder.BuildAsync();
        }

        /// <summary>
        /// Call back url called when the `ConnectorSetupMethod` is set to `Redirect`
        /// Usually used by `OAuth2Connector` connectors 
        /// </summary>
        public Task<string> GetRedirectUrlAsync(string targetRedirectUri, string secret, AuthorizationValuesModel authorizationValuesModel, CultureInfo currentCulture)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Ensure that `IHelloWorldClient` has a valid instance when requested
        /// </summary>
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

        /// <summary>
        /// A connector hook used for caching purposes
        /// Please feel free to ask any of the Smint.Io team members for more information
        /// </summary>
        public Task WarmCachesAsync(bool forceWarming = false)
        {
            return Task.CompletedTask;
        }
    }
}
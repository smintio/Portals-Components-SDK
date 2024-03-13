using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Picturepark.SDK.V1;
using Picturepark.SDK.V1.Authentication;
using Picturepark.SDK.V1.Contract;
using RestSharp;
using SmintIo.Portals.Connector.Picturepark.Client;
using SmintIo.Portals.Connector.Picturepark.Client.Impl;
using SmintIo.Portals.Connector.Picturepark.Metamodel;
using SmintIo.Portals.Connector.Picturepark.Model;
using SmintIo.Portals.SDK.Core.Http.Prefab.RetryPolicies;
using SmintIo.Portals.ConnectorSDK.Connectors.Prefab;
using SmintIo.Portals.ConnectorSDK.Metamodel;
using SmintIo.Portals.ConnectorSDK.Models;
using SmintIo.Portals.SDK.Core.Cache;
using SmintIo.Portals.SDK.Core.Configuration;
using SmintIo.Portals.SDK.Core.Models.Context;
using SmintIo.Portals.SDK.Core.Rest.Prefab;
using SmintIo.Portals.SDK.Core.Rest.Prefab.Exceptions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SmintIo.Portals.Connector.Picturepark
{
    public class PictureparkConnector : OAuth2AuthenticationCodeFlowWithPKCEConnector
    {
        private static string[] StaticPassThroughAuthenticationIdentityProviderKeys = new[] { "picturepark_ids" };

        public override string Key => PictureparkConnectorStartup.PictureparkConnector;

        public override string[] PassThroughAuthenticationIdentityProviderKeys => StaticPassThroughAuthenticationIdentityProviderKeys;

        private const string BaseUrlKey = "base_url";
        private const string CustomerIdKey = "customer_id";
        private const string CustomerAliasKey = "customer_alias";
        private const string ApiUrlKey = "api_url";

        private const string ThumbnailPortalPresent = "thumbnail_portal_present";
        private const string ThumbnailExtraLargePresent = "thumbnail_extra_large_present";

        public override string[] RequiredScopes { get; } =
        [
            "all_scopes",
            "channel_read",
            "schema_read",
            "openid",
            "profile",
            "picturepark_api",
            "offline_access"
        ];

        private readonly IPortalsContextModel _portalsContext;

        private readonly PictureparkConnectorConfiguration _configuration;

        private DefaultPictureparkClient _pictureparkClient;

        private readonly IHttpClientFactory _httpClientFactory;

        private readonly ILogger _logger;

        private readonly ICache _cache;

        public PictureparkConnector(IHttpClientFactory httpClientFactory, PictureparkConnectorConfiguration configuration, ICache cache, ILogger logger, IServiceProvider serviceProvider) :
            base(null, httpClientFactory, logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _cache = cache;
            _logger = logger;

            _portalsContext = serviceProvider.GetService<IPortalsContextModel>();
        }

        public override Task<string> GetRedirectUrlAsync(string targetRedirectUri, string secret, AuthorizationValuesModel authorizationValuesModel, CultureInfo currentCulture)
        {
            return Task.FromResult<string>(null);
        }

        public override Task<AuthorizationValuesModel> InitializeAuthorizationValuesAsync(string originalSecret, string secret, AuthorizationValuesModel bootstrapAuthorizationValuesModel)
        {
            // Picturepark password flow

            bootstrapAuthorizationValuesModel.AccessToken = _configuration.AccessToken;

            return Task.FromResult(bootstrapAuthorizationValuesModel);
        }

        public override async Task PerformPostConfigurationChecksAsync(AuthorizationValuesModel authorizationValuesModel)
        {
            if (authorizationValuesModel is null)
                throw new ArgumentNullException(nameof(authorizationValuesModel));

            var pictureparkUrl = _configuration.PictureparkUrl;

            Uri uri = new Uri(pictureparkUrl);

            UriBuilder uriBuilder = new UriBuilder();
            uriBuilder.Scheme = uri.Scheme;
            uriBuilder.Host = uri.Host;
            uriBuilder.Port = uri.Port;

            var parsedBaseUrl = uriBuilder.Uri;

            var httpClient = _httpClientFactory.CreateClient();
            var restSharpClient = new RestSharpClient(httpClient, parsedBaseUrl);

            var request = new RestRequest($"/service/info/customer", Method.Get);

            request.AcceptApplicationJson();

            RestResponse<CustomerServiceInfoResponse> getResponse = null;

            try
            {
                getResponse = await new RestSharpRetryPolicy(Key, "Get customer service info", isGet: false, requestFailedHandler: null, portalsContextModel: null, _logger, maxRequestRetryCount: 0)
                    .ExecuteAsync((_) =>
                    {
                        return restSharpClient.ExecuteTaskAsync<CustomerServiceInfoResponse>(request);
                    }).ConfigureAwait(false);
            }
            catch (Exception)
            {
                throw new ExternalDependencyException(ExternalDependencyStatusEnum.EndpointUrlInvalid, "The Picturepark URL did not deliver valid customer service info", Key);
            }

            var data = getResponse.Data;

            var baseUrl = data.BaseUrl;
            if (string.IsNullOrEmpty(baseUrl))
                throw new ExternalDependencyException(ExternalDependencyStatusEnum.EndpointDataInvalid, "Picturepark response base URL is missing", Key);

            var customerId = data.CustomerId;
            if (string.IsNullOrEmpty(customerId))
                throw new ExternalDependencyException(ExternalDependencyStatusEnum.EndpointDataInvalid, "Picturepark response customer ID is missing", Key);

            var customerAlias = data.CustomerAlias;
            if (string.IsNullOrEmpty(customerAlias))
                throw new ExternalDependencyException(ExternalDependencyStatusEnum.EndpointDataInvalid, "Picturepark response customer alias is missing", Key);

            var identityServerUrl = data.IdentityServerUrl;
            if (string.IsNullOrEmpty(identityServerUrl))
                throw new ExternalDependencyException(ExternalDependencyStatusEnum.EndpointDataInvalid, "Picturepark response identity server URL is missing", Key);

            var apiUrl = data.ApiUrl;
            if (string.IsNullOrEmpty(apiUrl))
                throw new ExternalDependencyException(ExternalDependencyStatusEnum.EndpointDataInvalid, "Picturepark API URL is missing", Key);

            if (!apiUrl.EndsWith("/"))
            {
                apiUrl = $"{apiUrl}/";
            }

            authorizationValuesModel.KeyValueStore.Remove(BaseUrlKey);
            authorizationValuesModel.KeyValueStore.Add(BaseUrlKey, baseUrl);

            authorizationValuesModel.KeyValueStore.Remove(CustomerIdKey);
            authorizationValuesModel.KeyValueStore.Add(CustomerIdKey, customerId);

            authorizationValuesModel.KeyValueStore.Remove(CustomerAliasKey);
            authorizationValuesModel.KeyValueStore.Add(CustomerAliasKey, customerAlias);

            authorizationValuesModel.KeyValueStore.Remove(ApiUrlKey);
            authorizationValuesModel.KeyValueStore.Add(ApiUrlKey, apiUrl);

            authorizationValuesModel.IdentityServerUrl = identityServerUrl;

            if (data.OutputFormats != null && data.OutputFormats.Any(outputFormat => string.Equals(outputFormat.Id, "ThumbnailPortal", StringComparison.OrdinalIgnoreCase)))
            {
                authorizationValuesModel.KeyValueStore.Remove(ThumbnailPortalPresent);
                authorizationValuesModel.KeyValueStore.Add(ThumbnailPortalPresent, "true");
            }
            else
            {
                authorizationValuesModel.KeyValueStore.Remove(ThumbnailPortalPresent);
            }

            if (data.OutputFormats != null && data.OutputFormats.Any(outputFormat => string.Equals(outputFormat.Id, "ThumbnailExtraLarge", StringComparison.OrdinalIgnoreCase)))
            {
                authorizationValuesModel.KeyValueStore.Remove(ThumbnailExtraLargePresent);
                authorizationValuesModel.KeyValueStore.Add(ThumbnailExtraLargePresent, "true");
            }
            else
            {
                authorizationValuesModel.KeyValueStore.Remove(ThumbnailExtraLargePresent);
            }

            var tenant = new Dictionary<string, string>()
            {
                { "id", customerId },
                { "alias", customerAlias }
            };

            var acrValues = "tenant:" + JsonConvert.SerializeObject(tenant);

            authorizationValuesModel.AcrValues = acrValues;
        }

        public override async Task PerformPostAuthorizationChecksAsync(FormFieldValuesModel formFieldValuesModel)
        {
            if (formFieldValuesModel is null)
                throw new ArgumentNullException(nameof(formFieldValuesModel));

            var channelFormFieldValueModel = formFieldValuesModel.Values.FirstOrDefault(FormFieldValueModel => string.Equals(FormFieldValueModel.Id, nameof(PictureparkConnectorConfiguration.Channel)));

            string configuredChannelId = null;

            if (channelFormFieldValueModel != null)
            {
                configuredChannelId = channelFormFieldValueModel.StringValue;
            }
            else
            {
                configuredChannelId = _configuration.Channel;
            }

            if (!string.IsNullOrEmpty(configuredChannelId))
            {
                var channelId = await ValidateChannelIdAsync(configuredChannelId).ConfigureAwait(false);

                if (!string.Equals(channelId, configuredChannelId))
                {
                    channelFormFieldValueModel.StringValue = channelId;
                }
            }
            else
            {
                var defaultChannelId = await GetDefaultChannelIdAsync().ConfigureAwait(false);

                channelFormFieldValueModel.StringValue = defaultChannelId;
            }
        }

        private async Task<string> ValidateChannelIdAsync(string configuredChannelId)
        {
            var defaultPictureparkClient = CreatePictureparkClient();

            var channels = await defaultPictureparkClient.GetAllChannelsAsync().ConfigureAwait(false);

            if (channels == null || channels.Count == 0 || !channels.Any(channel => string.Equals(channel.Id, configuredChannelId)))
            {
                return await GetDefaultChannelIdAsync();
            }

            return configuredChannelId;
        }

        private async Task<string> GetDefaultChannelIdAsync()
        {
            var defaultPictureparkClient = CreatePictureparkClient();

            var channels = await defaultPictureparkClient.GetAllChannelsAsync().ConfigureAwait(false);

            var firstChannel = channels?.First();

            if (firstChannel == null)
            {
                throw new ExternalDependencyException(ExternalDependencyStatusEnum.CannotReadMetamodel, "No channel is available", Key);
            }

            var firstChannelId = firstChannel.Id;

            if (string.IsNullOrEmpty(firstChannelId))
            {
                throw new ExternalDependencyException(ExternalDependencyStatusEnum.CannotReadMetamodel, "Channel ID is empty", Key);
            }

            return firstChannelId;
        }

        public async Task<(string AccessToken, PictureparkService Service)> RefreshPassThroughAuthorizationValuesAsync(bool log = false)
        {
            AuthorizationValuesModel.ClientId = PassThroughAuthenticationTokenProvider.ClientId;

            AuthorizationValuesModel.AccessToken = PassThroughAuthenticationTokenProvider.AccessToken;
            AuthorizationValuesModel.RefreshToken = PassThroughAuthenticationTokenProvider.RefreshToken;

            if (log)
            {
                _logger.LogInformation($"Starting access token refresh with AT {AuthorizationValuesModel.AccessToken}, RT {AuthorizationValuesModel.RefreshToken}");
            }

            // here we call the BASE because we really need the OAuth flow!

            var refreshedAuthorizationValuesModel = await base.RefreshAuthorizationValuesAsync(AuthorizationValuesModel).ConfigureAwait(false);

            if (log)
            {
                _logger.LogInformation($"Successfully refreshed with {refreshedAuthorizationValuesModel.AccessToken}, RT {refreshedAuthorizationValuesModel.RefreshToken}");
            }

            await PassThroughAuthenticationTokenProvider.StoreUpdatedAccessAndRefreshTokenAsync(refreshedAuthorizationValuesModel.AccessToken, refreshedAuthorizationValuesModel.RefreshToken).ConfigureAwait(false);

            // recreate PP client

            var apiUrl = GetApiUrl();
            var customerAlias = GetCustomerAlias();

            var accessToken = GetAccessToken();

            var authClient = new AccessTokenAuthClient(apiUrl, accessToken, customerAlias);

            var settings = new PictureparkServiceSettings(authClient)
            {
                DisplayLanguage = CultureInfo.CurrentCulture.TwoLetterISOLanguageName
            };

            var httpClient = _httpClientFactory.CreateClient("api");

            var pictureparkService = new PictureparkService(settings, httpClient);

            return (accessToken, pictureparkService);
        }

        public override async Task<AuthorizationValuesModel> RefreshAuthorizationValuesAsync(AuthorizationValuesModel authorizationValuesModel)
        {
            // Picturepark password flow

            authorizationValuesModel.AccessToken = _configuration.AccessToken;

            // just update the customer service info

            var pictureparkUrl = _configuration.PictureparkUrl;

            Uri uri = new Uri(pictureparkUrl);

            UriBuilder uriBuilder = new UriBuilder();
            uriBuilder.Scheme = uri.Scheme;
            uriBuilder.Host = uri.Host;
            uriBuilder.Port = uri.Port;

            var parsedBaseUrl = uriBuilder.Uri;

            var httpClient = _httpClientFactory.CreateClient();
            var restSharpClient = new RestSharpClient(httpClient, parsedBaseUrl);

            var request = new RestRequest($"/service/info/customer", Method.Get);

            request.AcceptApplicationJson();

            RestResponse<CustomerServiceInfoResponse> getResponse = null;

            try
            {
                getResponse = await new RestSharpRetryPolicy(Key, "Get customer service info", isGet: false, requestFailedHandler: null, portalsContextModel: null, _logger, maxRequestRetryCount: 0)
                    .ExecuteAsync((_) =>
                    {
                        return restSharpClient.ExecuteTaskAsync<CustomerServiceInfoResponse>(request);
                    }).ConfigureAwait(false);
            }
            catch (Exception)
            {
                throw new ExternalDependencyException(ExternalDependencyStatusEnum.EndpointUrlInvalid, "The Picturepark URL did not deliver valid customer service info", Key);
            }

            var data = getResponse.Data;

            var newApiUrl = data.ApiUrl;
            if (string.IsNullOrEmpty(newApiUrl))
                throw new ExternalDependencyException(ExternalDependencyStatusEnum.EndpointDataInvalid, "Picturepark API URL is missing", Key);

            if (!newApiUrl.EndsWith("/"))
            {
                newApiUrl = $"{newApiUrl}/";
            }

            string oldApiUrl = null;
            if (authorizationValuesModel.KeyValueStore.ContainsKey(ApiUrlKey))
            {
                oldApiUrl = authorizationValuesModel.KeyValueStore[ApiUrlKey];
            }

            if (!string.Equals(newApiUrl, oldApiUrl, StringComparison.Ordinal))
            {
                authorizationValuesModel.KeyValueStore.Remove(ApiUrlKey);
                authorizationValuesModel.KeyValueStore.Add(ApiUrlKey, newApiUrl);
            }

            if (data.OutputFormats != null && data.OutputFormats.Any(outputFormat => string.Equals(outputFormat.Id, "ThumbnailPortal", StringComparison.OrdinalIgnoreCase)))
            {
                authorizationValuesModel.KeyValueStore.Remove(ThumbnailPortalPresent);
                authorizationValuesModel.KeyValueStore.Add(ThumbnailPortalPresent, "true");
            }
            else
            {
                authorizationValuesModel.KeyValueStore.Remove(ThumbnailPortalPresent);
            }

            if (data.OutputFormats != null && data.OutputFormats.Any(outputFormat => string.Equals(outputFormat.Id, "ThumbnailExtraLarge", StringComparison.OrdinalIgnoreCase)))
            {
                authorizationValuesModel.KeyValueStore.Remove(ThumbnailExtraLargePresent);
                authorizationValuesModel.KeyValueStore.Add(ThumbnailExtraLargePresent, "true");
            }
            else
            {
                authorizationValuesModel.KeyValueStore.Remove(ThumbnailExtraLargePresent);
            }

            return authorizationValuesModel;
        }

        public override void ConfigureServicesForDataAdapter(ServiceCollection services)
        {
            services.AddSingleton((serviceProvider) =>
            {
                return CreatePictureparkClient();
            });
        }

        public override async Task<ConnectorMetamodel> GetConnectorMetamodelAsync()
        {
            var pictureparkClient = CreatePictureparkClient();

            var pictureparkMetamodelBuilder = new PictureparkMetamodelBuilder(pictureparkClient);

            var pictureparkMetamodel = await pictureparkMetamodelBuilder.BuildAsync();

            return pictureparkMetamodel;
        }

        private IPictureparkClient CreatePictureparkClient()
        {
            if (_pictureparkClient != null)
                return _pictureparkClient;

            var apiUrl = GetApiUrl();
            var customerAlias = GetCustomerAlias();

            var thumbnailPortalPresent = GetThumbnailPortalPresent();
            var thumbnailExtraLargePresent = GetThumbnailExtraLargePresent();

            var accessToken = GetAccessToken();

            var authClient = new AccessTokenAuthClient(apiUrl, accessToken, customerAlias);

            var settings = new PictureparkServiceSettings(authClient)
            {
                DisplayLanguage = CultureInfo.CurrentCulture.TwoLetterISOLanguageName
            };

            var httpClient = _httpClientFactory.CreateClient("api");

            var pictureparkService = new PictureparkService(settings, httpClient);

            _pictureparkClient = new DefaultPictureparkClient(
                this, 
                _portalsContext, 
                accessToken, 
                pictureparkService, 
                _configuration.Channel, 
                _cache, 
                _httpClientFactory, 
                httpClient, 
                apiUrl, 
                customerAlias, 
                thumbnailPortalPresent, 
                thumbnailExtraLargePresent, 
                _configuration.LegacyThumbnailsEnabled,
                _logger);

            return _pictureparkClient;
        }

        private string GetApiUrl()
        {
            if (AuthorizationValuesModel == null)
                throw new ArgumentNullException(nameof(AuthorizationValuesModel));

            if (!AuthorizationValuesModel.KeyValueStore.ContainsKey(ApiUrlKey))
                throw new ExternalDependencyException(ExternalDependencyStatusEnum.NotSetup, "The API URL is missing", Key);

            var apiUrl = AuthorizationValuesModel.KeyValueStore[ApiUrlKey];

            if (string.IsNullOrEmpty(apiUrl))
                throw new ExternalDependencyException(ExternalDependencyStatusEnum.NotSetup, "The API URL is empty", Key);

            return apiUrl;
        }

        private string GetCustomerId()
        {
            if (AuthorizationValuesModel == null)
                throw new ArgumentNullException(nameof(AuthorizationValuesModel));

            if (!AuthorizationValuesModel.KeyValueStore.ContainsKey(CustomerIdKey))
                throw new ExternalDependencyException(ExternalDependencyStatusEnum.NotSetup, "The customer ID is missing", Key);

            var customerId = AuthorizationValuesModel.KeyValueStore[CustomerIdKey];

            if (string.IsNullOrEmpty(customerId))
                throw new ExternalDependencyException(ExternalDependencyStatusEnum.NotSetup, "The customer ID is empty", Key);

            return customerId;
        }

        private string GetCustomerAlias()
        {
            if (AuthorizationValuesModel == null)
                throw new ArgumentNullException(nameof(AuthorizationValuesModel));

            if (!AuthorizationValuesModel.KeyValueStore.ContainsKey(CustomerAliasKey))
                throw new ExternalDependencyException(ExternalDependencyStatusEnum.NotSetup, "The customer alias is missing", Key);

            var customerAlias = AuthorizationValuesModel.KeyValueStore[CustomerAliasKey];

            if (string.IsNullOrEmpty(customerAlias))
                throw new ExternalDependencyException(ExternalDependencyStatusEnum.NotSetup, "The customer alias is empty", Key);

            return customerAlias;
        }

        private bool GetThumbnailPortalPresent()
        {
            if (AuthorizationValuesModel == null)
                return false;

            return AuthorizationValuesModel.KeyValueStore.ContainsKey(ThumbnailPortalPresent);
        }

        private bool GetThumbnailExtraLargePresent()
        {
            if (AuthorizationValuesModel == null)
                return false;

            return AuthorizationValuesModel.KeyValueStore.ContainsKey(ThumbnailExtraLargePresent);
        }

        public override Task WarmCachesAsync(bool forceWarming = false)
        {
            return Task.CompletedTask;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RestSharp;
using SmintIo.Portals.Connector.SharePoint.Client;
using SmintIo.Portals.Connector.SharePoint.Client.Impl;
using SmintIo.Portals.Connector.SharePoint.MicrosoftGraph.Metamodel;
using SmintIo.Portals.SDK.Core.Http.Prefab.RetryPolicies;
using SmintIo.Portals.ConnectorSDK.Connectors.Prefab;
using SmintIo.Portals.ConnectorSDK.Metamodel;
using SmintIo.Portals.ConnectorSDK.Models;
using SmintIo.Portals.ConnectorSDK.Models.Prefab;
using SmintIo.Portals.SDK.Core.Cache;
using SmintIo.Portals.SDK.Core.Configuration;
using SmintIo.Portals.SDK.Core.Models.Context;
using SmintIo.Portals.SDK.Core.Rest.Prefab;
using SmintIo.Portals.SDK.Core.Rest.Prefab.Exceptions;
using SmintIo.Portals.SDK.Core.Http.Prefab.Exceptions;

namespace SmintIo.Portals.Connector.SharePoint
{
    public class SharepointConnector : OAuth2AuthenticationCodeFlowWithPKCEConnector
    {
        public const string SharepointAccessTokenKey = "sharePointAccessToken";

        private const string DefaultClientId = "deb473a8-8ee0-40c5-aa5d-5d821d308c4f";
        private const string DefaultClientSecret = "RsF8Q~rEA5CZuEofJz1xzB6Bv-fWw4m1Mm2NNa4I";

        private const string IdentityServerUrl = "https://login.microsoftonline.com";
        private const string MicrosoftGraphUrl = "https://graph.microsoft.com";

        private readonly ILogger _logger;
        private readonly ICache _cache;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly SharepointConnectorConfiguration _configuration;
        private readonly IPortalsContextModel _portalsContext;

        private SharepointClient _sharepointClient;

        public SharepointConnector(
            IServiceProvider serviceProvider,
            ILogger logger,
            ICache cache,
            IHttpClientFactory httpClientFactory,
            SharepointConnectorConfiguration configuration)
            : base(null, httpClientFactory, logger)
        {
            _logger = logger;
            _cache = cache;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;

            _portalsContext = serviceProvider.GetService<IPortalsContextModel>();
        }

        public override string Key => SharepointConnectorStartup.SharepointConnector;

        public override string[] PassThroughAuthenticationIdentityProviderKeys => null;

        public AuthorizationValuesModel GetAuthorizationValues()
        {
            return AuthorizationValuesModel;
        }

        /// <summary>
        /// OK, its strange - but we had to do this for backwards compatibility reasons
        /// to make the client ID configuration override work that we temporarily needed
        /// until our Sharepoint App registration passed through Microsoft
        /// </summary>
        /// <param name="authorizationValuesModel"></param>
        /// <returns></returns>
        protected override string GetClientId(AuthorizationValuesModel authorizationValuesModel)
        {
            var clientId = authorizationValuesModel?.ClientId;

            if (!string.IsNullOrEmpty(clientId))
            {
                return base.GetClientId(authorizationValuesModel);
            }

            return DefaultClientId;
        }

        /// <summary>
        /// OK, its strange - but we had to do this for backwards compatibility reasons
        /// to make the client ID configuration override work that we temporarily needed
        /// until our Sharepoint App registration passed through Microsoft
        /// </summary>
        /// <param name="authorizationValuesModel"></param>
        /// <returns></returns>
        protected override string GetClientSecret(AuthorizationValuesModel authorizationValuesModel)
        {
            var clientSecret = authorizationValuesModel?.ClientSecret;

            if (!string.IsNullOrEmpty(clientSecret))
            {
                return base.GetClientSecret(authorizationValuesModel);
            }

            return DefaultClientSecret;
        }

        public override Task PerformPostConfigurationChecksAsync(AuthorizationValuesModel authorizationValuesModel)
        {
            authorizationValuesModel.IdentityServerUrl = IdentityServerUrl;

            if (_configuration.HighSecurityMode && !string.IsNullOrEmpty(_configuration.ClientId))
            {
                authorizationValuesModel.ClientId = _configuration.ClientId;
            }
            else
            {
                authorizationValuesModel.ClientId = null;
            }

            if (_configuration.HighSecurityMode && !string.IsNullOrEmpty(_configuration.ClientSecret))
            {
                authorizationValuesModel.ClientSecret = _configuration.ClientSecret;
            }
            else
            {
                authorizationValuesModel.ClientSecret = null;
            }

            authorizationValuesModel.Scope = string.Join(" ", RequiredScopes);

            return Task.CompletedTask;
        }

        public override async Task PerformPostAuthorizationChecksAsync(FormFieldValuesModel formFieldValuesModel)
        {
            var sharepointClient = EnsureSharepointClient();

            try
            {
                await sharepointClient.GetSitesAsync().ConfigureAwait(false);
            }
            catch (Exception)
            {
                throw new ExternalDependencyException(ExternalDependencyStatusEnum.EndpointTestFailed, "The SharePoint endpoint test failed", SharepointConnectorStartup.SharepointConnector);
            }
        }

        public override void ConfigureServicesForDataAdapter(ServiceCollection services)
        {
            services.AddTransient(_ => EnsureSharepointClient());
        }

        public override Task<ConnectorMetamodel> GetConnectorMetamodelAsync()
        {
            var sharepointClient = EnsureSharepointClient();

            string siteId;

            if (_configuration.HighSecurityMode)
            {
                siteId = _configuration.SiteIdString;
            }
            else
            {
                siteId = _configuration.SiteId;
            }
                
            var siteDriveId = _configuration.HighSecurityMode
                ? _configuration.SiteDriveIdString
                : _configuration.SiteDriveId;

            var siteListId = _configuration.HighSecurityMode
                ? _configuration.SiteListIdString
                : _configuration.SiteListId;

            var siteFolderIds = GetSiteFolderIds();

            var sharepointMetamodelBuilder = new SharepointMetamodelBuilder(_logger, sharepointClient, siteId, siteDriveId, siteListId, siteFolderIds);

            return sharepointMetamodelBuilder.BuildAsync();
        }

        private IEnumerable<string> GetSiteFolderIds()
        {
            var siteFolderIds = _configuration.HighSecurityMode
                ? _configuration.SiteFolderIdsStrings
                : _configuration.SiteFolderIds;

            return siteFolderIds ?? Enumerable.Empty<string>();
        }

        public override Task<string> GetRedirectUrlAsync(
            string targetRedirectUri,
            string secret,
            AuthorizationValuesModel authorizationValuesModel,
            CultureInfo currentCulture)
        {
            var encodedClientId = HttpUtility.UrlEncode(GetClientId(authorizationValuesModel));

            // the host for the /authorize and /token requests
            var idSrvUrl = GetIdentityServerUrl(authorizationValuesModel);

            var scopes = string.Join(" ", RequiredScopes);
            var encodedScopes = HttpUtility.UrlEncode(scopes);

            // Microsoft Identity requires the redirect uri
            var encodedRedirectUri = HttpUtility.UrlEncode(targetRedirectUri);

            var tenantId = _configuration.HighSecurityMode ? _configuration.TenantId : "common";

            var authorizeEndpoint =
                $"{idSrvUrl}/{tenantId}/oauth2/v2.0/authorize?client_id={encodedClientId}&response_type=code&redirect_uri={encodedRedirectUri}&response_mode=query&scope={encodedScopes}&state={HttpUtility.UrlEncode(secret)}";

            authorizationValuesModel.OriginalRedirectUrl = targetRedirectUri;

            return Task.FromResult(authorizeEndpoint);
        }

        public override async Task<AuthorizationValuesModel> InitializeAuthorizationValuesAsync(
            string originalSecret,
            string secret,
            AuthorizationValuesModel bootstrapAuthorizationValuesModel)
        {
            HandleInitializationError(originalSecret, secret, bootstrapAuthorizationValuesModel);

            var clientId = GetClientId(bootstrapAuthorizationValuesModel);

            var identityServerUrl = GetIdentityServerUrl(bootstrapAuthorizationValuesModel);
            var code = GetCode(bootstrapAuthorizationValuesModel);
            var originalRedirectUrl = GetOriginalRedirectUrl(bootstrapAuthorizationValuesModel);

            var httpClient = _httpClientFactory.CreateClient();
            var restSharpClient = new RestSharpClient(httpClient, new Uri(identityServerUrl));

            var tenantId = _configuration.HighSecurityMode ? _configuration.TenantId : "common";

            var request = new RestRequest($"/{tenantId}/oauth2/v2.0/token", Method.Post);

            // Microsoft Identity Platform requires that the redirect_uri be present
            request.AddParameter("client_id", clientId, ParameterType.GetOrPost);
            request.AddParameter("code", code, ParameterType.GetOrPost);
            request.AddParameter("grant_type", "authorization_code", ParameterType.GetOrPost);
            request.AddParameter("redirect_uri", originalRedirectUrl, ParameterType.GetOrPost);
            request.AddParameter("client_secret", GetClientIdAndSecret(bootstrapAuthorizationValuesModel).ClientSecret);
            request.AddParameter("scope", $"offline_access {MicrosoftGraphUrl}/Sites.Read.All");

            request.AcceptApplicationJson();

            var postResponse = await new RestSharpRetryPolicy(Key, "Get access token by authorization code", isGet: false, requestFailedHandler: null, portalsContextModel: null, _logger, maxRequestRetryCount: 0)
                .ExecuteAsync((_) =>
                {
                    return restSharpClient.ExecuteTaskAsync<OAuth2GetAccessTokenResponse>(request);
                }).ConfigureAwait(false);

            var data = postResponse.Data;

            var accessToken = data.AccessToken;

            if (string.IsNullOrEmpty(accessToken))
            {
                throw new ExternalDependencyException(
                    ExternalDependencyStatusEnum.NoAccessToken,
                      "The OAuth2 service did not deliver a access token", Key);
            }

            var refreshToken = data.RefreshToken;

            if (string.IsNullOrEmpty(refreshToken))
            {
                throw new ExternalDependencyException(
                    ExternalDependencyStatusEnum.NoRefreshToken,
                   "The OAuth2 service did not deliver a refresh token", Key);
            }

            bootstrapAuthorizationValuesModel.AccessToken = accessToken;
            bootstrapAuthorizationValuesModel.RefreshToken = refreshToken;
            bootstrapAuthorizationValuesModel.ExpiresAt = data.ExpiresAt;

            return await RefreshAuthorizationValuesAsync(bootstrapAuthorizationValuesModel).ConfigureAwait(false);
        }

        public override async Task<AuthorizationValuesModel> RefreshAuthorizationValuesAsync(AuthorizationValuesModel authorizationValuesModel)
        {
            var identityServerUrl = GetIdentityServerUrl(authorizationValuesModel);
            var (clientId, secret) = GetClientIdAndSecret(authorizationValuesModel);
            var redirectUri = GetOriginalRedirectUrl(authorizationValuesModel);

            var refreshToken = GetRefreshToken(authorizationValuesModel);

            var graphApiAccessTokenResponse = await GetAccessTokenResponseAsync(
                identityServerUrl,
                clientId,
                secret,
                redirectUri,
                refreshToken,
                isSharepointApiRequest: false)
                .ConfigureAwait(false);

            authorizationValuesModel.AccessToken = graphApiAccessTokenResponse.AccessToken;

            if (!string.IsNullOrEmpty(graphApiAccessTokenResponse.RefreshToken))
            {
                authorizationValuesModel.RefreshToken = graphApiAccessTokenResponse.RefreshToken;
            }

            authorizationValuesModel.ExpiresAt = graphApiAccessTokenResponse.ExpiresAt;

            var sharePointApiAccessTokenResponse = await GetAccessTokenResponseAsync(
                identityServerUrl,
                clientId,
                secret,
                redirectUri,
                refreshToken,
                isSharepointApiRequest: true)
                .ConfigureAwait(false);

            authorizationValuesModel.KeyValueStore[SharepointAccessTokenKey] = sharePointApiAccessTokenResponse.AccessToken;

            if (!string.IsNullOrEmpty(sharePointApiAccessTokenResponse.RefreshToken))
            {
                authorizationValuesModel.RefreshToken = sharePointApiAccessTokenResponse.RefreshToken;
            }

            authorizationValuesModel.ExpiresAt = sharePointApiAccessTokenResponse.ExpiresAt;

            return authorizationValuesModel;
        }

        /// <summary>
        /// If "offline_access" is not specified here, won't return a refresh token in the <code>/token</code> request. 
        /// </summary>
        public override string[] RequiredScopes =>
            _configuration.HighSecurityMode ?
                // low scope requirements

                new[]
                {
                    "offline_access",
                    $"{MicrosoftGraphUrl}/Sites.Read.All",
                    $"{_configuration.SharepointUrl}/AllSites.Read"
                }

                :

                // we can request more scopes

                new[]
                {
                    "offline_access",
                    $"{MicrosoftGraphUrl}/Sites.Read.All",
                    $"{_configuration.SharepointUrl}/AllSites.Read",
                    $"{_configuration.SharepointUrl}/Sites.Search.All"
                };

        private async Task<OAuth2GetAccessTokenResponse> GetAccessTokenResponseAsync(
            string identityServerUrl,
            string clientId,
            string secret,
            string redirectUri,
            string refreshToken,
            bool isSharepointApiRequest)
        {
            var tenantId = _configuration.HighSecurityMode ? _configuration.TenantId : "common";

            var request = new RestRequest($"/{tenantId}/oauth2/v2.0/token", Method.Post);

            request.AddParameter("client_id", clientId, ParameterType.GetOrPost);
            request.AddParameter("refresh_token", refreshToken, ParameterType.GetOrPost);
            request.AddParameter("grant_type", "refresh_token", ParameterType.GetOrPost);
            request.AddParameter("redirect_uri", redirectUri, ParameterType.GetOrPost);
            request.AddParameter("client_secret", secret);

            if (_configuration.HighSecurityMode)
            {
                // low scope requirements

                var scopes = isSharepointApiRequest
                    ? $"offline_access {_configuration.SharepointUrl}/AllSites.Read"
                    : $"offline_access {MicrosoftGraphUrl}/Sites.Read.All";

                request.AddParameter("scope", scopes);
            }
            else
            {
                // we can request more scopes

                var scopes = isSharepointApiRequest
                    ? $"offline_access {_configuration.SharepointUrl}/AllSites.Read {_configuration.SharepointUrl}/Sites.Search.All"
                    : $"offline_access {MicrosoftGraphUrl}/Sites.Read.All";

                request.AddParameter("scope", scopes);
            }

            var httpClient = _httpClientFactory.CreateClient();
            var restSharpClient = new RestSharpClient(httpClient, new Uri(identityServerUrl));

            request.AcceptApplicationJson();

            RestResponse<OAuth2GetAccessTokenResponse> postResponse = null;

            try
            {
                postResponse = await new RestSharpRetryPolicy(Key, "Refresh access token with refresh token", isGet: false, requestFailedHandler: null, portalsContextModel: null, _logger, maxRequestRetryCount: 0)
                    .ExecuteAsync((_) =>
                    {
                        return restSharpClient.ExecuteTaskAsync<OAuth2GetAccessTokenResponse>(request);
                    }).ConfigureAwait(false);
            }
            catch (ExternalDependencyException e)
            when (e.ErrorCode == ExternalDependencyStatusEnum.CompatiblityIssue)
            {
                if (postResponse != null &&
                    !string.IsNullOrEmpty(postResponse.Content) &&
                    postResponse.Content.IndexOf("invalid_grant") > -1)
                {
                    // this is delivered by OAuth clients when grants are expired

                    throw new ExternalDependencyException(
                        ExternalDependencyStatusEnum.AuthorizationValuesExpired,
                        "The authorization expired. Please re-authorize",
                        Key);
                }

                if (e.InnerException != null &&
                    e.InnerException is HttpException httpException &&
                    !string.IsNullOrEmpty(httpException.ResponseBody) &&
                    httpException.ResponseBody.IndexOf("invalid_grant") > -1)
                {
                    throw new ExternalDependencyException(
                        ExternalDependencyStatusEnum.AuthorizationValuesExpired,
                        "The authorization expired. Please re-authorize",
                        Key);
                }

                throw;
            }

            if (string.IsNullOrEmpty(postResponse.Data.AccessToken))
            {
                var noAccessTokenText = isSharepointApiRequest
                    ? "SharePoint Api"
                    : "Graph Api";

                throw new ExternalDependencyException(
                      ExternalDependencyStatusEnum.NoAccessToken,
                      $"The OAuth2 service did not deliver a {noAccessTokenText} access token.",
                      Key);
            }

            return postResponse.Data;
        }

        private ISharepointClient EnsureSharepointClient()
        {
            string siteId;

            if (_configuration.HighSecurityMode)
            {
                siteId = _configuration.SiteIdString;
            }
            else
            {
                siteId = _configuration.SiteId;
            }

            var siteDriveId = _configuration.HighSecurityMode
                ? _configuration.SiteDriveIdString
                : _configuration.SiteDriveId;

            var siteListId = _configuration.HighSecurityMode
                ? _configuration.SiteListIdString
                : _configuration.SiteListId;

            var siteFolderIds = GetSiteFolderIds();

            return _sharepointClient ??= new SharepointClient(
                _logger,
                _cache,
                _portalsContext,
                _httpClientFactory,
                () => AuthorizationValuesModel,
                _configuration.SharepointUrl,
                siteId,
                siteDriveId,
                siteListId,
                siteFolderIds);
        }

        public override Task WarmCachesAsync(bool forceWarming = false)
        {
            return Task.CompletedTask;
        }
    }
}
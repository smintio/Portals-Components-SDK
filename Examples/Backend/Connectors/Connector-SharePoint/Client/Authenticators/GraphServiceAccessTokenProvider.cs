using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions.Authentication;
using SmintIo.Portals.ConnectorSDK.Models;

namespace SmintIo.Portals.Connector.SharePoint.Client.Authenticators
{
    public class GraphServiceAccessTokenProvider(Func<AuthorizationValuesModel> getAuthorizationValuesFunc) : IAccessTokenProvider
    {
        private readonly Func<AuthorizationValuesModel> _getAuthorizationValuesFunc = getAuthorizationValuesFunc;

        public AllowedHostsValidator AllowedHostsValidator { get; }

        public Task<string> GetAuthorizationTokenAsync(Uri uri, Dictionary<string, object> additionalAuthenticationContext = null, CancellationToken cancellationToken = default)
        {
            var authorizationValuesModel = _getAuthorizationValuesFunc();

            return Task.FromResult(authorizationValuesModel.AccessToken);
        }
    }
}

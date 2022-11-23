using System;
using RestSharp;
using RestSharp.Authenticators;
using SmintIo.Portals.ConnectorSDK.Models;

namespace SmintIo.Portals.Connector.SharePoint.Client.Authenticators
{
    public class SharepointBearerAuthenticator : IAuthenticator
    {
        private readonly Func<AuthorizationValuesModel> _getAuthorizationValuesFunc;

        public SharepointBearerAuthenticator(Func<AuthorizationValuesModel> getAuthorizationValuesFunc)
        {
            _getAuthorizationValuesFunc = getAuthorizationValuesFunc;
        }

        public void Authenticate(IRestClient client, IRestRequest request)
        {
            var authorizationValues = _getAuthorizationValuesFunc();
            var sharepointAccessToken = authorizationValues.KeyValueStore[SharepointConnector.SharepointAccessTokenKey];

            request.AddHeader("Authorization", $"Bearer {sharepointAccessToken}");
        }
    }
}
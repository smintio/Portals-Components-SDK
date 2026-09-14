using System;
using System.Threading.Tasks;
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

        public ValueTask Authenticate(IRestClient client, RestRequest request)
        {
            var authorizationValues = _getAuthorizationValuesFunc();
            var sharepointAccessToken = authorizationValues.KeyValueStore[SharepointConnector.SharepointAccessTokenKey];

            request.AddHeader("Authorization", $"Bearer {sharepointAccessToken}");

            return ValueTask.CompletedTask;
        }
    }
}
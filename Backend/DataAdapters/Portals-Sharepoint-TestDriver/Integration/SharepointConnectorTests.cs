using System;
using System.Threading.Tasks;
using FluentAssertions;
using SmintIo.Portals.Connector.SharePoint;
using SmintIo.Portals.Connector.Test.Tests;
using SmintIo.Portals.ConnectorSDK.TestDriver.Sharepoint.Test.Harness;
using SmintIo.Portals.DataAdapterSDK.TestDriver.Models;
using Xunit;
using Xunit.Abstractions;

namespace SmintIo.Portals.ConnectorSDK.TestDriver.Sharepoint.Test.Integration
{
    [Collection(nameof(SharepointFixtureCollection))]
    public class SharepointConnectorTests : ConnectorTests<SharepointConnector>
    {
        private readonly ITestOutputHelper _helper;
        protected readonly SharepointFixture _fixture;

        public SharepointConnectorTests(ITestOutputHelper helper, SharepointFixture fixture)
        {
            _helper = helper;
            _fixture = fixture;
        }

        protected override SharepointConnector GetConnector(Type componentType) => _fixture.Connector;

        protected AssetOptions Options => _fixture.AssetOptions;

        protected OAuthOptions OauthConfig => _fixture.ConfigurationOptions;

        [Fact]
        public void ConnectorShouldNotBeNull()
        {
            _fixture.Connector.Should().NotBeNull();
        }

        [Fact]
        public void ConnectorShouldHaveAuthorizationData()
        {
            var authorizationValues = _fixture.Connector.GetAuthorizationValues();

            authorizationValues.AccessToken.Should().NotBeEmpty();
            authorizationValues.RefreshToken.Should().NotBeEmpty();

            authorizationValues.KeyValueStore.Count.Should().BePositive();
            authorizationValues.KeyValueStore[SharepointConnector.SharepointAccessTokenKey]
                .Should()
                .NotBeEmpty();

            authorizationValues.OriginalRedirectUrl.Should().NotBeEmpty();
        }

        [Fact]
        public async Task ConnectorShouldBeAbleToRefreshAuthorizationValuesAsync()
        {
            var authorizationValues = _fixture.Connector.GetAuthorizationValues();

            var graphApiAccessToken = authorizationValues.AccessToken;
            var graphApiRefreshToken = authorizationValues.RefreshToken;
            var restApiAccessToken = authorizationValues.KeyValueStore[SharepointConnector.SharepointAccessTokenKey];

            var authorizationValuesResult = await _fixture.Connector.RefreshAuthorizationValuesAsync(authorizationValues);

            authorizationValuesResult.AccessToken.Should().NotMatch(graphApiAccessToken);
            authorizationValuesResult.RefreshToken.Should().NotMatch(graphApiRefreshToken);
            authorizationValues.KeyValueStore[SharepointConnector.SharepointAccessTokenKey].Should().NotMatch(restApiAccessToken);
        }

        [Fact]
        public void ConnectorShouldHaveRequiredScopes()
        {
            var requiredScopes = _fixture.Connector.RequiredScopes;

            requiredScopes.Should().NotBeEmpty();

            requiredScopes.Should().ContainMatch("offline_access");
            requiredScopes.Should().ContainMatch("*/Sites.Read.All");
            requiredScopes.Should().ContainMatch("*/AllSites.Read");
            requiredScopes.Should().ContainMatch("*/Sites.Search.All");
        }
    }
}
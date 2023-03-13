using System;
using FluentAssertions;
using SmintIo.Portals.Connector.HelloWorld;
using SmintIo.Portals.Connector.Test.Tests;
using SmintIo.Portals.ConnectorSDK.TestDriver.HelloWorld.Test.Harness;
using SmintIo.Portals.DataAdapterSDK.TestDriver.Models;
using Xunit;
using Xunit.Abstractions;

namespace SmintIo.Portals.ConnectorSDK.TestDriver.HelloWorld.Test.Integration
{
    [Collection(nameof(HelloWorldFixtureCollection))]
    public class HelloWorldConnectorTests : ConnectorTests<HelloWorldConnector>
    {
        private readonly ITestOutputHelper _helper;
        protected readonly HelloWorldFixture _fixture;

        public HelloWorldConnectorTests(ITestOutputHelper helper, HelloWorldFixture fixture)
        {
            _helper = helper;
            _fixture = fixture;
        }

        protected override HelloWorldConnector GetConnector(Type componentType) => _fixture.Connector;

        protected AssetOptions Options => _fixture.AssetOptions;

        protected OAuthOptions OAuthConfig => _fixture.ConfigurationOptions;

        [Fact]
        public void ConnectorShouldNotBeNull()
        {
            _fixture.Connector.Should().NotBeNull();
        }

        [Fact]
        public void ConnectorShouldHaveAuthorizationData()
        {
            var authorizationValues = _fixture.Connector.AuthorizationValuesModel;

            authorizationValues.AccessToken.Should().NotBeEmpty();
            authorizationValues.RefreshToken.Should().NotBeEmpty();

            authorizationValues.OriginalRedirectUrl.Should().NotBeEmpty();
        }
    }
}
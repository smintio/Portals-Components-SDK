using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using SmintIo.Portals.Connector.SharePoint;
using SmintIo.Portals.ConnectorSDK.Metamodel;
using SmintIo.Portals.ConnectorSDK.TestDriver.Sharepoint.Test.Harness;
using SmintIo.Portals.SDK.Core.Models.Metamodel.Model;
using Xunit;
using Xunit.Abstractions;

namespace SmintIo.Portals.ConnectorSDK.TestDriver.Sharepoint.Test.Integration
{
    [Collection("SharepointCollection")]
    public class SharepointConnectorTest
    {
        private readonly ITestOutputHelper _helper;
        protected readonly SharepointFixture Fixture;

        protected SharepointOptions SharepointOptions => Fixture.SharepointOptions;
        protected OauthOptions OauthConfig => Fixture.OauthConfig;

        public SharepointConnectorTest(ITestOutputHelper helper, SharepointFixture fixture)
        {
            _helper = helper;
            Fixture = fixture;
        }

        [Fact]
        public void ConnectorShouldNotBeNull()
        {
            Fixture.Connector.Should().NotBeNull();
        }

        [Fact]
        public void ConnectorShouldHaveAuthorizationData()
        {
            var authorizationValues = Fixture.Connector.GetAuthorizationValues();

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
            var authorizationValues = Fixture.Connector.GetAuthorizationValues();

            var graphApiAccessToken = authorizationValues.AccessToken;
            var graphApiRefreshToken = authorizationValues.RefreshToken;
            var restApiAccessToken = authorizationValues.KeyValueStore[SharepointConnector.SharepointAccessTokenKey];

            var authorizationValuesResult = await Fixture.Connector.RefreshAuthorizationValuesAsync(authorizationValues);

            authorizationValuesResult.AccessToken.Should().NotMatch(graphApiAccessToken);
            authorizationValuesResult.RefreshToken.Should().NotMatch(graphApiRefreshToken);
            authorizationValues.KeyValueStore[SharepointConnector.SharepointAccessTokenKey].Should().NotMatch(restApiAccessToken);
        }

        [Fact]
        public void ConnectorShouldHaveRequiredScopes()
        {
            var requiredScopes = Fixture.Connector.RequiredScopes;

            requiredScopes.Should().NotBeEmpty();

            requiredScopes.Should().ContainMatch("offline_access");
            requiredScopes.Should().ContainMatch("*/Sites.Read.All");
            requiredScopes.Should().ContainMatch("*/AllSites.Read");
            requiredScopes.Should().ContainMatch("*/Sites.Search.All");
        }

        [Fact]
        public void MetamodelShouldHaveCorrectEntities()
        {
            var connectorMetamodel = Fixture.Metamodel;

            connectorMetamodel.Should().NotBeNull();
            connectorMetamodel.Identifier.Should().NotBeNull();
            connectorMetamodel.Entities.Should().NotBeEmpty();
            connectorMetamodel.EntityCount.Should().BeGreaterOrEqualTo(1);

            PrintMetamodel(connectorMetamodel);
        }

        #region helpers

        private void PrintMetamodel(ConnectorMetamodel connectorMetamodel)
        {
            foreach (var entity in connectorMetamodel.Entities)
            {
                var childStr = entity.ParentMetamodelEntityKey != null
                    ? $" (is child of \"{entity.ParentMetamodelEntityKey}\") and "
                    : "";

                var typeStr = entity is EnumEntityModel ? $" (type = {nameof(EnumEntityModel)}) " : "";
                _helper.WriteLine(
                    $"Entity \"{entity.Key}\" {typeStr}{childStr} has {entity.PropertyCount} properties:");

                foreach (var prop in entity.Properties)
                {
                    string targetStr = "";
                    if (prop.TargetMetamodelEntityKey != null)
                    {
                        targetStr = $" (type= {prop.TargetMetamodelEntityKey})";
                    }

                    _helper.WriteLine($"  - {prop.Key} : {prop.DataType} {targetStr}");
                }

                if (entity is EnumEntityModel eem)
                {
                    _helper.WriteLine($"  - Enum Values: {string.Join(", ", eem.EnumValues.Select(e => e.Id))}");
                }

                _helper.WriteLine("");
            }
        }

        #endregion
    }
}
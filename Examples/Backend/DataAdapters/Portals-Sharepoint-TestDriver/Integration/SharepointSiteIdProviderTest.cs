using System.Threading.Tasks;
using FluentAssertions;
using SmintIo.Portals.Connector.SharePoint.AllowedValues;
using SmintIo.Portals.ConnectorSDK.TestDriver.Sharepoint.Test.Harness;
using Xunit;
using Xunit.Abstractions;

namespace SmintIo.Portals.ConnectorSDK.TestDriver.Sharepoint.Test.Integration
{
    [Collection(nameof(SharepointFixtureCollection))]
    public class SharepointSiteIdProviderTest
    {
        private readonly SharepointFixture _fixture;
        private SharepointSiteIdProvider _provider;

        public SharepointSiteIdProviderTest(ITestOutputHelper helper, SharepointFixture fixture)
        {
            _fixture = fixture;

            _provider = new SharepointSiteIdProvider(_fixture.ServiceProvider);
        }

        [Fact]
        public async Task GetDynamicValueAsync_ShouldReturnAllSitesAsync()
        {
            var siteDetails = await _provider.GetDynamicValueAsync(_fixture.ConfigurationOptions.SiteId);
            siteDetails.Should().NotBeNull();
            siteDetails.Value.Should().BeEquivalentTo(_fixture.ConfigurationOptions.SiteId);
            siteDetails.Name.Should().NotBeNull();
            siteDetails.Description.Should().NotBeNull();
        }

        [Fact]
        public async Task GetDynamicValueAsync_WhenSharepointClientIsNull_ShouldReturnSimpleModelAsync()
        {
            _provider = new SharepointSiteIdProvider(null);

            var siteDetails = await _provider.GetDynamicValueAsync(_fixture.ConfigurationOptions.SiteId);
            siteDetails.Should().NotBeNull();
            siteDetails.Value.Should().BeEquivalentTo(_fixture.ConfigurationOptions.SiteId);
            siteDetails.Name.Should().BeNull();
            siteDetails.Description.Should().BeNull();
        }

        [Fact]
        public async Task GetDynamicValuesList_NoSearchTermAsync()
        {
            var pagingResult = await _provider.GetDynamicValueListAsync(null, null, null, null);

            pagingResult.Result.Should().NotBeEmpty();
            pagingResult.Result.Should().OnlyContain(model =>
                model.Description != null && model.Value != null && model.Description != null);
        }

        [Fact]
        public async Task GetDynamicValuesList_WithSearch_NoResultAsync()
        {
            var pagingResult = await _provider.GetDynamicValueListAsync("notexist", null, null, null);

            pagingResult.Result.Should().NotBeNull().And.Subject.Should().BeEmpty();
        }

        [Fact]
        public async Task GetDynamicValuesList_WithSearch_YieldsResultAsync()
        {
            var pagingResult = await _provider.GetDynamicValueListAsync("ConnectorTest", null, null, null);

            pagingResult.Result.Should().NotBeEmpty().And.HaveCount(1);
            pagingResult.Result.Should().OnlyContain(model =>
                model.Description != null && model.Value != null && model.Description != null);
        }
    }
}
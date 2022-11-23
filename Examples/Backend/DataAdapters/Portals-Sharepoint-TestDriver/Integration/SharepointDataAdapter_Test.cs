using System.Threading.Tasks;
using FluentAssertions;
using SmintIo.Portals.ConnectorSDK.TestDriver.Sharepoint.Test.Harness;
using SmintIo.Portals.DataAdapter.SharePoint.Assets;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets.Parameters;
using Xunit;
using Xunit.Abstractions;

namespace SmintIo.Portals.ConnectorSDK.TestDriver.Sharepoint.Test.Integration
{
    public class SharepointDataAdapterTest : SharepointConnectorTest
    {
        private readonly SharepointAssetsDataAdapter _dataAdapter;

        public SharepointDataAdapterTest(ITestOutputHelper helper, SharepointFixture fixture) : base(helper, fixture)
        {
            _dataAdapter = Fixture.DataAdapter;
        }

        [Fact]
        public async Task GetFeatureSupportAsync()
        {
            var capabilities= await _dataAdapter.GetFeatureSupportAsync(new GetAssetsSearchFeatureSupportParameters());
            capabilities.IsFolderNavigationSupported.Should().BeTrue();
            capabilities.IsRandomAccessSupported.Should().BeTrue();
            capabilities.IsFullTextSearchProposalsSupported.Should().BeFalse();
        }
    }
}
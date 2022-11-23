using System;
using System.Threading.Tasks;
using FluentAssertions;
using SmintIo.Portals.ConnectorSDK.TestDriver.Sharepoint.Test.Harness;
using SmintIo.Portals.DataAdapter.SharePoint.Assets;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets.Parameters;
using Xunit;
using Xunit.Abstractions;

namespace SmintIo.Portals.ConnectorSDK.TestDriver.Sharepoint.Test.Integration
{
    public class SharepointDataAdapterAssetReadRandomTest : SharepointConnectorTest
    {
        private readonly SharepointAssetsDataAdapter _dataAdapter;

        public SharepointDataAdapterAssetReadRandomTest(ITestOutputHelper helper, SharepointFixture fixture) : base(
            helper, fixture)
        {
            _dataAdapter = fixture.DataAdapter;
        }

        [Fact]
        public async Task ReadRandom_ReturnsElementAsync()
        {
            var res = await _dataAdapter.GetRandomAssetsAsync(
                new GetRandomAssetsParameters
                {
                    Max = 2
                });

            res.Should().NotBeNull();
            res.AssetDataObjects.Should().HaveCount(2);
        }

        [Fact]
        public async Task ReadRandom_MaxIsLargerThanItemCountAsync()
        {
            var parameters = new GetRandomAssetsParameters()
            {
                Max = 20
            };

            var res = await _dataAdapter.GetRandomAssetsAsync(parameters);

            res.Should().NotBeNull();
            res.AssetDataObjects.Should().OnlyContain(a => a != null);
            res.AssetDataObjects.Should().HaveCountLessOrEqualTo(20);
        }

        [Fact]
        public async Task ReadRandom_MaxIsNegativeAsync()
        {
            var parameters = new GetRandomAssetsParameters()
            {
                Max = -3
            };

            var res = await _dataAdapter.GetRandomAssetsAsync(parameters);

            res.Should().NotBeNull();
            res.AssetDataObjects.Should().OnlyContain(a => a != null);
            res.AssetDataObjects.Should().HaveCountGreaterOrEqualTo(1);
        }

        [Fact]
        public Task ReadRandom_NoParamsAsync()
        {
            return Assert.ThrowsAsync<ArgumentNullException>(() => _dataAdapter.GetRandomAssetsAsync(null));
        }
    }
}
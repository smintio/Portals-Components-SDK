using System.Threading.Tasks;
using FluentAssertions;
using SmintIo.Portals.ConnectorSDK.TestDriver.Sharepoint.Test.Harness;
using SmintIo.Portals.DataAdapter.SharePoint.Assets;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets.Parameters;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets.Results;
using Xunit;
using Xunit.Abstractions;

namespace SmintIo.Portals.ConnectorSDK.TestDriver.Sharepoint.Test.Integration
{
    public class SharepointDataAdapterAssetSearchTest : SharepointConnectorTest
    {
        private readonly SharepointAssetsDataAdapter _dataAdapter;

        public SharepointDataAdapterAssetSearchTest(ITestOutputHelper helper, SharepointFixture fixture) : base(helper, fixture)
        {
            _dataAdapter = Fixture.DataAdapter;
        }

        [Fact]
        public async Task GetFormItemDefinitionAllowedValuesAsync()
        {
            var res = await _dataAdapter.GetFormItemDefinitionAllowedValuesAsync(
                new GetFormItemDefinitionAllowedValuesParameters());

            res.Should().BeEquivalentTo(new GetFormItemDefinitionAllowedValuesResult());
        }

        [Fact]
        public async Task SearchWithStringAsync()
        {
            var query = this.SharepointOptions.Files[0].TestQueryString;
            var searchParams = new SearchAssetsParameters()
            {
                QueryString = query
            };
            var result = await _dataAdapter.SearchAssetsAsync(searchParams);

            result.Should().NotBeNull();
            result.Details.TotalResults.Should().BeGreaterOrEqualTo(1);
            result.AssetDataObjects.Should().HaveCountGreaterOrEqualTo(1);
            result.Details.HasMoreResults.Should().BeFalse();
            result.Details.SearchResultSetId.Should().BeNull();

            // During the generic search, we don't get additional data.
            result.AssetDataObjects[0].RawData.Should().BeNull();
        }

        [Fact]
        public async Task SearchWithString_AndPagingAsync()
        {
            const string query = "sample";
            var searchParams = new SearchAssetsParameters
            {
                QueryString = query,
                PageSize = 8
            };
            var result1 = await _dataAdapter.SearchAssetsAsync(searchParams);

            result1.Should().NotBeNull();
            result1.AssetDataObjects.Should().HaveCountGreaterThan(0);
            result1.AssetDataObjects.Should().HaveCountLessOrEqualTo(searchParams.PageSize.Value);
        }

        [Fact]
        public async Task SearchWithString_NoResultsAsync()
        {
            const string query = "notexist";
            var searchParams = new SearchAssetsParameters
            {
                QueryString = query
            };
            var result = await _dataAdapter.SearchAssetsAsync(searchParams);

            result.Should().NotBeNull();
            result.Details.Should().BeNull();
            result.DataObjects.Should().BeNull();
            result.AssetDataObjects.Should().BeEmpty();
        }

        [Fact]
        public async Task GetFullTextSearchProposalsAsync()
        {
            var query = SharepointOptions.Files[0].TestQueryString;

            var proposals = await _dataAdapter.GetFullTextSearchProposalsAsync(new GetFullTextSearchProposalsParameters
            {
                SearchQueryString = query
            });

            proposals.FullTextProposals.Should().HaveCountGreaterOrEqualTo(1);
        }

        [Fact]
        public async Task GetFullTextSearchProposals_NoResultAsync()
        {
            const string query = "notexist";

            var proposals = await _dataAdapter.GetFullTextSearchProposalsAsync(new GetFullTextSearchProposalsParameters
            {
                SearchQueryString = query
            });

            proposals.FullTextProposals.Should().BeEmpty();
        }
    }
}
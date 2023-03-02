using System.Collections.Generic;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;
using SmintIo.Portals.Connector.Picturepark.Metamodel.Aggregations;
using SmintIo.Portals.ConnectorSDK.Clients.Prefab;
using SmintIo.Portals.SDK.Core.Http.Prefab.Extensions;
using SmintIo.Portals.SDK.Core.Http.Prefab.Models;

namespace SmintIo.Portals.Connector.Picturepark.Client
{
    public enum SearchType
    {
        SimpleAnd,
        SimpleOr,
        Advanced
    };

    public interface IPictureparkClient : IClient
    {
        IRequestFailedHandler DefaultRequestFailedHandler { get; }

        Task InitializeChannelAggregatorsAsync();

        Task<AggregatorManager> GetAggregatorManagerAsync();

        Task<ICollection<Schema>> GetSchemasAsync();

        Task<ICollection<SchemaDetail>> GetSchemaDetailsAsync(ICollection<Schema> schemas);

        Task<ICollection<OutputFormatInfo>> GetOutputFormatsAsync();

        Task<ICollection<ListItem>> GetListItemsAsync(ICollection<Schema> schemas);

        Task<Channel> GetChannelAsync(string channelId);

        Task<ICollection<Channel>> GetAllChannelsAsync();

        Task<(ContentSearchResult, ICollection<ContentDetail>)> SearchContentAsync(string searchString, ICollection<AggregatorBase> aggregators,
            FilterBase filter, ICollection<AggregationFilter> aggregationFilters, string pageToken, int? pageSize, ICollection<SortInfo> sortInfos,
            SearchType? searchType = null, bool includeFulltext = false, bool resolveMetadata = false);

        Task<(ContentDetail ContentDetail, ContentType? OriginalContentType)> GetContentAsync(string id);

        Task<ICollection<ContentDetail>> GetContentsAsync(ICollection<string> ids, bool skipNonAccessibleContents = false);

        Task<ICollection<ContentDetail>> GetContentPermissionsAsync(ICollection<string> ids);

        Task<ICollection<ContentDetail>> GetContentOutputsAsync(ICollection<string> ids);

        Task<ICollection<OutputFormatDetail>> GetOutputFormatsAsync(ICollection<string> outputFormatIds);

        Task CreateContentsAsync(ICollection<ContentCreateRequest> contents);

        Task UpdateContentsAsync(ICollection<ContentMetadataUpdateItem> contents);

        Task<StreamResponse> GetImageDownloadStreamAsync(string id, ThumbnailSize size);

        Task<StreamResponse> GetPlaybackDownloadStreamAsync(string id, string size);

        Task<StreamResponse> GetDownloadStreamForOutputFormatIdAsync(string id, string outputFormatId);

        Task<ICollection<string>> GetProfileUserRoleIdsAsync();
    }
}
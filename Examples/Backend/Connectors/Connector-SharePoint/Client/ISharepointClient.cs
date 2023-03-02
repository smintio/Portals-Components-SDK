using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Graph;
using SmintIo.Portals.Connector.SharePoint.Models;
using SmintIo.Portals.ConnectorSDK.Clients.Prefab;
using SmintIo.Portals.SDK.Core.Http.Prefab.Extensions;
using SmintIo.Portals.SDK.Core.Http.Prefab.Models;

namespace SmintIo.Portals.Connector.SharePoint.Client
{
    /// <summary>
    /// A facade for the Microsoft GraphAPI client.  
    /// </summary>
    public interface ISharepointClient : IClient
    {
        IRequestFailedHandler DefaultRequestFailedHandler { get; }

        /// <summary>
        /// The identifier of sharepoint site.
        /// </summary>
        string SiteId { get; }

        /// <summary>
        /// Gets all sites of the Sharepoint instance the user has access to.
        /// </summary>
        /// <param name="query">optionally supply a query string, e.g. a part of the site's name</param>
        /// <returns>A list of <see cref="Site"/> objects</returns>
        Task<ICollection<SiteResponse>> GetSitesAsync(string query = null);

        /// <summary>
        /// Gets a specific site by identifier.
        /// </summary>
        /// <param name="siteId">The identifier.</param>
        /// <returns><see cref="Site"/></returns>
        Task<Site> GetSiteAsync(string siteId);

        /// <summary>
        /// Gets the metadata (= fields or "columns") for a particular site. Metadata is configured in Sharepoint Web and is
        /// valid site-wide. 
        /// </summary>
        /// <param name="siteId">The ID for the site. Normally this looks like "{host}.sharepoint.com,{guid},{guid}</param>
        /// <returns>The list of <see cref="ColumnDefinitionResponse"/> objects representing the metadata fields.</returns>
        public Task<ICollection<ColumnDefinitionResponse>> GetSiteMetadataAsync(string siteId);

        /// <summary>
        /// Gets the site's drives.
        /// </summary>
        /// <returns></returns>
        public Task<ICollection<Drive>> GetSiteDrivesAsync();

        /// <summary>
        /// Gets the site's drive.
        /// </summary>
        /// <param name="driveId"></param>
        /// <returns></returns>
        public Task<Drive> GetSiteDriveAsync(string driveId);

        /// <summary>
        /// Gets folder <see cref="DriveItem"/>'s by identifier from Sharepoint
        /// Without assetId the allowed folders drive items will be returned
        /// </summary>
        /// <param name="assetId">The asset id</param>
        /// <param name="skipToken">The skip token</param>
        /// <param name="pageSize">The page size</param>
        /// <returns>A list of <see cref="DriveItemListModel"/> items.</returns>
        Task<DriveItemListModel> GetFolderDriveItemsAsync(string assetId, string skipToken, int? pageSize);

        /// <summary>
        /// Gets a flat list of folders as <see cref="DriveItem"/> from Sharepoint.
        /// </summary>
        /// <returns>A list of <see cref="DriveItem"/> items.</returns>
        Task<ICollection<DriveItem>> GetFoldersListAsync();

        /// <summary>
        /// Gets a <see cref="DriveItem"/> by identifier from Sharepoint.
        /// </summary>
        /// <param name="assetId">The asset Id is a complex key following this format DriveId__DriveItemId</param>
        /// <returns>Returns null if nothing found.</returns>
        Task<DriveItem> GetFolderDriveItemAsync(string assetId);

        /// <summary>
        /// Gets a <see cref="DriveItem"/> for a given Sharepoint Id. 
        /// </summary>
        /// <param name="assetId">The asset Id is a complex key following this format DriveId__DriveItemId</param>
        /// <returns>Returns null if nothing found.</returns>
        Task<DriveItem> GetDriveItemAsync(string assetId);

        /// <summary>
        /// Gets additional (raw) meta data for a driveItem. 
        /// </summary>
        /// <param name="item">The <see cref="DriveItem"/> for which the metadata should be obtained.</param>
        /// <returns>A dictionary containing the field's name as Key, and its value as Value</returns>
        Task<IDictionary<string, object>> GetDriveItemAdditionalDataByFieldNamesAsync(DriveItem driveItem);

        /// <summary>
        /// Gets a <see cref="ItemPreviewInfo"/> for a given Sharepoint Id. 
        /// </summary>
        /// <param name="assetId">The Sharepoint Id of the file</param>
        /// <returns><see cref="ItemPreviewInfo"/></returns>
        Task<ItemPreviewInfo> GetDriveItemPreviewInfoAsync(string assetId);

        /// <summary>
        /// Loads the thumbails for multiple drive items. Batching is used, so we can reduce the network traffic to Sharepoint.
        /// </summary>
        /// <param name="driveItems"><see cref="DriveItem"/></param>
        /// <returns><see cref="DriveItem"/> with populated thumbnail information.</returns>
        Task<ICollection<DriveItem>> GetDriveItemsBatchAsync(List<string> assetsToFetch);

        /// <summary>
        /// Gets the thumbnail for a given size for file
        /// </summary>
        /// <returns>The thumbnail stream response.</returns>
        /// <exception cref="ArgumentException">if the <paramref name="size"/> is invalid</exception>
        Task<StreamResponse> GetDriveItemThumbnailContentAsync(string assetId, string thumbnailSetId, string size, long? maxFileSizeBytes);

        /// <summary>
        /// Gets the file as PDF format.
        /// </summary>
        /// <param name="assetId"></param>
        /// <returns>The stream response</returns>
        Task<StreamResponse> GetDriveItemPdfContentAsync(string assetId, long? maxFileSizeBytes);

        /// <summary>
        /// Downloads a file from Sharepoint.
        /// </summary>
        /// <returns>A stream containing the file raw data.</returns>
        Task<StreamResponse> GetDriveItemContentAsync(string assetId, long? maxFileSizeBytes);

        /// <summary>
        /// Gets the drive item changes based on a token.
        /// </summary>
        /// <returns>A list of <see cref="DriveItemChangesListModel"/> items.</returns>
        Task<DriveItemChangesListModel> GetDriveItemChangesListAsync(string deltaLink);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using SmintIo.Portals.Connector.SharePoint.Client;
using SmintIo.Portals.Connector.SharePoint.Extensions;
using SmintIo.Portals.SDK.Core.Extensions;
using SmintIo.Portals.SDK.Core.Models.Paging;
using SmintIo.Portals.SDK.Core.Models.Strings;
using SmintIo.Portals.SDK.Core.Providers;
using SmintIo.Portals.SDK.Core.Resources;
using SmintIo.Portals.SDK.Core.Rest.Prefab.Exceptions;

namespace SmintIo.Portals.Connector.SharePoint.AllowedValues
{
    public class SharepointFolderIdProvider : IDynamicValueListProvider<string>
    {
        private const string RootFolderId = "0";

        private readonly ISharepointClient _sharepointClient;

        public IDynamicAllowedValuesParametersProvider ParametersProvider => null;

        public SharepointFolderIdProvider(IServiceProvider serviceProvider)
        {
            try
            {
                _sharepointClient = serviceProvider?.GetService<ISharepointClient>();
            }
            catch (Exception e)
            when (e.InnerException != null &&
                  (e.InnerException is ExternalDependencyException ||
                   e.InnerException is ArgumentNullException))
            {
                // the connector is not yet set up
                _sharepointClient = null;
            }
        }

        public bool SupportsSearch { get; } = true;

        public bool SupportsPagination { get; } = true;

        public async Task<UiDetailsModel<string>> GetDynamicValueAsync(string assetId)
        {
            if (_sharepointClient == null || string.IsNullOrEmpty(_sharepointClient.SiteId))
            {
                return new UiDetailsModel<string>
                {
                    Value = assetId
                };
            }

            if (string.Equals(assetId, RootFolderId))
            {
                return new UiDetailsModel<string>()
                {
                    Value = RootFolderId,
                    Name = MetamodelMessages.core_root_folder.Localize()
                };
            }

            try
            {
                var folderDriveItem = await _sharepointClient.GetFolderDriveItemAsync(assetId).ConfigureAwait(false);

                return new UiDetailsModel<string>
                {
                    Value = folderDriveItem.GetAssetId(),
                    Name = folderDriveItem.Name.Localize()
                };
            }
            catch (Exception)
            {
                // some issue

                return new UiDetailsModel<string>
                {
                    Value = assetId
                };
            }
        }

        public async Task<PagingResult<UiDetailsModel<string>>> GetDynamicValueListAsync(string searchTerm, int? offset, int? limit, string parentValue)
        {
            if (_sharepointClient == null || string.IsNullOrEmpty(_sharepointClient.SiteId))
            {
                return null;
            }

            var folderDriveItems = await _sharepointClient.GetFoldersListAsync().ConfigureAwait(false);

            if (folderDriveItems == null)
            {
                folderDriveItems = new List<DriveItem>();
            }

            var uiDetailsModels = folderDriveItems
                .Where(di => !string.IsNullOrEmpty(searchTerm)
                    ? di.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                    : true)
                .Select(di => new UiDetailsModel<string>
                {
                    Value = di.GetAssetId(),
                    Name = di.Name.Localize()
                })
                .ToList();

            uiDetailsModels.Insert(0, new UiDetailsModel<string>()
            {
                Value = RootFolderId,
                Name = MetamodelMessages.core_root_folder.Localize()
            });

            if (offset.HasValue && limit.HasValue)
            {
                uiDetailsModels = uiDetailsModels
                    .Skip(offset.Value)
                    .Take(limit.Value)
                    .ToList();
            }

            return new PagingResult<UiDetailsModel<string>>
            {
                Result = uiDetailsModels,
                TotalCount = uiDetailsModels.Count
            };
        }
    }
}
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
using SmintIo.Portals.SDK.Core.Rest.Prefab.Exceptions;

namespace SmintIo.Portals.Connector.SharePoint.AllowedValues
{
    public class SharepointDriveIdProvider : IDynamicValueListProvider<string>
    {
        private readonly ISharepointClient _sharepointClient;

        public IDynamicAllowedValuesParametersProvider ParametersProvider => null;

        public SharepointDriveIdProvider(IServiceProvider serviceProvider)
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

            try
            {
                var drive = await _sharepointClient.GetSiteDriveAsync(assetId).ConfigureAwait(false);

                return new UiDetailsModel<string>
                {
                    Value = drive.Id,
                    Name = drive.Name.Localize()
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

            var drives = await _sharepointClient.GetSiteDrivesAsync().ConfigureAwait(false);

            if (drives == null)
            {
                drives = new List<Drive>();
            }

            var uiDetailsModels = drives
                .Where(di => !string.IsNullOrEmpty(searchTerm)
                    ? di.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                    : true)
                .Select(d => new UiDetailsModel<string>
                {
                    Value = d.Id,
                    Name = d.Name.Localize()
                })
                .ToList();

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
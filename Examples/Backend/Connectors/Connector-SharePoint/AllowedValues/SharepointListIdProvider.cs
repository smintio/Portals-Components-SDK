using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SmintIo.Portals.Connector.SharePoint.Client;
using SmintIo.Portals.SDK.Core.Extensions;
using SmintIo.Portals.SDK.Core.Models.Paging;
using SmintIo.Portals.SDK.Core.Models.Strings;
using SmintIo.Portals.SDK.Core.Providers;
using SmintIo.Portals.SDK.Core.Rest.Prefab.Exceptions;

namespace SmintIo.Portals.Connector.SharePoint.AllowedValues
{
    public class SharepointListIdProvider : IDynamicValueListProvider<string>
    {
        private readonly ISharepointClient _sharepointClient;

        public IDynamicAllowedValuesParametersProvider ParametersProvider => null;

        public SharepointListIdProvider(IServiceProvider serviceProvider)
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
                var site = await _sharepointClient.GetSiteAsync(_sharepointClient.SiteId).ConfigureAwait(false);

                var list = site.Lists.FirstOrDefault(l => l.Id == assetId);

                return new UiDetailsModel<string>
                {
                    Value = list.Id,
                    Name = list.Name.Localize()
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

            var site = await _sharepointClient.GetSiteAsync(_sharepointClient.SiteId).ConfigureAwait(false);

            var lists = site?.Lists?.ToList();

            if (lists == null)
            {
                lists = new List<Microsoft.Graph.Models.List>();
            }

            var uiDetailsModels = lists
                .Where(l => !string.IsNullOrEmpty(searchTerm)
                    ? l.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                    : true)
                .Select(l => new UiDetailsModel<string>
                {
                    Value = l.Id,
                    Name = l.Name.Localize()
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

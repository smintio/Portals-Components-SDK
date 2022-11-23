using System;
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
    public class SharepointSiteIdProvider : IDynamicValueListProvider<string>
    {
        private readonly ISharepointClient _sharepointClient;

        public SharepointSiteIdProvider(IServiceProvider serviceProvider)
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

        public bool SupportsPagination { get; } = false;

        public async Task<UiDetailsModel<string>> GetDynamicValueAsync(string siteId)
        {
            if (_sharepointClient == null)
            {
                return new UiDetailsModel<string>
                {
                    Value = siteId
                };
            }

            try
            {
                var site = await _sharepointClient.GetSiteAsync(siteId).ConfigureAwait(false);

                return new UiDetailsModel<string>
                {
                    Name = site.DisplayName.Localize(),
                    Value = site.Id,
                    Description = site.WebUrl.Localize()
                };
            }
            catch (Exception)
            {
                // some issue

                return new UiDetailsModel<string>
                {
                    Value = siteId
                };
            }
        }

        public async Task<PagingResult<UiDetailsModel<string>>> GetDynamicValueListAsync(string searchTerm, int? offset, int? limit, string parentValue)
        {
            if (_sharepointClient == null)
            {
                return null;
            }

            var sites = await _sharepointClient.GetSitesAsync(searchTerm).ConfigureAwait(false);

            var uiDetailsModels = sites
                .Select(site => new UiDetailsModel<string>
                {
                    Value = site.Id,
                    Name = site.DisplayName.Localize(),
                    Description = site.WebUrl.Localize()
                })
                .ToList();

            return new PagingResult<UiDetailsModel<string>>
            {
                Result = uiDetailsModels,
                TotalCount = uiDetailsModels.Count
            };
        }
    }
}
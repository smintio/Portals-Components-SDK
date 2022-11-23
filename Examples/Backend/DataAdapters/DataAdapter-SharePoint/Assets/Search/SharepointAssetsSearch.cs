using System;
using System.Threading.Tasks;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Impl;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets.Parameters;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets.Results;

namespace SmintIo.Portals.DataAdapter.SharePoint.Assets
{
    public partial class SharepointAssetsDataAdapter : AssetsDataAdapterBaseImpl, IAssetsIntegrationLayerApiProvider
    {
        public override Task<GetFormItemDefinitionAllowedValuesResult> GetFormItemDefinitionAllowedValuesAsync(GetFormItemDefinitionAllowedValuesParameters parameters)
        {
            if (_smintIoIntegrationLayerProvider == null)
            {
                throw new NotImplementedException();
            }

            return _smintIoIntegrationLayerProvider.GetFormItemDefinitionAllowedValuesAsync(Context, parameters);
        }

        public override Task<SearchAssetsResult> SearchAssetsAsync(SearchAssetsParameters parameters)
        {
            if (_smintIoIntegrationLayerProvider == null)
            {
                throw new NotImplementedException();
            }

            return _smintIoIntegrationLayerProvider.SearchAssetsAsync(Context, parameters);
        }

        public override Task<GetFullTextSearchProposalsResult> GetFullTextSearchProposalsAsync(GetFullTextSearchProposalsParameters parameters)
        {
            if (_smintIoIntegrationLayerProvider == null)
            {
                throw new NotImplementedException();
            }

            return _smintIoIntegrationLayerProvider.GetFullTextSearchProposalsAsync(Context, parameters);
        }
    }
}
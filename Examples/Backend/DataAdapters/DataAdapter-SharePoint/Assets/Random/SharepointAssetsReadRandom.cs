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
        public override Task<GetRandomAssetsResult> GetRandomAssetsAsync(GetRandomAssetsParameters parameters)
        {
            if (_smintIoIntegrationLayerProvider != null)
            {
                return _smintIoIntegrationLayerProvider.GetRandomAssetsAsync(Context, parameters);
            }

            throw new NotImplementedException();
        }
    }
}
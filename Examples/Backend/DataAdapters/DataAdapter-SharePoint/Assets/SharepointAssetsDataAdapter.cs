using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmintIo.Portals.Connector.SharePoint.Client;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Impl;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets.Parameters;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets.Results;
using SmintIo.Portals.DataAdapterSDK.Providers;
using SmintIo.Portals.SDK.Core.Models.Metamodel.Model;

namespace SmintIo.Portals.DataAdapter.SharePoint.Assets
{
    public partial class SharepointAssetsDataAdapter : AssetsDataAdapterBaseImpl, IAssetsIntegrationLayerApiProvider
    {
        private readonly ILogger _logger;
        private readonly ISharepointClient _sharepointClient;

        private readonly IEntityModelProvider _entityModelProvider;
        private readonly ISmintIoIntegrationLayerProvider _smintIoIntegrationLayerProvider;

        public SharepointAssetsDataAdapter(
            ILogger logger,
            IServiceProvider serviceProvider,
            ISharepointClient sharepointClient)
            : base(serviceProvider)
        {
            _logger = logger;
            _sharepointClient = sharepointClient;

            _entityModelProvider = serviceProvider.GetService<IEntityModelProvider>();
            _smintIoIntegrationLayerProvider = serviceProvider.GetService<ISmintIoIntegrationLayerProvider>();
        }

        public override Task<GetAssetsSearchFeatureSupportResult> GetFeatureSupportAsync(GetAssetsSearchFeatureSupportParameters parameters)
        {
            var featureSupport = new GetAssetsSearchFeatureSupportResult
            {
                IsRandomAccessSupported = true,
				IsFullTextSearchProposalsSupported = false,
                IsFolderNavigationSupported = false
            };

            return Task.FromResult(featureSupport);
        }
    }
}
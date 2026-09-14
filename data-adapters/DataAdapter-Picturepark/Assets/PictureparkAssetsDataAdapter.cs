using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmintIo.Portals.Connector.Picturepark.Client;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Impl;
using SmintIo.Portals.SDK.Core.Models.Context;
using SmintIo.Portals.SDK.Core.Models.Metamodel.Model;

namespace SmintIo.Portals.DataAdapter.Picturepark.Assets
{
    public partial class PictureparkAssetsDataAdapter : AssetsDataAdapterBaseImpl
    {
        private readonly PictureparkAssetsDataAdapterConfiguration _configuration;

        private readonly IPictureparkClient _client;

        private readonly IPortalsContextModel _portalsContext;

        private readonly IEntityModelProvider _entityModelProvider;

        private readonly ILogger _logger;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public PictureparkAssetsDataAdapter(
            PictureparkAssetsDataAdapterConfiguration configuration,
            IPictureparkClient client,
            ILogger logger,
            IServiceProvider serviceProvider) :
            base(serviceProvider)
        {
            _configuration = configuration;

            _client = client;

            _portalsContext = serviceProvider.GetService<IPortalsContextModel>();
            _entityModelProvider = serviceProvider.GetService<IEntityModelProvider>();

            _logger = logger;
        }
    }
}
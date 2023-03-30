using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmintIo.Portals.Connector.HelloWorld.Client;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Impl;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets.Parameters;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets.Results;
using SmintIo.Portals.SDK.Core.Models.Metamodel.Model;

namespace SmintIo.Portals.DataAdapter.HelloWorld.Assets
{
    /// <summary>
    /// The data adapter broken down into multiple partial classes
    /// The other parts are usually located in the `Search`, `Reading`, and `Random` folders.
    /// </summary>
    public partial class HelloWorldAssetsDataAdapter : AssetsDataAdapterBaseImpl
    {
        private readonly ILogger _logger;
        private readonly IHelloWorldClient _helloWorldClient;

        private readonly IEntityModelProvider _entityModelProvider;

        private readonly HelloWorldAssetsDataAdapterConfiguration _configuration;

        public HelloWorldAssetsDataAdapter(
            ILogger logger,
            IServiceProvider serviceProvider,
            IHelloWorldClient helloWorldClient,
            HelloWorldAssetsDataAdapterConfiguration configuration)
            : base(serviceProvider)
        {
            _logger = logger;
            _helloWorldClient = helloWorldClient;

            _entityModelProvider = serviceProvider.GetService<IEntityModelProvider>();

            _configuration = configuration;
        }

        /// <summary>
        /// Defines what features the data adapter supports
        /// </summary>
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
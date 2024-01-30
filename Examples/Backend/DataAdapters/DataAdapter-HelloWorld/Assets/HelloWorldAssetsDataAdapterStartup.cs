using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SmintIo.Portals.Connector.HelloWorld;
using SmintIo.Portals.DataAdapter.HelloWorld.Resources;
using SmintIo.Portals.DataAdapterSDK.DataAdapters;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Permissions;
using SmintIo.Portals.SDK.Core.Configuration;
using SmintIo.Portals.SDK.Core.Models.Strings;

namespace SmintIo.Portals.DataAdapter.HelloWorld.Assets
{
    public class HelloWorldAssetsDataAdapterStartup : IDataAdapterStartup
    {
        public const string HelloWorldAssetsDataAdapter = "assets";

        /// <summary>
        /// The data adapter unique key
        /// </summary>
        public string Key => HelloWorldAssetsDataAdapter;

        /// <summary>
        /// The localized strings
        /// </summary>
        public LocalizedStringsModel Name => new ResourceLocalizedStringsModel(nameof(Resources.ConfigurationMessages.da_assets_name));

        /// <summary>
        /// The localized strings
        /// </summary>
        public LocalizedStringsModel Description => new ResourceLocalizedStringsModel(nameof(Resources.ConfigurationMessages.da_assets_description));

        /// <summary>
        /// Optional data adapter logo link
        /// </summary>
        public string LogoUrl => null;

        /// <summary>
        /// Optional link for the data adapter icon
        /// </summary>
        public string IconUrl => null;

        /// <summary>
        /// The type of data adapter configuration
        /// </summary>
        public Type ConfigurationImplementation => typeof(HelloWorldAssetsDataAdapterConfiguration);

        /// <summary>
        /// The type of data adapter implementation
        /// </summary>
        public Type ComponentImplementation => typeof(HelloWorldAssetsDataAdapter);

        /// <summary>
        /// The type of resource file for configuration messages 
        /// </summary>
        public Type ConfigurationMessages => typeof(ConfigurationMessages);

        /// <summary>
        /// <see cref="IServiceCollection"/> injection hook for this data adapter
        /// </summary>
        public void ConfigureServices(IServiceCollection services)
        {
        }

        public Task FillDefaultFormFieldValuesModelAsync(string connectorEntityModelKey, FormFieldValuesModel formFieldValuesModel)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Returns which connector should be used by the data adapter
        /// </summary>
        public string ConnectorKey => HelloWorldConnectorStartup.HelloWorldConnector;

        /// <summary>
        /// Describes the permissions that the data adapter requires
        /// </summary>
        public DataAdapterPermission[] Permissions => null;

        /// <summary>
        /// Defines which interfaces should be implemented by the current data adapter so that the Smint.Io infrastructure knows how to include them
        /// For additional interfaces, feel free to contact Smint.Io
        /// </summary>
        public Type[] PublicApiInterfaces => new[] { typeof(IAssets) };
    }
}
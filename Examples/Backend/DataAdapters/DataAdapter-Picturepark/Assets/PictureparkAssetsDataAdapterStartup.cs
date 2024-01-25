using Microsoft.Extensions.DependencyInjection;
using SmintIo.Portals.Connector.Picturepark;
using SmintIo.Portals.DataAdapter.Picturepark.Resources;
using SmintIo.Portals.DataAdapterSDK.DataAdapters;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Permissions;
using SmintIo.Portals.SDK.Core.Configuration;
using SmintIo.Portals.SDK.Core.Models.Strings;
using System;
using System.Threading.Tasks;

namespace SmintIo.Portals.DataAdapter.Picturepark.Assets.Search
{
    public class PictureparkAssetsDataAdapterStartup : IDataAdapterStartup
    {
        public const string PictureparkAssetsDataAdapter = "assets";

        public string Key => PictureparkAssetsDataAdapter;

        public string ConnectorKey => PictureparkConnectorStartup.PictureparkConnector;

        public LocalizedStringsModel Name { get; } = new ResourceLocalizedStringsModel(nameof(Resources.ConfigurationMessages.da_assets_name));

        public LocalizedStringsModel Description { get; } = new ResourceLocalizedStringsModel(nameof(Resources.ConfigurationMessages.da_assets_description));

        public string LogoUrl => null;

        public string IconUrl => null;

        public void ConfigureServices(IServiceCollection services)
        {
        }

        public Type ComponentImplementation => typeof(PictureparkAssetsDataAdapter);

        public Type ConfigurationImplementation => typeof(PictureparkAssetsDataAdapterConfiguration);

        public Type ConfigurationMessages => typeof(ConfigurationMessages);

        public Type[] PublicApiInterfaces => new Type[] { typeof(IAssets) };

        public DataAdapterPermission[] Permissions => null;

        public Task FillDefaultFormFieldValuesModelAsync(string connectorEntityModelKey, FormFieldValuesModel formFieldValuesModel)
        {
            return Task.CompletedTask;
        }
    }
}
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SmintIo.Portals.Connector.SharePoint;
using SmintIo.Portals.DataAdapter.SharePoint.Resources;
using SmintIo.Portals.DataAdapterSDK.DataAdapters;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Permissions;
using SmintIo.Portals.SDK.Core.Configuration;
using SmintIo.Portals.SDK.Core.Models.Strings;

namespace SmintIo.Portals.DataAdapter.SharePoint.Assets
{
    public class OneDriveAssetsDataAdapterStartup : IDataAdapterStartup
    {
        public const string OneDriveAssetsDataAdapter = "assets";

        public void ConfigureServices(IServiceCollection services)
        {
        }

        public string Key => OneDriveAssetsDataAdapter;

        public LocalizedStringsModel Name { get; } = new ResourceLocalizedStringsModel(nameof(OneDriveConfigurationMessages.da_assets_name));

        public LocalizedStringsModel Description { get; } = new ResourceLocalizedStringsModel(nameof(OneDriveConfigurationMessages.da_assets_description));

        public string LogoUrl => null;
        public string IconUrl => null;

        public Type ConfigurationImplementation => typeof(SharepointAssetsDataAdapterConfiguration);

        public Type ComponentImplementation => typeof(SharepointAssetsDataAdapter);

        public Type ConfigurationMessages => typeof(OneDriveConfigurationMessages);

        public Type MetamodelMessages => null;

        public string ConnectorKey => OneDriveConnectorStartup.OneDriveConnector;

        public DataAdapterPermission[] Permissions => null;

        public Type[] PublicApiInterfaces { get; } = [typeof(IAssets), typeof(IAssetsIntegrationLayerApiProvider)];

        public Task FillDefaultFormFieldValuesModelAsync(string connectorEntityModelKey, FormFieldValuesModel formFieldValuesModel)
        {
            return Task.CompletedTask;
        }
    }
}
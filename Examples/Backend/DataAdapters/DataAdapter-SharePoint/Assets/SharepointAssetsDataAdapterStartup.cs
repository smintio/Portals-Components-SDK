using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SmintIo.Portals.Connector.SharePoint;
using SmintIo.Portals.DataAdapterSDK.DataAdapters;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Permissions;
using SmintIo.Portals.SDK.Core.Configuration;
using SmintIo.Portals.SDK.Core.Models.Strings;

namespace SmintIo.Portals.DataAdapter.SharePoint.Assets
{
    public class SharepointAssetsDataAdapterStartup : IDataAdapterStartup
    {
        public const string SharepointAssetsDataAdapter = "assets";

        public void ConfigureServices(IServiceCollection services)
        {
        }

        public string Key => SharepointAssetsDataAdapter;

        public LocalizedStringsModel Name => new LocalizedStringsModel
        {
            { LocalizedStringsModel.DefaultCulture, "Asset access" },
            { "de", "Zugriff auf Assets" },
        };

        public LocalizedStringsModel Description => new LocalizedStringsModel
        {
             { LocalizedStringsModel.DefaultCulture, "Provides services to read, search and download assets from SharePoint." },
            { "de", "Stellt Dienste zum Lesen, Suchen und Herunterladen von SharePoint-Assets zur Verfügung." }
        };

        public string LogoUrl => null;
        public string IconUrl => null;

        public Type ConfigurationImplementation => typeof(SharepointAssetsDataAdapterConfiguration);

        public Type ComponentImplementation => typeof(SharepointAssetsDataAdapter);

        public string ConnectorKey => SharepointConnectorStartup.SharepointConnector;

        public DataAdapterPermission[] Permissions => null;

        public Type[] PublicApiInterfaces => new[] { typeof(IAssets), typeof(IAssetsIntegrationLayerApiProvider) };

        public Task FillDefaultFormFieldValuesModelAsync(string connectorEntityModelKey, FormFieldValuesModel formFieldValuesModel)
        {
            return Task.CompletedTask;
        }
    }
}
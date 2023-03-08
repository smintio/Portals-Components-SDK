using System;
using Microsoft.Extensions.DependencyInjection;
using SmintIo.Portals.Connector.HelloWorld;
using SmintIo.Portals.DataAdapterSDK.DataAdapters;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Permissions;
using SmintIo.Portals.SDK.Core.Models.Strings;

namespace SmintIo.Portals.DataAdapter.HelloWorld.Assets
{
    public class HelloWorldAssetsDataAdapterStartup : IDataAdapterStartup
    {
        public const string HelloWorldAssetsDataAdapter = "assets";

        public void ConfigureServices(IServiceCollection services)
        {
        }

        public string Key => HelloWorldAssetsDataAdapter;

        public LocalizedStringsModel Name => new()
        {
            { LocalizedStringsModel.DefaultCulture, "Asset access" },
            { "de", "Zugriff auf Assets" },
        };

        public LocalizedStringsModel Description => new()
        {
            { LocalizedStringsModel.DefaultCulture, "Provides services to read, search and download assets from HelloWorld." },
            { "de", "Stellt Dienste zum Lesen, Suchen und Herunterladen von HelloWorld-Assets zur Verfügung." }
        };

        public string LogoUrl => null;

        public string IconUrl => null;

        public Type ConfigurationImplementation => typeof(HelloWorldAssetsDataAdapterConfiguration);

        public Type ComponentImplementation => typeof(HelloWorldAssetsDataAdapter);

        public string ConnectorKey => HelloWorldConnectorStartup.HelloWorldConnector;

        public DataAdapterPermission[] Permissions => null;

        public Type[] PublicApiInterfaces => new[] { typeof(IAssets) };
    }
}
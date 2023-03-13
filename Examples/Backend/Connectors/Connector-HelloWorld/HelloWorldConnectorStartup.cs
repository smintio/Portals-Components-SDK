using System;
using Microsoft.Extensions.DependencyInjection;
using SmintIo.Portals.ConnectorSDK.Connectors;
using SmintIo.Portals.SDK.Core.Models.Strings;

namespace SmintIo.Portals.Connector.HelloWorld
{
    public class HelloWorldConnectorStartup : IConnectorStartup
    {
        public const string HelloWorldConnector = "helloWorld";

        public string Key => HelloWorldConnector;

        public LocalizedStringsModel Name => new()
        {
            {LocalizedStringsModel.DefaultCulture, "Hello, World!"}
        };

        public LocalizedStringsModel Description => new()
        {
            {LocalizedStringsModel.DefaultCulture, "Connects Smint.io Portals to Hello World."},
            {"de", "Verbindet Smint.io-Portale mit Hello World."}
        };

        public string LogoUrl => "Hello World logo URL";

        public string IconUrl => "Hello World icon URL";

        public Type ConfigurationImplementation => typeof(HelloWorldConnectorConfiguration);
        public Type ComponentImplementation => typeof(HelloWorldConnector);

        public void ConfigureServices(IServiceCollection services)
        {
        }

        public string[] DefaultDataAdapterKeys => new[] { "assets" };

        public ConnectorSetupMethod SetupMethod => ConnectorSetupMethod.Setup;

        public bool IsAdvanced => false;

        public LocalizedStringsModel SetupDocumentationUrl => new()
        {
            { LocalizedStringsModel.DefaultCulture, "Hello World documentation URL" }
        };
    }
}
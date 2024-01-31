using System;
using Microsoft.Extensions.DependencyInjection;
using SmintIo.Portals.Connector.HelloWorld.Resources;
using SmintIo.Portals.ConnectorSDK.Connectors;
using SmintIo.Portals.SDK.Core.Models.Strings;

namespace SmintIo.Portals.Connector.HelloWorld
{
    public class HelloWorldConnectorStartup : IConnectorStartup
    {
        public const string HelloWorldConnector = "hello-world";

        /// <summary>
        /// The connector unique key
        /// </summary>
        public string Key => HelloWorldConnector;

        /// <summary>
        /// The localized strings
        /// </summary>
        public LocalizedStringsModel Name => new ResourceLocalizedStringsModel(nameof(Resources.ConfigurationMessages.c_hello_world_name));

        /// <summary>
        /// The localized strings
        /// </summary>
        public LocalizedStringsModel Description => new ResourceLocalizedStringsModel(nameof(Resources.ConfigurationMessages.c_hello_world_description));

        /// <summary>
        /// The localized strings
        /// </summary>
        public LocalizedStringsModel SetupDocumentationUrl => new ResourceLocalizedStringsModel(nameof(Resources.ConfigurationMessages.c_hello_world_setup_documentation_url));

        /// <summary>
        /// Optional connector logo link
        /// </summary>
        public string LogoUrl => "Hello World logo URL";

        /// <summary>
        /// Optional link for the connector icon
        /// </summary>
        public string IconUrl => "Hello World icon URL";

        /// <summary>
        /// The type of connector configuration
        /// </summary>
        public Type ConfigurationImplementation => typeof(HelloWorldConnectorConfiguration);

        /// <summary>
        /// The type of connector implementation
        /// </summary>
        public Type ComponentImplementation => typeof(HelloWorldConnector);

        /// <summary>
        /// The type of resource file for configuration messages 
        /// </summary>
        public Type ConfigurationMessages => typeof(ConfigurationMessages);

        /// <summary>
        /// The type of resource file for metamodel messages 
        /// </summary>
        public Type MetamodelMessages => typeof(MetamodelMessages);

        /// <summary>
        /// <see cref="IServiceCollection"/> injection hook for this connector
        /// </summary>
        public void ConfigureServices(IServiceCollection services)
        {
        }

        /// <summary>
        /// The default keys to use for the data adapter
        /// </summary>
        public string[] DefaultDataAdapterKeys => new[] { "assets" };

        /// <summary>
        /// The setup method determines how the connector should obtain access and refresh tokens.
        /// When the method is `Setup`, please feel free to implement `IConnector` with a custom configuration.
        /// When `Redirect` the connector can inherit from `OAuth2Connector` or `OAuth2AuthenticationCodeFlowWithPKCEConnector` as a starting point 
        /// </summary>
        public ConnectorSetupMethod SetupMethod => ConnectorSetupMethod.Setup;

        /// <summary>
        /// Indicates, if this connector should not be set up by an end user but by an IT system administrator (e.g. because there needs to be some auth set-ups in the target system)
        /// </summary>
        public bool IsAdvanced => false;
    }
}
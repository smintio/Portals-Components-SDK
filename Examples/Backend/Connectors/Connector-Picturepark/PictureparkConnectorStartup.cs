using Microsoft.Extensions.DependencyInjection;
using SmintIo.Portals.Connector.Picturepark.Resources;
using SmintIo.Portals.ConnectorSDK.Connectors;
using SmintIo.Portals.SDK.Core.Models.Strings;
using System;

namespace SmintIo.Portals.Connector.Picturepark
{
    public class PictureparkConnectorStartup : IConnectorStartup
    {
        public const string PictureparkConnector = "picturepark";

        public string Key => PictureparkConnector;

        public string[] DefaultDataAdapterKeys { get; } = ["assets"];

        public ConnectorSetupMethod SetupMethod => ConnectorSetupMethod.Setup;

        public LocalizedStringsModel Name { get; } = new ResourceLocalizedStringsModel(nameof(Resources.ConfigurationMessages.c_picturepark_name));

        public LocalizedStringsModel Description { get; } = new ResourceLocalizedStringsModel(nameof(Resources.ConfigurationMessages.c_picturepark_description));

        public LocalizedStringsModel SetupDocumentationUrl { get; } = new ResourceLocalizedStringsModel(nameof(Resources.ConfigurationMessages.c_picturepark_setup_documentation_url));

        public bool IsAdvanced => true;

        public string LogoUrl => "https://www.smint.io/images/connectors/picturepark.png";

        public string IconUrl => "https://www.smint.io/images/connectors/picturepark_icon_large.png";

        public void ConfigureServices(IServiceCollection services)
        {

        }

        public Type ComponentImplementation => typeof(PictureparkConnector);

        public Type ConfigurationImplementation => typeof(PictureparkConnectorConfiguration);

        public Type ConfigurationMessages => typeof(ConfigurationMessages);

        public Type MetamodelMessages => typeof(MetamodelMessages);
    }
}

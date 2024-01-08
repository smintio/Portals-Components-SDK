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

        public string[] DefaultDataAdapterKeys => new string[] { "assets" };

        public ConnectorSetupMethod SetupMethod => ConnectorSetupMethod.Setup;

        public LocalizedStringsModel Name => new LocalizedStringsModel()
        {
            { LocalizedStringsModel.DefaultCulture, "Picturepark Content Platform" }
        };

        public LocalizedStringsModel Description => new LocalizedStringsModel()
        {
            { LocalizedStringsModel.DefaultCulture, "Connects Smint.io Portals to the Picturepark Content Platform." },
            { "de", "Verbindet Smint.io Portals mit der Picturepark Content Platform." }
        };

        public LocalizedStringsModel SetupDocumentationUrl => new LocalizedStringsModel()
        {
            { LocalizedStringsModel.DefaultCulture, "https://www.picturepark.com" }
        };

        public bool IsAdvanced => true;

        public string LogoUrl => "https://www.smint.io/images/connectors/picturepark.png";

        public string IconUrl => "https://www.smint.io/images/connectors/picturepark_icon_large.png";

        public void ConfigureServices(IServiceCollection services)
        {

        }

        public Type ComponentImplementation => typeof(PictureparkConnector);

        public Type ConfigurationImplementation => typeof(PictureparkConnectorConfiguration);

        public Type ConfigurationMessages => typeof(ConfigurationMessages);

        public Type MetamodelMessages => null;
    }
}

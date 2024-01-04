using System;
using Microsoft.Extensions.DependencyInjection;
using SmintIo.Portals.Connector.SharePoint.Resources;
using SmintIo.Portals.ConnectorSDK.Connectors;
using SmintIo.Portals.SDK.Core.Models.Strings;

namespace SmintIo.Portals.Connector.SharePoint
{
    public class SharepointConnectorStartup : IConnectorStartup
    {
        public const string SharepointConnector = "sharepoint";
        public string Key => SharepointConnector;

        public LocalizedStringsModel Name => new LocalizedStringsModel()
        {
            {LocalizedStringsModel.DefaultCulture, "Microsoft SharePoint"}
        };

        public LocalizedStringsModel Description => new LocalizedStringsModel()
        {
            {LocalizedStringsModel.DefaultCulture, "Connects Smint.io Portals to Microsoft SharePoint."},
            {"de", "Verbindet Smint.io Portals mit Microsoft SharePoint."}
        };

        public string LogoUrl => "https://www.smint.io/images/connectors/sharepoint.png";

        public string IconUrl => "https://www.smint.io/images/connectors/sharepoint_icon_large.png";

        public Type ConfigurationImplementation => typeof(SharepointConnectorConfiguration);
        public Type ComponentImplementation => typeof(SharepointConnector);

        public Type ConfigurationMessages => typeof(ConfigurationMessages);

        public void ConfigureServices(IServiceCollection services)
        {
        }

        public string[] DefaultDataAdapterKeys => new[] { "assets" };

        public ConnectorSetupMethod SetupMethod => ConnectorSetupMethod.Redirect;

        public bool IsAdvanced =>
            true; //once we have the AuthorizationCodeProvider every user can setup the connector on their own

        public LocalizedStringsModel SetupDocumentationUrl => new LocalizedStringsModel()
        {
            { LocalizedStringsModel.DefaultCulture, "https://www.microsoft.com/microsoft-365/sharepoint" }
        };
    }
}
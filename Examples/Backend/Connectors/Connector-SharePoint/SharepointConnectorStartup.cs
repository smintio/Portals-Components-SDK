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

        public LocalizedStringsModel Name { get; } = new ResourceLocalizedStringsModel(nameof(Resources.ConfigurationMessages.c_sharepoint_name));

        public LocalizedStringsModel Description { get; } = new ResourceLocalizedStringsModel(nameof(Resources.ConfigurationMessages.c_sharepoint_description));        

        public string LogoUrl => "https://cdn.smint.io/images/connectors/sharepoint.png";

        public string IconUrl => "https://cdn.smint.io/images/connectors/sharepoint_icon_large.png";

        public Type ConfigurationImplementation => typeof(SharepointConnectorConfiguration);
        public Type ComponentImplementation => typeof(SharepointConnector);

        public Type ConfigurationMessages => typeof(ConfigurationMessages);

        public Type MetamodelMessages => typeof(MetamodelMessages);

        public void ConfigureServices(IServiceCollection services)
        {
        }

        public string[] DefaultDataAdapterKeys { get; } = ["assets"];

        public ConnectorSetupMethod SetupMethod => ConnectorSetupMethod.Redirect;

        public bool IsAdvanced =>
            true; //once we have the AuthorizationCodeProvider every user can setup the connector on their own

        public LocalizedStringsModel SetupDocumentationUrl { get; } = new ResourceLocalizedStringsModel(nameof(Resources.ConfigurationMessages.c_sharepoint_setup_documentation_url));
    }
}
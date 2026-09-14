using System;
using Microsoft.Extensions.DependencyInjection;
using SmintIo.Portals.Connector.SharePoint.Resources;
using SmintIo.Portals.ConnectorSDK.Connectors;
using SmintIo.Portals.SDK.Core.Models.Strings;

namespace SmintIo.Portals.Connector.SharePoint
{
    public class OneDriveConnectorStartup : IConnectorStartup
    {
        public const string OneDriveConnector = "onedrive";
        public string Key => OneDriveConnector;

        public LocalizedStringsModel Name { get; } = new ResourceLocalizedStringsModel(nameof(OneDriveConfigurationMessages.c_onedrive_name));

        public LocalizedStringsModel Description { get; } = new ResourceLocalizedStringsModel(nameof(OneDriveConfigurationMessages.c_onedrive_description));        

        public string LogoUrl => "https://cdn.smint.io/images/connectors/onedrive.png";

        public string IconUrl => "https://cdn.smint.io/images/connectors/onedrive_icon_large.png";

        public Type ConfigurationImplementation => typeof(OneDriveConnectorConfiguration);
        public Type ComponentImplementation => typeof(SharepointConnector);

        public Type ConfigurationMessages => typeof(OneDriveConfigurationMessages);

        public Type MetamodelMessages => typeof(OneDriveMetamodelMessages);

        public void ConfigureServices(IServiceCollection services)
        {
        }

        public string[] DefaultDataAdapterKeys { get; } = ["assets"];

        public ConnectorSetupMethod SetupMethod => ConnectorSetupMethod.Redirect;

        public bool IsAdvanced =>
            true; //once we have the AuthorizationCodeProvider every user can setup the connector on their own

        public LocalizedStringsModel SetupDocumentationUrl { get; } = new ResourceLocalizedStringsModel(nameof(OneDriveConfigurationMessages.c_onedrive_setup_documentation_url));
    }
}
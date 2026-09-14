using SmintIo.Portals.SDK.Core.Components;

namespace SmintIo.Portals.Connector.SharePoint
{
    public interface ISharepointOneDriveConnectorConfiguration : IComponentConfiguration
    {
        string SharepointUrl { get; set; }

        bool HighSecurityMode { get; set; }

        string TenantId { get; set; }

        string ClientId { get; set; }

        string ClientSecret { get; set; }

        string SiteId { get; set; }

        string SiteIdString { get; set; }

        string SiteDriveId { get; set; }

        string SiteDriveIdString { get; set; }

        string SiteListId { get; set; }

        string SiteListIdString { get; set; }

        string[] SiteFolderIds { get; set; }

        string[] SiteFolderIdsStrings { get; set; }
    }
}

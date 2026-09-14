namespace SmintIo.Portals.Connector.SharePoint.Models
{
    public class SiteResponse
    {
        public string Host { get; set; }

        public string SiteId { get; set; }

        public string WebId { get; set; }

        public string WebUrl { get; set; }

        public string DisplayName { get; set; }

        public string Id => $"{Host},{SiteId},{WebId}";
    }
}

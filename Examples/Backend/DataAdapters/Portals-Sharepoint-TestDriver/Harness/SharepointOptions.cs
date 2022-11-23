namespace SmintIo.Portals.ConnectorSDK.TestDriver.Sharepoint.Test.Harness
{
    public class SharepointOptions
    {
        public const string Name = "GraphApi";
        public string SiteId { get; set; }
        public SharepointFileOptions[] Files { get; set; }
        public SharepointFolderOptions[] Folders { get; set; }
    }
}
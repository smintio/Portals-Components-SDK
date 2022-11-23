using Newtonsoft.Json;

namespace SmintIo.Portals.DataAdapter.SharePoint.Assets.Models
{
    public class MediaServiceFastMetadata
    {
        [JsonProperty("media")]
        public MediaMetaData Media { get; set; }

        [JsonProperty("video")]
        public VideoMetaData Video { get; set; }
    }
}
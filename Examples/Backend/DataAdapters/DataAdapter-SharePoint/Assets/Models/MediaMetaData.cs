using Newtonsoft.Json;

namespace SmintIo.Portals.DataAdapter.SharePoint.Assets.Models
{
    public class MediaMetaData
    {
        [JsonProperty("duration")]
        public string Duration { get; set; }
    }
}
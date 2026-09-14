using Newtonsoft.Json;

namespace SmintIo.Portals.DataAdapter.SharePoint.Assets.Models
{
    public class VideoMetaData
    {
        [JsonProperty("altManifestMetadata")]
        public string AltManifestMetadata { get; set; }

        [JsonProperty("encodingBitrate")]
        public string EncodingBitrate { get; set; }

        [JsonProperty("fourCC")]
        public string FourCC { get; set; }

        [JsonProperty("frameHeight")]
        public string FrameHeight { get; set; }

        [JsonProperty("frameWidth")]
        public string FrameWidth { get; set; }

        [JsonProperty("manifestMetadata")]
        public string ManifestMetadata { get; set; }
    }
}
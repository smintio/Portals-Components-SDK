using System.Text.Json.Serialization;

namespace SmintIo.Portals.Connector.SharePoint.Models
{
    public class LookupModel
    {
        [JsonRequired]
        public int LookupId { get; set; }

        public string LookupValue { get; set; }

        public string Email { get; set; }
    }
}

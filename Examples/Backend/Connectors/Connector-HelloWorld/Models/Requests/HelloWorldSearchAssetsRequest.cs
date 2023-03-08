using System.Collections.Generic;
using SmintIo.Portals.Connector.HelloWorld.Models.Common;

namespace SmintIo.Portals.Connector.HelloWorld.Models.Requests
{
    public class HelloWorldSearchAssetsRequest
    {
        public string SearchQuery { get; set; }

        public ICollection<HelloWorldFacetFilterRequest> FacetFilters { get; set; }

        public ICollection<string> Ids { get; set; }

        public HelloWorldContentType? ContentType { get; set; }

        public int? Limit { get; set; }

        public int? Skip { get; set; }

        public string OrderBy { get; set; }
    }
}

using System.Collections.Generic;

namespace SmintIo.Portals.Connector.HelloWorld.Models.Responses
{
    public class HelloWorldSearchAssetsResponse
    {
        public ICollection<HelloWorldAssetResponse> Assets { get; set; }

        public ICollection<HelloWorldSearchFacetResponse> SearchFacets { get; set; }

        public int TotalCount { get; set; }
    }
}

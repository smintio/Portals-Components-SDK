using System.Collections.Generic;

namespace SmintIo.Portals.Connector.HelloWorld.Models.Responses
{
    public class HelloWorldSearchFacetResponse
    {
        public string Id { get; set; }

        public string Label { get; set; }

        public ICollection<HelloWorldSearchFacetValueResponse> SearchFacetValues { get; set; }
    }
}

using System.Collections.Generic;

namespace SmintIo.Portals.Connector.HelloWorld.Models.Responses
{
    public class HelloWorldAssetResponses
    {
        public ICollection<HelloWorldAssetResponse> Assets { get; set; }

        public int Count { get; set; }
    }
}

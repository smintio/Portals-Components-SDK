using System;

namespace SmintIo.Portals.Connector.HelloWorld.Models.Requests
{
    public class HelloWorldFacetFilterRequest
    {
        public string Id { get; set; }

        public string StringValue { get; set; }

        public DateTimeOffset? DateTimeValue { get; set; }

        public long? NumberValue { get; set; }
    }
}

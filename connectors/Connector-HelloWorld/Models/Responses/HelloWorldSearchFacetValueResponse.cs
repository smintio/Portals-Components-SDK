using System.Collections.Generic;

namespace SmintIo.Portals.Connector.HelloWorld.Models.Responses
{
    public class HelloWorldSearchFacetValueResponse
    {
        public string Id { get; set; }

        public string Label { get; set; }

        public IDictionary<string, string> LabelTranslationByCulture { get; set; }

        public string Value { get; set; }

        public int? Count { get; set; }
    }
}

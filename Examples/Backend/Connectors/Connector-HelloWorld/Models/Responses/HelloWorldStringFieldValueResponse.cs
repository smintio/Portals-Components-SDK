using System.Collections.Generic;

namespace SmintIo.Portals.Connector.HelloWorld.Models.Responses
{
    public class HelloWorldStringFieldValueResponse
    {
        public string Label { get; set; }

        public IDictionary<string, string> LabelTranslationByCulture { get; set; }
    }
}
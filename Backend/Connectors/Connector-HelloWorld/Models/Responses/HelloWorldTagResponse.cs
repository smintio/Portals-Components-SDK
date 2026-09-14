using System.Collections.Generic;

namespace SmintIo.Portals.Connector.HelloWorld.Models.Responses
{
    public class HelloWorldTagResponse
    {
        public string[] Labels { get; set; }

        public IDictionary<string, string[]> LabelsTranslationByCulture { get; set; }
    }
}

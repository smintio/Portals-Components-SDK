using System;

namespace SmintIo.Portals.Connector.HelloWorld.Models.Responses
{
    [Serializable]
    public class HelloWorldCustomFieldResponse
    {
        public string Id { get; set; }

        public string Label { get; set; }

        public HelloWorldCustomFieldType CustomFieldType { get; set; }

        public bool MultiOptions { get; set; }
    }
}

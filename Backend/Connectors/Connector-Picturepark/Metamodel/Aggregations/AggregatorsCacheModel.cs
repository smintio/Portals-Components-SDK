using System;
using System.Collections.Generic;

namespace SmintIo.Portals.Connector.Picturepark.Metamodel.Aggregations
{
    [Serializable]
    public class AggregatorsCacheModel
    {
        public Dictionary<string, string> AggregatorJsons { get; set; }

        public List<string> SortInfoJsons { get; set; }

        public List<string> SortFieldJsons { get; set; }
    }
}

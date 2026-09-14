using SmintIo.Portals.SDK.Core.Models.Strings;

namespace SmintIo.Portals.Connector.Picturepark.Metamodel.Aggregations
{
    public interface IAggregationNameProvider
    {
        LocalizedStringsModel GetNames(string aggregationName);
    }
}
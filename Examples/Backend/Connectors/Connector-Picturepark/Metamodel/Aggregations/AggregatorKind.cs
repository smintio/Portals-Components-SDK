
namespace SmintIo.Portals.Connector.Picturepark.Metamodel.Aggregations
{
    /// <summary>
    /// Lists the names of all available Picturepark aggregator types.
    /// </summary>
    public enum AggregatorKind
    {
        Unknown,
        DateRangeAggregator,
        GeoDistanceAggregator,
        NestedAggregator,
        NumericRangeAggregator,
        TermsAggregator,
        TermsRelationAggregator,
        TermsEnumAggregator
    }
}

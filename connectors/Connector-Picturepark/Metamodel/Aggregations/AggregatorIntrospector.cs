using Picturepark.SDK.V1.Contract;
using SmintIo.Portals.SDK.Core.Models.Strings;
using System;
using System.Linq;

namespace SmintIo.Portals.Connector.Picturepark.Metamodel.Aggregations
{
    /// <summary>
    /// An aggregator builds an aggregation with items. It takes an asset property's name, and
    /// based on the property's data type, it fetches all available instances of that type.
    /// 
    /// For example, an aggregator initialized with the field "contentType" will look for the
    /// property "contentType" inside the "Content" (oder "ContentDetail") and conclude that it is
    /// of type ContentType, which is enumeration. Then, it will collect all constants of that
    /// enumeration and return with the aggregation (the number of values retrieved is determined
    /// by the aggregator's property 'Size').
    /// 
    /// <see cref="TermsAggregator"/>: A multi-bucket value aggregator (creates multiple 
    /// buckets/aggregation-result-items).
    /// 
    /// <see cref="TermsRelationAggregator"/>: A multi-bucket value aggregator used for 
    /// aggregations on relation item IDs (key property is <see cref="TermsRelationAggregator.DocumentType"/>).
    /// 
    /// <see cref="TermsEnumAggregator"/>: A multi-bucket value aggregator used for aggregations
    /// on indexed enum values (key property is <see cref="TermsEnumAggregator.EnumType"/>).
    /// 
    ///                               AggregatorBase
    ///                                      |
    ///                               TermsAggregator
    ///                                      |
    ///                        +-------------+-------------+
    ///                        |                           |
    ///            TermsRelationAggregator         TermsEnumAggregator
    ///
    /// </summary>
    [Serializable]
    public class AggregatorIntrospector
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Aggregator"></param>
        /// <param name="kind"></param>
        /// <param name="size">Max number of buckets (aggregation-result-items)</param>
        /// <param name="searchString"></param>
        public AggregatorIntrospector(AggregatorBase aggregator, AggregatorKind kind, int? size, string searchString)
        {
            Aggregator = aggregator;
            Kind = kind;
            Size = size;
            SearchString = searchString;
        }

        public string Name { get { return Aggregator.Name; } }

        public AggregatorBase Aggregator { get; }

        public AggregatorKind Kind { get; }

        /// <summary>
        /// If the buckets are fetched lazily, this value might change with each click on
        /// "Show more" ("Mehr zeigen").
        /// 
        /// Note that not all aggregator kinds have a 'size' property (only <see cref="TermsAggregator"/>
        /// and its subclasses).
        /// </summary>
        public int? Size { get; set; }

        public string SearchString { get; set; }

        public LocalizedStringsModel Names
        {
            get
            {
                //
                // Here we use heuristics; if there is a nested aggregator, it usually contains
                // the human-friendly display names.
                //
                AggregatorBase aggregator = Aggregator.Aggregators?.FirstOrDefault();

                if (!(aggregator is null)) // If there exists any aggregator, it contains the display name
                {
                    return new LocalizedStringsModel(aggregator.Names);
                }
                return new LocalizedStringsModel(Aggregator.Names);
            }
        }

        public static AggregatorIntrospector GetIntrospector(AggregatorBase aggregator)
        {
            AggregatorKind kind = GetKind(aggregator);

            switch (kind)
            {
                case AggregatorKind.TermsAggregator:
                case AggregatorKind.TermsRelationAggregator:
                case AggregatorKind.TermsEnumAggregator:
                    TermsAggregator ta = (TermsAggregator)aggregator;
                    return new AggregatorIntrospector(aggregator, kind, ta.Size, ta.SearchString);
                default:
                    return new AggregatorIntrospector(aggregator, kind, -1, null);
            }
        }

        private static AggregatorKind GetKind(AggregatorBase aggregator)
        {
            switch (aggregator.GetType().Name)
            {
                case nameof(DateRangeAggregator): return AggregatorKind.DateRangeAggregator;
                case nameof(GeoDistanceAggregator): return AggregatorKind.GeoDistanceAggregator;
                case nameof(NestedAggregator): return AggregatorKind.NestedAggregator;
                case nameof(NumericRangeAggregator): return AggregatorKind.NumericRangeAggregator;
                case nameof(TermsAggregator): return AggregatorKind.TermsAggregator;
                case nameof(TermsRelationAggregator): return AggregatorKind.TermsRelationAggregator;
                case nameof(TermsEnumAggregator): return AggregatorKind.TermsEnumAggregator;
                default: return AggregatorKind.Unknown;
            }
        }
    }
}
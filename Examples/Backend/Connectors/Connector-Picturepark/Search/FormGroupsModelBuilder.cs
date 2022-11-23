using Picturepark.SDK.V1.Contract;
using SmintIo.Portals.Connector.Picturepark.Metamodel.Aggregations;
using SmintIo.Portals.SDK.Core.Configuration.Model;
using System.Collections.Generic;

namespace SmintIo.Portals.Connector.Picturepark.Search
{
    public class FormGroupsModelBuilder
    {
        public FormGroupsModelBuilder(IAggregationNameProvider aggregationNameProvider)
        {
            AggregationNameProvider = aggregationNameProvider;
        }

        private IAggregationNameProvider AggregationNameProvider { get; }

        public FormGroupsDefinitionModel Build(ICollection<AggregatorBase> aggregators)
        {
            RawFormGroupsModelBuilder builder = new RawFormGroupsModelBuilder(AggregationNameProvider);
            return builder.Build(aggregators);
        }

        public FormGroupsDefinitionModel Build(ICollection<AggregatorBase> aggregators, ICollection<AggregationResult> aggregationResults, int multiSelectItemCount)
        {
            AggregationResultBasedFormGroupsModelBuilder builder = new AggregationResultBasedFormGroupsModelBuilder(AggregationNameProvider);
            return builder.Build(aggregators, aggregationResults, multiSelectItemCount);
        }
    }
}
using Picturepark.SDK.V1.Contract;
using SmintIo.Portals.SDK.Core.Cache;
using SmintIo.Portals.SDK.Core.Models.Strings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmintIo.Portals.Connector.Picturepark.Metamodel.Aggregations
{
    /// <summary>
    /// This class manages aggregators of a Picturepark channel and facilitates access to aggregators.
    /// 
    /// Picturepark channels's consists of one or multiple aggregators; each aggregator
    /// defines a single asset property on which the aggregations are built. For each distinct
    /// value of the property available in the system, the aggregator counts how many assets
    /// in the system have that property initialized with a particular value.
    /// 
    /// For example, an aggregator could be defined for the asset's basic property "contentType"
    /// of the enumeration type "ContentType". For each distinct value of that enumeration 
    /// (Bitmap, Audio, Video, Document, etc..) the aggregator counts how many assets have their
    /// property "contentType" initialized with that particular value.
    /// 
    /// For each of those distinct values, an aggregation is built.
    /// 
    /// Thus a single aggregation consists of a distinct value of the property's type and the
    /// value representing how many assets have that particular value set.
    /// 
    /// Aggregators could have a filter, which reduces the initial result set which the 
    /// aggregations are built from.
    /// </summary>
    [Serializable]
    public class AggregatorManager : IAggregationNameProvider
    {
        public const int MaxAggregationCount = 15;

        public AggregatorManager()
        {

        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="aggregators"></param>
        public AggregatorManager(ICollection<AggregatorBase> aggregators, ICollection<SortInfo> sortInfos, ICollection<SortField> sortFields)
        {
            AggregatorIntrospectors = aggregators.ToDictionary(a => a.Name, a => AggregatorIntrospector.GetIntrospector(a));
            SortInfos = sortInfos;
            SortFields = sortFields;
        }

        public ICollection<AggregatorBase> Aggregators => AggregatorIntrospectors.Select(i => i.Value.Aggregator).ToList();

        private IDictionary<string, AggregatorIntrospector> AggregatorIntrospectors { get; set; }
        private ICollection<SortInfo> SortInfos { get; set; }
        private ICollection<SortField> SortFields { get; set; }

        public async Task CacheAggregatorsAsync(ICache cache)
        {
            var aggregatorsCacheModel = new AggregatorsCacheModel()
            {
                AggregatorJsons = new Dictionary<string, string>(),
                SortInfoJsons = new List<string>(),
                SortFieldJsons = new List<string>()
            };

            if (Aggregators != null && Aggregators.Count > 0)
            {
                foreach (var aggregator in Aggregators)
                {
                    ProcessAggregator(aggregator);

                    var aggregatorJson = aggregator.ToJson();

                    aggregatorsCacheModel.AggregatorJsons[aggregator.Name] = aggregatorJson;
                }
            }

            if (SortInfos != null && SortInfos.Count > 0)
            {
                foreach (var sortInfo in SortInfos)
                {
                    var sortInfoJson = sortInfo.ToJson();

                    aggregatorsCacheModel.SortInfoJsons.Add(sortInfoJson);
                }
            }

            if (SortFields != null && SortFields.Count > 0)
            {
                foreach (var sortField in SortFields)
                {
                    var sortFieldJson = sortField.ToJson();

                    aggregatorsCacheModel.SortFieldJsons.Add(sortFieldJson);
                }
            }

            await cache.StoreAsync("aggregators", aggregatorsCacheModel);
        }

        private void ProcessAggregator(AggregatorBase aggregator)
        {
            if (aggregator.Aggregators != null && aggregator.Aggregators.Count > 0)
            { 
                foreach (var subAggregator in aggregator.Aggregators)
                {
                    ProcessAggregator(subAggregator);
                }
            }
            
            if (aggregator is TermsAggregator termsAggregator)
            {
                // terms relation aggregator etc. all are subclasses to this one

                termsAggregator.Size = MaxAggregationCount;
            }
        }

        public void ResizeAggregator(AggregatorBase aggregator, int size)
        {
            if (aggregator.Aggregators != null && aggregator.Aggregators.Count > 0)
            {
                foreach (var subAggregator in aggregator.Aggregators)
                {
                    ResizeAggregator(subAggregator, size);
                }
            }

            if (aggregator is TermsAggregator termsAggregator)
            {
                // terms relation aggregator etc. all are subclasses to this one

                termsAggregator.Size = size;
            }
        }

        public async Task InitializeFromCacheAsync(ICache cache)
        {
            var aggregatorsCacheModel = await cache.GetAsync<AggregatorsCacheModel>("aggregators").ConfigureAwait(false);

            AggregatorIntrospectors = new Dictionary<string, AggregatorIntrospector>();
            
            SortInfos = new List<SortInfo>();
            SortFields = new List<SortField>();

            if (aggregatorsCacheModel == null)
                return;

            if (aggregatorsCacheModel.AggregatorJsons != null && aggregatorsCacheModel.AggregatorJsons.Count > 0)
            {
                foreach (var aggregatorName in aggregatorsCacheModel.AggregatorJsons.Keys)
                {
                    if (string.IsNullOrEmpty(aggregatorsCacheModel.AggregatorJsons[aggregatorName]))
                        continue;

                    try
                    {
                        var aggregatorBase = AggregatorBase.FromJson(aggregatorsCacheModel.AggregatorJsons[aggregatorName]);

                        AggregatorIntrospectors.Add(aggregatorName, AggregatorIntrospector.GetIntrospector(aggregatorBase));
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }

            if (aggregatorsCacheModel.SortInfoJsons != null && aggregatorsCacheModel.SortInfoJsons.Count > 0)
            {
                foreach (var sortInfoJson in aggregatorsCacheModel.SortInfoJsons)
                {
                    try
                    {
                        var sortInfo = SortInfo.FromJson(sortInfoJson);

                        SortInfos.Add(sortInfo);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }

            if (aggregatorsCacheModel.SortFieldJsons != null && aggregatorsCacheModel.SortFieldJsons.Count > 0)
            {
                foreach (var sortFieldJson in aggregatorsCacheModel.SortFieldJsons)
                {
                    try
                    {
                        var sortField = SortField.FromJson(sortFieldJson);

                        SortFields.Add(sortField);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }
        }

        public AggregatorBase GetAggregator(string name, int? maxResultCount)
        {
            return GetAggregator(Aggregators, name, maxResultCount);
        }

        private AggregatorBase GetAggregator(ICollection<AggregatorBase> aggregators, string name, int? maxResultCount)
        { 
            if (aggregators == null || aggregators.Count == 0)
                return null;

            foreach (var aggregator in aggregators)
            {
                var aggregatorName = aggregator.Name;

                if (aggregator is NestedAggregator nestedAggregator)
                {
                    var foundNestedAggregator = GetAggregator(nestedAggregator.Aggregators, name, maxResultCount);

                    if (foundNestedAggregator != null)
                        return foundNestedAggregator;
                }
                else if (string.Equals(aggregatorName, name))
                {
                    if (aggregator is TermsAggregator termsAggregator && maxResultCount != null)
                    {
                        // terms relation aggregator etc. all are subclasses to this one

                        termsAggregator.Size = maxResultCount;
                    }

                    return aggregator;
                }
            }

            return null;
        }

        public LocalizedStringsModel GetNames(string aggregationName)
        {
            AggregatorIntrospector introspector;

            if (AggregatorIntrospectors.TryGetValue(aggregationName, out introspector))
            {
                return introspector.Names;
            }

            return null;
        }

        public SortInfo GetDefaultSortInfo()
        {
            var sortInfo = SortInfos.FirstOrDefault();

            if (sortInfo == null)
            {
                return new SortInfo()
                {
                    Field = "_score",
                    Direction = SortDirection.Desc
                };
            }

            return sortInfo;
        }

        public ICollection<SortField> GetSortFields()
        {
            return SortFields != null ? SortFields : new List<SortField>();
        }
    }
}
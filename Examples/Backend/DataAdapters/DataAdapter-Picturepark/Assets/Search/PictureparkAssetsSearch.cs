using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;
using SmintIo.Portals.Connector.Picturepark.Client;
using SmintIo.Portals.Connector.Picturepark.Search;
using SmintIo.Portals.DataAdapter.Picturepark.Assets.Common;
using SmintIo.Portals.DataAdapter.Picturepark.Assets.Extensions;
using SmintIo.Portals.DataAdapter.Picturepark.Resources;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Constants;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Impl;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets.Models;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets.Parameters;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets.Results;
using SmintIo.Portals.SDK.Core.Configuration;
using SmintIo.Portals.SDK.Core.Configuration.Model;
using SmintIo.Portals.SDK.Core.Extensions;
using SmintIo.Portals.SDK.Core.Helpers;
using SmintIo.Portals.SDK.Core.Models.Strings;

namespace SmintIo.Portals.DataAdapter.Picturepark.Assets
{
    public partial class PictureparkAssetsDataAdapter : AssetsDataAdapterBaseImpl
    {
        public override Task<GetAssetsSearchFeatureSupportResult> GetFeatureSupportAsync(GetAssetsSearchFeatureSupportParameters parameters)
        {
            return Task.FromResult(new GetAssetsSearchFeatureSupportResult()
            {
                IsRandomAccessSupported = false,
                IsFullTextSearchProposalsSupported = true,
                IsFolderNavigationSupported = false
            });
        }

        public override Task<GetAssetsReadFeatureSupportResult> GetAssetsReadFeatureSupportAsync(GetAssetsReadFeatureSupportParameters parameters)
        {
            var featureSupport = new GetAssetsReadFeatureSupportResult
            {
                IsFastGetAssetsSupported = true
            };

            return Task.FromResult(featureSupport);
        }

        public override async Task<GetFullTextSearchProposalsResult> GetFullTextSearchProposalsAsync(GetFullTextSearchProposalsParameters parameters)
        {
            var assets = await SearchAssetsAsync(new SearchAssetsParameters()
            {
                QueryString = parameters?.SearchQueryString,
                CurrentFilters = parameters?.CurrentFilters,
                PageSize = parameters?.MaxResultCount
            }).ConfigureAwait(false);

            var cultureInfo = CultureInfo.CurrentCulture;

            return new GetFullTextSearchProposalsResult
            {
                FullTextProposals = assets.AssetDataObjects
                    .Where(assetDataObject => assetDataObject.Name != null)
                    .Select(assetDataObject => assetDataObject.Name.ResolveLocalizedString(cultureInfo))
                    .ToArray()
            };
        }

        public override async Task<GetFormItemDefinitionAllowedValuesResult> GetFormItemDefinitionAllowedValuesAsync(GetFormItemDefinitionAllowedValuesParameters parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            await _client.InitializeChannelAggregatorsAsync().ConfigureAwait(false);

            FilterBase filter = null;
            var aggregationFilters = new List<AggregationFilter>();

            string searchType = null;

            if (parameters != null && parameters.CurrentFilters != null && parameters.CurrentFilters.Values != null && parameters.CurrentFilters.Values.Count > 0)
            {
                foreach (var formFieldValueModel in parameters.CurrentFilters.Values)
                {
                    var id = formFieldValueModel.Id;

                    if (string.Equals(id, FormConstants.DefaultSmintIoSearchTypeFormFieldValueKey) &&
                        formFieldValueModel.DataType == ValueTypeEnum.String &&
                        !string.IsNullOrEmpty(formFieldValueModel.StringValue))
                    {
                        searchType = formFieldValueModel.StringValue;

                        continue;
                    }

                    if (formFieldValueModel.DataType == ValueTypeEnum.String)
                    {
                        var filterJson = formFieldValueModel.StringValue;

                        if (string.IsNullOrEmpty(filterJson))
                            continue;

                        try
                        {
                            if (string.Equals(formFieldValueModel.Hint, FormConstants.FormFilterHint))
                            {
                                filter = FilterBase.FromJson(filterJson);
                            }
                            else
                            {
                                aggregationFilters.Add(AggregationFilter.FromJson(filterJson));
                            }
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }
                    else if (formFieldValueModel.DataType == ValueTypeEnum.StringArray)
                    {
                        var filtersJson = formFieldValueModel.StringArrayValue;

                        if (filtersJson == null || filtersJson.Length == 0)
                            continue;

                        foreach (var filterJson in filtersJson)
                        {
                            if (string.IsNullOrEmpty(filterJson))
                                continue;

                            try
                            {
                                if (string.Equals(formFieldValueModel.Hint, FormConstants.FormFilterHint))
                                {
                                    filter = FilterBase.FromJson(filterJson);
                                }
                                else
                                {
                                    aggregationFilters.Add(AggregationFilter.FromJson(filterJson));
                                }
                            }
                            catch (Exception)
                            {
                                continue;
                            }
                        }
                    }
                }
            }

            if (parameters.ContentType.HasValue)
            {
                var pictureparkContentTypes = parameters.ContentType.GetPictureparkContentTypes();

                var contentTypeAggregationFilter = new AggregationFilter()
                {
                    Filter = new TermsFilter
                    {
                        Field = nameof(ContentDetail.ContentType).ToLowerCamelCase(),
                        Terms = pictureparkContentTypes
                    }
                };

                var selectedContentTypeAggregationFilter = aggregationFilters.FirstOrDefault(af => af.AggregationName == "contentType");

                if (selectedContentTypeAggregationFilter == null)
                {
                    aggregationFilters.Add(contentTypeAggregationFilter);
                }
                else
                {
                    var orFilter = new OrFilter()
                    {
                        Filters = new List<FilterBase>()
                        {
                            selectedContentTypeAggregationFilter.Filter,
                            contentTypeAggregationFilter.Filter
                        }
                    };

                    selectedContentTypeAggregationFilter.Filter = orFilter;
                }
            }

            SearchType? currentSearchType = null;

            switch (searchType)
            {
                case "SimpleOr":
                    currentSearchType = SearchType.SimpleOr;
                    break;
                case "Advanced":
                    currentSearchType = SearchType.SimpleAnd;
                    break;
                case "SimpleAnd":
                    break;

                default:
                    currentSearchType = SearchType.SimpleAnd;
                    break;
            }

            var aggregatorManager = await _client.GetAggregatorManagerAsync().ConfigureAwait(false);

            var aggregator = aggregatorManager.GetAggregator(parameters.FormItemDefinitionId, parameters.MaxResultCount);

            if (aggregator == null)
            {
                return new GetFormItemDefinitionAllowedValuesResult()
                {
                    AllowedValues = new List<ValueForJsonUiDetailsModel>(),
                    AllowedValuesTotalCount = 0
                };
            }

            if (aggregator is TermsAggregator termsAggregator)
            {
                termsAggregator.SearchString = parameters.FormItemDefinitionQueryString;
            }

            var (response, _) = await _client.SearchContentAsync(
                parameters?.SearchQueryString,
                aggregatorManager.Aggregators,
                filter,
                aggregationFilters,
                parameters?.SearchResultSetUuid,
                pageSize: 0,
                Array.Empty<SortInfo>(),
                currentSearchType,
                includeFulltext: true).ConfigureAwait(false);

            FormGroupsModelBuilder builder = new FormGroupsModelBuilder(aggregatorManager);
            IFormGroupsDefinitionModel formGroupsModel = builder.Build(aggregatorManager.Aggregators, response.AggregationResults, _configuration.MultiSelectItemCount);

            if (formGroupsModel.FormGroupDefinitions != null && formGroupsModel.FormGroupDefinitions.Count > 0)
            {
                foreach (var formGroupModel in formGroupsModel.FormGroupDefinitions)
                {
                    if (formGroupModel.FormItemDefinitions == null || formGroupModel.FormItemDefinitions.Count == 0)
                        continue;

                    foreach (var formItemModel in formGroupModel.FormItemDefinitions)
                    {
                        if (string.Equals(formItemModel.Id, parameters.FormItemDefinitionId))
                        {
                            return new GetFormItemDefinitionAllowedValuesResult()
                            {
                                AllowedValues = formItemModel.AllowedValues,
                                AllowedValuesTotalCount = formItemModel.AllowedValuesTotalCount
                            };
                        }
                    }
                }
            }

            return new GetFormItemDefinitionAllowedValuesResult()
            {
                AllowedValues = new List<ValueForJsonUiDetailsModel>(),
                AllowedValuesTotalCount = 0
            };
        }

        /// <summary>
        /// TODO:
        /// Add sort-info parameter to the method. The sorting informations come together with the channel from Picturepark
        /// and are to be cached together with the channel. The problem with this is that currently the <see cref="ChannelDescriptor"/> 
        /// class is in Connector-Picturepark project, whereas the sorting-related classes are placed in DataAdapter-SDK. So those
        /// classes should be moved to Connector-SDK.
        /// </summary>
        /// <param name="queryString"></param>
        /// <param name="filters"></param>
        /// <param name="searchResultSetUuid"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public override async Task<SearchAssetsResult> SearchAssetsAsync(SearchAssetsParameters parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            await _client.InitializeChannelAggregatorsAsync().ConfigureAwait(false);

            var aggregatorManager = await _client.GetAggregatorManagerAsync().ConfigureAwait(false);

            FilterBase filter = null;
            var aggregationFilters = new List<AggregationFilter>();

            string searchType = null;
            string sortBy = null;
            string sortOrder = null;

            if (parameters != null && parameters.CurrentFilters != null && parameters.CurrentFilters.Values != null && parameters.CurrentFilters.Values.Count > 0)
            {
                foreach (var formFieldValueModel in parameters.CurrentFilters.Values)
                {
                    var id = formFieldValueModel.Id;

                    if (string.Equals(id, FormConstants.DefaultSmintIoSearchTypeFormFieldValueKey) &&
                        formFieldValueModel.DataType == ValueTypeEnum.String &&
                        !string.IsNullOrEmpty(formFieldValueModel.StringValue))
                    {
                        searchType = formFieldValueModel.StringValue;

                        continue;
                    }

                    if (string.Equals(id, FormConstants.DefaultSmintIoSortByFormFieldValueKey) &&
                        formFieldValueModel.DataType == ValueTypeEnum.String &&
                        !string.IsNullOrEmpty(formFieldValueModel.StringValue))
                    {
                        sortBy = formFieldValueModel.StringValue;

                        continue;
                    }

                    if (string.Equals(id, FormConstants.DefaultSmintIoSortOrderFormFieldValueKey) &&
                       formFieldValueModel.DataType == ValueTypeEnum.String &&
                       !string.IsNullOrEmpty(formFieldValueModel.StringValue))
                    {
                        sortOrder = formFieldValueModel.StringValue;

                        continue;
                    }

                    if (formFieldValueModel.DataType == ValueTypeEnum.String)
                    {
                        var filterJson = formFieldValueModel.StringValue;

                        if (string.IsNullOrEmpty(filterJson))
                            continue;

                        try
                        {
                            if (string.Equals(formFieldValueModel.Hint, FormConstants.FormFilterHint))
                            {
                                filter = FilterBase.FromJson(filterJson);
                            }
                            else
                            {
                                aggregationFilters.Add(AggregationFilter.FromJson(filterJson));
                            }
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }
                    else if (formFieldValueModel.DataType == ValueTypeEnum.StringArray)
                    {
                        var filtersJson = formFieldValueModel.StringArrayValue;

                        if (filtersJson == null || filtersJson.Length == 0)
                            continue;

                        foreach (var filterJson in filtersJson)
                        {
                            if (string.IsNullOrEmpty(filterJson))
                                continue;

                            try
                            {
                                if (string.Equals(formFieldValueModel.Hint, FormConstants.FormFilterHint))
                                {
                                    filter = FilterBase.FromJson(filterJson);
                                }
                                else
                                {
                                    aggregationFilters.Add(AggregationFilter.FromJson(filterJson));
                                }
                            }
                            catch (Exception)
                            {
                                continue;
                            }
                        }
                    }
                }
            }

            if (parameters.ContentType.HasValue)
            {
                var pictureparkContentTypes = parameters.ContentType.GetPictureparkContentTypes();

                var contentTypeAggregationFilter = new AggregationFilter()
                {
                    Filter = new TermsFilter
                    {
                        Field = nameof(ContentDetail.ContentType).ToLowerCamelCase(),
                        Terms = pictureparkContentTypes
                    }
                };

                var selectedContentTypeAggregationFilter = aggregationFilters.FirstOrDefault(af => af.AggregationName == "contentType");

                if (selectedContentTypeAggregationFilter == null)
                {
                    aggregationFilters.Add(contentTypeAggregationFilter);
                }
                else
                {
                    var orFilter = new OrFilter()
                    {
                        Filters = new List<FilterBase>()
                        {
                            selectedContentTypeAggregationFilter.Filter,
                            contentTypeAggregationFilter.Filter
                        }
                    };

                    selectedContentTypeAggregationFilter.Filter = orFilter;
                }
            }

            SearchType? currentSearchType = null;

            switch (searchType)
            {
                case "SimpleOr":
                    currentSearchType = SearchType.SimpleOr;
                    break;
                case "Advanced":
                    currentSearchType = SearchType.Advanced;
                    break;
                case "SimpleAnd":
                    break;

                default:
                    currentSearchType = SearchType.SimpleAnd;
                    searchType = "SimpleAnd";

                    break;
            }

            string defaultSortOrder;

            var defaultSortInfo = aggregatorManager.GetDefaultSortInfo();

            if (string.IsNullOrEmpty(sortBy))
            {
                sortBy = defaultSortInfo.Field;

                defaultSortOrder = defaultSortInfo.Direction == SortDirection.Asc ? "asc" : "desc";
            }
            else
            {
                switch (sortBy)
                {
                    case "_score":
                    case "audit.creationDate":
                    case "audit.modificationDate":
                        defaultSortOrder = "desc";
                        break;
                    case "fileMetadata.fileName":
                        defaultSortOrder = "asc";
                        break;

                    default:
                        // some other sort value
                        // unfortunately Picturepark does not provide a sort order for the sort fields
                        // so we need to guess

                        defaultSortOrder = defaultSortInfo.Direction == SortDirection.Asc ? "asc" : "desc";

                        break;
                }
            }


            switch (sortOrder)
            {
                case "asc":
                case "desc":
                    break;
                default:
                    sortOrder = defaultSortOrder;
                    break;
            }

            var sortInfo = new SortInfo()
            {
                Field = sortBy,
                Direction = string.Equals(sortOrder, "asc") ? SortDirection.Asc : SortDirection.Desc
            };

            var aggregators = aggregatorManager.Aggregators;

            foreach (var aggregator in aggregators)
            {
                aggregatorManager.ResizeAggregator(aggregator, _configuration.MultiSelectItemCount);
            }

            int pageSize = parameters?.PageSize ?? _configuration.DefaultPageSize;

            // maximum set by Picturepark is 10000
            // but we say 500

            if (pageSize > 500)
                pageSize = 500;

            var sanitizedQuery = StringHelpers.Sanitize(parameters?.QueryString ?? string.Empty);

            var (response, contentDetails) = await _client.SearchContentAsync(
                sanitizedQuery,
                aggregators,
                filter,
                aggregationFilters,
                parameters?.SearchResultSetUuid,
                pageSize,
                new SortInfo[] { sortInfo },
                currentSearchType,
                includeFulltext: true,
                resolveMetadata: true).ConfigureAwait(false);

            FormGroupsModelBuilder builder = new FormGroupsModelBuilder(aggregatorManager);
            IFormGroupsDefinitionModel formGroupsModel = builder.Build(aggregatorManager.Aggregators, response.AggregationResults, _configuration.MultiSelectItemCount);

            var searchTypeModified = currentSearchType != null && currentSearchType != SearchType.SimpleAnd;
            var sortByModified = !string.Equals(sortBy, defaultSortInfo.Field);
            var sortOrderModified = !string.Equals(sortOrder, defaultSortOrder);

            formGroupsModel.FormGroupDefinitions.Add(new FormGroupDefinitionModel()
            {
                Id = FormConstants.DefaultSmintIoSearchTypeFormGroupKey,
                Name = MetamodelMessages.da_assets_search_type.Localize(),
                IsModified = searchTypeModified,
                FormItemDefinitions = new List<FormFieldItemDefinitionModel>()
                {
                    new FormFieldItemDefinitionModel()
                    {
                        Id = FormConstants.DefaultSmintIoSearchTypeFormFieldValueKey,
                        DataType = ValueTypeEnum.String,
                        DefaultValue = new ValueForJson()
                        {
                            StringValue = "SimpleAnd"
                        },
                        CurrentValue = new ValueForJson()
                        {
                            StringValue = searchType
                        },
                        IsModified = searchTypeModified,
                        AllowedValues = new List<ValueForJsonUiDetailsModel>()
                        {
                            new ValueForJsonUiDetailsModel()
                            {
                                Value = new ValueForJson() {
                                    StringValue = "SimpleAnd"
                                },
                                Name = MetamodelMessages.da_assets_simple_and.Localize()
                            },
                            new ValueForJsonUiDetailsModel()
                            {
                                Value = new ValueForJson() {
                                    StringValue = "SimpleOr"
                                },
                                Name = MetamodelMessages.da_assets_simple_or.Localize()
                            },
                            new ValueForJsonUiDetailsModel()
                            {
                                Value = new ValueForJson() {
                                    StringValue = "Advanced"
                                },
                                Name = MetamodelMessages.da_assets_advanced.Localize()
                            }
                        }
                    },
                }
            });

            var sortFieldsAllowedValues = new List<ValueForJsonUiDetailsModel>();

            foreach (var sortField in aggregatorManager.GetSortFields())
            {
                sortFieldsAllowedValues.Add(new ValueForJsonUiDetailsModel()
                {
                    Value = new ValueForJson()
                    {
                        StringValue = sortField.Path
                    },
                    Name = sortField.Names.ConvertToLocalizedStringsModel()
                });
            }

            formGroupsModel.FormGroupDefinitions.Add(new FormGroupDefinitionModel()
            {
                Id = FormConstants.DefaultSmintIoSortByFormGroupKey,
                Name = DataAdapterSDK.Resources.MetamodelMessages.da_form_group_definition_sort_by.Localize(),
                IsModified = sortByModified,
                FormItemDefinitions = new List<FormFieldItemDefinitionModel>()
                {
                    new FormFieldItemDefinitionModel()
                    {
                        Id = FormConstants.DefaultSmintIoSortByFormFieldValueKey,
                        DataType = ValueTypeEnum.String,
                        DefaultValue = new ValueForJson()
                        {
                            StringValue = defaultSortInfo.Field
                        },
                        CurrentValue = new ValueForJson()
                        {
                            StringValue = sortBy
                        },
                        IsModified = sortByModified,
                        AllowedValues = sortFieldsAllowedValues
                    },
                }
            });

            formGroupsModel.FormGroupDefinitions.Add(new FormGroupDefinitionModel()
            {
                Id = FormConstants.DefaultSmintIoSortOrderFormGroupKey,
                Name = DataAdapterSDK.Resources.MetamodelMessages.da_form_group_definition_sort_order.Localize(),
                IsModified = sortOrderModified,
                FormItemDefinitions = new List<FormFieldItemDefinitionModel>()
                {
                    new FormFieldItemDefinitionModel()
                    {
                        Id = FormConstants.DefaultSmintIoSortOrderFormFieldValueKey,
                        DataType = ValueTypeEnum.String,
                        DefaultValue = new ValueForJson()
                        {
                            StringValue = "desc"
                        },
                        CurrentValue = new ValueForJson()
                        {
                            StringValue = sortOrder
                        },
                        IsModified = sortOrderModified,
                        AllowedValues = new List<ValueForJsonUiDetailsModel>()
                        {
                            new ValueForJsonUiDetailsModel()
                            {
                                Value = new ValueForJson() {
                                    StringValue = "asc"
                                },
                                Name = DataAdapterSDK.Resources.MetamodelMessages.da_sort_order_fields_ascending.Localize()
                            },
                            new ValueForJsonUiDetailsModel()
                            {
                                Value = new ValueForJson() {
                                    StringValue = "desc"
                                },
                                Name = DataAdapterSDK.Resources.MetamodelMessages.da_sort_order_fields_descending.Localize()
                            }
                        }
                    }
                }
            });

            AssetSearchDetailsModel details = new AssetSearchDetailsModel()
            {
                MaxPages = (int)Math.Ceiling((double)response.TotalResults / (parameters?.PageSize ?? _configuration.DefaultPageSize)),
                CurrentItemsPerPage = contentDetails.Count,
                CurrentPage = parameters?.Page ?? -1,
                HasMoreResults = response.PageToken != null,
                SearchResultSetId = response.PageToken,
                TotalResults = (int?)response.TotalResults
            };

            // let's not resolve EVERYTHING - we are in search

            var converter = new PictureparkContentConverter(
                _logger,
                Context,
                _entityModelProvider,
                _configuration.ListNameAttribute,
                _configuration.ListNameAttribute2,
                _configuration.ResolveListDataAttributes,
                new PictureparkPostProcessObjectConverter(_entityModelProvider));

            var results = contentDetails
                .Select(cd => converter.Convert(cd, _configuration.GalleryTitleDisplayPattern))
                .ToArray();

            return new SearchAssetsResult()
            {
                AssetDataObjects = results,
                Details = details,
                FilterModel = formGroupsModel
            };
        }
    }
}
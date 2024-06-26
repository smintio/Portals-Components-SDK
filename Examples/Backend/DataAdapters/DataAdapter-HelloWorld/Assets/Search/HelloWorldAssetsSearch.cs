using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using SmintIo.Portals.Connector.HelloWorld.Models.Requests;
using SmintIo.Portals.Connector.HelloWorld.Models.Responses;
using SmintIo.Portals.DataAdapter.HelloWorld.Assets.Common;
using SmintIo.Portals.DataAdapter.HelloWorld.Assets.Search;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Impl;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets.Models;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets.Parameters;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets.Results;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Specs;
using SmintIo.Portals.SDK.Core.Configuration;
using SmintIo.Portals.SDK.Core.Configuration.Model;
using SmintIo.Portals.SDK.Core.Helpers;
using SmintIo.Portals.SDK.Core.Models.Metamodel.Data;
using SmintIo.Portals.SDK.Core.Models.Strings;

namespace SmintIo.Portals.DataAdapter.HelloWorld.Assets
{
    /// <summary>
    /// Searches the external systems using the <see cref="IHelloWorldClient"/> by values coming from the user interface
    /// </summary>
    public partial class HelloWorldAssetsDataAdapter : AssetsDataAdapterBaseImpl
    {
        /// <summary>
        /// Smint.Io typeahead functionality hook
        /// </summary>
        public override async Task<GetFullTextSearchProposalsResult> GetFullTextSearchProposalsAsync(GetFullTextSearchProposalsParameters parameters)
        {
            var assets = await SearchAssetsAsync(new SearchAssetsParameters()
            {
                QueryString = parameters?.SearchQueryString,
                CurrentFilters = parameters?.CurrentFilters,
                PageSize = parameters?.MaxResultCount
            }).ConfigureAwait(false);

            return new GetFullTextSearchProposalsResult
            {
                FullTextProposals = assets.AssetDataObjects
                    .Where(assetDataObject => assetDataObject.Name != null)
                    .Select(assetDataObject => assetDataObject.Name.ResolveLocalizedString(CultureInfo.CurrentCulture))
                    .ToArray()
            };
        }

        /// <summary>
        /// This Smint.Io infrastructure hook is called when we have a search facet and want to search for a specific value in it, or the values have been clipped due to the value of `MultiSelectItemCount`
        /// </summary>
        public override async Task<GetFormItemDefinitionAllowedValuesResult> GetFormItemDefinitionAllowedValuesAsync(GetFormItemDefinitionAllowedValuesParameters parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            var maxResultCount = parameters.MaxResultCount.GetValueOrDefault(_configuration.MultiSelectItemCount);

            var searchAssetsFormGroupsModelBuilder = new HelloWorldSearchAssetsFormGroupsModelBuilder(multiSelectItemCount: maxResultCount);

            var (sortBy, sortDirection, sortDirectionIsModified, formFieldValueModelById) = searchAssetsFormGroupsModelBuilder.ParseFilters(parameters.CurrentFilters);

            var searchAssetsResponse = await GetSearchAssetsResultAsync(formFieldValueModelById, queryString: parameters.SearchQueryString, sortBy, sortDirection, limit: 1, offset: 0).ConfigureAwait(false);

            if (searchAssetsResponse == null)
            {
                return new GetFormItemDefinitionAllowedValuesResult()
                {
                    AllowedValues = new List<ValueForJsonUiDetailsModel>(),
                    AllowedValuesTotalCount = 0
                };
            }

            var customFieldById = await _helloWorldClient.GetCustomFieldByIdAsync(getFreshData: false).ConfigureAwait(false);

            var formGroupsDefinitionModel = searchAssetsFormGroupsModelBuilder.Build(
                searchAssetsResponse.SearchFacets,
                customFieldById,
                formFieldValueModelById,
                filterValue: parameters.FormItemDefinitionQueryString,
                sortBy,
                sortDirection,
                sortDirectionIsModified);

            if (formGroupsDefinitionModel != null && formGroupsDefinitionModel.FormGroupDefinitions != null && formGroupsDefinitionModel.FormGroupDefinitions.Count > 0)
            {
                foreach (var formGroupModel in formGroupsDefinitionModel.FormGroupDefinitions)
                {
                    if (formGroupModel.FormItemDefinitions == null || formGroupModel.FormItemDefinitions.Count == 0)
                    {
                        continue;
                    }

                    foreach (var formItemModel in formGroupModel.FormItemDefinitions)
                    {
                        if (!string.Equals(formItemModel.Id, parameters.FormItemDefinitionId))
                        {
                            continue;
                        }

                        return new GetFormItemDefinitionAllowedValuesResult()
                        {
                            AllowedValues = formItemModel.AllowedValues,
                            AllowedValuesTotalCount = formItemModel.AllowedValuesTotalCount
                        };
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
        /// Smint.Io search assets functionality to be implemented by the data adapter
        /// Ideally the flow would be
        /// 1) Parse the incoming values (CurrentFields) from the UI
        /// 2) Perform search
        /// 3) Convert the data
        /// 4) Return the asset data objects and possible filters
        /// </summary>
        public override async Task<SearchAssetsResult> SearchAssetsAsync(SearchAssetsParameters parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            var searchAssetsFormGroupsModelBuilder = new HelloWorldSearchAssetsFormGroupsModelBuilder(_configuration.MultiSelectItemCount);

            var (sortBy, sortDirection, sortDirectionIsModified, formFieldValueModelById) = searchAssetsFormGroupsModelBuilder.ParseFilters(parameters.CurrentFilters);

            var limit = parameters.PageSize.GetValueOrDefault(50);

            var offset = parameters.Page.HasValue
                ? limit * parameters.Page.Value
                : 0;

            parameters.Page ??= 0;

            var searchResponse = await GetSearchAssetsResultAsync(formFieldValueModelById, queryString: parameters.QueryString, sortBy, sortDirection, limit, offset).ConfigureAwait(false);

            if (searchResponse == null || searchResponse.Assets.Count == 0)
            {
                return GetEmptySearchAssetsResult();
            }

            var details = new AssetSearchDetailsModel
            {
                TotalResults = searchResponse.TotalCount,
                HasMoreResults = searchResponse.TotalCount > (offset + searchResponse.Assets.Count),
                CurrentItemsPerPage = searchResponse.Assets.Count
            };

            var converter = new HelloWorldContentConverter(_logger, Context, entityModelProvider: null, customFieldById: null);

            var assetDataObjects = searchResponse.Assets
                .Select(a => converter.GetAssetDataObject(a))
                .ToArray();

            var customFieldById = await _helloWorldClient.GetCustomFieldByIdAsync(getFreshData: false).ConfigureAwait(false);

            var formGroupsDefinitionModel = searchAssetsFormGroupsModelBuilder.Build(
                searchResponse.SearchFacets,
                customFieldById,
                formFieldValueModelById,
                filterValue: null,
                sortBy,
                sortDirection,
                sortDirectionIsModified);

            return new SearchAssetsResult
            {
                Details = details,
                AssetDataObjects = assetDataObjects,
                FilterModel = formGroupsDefinitionModel
            };
        }

        private static SearchAssetsResult GetEmptySearchAssetsResult()
        {
            return new SearchAssetsResult()
            {
                Details = new AssetSearchDetailsModel()
                {
                    CurrentItemsPerPage = 0,
                    CurrentPage = 0,
                    HasMoreResults = false,
                    MaxPages = 0,
                    SearchResultSetId = null,
                    TotalResults = 0
                },
                AssetDataObjects = Array.Empty<AssetDataObject>(),
                FilterModel = new FormGroupsDefinitionModel()
                {
                    FormGroupDefinitions = new List<FormGroupDefinitionModel>()
                }
            };
        }

        /// <summary>
        /// Parses the values coming from the user interface represented as `formFieldValueModelById` into the appropriate format (facetFilters) and triggers the external system search
        /// </summary>
        private async Task<HelloWorldSearchAssetsResponse> GetSearchAssetsResultAsync(
            Dictionary<string, FormFieldValueModel> formFieldValueModelById,
            string queryString,
            string sortBy,
            SortDirection sortDirection,
            int limit,
            int offset)
        {
            var sanitizedQuery = StringHelpers.Sanitize(queryString ?? string.Empty);

            var stringArrayPropertyOptions = formFieldValueModelById.Values
                .Where(ffvm =>
                    ffvm.DataType == ValueTypeEnum.StringArray &&
                    ffvm.StringArrayValue != null &&
                    ffvm.StringArrayValue.Length > 0
                )
                .SelectMany(ffvm => ffvm.StringArrayValue.Select(v => new HelloWorldFacetFilterRequest
                {
                    Id = ffvm.Id,
                    StringValue = v
                }));

            var stringPropertyOptions = formFieldValueModelById.Values
                .Where(ffvm =>
                    ffvm.DataType == ValueTypeEnum.String &&
                    !string.IsNullOrEmpty(ffvm.StringValue)
                )
                .Select(ffvm => new HelloWorldFacetFilterRequest
                {
                    Id = ffvm.Id,
                    StringValue = ffvm.StringValue
                });

            var facetFilters = stringArrayPropertyOptions
                .Union(stringPropertyOptions)
                .ToList();

            var orderBy = sortDirection == SortDirection.Desc
                ? $"!{sortBy}"
                : sortBy;

            var searchAssetsRequest = new HelloWorldSearchAssetsRequest
            {
                SearchQuery = sanitizedQuery.Trim(),
                FacetFilters = facetFilters,
                Limit = limit,
                Skip = offset,
                OrderBy = orderBy
            };

            var searchAssetsResponse = await _helloWorldClient.SearchAssetsAsync(searchAssetsRequest).ConfigureAwait(false);

            return searchAssetsResponse;
        }
    }
}
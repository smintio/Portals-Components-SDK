using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SmintIo.Portals.Connector.HelloWorld.Extensions;
using SmintIo.Portals.Connector.HelloWorld.Metamodel;
using SmintIo.Portals.Connector.HelloWorld.Models.Responses;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Constants;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Specs;
using SmintIo.Portals.SDK.Core.Configuration;
using SmintIo.Portals.SDK.Core.Configuration.Model;
using SmintIo.Portals.SDK.Core.Extensions;
using SmintIo.Portals.SDK.Core.Models.Metamodel.Data;
using SmintIo.Portals.SDK.Core.Models.Strings;

namespace SmintIo.Portals.DataAdapter.HelloWorld.Assets.Search
{
    internal class HelloWorldSearchAssetsFormGroupsModelBuilder
    {
        public const string _defaultSortField = "name";

        private readonly int _multiSelectItemCount;

        private readonly Dictionary<string, LocalizedStringsModel> _defaultAllowedSortFields = new()
        {
            { "name", DataAdapterSDK.Resources.MetamodelMessages.da_sort_fields_name.Localize() }
        };

        internal HelloWorldSearchAssetsFormGroupsModelBuilder(int multiSelectItemCount)
        {
            _multiSelectItemCount = multiSelectItemCount;
        }

        internal (string sortBy, SortDirection sortDirection, bool sortDirectionIsModified, Dictionary<string, FormFieldValueModel> formFieldValueModelById) ParseFilters(FormFieldValuesModel formFieldValuesModel)
        {
            var sortBy = _defaultSortField;
            SortDirection? sortDirection = null;
            var defaultSortDirection = SortDirection.Desc;
            var formFieldValueModels = new Dictionary<string, FormFieldValueModel>();

            if (formFieldValuesModel != null && formFieldValuesModel.Values != null && formFieldValuesModel.Values.Any())
            {
                foreach (var formFieldValueModel in formFieldValuesModel.Values)
                {
                    var id = formFieldValueModel.Id;

                    if (string.IsNullOrEmpty(id))
                    {
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
                        if (formFieldValueModel.StringValue == SortDirection.Asc.ToString().ToLower())
                        {
                            sortDirection = SortDirection.Asc;
                        }
                        else if (formFieldValueModel.StringValue == SortDirection.Desc.ToString().ToLower())
                        {
                            sortDirection = SortDirection.Desc;
                        }

                        continue;
                    }

                    formFieldValueModels[id] = formFieldValueModel;
                }
            }

            sortDirection = sortDirection != null
                ? (SortDirection)sortDirection
                : defaultSortDirection;

            return (sortBy, (SortDirection)sortDirection, sortDirection != defaultSortDirection, formFieldValueModels);
        }

        internal IFormGroupsDefinitionModel Build(
            ICollection<HelloWorldSearchFacetResponse> searchFacets,
            IDictionary<string, HelloWorldCustomFieldResponse> customFieldById,
            Dictionary<string, FormFieldValueModel> formFieldValueModelById,
            string filterValue,
            string sortBy,
            SortDirection sortDirection,
            bool sortDirectionIsModified)
        {
            var formGroupDefinitionModels = new List<FormGroupDefinitionModel>();

            var formGroupsDefinitionModel = new FormGroupsDefinitionModel
            {
                FormGroupDefinitions = formGroupDefinitionModels
            };

            if (searchFacets.Count == 0)
            {
                return formGroupsDefinitionModel;
            }

            AddCustomFieldsToFormGroupDefinitions(searchFacets, customFieldById, formFieldValueModelById, formGroupDefinitionModels, filterValue);

            if (string.IsNullOrEmpty(sortBy))
            {
                sortBy = _defaultSortField;
            }

            var sortByModified = !sortBy.Equals(_defaultSortField, StringComparison.OrdinalIgnoreCase);

            formGroupDefinitionModels.Add(new FormGroupDefinitionModel()
            {
                Id = FormConstants.DefaultSmintIoSortByFormGroupKey,
                Name = DataAdapterSDK.Resources.MetamodelMessages.da_form_group_definition_sort_by.Localize(),
                IsModified = sortByModified,
                FormItemDefinitions = new List<FormFieldItemDefinitionModel>()
                {
                    new StringFormFieldItemModel()
                    {
                        Id = FormConstants.DefaultSmintIoSortByFormFieldValueKey,
                        DefaultValue = new ValueForJson()
                        {
                            StringValue = _defaultSortField
                        },
                        CurrentValue = new ValueForJson()
                        {
                            StringValue = sortBy
                        },
                        IsModified = sortByModified,
                        AllowedValues = _defaultAllowedSortFields
                            .Select(sf =>  new ValueForJsonUiDetailsModel()
                            {
                                Value = new ValueForJson()
                                {
                                    StringValue = sf.Key
                                },
                                Name = sf.Value
                            })
                            .ToList()
                    }
                }
            });

            formGroupDefinitionModels.Add(new FormGroupDefinitionModel()
            {
                Id = FormConstants.DefaultSmintIoSortOrderFormGroupKey,
                Name = DataAdapterSDK.Resources.MetamodelMessages.da_form_group_definition_sort_order.Localize(),
                IsModified = sortDirectionIsModified,
                FormItemDefinitions = new List<FormFieldItemDefinitionModel>()
                {
                    new StringFormFieldItemModel()
                    {
                        Id = FormConstants.DefaultSmintIoSortOrderFormFieldValueKey,
                        DefaultValue = new ValueForJson()
                        {
                            StringValue = SortDirection.Desc.ToString().ToLower()
                        },
                        CurrentValue = new ValueForJson()
                        {
                            StringValue = sortDirection.ToString().ToLower()
                        },
                        IsModified = sortDirectionIsModified,
                        AllowedValues = new List<ValueForJsonUiDetailsModel>()
                        {
                            new ValueForJsonUiDetailsModel()
                            {
                                Value = new ValueForJson() {
                                    StringValue = SortDirection.Asc.ToString().ToLower()
                                },
                                Name = DataAdapterSDK.Resources.MetamodelMessages.da_sort_order_fields_ascending.Localize()
                            },
                            new ValueForJsonUiDetailsModel()
                            {
                                Value = new ValueForJson() {
                                    StringValue = SortDirection.Desc.ToString().ToLower()
                                },
                                Name = DataAdapterSDK.Resources.MetamodelMessages.da_sort_order_fields_descending.Localize()
                            }
                        }
                    }
                }
            });

            return formGroupsDefinitionModel;
        }

        private void AddCustomFieldsToFormGroupDefinitions(
            ICollection<HelloWorldSearchFacetResponse> searchFacets,
            IDictionary<string, HelloWorldCustomFieldResponse> customFieldById,
            Dictionary<string, FormFieldValueModel> formFieldValueModelById,
            List<FormGroupDefinitionModel> formGroupDefinitionModels,
            string filterValue)
        {
            foreach (var searchFacet in searchFacets)
            {
                if (string.IsNullOrEmpty(searchFacet.Label))
                {
                    continue;
                }

                if (searchFacet.Label.Equals(HelloWorldMetamodelBuilder.ContentTypeId, StringComparison.OrdinalIgnoreCase))
                {
                    AddContentTypeToFormGroupDefinitions(searchFacet, formFieldValueModelById, formGroupDefinitionModels, filterValue);

                    continue;
                }

                if (!customFieldById.TryGetValue(searchFacet.Id ?? string.Empty, out var customField))
                {
                    continue;
                }

                var allowedValuesTotalCount = 0;
                var assetTypeItemJsonUiDetailsModels = new List<ValueForJsonUiDetailsModel>();

                var searchFacetValues = searchFacet.SearchFacetValues.OrderByDescending(v => v.Count);

                var hasFilterValue = !string.IsNullOrEmpty(filterValue);

                foreach (var searchFacetValue in searchFacetValues)
                {
                    if (string.IsNullOrEmpty(searchFacetValue.Value))
                    {
                        continue;
                    }

                    if (hasFilterValue)
                    {
                        if (!searchFacetValue.Label.Contains(filterValue, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }
                    }

                    allowedValuesTotalCount++;

                    if (assetTypeItemJsonUiDetailsModels.Count >= _multiSelectItemCount)
                    {
                        continue;
                    }

                    var valueForJsonUiDetailsModel = new ValueForJsonUiDetailsModel()
                    {
                        Name = searchFacetValue.Label.Localize().AddTranslations(searchFacetValue.LabelTranslationByCulture),
                        Value = new ValueForJson()
                        {
                            StringValue = searchFacetValue.Value
                        },
                        Count = searchFacetValue.Count
                    };

                    assetTypeItemJsonUiDetailsModels.Add(valueForJsonUiDetailsModel);
                }

                var escapedDataKey = Uri.EscapeDataString(searchFacet.Id);

                var currentValue = GetCurrentValue(formFieldValueModelById, escapedDataKey);

                var formGroupDefinitionModel = GetFormGroupDefinitionModel(
                    escapedDataKey,
                    labels: customField.Label.Localize(),
                    isMultipleOption: customField.MultiOptions,
                    currentValue,
                    assetTypeItemJsonUiDetailsModels,
                    allowedValuesTotalCount,
                    dynamicAllowedValuesProviderType: typeof(HelloWorldCustomFieldResponse));

                formGroupDefinitionModels.Add(formGroupDefinitionModel);
            }
        }

        private void AddContentTypeToFormGroupDefinitions(HelloWorldSearchFacetResponse searchFacet, Dictionary<string, FormFieldValueModel> formFieldValueModelById, List<FormGroupDefinitionModel> formGroupDefinitionModels, string filterValue)
        {
            var allowedValuesTotalCount = 0;
            var assetTypeItemJsonUiDetailsModels = new List<ValueForJsonUiDetailsModel>();

            var searchFacetValues = searchFacet.SearchFacetValues.OrderByDescending(v => v.Count);

            var hasFilterValue = !string.IsNullOrEmpty(filterValue);

            foreach (var searchFacetValue in searchFacetValues)
            {
                if (string.IsNullOrEmpty(searchFacetValue.Value))
                {
                    continue;
                }

                var contentTypeEnumDataObject = ContentTypeEnumDataObject.ENTITY.GetEnumValue(searchFacetValue.Value);

                if (contentTypeEnumDataObject == null)
                {
                    continue;
                }

                if (hasFilterValue)
                {
                    var localizedLabel = contentTypeEnumDataObject.ListDisplayName.ResolveLocalizedString(CultureInfo.CurrentCulture);

                    if (!localizedLabel.Contains(filterValue, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                }

                allowedValuesTotalCount++;

                if (assetTypeItemJsonUiDetailsModels.Count >= _multiSelectItemCount)
                {
                    continue;
                }

                var valueForJsonUiDetailsModel = new ValueForJsonUiDetailsModel()
                {
                    Name = contentTypeEnumDataObject.ListDisplayName,
                    Value = new ValueForJson()
                    {
                        StringValue = searchFacetValue.Value
                    },
                    Count = searchFacetValue.Count
                };

                assetTypeItemJsonUiDetailsModels.Add(valueForJsonUiDetailsModel);
            }

            var currentValue = GetCurrentValue(formFieldValueModelById, key: HelloWorldMetamodelBuilder.ContentTypeId);

            var formGroupDefinitionModel = GetFormGroupDefinitionModel(
                HelloWorldMetamodelBuilder.ContentTypeId,
                labels: Connector.HelloWorld.Resources.MetamodelMessages.c_hello_world_content_type.Localize(),
                isMultipleOption: false,
                currentValue,
                assetTypeItemJsonUiDetailsModels,
                allowedValuesTotalCount,
                dynamicAllowedValuesProviderType: null);

            formGroupDefinitionModels.Add(formGroupDefinitionModel);
        }

        private static ValueForJson GetCurrentValue(Dictionary<string, FormFieldValueModel> formFieldValueModelById, string key)
        {
            if (!formFieldValueModelById.TryGetValue(key, out var formFieldValueModel))
            {
                return null;
            }

            if (formFieldValueModel.StringArrayValue != null && formFieldValueModel.StringArrayValue.Length > 0)
            {
                return new ValueForJson
                {
                    StringArrayValue = formFieldValueModel.StringArrayValue
                };
            }

            if (!string.IsNullOrEmpty(formFieldValueModel.StringValue))
            {
                return new ValueForJson
                {
                    StringValue = formFieldValueModel.StringValue
                };
            }

            return null;
        }

        private FormGroupDefinitionModel GetFormGroupDefinitionModel(
            string id,
            LocalizedStringsModel labels,
            bool isMultipleOption,
            ValueForJson currentValue,
            List<ValueForJsonUiDetailsModel> valueForJsonUiDetailsModel,
            int allowedValuesTotalCount,
            Type dynamicAllowedValuesProviderType)
        {
            FormFieldItemDefinitionModel formFieldItemDefinitionModel = isMultipleOption
                ? new StringArrayFormFieldItemModel()
                : new StringFormFieldItemModel();

            formFieldItemDefinitionModel.Id = id;
            formFieldItemDefinitionModel.Name = labels;
            formFieldItemDefinitionModel.CurrentValue = currentValue;
            formFieldItemDefinitionModel.IsModified = currentValue != null;
            formFieldItemDefinitionModel.AllowedValues = valueForJsonUiDetailsModel;
            formFieldItemDefinitionModel.AllowedValuesPageSize = _multiSelectItemCount;
            formFieldItemDefinitionModel.AllowedValuesTotalCount = allowedValuesTotalCount;
            formFieldItemDefinitionModel.DynamicAllowedValuesProviderPropertyName = id;
            formFieldItemDefinitionModel.DynamicAllowedValuesProviderType = dynamicAllowedValuesProviderType;

            return new FormGroupDefinitionModel
            {
                Id = id,
                Name = labels,
                IsModified = currentValue != null,
                FormItemDefinitions = new List<FormFieldItemDefinitionModel>
                {
                    formFieldItemDefinitionModel
                }
            };
        }
    }
}

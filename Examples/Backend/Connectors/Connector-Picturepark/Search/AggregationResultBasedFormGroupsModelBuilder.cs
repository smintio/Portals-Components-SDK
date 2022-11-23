using Picturepark.SDK.V1.Contract;
using SmintIo.Portals.Connector.Picturepark.Metamodel.Aggregations;
using SmintIo.Portals.SDK.Core.Configuration;
using SmintIo.Portals.SDK.Core.Configuration.Model;
using SmintIo.Portals.SDK.Core.Models.Strings;
using System.Collections.Generic;
using System.Linq;

namespace SmintIo.Portals.Connector.Picturepark.Search
{
    internal class AggregationResultBasedFormGroupsModelBuilder
    {
        public AggregationResultBasedFormGroupsModelBuilder(IAggregationNameProvider aggregationNameProvider)
        {
            AggregationNameProvider = aggregationNameProvider;
        }

        private IAggregationNameProvider AggregationNameProvider { get; }

        public FormGroupsDefinitionModel Build(ICollection<AggregatorBase> aggregators, ICollection<AggregationResult> aggregationResults, int multiSelectItemCount)
        {
            List<FormGroupDefinitionModel> formGroups = new List<FormGroupDefinitionModel>(16);

            Build(aggregators, aggregationResults, formGroups, multiSelectItemCount);

            FormGroupsDefinitionModel groupsModel = new FormGroupsDefinitionModel();
            
            groupsModel.FormGroupDefinitions = formGroups;

            return groupsModel;
        }

        private void Build(ICollection<AggregatorBase> aggregators, ICollection<AggregationResult> aggregationResults, List<FormGroupDefinitionModel> formGroups, int multiSelectItemCount)
        {
            foreach (var aggregator in aggregators)
            {
                var aggregatorAggregationResults = aggregationResults.FirstOrDefault(aggregationResult => string.Equals(aggregationResult.Name, aggregator.Name));

                if (aggregator is NestedAggregator nestedAggregator)
                {
                    Build(aggregator.Aggregators, aggregatorAggregationResults.AggregationResultItems.First().AggregationResults, formGroups, multiSelectItemCount);
                }
                else if (aggregatorAggregationResults.AggregationResultItems.Count > 0)
                {
                    FormGroupDefinitionModel formGroup = CreateFormGroup(aggregator, aggregatorAggregationResults, multiSelectItemCount);

                    if (formGroup.FormItemDefinitions != null && formGroup.FormItemDefinitions.Count > 0)
                        formGroups.Add(formGroup);
                }
            }
        }

        private FormGroupDefinitionModel CreateFormGroup(AggregatorBase aggregator, AggregationResult aggregationResult, int multiSelectItemCount)
        {
            FormGroupDefinitionModel formGroup = new FormGroupDefinitionModel();

            formGroup.Id = aggregator.Name;
            formGroup.Name = new LocalizedStringsModel(aggregator.Names);

            StringArrayFormFieldItemModel formItem = CreateFormItem(aggregator, aggregationResult, multiSelectItemCount);

            List<FormFieldItemDefinitionModel> formItems = new List<FormFieldItemDefinitionModel>(1);

            if (formItem.AllowedValues != null && formItem.AllowedValues.Count > 0)
            {
                formItems.Add(formItem);
            }

            formGroup.FormItemDefinitions = formItems;

            return formGroup;
        }

        private StringArrayFormFieldItemModel CreateFormItem(AggregatorBase aggregator, AggregationResult aggregationResult, int multiSelectItemCount)
        {
            StringArrayFormFieldItemModel formItem = new StringArrayFormFieldItemModel();

            formItem.Id = aggregationResult.Name;
            formItem.Name = new LocalizedStringsModel(aggregator.Names);

            bool hasMore;

            (formItem.AllowedValues, hasMore) = CreateAllowedValues(aggregationResult);

            if (formItem.AllowedValues != null)
            {
                // we just give an indicator, because we have no absolute count

                formItem.AllowedValuesTotalCount = formItem.AllowedValues.Count + (hasMore ? AggregatorManager.MaxAggregationCount : 0);

                formItem.AllowedValuesPageSize = multiSelectItemCount;
            }

            if (aggregator is TermsAggregator)
            {
                formItem.DynamicAllowedValuesProviderPropertyName = formItem.Id;
                formItem.DynamicAllowedValuesProviderType = typeof(AggregatorBase);
            }

            var currentValues = GetCurrentValues(aggregationResult);

            if (currentValues != null && currentValues.Count > 0)
            {
                formItem.CurrentValue = new ValueForJson()
                {
                    StringArrayValue = currentValues.ToArray()
                };

                formItem.IsModified = true;
            }

            return formItem;
        }

        private (List<ValueForJsonUiDetailsModel>, bool HasMore) CreateAllowedValues(AggregationResult aggregationResult)
        {
            List<ValueForJsonUiDetailsModel> allowedValues = new List<ValueForJsonUiDetailsModel>(aggregationResult.AggregationResultItems.Count);

            bool hasMore = false;

            foreach (AggregationResultItem aggregationResultItem in aggregationResult.AggregationResultItems)
            {
                if (!(aggregationResultItem.Filter is null))
                {
                    ValueForJsonUiDetailsModel allowedValue = CreateAllowedValue(aggregationResultItem);

                    allowedValues.Add(allowedValue);
                }
                else if (!(aggregationResultItem.AggregationResults is null))
                {
                    foreach (AggregationResult aggregationResultInner in aggregationResultItem.AggregationResults)
                    {
                        if (aggregationResultInner.SumOtherDocCount > 0)
                        {
                            // we have something more

                            hasMore = true;
                        }

                        var (subAllowedValues, hasMoreInner) = CreateAllowedValues(aggregationResultInner);

                        allowedValues.AddRange(subAllowedValues);

                        if (hasMoreInner)
                        {
                            // we have something more

                            hasMore = true;
                        }
                    }
                }
            }

            if (aggregationResult.SumOtherDocCount > 0)
            {
                // we have something more

                hasMore = true;
            }

            return (allowedValues, hasMore);
        }

        private List<string> GetCurrentValues(AggregationResult aggregationResult)
        {
            List<string> currentValues = new List<string>();

            foreach (AggregationResultItem aggregationResultItem in aggregationResult.AggregationResultItems)
            {
                if (aggregationResultItem.Active && !(aggregationResultItem.Filter is null))
                {
                    currentValues.Add(aggregationResultItem.Filter.ToJson());
                }
                else if (!(aggregationResultItem.AggregationResults is null))
                {
                    foreach (AggregationResult aggregationResultInner in aggregationResultItem.AggregationResults)
                    {
                        currentValues.AddRange(GetCurrentValues(aggregationResultInner));
                    }
                }
            }

            return currentValues;
        }

        private ValueForJsonUiDetailsModel CreateAllowedValue(AggregationResultItem aggregationResultItem)
        {
            ValueForJsonUiDetailsModel model = new ValueForJsonUiDetailsModel();

            LocalizedStringsModel name;

            if (!string.IsNullOrEmpty(aggregationResultItem.Name))
            {
                name = new LocalizedStringsModel().Add(LocalizedStringsModel.DefaultCulture, aggregationResultItem.Name);
            }
            else
            {
                name = new LocalizedStringsModel()
                {
                    { LocalizedStringsModel.DefaultCulture, "No value" },
                    { "de", "Kein Wert" },
                    { "es", "Sin valor" },
                    { "pt", "Sem valor" },
                    { "it", "Nessun valore" }
                };
            }

            model.Name = name;
            model.Description = null; // No description exists

            model.Value = new ValueForJson()
            {
                StringValue = aggregationResultItem.Filter.ToJson()
            };

            model.Count = aggregationResultItem.Count;

            // value.Active = item.Active; // This should decided on whether the value is among the submitted values of StringArrayValue
            return model;
        }
    }
}
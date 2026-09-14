using Picturepark.SDK.V1.Contract;
using SmintIo.Portals.Connector.Picturepark.Metamodel.Aggregations;
using SmintIo.Portals.SDK.Core.Configuration.Model;
using SmintIo.Portals.SDK.Core.Models.Strings;
using System.Collections.Generic;

namespace SmintIo.Portals.Connector.Picturepark.Search
{
    internal class RawFormGroupsModelBuilder
    {
        public RawFormGroupsModelBuilder(IAggregationNameProvider aggregationNameProvider)
        {
            AggregationNameProvider = aggregationNameProvider;
        }

        private IAggregationNameProvider AggregationNameProvider { get; }

        public FormGroupsDefinitionModel Build(ICollection<AggregatorBase> aggregators)
        {
            List<FormGroupDefinitionModel> formGroups = new List<FormGroupDefinitionModel>(16);

            Build(aggregators, formGroups);

            FormGroupsDefinitionModel groupsModel = new FormGroupsDefinitionModel();
            
            groupsModel.FormGroupDefinitions = formGroups;

            return groupsModel;
        }

        private void Build(ICollection<AggregatorBase> aggregators, List<FormGroupDefinitionModel> formGroups)
        {
            foreach (var aggregator in aggregators)
            {
                if (aggregator is NestedAggregator _)
                {
                    Build(aggregator.Aggregators, formGroups);
                }
                else
                {
                    FormGroupDefinitionModel formGroup = CreateFormGroup(aggregator);

                    if (formGroup.FormItemDefinitions != null && formGroup.FormItemDefinitions.Count > 0)
                        formGroups.Add(formGroup);
                }
            }
        }

        private FormGroupDefinitionModel CreateFormGroup(AggregatorBase aggregator)
        {
            FormGroupDefinitionModel formGroup = new FormGroupDefinitionModel();

            formGroup.Id = aggregator.Name;
            formGroup.Name = new LocalizedStringsModel(aggregator.Names);

            StringArrayFormFieldItemModel formItem = CreateFormItem(aggregator);

            List<FormFieldItemDefinitionModel> formItems = new List<FormFieldItemDefinitionModel>(1);

            formItems.Add(formItem);

            formGroup.FormItemDefinitions = formItems;

            return formGroup;
        }

        private StringArrayFormFieldItemModel CreateFormItem(AggregatorBase aggregator)
        {
            StringArrayFormFieldItemModel formItem = new StringArrayFormFieldItemModel();

            formItem.Id = aggregator.Name;
            formItem.Name = new LocalizedStringsModel(aggregator.Names);

            return formItem;
        }
    }
}
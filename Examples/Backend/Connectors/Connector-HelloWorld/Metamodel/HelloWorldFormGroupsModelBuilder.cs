using System.Collections.Generic;
using SmintIo.Portals.Connector.HelloWorld.Models.Responses;
using SmintIo.Portals.SDK.Core.Configuration;
using SmintIo.Portals.SDK.Core.Configuration.Model;
using SmintIo.Portals.SDK.Core.Extensions;

namespace SmintIo.Portals.Connector.HelloWorld.Metamodel
{
    internal class HelloWorldFormGroupsModelBuilder
    {
        /// <summary>
        /// Here we build the <see cref="FormGroupsDefinitionModel"/>
        /// The means that the custom fields that are eliggible for filtering will be added as `FormItemDefinitions`
        /// The UI will know what to expect in terms of raw metadata (coming from the assets) and what the rendered filter pattern should be
        /// E.g. `Custom multi select list` can be represented as multi select checkbox list by using <see cref="StringArrayFormFieldItemModel"/> 
        /// `Custom single select list` will be represented as single select radio button list by using <see cref="StringFormFieldItemModel"/> 
        /// </summary>
        internal static IFormGroupsDefinitionModel Build(ICollection<HelloWorldCustomFieldResponse> customFields)
        {
            var formGroupsDefinitionModel = new FormGroupsDefinitionModel
            {
                FormGroupDefinitions = new List<FormGroupDefinitionModel>()
            };

            foreach (var customField in customFields)
            {
                if (customField.CustomFieldType != HelloWorldCustomFieldType.SingleSelect && customField.CustomFieldType != HelloWorldCustomFieldType.MultiSelect)
                {
                    continue;
                }

                FormFieldItemDefinitionModel formFieldItemDefinitionModel = customField.MultiOptions
                    ? new StringArrayFormFieldItemModel()
                    : new StringFormFieldItemModel();

                formFieldItemDefinitionModel.Id = customField.Id;
                formFieldItemDefinitionModel.Name = customField.Label.Localize();

                var formGroupDefinitionModel = new FormGroupDefinitionModel
                {
                    Id = customField.Id,
                    Name = customField.Label.Localize(),
                    FormItemDefinitions = new List<FormFieldItemDefinitionModel>
                    {
                       formFieldItemDefinitionModel
                    }
                };

                formGroupsDefinitionModel.FormGroupDefinitions.Add(formGroupDefinitionModel);
            }

            return formGroupsDefinitionModel;
        }
    }
}

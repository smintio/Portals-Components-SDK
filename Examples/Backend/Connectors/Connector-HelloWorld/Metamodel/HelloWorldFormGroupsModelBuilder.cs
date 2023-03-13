using System.Collections.Generic;
using SmintIo.Portals.Connector.HelloWorld.Models.Responses;
using SmintIo.Portals.SDK.Core.Configuration;
using SmintIo.Portals.SDK.Core.Configuration.Model;
using SmintIo.Portals.SDK.Core.Extensions;

namespace SmintIo.Portals.Connector.HelloWorld.Metamodel
{
    internal class HelloWorldFormGroupsModelBuilder
    {
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

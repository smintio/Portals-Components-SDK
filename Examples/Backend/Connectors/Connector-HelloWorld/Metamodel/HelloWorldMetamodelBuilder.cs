using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SmintIo.Portals.Connector.HelloWorld.Client;
using SmintIo.Portals.Connector.HelloWorld.Models.Responses;
using SmintIo.Portals.ConnectorSDK.Metamodel;
using SmintIo.Portals.SDK.Core.Extensions;
using SmintIo.Portals.SDK.Core.Models.Metamodel.Model;
using SmintIo.Portals.SDK.Core.Models.Strings;

namespace SmintIo.Portals.Connector.HelloWorld.Metamodel
{
    public class HelloWorldMetamodelBuilder : IMetamodelBuilder
    {
        public const string RootEntityKey = "HelloWorldAsset";
        public const string ContentTypeId = "contentType";

        public static readonly LocalizedStringsModel ContentTypeLabels = new()
        {
            { LocalizedStringsModel.DefaultCulture, "Content type" },
            { "de", "Inhaltstyp" }
        };

        private readonly IHelloWorldClient _helloWorldClient;
        private readonly ILogger _logger;

        private readonly ConnectorMetamodel _metamodel;

        public HelloWorldMetamodelBuilder(ILogger logger, IHelloWorldClient helloWorldClient, HelloWorldConnectorConfiguration connectorConfiguration)
        {
            _logger = logger;
            _helloWorldClient = helloWorldClient;

            _metamodel = new ConnectorMetamodel(
                $"{HelloWorldConnectorStartup.HelloWorldConnector}-{connectorConfiguration.ClientId}",
                isRandomAccessSupported: true,
                isFullTextSearchProposalsSupported: false,
                isFolderNavigationSupported: false);
        }

        public async Task<ConnectorMetamodel> BuildAsync()
        {
            var customFieldById = await _helloWorldClient.GetCustomFieldByIdAsync(getFreshData: true).ConfigureAwait(false);

            AddEntities(customFieldById.Values);

            AddFormGroups(customFieldById.Values);

            return _metamodel;
        }

        private void AddEntities(ICollection<HelloWorldCustomFieldResponse> customFields)
        {
            var rootEntityModel = CreateRootEntityModel();

            rootEntityModel.AddProperty(
                ContentTypeId,
                DataType.String,
                targetMetamodelEntityKey: null,
                labels: ContentTypeLabels);

            foreach (var customField in customFields)
            {
                AddProperty(rootEntityModel, customField);
            }
        }

        private void AddFormGroups(ICollection<HelloWorldCustomFieldResponse> customFields)
        {
            var formGroupsDefinitionModel = HelloWorldFormGroupsModelBuilder.Build(customFields);

            formGroupsDefinitionModel.FormGroupDefinitions
                .ForEach(formGroupDefinitionModel =>
                {
                    var formItemModels = formGroupDefinitionModel.FormItemDefinitions
                        .Select(formItemDefinition => new FormItemModel(formItemDefinition.Id, formItemDefinition.Name))
                        .ToList();

                    _metamodel.AddFormGroup(formGroupDefinitionModel.Id, formGroupDefinitionModel.Name, formItemModels);
                });
        }

        private EntityModel CreateRootEntityModel()
        {
            var rootEntityLabels = "HelloWorld Asset".LocalizeByCulture();

            rootEntityLabels.TryAdd("de", "HelloWorld-Asset");

            var rootEntityModel = CreateEntityModel(RootEntityKey, rootEntityLabels);

            rootEntityModel.Type = EntityType.TopLevelObject;

            return rootEntityModel;
        }

        private void AddProperty(EntityModel entityModel, HelloWorldCustomFieldResponse customField)
        {
            string targetMetamodelEntityKey = null;

            var labels = customField.Label.Localize();

            var isEnum = customField.CustomFieldType == HelloWorldCustomFieldType.SingleSelect;

            if (isEnum)
            {
                targetMetamodelEntityKey = GetEnumEntityModel(customField, labels).Key;
            }

            var dataType = GetDataType(customField);

            var propertyModel = entityModel.AddProperty(
                $"p_cf_{customField.Id}",
                dataType,
                targetMetamodelEntityKey,
                labels);

            if (isEnum)
            {
                propertyModel.SemanticHint = customField.Id;
            }
        }

        private EntityModel GetEnumEntityModel(HelloWorldCustomFieldResponse customField, LocalizedStringsModel labels)
        {
            var enumEntityModel = _metamodel.GetEntity(customField.Id);

            if (enumEntityModel == null)
            {
                enumEntityModel = CreateEntityModel(customField.Id, labels, isEnum: true);
            }

            return enumEntityModel;
        }

        private EntityModel CreateEntityModel(string entityModelKey, LocalizedStringsModel localizedStringsModel = null, bool isEnum = false)
        {
            var entityModel = isEnum
                ? EnumEntityModel.CreateEntity($"e_cf_{entityModelKey}", parentEntityModelKey: null, localizedStringsModel)
                : new EntityModel(entityModelKey, localizedStringsModel)
                {
                    Type = EntityType.Fieldset
                };

            _logger.LogDebug($"Creating {nameof(EntityModel)} for key '{entityModel.Key}'");

            _metamodel.AddEntity(entityModel);

            return entityModel;
        }

        private static DataType GetDataType(HelloWorldCustomFieldResponse customField)
        {
            return customField.CustomFieldType switch
            {
                HelloWorldCustomFieldType.String => DataType.String,
                HelloWorldCustomFieldType.Date => DataType.DateTime,
                HelloWorldCustomFieldType.Number => DataType.Decimal,
                HelloWorldCustomFieldType.SingleSelect => DataType.Enum,
                HelloWorldCustomFieldType.MultiSelect => DataType.LocalizedStringsArrayModel,
                _ => throw new ArgumentOutOfRangeException(nameof(customField.CustomFieldType), $"Not expected type value: {customField.CustomFieldType}", null),
            };
        }
    }
}

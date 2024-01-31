using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SmintIo.Portals.Connector.HelloWorld.Client;
using SmintIo.Portals.Connector.HelloWorld.Extensions;
using SmintIo.Portals.Connector.HelloWorld.Models.Responses;
using SmintIo.Portals.Connector.HelloWorld.Resources;
using SmintIo.Portals.ConnectorSDK.Metamodel;
using SmintIo.Portals.SDK.Core.Extensions;
using SmintIo.Portals.SDK.Core.Models.Metamodel.Model;
using SmintIo.Portals.SDK.Core.Models.Strings;

namespace SmintIo.Portals.Connector.HelloWorld.Metamodel
{
    /// <summary>
    /// Maps the external system's metadata
    /// </summary>
    public class HelloWorldMetamodelBuilder : IMetamodelBuilder
    {
        public const string RootEntityKey = "HelloWorldAsset";
        public const string ContentTypeId = "contentType";

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

        /// <summary>
        /// Builds the <see cref="ConnectorMetamodel"/>
        /// </summary>
        public async Task<ConnectorMetamodel> BuildAsync()
        {
            var customFieldById = await _helloWorldClient.GetCustomFieldByIdAsync(getFreshData: true).ConfigureAwait(false);

            AddEntities(customFieldById.Values);

            AddFormGroups(customFieldById.Values);

            return _metamodel;
        }

        /// <summary>
        /// Represents each custom field as a Smint.Io property
        /// </summary>
        private void AddEntities(ICollection<HelloWorldCustomFieldResponse> customFields)
        {
            var rootEntityModel = CreateRootEntityModel();

            rootEntityModel.AddProperty(
                ContentTypeId,
                DataType.LocalizedStringsModel,
                targetMetamodelEntityKey: null,
                labels: new ResourceLocalizedStringsModel(nameof(MetamodelMessages.c_hello_world_content_type)));

            foreach (var customField in customFields)
            {
                AddProperty(rootEntityModel, customField);
            }
        }

        /// <summary>
        /// Form groups drives how the search facets should look like
        /// E.g. would be if we want to have a `Custom single select list` filtering functionality while searching for assets
        /// or if we want to have a list of radio buttons with possible sorting by criteria
        /// </summary>
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

        /// <summary>
        /// The top level entity object
        /// Used by Smint.Io for UI selection mapping and raw metadata visizualization
        /// In case the external system is enabled with multiple templates, then it is recommended to use it <see cref="EntityType.MetadataLayer"/>
        /// </summary>
        private EntityModel CreateRootEntityModel()
        {
            var rootEntityLabels = "HelloWorld Asset".LocalizeByCulture();

            rootEntityLabels.TryAdd("de", "HelloWorld-Asset");

            var rootEntityModel = CreateEntityModel(RootEntityKey, rootEntityLabels);

            rootEntityModel.Type = EntityType.TopLevelObject;

            return rootEntityModel;
        }

        /// <summary>
        /// Maps each individual external system field to the corresponding Smint.Io property type
        /// </summary>
        /// <param name="entityModel"></param>
        /// <param name="customField"></param>
        private void AddProperty(EntityModel entityModel, HelloWorldCustomFieldResponse customField)
        {
            string targetMetamodelEntityKey = null;

            var labels = customField.Label.Localize().AddTranslations(customField.LabelTranslationByCulture);

            var isEnum = customField.CustomFieldType == HelloWorldCustomFieldType.SingleSelect;

            if (isEnum)
            {
                targetMetamodelEntityKey = GetEnumEntityModel(customField, labels).Key;
            }

            var dataType = GetDataType(customField);

            entityModel.AddProperty(
                customField.Id,
                dataType,
                targetMetamodelEntityKey,
                labels);
        }

        /// <summary>
        /// External system's enumeration object representation
        /// </summary>
        private EntityModel GetEnumEntityModel(HelloWorldCustomFieldResponse customField, LocalizedStringsModel labels)
        {
            var enumEntityModel = _metamodel.GetEntity(customField.Id);

            if (enumEntityModel == null)
            {
                enumEntityModel = CreateEntityModel(customField.Id, labels, isEnum: true);
            }

            return enumEntityModel;
        }

        /// <summary>
        /// External system's complex objects are reprenseted as <see cref="EntityModel"/>
        /// </summary>
        private EntityModel CreateEntityModel(string entityModelKey, LocalizedStringsModel localizedStringsModel = null, bool isEnum = false)
        {
            var entityModel = isEnum
                ? EnumEntityModel.CreateEntity(entityModelKey, parentEntityModelKey: null, localizedStringsModel)
                : new EntityModel(entityModelKey, localizedStringsModel)
                {
                    Type = EntityType.Fieldset
                };

            _logger.LogDebug($"Creating {nameof(EntityModel)} for key '{entityModel.Key}'");

            _metamodel.AddEntity(entityModel);

            return entityModel;
        }

        /// <summary>
        /// Each of the external custom field types is represented as Smint.Io <see cref="DataType"/>
        /// By default strings are represented as `LocalizedStringsModel` if the field is localized, otherwise as `String`
        /// Static defined list are mapped as `Enum`. This means that a value in the list has an identifier
        /// Statically defined multi-select lists can be represented as `EnumArray` as long as we have a way to map their values, otherwise they are usually represented as a sequence of strings `LocalizedStringsArrayModel`
        /// </summary>
        private static DataType GetDataType(HelloWorldCustomFieldResponse customField)
        {
            return customField.CustomFieldType switch
            {
                HelloWorldCustomFieldType.String => DataType.LocalizedStringsModel,
                HelloWorldCustomFieldType.Date => DataType.DateTime,
                HelloWorldCustomFieldType.Number => DataType.Decimal,
                HelloWorldCustomFieldType.SingleSelect => DataType.Enum,
                HelloWorldCustomFieldType.MultiSelect => DataType.LocalizedStringsArrayModel,
                _ => throw new ArgumentOutOfRangeException(nameof(customField.CustomFieldType), $"Not expected type value: {customField.CustomFieldType}", null),
            };
        }
    }
}

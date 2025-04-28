using Microsoft.Extensions.Logging;
using Picturepark.SDK.V1.Contract;
using SmintIo.Portals.Connector.Picturepark.Client;
using SmintIo.Portals.Connector.Picturepark.Search;
using SmintIo.Portals.ConnectorSDK.Metamodel;
using SmintIo.Portals.SDK.Core.Configuration;
using SmintIo.Portals.SDK.Core.Models.Metamodel.Model;
using SmintIo.Portals.SDK.Core.Models.Strings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmintIo.Portals.Connector.Picturepark.Metamodel
{
    public class PictureparkMetamodelBuilder : IMetamodelBuilder
    {
        private ConnectorMetamodel _metamodel;
        private IPictureparkClient _client;
        private Dictionary<SchemaDetail, LabelFieldInfo> _labelFields = new Dictionary<SchemaDetail, LabelFieldInfo>(256);
        private readonly ILogger<PictureparkMetamodelBuilder> _logger;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="client"></param>
        public PictureparkMetamodelBuilder(IPictureparkClient client)
        : this(client, null)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public PictureparkMetamodelBuilder(IPictureparkClient client, ILogger<PictureparkMetamodelBuilder> logger)
        {
            _metamodel = new ConnectorMetamodel(
                PictureparkConnectorStartup.PictureparkConnector, 
                isRandomAccessSupported: false, 
                isFullTextSearchProposalsSupported: true, 
                isFolderNavigationSupported: false);

            _client = client;
            _logger = logger;
        }

        public async Task<ConnectorMetamodel> BuildAsync()
        {
            DEBUG("Loading schemas . . .");
            ICollection<Schema> schemas = await _client.GetSchemasAsync();
            DEBUG($"{schemas.Count} schemas loaded!");

            DEBUG("Loading schema details asynchronously . . .");

            // Schema details have to be loaded before the entities are generated, because
            // enum entities need access to label fields, which are obtained from the schema
            // details.

            ICollection<SchemaDetail> schemaDetails = await _client.GetSchemaDetailsAsync(schemas);

            DEBUG($"{schemaDetails.Count} schema details loaded!");

            DEBUG("Loading output formats . . .");

            ICollection<OutputFormatInfo> outputFormats = await _client.GetOutputFormatsAsync();

            DEBUG($"{outputFormats.Count} output formats loaded!");

            DEBUG("Reflecting form groups . . .");

            await _client.InitializeChannelAggregatorsAsync().ConfigureAwait(false);

            var aggregatorManager = await _client.GetAggregatorManagerAsync().ConfigureAwait(false);

            FormGroupsModelBuilder builder = new FormGroupsModelBuilder(aggregatorManager);
            IFormGroupsDefinitionModel formGroupsDefinitionModel = builder.Build(aggregatorManager.Aggregators);

            DEBUG($"Reflected form groups!");

            IDictionary<string, SchemaDetail> schemaDetailsMap = schemaDetails.ToDictionary(d => d.Id, d => d);

            DEBUG("Creating entities . . .");
            AddEntities(schemaDetailsMap);
            DEBUG("Entities created!");

            DEBUG("Adding properties . . .");
            AddProperties(schemaDetails);
            DEBUG("Properties added!");

            DEBUG("Adding download sizes . . .");
            AddDownloadSizes(outputFormats);
            DEBUG("Download sizes added!");

            DEBUG("Adding form groups . . .");
            AddFormGroups(formGroupsDefinitionModel);
            DEBUG("Form groups added!");

            var translationLinker = new PictureparkTranslationLinker();

            translationLinker.Link(_metamodel);

            return _metamodel;
        }

        private int AddEntities(IDictionary<string, SchemaDetail> schemaDetails)
        {
            int count = 0;

            foreach (SchemaDetail schemaDetail in schemaDetails.Values)
            {
                EntityModel entity = AddEntity(schemaDetail, schemaDetails);
                if (!(entity is null))
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Create an entity from the schema.
        /// </summary>
        /// <param name="schemaDetail"></param>
        /// <param name="lookup">Schema lookup map, used to retrieve the parent schema, if any</param>
        /// <returns></returns>
        private EntityModel AddEntity(SchemaDetail schemaDetail, IDictionary<string, SchemaDetail> lookup)
        {
            if (schemaDetail is null)
            {
                return null;
            }

            EntityModel entity = _metamodel.GetEntity(schemaDetail.Id);

            if (!(entity is null))
            {
                return entity;
            }

            LocalizedStringsModel labels = new LocalizedStringsModel(schemaDetail.Names);

            // Resolve the parent

            EntityModel parent = null;

            if (!(schemaDetail.ParentSchemaId is null))
            {
                if (lookup.TryGetValue(schemaDetail.ParentSchemaId, out SchemaDetail parentSchema))
                {
                    parent = AddEntity(parentSchema, lookup);
                }
            }

            if (string.Equals(schemaDetail.Id, "ImageMetadata") ||
                string.Equals(schemaDetail.Id, "VideoMetadata") ||
                string.Equals(schemaDetail.Id, "AudioMetadata") ||
                string.Equals(schemaDetail.Id, "DocumentMetadata"))
            {
                // native Picturepark types

                return _metamodel.AddEntity(schemaDetail.Id, EntityType.TopLevelObject, parent?.Key, labels);
            }
            else if (schemaDetail.Types.Contains(SchemaType.Content))
            {
                // virtual types

                return _metamodel.AddEntity(schemaDetail.Id, EntityType.TopLevelObject, parent?.Key, labels);
            }
            else if (schemaDetail.Types.Contains(SchemaType.Layer))
            {
                // metadata layers

                return _metamodel.AddEntity(schemaDetail.Id, EntityType.MetadataLayer, parent?.Key, labels);
            }
            else if (schemaDetail.Types.Contains(SchemaType.Struct))
            {
                // fieldsets

                return _metamodel.AddEntity(schemaDetail.Id, EntityType.Fieldset, parent?.Key, labels);
            }
            else if (schemaDetail.Types.Contains(SchemaType.List))
            {
                // lists = enums

                var enumEntityModel = EnumEntityModel.CreateEntity(schemaDetail.Id, parent?.Key, labels);

                return _metamodel.AddEntity(enumEntityModel);
            }
            else
            {
                // other

                return _metamodel.AddEntity(schemaDetail.Id, EntityType.Other, parent?.Key, labels);
            }

        }

        private void AddProperties(ICollection<SchemaDetail> details)
        {
            foreach (SchemaDetail detail in details)
            {
                EntityModel entity = _metamodel.GetEntity(detail.Id);

                AddProperties(entity, detail);
            }
        }

        private void AddProperties(EntityModel entity, SchemaDetail detail)
        {
            foreach (FieldBase field in detail.Fields)
            {
                IAddPropertyHandler handler = AddPropertyHandlerFactory.Instance.GetHandler(field);

                if (handler != null)
                {
                    PropertyModel property = handler.AddProperty(field, entity, _metamodel);

                    if (property is null)
                    {
                        WARN($"Property could not be produced from the field {entity.Key}"
                            + $"({nameof(SchemaType)}.{detail.Types.FirstOrDefault()}).{field.Id} [{field.GetType().Name}]");
                    }
                }
                else
                {
                    WARN($"No mapping found for field's type: {entity.Key}.{field.Id} [{field.GetType().Name}]");
                }
            }
        }

        private void AddDownloadSizes(ICollection<OutputFormatInfo> outputFormats)
        {
            if (outputFormats == null || outputFormats.Count == 0)
                return;

            foreach (OutputFormatInfo outputFormatInfo in outputFormats)
            {
                _metamodel.AddDownloadSize(outputFormatInfo.Id, new LocalizedStringsModel(outputFormatInfo.Names));
            }
        }

        private void AddFormGroups(IFormGroupsDefinitionModel formGroupsDefinitionModel)
        {
            if (formGroupsDefinitionModel == null)
                return;

            var formGroupDefinitionModels = formGroupsDefinitionModel.FormGroupDefinitions;

            if (formGroupDefinitionModels == null || formGroupDefinitionModels.Count == 0)
                return;

            foreach (var formGroupDefinitionModel in formGroupDefinitionModels)
            {
                var formItemModels = new List<FormItemModel>();

                var formItemDefinitionModels = formGroupDefinitionModel.FormItemDefinitions;

                if (formItemDefinitionModels != null && formItemDefinitionModels.Count > 0)
                {
                    foreach (var formItemDefinitionModel in formItemDefinitionModels)
                    {
                        formItemModels.Add(new FormItemModel(formItemDefinitionModel.Id, formItemDefinitionModel.Name, formItemDefinitionModel.DataType));
                    }
                }

                _metamodel.AddFormGroup(formGroupDefinitionModel.Id, formGroupDefinitionModel.Name, formItemModels);
            }
        }

        /// <summary>
        /// Search for labels in the following order:
        /// 
        /// 1. If a 'name' field exists, use that field to retrieve the labels.
        /// 2. Otherwise, look in the display values.
        /// 3. Otherwise, look in the first translatable string field.
        /// 4. Otherweise, look in the first string field and use its value as label.
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="detail"></param>
        /// <returns></returns>
        private LocalizedStringsModel GetLabels(ListItem item, SchemaDetail detail)
        {
            LabelFieldInfo info = GetLabelField(detail);
            if (info is null)
            {
                return null;
            }

            LocalizedStringsModel labels;

            // If there exists a 'name' property, it is preferred
            if (info.Origin == LabelFieldInfo.ORIGIN_NAME_FIELD)
            {
                labels = GetLabels(item, info.Field);
                if (!(labels is null))
                {
                    return labels;
                }
            }

            // Second, if no 'name' property exists, look in display values, if available
            labels = GetLabelsByDisplayValues(item);
            if (!(labels is null))
            {
                return labels;
            }

            // Third, field is either a translatable string field or simple string field
            labels = GetLabels(item, info.Field);

            return labels;
        }

        private LocalizedStringsModel GetLabels(ListItem item, FieldBase labelField)
        {
            IAddPropertyHandler handler = AddPropertyHandlerFactory.Instance.GetHandler(labelField);
            if (handler is null)
            {
                return null;
            }

            Dictionary<string, string> labels = null;
            try
            {
                if (handler.Type == DataType.String)
                {
                    DataDictionary content = item.ConvertTo<DataDictionary>();
                    if (!(content is null))
                    {
                        labels = new Dictionary<string, string>(1);
                        string value = Convert.ToString(content[labelField.Id]);
                        labels.Add(LocalizedStringsModel.DefaultCulture, value);
                    }
                }
            }
            catch (Exception ex)
            {
                ERROR($"Unexpected error: field {labelField.Id} from schema {item.ContentSchemaId} could not "
                    + $"be converted to {nameof(DataDictionary)}. Original message: {ex.Message}");
            }
            return (labels is null) ? null : new LocalizedStringsModel(new LocalizedStringsModel(labels));
        }

        private LabelFieldInfo GetLabelField(SchemaDetail schemaDetail)
        {
            LabelFieldInfo fieldInfo;

            if (_labelFields.TryGetValue(schemaDetail, out fieldInfo))
            {
                return fieldInfo;
            }

            // 1. Look for the 'name' property.
            FieldBase field = schemaDetail.Fields.FirstOrDefault(f => StringComparer.InvariantCultureIgnoreCase.Equals("name", f.Id));
            if (!(field is null))
            {
                fieldInfo = new LabelFieldInfo(LabelFieldInfo.ORIGIN_NAME_FIELD, field);
            }

            if (fieldInfo is null)
            {
                FieldBase stringField = null;

                // 2. Look for the first translatable field.
                for (IEnumerator<FieldBase> e = schemaDetail.Fields.GetEnumerator(); (fieldInfo is null) && e.MoveNext();)
                {
                    IAddPropertyHandler handler = AddPropertyHandlerFactory.Instance.GetHandler(e.Current);
                    if (handler is null)
                    {
                        continue;
                    }

                    if (handler.Type == DataType.String && ((stringField is null) || e.Current.SimpleSearch && !stringField.SimpleSearch))
                    {
                        stringField = e.Current;
                    }
                }

                if (fieldInfo is null && !(stringField is null))
                {
                    // 3. If no translatable field found, use the first encountered and possibly searchable string field
                    fieldInfo = new LabelFieldInfo(LabelFieldInfo.ORIGIN_FIRST_STRING_FIELD, stringField);
                }
            }

            _labelFields.Add(schemaDetail, fieldInfo);

            if (fieldInfo is null)
            {
                ERROR($"Could not find the schema {schemaDetail.Id}'s field for labels");
            }

            return fieldInfo;
        }

        private LocalizedStringsModel GetLabelsByDisplayValues(ListItem item)
        {
            DisplayValueDictionary values = item.DisplayValues;
            if (!(values is null))
            {
                Dictionary<string, string> labels = new Dictionary<string, string>(1);
                labels.Add(LocalizedStringsModel.DefaultCulture, values.Name);
                return new LocalizedStringsModel(labels);
            }
            return null;
        }

        protected void DEBUG(string message, params object[] args)
        {
            LOG(LogLevel.Debug, message, args);
        }

        protected void ERROR(string message, params object[] args)
        {
            LOG(LogLevel.Error, message, args);
        }

        protected void WARN(string message, params object[] args)
        {
            LOG(LogLevel.Warning, message, args);
        }

        protected void LOG(LogLevel logLevel, string message, params object[] args)
        {
            if (!(_logger is null))
            {
                _logger.Log(logLevel, message, args);
            }
            else
            {
                // Console.WriteLine(message, args);
            }
        }

        private class LabelFieldInfo
        {
            public const int
                ORIGIN_NAME_FIELD = 1,
                ORIGIN_FIRST_TRANSLATABLE_FIELD = 2,
                ORIGIN_FIRST_STRING_FIELD = 3,
                ORIGIN_DISPLAY_VALUES = 4;

            public LabelFieldInfo(int origin, FieldBase field)
            {
                Origin = origin;
                Field = field;
            }
            public int Origin { get; }

            public FieldBase Field { get; }
        }
    }
}
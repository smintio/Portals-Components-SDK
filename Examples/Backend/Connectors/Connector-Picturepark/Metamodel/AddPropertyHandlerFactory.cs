using Picturepark.SDK.V1.Contract;
using SmintIo.Portals.Connector.Picturepark.Resources;
using SmintIo.Portals.ConnectorSDK.Metamodel;
using SmintIo.Portals.SDK.Core.Models.Metamodel.Model;
using SmintIo.Portals.SDK.Core.Models.Strings;
using System;
using System.Collections.Generic;

namespace SmintIo.Portals.Connector.Picturepark.Metamodel
{
    class AddPropertyHandlerFactory
    {
        public static readonly AddPropertyHandlerFactory Instance = new AddPropertyHandlerFactory();

        private static readonly Dictionary<Type, IAddPropertyHandler>
            _Handlers = new Dictionary<Type, IAddPropertyHandler>(16);

        /// <summary>
        /// Static (class) constructor.
        /// </summary>
        static AddPropertyHandlerFactory()
        {
            Map<FieldString>(new DefaultHandler(DataType.String));
            Map<FieldStringArray>(new DefaultHandler(DataType.StringArray));

            Map<FieldDecimal>(new DefaultHandler(DataType.Decimal));

            Map<FieldTranslatedString>(new TranslatedStringHandler());

            Map<FieldBoolean>(new DefaultHandler(DataType.Boolean));

            Map<FieldLong>(new DefaultHandler(DataType.Int64));
            Map<FieldLongArray>(new DefaultHandler(DataType.Int64Array));

            Map<FieldDate>(new DefaultHandler(DataType.DateTime));
            Map<FieldDateTime>(new DefaultHandler(DataType.DateTime));

            // Map<FieldDateTimeArray>(new DefaultHandler(DataType.??????));

            Map<FieldSingleTagbox>(new SingleTagboxHandler());
            Map<FieldMultiTagbox>(new MultiTagboxHandler());

            Map<FieldSingleFieldset>(new SingleFieldsetHandler());
            Map<FieldMultiFieldset>(new MultiFieldsetHandler());

            Map<FieldSingleRelation>(new SingleRelationHandler());
            Map<FieldMultiRelation>(new MultiRelationHandler());

            Map<FieldDictionary>(new DictionaryHandler());
            Map<FieldDictionaryArray>(new DictionaryArrayHandler());

            // Map<FieldGeoPoint>(new DefaultHandler(DataType.??????));

            Map<FieldTrigger>(new TriggerHandler());
        }

        /// <summary>
        /// Private costructor (class not directly instantiable).
        /// </summary>
        private AddPropertyHandlerFactory()
        {
        }

        public IAddPropertyHandler GetHandler(FieldBase field)
        {
            _Handlers.TryGetValue(field.GetType(), out IAddPropertyHandler handler);
            return handler;
        }

        private static void Map<T>(IAddPropertyHandler handler) where T : FieldBase
        {
            _Handlers.Add(typeof(T), handler);
        }

        private class DefaultHandler : AbstractAddPropertyHandler
        {
            public DefaultHandler(DataType dataType)
                : base(dataType)
            {
            }

            public override PropertyModel AddProperty(FieldBase field, EntityModel entity, ConnectorMetamodel metamodel)
            {
                return entity.AddProperty(field.Id, Type, new LocalizedStringsModel(field.Names));
            }
        }

        private class TranslatedStringHandler : AbstractAddPropertyHandler
        {
            public TranslatedStringHandler()
                : base(DataType.LocalizedStringsModel)
            {
            }

            public override PropertyModel AddProperty(FieldBase field, EntityModel entity, ConnectorMetamodel metamodel)
            {
                return entity.AddProperty(field.Id, Type, new LocalizedStringsModel(field.Names));
            }
        }

        /// <summary>
        /// Represents a property referencing a single instance of SchemaType.Enum.
        /// </summary>
        private class SingleTagboxHandler : AbstractAddPropertyHandler
        {
            public SingleTagboxHandler()
                : base(DataType.Enum)
            {
            }

            public override PropertyModel AddProperty(FieldBase field, EntityModel entity, ConnectorMetamodel metamodel)
            {
                FieldSingleTagbox tagbox = (FieldSingleTagbox)field;

                EntityModel targetEntity = metamodel.GetEntity(tagbox.SchemaId);

                if (targetEntity == null)
                    return null;

                return entity.AddProperty(field.Id, Type, targetEntity.Key, new LocalizedStringsModel(field.Names));
            }
        }

        /// <summary>
        /// Represents a property referencing an array or list of SchemaType.Enum instances.
        /// </summary>
        private class MultiTagboxHandler : AbstractAddPropertyHandler
        {
            public MultiTagboxHandler()
                : base(DataType.EnumArray)
            {
            }

            public override PropertyModel AddProperty(FieldBase field, EntityModel entity, ConnectorMetamodel metamodel)
            {
                FieldMultiTagbox tagbox = (FieldMultiTagbox)field;

                EntityModel targetEntity = metamodel.GetEntity(tagbox.SchemaId);

                if (targetEntity == null)
                    return null;

                return entity.AddProperty(field.Id, Type, targetEntity.Key, new LocalizedStringsModel(field.Names));
            }
        }

        /// <summary>
        /// Represents a property referencing a single instance of SchemaType.Struct.
        /// </summary>
        private class SingleFieldsetHandler : AbstractAddPropertyHandler
        {
            public SingleFieldsetHandler()
                : base(DataType.DataObject)
            {
            }

            public override PropertyModel AddProperty(FieldBase field, EntityModel entity, ConnectorMetamodel metamodel)
            {
                FieldSingleFieldset fieldset = (FieldSingleFieldset)field;

                EntityModel targetEntity = metamodel.GetEntity(fieldset.SchemaId);

                if (targetEntity == null)
                    return null;

                return entity.AddProperty(field.Id, Type, targetEntity.Key, new LocalizedStringsModel(field.Names));
            }
        }

        /// <summary>
        /// Represents a property referencing an array or list of SchemaType.Struct instances.
        /// </summary>
        private class MultiFieldsetHandler : AbstractAddPropertyHandler
        {
            public MultiFieldsetHandler()
                : base(DataType.DataObjectArray)
            {
            }

            public override PropertyModel AddProperty(FieldBase field, EntityModel entity, ConnectorMetamodel metamodel)
            {
                FieldMultiFieldset fieldset = (FieldMultiFieldset)field;

                EntityModel targetEntity = metamodel.GetEntity(fieldset.SchemaId);

                if (targetEntity == null)
                    return null;

                return entity.AddProperty(field.Id, Type, targetEntity.Key, new LocalizedStringsModel(field.Names));
            }
        }

        private class DictionaryHandler : AbstractAddPropertyHandler
        {
            public DictionaryHandler()
                : base(DataType.DataObjectArray)
            {
            }

            public override PropertyModel AddProperty(FieldBase field, EntityModel entity, ConnectorMetamodel metamodel)
            {
                return entity.AddProperty(field.Id, Type, new LocalizedStringsModel(field.Names));
            }
        }

        private class DictionaryArrayHandler : AbstractAddPropertyHandler
        {
            public DictionaryArrayHandler()
                : base(DataType.DataObjectArray)
            {
            }

            public override PropertyModel AddProperty(FieldBase field, EntityModel entity, ConnectorMetamodel metamodel)
            {
                return entity.AddProperty(field.Id, Type, new LocalizedStringsModel(field.Names));
            }
        }

        /// <summary>
        /// Represents a property referencing an instance of <see cref="SchemaType.Struct"/>
        /// which derives from the Picturepark system type <see cref="Picturepark.SDK.V1.Contract.SystemTypes.Relation"/>.
        /// </summary>
        private class SingleRelationHandler : AbstractAddPropertyHandler
        {
            public SingleRelationHandler()
                : base(DataType.DataObject)
            {
            }

            public override PropertyModel AddProperty(FieldBase field, EntityModel entity, ConnectorMetamodel metamodel)
            {
                FieldSingleRelation relation = (FieldSingleRelation)field;

                EntityModel targetEntity = metamodel.GetEntity(relation.SchemaId);

                if (targetEntity == null)
                    return null;

                var targetIdProperty = targetEntity.AddProperty("_targetId", DataType.String, new ResourceLocalizedStringsModel(MetamodelMessages.c_picturepark_target_id));
                targetIdProperty.SemanticType = SemanticType.Relationship;

                return entity.AddProperty(field.Id, Type, targetEntity.Key, new LocalizedStringsModel(field.Names));
            }
        }

        /// <summary>
        /// Represents a property referencing an array or list of <see cref="SchemaType.Struct"/>
        /// instances of a class which derives from the Picturepark system type 
        /// <see cref="Picturepark.SDK.V1.Contract.SystemTypes.Relation"/>.
        /// </summary>
        private class MultiRelationHandler : AbstractAddPropertyHandler
        {
            public MultiRelationHandler()
                : base(DataType.DataObjectArray)
            {
            }

            public override PropertyModel AddProperty(FieldBase field, EntityModel entity, ConnectorMetamodel metamodel)
            {
                FieldMultiRelation relation = (FieldMultiRelation)field;

                EntityModel targetEntity = metamodel.GetEntity(relation.SchemaId);

                if (targetEntity == null)
                    return null;

                var targetIdProperty = targetEntity.AddProperty("_targetId", DataType.String, new ResourceLocalizedStringsModel(MetamodelMessages.c_picturepark_target_id));
                targetIdProperty.SemanticType = SemanticType.Relationship;

                return entity.AddProperty(field.Id, Type, targetEntity.Key, new LocalizedStringsModel(field.Names));
            }
        }

        private class TriggerHandler : AbstractAddPropertyHandler
        {
            public TriggerHandler()
                : base(DataType.Undefined) // TODO: adjust the type when clarified (note that FieldTrigger has never been encountered during the tests)
            {
            }

            public override PropertyModel AddProperty(FieldBase field, EntityModel entity, ConnectorMetamodel metamodel)
            {
                FieldTrigger trigger = (FieldTrigger)field;
                return null;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using SmintIo.Portals.Connector.SharePoint.Models;
using SmintIo.Portals.ConnectorSDK.Linkers;
using SmintIo.Portals.ConnectorSDK.Metamodel;
using SmintIo.Portals.ConnectorSDK.Models;
using SmintIo.Portals.ConnectorSDK.Parsers;
using SmintIo.Portals.SDK.Core.Models.Metamodel.Model;
using SmintIo.Portals.SDK.Core.Models.Strings;

namespace SmintIo.Portals.Connector.SharePoint.MicrosoftGraph.Metamodel
{
    public class SharepointTranslationLinker : TranslationLinker
    {
        public SharepointTranslationLinker(bool skipEnumLinking = false)
            : base(skipEnumLinking)
        {
        }

        public override void Link(ConnectorMetamodel connectorMetamodel)
        {
            if (connectorMetamodel == null)
            {
                throw new ArgumentNullException(nameof(connectorMetamodel));
            }

            var entitiesToRemove = new HashSet<string>();

            foreach (var entityModel in connectorMetamodel.Entities)
            {
                if (!entityModel.Labels.TryGetValue(LocalizedStringsModel.DefaultCulture, out var labelValue))
                {
                    throw new InvalidOperationException($"Missing '{LocalizedStringsModel.DefaultCulture}' label for {nameof(EntityModel)} with key '{entityModel.Key}'");
                }

                var entityModelKey = LocalizedStringColumnParser.GetKey(labelValue);

                var (entityCulture, _, _) = LocalizedStringColumnParser.GetCultureAndTranslation(labelValue, trimParent: false);

                if (entityCulture != LocalizedStringsModel.DefaultCulture)
                {
                    entitiesToRemove.Add(entityModel.Key);

                    continue;
                }

                var translatableEntities = connectorMetamodel.Entities
                    .Where(em => em.Labels != null && em.Labels
                        .Any(l => LocalizedStringColumnParser.GetKey(l.Value) == entityModelKey));

                foreach (var translatableEntityModel in translatableEntities)
                {
                    var translatableEntityModelKey = LocalizedStringColumnParser.GetKey(translatableEntityModel.Key);

                    if (entityModel.Key == translatableEntityModel.Key)
                    {
                        LocalizeProperties(entityModel);

                        continue;
                    }

                    LocalizeEntity(entityModel, translatableEntityModel, entityModelKey);

                    LocalizeProperties(entityModel, translatableEntityModel);
                }
            }

            foreach (var entityModelKey in entitiesToRemove)
            {
                connectorMetamodel.RemoveEntity(entityModelKey);
            }
        }

        protected override void LocalizeProperties(EntityModel existingEntityModel)
        {
            var translatableFields = GetTranslatableFields(existingEntityModel);

            var propertiesToRemove = new HashSet<string>();

            for (var i = existingEntityModel.Properties.Count - 1; i >= 0; i--)
            {
                var propertyModel = existingEntityModel.Properties.ElementAt(i);

                if (PropertiesToSkip.Contains(propertyModel.Key))
                {
                    continue;
                }

                if (propertiesToRemove.Contains(propertyModel.Key))
                {
                    continue;
                }

                if (propertyModel.Labels == null || !propertyModel.Labels.TryGetValue(LocalizedStringsModel.DefaultCulture, out var existingLabel))
                {
                    continue;
                }

                var key = LocalizedStringColumnParser.GetKey(existingLabel);

                var translatableFieldsByKeys = translatableFields
                    .GroupBy(tf => LocalizedStringColumnParser.GetKey(tf.Value))
                    .Where(tfk => tfk.Key == key);

                foreach (var translatableFieldsByKey in translatableFieldsByKeys)
                {
                    var localizedData = LocalizedStringColumnParser.GetLocalizedData(translatableFieldsByKey, existingLabel);

                    SetLabels(propertyModel.Labels, localizedData);

                    SetLinkedTranslationProperties(existingEntityModel, propertyModel, localizedData.LinkedTranslationPropertyModels);

                    if (propertyModel.LinkedTranslationProperties == null || !propertyModel.LinkedTranslationProperties.Any())
                    {
                        propertyModel.LinkedTranslationProperties = null;

                        continue;
                    }

                    foreach (var linkedTranslationProperty in propertyModel.LinkedTranslationProperties)
                    {
                        propertiesToRemove.Add(linkedTranslationProperty.TranslationPropertyKey);
                    }
                }
            }

            foreach (var propertyKey in propertiesToRemove)
            {
                existingEntityModel.RemoveProperty(propertyKey);
            }
        }

        private void LocalizeEntity(EntityModel existingEntityModel, EntityModel translatableEntityModel, string defaultLocalizedValue)
        {
            var translatableFields = GetTranslatableFields(existingEntityModel, translatableEntityModel);

            var localizedData = LocalizedStringColumnParser.GetLocalizedData(translatableFields, defaultLocalizedValue: defaultLocalizedValue);

            SetLabels(existingEntityModel.Labels, localizedData);
        }

        private void LocalizeProperties(EntityModel existingEntityModel, EntityModel translatableEntityModel)
        {
            for (var i = existingEntityModel.Properties.Count - 1; i >= 0; i--)
            {
                var existingPropertyModel = existingEntityModel.Properties.ElementAt(i);

                if (PropertiesToSkip.Contains(existingPropertyModel.Key))
                {
                    continue;
                }

                if (existingPropertyModel.Labels == null || !existingPropertyModel.Labels.TryGetValue(LocalizedStringsModel.DefaultCulture, out var existingLabel))
                {
                    continue;
                }

                var key = LocalizedStringColumnParser.GetKey(existingLabel);

                var translatablePropertyModel = translatableEntityModel.Properties
                    .FirstOrDefault(tp => tp.Labels.TryGetValue(LocalizedStringsModel.DefaultCulture, out var localizedLabel)
                        && LocalizedStringColumnParser.GetKey(localizedLabel) == key);

                if (translatablePropertyModel == null)
                {
                    continue;
                }

                existingPropertyModel.LinkedTranslationProperties ??= Array.Empty<LinkedTranslationPropertyModel>();

                var translatableFields = GetTranslatableFields(existingPropertyModel, translatablePropertyModel);

                var localizedData = LocalizedStringColumnParser.GetLocalizedData(translatableFields, existingLabel);

                SetLabels(existingPropertyModel.Labels, localizedData);

                existingPropertyModel.LinkedTranslationProperties = existingPropertyModel.LinkedTranslationProperties
                    .Union(localizedData.LinkedTranslationPropertyModels)
                    .ToArray();

                if (existingPropertyModel.LinkedTranslationProperties.Length == 0)
                {
                    existingPropertyModel.LinkedTranslationProperties = null;

                    return;
                }

                if (existingPropertyModel.DataType != DataType.String)
                {
                    return;
                }

                ChangePropertyDataType(existingEntityModel, existingPropertyModel);
            }
        }

        private static IEnumerable<TranslatableField> GetTranslatableFields(EntityModel existingEntityModel, EntityModel translatableEntityModel)
        {
            return new[]
            {
                GetTranslatableField(existingEntityModel.Key, existingEntityModel.Labels),
                GetTranslatableField(translatableEntityModel.Key, translatableEntityModel.Labels)
            };
        }

        private static IEnumerable<TranslatableField> GetTranslatableFields(PropertyModel existingPropertyModel, PropertyModel translatablePropertyModel)
        {
            return new[]
            {
                GetTranslatableField(existingPropertyModel.Key, existingPropertyModel.Labels),
                GetTranslatableField(translatablePropertyModel.Key, translatablePropertyModel.Labels)
            };
        }

        private static TranslatableField GetTranslatableField(string key, LocalizedStringsModel labels)
        {
            var labelKey = labels.Keys.FirstOrDefault(l => l == LocalizedStringsModel.DefaultCulture)
                ?? labels.Keys.First();

            var value = labels[labelKey];

            return new TranslatableField
            {
                Key = key,
                Value = value,
                TrimParent = key.StartsWith(LocationModel.Key)
            };
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Converters;
using SmintIo.Portals.SDK.Core.Models.Metamodel.Data;
using SmintIo.Portals.SDK.Core.Models.Metamodel.Model;
using SmintIo.Portals.SDK.Core.Models.Strings;

namespace SmintIo.Portals.DataAdapter.SharePoint.Assets.Common
{
    public class SharepointPostProcessObjectConverter : TranslationPostProcessObjectConverter
    {
        public SharepointPostProcessObjectConverter(IEntityModelProvider entityModelProvider)
            : base(entityModelProvider)
        {
        }

        protected override void ProcessPropertyModels(IEnumerable<PropertyModel> propertyModels, DataObject dataObject, ICollection<string> propertyKeysToRemove)
        {
            foreach (var propertyModel in propertyModels)
            {
                if (!dataObject.Values.TryGetValue(propertyModel.Key, out var propertyValue))
                {
                    continue;
                }

                if (propertyModel.LinkedTranslationProperties != null && propertyModel.LinkedTranslationProperties.Any())
                {
                    MergeProperties(dataObject, propertyModel, propertyValue, propertyKeysToRemove);
                }

                // this is something ultra-strange in Sharepoint - sometimes enums contain ;# at the start and the end

                FixPropertyValue(propertyModel, propertyValue);
            }
        }

        private void FixPropertyValue(PropertyModel propertyModel, SimplePropertyValue propertyValue)
        {
            if (propertyModel == null || propertyValue == null)
                return;

            if (propertyModel.DataType == DataType.Enum)
            {
                var enumDataObjectValue = propertyValue.GetValue<EnumDataObject>();

                if (enumDataObjectValue != null)
                {
                    var listDisplayName = enumDataObjectValue.ListDisplayName;

                    FixEnum(listDisplayName);
                }
            }
            else if (propertyModel.DataType == DataType.EnumArray)
            {
                var enumDataObjectsValue = propertyValue.GetValue<EnumDataObject[]>();

                if (enumDataObjectsValue != null && enumDataObjectsValue.Length > 0)
                {
                    foreach (var enumDataObjectValue in enumDataObjectsValue)
                    {
                        var listDisplayName = enumDataObjectValue.ListDisplayName;

                        FixEnum(listDisplayName);
                    }
                }
            }
        }

        private void FixEnum(LocalizedStringsModel localizedStringsModel)
        {
            if (localizedStringsModel == null)
                return;

            foreach (var culture in localizedStringsModel.Keys.ToList())
            {
                var value = localizedStringsModel[culture];

                if (string.IsNullOrEmpty(value))
                    continue;

                var hasBeenModified = false;

                if (value.StartsWith(";#"))
                {
                    value = value.Substring(2);

                    hasBeenModified = true;
                }

                if (value.EndsWith(";#"))
                {
                    value = value.Substring(0, value.Length - 2);

                    hasBeenModified = true;
                }

                if (hasBeenModified)
                {
                    localizedStringsModel[culture] = value;
                }
            }
        }

        protected override void MergePropertyValues(
            PropertyModel propertyModel,
            IPropertyValue propertyValue,
            LinkedTranslationPropertyModel linkedTranslationPropertyModel,
            IPropertyValue translationPropertyValue)
        {
            if (propertyModel.DataType != DataType.DataObject && propertyModel.DataType != DataType.Enum)
            {
                base.MergePropertyValues(propertyModel, propertyValue, linkedTranslationPropertyModel, translationPropertyValue);

                return;
            }

            var dataObject = propertyValue.GetValue<DataObject>();
            var translationDataObject = translationPropertyValue.GetValue<DataObject>();

            var entityModel = EntityModelProvider.GetEntityModel(dataObject.MetamodelEntityKey);

            foreach (var childPropertyModel in entityModel.Properties)
            {
                if (!translationDataObject.Values.TryGetValue(childPropertyModel.Key, out var childTranslationPropertyValue))
                {
                    continue;
                }

                if (!dataObject.Values.TryGetValue(childPropertyModel.Key, out var childPropertyValue))
                {
                    // Sharepoint can send partial data for Location, thus we treat the linked object value as main.
                    dataObject.Values.TryAdd(childPropertyModel.Key, childTranslationPropertyValue);

                    continue;
                }

                if (childPropertyModel.DataType != DataType.DataObject)
                {
                    base.MergePropertyValues(childPropertyModel, childPropertyValue, linkedTranslationPropertyModel, childTranslationPropertyValue);
                }
                else
                {
                    MergePropertyValues(childPropertyModel, childPropertyValue, linkedTranslationPropertyModel, childTranslationPropertyValue);
                }
            }
        }
    }
}
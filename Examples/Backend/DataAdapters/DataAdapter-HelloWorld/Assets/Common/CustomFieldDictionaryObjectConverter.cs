using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SmintIo.Portals.Connector.HelloWorld.Extensions;
using SmintIo.Portals.Connector.HelloWorld.Models.Responses;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Converters;
using SmintIo.Portals.SDK.Core.Extensions;
using SmintIo.Portals.SDK.Core.Models.Metamodel.Model;
using SmintIo.Portals.SDK.Core.Models.Strings;

namespace SmintIo.Portals.DataAdapter.HelloWorld.Assets.Common
{
    /// <summary>
    /// Inherits <see cref="DictionaryObjectConverter"/> and respectivelly the base <see cref="DataObjectConverter<TObject, TValue>"/> class
    /// The purpose of the dictionary object converter is to convert external system custom data type values to the corresponding Smint.Io data types
    /// In the case of `GetStringDataType`, we would expect the value to be of a specific type, so this instance tries to convert it to a `String`
    /// Other overrides are available for the remaining Smint.Io supported data types
    /// Please see <see cref="DictionaryObjectConverter"/> 
    /// Smint.Io supports various json type converters by default if you inherit from <see cref="JObjectDictionaryObjectConverter"/>, <see cref="JsonElementDictionaryObjectConverter"/> or <see cref="JsonObjectConverter"/>
    /// For more information, feel free to contact one of the Smint.Io dev representatives
    /// </summary>
    public class CustomFieldDictionaryObjectConverter : DictionaryObjectConverter
    {
        public CustomFieldDictionaryObjectConverter(
            ILogger logger,
            IEntityModelProvider entityModelProvider,
            IDictionary<string, HelloWorldCustomFieldResponse> customFieldById)
            : base(logger, entityModelProvider)
        {
            CustomFieldById = customFieldById ?? new Dictionary<string, HelloWorldCustomFieldResponse>();
        }

        protected IDictionary<string, HelloWorldCustomFieldResponse> CustomFieldById { get; }

        protected override string GetStringDataType(string propertyKey, object value, string semanticHint)
        {
            var customFieldValue = GetTypedValue<HelloWorldCustomFieldValueResponse>(propertyKey, value, logWarning: false);

            if (customFieldValue != null)
            {
                var stringValue = GetTypedValue<string>(propertyKey, customFieldValue.Value, logWarning: true);

                return stringValue;
            }

            return base.GetStringDataType(propertyKey, value, semanticHint);
        }

        protected override string[] GetStringArrayDataType(string propertyKey, object value, string semanticHint)
        {
            var customFieldValue = GetTypedValue<HelloWorldCustomFieldValueResponse>(propertyKey, value, logWarning: false);

            if (customFieldValue != null)
            {
                var stringArrayValue = GetTypedValue<string[]>(propertyKey, customFieldValue.Value, logWarning: true);

                return stringArrayValue;
            }

            return base.GetStringArrayDataType(propertyKey, value, semanticHint);
        }

        protected override LocalizedStringsModel GetLocalizedStringsModelDataType(string propertyKey, object value, string semanticHint)
        {
            var customFieldValue = GetTypedValue<HelloWorldCustomFieldValueResponse>(propertyKey, value, logWarning: true);

            if (customFieldValue != null)
            {
                var stringFieldValue = GetTypedValue<HelloWorldStringFieldValueResponse>(propertyKey, customFieldValue.Value, logWarning: true);

                if (stringFieldValue != null)
                {
                    var localizedStringsModel = stringFieldValue.Label.Localize().AddTranslations(stringFieldValue.LabelTranslationByCulture);

                    return localizedStringsModel;
                }
            }

            return base.GetLocalizedStringsModelDataType(propertyKey, value, semanticHint);
        }

        protected override LocalizedStringsArrayModel GetLocalizedStringsArrayModelDataType(string propertyKey, object value, string semanticHint)
        {
            var customFieldValue = GetTypedValue<HelloWorldCustomFieldValueResponse>(propertyKey, value, logWarning: false);

            if (customFieldValue != null)
            {
                var stringArrayFieldValue = GetTypedValue<HelloWorldStringArrayFieldValueResponse>(propertyKey, customFieldValue.Value, logWarning: true);

                if (stringArrayFieldValue != null)
                {
                    var localizedStringsArrayModel = stringArrayFieldValue.Labels.Localize().AddTranslations(stringArrayFieldValue.LabelsTranslationByCulture);

                    return localizedStringsArrayModel;
                }
            }

            return base.GetLocalizedStringsArrayModelDataType(propertyKey, value, semanticHint);
        }

        protected override DateTimeOffset? GetDateTimeDataType(string propertyKey, object value, string semanticHint)
        {
            var customFieldValue = GetTypedValue<HelloWorldCustomFieldValueResponse>(propertyKey, value, logWarning: false);

            if (customFieldValue != null)
            {
                var dateTimeValue = GetTypedValue<DateTimeOffset?>(propertyKey, customFieldValue.Value, logWarning: true);

                return dateTimeValue;
            }

            return base.GetDateTimeDataType(propertyKey, value, semanticHint);
        }

        protected override decimal? GetDecimalDataType(string propertyKey, object value, string semanticHint)
        {
            var customFieldValue = GetTypedValue<HelloWorldCustomFieldValueResponse>(propertyKey, value, logWarning: false);

            if (customFieldValue != null)
            {
                var decimalValue = GetTypedValue<decimal?>(propertyKey, customFieldValue.Value, logWarning: true);

                return decimalValue;
            }

            return base.GetDecimalDataType(propertyKey, value, semanticHint);
        }

        protected override IDictionary<string, object> GetEnumObject(string propertyKey, object value, string semanticHint)
        {
            var customFieldValue = GetTypedValue<HelloWorldCustomFieldValueResponse>(propertyKey, value, logWarning: false);

            if (customFieldValue != null)
            {
                var singleSelectFieldValue = GetTypedValue<HelloWorldSingleSelectFieldValueResponse>(propertyKey, customFieldValue.Value, logWarning: true);

                if (singleSelectFieldValue != null)
                {
                    var displayName = singleSelectFieldValue.Label.Localize().AddTranslations(singleSelectFieldValue.LabelTranslationByCulture);

                    var enumObject = new Dictionary<string, object>
                    {
                        { EntityModel.PropName_Id, singleSelectFieldValue.Id },
                        { EntityModel.PropName_ListDisplayName, displayName }
                    };

                    return enumObject;
                }
            }

            var baseEnumObject = base.GetEnumObject(propertyKey, value, semanticHint);

            return baseEnumObject;
        }

        protected override IDictionary<string, object> GetObject(string propertyKey, object value, string semanticHint)
        {
            throw new NotImplementedException();
        }

        protected override IDictionary<string, object>[] GetObjects(string propertyKey, object value, string semanticHint)
        {
            throw new NotImplementedException();
        }
    }
}
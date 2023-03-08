using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SmintIo.Portals.Connector.HelloWorld.Models.Responses;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Converters;
using SmintIo.Portals.SDK.Core.Extensions;
using SmintIo.Portals.SDK.Core.Models.Metamodel.Model;
using SmintIo.Portals.SDK.Core.Models.Strings;

namespace SmintIo.Portals.DataAdapter.HelloWorld.Assets.Common
{
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

        protected override string GetStringDataType(object value, string semanticHint)
        {
            var customFieldValue = GetTypedValue<HelloWorldCustomFieldValueResponse>(value, logWarning: false);

            if (customFieldValue != null)
            {
                var stringValue = GetTypedValue<string>(customFieldValue.Value, logWarning: false);

                return stringValue;
            }

            return base.GetStringDataType(value, semanticHint);
        }

        protected override string[] GetStringArrayDataType(object value, string semanticHint)
        {
            var customFieldValue = GetTypedValue<HelloWorldCustomFieldValueResponse>(value, logWarning: false);

            if (customFieldValue != null)
            {
                var stringArrayValue = GetTypedValue<string[]>(customFieldValue.Value, logWarning: false);

                return stringArrayValue;
            }

            return base.GetStringArrayDataType(value, semanticHint);
        }

        protected override DateTimeOffset? GetDateTimeDataType(object value, string semanticHint)
        {
            var customFieldValue = GetTypedValue<HelloWorldCustomFieldValueResponse>(value, logWarning: false);

            if (customFieldValue != null)
            {
                var dateTimeValue = GetTypedValue<DateTimeOffset?>(customFieldValue.Value, logWarning: false);

                return dateTimeValue;
            }

            return base.GetDateTimeDataType(value, semanticHint);
        }

        protected override decimal? GetDecimalDataType(object value, string semanticHint)
        {
            var customFieldValue = GetTypedValue<HelloWorldCustomFieldValueResponse>(value, logWarning: false);

            if (customFieldValue != null)
            {
                var decimalValue = GetTypedValue<decimal?>(customFieldValue.Value, logWarning: false);

                return decimalValue;
            }

            return base.GetDecimalDataType(value, semanticHint);
        }

        protected override IDictionary<string, object> GetEnumObject(object value, string semanticHint)
        {
            var stringValue = GetStringDataType(value, semanticHint);

            if (stringValue != null && CustomFieldById.TryGetValue(semanticHint, out var customField))
            {
                var displayName = stringValue.Localize();

                var enumObject = new Dictionary<string, object>
                {
                    { EntityModel.PropName_Id, customField.Id },
                    { EntityModel.PropName_ListDisplayName, displayName },
                    { EntityModel.PropName_DetailDisplayName, displayName }
                };

                return enumObject;
            }

            var baseEnumObject = base.GetEnumObject(value, semanticHint);

            return baseEnumObject;
        }

        protected override LocalizedStringsArrayModel GetLocalizedStringsArrayModelDataType(object value, string semanticHint)
        {
            var stringArrayValue = GetStringArrayDataType(value, semanticHint);

            if (stringArrayValue != null)
            {
                var localizedStringsArrayByCulture = new List<KeyValuePair<string, string[]>>
                {
                    new KeyValuePair<string, string[]>(LocalizedStringsArrayModel.DefaultCulture, stringArrayValue)
                };

                return new LocalizedStringsArrayModel(localizedStringsArrayByCulture);
            }

            return base.GetLocalizedStringsArrayModelDataType(value, semanticHint);
        }

        protected override IDictionary<string, object> GetObject(object value, string semanticHint)
        {
            throw new NotImplementedException();
        }

        protected override IDictionary<string, object>[] GetObjects(object value, string semanticHint)
        {
            throw new NotImplementedException();
        }
    }
}
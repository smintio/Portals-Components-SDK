using System;
using SmintIo.Portals.Connector.SharePoint.Models;
using SmintIo.Portals.SDK.Core.Models.Metamodel.Model;

namespace SmintIo.Portals.Connector.SharePoint.MicrosoftGraph.Metamodel
{
    public static class SharepointColumnDefinitionExtensions
    {
        /// <summary>
        /// Determines the type for a Sharepoint column (i.e. metadata field). Sharepoint offers a limited set of "datatypes"
        /// that custom fields can be made of, e.g. a "Choice".
        /// </summary>
        /// <param name="columnDefinitionResponse">A column definition that is returned from Graph API</param>
        public static ColumnType GetColumnType(this ColumnDefinitionResponse columnDefinitionResponse)
        {
            switch (columnDefinitionResponse.FieldType)
            {
                case SharepointFieldType.Text:
                case SharepointFieldType.Note:
                // For some reason Sharepoint sends text when the type is File.
                case SharepointFieldType.File:
                    return ColumnType.Text;
                case SharepointFieldType.DateTime:
                    return ColumnType.DateTime;
                case SharepointFieldType.Choice:
                    return ColumnType.Choice;
                case SharepointFieldType.Lookup:
                    return ColumnType.Lookup;
                case SharepointFieldType.Boolean:
                    return ColumnType.YesNo;
                case SharepointFieldType.Counter:
                case SharepointFieldType.Number:
                    return ColumnType.Number;
                case SharepointFieldType.Currency:
                    return ColumnType.Currency;
                case SharepointFieldType.Url:
                    return ColumnType.Url;
                case SharepointFieldType.MultiChoice:
                    return ColumnType.MultiChoice;
                case SharepointFieldType.Computed:
                case SharepointFieldType.Calculated:
                    return ColumnType.CalculatedValue;
                case SharepointFieldType.User:
                    return ColumnType.PersonOrGroup;
                case SharepointFieldType.Geolocation:
                    return ColumnType.Geolocation;
                case SharepointFieldType.Location:
                    return ColumnType.Location;
                case SharepointFieldType.Image:
                    return ColumnType.Image;
                case SharepointFieldType.Integer:
                    return ColumnType.Long;
                case SharepointFieldType.Taxonomy:
                    return ColumnType.Taxonomy;
                case SharepointFieldType.TaxonomyMulti:
                    return ColumnType.TaxonomyMulti;
                default:
                    throw new InvalidOperationException($"Unsupported Sharepoint field type '{columnDefinitionResponse.TypeAsString}' with value of '{columnDefinitionResponse.FieldTypeKind}'");
            }
        }

        /// <summary>
        /// Checks whether the type is enum.
        /// </summary>
        public static bool IsEnum(this ColumnType columnType)
        {
            return columnType == ColumnType.Choice
                || columnType == ColumnType.MultiChoice;
        }

        /// <summary>
        /// Determines whether the column type is complex or basic.
        /// </summary>
        public static bool IsComplexType(this ColumnType columnType)
        {
            switch (columnType)
            {
                case ColumnType.Choice:
                case ColumnType.MultiChoice:
                case ColumnType.Location:
                case ColumnType.Url:
                case ColumnType.Image:
                case ColumnType.Taxonomy:
                case ColumnType.TaxonomyMulti:
                    return true;
                case ColumnType.CalculatedValue:
                case ColumnType.YesNo:
                case ColumnType.Number:
                case ColumnType.Text:
                case ColumnType.Lookup:
                case ColumnType.DateTime:
                case ColumnType.PersonOrGroup:
                case ColumnType.Currency:
                case ColumnType.Geolocation:
                case ColumnType.Long:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException(nameof(columnType), columnType, null);
            }
        }

        /// <summary>
        /// Constructs a unique key for a <see cref="ColumnDefinitionResponse"/>. If the column is configurable (for example: Choice),
        /// the name of the column is appended to its type.
        /// </summary>
        /// <returns>A unique string</returns>
        public static string GetKey(this ColumnDefinitionResponse columnDefinitionResponse)
        {
            var columnType = columnDefinitionResponse.GetColumnType();

            var key = columnType.IsComplexType()
                ? columnDefinitionResponse.Name
                : columnDefinitionResponse.GetColumnType().ToString();

            return key;
        }

        public static DataType GetDataType(this ColumnType columnType)
        {
            return columnType switch
            {
                ColumnType.Choice => DataType.Enum,
                ColumnType.MultiChoice => DataType.LocalizedStringsArrayModel,
                ColumnType.YesNo => DataType.Boolean,
                ColumnType.Currency => DataType.CurrencyModel,
                ColumnType.Geolocation => DataType.GeoLocationModel,
                ColumnType.Number => DataType.Decimal,
                ColumnType.Long => DataType.Int64,
                ColumnType.DateTime => DataType.DateTime,
                ColumnType.PersonOrGroup => DataType.String,
                ColumnType.Location => DataType.DataObject,
                ColumnType.Url => DataType.DataObject,
                ColumnType.Taxonomy => DataType.DataObject,
                ColumnType.TaxonomyMulti => DataType.DataObjectArray,
                ColumnType.Image => DataType.DataObject,
                ColumnType.Text => DataType.String,
                ColumnType.CalculatedValue => DataType.String,
                _ => throw new ArgumentOutOfRangeException(nameof(columnType), columnType, null)
            };
        }
    }
}
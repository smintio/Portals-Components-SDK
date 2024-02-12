using System;

namespace SmintIo.Portals.Connector.SharePoint.Models
{
    public class ColumnDefinitionResponse
    {
        private int _fieldTypeKind;
        private string _typeAsString;

        public string Id { get; set; }

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public bool IsHidden { get; set; }

        public int FieldTypeKind
        {
            get
            {
                return _fieldTypeKind;
            }
            set
            {
                _fieldTypeKind = value;

                if (Enum.IsDefined(typeof(SharepointFieldType), _fieldTypeKind))
                {
                    FieldType = (SharepointFieldType)_fieldTypeKind;
                }
            }
        }

        public string TypeAsString
        {
            get
            {
                return _typeAsString;
            }
            set
            {
                _typeAsString = value;

                // Sharepint doesn't have Taxonomy field defined as fieldType.
                if (string.Equals(_typeAsString, "TaxonomyFieldType"))
                {
                    FieldTypeKind = (int)SharepointFieldType.Taxonomy;
                }
                else if (string.Equals(_typeAsString, "TaxonomyFieldTypeMulti"))
                {
                    FieldTypeKind = (int)SharepointFieldType.TaxonomyMulti;
                }
                else if (string.Equals(_typeAsString, "UserMulti"))
                {
                    FieldTypeKind = (int)SharepointFieldType.UserMulti;
                }
            }
        }

        public string CurrencyLocale { get; set; }

        public SharepointFieldType? FieldType { get; set; } = SharepointFieldType.Text;
    }
}
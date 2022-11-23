using System;
using SmintIo.Portals.DataAdapter.Picturepark.Assets.AllowedValues;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Configurations;
using SmintIo.Portals.SDK.Core.Components;
using SmintIo.Portals.SDK.Core.Configuration.Annotations;
using SmintIo.Portals.SDK.Core.Models.MetadataAttributes;
using SmintIo.Portals.SDK.Core.Models.Strings;

namespace SmintIo.Portals.DataAdapter.Picturepark.Assets
{
    [Serializable]
    [FormGroupDeclaration("data_mapping")]
    [FormGroupDisplayName("data_mapping", "en", "Data mapping", IsDefault = true)]
    [FormGroupDisplayName("data_mapping", "de", "Daten-Mapping")]
    public class PictureparkAssetsDataAdapterConfiguration : IModifyThumbnailsDataAdapterConfiguration, IIndicatorDataAdapterConfiguration, IOutputFormatDataAdapterConfiguration, IOverrideNameDataAdapterConfiguration
    {
        /// <summary>
        /// Returns the default page size.
        /// </summary>
        [DisplayName("en", "Search result page size", IsDefault = true)]
        [DisplayName("de", "Größe einer Suchergebnis-Seite")]
        [Description("en", "Default size of a search result page.", IsDefault = true)]
        [Description("de", "Standard-Größe einer Suchergebnis-Seite.")]
        [MinValue(1)]
        [MaxValue(500)]
        [DefaultValue(30)]
        [FormItemVisibility(Visibility = FormItemVisibilityEnum.Advanced)]
        public int DefaultPageSize { get; set; }

        /// <summary>
        /// Multi selection item count.
        /// </summary>
        [DisplayName("en", "Item count for multi selection", IsDefault = true)]
        [DisplayName("de", "Anzahl der Werte für Mehrfachauswahl")]
        [Description("en", "If a search fragment displays a multi selection list, you can change the number of shown items here.", IsDefault = true)]
        [Description("de", "Wenn ein Suchfragment aus einer Mehrfachauswahl besteht, können Sie hier die Anzahl der angezeigten Elemente einstellen.")]
        [MinValue(5)]
        [MaxValue(30)]
        [DefaultValue(15)]
        [FormItemVisibility(Visibility = FormItemVisibilityEnum.Advanced)]
        public int MultiSelectItemCount { get; set; }

        public enum PictureparkDisplayPattern
        {
            [DisplayName("en", "List", IsDefault = true)]
            [DisplayName("de", "Liste")]
            List = 1,

            [DisplayName("en", "Name", IsDefault = true)]
            Name
        }

        [DefaultValue((int)PictureparkDisplayPattern.Name)]
        [DisplayName("en", "Gallery title display pattern", IsDefault = true)]
        [DisplayName("de", "Anzeigemuster für Galerie-Titel")]
        [Description("en", "Please select the Picturepark display pattern that should be used for the asset title when showing the asset in a gallery view.", IsDefault = true)]
        [Description("de", "Bitte wählen Sie das Picturepark-Anzeigemuster das zur Anzeige des Asset-Titels verwendet werden soll, wenn das Asset in einer Galerieansicht dargestellt wird.")]
        [FormItemVisibility(Visibility = FormItemVisibilityEnum.Advanced)]
        [FormGroup("data_mapping")]
        public PictureparkDisplayPattern GalleryTitleDisplayPattern { get; set; }

        /// <summary>
        /// Custom field for enum resolution.
        /// </summary>
        [DisplayName("en", "List value display name attribute (priority 1)", IsDefault = true)]
        [DisplayName("de", "Attribut für die Namensanzeige von Listenwerten (Priorität 1)")]
        [Description("en", "If your list definitions use a custom attribute for storing the display name, you can give the attribute identifier here. If no setting is specified here, or the attribute is not found, the default logic for resolving the display name is used.", IsDefault = true)]
        [Description("de", "Wenn Sie in Ihren Listendefinitionen benutzerdefinierte Attribute verwenden, um den Wert für die Namensanzeige der Listenwerte zu speichern, können Sie die entsprechenden Attribut-Kennung hier eingeben. Wenn die Einstellung nicht angegeben ist, oder das Attribut nicht gefunden wurde, wird die Standard-Logik verwendet, um den Anzeigewert zu ermitteln.")]
        [FormItemVisibility(Visibility = FormItemVisibilityEnum.Expert)]
        [FormGroup("data_mapping")]
        public string ListNameAttribute { get; set; }

        /// <summary>
        /// Custom field for enum resolution (fallback).
        /// </summary>
        [DisplayName("en", "List value display name attribute (priority 2)", IsDefault = true)]
        [DisplayName("de", "Attribut für die Namensanzeige von Listenwerten (Priorität 2)")]
        [Description("en", "If your list definitions use a custom attribute for storing the display name, you can give the attribute identifier here. If no setting is specified here, or the attribute is not found, the default logic for resolving the display name is used.", IsDefault = true)]
        [Description("de", "Wenn Sie in Ihren Listendefinitionen benutzerdefinierte Attribute verwenden, um den Wert für die Namensanzeige der Listenwerte zu speichern, können Sie die entsprechenden Attribut-Kennung hier eingeben. Wenn die Einstellung nicht angegeben ist, oder das Attribut nicht gefunden wurde, wird die Standard-Logik verwendet, um den Anzeigewert zu ermitteln.")]
        [FormItemVisibility(Visibility = FormItemVisibilityEnum.Expert)]
        [FormGroup("data_mapping")]
        public string ListNameAttribute2 { get; set; }

        [DisplayName("en", "Resolve list data attributes for objects", IsDefault = true)]
        [DisplayName("de", "Datenattribute von Listen für Objekte auflösen")]
        [Description("en", "Reading user defined data attributes of lists is very resource and data consuming. This is the reason why by default no user defined list data attributes for objects are read that reference multiple list values at the same time. Here you can specify the name of the objects where such list data attributes should be read anyways.", IsDefault = true)]
        [Description("de", "Das Lesen von benutzerdefinierten Daten aus Listenwerten ist äußerst ressourcen- und datenintensiv. Deshalb werden standardmäßig keine benutzerdefinierten Daten aus Listenwerten für Objekte aufgelöst, die mehrere Listenwerte zur gleichen Zeit referenzieren. Sie können hier die Namen jener Objekte angeben, zu denen diese benutzerdefinierte Daten trotzdem ausgelesen werden sollen.")]
        [FormItemVisibility(Visibility = FormItemVisibilityEnum.Expert)]
        [FormGroup("data_mapping")]
        public string[] ResolveListDataAttributes { get; set; }

        public MetadataAttributeModel SmintIoOverrideThumbnailAttribute { get; set; }

        public MetadataAttributeModel SmintIoThumbnailAlignmentAttribute { get; set; }

        public MetadataAttributeModel SmintIoIndicatorActivatorAttribute { get; set; }

        public MetadataAttributeModel SmintIoOverrideNameAttribute { get; set; }

        public string SmintIoIndicatorActivatorValue { get; set; }

        public string SmintIoIndicatorIcon { get; set; }

        public string SmintIoIndicatorBackgroundColor { get; set; }

        public LocalizedStringsModel SmintIoIndicatorText { get; set; }

        [DynamicAllowedValuesProvider(typeof(OutputFormatAllowedValuesProvider))]
        public string[] OutputFormatIdAllowList { get; set; }

        [DynamicAllowedValuesProvider(typeof(OutputFormatAllowedValuesProvider))]
        public string[] OutputFormatIdImagesAllowList { get; set; }

        [DynamicAllowedValuesProvider(typeof(OutputFormatAllowedValuesProvider))]
        public string[] OutputFormatIdVideosAllowList { get; set; }

        [DynamicAllowedValuesProvider(typeof(OutputFormatAllowedValuesProvider))]
        public string[] OutputFormatIdAudioFilesAllowList { get; set; }

        [DynamicAllowedValuesProvider(typeof(OutputFormatAllowedValuesProvider))]
        public string[] OutputFormatIdDocumentsAllowList { get; set; }

        [DynamicAllowedValuesProvider(typeof(OutputFormatAllowedValuesProvider))]
        public string[] OutputFormatIdOtherFileTypesAllowList { get; set; }

        [DynamicAllowedValuesProvider(typeof(OutputFormatAllowedValuesProvider))]
        public string[] HiResOutputFormatIdList { get; set; }
    }
}
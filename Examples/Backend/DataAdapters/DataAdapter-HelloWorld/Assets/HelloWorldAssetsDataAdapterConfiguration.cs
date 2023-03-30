using System;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.AllowedValuesProviders;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Configurations;
using SmintIo.Portals.SDK.Core.Configuration.Annotations;
using SmintIo.Portals.SDK.Core.Models.MetadataAttributes;

namespace SmintIo.Portals.DataAdapter.HelloWorld.Assets
{
    [Serializable]
    /// <summary>
    /// Standartized Smint.Io data adapter configuration
    /// Used by the Smint.Io infrastructure to configure which output formats should be available for download, what the maximum number of search fragments elements should be displayed, and more
    /// </summary>
    public class HelloWorldAssetsDataAdapterConfiguration : IOutputFormatDataAdapterConfiguration, IPreserveMetadataDataAdapterConfiguration
    {
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

        [DynamicAllowedValuesProvider(typeof(OutputFormatAllowedValuesProviderBaseImpl))]
        public string[] OutputFormatIdAllowList { get; set; }

        [DynamicAllowedValuesProvider(typeof(OutputFormatAllowedValuesProviderBaseImpl))]
        public string[] OutputFormatIdImagesAllowList { get; set; }

        [DynamicAllowedValuesProvider(typeof(OutputFormatAllowedValuesProviderBaseImpl))]
        public string[] OutputFormatIdVideosAllowList { get; set; }

        [DynamicAllowedValuesProvider(typeof(OutputFormatAllowedValuesProviderBaseImpl))]
        public string[] OutputFormatIdAudioFilesAllowList { get; set; }

        [DynamicAllowedValuesProvider(typeof(OutputFormatAllowedValuesProviderBaseImpl))]
        public string[] OutputFormatIdDocumentsAllowList { get; set; }

        [DynamicAllowedValuesProvider(typeof(OutputFormatAllowedValuesProviderBaseImpl))]
        public string[] OutputFormatIdOtherFileTypesAllowList { get; set; }

        [DynamicAllowedValuesProvider(typeof(OutputFormatAllowedValuesProviderBaseImpl))]
        public string[] HiResOutputFormatIdList { get; set; }

        public MetadataAttributeModel[] SmintIoPreserveMetadataAttributes { get; set; }
    }
}
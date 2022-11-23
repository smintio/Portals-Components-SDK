using System;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.AllowedValuesProviders;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Configurations;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Prefab;
using SmintIo.Portals.SDK.Core.Configuration.Annotations;
using SmintIo.Portals.SDK.Core.Models.MetadataAttributes;
using SmintIo.Portals.SDK.Core.Models.Strings;
using static SmintIo.Portals.DataAdapterSDK.DataAdapters.Prefab.IAlwaysOnIntegrationLayerDataAdapterConfiguration;

namespace SmintIo.Portals.DataAdapter.SharePoint.Assets
{
    [Serializable]
    public class SharepointAssetsDataAdapterConfiguration : IAlwaysOnIntegrationLayerDataAdapterConfiguration, IModifyThumbnailsDataAdapterConfiguration, IIndicatorDataAdapterConfiguration, IOutputFormatDataAdapterConfiguration
    {
        public MetadataAttributeModel SmintIoOverrideThumbnailAttribute { get; set; }

        public MetadataAttributeModel SmintIoThumbnailAlignmentAttribute { get; set; }

        public MetadataAttributeModel SmintIoIndicatorActivatorAttribute { get; set; }

        public string SmintIoIndicatorActivatorValue { get; set; }

        public string SmintIoIndicatorIcon { get; set; }

        public string SmintIoIndicatorBackgroundColor { get; set; }

        public LocalizedStringsModel SmintIoIndicatorText { get; set; }

        public MetadataAttributeModel[] SmintIoIntegrationLayerSearchIndexMetadataAttributeList { get; set; }

        public MetadataAttributeModel[] SmintIoIntegrationLayerSearchIndexSortMetadataAttributeList { get; set; }

        public MetadataAttributeModel SmintIoIntegrationLayerSearchIndexOverrideNameAttribute { get; set; }

        public SmintIoDataSearchIndexFileType[] SmintIoSearchIndexFileTypeAllowList { get; set; }

        public SmintIoDataSearchIndexFileType[] SmintIoSearchIndexFileTypeDenyList { get; set; }

        public string[] SmintIoSearchIndexIndexedLanguages { get; set; }

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
    }
}
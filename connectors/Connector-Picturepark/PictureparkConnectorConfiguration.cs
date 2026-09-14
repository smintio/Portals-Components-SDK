using SmintIo.Portals.Connector.Picturepark.AllowedValues;
using SmintIo.Portals.Connector.Picturepark.Resources;
using SmintIo.Portals.ConnectorSDK.Connectors.Configurations;
using SmintIo.Portals.SDK.Core.Configuration.Annotations;
using SmintIo.Portals.SDK.Core.Configuration.Annotations.Validation;
using System;

namespace SmintIo.Portals.Connector.Picturepark
{
    [Serializable]
    public class PictureparkConnectorConfiguration : IMetamodelConnectorConfiguration
    {
        [DisplayName(translationKey: nameof(ConfigurationMessages.c_picturepark_picturepark_url_display_name))]
        [Description(translationKey: nameof(ConfigurationMessages.c_picturepark_picturepark_url_description))]
        [Required]
        [IsUri(EnforceHttps = true, RemovePathAndQueryString = true)]
        public string PictureparkUrl { get; set; }

        [DisplayName(translationKey: nameof(ConfigurationMessages.c_picturepark_access_token_display_name))]
        [Description(translationKey: nameof(ConfigurationMessages.c_picturepark_access_token_description))]
        [MaxLength(100)]
        [Required]
        public string AccessToken { get; set; }

        [DisplayName(translationKey: nameof(ConfigurationMessages.c_picturepark_channel_display_name))]
        [Description(translationKey: nameof(ConfigurationMessages.c_picturepark_channel_description))]
        [DefaultValue("rootChannel")]
        [DynamicAllowedValuesProvider(typeof(ChannelAllowedValuesProvider))]
        [FormItemVisibility(Visibility = FormItemVisibilityEnum.Advanced)]
        [MaxLength(255)]
        public string Channel { get; set; }

        [DisplayName(translationKey: nameof(ConfigurationMessages.c_picturepark_legacy_thumbnails_enabled_display_name))]
        [Description(translationKey: nameof(ConfigurationMessages.c_picturepark_legacy_thumbnails_enabled_description))]
        [Required]
        [DefaultValue(false)]
        [FormItemVisibility(Visibility = FormItemVisibilityEnum.Expert)]
        public bool LegacyThumbnailsEnabled { get; set; }

        public int? MergeMetamodelToConnectorId { get; set; }

        public int? OverrideDataSourceIds { get; set; }
    }
}

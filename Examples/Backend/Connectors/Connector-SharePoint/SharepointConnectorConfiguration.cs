using System;
using SmintIo.Portals.Connector.SharePoint.AllowedValues;
using SmintIo.Portals.Connector.SharePoint.Resources;
using SmintIo.Portals.SDK.Core.Components;
using SmintIo.Portals.SDK.Core.Configuration.Annotations;
using SmintIo.Portals.SDK.Core.Configuration.Annotations.Validation;

namespace SmintIo.Portals.Connector.SharePoint
{
    [Serializable]
    public class SharepointConnectorConfiguration : IComponentConfiguration
    {
        [DisplayName(translationKey: nameof(ConfigurationMessages.c_sharepoint_sharepoint_url_display_name))]
        [Description(translationKey: nameof(ConfigurationMessages.c_sharepoint_sharepoint_url_description))]
        [Required]
        [IsUri(EnforceHttps = true, RemovePathAndQueryString = true)]
        public string SharepointUrl { get; set; }

        [DisplayName(translationKey: nameof(ConfigurationMessages.c_sharepoint_high_security_mode_display_name))]
        [Description(translationKey: nameof(ConfigurationMessages.c_sharepoint_high_security_mode_description))]
        [DefaultValue(false)]
        public bool HighSecurityMode { get; set; }

        [DisplayName(translationKey: nameof(ConfigurationMessages.c_sharepoint_site_id_display_name))]
        [Description(translationKey: nameof(ConfigurationMessages.c_sharepoint_site_id_description))]
        [DynamicAllowedValuesProvider(typeof(SharepointSiteIdProvider))]
        [FormItemVisibility(Visibility = FormItemVisibilityEnum.Advanced)]
        [VisibleIf(nameof(HighSecurityMode), VisibleIfOperators.Equal, false)]
        public string SiteId { get; set; }

        [DisplayName(translationKey: nameof(ConfigurationMessages.c_sharepoint_site_id_string_display_name))]
        [Description(translationKey: nameof(ConfigurationMessages.c_sharepoint_site_id_string_description))]
        [FormItemVisibility(Visibility = FormItemVisibilityEnum.Advanced)]
        [VisibleIf(nameof(HighSecurityMode), VisibleIfOperators.Equal, true)]
        public string SiteIdString { get; set; }

        [DisplayName(translationKey: nameof(ConfigurationMessages.c_sharepoint_site_drive_id_display_name))]
        [Description(translationKey: nameof(ConfigurationMessages.c_sharepoint_site_drive_id_description))]
        [DynamicAllowedValuesProvider(typeof(SharepointDriveIdProvider))]
        [FormItemVisibility(Visibility = FormItemVisibilityEnum.Advanced)]
        [VisibleIf(nameof(HighSecurityMode), VisibleIfOperators.Equal, false)]
        [VisibleIf(nameof(SiteId), VisibleIfOperators.NotEqual, (string)null)]
        public string SiteDriveId { get; set; }

        [DisplayName(translationKey: nameof(ConfigurationMessages.c_sharepoint_site_list_id_display_name))]
        [Description(translationKey: nameof(ConfigurationMessages.c_sharepoint_site_list_id_description))]
        [DynamicAllowedValuesProvider(typeof(SharepointListIdProvider))]
        [FormItemVisibility(Visibility = FormItemVisibilityEnum.Advanced)]
        [VisibleIf(nameof(HighSecurityMode), VisibleIfOperators.Equal, false)]
        [VisibleIf(nameof(SiteId), VisibleIfOperators.NotEqual, (string)null)]
        public string SiteListId { get; set; }

        [DisplayName(translationKey: nameof(ConfigurationMessages.c_sharepoint_site_folder_ids_display_name))]
        [Description(translationKey: nameof(ConfigurationMessages.c_sharepoint_site_folder_ids_description))]
        [DynamicAllowedValuesProvider(typeof(SharepointFolderIdProvider))]
        [FormItemVisibility(Visibility = FormItemVisibilityEnum.Advanced)]
        [VisibleIf(nameof(HighSecurityMode), VisibleIfOperators.Equal, false)]
        [VisibleIf(nameof(SiteId), VisibleIfOperators.NotEqual, (string)null)]
        public string[] SiteFolderIds { get; set; }

        [Required]
        [DisplayName(translationKey: nameof(ConfigurationMessages.c_sharepoint_tenant_id_display_name))]
        [Description(translationKey: nameof(ConfigurationMessages.c_sharepoint_tenant_id_description))]
        [VisibleIf(nameof(HighSecurityMode), VisibleIfOperators.Equal, true)]
        public string TenantId { get; set; }

        [DisplayName(translationKey: nameof(ConfigurationMessages.c_sharepoint_client_id_display_name))]
        [Description(translationKey: nameof(ConfigurationMessages.c_sharepoint_client_id_description))]
        [VisibleIf(nameof(HighSecurityMode), VisibleIfOperators.Equal, true)]
        public string ClientId { get; set; }

        [DisplayName(translationKey: nameof(ConfigurationMessages.c_sharepoint_client_secret_display_name))]
        [Description(translationKey: nameof(ConfigurationMessages.c_sharepoint_client_secret_description))]
        [VisibleIf(nameof(HighSecurityMode), VisibleIfOperators.Equal, true)]
        public string ClientSecret { get; set; }

        [DisplayName(translationKey: nameof(ConfigurationMessages.c_sharepoint_site_drive_id_string_display_name))]
        [Description(translationKey: nameof(ConfigurationMessages.c_sharepoint_site_drive_id_string_description))]
        [DynamicAllowedValuesProvider(typeof(SharepointDriveIdProvider))]
        [FormItemVisibility(Visibility = FormItemVisibilityEnum.Advanced)]
        [VisibleIf(nameof(HighSecurityMode), VisibleIfOperators.Equal, true)]
        [VisibleIf(nameof(SiteIdString), VisibleIfOperators.NotEqual, (string)null)]
        public string SiteDriveIdString { get; set; }

        [DisplayName(translationKey: nameof(ConfigurationMessages.c_sharepoint_site_list_id_string_display_name))]
        [Description(translationKey: nameof(ConfigurationMessages.c_sharepoint_site_list_id_string_description))]
        [DynamicAllowedValuesProvider(typeof(SharepointListIdProvider))]
        [FormItemVisibility(Visibility = FormItemVisibilityEnum.Advanced)]
        [VisibleIf(nameof(HighSecurityMode), VisibleIfOperators.Equal, true)]
        [VisibleIf(nameof(SiteIdString), VisibleIfOperators.NotEqual, (string)null)]
        public string SiteListIdString { get; set; }

        [DisplayName(translationKey: nameof(ConfigurationMessages.c_sharepoint_site_folder_ids_string_display_name))]
        [Description(translationKey: nameof(ConfigurationMessages.c_sharepoint_site_folder_ids_string_description))]
        [DynamicAllowedValuesProvider(typeof(SharepointFolderIdProvider))]
        [FormItemVisibility(Visibility = FormItemVisibilityEnum.Advanced)]
        [VisibleIf(nameof(HighSecurityMode), VisibleIfOperators.Equal, true)]
        [VisibleIf(nameof(SiteIdString), VisibleIfOperators.NotEqual, (string)null)]
        public string[] SiteFolderIdsStrings { get; set; }
    }
}
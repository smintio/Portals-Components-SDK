using System;
using SmintIo.Portals.Connector.SharePoint.AllowedValues;
using SmintIo.Portals.SDK.Core.Components;
using SmintIo.Portals.SDK.Core.Configuration.Annotations;
using SmintIo.Portals.SDK.Core.Configuration.Annotations.Validation;

namespace SmintIo.Portals.Connector.SharePoint
{
    [Serializable]
    public class SharepointConnectorConfiguration : IComponentConfiguration
    {
        [DisplayName("en", "Your SharePoint URL", IsDefault = true)]
        [DisplayName("de", "Ihre SharePoint URL")]
        [Description("en", "The web address of your SharePoint instance. Please visit SharePoint, log in, and then copy the URL from the browser address bar. Finally, paste the copied URL here.", IsDefault = true)]
        [Description("de", "Die Adresse Ihrer SharePoint Instanz. Bitte besuchen Sie SharePoint, melden Sie sich an, und kopieren Sie dann die URL aus der Browser Adresszeile. Fügen Sie in Folge die kopierte URL hier ein.")]
        [Required]
        [IsUri(EnforceHttps = true, RemovePathAndQueryString = true)]
        public string SharepointUrl { get; set; }

        [DisplayName("en", "High security mode (only for experts)", IsDefault = true)]
        [DisplayName("de", "Hochsicherer Modus (nur für Experten)")]
        [Description("en", "If this SharePoint connector is to be used in high security environments, please activate this option. Please note that some convenience features of the connector will then be turned off, e.g. the ability to easily select the SharePoint site from a drop-down list. This feature should be used by experts only.",
                    IsDefault = true)]
        [Description("de",
                    "Wenn dieser SharePoint-Connector in einer hochsicheren Umgebung verwendet werden soll, aktivieren Sie bitte diese Option. Beachten Sie, dass in diesem Fall einige bequeme Features des Konnektors nicht verfügbar sind, wie zB. die Möglichkeit, die zu veröffentlichende SharePoint Site aus einer Liste auszuwählen. Diese Funktionalität sollte nur von Experten verwendet werden.")]
        [DefaultValue(false)]
        public bool HighSecurityMode { get; set; }

        [DisplayName("en", "SharePoint site", IsDefault = true)]
        [DisplayName("de", "SharePoint Site")]
        [Description("en", "Please select the SharePoint site whose documents structure should be processed by this connector.",
            IsDefault = true)]
        [Description("de",
            "Bitte wählen Sie die SharePoint Site, deren Dokumentenstruktur von diesem Connector verarbeitet werden soll.")]
        [DynamicAllowedValuesProvider(typeof(SharepointSiteIdProvider))]
        [FormItemVisibility(Visibility = FormItemVisibilityEnum.Advanced)]
        [VisibleIf(nameof(HighSecurityMode), VisibleIfOperators.Equal, false)]
        public string SiteId { get; set; }

        [DisplayName("en", "SharePoint drive", IsDefault = true)]
        [DisplayName("de", "SharePoint-Laufwerk")]
        [Description("en", "Please select the SharePoint drive whose documents structure should be processed by this connector.",
            IsDefault = true)]
        [Description("de",
            "Bitte wählen Sie das SharePoint-Laufwerk aus, deren Dokumentenstruktur von diesem Connector verarbeitet werden soll.")]
        [DynamicAllowedValuesProvider(typeof(SharepointDriveIdProvider))]
        [FormItemVisibility(Visibility = FormItemVisibilityEnum.Advanced)]
        [VisibleIf(nameof(HighSecurityMode), VisibleIfOperators.Equal, false)]
        [VisibleIf(nameof(SiteId), VisibleIfOperators.NotEqual, (string)null)]
        public string SiteDriveId { get; set; }

        [DisplayName("en", "SharePoint list", IsDefault = true)]
        [DisplayName("de", "SharePoint-Liste")]
        [Description("en", "Please select the SharePoint list whose metadata definitions should be processed by this connector.",
            IsDefault = true)]
        [Description("de",
            "Bitte wählen Sie die SharePoint-Liste aus, deren Metadaten-Definitionen von diesem Connector verarbeitet werden sollen.")]
        [DynamicAllowedValuesProvider(typeof(SharepointListIdProvider))]
        [FormItemVisibility(Visibility = FormItemVisibilityEnum.Advanced)]
        [VisibleIf(nameof(HighSecurityMode), VisibleIfOperators.Equal, false)]
        [VisibleIf(nameof(SiteId), VisibleIfOperators.NotEqual, (string)null)]
        public string SiteListId { get; set; }

        [DisplayName("en", "SharePoint folders to sync", IsDefault = true)]
        [DisplayName("de", "Zu synchronisierende SharePoint-Ordner")]
        [Description("en", "Please select the SharePoint folders whose documents structure should be processed by this connector.",
            IsDefault = true)]
        [Description("de",
            "Bitte wählen Sie die SharePoint-Ordner aus, deren Dokumentenstruktur von diesem Connector verarbeitet werden soll.")]
        [DynamicAllowedValuesProvider(typeof(SharepointFolderIdProvider))]
        [FormItemVisibility(Visibility = FormItemVisibilityEnum.Advanced)]
        [VisibleIf(nameof(HighSecurityMode), VisibleIfOperators.Equal, false)]
        [VisibleIf(nameof(SiteId), VisibleIfOperators.NotEqual, (string)null)]
        public string[] SiteFolderIds { get; set; }

        [Required]
        [DisplayName("en", "Azure tenant ID", IsDefault = true)]
        [DisplayName("de", "Azure Tenant-ID")]
        [Description("en", "Please enter the tenant ID of the Azure app registration.", IsDefault = true)]
        [Description("de", "Bitte geben Sie die Tenant-ID Ihrer Azure App-Registrierung ein.")]
        [VisibleIf(nameof(HighSecurityMode), VisibleIfOperators.Equal, true)]
        public string TenantId { get; set; }

        [DisplayName("en", "Azure client ID", IsDefault = true)]
        [DisplayName("de", "Azure Client-ID")]
        [Description("en", "Please enter the client ID of the Azure app registration.", IsDefault = true)]
        [Description("de", "Bitte geben Sie die Client-ID Ihrer Azure App-Registrierung ein.")]
        [VisibleIf(nameof(HighSecurityMode), VisibleIfOperators.Equal, true)]
        public string ClientId { get; set; }

        [DisplayName("en", "Azure client secret", IsDefault = true)]
        [DisplayName("de", "Azure Client Secret")]
        [Description("en", "Please enter the client secret of the Azure app registration.", IsDefault = true)]
        [Description("de", "Bitte geben Sie das Client-Secret Ihrer Azure App-Registrierung ein.")]
        [VisibleIf(nameof(HighSecurityMode), VisibleIfOperators.Equal, true)]
        public string ClientSecret { get; set; }

        [DisplayName("en", "SharePoint site ID", IsDefault = true)]
        [DisplayName("de", "SharePoint Site ID")]
        [Description("en", "Please enter the SharePoint site ID of the SharePoint site whose documents structure should be processed by this connector.",
            IsDefault = true)]
        [Description("de",
            "Bitte geben Sie die SharePoint Site ID der SharePoint Site ein, deren Dokumentenstruktur von diesem Connector verarbeitet werden soll.")]
        [FormItemVisibility(Visibility = FormItemVisibilityEnum.Advanced)]
        [VisibleIf(nameof(HighSecurityMode), VisibleIfOperators.Equal, true)]
        public string SiteIdString { get; set; }

        [DisplayName("en", "SharePoint drive", IsDefault = true)]
        [DisplayName("de", "SharePoint-Laufwerk")]
        [Description("en", "Please select the SharePoint drive whose documents structure should be processed by this connector.",
            IsDefault = true)]
        [Description("de",
            "Bitte wählen Sie das SharePoint-Laufwerk aus, deren Dokumentenstruktur von diesem Connector verarbeitet werden soll.")]
        [DynamicAllowedValuesProvider(typeof(SharepointDriveIdProvider))]
        [FormItemVisibility(Visibility = FormItemVisibilityEnum.Advanced)]
        [VisibleIf(nameof(HighSecurityMode), VisibleIfOperators.Equal, true)]
        [VisibleIf(nameof(SiteIdString), VisibleIfOperators.NotEqual, (string)null)]
        public string SiteDriveIdString { get; set; }

        [DisplayName("en", "SharePoint list", IsDefault = true)]
        [DisplayName("de", "SharePoint-Liste")]
        [Description("en", "Please select the SharePoint list whose metadata definitions should be processed by this connector.",
            IsDefault = true)]
        [Description("de",
            "Bitte wählen Sie die SharePoint-Liste aus, deren Metadaten-Definitionen von diesem Connector verarbeitet werden sollen.")]
        [DynamicAllowedValuesProvider(typeof(SharepointListIdProvider))]
        [FormItemVisibility(Visibility = FormItemVisibilityEnum.Advanced)]
        [VisibleIf(nameof(HighSecurityMode), VisibleIfOperators.Equal, true)]
        [VisibleIf(nameof(SiteIdString), VisibleIfOperators.NotEqual, (string)null)]
        public string SiteListIdString { get; set; }

        [DisplayName("en", "SharePoint site folders to sync", IsDefault = true)]
        [DisplayName("de", "Zu synchronisierende SharePoint-Site-Ordner")]
        [Description("en", "Please select the SharePoint site folders whose documents structure should be processed by this connector.",
            IsDefault = true)]
        [Description("de",
            "Bitte wählen Sie die SharePoint-Site-Ordner aus, deren Dokumentenstruktur von diesem Connector verarbeitet werden soll.")]
        [DynamicAllowedValuesProvider(typeof(SharepointFolderIdProvider))]
        [FormItemVisibility(Visibility = FormItemVisibilityEnum.Advanced)]
        [VisibleIf(nameof(HighSecurityMode), VisibleIfOperators.Equal, true)]
        [VisibleIf(nameof(SiteIdString), VisibleIfOperators.NotEqual, (string)null)]
        public string[] SiteFolderIdsStrings { get; set; }
    }
}
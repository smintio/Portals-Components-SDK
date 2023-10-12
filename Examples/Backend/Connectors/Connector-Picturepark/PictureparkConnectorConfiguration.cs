using SmintIo.Portals.Connector.Picturepark.AllowedValues;
using SmintIo.Portals.ConnectorSDK.Connectors.Configurations;
using SmintIo.Portals.SDK.Core.Configuration.Annotations;
using SmintIo.Portals.SDK.Core.Configuration.Annotations.Validation;
using System;

namespace SmintIo.Portals.Connector.Picturepark
{
    [Serializable]
    public class PictureparkConnectorConfiguration : IMetamodelConnectorConfiguration
    {
        [DisplayName("en", "Your Picturepark URL", IsDefault = true)]
        [DisplayName("de", "Ihre Picturepark URL")]
        [Description("en", "The web address of your Picturepark Content Platform instance. Please visit Picturepark Content Platform, log in, and then copy the URL from the browser address bar. Finally, paste the copied URL here.", IsDefault = true)]
        [Description("de", "Die Adresse Ihrer Picturepark Content Platform Instanz. Bitte besuchen Sie die Picturepark Content Platform, melden Sie sich an, und kopieren Sie dann die URL aus der Browser Adresszeile. Fügen Sie in Folge die kopierte URL hier ein.")]
        [Required]
        [IsUri(EnforceHttps = true, RemovePathAndQueryString = true)]
        public string PictureparkUrl { get; set; }

        [DisplayName("en", "Your access token", IsDefault = true)]
        [DisplayName("de", "Ihr Zugriffs-Token")]
        [Description("en", "The access token for your Picturepark API Client. You can set up the API client in Picturepark Content Platform by visiting Settings > API Clients > Create New. Use the password method. Once the API client has been created you can create the access token.", IsDefault = true)]
        [Description("de", "Der Zugriffs-Token für Ihren Picturepark API Clients. Sie können den API Client in Picturepark Content Platform unter Einstellungen > Neu erstellen anlegen. Benutzen Sie die Passwort-Methode. Nachdem der API Client erstellt wurde, können Sie das Zugriffs-Token erstellen.")]
        [MaxLength(100)]
        [Required]
        public string AccessToken { get; set; }

        [DisplayName("en", "Channel", IsDefault = true)]
        [Description("en", "The Picturepark channel to use.", IsDefault = true)]
        [Description("de", "Der zu verwendende Picturepark-Channel.")]
        [DefaultValue("rootChannel")]
        [DynamicAllowedValuesProvider(typeof(ChannelAllowedValuesProvider))]
        [FormItemVisibility(Visibility = FormItemVisibilityEnum.Advanced)]
        [MaxLength(255)]
        public string Channel { get; set; }

        [DisplayName("en", "Enable legacy thumbnail override mode", IsDefault = true)]
        [DisplayName("de", "Veralteten Thumbnail-Override-Modus aktivieren")]
        [Description("en", "Enable the legacy thumbnail override mode. Please only enable if instructed to do so by Smint.io support.", IsDefault = true)]
        [Description("de", "Den veralteten Thumbnail-Override-Modus aktivieren. Bitte tun Sie dies nur, wenn Sie vom Smint.io Kundendienst dazu aufgefordert wurden.")]
        [Required]
        [DefaultValue(false)]
        [FormItemVisibility(Visibility = FormItemVisibilityEnum.Expert)]
        public bool LegacyThumbnailsEnabled { get; set; }

        public int? MergeMetamodelToConnectorId { get; set; }

        public int? OverrideDataSourceIds { get; set; }
    }
}

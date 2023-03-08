using System;
using SmintIo.Portals.SDK.Core.Components;
using SmintIo.Portals.SDK.Core.Configuration.Annotations;

namespace SmintIo.Portals.Connector.HelloWorld
{
    [Serializable]
    public class HelloWorldConnectorConfiguration : IComponentConfiguration
    {
        [DisplayName("en", "HelloWorld site", IsDefault = true)]
        [DisplayName("de", "HelloWorld Site")]
        [Description("en", "Please select the HelloWorld site whose documents structure should be processed by this connector.", IsDefault = true)]
        [Description("de", "Bitte wählen Sie die HelloWorld Site, deren Dokumentenstruktur von diesem Connector verarbeitet werden soll.")]
        [FormItemVisibility(Visibility = FormItemVisibilityEnum.Advanced)]
        public string SiteUrl { get; set; }

        [Required]
        [DisplayName("en", "Your HelloWorld client ID", IsDefault = true)]
        [DisplayName("de", "Ihre HelloWorld Client-ID")]
        [Description("en", "Please enter the client ID of the HelloWorld app registration.", IsDefault = true)]
        [Description("de", "Bitte geben Sie die Client-ID Ihrer HelloWorld App-Registrierung ein.")]
        public string ClientId { get; set; }

        [Required]
        [DisplayName("en", "HelloWorld client secret", IsDefault = true)]
        [DisplayName("de", "HelloWorld Client Secret")]
        [Description("en", "Please enter the client secret of the HelloWorld app registration.", IsDefault = true)]
        [Description("de", "Bitte geben Sie das Client-Secret Ihrer HelloWorld App-Registrierung ein.")]
        public string ClientSecret { get; set; }
    }
}
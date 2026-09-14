using System;
using SmintIo.Portals.SDK.Core.Components;
using SmintIo.Portals.SDK.Core.Configuration.Annotations;
using SmintIo.Portals.SDK.Core.Configuration.Annotations.Validation;

namespace SmintIo.Portals.Connector.HelloWorld
{
    [Serializable]
    /// <summary>
    /// SmintIo.Portals.SDK.Core.Configuration.Annotations attributes are used to drive how the connector configuration UI page should look like
    /// In case of `SiteUrl`, a text box will be rendered with a proper label attribute (display name) and with a tool tip for the description
    /// During connector configuration UI page save a custom form validation can be triggered using Smint.Io validation attributes such as `Required`, `IsUri` and others
    /// </summary>
    public class HelloWorldConnectorConfiguration : IComponentConfiguration
    {
        [Required]
        [DisplayName("en", "HelloWorld site", IsDefault = true)]
        [DisplayName("de", "HelloWorld Site")]
        [Description("en", "Please select the HelloWorld site whose documents structure should be processed by this connector.", IsDefault = true)]
        [Description("de", "Bitte wählen Sie die HelloWorld Site, deren Dokumentenstruktur von diesem Connector verarbeitet werden soll.")]
        [IsUri(EnforceHttps = true, RemovePathAndQueryString = true)]
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
using Newtonsoft.Json;
using SmintIo.Portals.SDK.Core.Rest;
using System.Collections.Generic;

namespace SmintIo.Portals.Connector.Picturepark.Model
{
    public class CustomerServiceInfoResponse : IJsonResponse
    {
        [JsonProperty("customerId")]
        public string CustomerId { get; set; }

        [JsonProperty("customerAlias")]
        public string CustomerAlias { get; set; }

        [JsonProperty("identityServerUrl")]
        public string IdentityServerUrl { get; set; }

        [JsonProperty("apiUrl")]
        public string ApiUrl { get; set; }

        [JsonProperty("baseUrl")]
        public string BaseUrl { get; set; }

        [JsonProperty("outputFormats")]
        public List<OutputFormatInfo> OutputFormats { get; set; }
    }

    public class OutputFormatInfo {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("names")]
        public Dictionary<string, string> Names { get; set; }
    }


    /*
    
    {
       "customerId":"07a0e174138e486e91a256a7b2d57220",
       "name":"Local\\smint",
       "customerAlias":"smint",
       "identityServerUrl":"https://identity.poc-picturepark.com/",
       "apiUrl":"https://api.poc-picturepark.com/",
       "enableQueryDetails":false,
       "languageConfiguration":{
          "systemLanguages":[
             "en",
             "de"
          ],
          "metadataLanguages":[
             "en",
             "de"
          ],
          "defaultLanguage":"en"
       },
       "languages":[
          {
             "name":{
                "x-default":"English",
                "de":"Englisch",
                "en":"English"
             },
             "ietf":"en",
             "twoLetterISOLanguageName":"iv",
             "threeLetterISOLanguageName":"ivl",
             "regionCode":""
          },
          {
             "name":{
                "x-default":"German",
                "de":"Deutsch",
                "en":"German"
             },
             "ietf":"de",
             "twoLetterISOLanguageName":"iv",
             "threeLetterISOLanguageName":"ivl",
             "regionCode":""
          }
       ],
       "outputFormats":[
          {
             "id":"AudioPreview",
             "names":{
                "x-default":"Audio waveform preview",
                "de":"Audio-Wellenform Voransicht",
                "en":"Audio waveform preview"
             }
          },
          {
             "id":"AudioSmall",
             "names":{
                "x-default":"Audio",
                "de":"Audio",
                "en":"Audio"
             }
          },
          {
             "id":"DocumentPreview",
             "names":{
                "x-default":"Document preview",
                "de":"Dokument Voransicht",
                "en":"Document preview"
             }
          },
          {
             "id":"Original",
             "names":{
                "x-default":"Original",
                "de":"Original",
                "en":"Original"
             }
          },
          {
             "id":"Pdf",
             "names":{
                "x-default":"PDF",
                "de":"PDF",
                "en":"PDF"
             }
          },
          {
             "id":"Preview",
             "names":{
                "x-default":"Preview",
                "de":"Voransicht",
                "en":"Preview"
             }
          },
          {
             "id":"Svg",
             "names":{
                "x-default":"SVG",
                "de":"SVG",
                "en":"SVG"
             }
          },
          {
             "id":"ThumbnailLarge",
             "names":{
                "x-default":"Large thumbnail",
                "de":"Vorschau gross",
                "en":"Large thumbnail"
             }
          },
          {
             "id":"ThumbnailMedium",
             "names":{
                "x-default":"Medium thumbnail",
                "de":"Vorschau mittel",
                "en":"Medium thumbnail"
             }
          },
          {
             "id":"ThumbnailSmall",
             "names":{
                "x-default":"Small thumbnail",
                "de":"Vorschau klein",
                "en":"Small thumbnail"
             }
          },
          {
             "id":"VectorPreview",
             "names":{
                "x-default":"Vector preview",
                "de":"Vektor Voransicht",
                "en":"Vector preview"
             }
          },
          {
             "id":"VideoKeyframes",
             "names":{
                "x-default":"Video keyframes",
                "de":"Video-Keyframes",
                "en":"Video keyframes"
             }
          },
          {
             "id":"VideoLarge",
             "names":{
                "x-default":"Large video",
                "de":"Video gross",
                "en":"Large video"
             }
          },
          {
             "id":"VideoPreview",
             "names":{
                "x-default":"Video preview",
                "de":"Video Voransicht",
                "en":"Video preview"
             }
          },
          {
             "id":"VideoSmall",
             "names":{
                "x-default":"Small video",
                "de":"Video klein",
                "en":"Small video"
             }
          }
       ],
       "boostValues":[
          1.0,
          5.0,
          10.0,
          25.0,
          50.0,
          100.0,
          500.0
       ],
       "apps":[
          {
             "appId":"dataExchange",
             "name":{
                "de":"Excel Roundtripping App",
                "en":"Excel Roundtripping App",
                "x-default":"Excel Roundtripping App"
             },
             "description":{
                "de":"Listen- und Content-Items mittels Excel exportieren, importieren und aktualisieren.<br />Entwickelt von <a href=\"https://www.picturepark.com\" target=_blank>Picturepark</a> | <a href=\"https://www.picturepark.com/terms\" target=_blank>Rechtliches</a> | Picturepark-zertifiziert.",
                "en":"Export, import and update list and content items using Excel spreadsheets.<br />Developed by <a href=\"https://www.picturepark.com\" target=_blank>Picturepark</a> | <a href=\"https://www.picturepark.com/terms\" target=_blank>Terms</a> | Picturepark Certified.",
                "x-default":"Export, import and update list and content items using Excel spreadsheets.<br />Developed by <a href=\"https://www.picturepark.com\" target=_blank>Picturepark</a> | <a href=\"https://www.picturepark.com/terms\" target=_blank>Terms</a> | Picturepark Certified."
             },
             "icon":"table-edit"
          }
       ],
       "modificationDate":"2020-10-02T07:49:50.0156766Z",
       "baseUrl":"https://smint.poc-picturepark.com",
       "logosUrl":"https://smint.poc-picturepark.com/service/info/customer/logo/"
    }

    */
}

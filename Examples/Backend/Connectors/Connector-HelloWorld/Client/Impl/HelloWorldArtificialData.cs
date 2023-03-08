using System;
using System.Collections.Generic;
using SmintIo.Portals.Connector.HelloWorld.Models.Common;
using SmintIo.Portals.Connector.HelloWorld.Models.Responses;

namespace SmintIo.Portals.Connector.HelloWorld.Client.Impl
{
    internal static class HelloWorldArtificialData
    {
        public const string CustomStringFieldKey = "100";
        public const string CustomDateFieldKey = "101";
        public const string CustomNumberFieldKey = "102";
        public const string CustomSingleSelectFieldKey = "103";
        public const string CustomMultiSelectFieldKey = "104";

        public const string CustomSingleSelectValueOneKey = "203";

        private const string _helloWorldDownloadUrl = "https://hello-world.fake/download";
        private const string _helloWorldPreviewUrl = "https://hello-world.fake/preview";

        public static readonly HelloWorldCustomFieldResponse CustomStringFieldResponse = new()
        {
            Id = CustomStringFieldKey,
            Label = "Custom string",
            LabelTranslationByCulture = new Dictionary<string, string>
            {
                ["de"] = "Benutzerdefinierte Zeichenfolge"
            },
            CustomFieldType = HelloWorldCustomFieldType.String
        };

        public static readonly HelloWorldCustomFieldResponse CustomDateFieldResponse = new()
        {
            Id = CustomDateFieldKey,
            Label = "Custom date",
            LabelTranslationByCulture = new Dictionary<string, string>
            {
                ["de"] = "Benutzerdefiniertes Datum"
            },
            CustomFieldType = HelloWorldCustomFieldType.Date
        };

        public static readonly HelloWorldCustomFieldResponse CustomNumberFieldResponse = new()
        {
            Id = CustomNumberFieldKey,
            Label = "Custom number",
            LabelTranslationByCulture = new Dictionary<string, string>
            {
                ["de"] = "Benutzerdefinierte Nummer"
            },
            CustomFieldType = HelloWorldCustomFieldType.Number
        };

        public static readonly HelloWorldCustomFieldResponse CustomSingleSelectFieldResponse = new()
        {
            Id = CustomSingleSelectFieldKey,
            Label = "Custom single select list",
            LabelTranslationByCulture = new Dictionary<string, string>
            {
                ["de"] = "Benutzerdefinierte Einzelauswahlliste"
            },
            CustomFieldType = HelloWorldCustomFieldType.SingleSelect,
            MultiOptions = false
        };

        public static readonly HelloWorldCustomFieldResponse CustomMultiSelectFieldResponse = new()
        {
            Id = CustomMultiSelectFieldKey,
            Label = "Custom multi select list",
            LabelTranslationByCulture = new Dictionary<string, string>
            {
                ["de"] = "Benutzerdefinierte Mehrfachauswahlliste"
            },
            CustomFieldType = HelloWorldCustomFieldType.MultiSelect,
            MultiOptions = true
        };

        public static readonly HelloWorldAssetResponse HelloWorldImageAsset = new()
        {
            Id = "1",
            FileName = "sample-image-asset.jpg",
            FileSize = 461229,
            FileExtension = "jpg",
            MimeType = "image/jpeg",
            Version = "2",
            ContentType = HelloWorldContentType.Image,
            CustomFieldValues = new List<HelloWorldCustomFieldValueResponse>
            {
                new HelloWorldCustomFieldValueResponse
                {
                    CustomFieldId = CustomSingleSelectFieldKey,
                    Value = new HelloWorldSingleSelectFieldValueResponse
                    {
                        Id =  CustomSingleSelectValueOneKey,
                        Label = "Value 1",
                        LabelTranslationByCulture  = new Dictionary<string, string>
                        {
                            ["de"] = "Wert 1"
                        }
                    }
                },
                new HelloWorldCustomFieldValueResponse
                {
                    CustomFieldId = CustomStringFieldKey,
                    Value = new HelloWorldStringFieldValueResponse
                    {
                        Label = "Custom string value",
                        LabelTranslationByCulture  = new Dictionary<string, string>
                        {
                            ["de"] = "Benutzerdefinierter Zeichenfolgenwert"
                        }
                    }
                }
            },
            Tags = new List<HelloWorldTagResponse>
            {
                new HelloWorldTagResponse
                {
                    Labels = new[] { "Tag 1", "Tag 2" },
                    LabelsTranslationByCulture = new Dictionary<string, string[]>
                    {
                        ["de"] = new[] { "Etikett 1", "Etikett 2" }
                    }
                }
            },
            DownloadUrl = $"{_helloWorldDownloadUrl}/1",
            PreviewUrl = $"{_helloWorldPreviewUrl}/1"
        };

        public static readonly HelloWorldAssetResponse HelloWorldVideoAsset = new()
        {
            Id = "2",
            FileName = "sample-video-asset.mp4",
            FileSize = 543879,
            FileExtension = "mp4",
            MimeType = "video/mp4",
            Version = "1",
            ContentType = HelloWorldContentType.Video,
            CustomFieldValues = Array.Empty<HelloWorldCustomFieldValueResponse>(),
            DownloadUrl = $"{_helloWorldDownloadUrl}/2",
            PreviewUrl = $"{_helloWorldPreviewUrl}/2"
        };

        public static readonly HelloWorldAssetResponse HelloWorldAudioAsset = new()
        {
            Id = "3",
            FileName = "sample-audio-asset.mp3",
            FileSize = 2113939,
            FileExtension = "mp3",
            MimeType = "audio/mpeg",
            Version = "1",
            ContentType = HelloWorldContentType.Audio,
            CustomFieldValues = Array.Empty<HelloWorldCustomFieldValueResponse>(),
            DownloadUrl = $"{_helloWorldDownloadUrl}/3",
            PreviewUrl = $"{_helloWorldPreviewUrl}/3"
        };

        public static readonly HelloWorldAssetResponse HelloWorldDocumentAsset = new()
        {
            Id = "4",
            FileName = "sample-document-asset.pdf",
            FileSize = 364907,
            FileExtension = "pdf",
            MimeType = "application/pdf",
            Version = "1",
            ContentType = HelloWorldContentType.Document,
            CustomFieldValues = Array.Empty<HelloWorldCustomFieldValueResponse>(),
            DownloadUrl = $"{_helloWorldDownloadUrl}/4",
            PreviewUrl = $"{_helloWorldPreviewUrl}/4"
        };

        public static readonly Dictionary<string, HelloWorldCustomFieldResponse> CustomFieldsById = new()
        {
            [CustomStringFieldKey] = CustomStringFieldResponse,
            [CustomDateFieldKey] = CustomDateFieldResponse,
            [CustomNumberFieldKey] = CustomNumberFieldResponse,
            [CustomSingleSelectFieldKey] = CustomSingleSelectFieldResponse,
            [CustomMultiSelectFieldKey] = CustomMultiSelectFieldResponse
        };

        public static readonly List<HelloWorldAssetResponse> HelloWorldAssets = new()
        {
            HelloWorldImageAsset,
            HelloWorldVideoAsset,
            HelloWorldAudioAsset,
            HelloWorldDocumentAsset
        };
    }
}

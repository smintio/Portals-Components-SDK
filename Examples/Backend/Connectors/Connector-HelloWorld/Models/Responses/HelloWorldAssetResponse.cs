using System;
using System.Collections.Generic;
using SmintIo.Portals.Connector.HelloWorld.Models.Common;

namespace SmintIo.Portals.Connector.HelloWorld.Models.Responses
{
    public class HelloWorldAssetResponse
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string FileName { get; set; }

        public string FileExtension { get; set; }

        public long? FileSize { get; set; }

        public string MimeType { get; set; }

        public HelloWorldContentType ContentType { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset? ModifiedAt { get; set; }

        public string CreatedBy { get; set; }

        public string ModifiedBy { get; set; }

        public string[] ParantFolders { get; set; } = Array.Empty<string>();

        public string[] Tags { get; set; }

        public string Version { get; set; }

        public string ETag { get; set; }

        public HelloWorldImagePreviewResponse ImagePreview { get; set; }

        public HelloWorldVideoPreviewResponse VideoPreview { get; set; }

        public ICollection<HelloWorldCustomFieldValueResponse> CustomFieldValues { get; set; }

        public string PreviewUrl { get; set; }

        public string DownloadUrl { get; set; }
    }
}

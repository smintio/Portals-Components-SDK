using System;
using System.Web;
using Newtonsoft.Json;

namespace SmintIo.Portals.DataAdapter.SharePoint.Assets.Models
{
    public class SharepointItemPreview
    {
        [JsonProperty(".spItemUrl")]
        public string SpItemUrl { get; set; }

        [JsonProperty(".transformUrl")]
        public Uri TransformUrl { get; set; }

        [JsonProperty(".driveAccessToken")]
        public string DriveAccessToken { get; set; }

        [JsonProperty(".driveAccessTokenV21")]
        public string DriveAccessTokenV21 { get; set; }

        [JsonProperty(".downloadUrl")]
        public string DownloadUrl { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("size")]
        public string Size { get; set; }

        [JsonProperty(".etag")]
        public string Etag { get; set; }

        [JsonProperty(".ctag")]
        public string Ctag { get; set; }

        [JsonProperty("webUrl")]
        public string WebUrl { get; set; }

        [JsonProperty("MediaServiceFastMetadata")]
        public string MediaServiceFastMetadata { get; set; }

        public MediaServiceFastMetadata ParsedMediaServiceFastMetadata { get; set; }

        public override string ToString()
        {
            if (TransformUrl == null)
            {
                throw new InvalidOperationException($"Couldn't process {nameof(SharepointItemPreview)}.{nameof(TransformUrl)}");
            }

            var path = TransformUrl.GetLeftPart(UriPartial.Path).Replace("thumbnail", "videomanifest");

            var uriBuilder = new UriBuilder(path);
            var manifestQuery = HttpUtility.ParseQueryString(uriBuilder.Query);

            var parsedTransformUrlQuery = HttpUtility.ParseQueryString(TransformUrl.Query);

            var provider = parsedTransformUrlQuery.Get("provider");
            manifestQuery[nameof(provider)] = provider;

            var inputFormat = parsedTransformUrlQuery.Get("inputFormat");
            manifestQuery[nameof(inputFormat)] = inputFormat;

            manifestQuery["ctag"] = Ctag;

            var cs = parsedTransformUrlQuery.Get("cs");
            manifestQuery[nameof(cs)] = cs;

            var docid = HttpUtility.UrlDecode(parsedTransformUrlQuery.Get("docid"));
            var docUri = new Uri(docid);

            if (docUri == null)
            {
                throw new InvalidOperationException($"Couldn't process {nameof(SharepointItemPreview)}.{nameof(docUri)}");
            }

            manifestQuery[nameof(docid)] = docUri.GetLeftPart(UriPartial.Path);

            manifestQuery["action"] = "Access";
            manifestQuery["part"] = "index";
            manifestQuery["format"] = "dash";
            manifestQuery["useScf"] = "True";
            manifestQuery["altTranscode"] = "1";

            if (ParsedMediaServiceFastMetadata != null && ParsedMediaServiceFastMetadata.Video != null)
            {
                manifestQuery["altManifestMetadata"] = ParsedMediaServiceFastMetadata.Video.AltManifestMetadata;
            }

            manifestQuery["pretranscode"] = "0";
            manifestQuery["transcodeahead"] = "0";
            manifestQuery["enableCdn"] = "1";
            manifestQuery["ccat"] = "0";
            manifestQuery["hybridPlayback"] = "true";
            manifestQuery["tempauth"] = DriveAccessToken;

            uriBuilder.Query = manifestQuery.ToString();

            var videoManifestUrl = uriBuilder.Uri.ToString();

            return videoManifestUrl;
        }
    }
}
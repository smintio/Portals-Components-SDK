using System;
using System.IO;
using System.Linq;
using Microsoft.Graph;
using SmintIo.Portals.Connector.SharePoint.Helpers;
using SmintIo.Portals.SDK.Core.Models.Metamodel.Data;

namespace SmintIo.Portals.Connector.SharePoint.Extensions
{
    public static class DriveItemExtensions
    {
        public static string GetAssetId(this DriveItem driveItem)
        {
            return $"{EscapeUnderscore(driveItem.ParentReference.DriveId)}__{driveItem.Id}";
        }

        public static string GetParentAssetId(this DriveItem driveItem)
        {
            if (string.IsNullOrEmpty(driveItem.ParentReference.Id))
            {
                return null;
            }

            return $"{EscapeUnderscore(driveItem.ParentReference.DriveId)}__{driveItem.ParentReference.Id}";
        }

        private static string EscapeUnderscore(string id)
        {
            if (string.IsNullOrEmpty(id))
                return id;

            if (id.Contains("|"))
            {
                throw new Exception($"ID {id} contains escape character '|'");
            }

            id = id.Replace("_", "|");

            return id;
        }

        public static string GetFileName(this DriveItem driveItem)
        {
            return Path.GetFileNameWithoutExtension(driveItem.Name);
        }

        public static string GetFileExtension(this DriveItem driveItem)
        {
            return Path.GetExtension(driveItem.Name).ToLowerInvariant();
        }

        public static ContentTypeEnumDataObject GetContentType(this DriveItem driveItem, string fileExtension)
        {
            // Mime type is needed for cases like 'svg'.
            // Sharepoint doesn't treat it as image.
            var mimeType = driveItem.File?.MimeType ?? string.Empty;

            // During the initial graphApi search we don't have the file, audio, image nor the video information.
            if (driveItem.Audio != null || mimeType.StartsWith("audio"))
            {
                return ContentTypeEnumDataObject.Audio;
            }
            else if (driveItem.Video != null || mimeType.StartsWith("video"))
            {
                return ContentTypeEnumDataObject.Video;
            }
            else if (driveItem.Image != null || driveItem.Photo != null || mimeType.StartsWith("image"))
            {
                return ContentTypeEnumDataObject.Image;
            }
            else if (PathHelpers.KnownDocumentFormats.Contains(fileExtension))
            {
                return ContentTypeEnumDataObject.Document;
            }

            return ContentTypeEnumDataObject.Other;
        }

        public static bool IsFolder(this DriveItem driveItem)
        {
            return driveItem.Folder != null;
        }

        public static bool IsAsset(this DriveItem driveItem)
        {
            return driveItem.Folder == null;
        }

        public static bool CanBeDeleted(this DriveItem driveItem)
        {
            return driveItem.Deleted != null;
        }

        public static bool CanBeUpdated(this DriveItem driveItem)
        {
            return driveItem.Deleted == null;
        }
    }
}

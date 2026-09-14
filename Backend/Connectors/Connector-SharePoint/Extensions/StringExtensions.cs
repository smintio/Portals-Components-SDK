using System;

namespace SmintIo.Portals.Connector.SharePoint.Extensions
{
    public static class StringExtensions
    {
        public const string DoubleUnderscore = "__";

        public static (string driveId, string itemId) Parse(this string assetId)
        {
            if (!assetId.Contains(DoubleUnderscore))
            {
                throw new ArgumentException($"AssetId should be prefixed with driveId and itemId in the form <driveId>{DoubleUnderscore}<itemId> (separated by a double-underscore), but it wasn't!");
            }

            var assetIdParts = assetId.Split(DoubleUnderscore, StringSplitOptions.RemoveEmptyEntries);

            if (assetIdParts.Length != 2)
            {
                throw new ArgumentException($"AssetId should contain exactly TWO parts, separated by a double-underscore (\"{DoubleUnderscore}\"), but it had {assetIdParts.Length}");
            }

            var driveId = assetIdParts[0];

            driveId = driveId.Replace("|", "_");

            return (driveId, assetIdParts[1]);
        }
    }
}

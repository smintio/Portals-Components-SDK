using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Graph;

namespace SmintIo.Portals.Connector.SharePoint.Extensions
{
    public static class QueryOptionExtensions
    {
        public static void SetSkipToken(this ICollection<QueryOption> queryOptions, string skipToken)
        {
            if (string.IsNullOrEmpty(skipToken))
            {
                return;
            }

            queryOptions.Add(new QueryOption("$skipToken", skipToken));
        }

        public static void SetPageSize(this ICollection<QueryOption> queryOptions, int? pageSize)
        {
            if (!pageSize.HasValue)
            {
                return;
            }

            queryOptions.Add(new QueryOption("$top", pageSize.Value.ToString()));
        }

        public static void Expand(this ICollection<QueryOption> queryOptions, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            queryOptions.Add(new QueryOption("$expand", value));
        }

        public static string GetNextSkipToken(this IEnumerable<QueryOption> queryOptions)
        {
            if (queryOptions == null || !queryOptions.Any())
            {
                return null;
            }

            return queryOptions
                .FirstOrDefault(qo => qo.Name.Contains("$skipToken", StringComparison.OrdinalIgnoreCase))
                ?.Value;
        }
    }
}

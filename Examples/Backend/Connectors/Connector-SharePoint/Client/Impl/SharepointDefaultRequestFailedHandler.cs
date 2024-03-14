using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Models.ODataErrors;
using SmintIo.Portals.SDK.Core.Http.Prefab.Exceptions;
using SmintIo.Portals.SDK.Core.Http.Prefab.Extensions;
using SmintIo.Portals.SDK.Core.Http.Prefab.RequestFailedHandlers;
using SmintIo.Portals.SDK.Core.Models.Context;
using SmintIo.Portals.SDK.Core.Rest.Prefab.Exceptions;

namespace SmintIo.Portals.Connector.SharePoint.Client.Impl
{
    public class SharepointDefaultRequestFailedHandler : DefaultRequestFailedHandler
    {
        private const string _retryAfter = "Retry-After";

        private readonly ILogger _logger;

        public SharepointDefaultRequestFailedHandler(ILogger logger)
        {
            _logger = logger;
        }

        public override Task<RequestFailedHandlerResult> HandleOtherExceptionAsync(string requestUri, int tryCount, IPortalsContextModel portalsContextModel, Exception exception)
        {
            if (exception is ODataError oDataError)
            {
                var errorCode = oDataError?.Error?.Code;
                var statusCode = oDataError.ResponseStatusCode;

                // translate service exception to HttpStatusException, handle other Sharepoint states

                var message = oDataError.Message;

                if (!string.IsNullOrEmpty(message))
                {
                    if (message.Contains("VideoProcessing_ByteStreamTypeNotSupported") ||
                        message.Contains("SubStreamCached_GeneralFailure") ||
                        message.Contains("SubStreamCached_FileTooBig") ||
                        message.Contains("SubStreamCached_Fatal") ||
                        message.Contains("Service_InvalidInput_FileTooBigToConvert") ||
                        message.Contains("OfficeConversion_BadRequest") ||
                        message.Contains("VideoBitrateUnsupported_BitrateTooHigh") ||
                        message.Contains("Web_416RequestedRangeNotSatisfiable"))
                    {
                        // permanent error, occuring when reading streams
                        // this will cause the returned stream to be NULL

                        return Task.FromResult(RequestFailedHandlerResult.Ignore);
                    }
                }

                // https://learn.microsoft.com/en-us/graph/errors
                // https://github.com/microsoftgraph/msgraph-sdk-dotnet/blob/dev/src/Microsoft.Graph/Enums/GraphErrorCode.cs

                if (!string.IsNullOrEmpty(errorCode))
                {
                    if (string.Equals(errorCode, "itemNotFound"))
                    {
                        throw new ExternalDependencyException(ExternalDependencyStatusEnum.GetNotFound, "The desired record was not found", SharepointConnectorStartup.SharepointConnector, oDataError);
                    }
                    else if (statusCode == (int)HttpStatusCode.TooManyRequests ||
                        statusCode == 509 || // Bandwidth Limit Exceeded
                        string.Equals(errorCode, "activityLimitReached") ||
                        string.Equals(errorCode, "tooManyRetries") ||
                        string.Equals(errorCode, "tooManyRedirects"))
                    {
                        var retryAfter = GetRetryAfter(oDataError.ResponseHeaders);

                        throw new TooManyRequestsExternalDependencyException("Quota exceeded", retryAfter, SharepointConnectorStartup.SharepointConnector, oDataError);
                    }
                    else if (string.Equals(errorCode, "notSupported"))
                    {
                        // permanent error, occuring when reading streams
                        // this will cause the returned stream to be NULL

                        return Task.FromResult(RequestFailedHandlerResult.Ignore);
                    }
                }

                throw new HttpStatusException(
                    requestUri,
                    tryCount,
                    portalsContextModel,
                    statusCode,
                    statusMessage: $"{message} (error code {errorCode})",
                    responseBody: null,
                    innerException: oDataError);
            }

            return base.HandleOtherExceptionAsync(requestUri, tryCount, portalsContextModel, exception);
        }

        public override Task<RequestFailedHandlerResult> HandleHttpStatusExceptionAsync(HttpStatusException httpStatusException)
        {
            var httpResponseMessage = httpStatusException.HttpResponseMessage;

            if (httpResponseMessage != null &&
                httpResponseMessage.Headers != null &&
                httpResponseMessage.Headers.TryGetValues("x-errorcode", out var values) &&
                values != null &&
                values.Any())
            {
                var errorCode = values.First();

                if (!string.IsNullOrEmpty(errorCode))
                {
                    if (string.Equals(errorCode, "VideoProcessing_ByteStreamTypeNotSupported") ||
                        string.Equals(errorCode, "SubStreamCached_GeneralFailure") ||
                        string.Equals(errorCode, "SubStreamCached_FileTooBig") ||
                        string.Equals(errorCode, "SubStreamCached_Fatal") ||
                        string.Equals(errorCode, "Service_InvalidInput_FileTooBigToConvert") ||
                        string.Equals(errorCode, "OfficeConversion_BadRequest") ||
                        string.Equals(errorCode, "VideoBitrateUnsupported_BitrateTooHigh") ||
                        string.Equals(errorCode, "Web_416RequestedRangeNotSatisfiable"))
                    {
                        // permanent error, occuring when reading streams
                        // this will cause the returned stream to be NULL

                        return Task.FromResult(RequestFailedHandlerResult.Ignore);
                    }
                }
            }

            if (httpStatusException.StatusCode == (int)HttpStatusCode.RequestedRangeNotSatisfiable)
            {
                // permanent error, occuring when reading streams
                // this will cause the returned stream to be NULL

                return Task.FromResult(RequestFailedHandlerResult.Ignore);
            }

            return base.HandleHttpStatusExceptionAsync(httpStatusException);
        }

        /// <summary>
        /// https://learn.microsoft.com/en-us/graph/throttling
        /// https://learn.microsoft.com/en-us/graph/throttling-limits
        /// https://learn.microsoft.com/en-us/azure/architecture/patterns/throttling
        /// </summary>
        private TimeSpan? GetRetryAfter(IDictionary<string, IEnumerable<string>> httpResponseHeaders)
        {
            if (httpResponseHeaders == null || !httpResponseHeaders.TryGetValue(_retryAfter, out var retryAfterValues))
            {
                _logger.LogWarning("SharePoint returned too many requests response without retry after header");

                // IL will fall back to the default retry after value

                return null;
            }

            var retryAfterStringValue = retryAfterValues?.FirstOrDefault();

            if (retryAfterStringValue == null || !double.TryParse(retryAfterStringValue, out var retryAfterInSeconds) || retryAfterInSeconds < 0)
            {
                _logger.LogWarning($"SharePoint returned too many requests response with invalid retry after header ({retryAfterStringValue})");

                return null;
            }

            return TimeSpan.FromSeconds(retryAfterInSeconds);
        }
    }
}

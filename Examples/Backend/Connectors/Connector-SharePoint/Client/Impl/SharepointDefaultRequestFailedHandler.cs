using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Graph;
using SmintIo.Portals.SDK.Core.Http.Prefab.RequestFailedHandlers;
using SmintIo.Portals.SDK.Core.Http.Prefab.Exceptions;
using SmintIo.Portals.SDK.Core.Http.Prefab.Extensions;
using SmintIo.Portals.SDK.Core.Models.Context;
using SmintIo.Portals.SDK.Core.Rest.Prefab.Exceptions;

namespace SmintIo.Portals.Connector.SharePoint.Client.Impl
{
    public class SharepointDefaultRequestFailedHandler : DefaultRequestFailedHandler
    {
        public override Task<RequestFailedHandlerResult> HandleOtherExceptionAsync(string requestUri, int tryCount, IPortalsContextModel portalsContextModel, Exception exception)
        {
            if (exception is ServiceException serviceException)
            {
                // translate service exception to HttpStatusException, handle other Sharepoint states

                var message = serviceException.Message;

                if (!string.IsNullOrEmpty(message))
                {
                    if (message.Contains("VideoProcessing_ByteStreamTypeNotSupported") ||
                        message.Contains("SubStreamCached_GeneralFailure") ||
                        message.Contains("SubStreamCached_FileTooBig") ||
                        message.Contains("Service_InvalidInput_FileTooBigToConvert") ||
                        message.Contains("OfficeConversion_BadRequest") ||
                        message.Contains("VideoBitrateUnsupported_BitrateTooHigh"))
                    {
                        // permanent error, occuring when reading streams
                        // this will cause the returned stream to be NULL

                        return Task.FromResult(RequestFailedHandlerResult.Ignore);
                    }
                }

                var errorCode = serviceException.Error?.Code;
                var statusCode = (int)serviceException.StatusCode;

                if (!string.IsNullOrEmpty(errorCode))
                {
                    if (string.Equals(errorCode, "itemNotFound"))
                    {
                        throw new ExternalDependencyException(ExternalDependencyStatusEnum.GetNotFound, "The desired record was not found", SharepointConnectorStartup.SharepointConnector, serviceException);
                    }
                    else if (string.Equals(errorCode, "tooManyRetries"))
                    {
                        // change status code

                        statusCode = (int)HttpStatusCode.TooManyRequests;
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
                    statusMessage: $"{serviceException.Message} (error code {errorCode})",
                    responseBody: null,
                    innerException: serviceException);
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
                        string.Equals(errorCode, "Service_InvalidInput_FileTooBigToConvert") ||
                        string.Equals(errorCode, "OfficeConversion_BadRequest") ||
                        string.Equals(errorCode, "VideoBitrateUnsupported_BitrateTooHigh"))
                    {
                        // permanent error, occuring when reading streams
                        // this will cause the returned stream to be NULL

                        return Task.FromResult(RequestFailedHandlerResult.Ignore);
                    }
                }
            }

            return base.HandleHttpStatusExceptionAsync(httpStatusException);
        }
    }
}

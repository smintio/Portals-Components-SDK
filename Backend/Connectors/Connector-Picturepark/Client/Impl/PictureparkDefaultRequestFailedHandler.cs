using System;
using System.Net;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;
using SmintIo.Portals.SDK.Core.Http.Prefab.RequestFailedHandlers;
using SmintIo.Portals.SDK.Core.Http.Prefab.Exceptions;
using SmintIo.Portals.SDK.Core.Http.Prefab.Extensions;
using SmintIo.Portals.SDK.Core.Models.Context;
using SmintIo.Portals.SDK.Core.Rest.Prefab.Exceptions;

namespace SmintIo.Portals.Connector.Picturepark.Client.Impl
{
    public class PictureparkDefaultRequestFailedHandler : DefaultRequestFailedHandler
    {
        private readonly DefaultPictureparkClient _defaultPictureparkClient;

        public PictureparkDefaultRequestFailedHandler(DefaultPictureparkClient defaultPictureparkClient)
        {
            _defaultPictureparkClient = defaultPictureparkClient;
        }

        public override async Task<RequestFailedHandlerResult> HandleOtherExceptionAsync(string requestUri, int tryCount, IPortalsContextModel portalsContextModel, Exception exception)
        {
            if (exception is ContentPermissionException)
            {
                throw new ExternalDependencyException(ExternalDependencyStatusEnum.Forbidden, $"Picturepark did not allow the operation to complete", PictureparkConnectorStartup.PictureparkConnector, innerException: exception);
            }

            if (exception is PictureparkNotFoundException)
            {
                throw new ExternalDependencyException(ExternalDependencyStatusEnum.GetNotFound, $"Picturepark did not find the requested record (not found)", PictureparkConnectorStartup.PictureparkConnector, innerException: exception);
            }

            if (exception is ApiException apiException)
            {
                if (apiException.StatusCode == (int)HttpStatusCode.Unauthorized)
                {
                    var requestFailedHandlerResult = await _defaultPictureparkClient.HandleUnauthorizedAsync().ConfigureAwait(false);

                    return requestFailedHandlerResult ?? RequestFailedHandlerResult.Default;
                }
                else
                {
                    // translate Picturepark API exception to HttpStatusException

                    throw new HttpStatusException(
                        requestUri,
                        tryCount,
                        portalsContextModel,
                        statusCode: (int)apiException.StatusCode,
                        statusMessage: apiException.Message,
                        responseBody: null,
                        innerException: apiException);
                }
            }

            return await base.HandleOtherExceptionAsync(requestUri, tryCount, portalsContextModel, exception).ConfigureAwait(false);
        }
    }
}

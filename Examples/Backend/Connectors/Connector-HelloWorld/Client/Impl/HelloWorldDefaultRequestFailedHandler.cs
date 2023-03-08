using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SmintIo.Portals.SDK.Core.Http.Prefab.Exceptions;
using SmintIo.Portals.SDK.Core.Http.Prefab.Extensions;
using SmintIo.Portals.SDK.Core.Http.Prefab.RequestFailedHandlers;

namespace SmintIo.Portals.Connector.HelloWorld.Client.Impl
{
    public class HelloWorldDefaultRequestFailedHandler : DefaultRequestFailedHandler
    {
        public override Task<RequestFailedHandlerResult> HandleHttpStatusExceptionAsync(HttpStatusException httpStatusException)
        {
            if (httpStatusException.StatusCode == (int)HttpStatusCode.Processing)
            {
                return Task.FromResult(RequestFailedHandlerResult.Retry);
            }
            else if (httpStatusException.StatusCode == (int)HttpStatusCode.BadRequest)
            {
                var restResponse = httpStatusException.RestResponse;

                if (restResponse != null)
                {
                    try
                    {
                        // Determine the RequestFailedHandlerResult based on the restResponse.Content

                        return Task.FromResult(RequestFailedHandlerResult.Retry);
                    }
                    catch (JsonSerializationException)
                    {
                        // Nothing to do here
                    }
                }
            }

            return base.HandleHttpStatusExceptionAsync(httpStatusException);
        }
    }
}

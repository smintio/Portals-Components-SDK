using Microsoft.Extensions.DependencyInjection;
using Picturepark.SDK.V1.Contract;
using SmintIo.Portals.Connector.Picturepark.Client;
using SmintIo.Portals.SDK.Core.Models.Paging;
using SmintIo.Portals.SDK.Core.Providers;
using SmintIo.Portals.SDK.Core.Models.Strings;
using SmintIo.Portals.SDK.Core.Rest.Prefab.Exceptions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SmintIo.Portals.Connector.Picturepark.AllowedValues
{
    public class ChannelAllowedValuesProvider : IDynamicValueListProvider<string>
    {
        public bool SupportsSearch => false;

        public bool SupportsPagination => false;

        private readonly IPictureparkClient _pictureparkClient;

        public ChannelAllowedValuesProvider(IServiceProvider serviceProvider)
        {
            // if we are configuring the connector, this can be null

            try
            {
                _pictureparkClient = serviceProvider.GetService<IPictureparkClient>();
            } 
            catch (Exception e)
            when (e.InnerException != null && 
                  (e.InnerException is ExternalDependencyException ||
                   e.InnerException is ArgumentNullException))
            {
                // the connector is not yet set up

                _pictureparkClient = null;
            }
        }

        public async Task<UiDetailsModel<string>> GetDynamicValueAsync(string channelId)
        {
            if (_pictureparkClient == null)
            {
                return new UiDetailsModel<string>()
                {
                    Value = channelId
                };
            }

            try
            {
                var channel = await _pictureparkClient.GetChannelAsync(channelId).ConfigureAwait(false);

                if (channel == null)
                    return null;

                return new UiDetailsModel<string>()
                {
                    Value = channel.Id,
                    Name = channel.Names?.ConvertToLocalizedStringsModel()
                };
            } 
            catch (Exception)
            {
                // do not make major issue out of this

                return new UiDetailsModel<string>()
                {
                    Value = channelId,
                    Name = new LocalizedStringsModel()
                    {
                        { LocalizedStringsModel.DefaultCulture, channelId }
                    }
                };
            }
        }

        public async Task<PagingResult<UiDetailsModel<string>>> GetDynamicValueListAsync(string searchTerm, int? offset, int? limit, string parentValue)
        {
            if (_pictureparkClient == null)
                return null;

            var channels = await _pictureparkClient.GetAllChannelsAsync().ConfigureAwait(false);

            var uiDetailsModels = channels?.Select(channel => new UiDetailsModel<string>()
            {
                Value = channel.Id,
                Name = channel.Names?.ConvertToLocalizedStringsModel()
            }).ToList();

            return new PagingResult<UiDetailsModel<string>>()
            {
                Result = uiDetailsModels,
                TotalCount = uiDetailsModels?.Count ?? 0
            };
        }
    }
}

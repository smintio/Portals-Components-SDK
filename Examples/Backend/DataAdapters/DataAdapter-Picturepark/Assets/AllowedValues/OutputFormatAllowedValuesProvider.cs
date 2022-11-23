using Microsoft.Extensions.DependencyInjection;
using Picturepark.SDK.V1.Contract;
using SmintIo.Portals.SDK.Core.Models.Paging;
using SmintIo.Portals.SDK.Core.Providers;
using SmintIo.Portals.SDK.Core.Models.Strings;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SmintIo.Portals.DataAdapter.Picturepark.Assets.AllowedValues
{
    public class OutputFormatAllowedValuesProvider : IDynamicValueListProvider<string>
    {
        public bool SupportsSearch => false;

        public bool SupportsPagination => false;

        private readonly PictureparkAssetsDataAdapter _pictureparkAssetsDataAdapter;

        public OutputFormatAllowedValuesProvider(IServiceProvider serviceProvider)
        {
            _pictureparkAssetsDataAdapter = serviceProvider.GetService<PictureparkAssetsDataAdapter>();
        }

        public async Task<UiDetailsModel<string>> GetDynamicValueAsync(string outputFormatId)
        {
            if (_pictureparkAssetsDataAdapter == null)
            {
                return new UiDetailsModel<string>()
                {
                    Value = outputFormatId
                };
            }

            try
            {
                var outputFormats = await _pictureparkAssetsDataAdapter.GetOutputFormatsAsync().ConfigureAwait(false);

                if (outputFormats == null || outputFormats.Count == 0)
                    return null;

                var outputFormat = outputFormats.FirstOrDefault(outputFormat => string.Equals(outputFormat.Id, outputFormatId));

                if (outputFormat == null)
                    return null;

                return new UiDetailsModel<string>()
                {
                    Value = outputFormat.Id,
                    Name = outputFormat.Names?.ConvertToLocalizedStringsModel()
                };
            } 
            catch (Exception)
            {
                // do not make major issue out of this

                return new UiDetailsModel<string>()
                {
                    Value = outputFormatId,
                    Name = new LocalizedStringsModel()
                    {
                        { LocalizedStringsModel.DefaultCulture, outputFormatId }
                    }
                };
            }
        }

        public async Task<PagingResult<UiDetailsModel<string>>> GetDynamicValueListAsync(string searchTerm, int? offset, int? limit, string parentValue)
        {
            if (_pictureparkAssetsDataAdapter == null)
                return null;

            var outputFormats = await _pictureparkAssetsDataAdapter.GetOutputFormatsAsync().ConfigureAwait(false);

            var uiDetailsModels = outputFormats?.Select(outputFormat => new UiDetailsModel<string>()
            {
                Value = outputFormat.Id,
                Name = outputFormat.Names?.ConvertToLocalizedStringsModel()
            }).ToList();

            return new PagingResult<UiDetailsModel<string>>()
            {
                Result = uiDetailsModels,
                TotalCount = uiDetailsModels?.Count ?? 0
            };
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Converters;
using SmintIo.Portals.SDK.Core.Models.Metamodel.Data;
using SmintIo.Portals.SDK.Core.Models.Metamodel.Model;

namespace SmintIo.Portals.DataAdapter.Picturepark.Assets.Common
{
    public class PictureparkPostProcessObjectConverter : TranslationPostProcessObjectConverter
    {
        public PictureparkPostProcessObjectConverter(IEntityModelProvider entityModelProvider)
            : base(entityModelProvider)
        {
        }

        protected override void ProcessPropertyModels(IEnumerable<PropertyModel> propertyModels, DataObject dataObject, ICollection<string> propertyKeysToRemove)
        {
            foreach (var propertyModel in propertyModels)
            {
                if (!dataObject.Values.TryGetValue(propertyModel.Key, out var propertyValue))
                {
                    continue;
                }

                if (propertyModel.LinkedTranslationProperties == null || !propertyModel.LinkedTranslationProperties.Any())
                {
                    continue;
                }

                MergeProperties(dataObject, propertyModel, propertyValue, propertyKeysToRemove);
            }
        }
    }
}
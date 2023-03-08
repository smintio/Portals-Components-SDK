using System.Collections.Generic;
using SmintIo.Portals.SDK.Core.Models.Strings;

namespace SmintIo.Portals.Connector.HelloWorld.Extensions
{
    public static class LabelExtensions
    {
        public static LocalizedStringsModel AddTranslations(this LocalizedStringsModel localizedStringsModel, IDictionary<string, string> translationByCulture)
        {
            if (translationByCulture == null || translationByCulture.Count == 0)
            {
                return localizedStringsModel;
            }

            foreach (var translationPair in translationByCulture)
            {
                if (string.IsNullOrEmpty(translationPair.Key))
                {
                    continue;
                }

                if (string.IsNullOrEmpty(translationPair.Value))
                {
                    continue;
                }

                localizedStringsModel.Add(translationPair.Key, translationPair.Value);
            }

            return localizedStringsModel;
        }
    }
}

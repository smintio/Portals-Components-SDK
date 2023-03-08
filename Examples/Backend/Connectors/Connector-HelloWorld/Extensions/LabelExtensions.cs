using System.Collections.Generic;
using System.Linq;
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

        public static LocalizedStringsArrayModel Localize(this string[] values)
        {
            if (values == null || !values.Any())
            {
                return null;
            }

            return new LocalizedStringsArrayModel
            {
                { LocalizedStringsArrayModel.DefaultCulture, values }
            };
        }

        public static LocalizedStringsArrayModel AddTranslations(this LocalizedStringsArrayModel localizedStringsArrayModel, IDictionary<string, string[]> translationByCulture)
        {
            if (translationByCulture == null || translationByCulture.Count == 0)
            {
                return localizedStringsArrayModel;
            }

            foreach (var translationPair in translationByCulture)
            {
                if (string.IsNullOrEmpty(translationPair.Key))
                {
                    continue;
                }

                if (translationPair.Value == null || !translationPair.Value.Any())
                {
                    continue;
                }

                localizedStringsArrayModel.Add(translationPair.Key, translationPair.Value);
            }

            return localizedStringsArrayModel;
        }
    }
}

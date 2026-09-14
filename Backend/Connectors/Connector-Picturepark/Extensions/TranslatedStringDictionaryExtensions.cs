using SmintIo.Portals.SDK.Core.Models.Strings;

namespace Picturepark.SDK.V1.Contract
{
    public static class TranslatedStringDictionaryExtensions
    {
        public static LocalizedStringsModel ConvertToLocalizedStringsModel(this TranslatedStringDictionary translatedStringDictionary)
        {
            if (translatedStringDictionary == null)
                return null;

            return new LocalizedStringsModel(translatedStringDictionary);
        }
    }
}

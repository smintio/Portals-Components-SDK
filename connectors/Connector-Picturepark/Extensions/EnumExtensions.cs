using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Linq;

namespace System
{
    public static class EnumExtensions
    {
        private static List<JsonConverter> converters = new JsonConverter[] { new StringEnumConverter() }.ToList();

        public static string ToPictureparkEnumMemberAttrValue(this Enum e)
        {
            string json = JsonConvert.SerializeObject(e, new JsonSerializerSettings()
            {
                Converters = converters
            });

            json = json?.Replace("\"", "");

            return json;
        }
    }
}
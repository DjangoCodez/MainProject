using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SoftOne.Soe.Common.DTO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Util
{
    public class ReportDataSelectionDTOJsonConverter : JsonConverter
    {
        private readonly static Type baseType = typeof(ReportDataSelectionDTO);
        private readonly static string typePropertyName = Char.ToLowerInvariant(nameof(ReportDataSelectionDTO.TypeName)[0]) + nameof(ReportDataSelectionDTO.TypeName).Substring(1);
        private readonly static Dictionary<string, Type> knownSelectionTypes;

        static ReportDataSelectionDTOJsonConverter()
        {
            knownSelectionTypes = baseType.Assembly.GetTypes()
                .Where(t => t.BaseType == baseType)
                .ToDictionary(t => t.Name);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == baseType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            try
            {
                var jObject = JObject.Load(reader);
                var typeName = jObject.GetValue(typePropertyName).Value<string>();

                Type foundConcreteType = null;
                if (knownSelectionTypes.TryGetValue(typeName, out foundConcreteType))
                {
                    return jObject.ToObject(foundConcreteType, serializer);
                }

                return null;

            }
            catch (JsonReaderException)
            {
                return null;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }

}

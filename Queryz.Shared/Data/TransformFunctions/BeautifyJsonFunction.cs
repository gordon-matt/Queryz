using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Queryz.Data.Domain;

namespace Queryz.Data.TransformFunctions
{
    public class BeautifyJsonFunction : ITransformFunction
    {
        public string Name => "Beautify JSON";

        public dynamic Transform(dynamic value, Report report)
        {
            if (!(value is string))
            {
                return value;
            }

            if (value == null)
            {
                return value;
            }

            JToken obj = null;
            if (!IsValidJson(value, out obj))
            {
                return value;
            }

            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }

        private static bool IsValidJson(string json, out JToken obj)
        {
            json = json.Trim();
            if ((json.StartsWith("{") && json.EndsWith("}")) || // For object
                (json.StartsWith("[") && json.EndsWith("]"))) // For array
            {
                try
                {
                    obj = JToken.Parse(json);
                    return true;
                }
                catch
                {
                    obj = null;
                    return false;
                }
            }
            else
            {
                obj = null;
                return false;
            }
        }
    }
}
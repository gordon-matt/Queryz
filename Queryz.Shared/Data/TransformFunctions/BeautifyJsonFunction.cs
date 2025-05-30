using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Queryz.Data.Entities;

namespace Queryz.Data.TransformFunctions;

public class BeautifyJsonFunction : ITransformFunction
{
    public string Name => "Beautify JSON";

    public dynamic Transform(dynamic value, Report report) => value is not string
        ? value
        : value == null ? value : !IsValidJson(value, out JToken obj) ? value : JsonConvert.SerializeObject(obj, Formatting.Indented);

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
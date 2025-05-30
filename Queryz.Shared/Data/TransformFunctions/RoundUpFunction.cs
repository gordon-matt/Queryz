using Queryz.Data.Entities;

namespace Queryz.Data.TransformFunctions;

public class RoundUpFunction : ITransformFunction
{
    public string Name => "Round Up";

    public dynamic Transform(dynamic value, Report report) => value is not float and not decimal and not double ? value : Math.Ceiling(value);
}
using Queryz.Data.Entities;

namespace Queryz.Data.TransformFunctions;

public interface ITransformFunction
{
    string Name { get; }

    dynamic Transform(dynamic value, Report report);
}
using Queryz.Data.Domain;

namespace Queryz.Data.TransformFunctions
{
    public interface ITransformFunction
    {
        string Name { get; }

        dynamic Transform(dynamic value, Report report);
    }
}
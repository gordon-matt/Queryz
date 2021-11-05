using System;
using Queryz.Data.Domain;

namespace Queryz.Data.TransformFunctions
{
    public class RoundUpFunction : ITransformFunction
    {
        public string Name => "Round Up";

        public dynamic Transform(dynamic value, Report report)
        {
            if (!(value is float) && !(value is decimal) && !(value is double))
            {
                return value;
            }

            return Math.Ceiling(value);
        }
    }
}
using System.Collections.Generic;

namespace Queryz.Models
{
    public interface IConnectionBuilderModel
    {
        string ToConnectionString();

        IDictionary<string, string> GetCustomProperties();
    }
}
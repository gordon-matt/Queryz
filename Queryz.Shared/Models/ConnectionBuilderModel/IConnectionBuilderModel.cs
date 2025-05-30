namespace Queryz.Models;

public interface IConnectionBuilderModel
{
    string ToConnectionString();

    IDictionary<string, string> GetCustomProperties();
}
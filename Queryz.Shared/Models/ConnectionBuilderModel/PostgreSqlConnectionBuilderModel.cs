namespace Queryz.Models;

public class PostgreSqlConnectionBuilderModel : IConnectionBuilderModel
{
    public PostgreSqlConnectionBuilderModel()
    {
        Database = "postgres";
    }

    public string Server { get; set; }

    public int Port { get; set; }

    public string Database { get; set; }

    public string UserId { get; set; }

    public string Password { get; set; }

    public string Schema { get; set; }

    public string ToConnectionString() => string.Format(
            "Server={0};port={1};Database={2};User Id={3};Password={4};CommandTimeout=60; Pooling=true;MinPoolSize=1;MaxPoolSize=100;",
            Server,
            Port,
            Database,
            UserId,
            Password);

    public IDictionary<string, string> GetCustomProperties() => new Dictionary<string, string>
        {
            { "Schema", Schema }
        };
}
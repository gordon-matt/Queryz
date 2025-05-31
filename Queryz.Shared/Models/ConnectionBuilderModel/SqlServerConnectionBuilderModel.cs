namespace Queryz.Models;

public class SqlServerConnectionBuilderModel : IConnectionBuilderModel
{
    public string Server { get; set; }

    public string Database { get; set; }

    public bool IntegratedSecurity { get; set; }

    public string UserId { get; set; }

    public string Password { get; set; }

    public string ToConnectionString() => IntegratedSecurity
        ? string.Format("Server={0};Database={1};Integrated Security=True;Connection Timeout=300;TrustServerCertificate=True", Server, Database)
        : string.Format("Server={0};Database={1};User Id={2};Password={3};Connection Timeout=300;TrustServerCertificate=True", Server, Database, UserId, Password);

    public IDictionary<string, string> GetCustomProperties() => new Dictionary<string, string>();
}
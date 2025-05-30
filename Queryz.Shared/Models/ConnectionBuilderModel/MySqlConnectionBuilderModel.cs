namespace Queryz.Models;

public class MySqlConnectionBuilderModel : IConnectionBuilderModel
{
    public string Server { get; set; }

    public uint Port { get; set; }

    public string Database { get; set; }

    public string UserId { get; set; }

    public string Password { get; set; }

    public string ToConnectionString() => string.Format(
            "server={0};port={1};database={2};uid={3};password={4}",
            Server,
            Port,
            Database,
            UserId,
            Password);

    public IDictionary<string, string> GetCustomProperties() => new Dictionary<string, string>();
}
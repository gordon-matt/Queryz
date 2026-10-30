using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using MySqlConnector;
using Npgsql;

namespace Queryz.Services.Reports;

public static class DataSourceConnections
{
    public static string Build(ReportProvider provider, DataSourceConnectionRequest connection)
    {
        switch (provider)
        {
            case ReportProvider.SqlServer:
            {
                var builder = new SqlConnectionStringBuilder
                {
                    DataSource = connection.Server.Trim(),
                    InitialCatalog = connection.Database.Trim(),
                    IntegratedSecurity = connection.IntegratedSecurity,
                    TrustServerCertificate = true,
                    ConnectTimeout = 300,
                };

                if (!connection.IntegratedSecurity)
                {
                    builder.UserID = connection.UserId.Trim();
                    builder.Password = connection.Password;
                }

                return builder.ConnectionString;
            }
            case ReportProvider.PostgreSql:
            {
                var builder = new NpgsqlConnectionStringBuilder
                {
                    Host = connection.Server.Trim(),
                    Port = connection.Port ?? 5432,
                    Database = connection.Database.Trim(),
                    Username = connection.UserId.Trim(),
                    Password = connection.Password,
                    CommandTimeout = 60,
                };

                if (!string.IsNullOrWhiteSpace(connection.Schema))
                {
                    builder.SearchPath = connection.Schema.Trim();
                }

                return builder.ConnectionString;
            }
            case ReportProvider.MySql:
            {
                var builder = new MySqlConnectionStringBuilder
                {
                    Server = connection.Server.Trim(),
                    Port = (uint)(connection.Port ?? 3306),
                    Database = connection.Database.Trim(),
                    UserID = connection.UserId.Trim(),
                    Password = connection.Password,
                };

                return builder.ConnectionString;
            }
            case ReportProvider.Sqlite:
            {
                var builder = new SqliteConnectionStringBuilder
                {
                    DataSource = connection.FilePath.Trim(),
                    Mode = SqliteOpenMode.ReadWriteCreate,
                };

                return builder.ConnectionString;
            }
            default:
                throw new InvalidOperationException($"Unsupported report provider '{provider}'.");
        }
    }

    public static string ForCatalog(ReportProvider provider, DataSourceConnectionRequest connection)
    {
        var copy = new DataSourceConnectionRequest
        {
            Server = connection.Server,
            Port = connection.Port,
            Database = connection.Database,
            Schema = connection.Schema,
            IntegratedSecurity = connection.IntegratedSecurity,
            UserId = connection.UserId,
            Password = connection.Password,
            FilePath = connection.FilePath,
        };

        switch (provider)
        {
            case ReportProvider.SqlServer:
                copy.Database = "master";
                break;
            case ReportProvider.PostgreSql:
                copy.Database = "postgres";
                break;
            case ReportProvider.MySql:
                copy.Database = string.Empty;
                break;
        }

        return Build(provider, copy);
    }

    public static DataSourceConnectionRequest Parse(ReportProvider provider, string connectionString)
    {
        var connection = new DataSourceConnectionRequest();

        switch (provider)
        {
            case ReportProvider.SqlServer:
            {
                var builder = new SqlConnectionStringBuilder(connectionString);
                connection.Server = builder.DataSource;
                connection.Database = builder.InitialCatalog;
                connection.IntegratedSecurity = builder.IntegratedSecurity;
                connection.UserId = builder.UserID;
                break;
            }
            case ReportProvider.PostgreSql:
            {
                var builder = new NpgsqlConnectionStringBuilder(connectionString);
                connection.Server = builder.Host ?? string.Empty;
                connection.Port = builder.Port;
                connection.Database = builder.Database ?? string.Empty;
                connection.UserId = builder.Username ?? string.Empty;
                connection.Schema = builder.SearchPath ?? string.Empty;
                break;
            }
            case ReportProvider.MySql:
            {
                var builder = new MySqlConnectionStringBuilder(connectionString);
                connection.Server = builder.Server;
                connection.Port = (int)builder.Port;
                connection.Database = builder.Database;
                connection.UserId = builder.UserID;
                break;
            }
            case ReportProvider.Sqlite:
            {
                var builder = new SqliteConnectionStringBuilder(connectionString);
                connection.FilePath = builder.DataSource;
                break;
            }
        }

        return connection;
    }

    public static bool HasPassword(ReportProvider provider, string connectionString)
    {
        return provider switch
        {
            ReportProvider.SqlServer => !string.IsNullOrEmpty(new SqlConnectionStringBuilder(connectionString).Password),
            ReportProvider.PostgreSql => !string.IsNullOrEmpty(new NpgsqlConnectionStringBuilder(connectionString).Password),
            ReportProvider.MySql => !string.IsNullOrEmpty(new MySqlConnectionStringBuilder(connectionString).Password),
            _ => false,
        };
    }

    public static string? ReadPassword(ReportProvider provider, string connectionString)
    {
        return provider switch
        {
            ReportProvider.SqlServer => new SqlConnectionStringBuilder(connectionString).Password,
            ReportProvider.PostgreSql => new NpgsqlConnectionStringBuilder(connectionString).Password,
            ReportProvider.MySql => new MySqlConnectionStringBuilder(connectionString).Password,
            _ => null,
        };
    }
}

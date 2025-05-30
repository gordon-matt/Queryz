using System.Data.Common;
using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
using Npgsql;
using Queryz.Data.Entities;
using Queryz.Models;

namespace Queryz.Extensions;

public static class DataProviderExtensions
{
    public static DbConnection GetConnection(this DataProvider provider, string connectionString) => provider switch
    {
        DataProvider.SqlServer => new SqlConnection(connectionString),
        DataProvider.PostgreSql => new NpgsqlConnection(connectionString),
        DataProvider.MySql => new MySqlConnection(connectionString),
        _ => throw new NotSupportedException(),
    };

    public static string GetConnectionString(this DataProvider provider, string connectionDetails)
    {
        IConnectionBuilderModel model = provider switch
        {
            DataProvider.SqlServer => connectionDetails.JsonDeserialize<SqlServerConnectionBuilderModel>(),
            DataProvider.PostgreSql => connectionDetails.JsonDeserialize<PostgreSqlConnectionBuilderModel>(),
            DataProvider.MySql => connectionDetails.JsonDeserialize<MySqlConnectionBuilderModel>(),
            _ => throw new NotSupportedException(),
        };
        return model.ToConnectionString();
    }

    public static string GetConnectionString(this DataProvider provider, string connectionDetails, out IDictionary<string, string> customProperties)
    {
        IConnectionBuilderModel model = provider switch
        {
            DataProvider.SqlServer => connectionDetails.JsonDeserialize<SqlServerConnectionBuilderModel>(),
            DataProvider.PostgreSql => connectionDetails.JsonDeserialize<PostgreSqlConnectionBuilderModel>(),
            DataProvider.MySql => connectionDetails.JsonDeserialize<MySqlConnectionBuilderModel>(),
            _ => throw new NotSupportedException(),
        };
        customProperties = model.GetCustomProperties();

        return model.ToConnectionString();
    }
}
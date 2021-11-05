using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using Extenso;
using MySql.Data.MySqlClient;
using Npgsql;
using Queryz.Data.Domain;
using Queryz.Models;

namespace Queryz.Extensions
{
    public static class DataProviderExtensions
    {
        public static DbConnection GetConnection(this DataProvider provider, string connectionString)
        {
            switch (provider)
            {
                case DataProvider.SqlServer: return new SqlConnection(connectionString);
                case DataProvider.PostgreSql: return new NpgsqlConnection(connectionString);
                case DataProvider.MySql: return new MySqlConnection(connectionString);
                default: throw new NotSupportedException();
            }
        }

        public static string GetConnectionString(this DataProvider provider, string connectionDetails)
        {
            IConnectionBuilderModel model;
            switch (provider)
            {
                case DataProvider.SqlServer: model = connectionDetails.JsonDeserialize<SqlServerConnectionBuilderModel>(); break;
                case DataProvider.PostgreSql: model = connectionDetails.JsonDeserialize<PostgreSqlConnectionBuilderModel>(); break;
                case DataProvider.MySql: model = connectionDetails.JsonDeserialize<MySqlConnectionBuilderModel>(); break;
                default: throw new NotSupportedException();
            }

            return model.ToConnectionString();
        }

        public static string GetConnectionString(this DataProvider provider, string connectionDetails, out IDictionary<string, string> customProperties)
        {
            IConnectionBuilderModel model;
            switch (provider)
            {
                case DataProvider.SqlServer: model = connectionDetails.JsonDeserialize<SqlServerConnectionBuilderModel>(); break;
                case DataProvider.PostgreSql: model = connectionDetails.JsonDeserialize<PostgreSqlConnectionBuilderModel>(); break;
                case DataProvider.MySql: model = connectionDetails.JsonDeserialize<MySqlConnectionBuilderModel>(); break;
                default: throw new NotSupportedException();
            }

            customProperties = model.GetCustomProperties();

            return model.ToConnectionString();
        }
    }
}
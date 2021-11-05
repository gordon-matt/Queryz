using System;
using System.Collections.Generic;
using Extenso;
using Extenso.Collections;
using Extenso.Data.QueryBuilder;
using Extenso.Data.QueryBuilder.MySql;
using Extenso.Data.QueryBuilder.Npgsql;
using Queryz.Data.Domain;

namespace Queryz.Extensions
{
    public static class DataSourceExtensions
    {
        public static ISelectQueryBuilder GetSelectQueryBuilder(this DataSource dataSource)
        {
            switch (dataSource.DataProvider)
            {
                case DataProvider.SqlServer: return new SqlServerSelectQueryBuilder();
                case DataProvider.PostgreSql:
                    {
                        var customProperties = dataSource.SafeGetCustomProperties();
                        return new NpgsqlSelectQueryBuilder(customProperties["Schema"]);
                    }
                case DataProvider.MySql: return new MySqlSelectQueryBuilder();
                default: throw new NotSupportedException();
            }
        }

        public static Dictionary<string, string> SafeGetCustomProperties(this DataSource dataSource)
        {
            return dataSource.CustomProperties.IsNullOrEmpty()
                ? dataSource.DataProvider == DataProvider.PostgreSql
                    ? new Dictionary<string, string> { { "Schema", "public" } }
                    : new Dictionary<string, string>()
                : dataSource.CustomProperties.JsonDeserialize<Dictionary<string, string>>();
        }
    }
}
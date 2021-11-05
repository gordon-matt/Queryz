using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using Extenso.Data;
using Extenso.Data.MySql;
using Extenso.Data.Npgsql;
using Extenso.Data.SqlClient;
using MySql.Data.MySqlClient;
using Npgsql;
using Queryz.Data.Domain;

namespace Queryz.Extensions
{
    public static class DbConnectionHelpers
    {
        public static ColumnInfoCollection GetColumnData(DbConnection connection, DataSource dataSource, string tableName)
        {
            if (connection is SqlConnection)
            {
                return (connection as SqlConnection).GetColumnData(tableName);
            }
            if (connection is NpgsqlConnection)
            {
                var customProperties = dataSource.SafeGetCustomProperties();
                return (connection as NpgsqlConnection).GetColumnData(tableName, customProperties["Schema"]);
            }
            if (connection is MySqlConnection)
            {
                return (connection as MySqlConnection).GetColumnData(tableName);
            }

            throw new NotSupportedException();
        }

        public static IEnumerable<string> GetDatabaseNames(DbConnection connection)
        {
            if (connection is SqlConnection)
            {
                return (connection as SqlConnection).GetDatabaseNames();
            }
            if (connection is NpgsqlConnection)
            {
                return (connection as NpgsqlConnection).GetDatabaseNames();
            }
            if (connection is MySqlConnection)
            {
                return (connection as MySqlConnection).GetDatabaseNames();
            }

            throw new NotSupportedException();
        }

        public static ForeignKeyInfoCollection GetForeignKeyData(DbConnection connection, DataSource dataSource, string tableName)
        {
            if (connection is SqlConnection)
            {
                return (connection as SqlConnection).GetForeignKeyData(tableName);
            }
            if (connection is NpgsqlConnection)
            {
                var customProperties = dataSource.SafeGetCustomProperties();
                return (connection as NpgsqlConnection).GetForeignKeyData(tableName, customProperties["Schema"]);
            }
            if (connection is MySqlConnection)
            {
                return (connection as MySqlConnection).GetForeignKeyData(tableName);
            }

            throw new NotSupportedException();
        }

        public static IEnumerable<string> GetTableNames(DbConnection connection, DataSource dataSource, bool includeViews = false)
        {
            if (connection is SqlConnection)
            {
                return (connection as SqlConnection).GetTableNames(includeViews);
            }
            if (connection is NpgsqlConnection)
            {
                var customProperties = dataSource.SafeGetCustomProperties();
                return (connection as NpgsqlConnection).GetTableNames(includeViews, customProperties["Schema"]);
            }
            if (connection is MySqlConnection)
            {
                return (connection as MySqlConnection).GetTableNames(includeViews);
            }

            throw new NotSupportedException();
        }
    }
}
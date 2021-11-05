using System;
using System.Data;

namespace Queryz.Extensions
{
    public static class DataColumnExtensions
    {
        public static void ChangeDataType<TFrom, TTo>(this DataColumn column, Func<TFrom, TTo> convert)
        {
            string temp = "_1_TEMP_1_";

            var newColumn = new DataColumn(column.ColumnName + temp, typeof(TTo));
            column.Table.Columns.Add(newColumn);

            foreach (DataRow row in column.Table.Rows)
            {
                var value = row.Field<TFrom>(column);
                row[newColumn] = convert(value);
            }

            //Delete original column
            int ordinal = column.Ordinal;
            column.Table.Columns.Remove(column);
            newColumn.ColumnName = newColumn.ColumnName.Replace(temp, string.Empty);
            newColumn.SetOrdinal(ordinal);
        }
    }
}
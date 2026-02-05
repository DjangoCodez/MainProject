using System;
using System.Data;

namespace SoftOne.Soe.Common.Util
{
    public static class ExcelUtil
    {
        #region DataRow/DataColumn

        public static object GetColumn(DataRow row, Enum column)
        {
            return GetColumn(row, column.ToString());
        }

        public static object GetColumn(DataRow row, string columnName)
        {
            object column = null;
            if (row != null && row.Table.Columns != null && row.Table.Columns.Contains(columnName))
                column = row[columnName];
            return column;
        }

        public static object GetColumnValueByInt(DataRow row, int column)
        {
            return row[column];
        }

        public static object GetColumnValue(DataRow row, Enum column)
        {
            return GetColumnValue(row, column.ToString());
        }

        public static object GetColumnValue(System.Data.DataRow row, string columnName)
        {
            object column = GetColumn(row, columnName);
            if (!HasValue(column))
                return null;

            return column;
        }

        public static bool HasValue(object source)
        {
            return source != null && !String.IsNullOrEmpty(source.ToString().Trim());
        }

        #endregion
    }
}

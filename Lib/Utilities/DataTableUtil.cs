using Lib.Utilities;
using System.Data;
using System.Reflection;

namespace Lib.Utilities
{
    public static class DataTableExUtil
    {
        public static List<T> ToList<T>(this DataTable table) where T : new()
        {
            List<PropertyInfo> properties = typeof(T).GetProperties().Where(p => table.Columns.Contains(p.Name)).ToList();
            List<T> result = new List<T>();

            foreach (var row in table.Rows)
            {
                var item = CreateItemFromRow<T>((DataRow)row, properties);
                result.Add(item);
            }

            return result;
        }

        public static List<T> ToList<T>(this DataTable table, Dictionary<string, string> mappings) where T : new()
        {
            List<PropertyInfo> properties = typeof(T).GetProperties().ToList();
            List<T> result = new List<T>();

            foreach (var row in table.Rows)
            {
                var item = CreateItemFromRow<T>((DataRow)row, properties, mappings);
                result.Add(item);
            }

            return result;
        }

        private static T CreateItemFromRow<T>(DataRow row, IList<PropertyInfo> properties) where T : new()
        {
            T item = new T();
            foreach (var property in properties)
            {
                property.SetValue(item, row[property.Name], null);
            }
            return item;
        }

        private static T CreateItemFromRow<T>(DataRow row, IList<PropertyInfo> properties, Dictionary<string, string> mappings) where T : new()
        {
            T item = new T();
            foreach (var property in properties)
            {
                if (mappings.ContainsKey(property.Name))
                    property.SetValue(item, row[mappings[property.Name]], null);
            }
            return item;
        }

        /// <summary>
        /// 字串處理
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="trimType"></param>
        /// <param name="nullToEmpty"></param>
        /// <returns></returns>
        public static DataTable? StrProcess(this DataTable? dt,
            TrimType trimType = TrimType.TrimEnd,
            bool nullToEmpty = true)
        {
            try
            {
                if (dt == null) return dt;
                foreach (DataRow row in dt.Rows)
                    foreach (DataColumn dc in dt.Columns)
                    {
                        if (dc.DataType != typeof(string)) continue;
                        //dc.ReadOnly = false;

                        // DataTable 欄位值即使 Assign null 仍轉為 DBNull 
                        if (!(row[dc.ColumnName] == DBNull.Value))
                        {
                            if (trimType == TrimType.Trim)
                                row[dc.ColumnName] = row[dc.ColumnName].ToString()?.Trim();
                            else if (trimType == TrimType.TrimEnd)
                                row[dc.ColumnName] = row[dc.ColumnName].ToString()?.TrimEnd();
                            else if (trimType == TrimType.TrimStart)
                                row[dc.ColumnName] = row[dc.ColumnName].ToString()?.TrimStart();
                        }

                        // DBNull.Value.ToString() == string.Empty
                        if (nullToEmpty) row[dc.ColumnName] = row[dc.ColumnName].ToString();
                    }
                dt.AcceptChanges();
            }
            catch (Exception) { }
            return dt;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.EdiAdmin.Business.Util
{
    public static class ExtensionMethods
    {
        public static bool IsNull<T>(this T obj) where T : class
        {
            return obj == null;
        }

        public static J NotNull<T, J>(this T obj, Func<T, J> selector) where T : class where J: class
        {
            return obj != null ? selector(obj) : null;
        }

        public static string SubstringBetween(this string text, string startText, string untilNext, bool includeStartText)
        {
            int startIndex = includeStartText ? text.IndexOf(startText) : text.IndexOf(startText) - startText.Length;
            int endIndex = text.Substring(startIndex).IndexOf(untilNext);

            return text.Substring(startIndex, endIndex);
        }

        public static DataRow ToDataRow(this EntityObject entity)
        {
            var table = ToDataTable(new[] { entity });
            return table.Rows[0];
        }

        public static DataTable ToDataTable(this IEnumerable<EntityObject> entities)
        {
            var entityType = entities.FirstOrDefault().GetType();
            DataTable table = new DataTable(entityType.Name);

            foreach (PropertyInfo info in entityType.GetProperties())
            {
                table.Columns.Add(info.Name, info.PropertyType);
            }
            
            table.AcceptChanges();

            foreach (var item in entities)
            {
                DataRow row = table.NewRow();

                foreach (var property in entityType.GetProperties())
                {
                    row[property.Name] = property.GetValue(item);
                }

                table.Rows.Add(row);
            }

            table.AcceptChanges();

            return table;
        }
    }
}

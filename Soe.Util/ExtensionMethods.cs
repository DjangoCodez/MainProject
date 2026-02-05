using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SoftOne.Soe.Util
{
    public static class ExtensionMethods
    {
        #region String

        public static string SubstringFromEnd(this string source, int tail_length)
        {
            if (tail_length >= source.Length)
                return source;
            return source.Substring(source.Length - tail_length);
        }

        public static Dictionary<T, string> Split<T>(this string s, char delimiter)
        {
            var array = s.Split(delimiter);
            var dict = new Dictionary<T, string>(array.Length);

            foreach (var item in Enum.GetValues(typeof(T)))
            {
                var pos = (int)item;
                if (pos > 0 && pos < array.Length)
                    dict.Add((T)item, array[(int)item]);
            }

            return dict;
        }
        public static string CleanKeyString(this string value)
        {
            value = value.Trim();
            value = value.Replace("-", "");
            value = value.Replace(" ", "");
            value = value.Replace("_", "");
            value = value.Replace(".", "");
            value = value.ToLower();

            return value;
        }

        #endregion

        #region Date

        public static string ToShortDateShortTimeString(this DateTime date)
        {
            return String.Format("{0} {1}", date.ToShortDateString(), date.ToShortTimeString());
        }

        public static string ToShortDateShortTimeStringT(this DateTime date)
        {
            return String.Format("{0}T{1}", date.ToShortDateString(), date.ToShortTimeString());
        }

        public static string ToShortDateLongTimeString(this DateTime date)
        {
            return String.Format("{0} {1}", date.ToShortDateString(), date.ToLongTimeString());
        }
        public static bool IsWeekendDay(this DateTime date)
        {
            return (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday);
        }
        public static bool IsBefore(this DateTime date, DateTime compareDate)
        {
            return date < compareDate;
        }
        public static bool IsAfter(this DateTime date, DateTime compareDate)
        {
            return date > compareDate;
        }

        #endregion

        #region Decimal

        public static decimal GetValue(this decimal? value, decimal defaultValue = 0)
        {
            return value.HasValue ? value.Value : defaultValue;
        }

        #endregion

        #region TimeSpan

        public static string ToShortTimeString(this TimeSpan span)
        {
            int hours = Convert.ToInt32(Math.Floor(span.TotalHours));

            DateTime date = DateTime.Today;
            date = date.AddHours(hours);
            date = date.AddMinutes(span.Minutes);

            return date.ToShortTimeString();
        }

        public static string ToLongTimeString(this TimeSpan span)
        {
            int hours = Convert.ToInt32(Math.Floor(span.TotalHours));

            DateTime date = DateTime.Today;
            date = date.AddHours(hours);
            date = date.AddMinutes(span.Minutes);

            return date.ToLongTimeString();
        }

        #endregion

        #region Linq

        public delegate void Updater<TSource>(TSource updater);

        /// <summary>
        /// Used to modify properties of an object returned from a LINQ query
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="input"></param>
        /// <param name="updater"></param>
        /// <returns></returns>
        public static TSource Set<TSource>(this TSource input, Updater<TSource> updater)
        {
            updater(input);
            return input;
        }

        /// <summary>
        /// Warning! Dont use this extension if the values collection is very large (>50). Can cause the IIS to crash
        /// </summary>
        /// <typeparam name="TElement"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="valueSelector"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static Expression<Func<TElement, bool>> BuildContainsExpression<TElement, TValue>(Expression<Func<TElement, TValue>> valueSelector, IEnumerable<TValue> values)
        {
            ParameterExpression p = valueSelector.Parameters.Single();
            if (!values.Any())
            {
                return e => false;
            }

            var equals = values.Select(value => (Expression)Expression.Equal(valueSelector.Body, Expression.Constant(value, typeof(TValue))));
            var body = equals.Aggregate<Expression>((accumulate, equal) => Expression.Or(accumulate, equal));
            return Expression.Lambda<Func<TElement, bool>>(body, p);
        }

        public static IEnumerable<T> AppendElement<T>(this IEnumerable<T> collection, T element)
        {
            return collection.Concat(Enumerable.Repeat(element, 1));
        }

        public static IEnumerable<T> PrependElement<T>(this IEnumerable<T> collection, T element)
        {
            return Enumerable.Repeat(element, 1).Concat(collection);
        }

        #endregion

        #region List

        /// <summary>
        /// Removes all elements that occurs in the provided selection of items
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">List to remove from</param>
        /// <param name="items">Items to remove</param>
        /// <returns>The number of items that were removed</returns>
        public static int RemoveItems<T>(this List<T> list, IEnumerable<T> items)
        {
            return list.RemoveAll(x => items.Contains(x));
        }

        #endregion

        #region IEnumerable

        public static void AddRange<T>(this EntityCollection<T> collection, IEnumerable<T> items)
            where T : class
        {
            foreach (T item in items)
            {
                collection.Add(item);
            }
        }

        #endregion

        #region IQueryable

        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string property)
        {
            return ApplyOrder<T>(source, property, "OrderBy");
        }

        public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> source, string property)
        {
            return ApplyOrder<T>(source, property, "OrderByDescending");
        }

        public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> source, string property)
        {
            return ApplyOrder<T>(source, property, "ThenBy");
        }

        public static IOrderedQueryable<T> ThenByDescending<T>(this IOrderedQueryable<T> source, string property)
        {
            return ApplyOrder<T>(source, property, "ThenByDescending");
        }

        public static IOrderedQueryable<T> ApplyOrder<T>(IQueryable<T> source, string property, string methodName)
        {
            string[] props = property.Split('.');
            Type type = typeof(T);
            ParameterExpression arg = Expression.Parameter(type, "x");
            Expression expr = arg;
            foreach (string prop in props)
            {
                // use reflection (not ComponentModel) to mirror LINQ
                PropertyInfo pi = type.GetProperty(prop);
                expr = Expression.Property(expr, pi);
                type = pi.PropertyType;
            }
            Type delegateType = typeof(Func<,>).MakeGenericType(typeof(T), type);
            LambdaExpression lambda = Expression.Lambda(delegateType, expr, arg);

            object result = typeof(Queryable).GetMethods().Single(
                    method => method.Name == methodName
                            && method.IsGenericMethodDefinition
                            && method.GetGenericArguments().Length == 2
                            && method.GetParameters().Length == 2)
                    .MakeGenericMethod(typeof(T), type)
                    .Invoke(null, new object[] { source, lambda });
            return (IOrderedQueryable<T>)result;
        }

        #endregion

        #region NameValueCollection

        public static Dictionary<string, string> ConvertToDict(this NameValueCollection coll)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            return coll.Cast<string>()
                     .Select(s => new { Key = s, Value = coll[s] })
                     .ToDictionary(p => p.Key, p => p.Value);
        }

        public static List<KeyValuePair<string, string>> ToList(this NameValueCollection nameValueCollection)
        {
            if(nameValueCollection == null)
                return Enumerable.Empty<KeyValuePair<string, string>>().ToList();

            return nameValueCollection.AllKeys.SelectMany(nameValueCollection.GetValues, (key, value) => new KeyValuePair<string, string>(key, value)).ToList();
        }
        #endregion

    }
}

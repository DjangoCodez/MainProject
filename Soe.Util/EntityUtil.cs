using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Data.Entity.Core.Objects.DataClasses;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace SoftOne.Soe.Util
{
    public static class EntityUtil
    {
        #region Generic

        public static T Clone<T>(T prototype)
        {
            DataContractSerializer dcSer = new DataContractSerializer(prototype.GetType());
            MemoryStream memoryStream = new MemoryStream();

            dcSer.WriteObject(memoryStream, prototype);
            memoryStream.Position = 0;

            return (T)dcSer.ReadObject(memoryStream);
        }

        public static void Copy<T>(T clone, T prototype, bool includeKeys = false)
        {
            //Only Properties of value types (not NavigationProperties and EntityKeys)
            var properties = from p in typeof(T).GetProperties()
                             where (p.CanWrite) &&
                             (p.MemberType == MemberTypes.Property) &&
                             (p.PropertyType.IsValueType || p.PropertyType.Name == "String") &&
                             ((from a in p.GetCustomAttributes(false)
                               where a is EdmScalarPropertyAttribute &&
                               (!((EdmScalarPropertyAttribute)a).EntityKeyProperty || includeKeys)
                               select true).FirstOrDefault())
                             select p;

            foreach (var property in properties)
            {
                if (property.Name == "Created" || property.Name == "CreatedBy" || property.Name == "Modified" || property.Name == "ModifiedBy")
                    continue;

                PropertyInfo prototypePi = prototype.GetType().GetProperty(property.Name);
                PropertyInfo clonePi = prototype.GetType().GetProperty(property.Name);
                if (prototypePi != null && clonePi != null)
                    clonePi.SetValue(clone, prototypePi.GetValue(prototype, null), null);
            }
        }

        public static void CopyDTO<T>(T clone, T prototype)
        {
            //Only Properties of value types (not NavigationProperties and EntityKeys)
            var properties = from p in typeof(T).GetProperties()
                             where p.CanWrite
                             select p;

            foreach (var property in properties)
            {
                if (property.Name == "Created" || property.Name == "CreatedBy" || property.Name == "Modified" || property.Name == "ModifiedBy")
                    continue;

                PropertyInfo prototypePi = prototype.GetType().GetProperty(property.Name);
                PropertyInfo clonePi = prototype.GetType().GetProperty(property.Name);
                if (prototypePi != null && clonePi != null)
                    clonePi.SetValue(clone, prototypePi.GetValue(prototype, null), null);
            }
        }

        public static string GetPropertyValue<T>(T entity, string propertyName)
        {
            object value = null;

            try
            {
                PropertyInfo property = entity.GetType().GetProperty(propertyName);
                if (property != null)
                {
                    Type type = property.PropertyType;
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                        type = type.GetGenericArguments()[0];

                    value = property.GetValue(entity, null);
                    if (value != null)
                    {
                        if (type.Name == "DateTime")
                            value = DateTime.Parse(value.ToString()).ToShortDateString();
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }

            return value != null ? value.ToString() : String.Empty;
        }

        public static string ParseEntityTaggedText<T>(T entity, string text)
        {
            try
            {
                Regex regex = new Regex("\\[([^\\s]*)\\]");
                string propertyName = null;
                string value = null;

                MatchCollection matches = regex.Matches(text);
                foreach (Match match in matches)
                {
                    propertyName = Regex.Replace(match.Value, "\\[([^\\s]*)\\]", "$1");
                    value = !String.IsNullOrEmpty(propertyName) ? GetPropertyValue<T>(entity, propertyName) : String.Empty;
                    text = Regex.Replace(text, "\\[(" + propertyName + ")\\]", value);
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }

            return text;
        }

        private static readonly ConcurrentDictionary<Type,bool> _isEfCache = new ConcurrentDictionary<Type,bool>();

        public static bool IsEntityFrameworkClass<T>(T value)
        {
            if (value == null) return false;
            var type = value.GetType();
            return _isEfCache.GetOrAdd(type, t =>
            {
                // If it's a collection (and not string) check the element type first.
                // Generic collections like List<T> have namespaces under System.* and would fail a namespace-only check.
                if (typeof(System.Collections.IEnumerable).IsAssignableFrom(t) && t != typeof(string))
                {
                    var el = GetEnumerableElementType(t);   
                    return el != null && IsEntityObjectType(el);
                }

                var ns = t.Namespace ?? string.Empty;
                if (!(ns.IndexOf("compentities", StringComparison.OrdinalIgnoreCase) >= 0
                    || ns.IndexOf("sysentities", StringComparison.OrdinalIgnoreCase) >= 0
                    || ns.IndexOf("dynamicproxies", StringComparison.OrdinalIgnoreCase) >= 0
                    || ns.IndexOf("softone.soe.data", StringComparison.OrdinalIgnoreCase) >= 0))
                    return false;

                return IsEntityObjectType(t);
            });
        }

        private static Type GetEnumerableElementType(Type collectionType)
        {
            if (collectionType == null) return null;

            if (collectionType.IsArray)
                return collectionType.GetElementType();

            if (collectionType.IsGenericType)
            {
                var args = collectionType.GetGenericArguments();
                if (args.Length == 1)
                    return args[0];
            }

            // Look for IEnumerable<T> on interfaces
            var ie = collectionType.GetInterfaces()
                                   .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
            if (ie != null)
                return ie.GetGenericArguments()[0];

            return null;
        }

        private static bool IsEntityObjectType(Type t)
        {
            if (t == null) return false;

            // Walk base types to see if any is EntityObject
            var cur = t;
            while (cur != null)
            {
                if (cur == typeof(EntityObject) ||
                    cur.Name == "SysEntity")
                    return true;
                cur = cur.BaseType;
            }

            return false;
        }
    }

    #endregion
}


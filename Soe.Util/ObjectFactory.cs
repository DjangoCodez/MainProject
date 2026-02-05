using System;
using System.Reflection;

namespace SoftOne.Soe.Util
{
    public sealed class ObjectFactory
    {
        private ObjectFactory() { }

        public static object Create(string assemblyName, string className)
        {
            // Resolve the type
            Type targetType = ResolveType(assemblyName, className);
            if (targetType == null)
                throw new ArgumentException(String.Format("Can't load type {0}, {1}", assemblyName, className));

            // Get the default constructor and instantiate
            Type[] types = new Type[0];
            ConstructorInfo info = targetType.GetConstructor(types);
            object targetObject = info.Invoke(null);
            if (targetObject == null)
                throw new ArgumentException(String.Format("Can't instantiate type {0}, {1}", assemblyName, className));

            return targetObject;
        }

        public static Type ResolveType(string assemblyName, string className)
        {
            // Remove file extension
            if (assemblyName.EndsWith(".dll"))
                assemblyName = assemblyName.Substring(0, assemblyName.Length - 4);

            // Get the assembly containing the handler
            Assembly assembly = null;
            try
            {
                assembly = Assembly.Load(assemblyName);
            }
            catch (Exception ex)
            {
                try
                {
                    ex.ToString(); //prevent compiler warning
                    assembly = Assembly.LoadFrom(assemblyName);
                }
                catch (Exception ex2)
                {
                    ex2.ToString(); //prevent compiler warning
                    throw new ArgumentException("Can't load assembly " + assemblyName, ex2);
                }
            }

            // Get the handler
            if (assembly != null)
            {
                try
                {
                    return assembly.GetType(className, true, false);
                }
                catch (Exception ex3)
                {
                    throw new ArgumentException("Can't load assembly " + assemblyName, ex3);
                }
            }
            else
                throw new ArgumentException("Can't load assembly " + assemblyName);
        }
    }
}

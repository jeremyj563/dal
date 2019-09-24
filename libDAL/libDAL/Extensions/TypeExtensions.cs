using System;
using System.Reflection;

namespace libDAL.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="Type"/>
    /// </summary>
    public static class TypeExtensions
    {
        public static PropertyInfo[] GetPublicInstanceProperties(this Type source)
        {
            return source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        }
    }
}

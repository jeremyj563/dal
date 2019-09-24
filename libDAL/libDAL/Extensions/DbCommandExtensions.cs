using System;
using System.Data.Common;
using System.Reflection;

namespace libDAL.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="DbCommand"/>
    /// </summary>
    public static class DbCommandExtensions
    {
        public static void AddParameters<TSchema>(this DbCommand command, (string, object)[] @params = null, TSchema schema = default)
        {
            if (@params != null)
            {
                foreach ((string key, object value) param in @params)
                {
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = param.key;
                    parameter.Value = param.value;

                    command.Parameters.Add(parameter);
                }
            }

            if (schema != null)
            {
                foreach (PropertyInfo property in typeof(TSchema).GetPublicInstanceProperties())
                {
                    if (property.CanRead)
                    {
                        var value = property.GetValue(schema, null);
                        if (value == null) value = DBNull.Value;

                        var parameter = command.CreateParameter();
                        parameter.ParameterName = property.Name;
                        parameter.Value = value;
                        command.Parameters.Add(parameter);
                    }
                }
            }
        }
    }
}

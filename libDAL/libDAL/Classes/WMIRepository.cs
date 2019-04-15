using System;
using System.Collections.Generic;

namespace DataRepositories.Classes
{
    public class WMIRepository //: IDataRepository
    {
        public WMIRepository()
        {

        }

        #region External Interface

        public void New<T>(string cmd, T record) where T : new()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> Get<T>(string cmd, (string, object)[] @params = null) where T : new()
        {
            return null;
        }

        public void Edit<T>(string cmd, T record) where T : new()
        {
            throw new NotImplementedException();
        }

        public void Remove<T>(string cmd, T record) where T : new()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Internal Methods

        protected internal enum WMIVerb
        {
            SELECT
        }

        //protected internal IEnumerable<TRecord> Query<TConnection, TCommand, TRecord>(WMIVerb verb, string cmd, (string, object)[] @params)
        //    where TConnection : DbConnection, new()
        //    where TCommand : DbCommand, new()
        //    where TRecord : new()
        //{
        //    // A SELECT command should not require a new record instance so a generic data type is required for parameters
        //    return Execute<TConnection, TCommand, TRecord>(verb, cmd, @params: @params);
        //}

        //protected internal void NonQuery<TConnection, TCommand, TRecord>(WMIVerb verb, string cmd, TRecord record)
        //    where TConnection : DbConnection, new()
        //    where TCommand : DbCommand, new()
        //    where TRecord : new()
        //{
        //    // For all other commands a record instance should be provided and will be used to populate parameters
        //    Execute<TConnection, TCommand, TRecord>(verb, cmd, record: record);
        //}

        //protected internal IEnumerable<TRecord> Execute<TConnection, TCommand, TRecord>(WMIVerb verb, string cmd, (string, object)[] @params = null, TRecord record = default(TRecord))
        //    where TConnection : DbConnection, new()
        //    where TCommand : DbCommand, new()
        //    where TRecord : new()
        //{
        //    // Check to make sure the called method matches the SQL verb used in the command text
        //    string verbName = Enum.GetName(typeof(WMIVerb), verb);
        //    if (cmd.ToUpper().StartsWith(verbName))
        //    {
        //        try
        //        {
        //            using (var connection = new TConnection())
        //            {
        //                connection.ConnectionString = this.ConnectionString;
        //                connection.Open();

        //                using (var command = new TCommand())
        //                {
        //                    command.CommandText = cmd;
        //                    command.Connection = connection;

        //                    if (verb == WMIVerb.SELECT)
        //                    {
        //                        // SELECT verb used so return a collection of records
        //                        AddParameters<TRecord>(command, @params: @params);
        //                        var records = new List<TRecord>();

        //                        PropertyInfo[] properties = GetProperties<TRecord>();
        //                        DbDataReader reader = command.ExecuteReader();

        //                        while (reader.Read())
        //                            records.Add(NewInstance<TRecord>(properties, reader));

        //                        return records;
        //                    }
        //                    else
        //                    {
        //                        // For all other verbs return null
        //                        AddParameters<TRecord>(command, record: record);
        //                        command.ExecuteNonQuery();
        //                    }
        //                }
        //            }
        //        }
        //        catch (Exception)
        //        {
        //            // TODO
        //        }
        //    }
        //    else
        //    {
        //        throw new InvalidCimCommandException(string.Format(ExceptionMessage, MethodBase.GetCurrentMethod().Name, verbName));
        //    }

        //    return null;
        //}

        //protected internal void AddParameters<TRecord>(DbCommand command, TRecord record = default(TRecord), (string, object)[] @params = null)
        //{
        //    // Parameters come from @params when the SELECT verb is used
        //    if (@params != null)
        //    {
        //        foreach ((string key, object value) param in @params)
        //        {
        //            var parameter = command.CreateParameter();
        //            parameter.ParameterName = param.key;
        //            parameter.Value = param.value;

        //            command.Parameters.Add(parameter);
        //        }
        //    }

        //    // Parameters come from record for all other verbs
        //    if (record != null)
        //    {
        //        foreach (PropertyInfo property in GetProperties<TRecord>())
        //        {
        //            if (property.CanRead)
        //            {
        //                var value = property.GetValue(record);
        //                if (value != null)
        //                {
        //                    var parameter = command.CreateParameter();
        //                    parameter.ParameterName = property.Name;
        //                    parameter.Value = value;

        //                    command.Parameters.Add(parameter);
        //                }
        //            }
        //        }
        //    }
        //}

        //protected internal TRecord NewInstance<TRecord>(PropertyInfo[] properties, DbDataReader reader) where TRecord : new()
        //{
        //    var instance = new TRecord();

        //    foreach (PropertyInfo property in properties)
        //    {
        //        if (property.CanWrite)
        //        {
        //            if (!reader.IsDBNull(reader.GetOrdinal(property.Name)))
        //            {
        //                var value = reader[property.Name];
        //                if (value.GetType().Equals(typeof(string)))
        //                    value = value.ToString().Trim();

        //                property.SetValue(instance, value);
        //            }
        //        }
        //    }

        //    return instance;
        //}

        //protected internal PropertyInfo[] GetProperties<TRecord>()
        //{
        //    return typeof(TRecord).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        //}

        #endregion

        #region Exceptions

        protected internal class InvalidCimCommandException : Exception
        {
            internal InvalidCimCommandException(string message) : base(message)
            {
            }
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Design.PluralizationServices;
using System.Globalization;
using System.Reflection;
using DataRepositories.Interfaces;

namespace DataRepositories
{
    public abstract class BaseSQLRepository : IDataRepository
    {

        protected internal string ConnectionString { get; }
        protected internal string IDCommand { get; }
        protected internal DbConnection Connection { get; set; }

        protected internal BaseSQLRepository(string connectionString, string idCommand)
        {
            this.ConnectionString = connectionString;
            this.IDCommand = idCommand;
        }

        #region External Interface

        public abstract int New<T>(string cmd, T record) where T : new();
        public abstract void New<T>(IEnumerable<T> records, string tableName) where T : new();
        public abstract IEnumerable<T> Get<T>(string cmd, (string, object)[] @params = null) where T : new();
        public abstract int Edit<T>(string cmd, T record, (string, object)[] @params = null) where T : new();
        public abstract int Remove<T>(string cmd, T record, (string, object)[] @params = null) where T : new();

        #endregion

        #region Internal Methods

        protected internal IEnumerable<TRecord> Query<TConnection, TCommand, TRecord>(string cmd, (string, object)[] @params)
            where TConnection : DbConnection, new()
            where TCommand : DbCommand, new()
            where TRecord : new()
        {
            var records = new List<TRecord>();

            try
            {
                using (var command = NewCommand<TConnection, TCommand>(cmd))
                {
                    AddParameters<TRecord>(command, @params: @params);

                    PropertyInfo[] properties = GetProperties<TRecord>();
                    DbDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                        records.Add(NewInstance<TRecord>(properties, reader));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                this.Connection.Close();
            }

            return records;
        }

        protected internal int NonQuery<TConnection, TCommand, TRecord>(string cmd, TRecord record, (string, object)[] @params)
            where TConnection : DbConnection, new()
            where TCommand : DbCommand, new()
            where TRecord : new()
        {
            int result = default(int);

            try
            {
                using (var command = NewCommand<TConnection, TCommand>(cmd + this.IDCommand))
                {
                    AddParameters(command, record, @params);

                    var id = command.ExecuteScalar();
                    if (id is int)
                        result = (int)id;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                this.Connection.Close();
            }

            return result;
        }

        protected internal TCommand NewCommand<TConnection, TCommand>(string cmd)
            where TConnection : DbConnection, new()
            where TCommand : DbCommand, new()
        {
            this.Connection = new TConnection();
            this.Connection.ConnectionString = this.ConnectionString;
            this.Connection.Open();

            TCommand command = new TCommand();
            command.CommandText = cmd;
            command.Connection = this.Connection;

            return command;
        }

        protected internal void AddParameters<TRecord>(DbCommand command, TRecord record = default(TRecord), (string, object)[] @params = null)
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

            if (record != null)
            {
                foreach (PropertyInfo property in GetProperties<TRecord>())
                {
                    if (property.CanRead)
                    {
                        var value = property.GetValue(record, null);

                        if (value != null)
                        {
                            var parameter = command.CreateParameter();
                            parameter.ParameterName = property.Name;
                            parameter.Value = value;
                            command.Parameters.Add(parameter);
                        }
                    }
                }
            }
        }

        protected internal TRecord NewInstance<TRecord>(PropertyInfo[] properties, DbDataReader reader) where TRecord : new()
        {
            var instance = new TRecord();

            foreach (PropertyInfo property in properties)
            {
                if (property.CanWrite)
                {
                    if (!reader.IsDBNull(reader.GetOrdinal(property.Name)))
                    {
                        var value = reader[property.Name];
                        if (value is string)
                            value = (value as string).Trim();

                        property.SetValue(instance, value, null);
                    }
                }
            }

            return instance;
        }

        protected internal PropertyInfo[] GetProperties<TRecord>()
        {
            return typeof(TRecord).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        }

        protected internal string Pluralize(ref string text)
        {
            var info = new CultureInfo("en-us");
            var service = PluralizationService.CreateService(info);
            if (service.IsSingular(text))
                text = service.Pluralize(text);

            return text;
        }

        #endregion
    }
}
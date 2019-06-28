using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Design.PluralizationServices;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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

        public abstract Task<int> NewAsync<T>(string cmd, T record) where T : new();
        public abstract Task NewAsync<T>(IEnumerable<T> records, string tableName) where T : new();
        public abstract Task<IQueryable<T>> GetAsync<T>(string cmd, (string, object)[] @params = null) where T : new();
        public abstract Task<int> EditAsync<T>(string cmd, T record, (string, object)[] @params = null) where T : new();
        public abstract Task<int> RemoveAsync<T>(string cmd, T record, (string, object)[] @params = null) where T : new();
        public abstract Task<bool> IsConnectionAvailableAsync();

        #endregion

        #region Internal Methods

        protected async internal Task<IQueryable<TRecord>> QueryAsync<TConnection, TCommand, TRecord>(string cmd, (string, object)[] @params)
            where TConnection : DbConnection, new()
            where TCommand : DbCommand, new()
            where TRecord : new()
        {
            var records = new List<TRecord>();

            try
            {
                using (var command = await NewCommandAsync<TConnection, TCommand>(cmd))
                {
                    AddParameters<TRecord>(command, @params: @params);

                    PropertyInfo[] properties = GetProperties<TRecord>();
                    DbDataReader reader = command.ExecuteReader();

                    while (await reader.ReadAsync())
                        records.Add(NewInstance<TRecord>(properties, reader));
                }
            }
            catch (Exception ex)
            { throw ex; }
            finally
            { this.Connection.Close(); }

            return records.AsQueryable();
        }

        protected async internal Task<int> NonQueryAsync<TConnection, TCommand, TRecord>(string cmd, TRecord record, (string, object)[] @params)
            where TConnection : DbConnection, new()
            where TCommand : DbCommand, new()
            where TRecord : new()
        {
            int result = default(int);

            try
            {
                using (var command = await NewCommandAsync<TConnection, TCommand>(cmd + this.IDCommand))
                {
                    AddParameters(command, record, @params);

                    var id = await command.ExecuteScalarAsync();
                    if (id is int)
                        result = (int)id;
                }
            }
            catch (Exception ex)
            { throw ex; }
            finally
            { this.Connection.Close(); }

            return result;
        }

        protected async internal Task<TCommand> NewCommandAsync<TConnection, TCommand>(string cmd)
            where TConnection : DbConnection, new()
            where TCommand : DbCommand, new()
        {
            await this.OpenConnectionAsync<TConnection>();

            TCommand command = new TCommand();
            command.CommandText = cmd;
            command.Connection = this.Connection;

            return command;
        }

        protected async internal Task<bool> OpenConnectionAsync<TConnection>()
            where TConnection : DbConnection, new()
        {
            try
            {
                this.Connection = new TConnection();
                this.Connection.ConnectionString = this.ConnectionString;
                await this.Connection.OpenAsync();
            }
            catch (Exception ex)
            { throw ex; }

            return true;
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
                        if (value == null) value = DBNull.Value;

                        var parameter = command.CreateParameter();
                        parameter.ParameterName = property.Name;
                        parameter.Value = value;
                        command.Parameters.Add(parameter);
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
            { text = service.Pluralize(text); }

            return text;
        }

        #endregion
    }
}

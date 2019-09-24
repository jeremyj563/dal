using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Design.PluralizationServices;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DataRepositories.Classes;
using DataRepositories.Interfaces;

namespace DataRepositories
{
    public abstract class BaseSQLRepository : IDataRepository
    {

        protected string ConnectionString { get; }
        protected string TableName { get; set; }
        protected string IDCommand { get; }
        protected DbConnection Connection { get; set; }

        protected BaseSQLRepository(string connectionString, string idCommand)
        {
            this.ConnectionString = connectionString;
            this.IDCommand = idCommand;
        }

        #region Public Methods

        public abstract Task<int> NewAsync<TSchema>(string cmd, TSchema schema) where TSchema : new();
        public abstract Task NewAsync<TSchema>(IEnumerable<TSchema> instances, string tableName) where TSchema : new();
        public abstract Task<IQueryable<TSchema>> GetAsync<TSchema>(string cmd, (string, object)[] @params = null) where TSchema : new();
        public abstract Task<IQueryable<Dynamic>> GetDynamicAsync(Dynamic schema, string cmd, (string, object)[] @params = null);
        public abstract Task<int> EditAsync<TSchema>(string cmd, TSchema schema, (string, object)[] @params = null) where TSchema : new();
        public abstract Task<int> RemoveAsync<TSchema>(string cmd, TSchema schema, (string, object)[] @params = null) where TSchema : new();
        public abstract Task<bool> IsConnectionAvailableAsync();

        #endregion

        #region Protected Methods

        protected async Task<IQueryable<TSchema>> QueryAsync<TConnection, TCommand, TSchema>(string cmd, (string, object)[] @params)
            where TConnection : DbConnection, new()
            where TCommand : DbCommand, new()
            where TSchema : new()
        {
            var instances = new List<TSchema>();

            try
            {
                using (var command = await NewCommandAsync<TConnection, TCommand>(cmd))
                {
                    if (command.Connection.State == ConnectionState.Open)
                    {
                        AddParameters<TSchema>(command, @params: @params);
                        var properties = GetProperties<TSchema>();

                        DbDataReader reader = await command.ExecuteReaderAsync();
                        if (!reader.IsClosed && reader.HasRows)
                        {
                            while (await reader.ReadAsync())
                            { instances.Add(NewInstance<TSchema>(properties, reader)); }
                        }
                    }
                }
            }
            catch (Exception) { throw; }
            finally { this.Connection.Close(); }

            return instances.AsQueryable();
        }

        protected async Task<IQueryable<Dynamic>> QueryDynamicAsync<TConnection, TCommand>(Dynamic schema, string cmd, (string, object)[] @params)
            where TConnection : DbConnection, new()
            where TCommand : DbCommand, new()
        {
            var instances = new List<Dynamic>();

            try
            {
                using (var command = await NewCommandAsync<TConnection, TCommand>(cmd))
                {
                    if (command.Connection.State == ConnectionState.Open)
                    {
                        AddParameters<Dynamic>(command, @params: @params);
                        var properties = GetDynamicProperties(schema);

                        DbDataReader reader = await command.ExecuteReaderAsync();
                        if (!reader.IsClosed && reader.HasRows)
                        {
                            while (await reader.ReadAsync())
                            { instances.Add(NewDynamicInstance(schema, properties, reader)); }
                        }
                    }
                }
            }
            catch (Exception) { throw; }
            finally { this.Connection.Close(); }

            return instances.AsQueryable();
        }

        protected async Task<int> NonQueryAsync<TConnection, TCommand, TSchema>(string cmd, TSchema schema, (string, object)[] @params)
            where TConnection : DbConnection, new()
            where TCommand : DbCommand, new()
            where TSchema : new()
        {
            int result = default;

            try
            {
                using (var command = await NewCommandAsync<TConnection, TCommand>(cmd + this.IDCommand))
                {
                    AddParameters(command, schema, @params);

                    if (this.Connection.State == ConnectionState.Open)
                    {
                        var id = await command.ExecuteScalarAsync();
                        if (id is int)
                        { result = (int)id; }
                    }
                }
            }
            catch (Exception) { throw; }
            finally { this.Connection.Close(); }

            return result;
        }

        protected async Task<bool> OpenConnectionAsync<TConnection>()
            where TConnection : DbConnection, new()
        {
            try
            {
                this.Connection = new TConnection();
                this.Connection.ConnectionString = this.ConnectionString;
                await this.Connection.OpenAsync();
            }
            catch (Exception) { throw; }

            if (this.Connection.State == ConnectionState.Open)
            { return true; }

            return false;
        }

        protected string Pluralize(ref string text)
        {
            var info = new CultureInfo("en-us");
            var service = PluralizationService.CreateService(info);
            if (service.IsSingular(text))
            { text = service.Pluralize(text); }

            return text;
        }

        #endregion

        #region Private Methods

        private async Task<TCommand> NewCommandAsync<TConnection, TCommand>(string cmd)
            where TConnection : DbConnection, new()
            where TCommand : DbCommand, new()
        {
            await this.OpenConnectionAsync<TConnection>();

            TCommand command = new TCommand();
            command.CommandText = cmd;
            command.Connection = this.Connection;

            return command;
        }

        private void AddParameters<TSchema>(DbCommand command, TSchema schema = default, (string, object)[] @params = null)
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
                foreach (PropertyInfo property in GetProperties<TSchema>())
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

        private TSchema NewInstance<TSchema>(PropertyInfo[] properties, IDataReader reader) where TSchema : new()
        {
            var instance = new TSchema();

            foreach (PropertyInfo property in properties)
            {
                if (this.Connection.State == ConnectionState.Open && property.CanWrite)
                {
                    if (!reader.IsClosed && !reader.IsDBNull(reader.GetOrdinal(property.Name)))
                    {
                        var value = reader[property.Name];
                        if (value is string)
                        { value = (value as string).Trim(); }

                        property.SetValue(instance, value, null);
                    }
                }
            }

            return instance;
        }

        private Dynamic NewDynamicInstance(Dynamic prototype, string[] properties, DbDataReader reader)
        {
            var instance = prototype.Clone() as Dynamic;
            instance.Properties.Clear();

            foreach (string property in properties)
            {
                object value = null;
                if (!reader.IsClosed && !reader.IsDBNull(reader.GetOrdinal(property)))
                {
                    value = reader[property];
                    if (value is string)
                    { value = (value as string).Trim(); }
                }

                instance[property] = value;
            }

            return instance;
        }

        private PropertyInfo[] GetProperties<TSchema>()
        {
            return typeof(TSchema).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        }

        private string[] GetDynamicProperties(Dynamic instance)
        {
            return instance.Properties.Select(p => p.Key).ToArray();
        }

        #endregion
    }
}

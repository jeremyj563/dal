using System;
using System.Collections.Generic;
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

        protected internal string ConnectionString { get; }
        protected internal string IDCommand { get; }
        protected internal DbConnection Connection { get; set; }

        protected internal BaseSQLRepository(string connectionString, string idCommand)
        {
            this.ConnectionString = connectionString;
            this.IDCommand = idCommand;
        }

        #region External Interface

        public abstract Task<int> NewAsync<TSchema>(string cmd, TSchema schema) where TSchema : new();
        public abstract Task NewAsync<TSchema>(IEnumerable<TSchema> instances, string tableName) where TSchema : new();
        public abstract Task<IQueryable<TSchema>> GetAsync<TSchema>(string cmd, (string, object)[] @params = null) where TSchema : new();
        public abstract Task<IQueryable<Dynamic>> GetDynamicAsync(Dynamic schema, string cmd, (string, object)[] @params = null);
        public abstract Task<int> EditAsync<TSchema>(string cmd, TSchema schema, (string, object)[] @params = null) where TSchema : new();
        public abstract Task<int> RemoveAsync<TSchema>(string cmd, TSchema schema, (string, object)[] @params = null) where TSchema : new();
        public abstract Task<bool> IsConnectionAvailableAsync();

        #endregion

        #region Internal Methods

        protected async internal Task<IQueryable<TSchema>> QueryAsync<TConnection, TCommand, TSchema>(string cmd, (string, object)[] @params)
            where TConnection : DbConnection, new()
            where TCommand : DbCommand, new()
            where TSchema : new()
        {
            var instances = new List<TSchema>();

            try
            {
                using (var command = await NewCommandAsync<TConnection, TCommand>(cmd))
                {
                    AddParameters<TSchema>(command, @params: @params);
                    var properties = GetProperties<TSchema>();

                    DbDataReader reader = command.ExecuteReader();
                    while (await reader.ReadAsync())
                    { instances.Add(NewInstance<TSchema>(properties, reader)); }
                }
            }
            catch (Exception) { throw; }
            finally { this.Connection.Close(); }

            return instances.AsQueryable();
        }

        protected async internal Task<IQueryable<Dynamic>> QueryDynamicAsync<TConnection, TCommand>(Dynamic schema, string cmd, (string, object)[] @params)
            where TConnection : DbConnection, new()
            where TCommand : DbCommand, new()
        {
            var instances = new List<Dynamic>();

            try
            {
                using (var command = await NewCommandAsync<TConnection, TCommand>(cmd))
                {
                    AddParameters<dynamic>(command, @params: @params);
                    (string, object)[] properties = GetDynamicProperties(schema);

                    DbDataReader reader = command.ExecuteReader();
                    while (await reader.ReadAsync())
                    { instances.Add(NewDynamicInstance(schema, properties, reader)); }
                }
            }
            catch (Exception) { throw; }
            finally { this.Connection.Close(); }

            return instances.AsQueryable();
        }

        protected async internal Task<int> NonQueryAsync<TConnection, TCommand, TSchema>(string cmd, TSchema schema, (string, object)[] @params)
            where TConnection : DbConnection, new()
            where TCommand : DbCommand, new()
            where TSchema : new()
        {
            int result = default(int);

            try
            {
                using (var command = await NewCommandAsync<TConnection, TCommand>(cmd + this.IDCommand))
                {
                    AddParameters(command, schema, @params);

                    var id = await command.ExecuteScalarAsync();
                    if (id is int)
                    { result = (int)id; }
                }
            }
            catch (Exception) { throw; }
            finally { this.Connection.Close(); }

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
            catch (Exception) { throw; }

            return true;
        }

        protected internal void AddParameters<TSchema>(DbCommand command, TSchema schema = default(TSchema), (string, object)[] @params = null)
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

        protected internal TSchema NewInstance<TSchema>(PropertyInfo[] properties, DbDataReader reader) where TSchema : new()
        {
            var instance = new TSchema();

            foreach (PropertyInfo property in properties)
            {
                if (property.CanWrite)
                {
                    if (!reader.IsDBNull(reader.GetOrdinal(property.Name)))
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

        protected internal Dynamic NewDynamicInstance(Dynamic prototype, (string, object)[] properties, DbDataReader reader)
        {
            var instance = prototype.Clone() as Dynamic;
            instance.Properties.Clear();

            foreach ((string key, object o) in properties)
            {
                if (!reader.IsDBNull(reader.GetOrdinal(key)))
                {
                    var value = reader[key];
                    if (value is string)
                    { value = (value as string).Trim(); }

                    instance[key] = value;
                }
            }

            return instance;
        }

        protected internal PropertyInfo[] GetProperties<TSchema>()
        {
            return typeof(TSchema).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        }

        protected internal (string, object)[] GetDynamicProperties(Dynamic instance)
        {
            var properties = new List<(string, object)>();

            if (instance != null)
            {
                foreach (var kvp in instance.Properties)
                { properties.Add((kvp.Key, kvp.Value)); }
            }

            return properties.ToArray();
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

using DataRepositories.Classes;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace DataRepositories
{
    public class PGSQLRepository : BaseSQLRepository
    {
        public PGSQLRepository(string connectionString, string idCommand) : base(connectionString, idCommand)
        {
        }

        public async override Task<int> NewAsync<TSchema>(string cmd, TSchema schema)
        {
            var id = await base.NonQueryAsync<NpgsqlConnection, NpgsqlCommand, TSchema>(cmd, schema, null);
            return id;
        }

        public async override Task NewAsync<TSchema>(IEnumerable<TSchema> instances, string tableName)
        {
            throw new System.NotImplementedException();
        }

        public async override Task<IQueryable<TSchema>> GetAsync<TSchema>(string cmd, (string, object)[] @params = null)
        {
            var instances = await base.QueryAsync<NpgsqlConnection, NpgsqlCommand, TSchema>(cmd, @params);
            return instances;
        }

        public async override Task<IQueryable<Dynamic>> GetDynamicAsync(Dynamic schema, string cmd, (string, object)[] @params = null)
        {
            var instances = await base.QueryDynamicAsync<NpgsqlConnection, NpgsqlCommand>(schema, cmd, @params);
            return instances;
        }

        public async override Task<int> EditAsync<TSchema>(string cmd, TSchema record = default, (string, object)[] @params = null)
        {
            var id = await base.NonQueryAsync<NpgsqlConnection, NpgsqlCommand, TSchema>(cmd, record, @params);
            return id;
        }

        public async override Task<int> RemoveAsync<TSchema>(string cmd, TSchema schema = default, (string, object)[] @params = null)
        {
            var id = await base.NonQueryAsync<NpgsqlConnection, NpgsqlCommand, TSchema>(cmd, schema, @params);
            return id;
        }

        public async override Task<bool> IsConnectionAvailableAsync()
        {
            bool result = default(bool);
            try
            {
                result = await base.OpenConnectionAsync<NpgsqlConnection>();
                base.Connection.Close();
            }
            catch (Exception) { throw; }

            return result;
        }
    }
}

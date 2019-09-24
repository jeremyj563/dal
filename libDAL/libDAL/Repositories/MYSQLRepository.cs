using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataRepositories.Classes;
using MySql.Data.MySqlClient;

namespace DataRepositories
{
    public class MYSQLRepository : BaseSQLRepository
    {
        public MYSQLRepository(string connectionString, string idCommand = "; SELECT LAST_INSERT_ID();") 
            : base(connectionString, idCommand)
        {
        }

        #region Public Methods

        public override async Task<int> NewAsync<TSchema>(string cmd, TSchema schema)
        {
            var id = await base.NonQueryAsync<MySqlConnection, MySqlCommand, TSchema>(cmd, schema, null);
            return id;
        }

        public override async Task NewAsync<TSchema>(IEnumerable<TSchema> instances, string tableName)
        {
            throw new NotImplementedException();
        }

        public override async Task<IQueryable<TSchema>> GetAsync<TSchema>(string cmd, (string, object)[] @params = null)
        {
            var instances = await base.QueryAsync<MySqlConnection, MySqlCommand, TSchema>(cmd, @params);
            return instances;
        }

        public override async Task<IQueryable<Dynamic>> GetDynamicAsync(Dynamic schema, string cmd, (string, object)[] @params = null)
        {
            var instances = await base.QueryDynamicAsync<MySqlConnection, MySqlCommand>(schema, cmd, @params);
            return instances;
        }

        public override async Task<int> EditAsync<TSchema>(string cmd, TSchema schema = default, (string, object)[] @params = null)
        {
            var id = await base.NonQueryAsync<MySqlConnection, MySqlCommand, TSchema>(cmd, schema, @params);
            return id;
        }

        public override async Task<int> RemoveAsync<TSchema>(string cmd, TSchema schema = default, (string, object)[] @params = null)
        {
            var id = await base.NonQueryAsync<MySqlConnection, MySqlCommand, TSchema>(cmd, schema, @params);
            return id;
        }

        public override async Task<bool> IsConnectionAvailableAsync()
        {
            bool result = false;
            try { result = await base.OpenConnectionAsync<MySqlConnection>(); }
            catch (Exception) { throw; }
            finally { base.Connection.Close(); }

            return result;
        }

        #endregion
    }
}

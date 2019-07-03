using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using DataRepositories.Classes;
using DataRepositories.Extensions;

namespace DataRepositories
{
    public class MSSQLRepository : BaseSQLRepository
    {
        public MSSQLRepository(string connectionString, string idCommand = "; SELECT SCOPE_IDENTITY()") : base(connectionString, idCommand)
        {
        }

        #region External Interface

        public async override Task<int> NewAsync<TSchema>(string cmd, TSchema schema)
        {
            var id = await base.NonQueryAsync<SqlConnection, SqlCommand, TSchema>(cmd, schema, null);
            return id;
        }

        public async override Task NewAsync<TSchema>(IEnumerable<TSchema> instances, string tableName = null)
        {
            await BulkInsert(instances, tableName);
        }

        public async override Task<IQueryable<TSchema>> GetAsync<TSchema>(string cmd, (string, object)[] @params = null)
        {
            var instances = await base.QueryAsync<SqlConnection, SqlCommand, TSchema>(cmd, @params);
            return instances;
        }

        public async override Task<IQueryable<Dynamic>> GetDynamicAsync(Dynamic schema, string cmd, (string, object)[] @params = null)
        {
            var instances = await base.QueryDynamicAsync<SqlConnection, SqlCommand>(schema, cmd, @params);
            return instances;
        }

        public async override Task<int> EditAsync<TSchema>(string cmd, TSchema record = default, (string, object)[] @params = null)
        {
            var id = await base.NonQueryAsync<SqlConnection, SqlCommand, TSchema>(cmd, record, @params);
            return id;
        }

        public async override Task<int> RemoveAsync<TSchema>(string cmd, TSchema record = default, (string, object)[] @params = null)
        {
            var id = await base.NonQueryAsync<SqlConnection, SqlCommand, TSchema>(cmd, record, @params);
            return id;
        }

        public async override Task<bool> IsConnectionAvailableAsync()
        {
            bool result = default(bool);
            try
            {
                result = await base.OpenConnectionAsync<SqlConnection>();
                base.Connection.Close();
            }
            catch (Exception) { throw; }

            return result;
        }

        #endregion

        #region Internal Methods

        private async Task BulkInsert<TSchema>(IEnumerable<TSchema> instances, string tableName = null)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            { tableName = typeof(TSchema).Name; base.Pluralize(ref tableName); }

            try
            {
                using (var bulkCopy = new SqlBulkCopy(base.ConnectionString) {DestinationTableName = tableName})
                { await bulkCopy.WriteToServerAsync(instances.CopyToDataTable()); }
            }
            catch (Exception) { throw; }
        }

        #endregion
    }
}

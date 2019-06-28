using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using DataRepositories.Extensions;

namespace DataRepositories
{
    public class MSSQLRepository : BaseSQLRepository
    {
        public MSSQLRepository(string connectionString, string idCommand = "; SELECT SCOPE_IDENTITY()") : base(connectionString, idCommand)
        {
        }

        #region External Interface

        public async override Task<int> NewAsync<T>(string cmd, T record)
        {
            var id = await base.NonQueryAsync<SqlConnection, SqlCommand, T>(cmd, record, null);

            return id;
        }

        public async override Task NewAsync<T>(IEnumerable<T> records, string tableName = null)
        {
            await BulkInsert(records, tableName);
        }

        public async override Task<IQueryable<T>> GetAsync<T>(string cmd, (string, object)[] @params = null)
        {
            var records = await base.QueryAsync<SqlConnection, SqlCommand, T>(cmd, @params);

            return records;
        }

        public async override Task<int> EditAsync<T>(string cmd, T record = default(T), (string, object)[] @params = null)
        {
            var id = await base.NonQueryAsync<SqlConnection, SqlCommand, T>(cmd, record, @params);

            return id;
        }

        public async override Task<int> RemoveAsync<T>(string cmd, T record = default(T), (string, object)[] @params = null)
        {
            var id = await base.NonQueryAsync<SqlConnection, SqlCommand, T>(cmd, record, @params);

            return id;
        }

        public async override Task<bool> IsConnectionAvailableAsync()
        {
            bool result = default(bool);
            try { result = await base.OpenConnectionAsync<SqlConnection>(); }
            catch (Exception ex) { /* sliently fail */ }

            return result;
        }

        #endregion

        #region Internal Methods

        private async Task BulkInsert<T>(IEnumerable<T> records, string tableName = null)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                tableName = typeof(T).Name;
                base.Pluralize(ref tableName);
            }

            try
            {
                using (var bulkCopy = new SqlBulkCopy(base.ConnectionString) {DestinationTableName = tableName})
                {
                    await bulkCopy.WriteToServerAsync(records.CopyToDataTable());
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion
    }
}

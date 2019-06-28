using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace DataRepositories
{
    public class MYSQLRepository : BaseSQLRepository
    {
        public MYSQLRepository(string connectionString, string idCommand = "; SELECT LAST_INSERT_ID();") : base(connectionString, idCommand)
        {
        }

        public async override Task<int> NewAsync<T>(string cmd, T record)
        {
            var id = await base.NonQueryAsync<MySqlConnection, MySqlCommand, T>(cmd, record, null);

            return id;
        }

        public async override Task NewAsync<T>(IEnumerable<T> records, string tableName)
        {
            throw new System.NotImplementedException();
        }

        public async override Task<IQueryable<T>> GetAsync<T>(string cmd, (string, object)[] @params = null)
        {
            var records = await base.QueryAsync<MySqlConnection, MySqlCommand, T>(cmd, @params);

            return records;
        }

        public async override Task<int> EditAsync<T>(string cmd, T record = default(T), (string, object)[] @params = null)
        {
            var id = await base.NonQueryAsync<MySqlConnection, MySqlCommand, T>(cmd, record, @params);

            return id;
        }

        public async override Task<int> RemoveAsync<T>(string cmd, T record = default(T), (string, object)[] @params = null)
        {
            var id = await base.NonQueryAsync<MySqlConnection, MySqlCommand, T>(cmd, record, @params);

            return id;
        }

        public async override Task<bool> IsConnectionAvailableAsync()
        {
            bool result = default(bool);
            try { result = await base.OpenConnectionAsync<MySqlConnection>(); }
            catch (Exception ex) { /* sliently fail */ }

            return result;
        }
    }
}

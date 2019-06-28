using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataRepositories
{
    public class PGSQLRepository : BaseSQLRepository
    {
        public PGSQLRepository(string connectionString, string idCommand) : base(connectionString, idCommand)
        {
        }

        public async override Task<int> NewAsync<T>(string cmd, T record)
        {
            var id = await base.NonQueryAsync<NpgsqlConnection, NpgsqlCommand, T>(cmd, record, null);

            return id;
        }

        public async override Task NewAsync<T>(IEnumerable<T> records, string tableName)
        {
            throw new System.NotImplementedException();
        }

        public async override Task<IQueryable<T>> GetAsync<T>(string cmd, (string, object)[] @params = null)
        {
            var records = await base.QueryAsync<NpgsqlConnection, NpgsqlCommand, T>(cmd, @params);

            return records;
        }

        public async override Task<int> EditAsync<T>(string cmd, T record = default(T), (string, object)[] @params = null)
        {
            var id = await base.NonQueryAsync<NpgsqlConnection, NpgsqlCommand, T>(cmd, record, @params);

            return id;
        }

        public async override Task<int> RemoveAsync<T>(string cmd, T record = default(T), (string, object)[] @params = null)
        {
            var id = await base.NonQueryAsync<NpgsqlConnection, NpgsqlCommand, T>(cmd, record, @params);

            return id;
        }

        public async override Task<bool> IsConnectionAvailableAsync()
        {
            bool result = default(bool);
            try { result = await base.OpenConnectionAsync<NpgsqlConnection>(); }
            catch(Exception ex) { /* sliently fail */ }

            return result;
        }
    }
}

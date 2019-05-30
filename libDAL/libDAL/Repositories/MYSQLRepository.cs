using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;

namespace DataRepositories
{
    public class MYSQLRepository : BaseSQLRepository
    {
        public MYSQLRepository(string connectionString, string idCommand = "; SELECT LAST_INSERT_ID();") : base(connectionString, idCommand)
        {
        }

        public override int New<T>(string cmd, T record)
        {
            var id = base.NonQuery<MySqlConnection, MySqlCommand, T>(cmd, record, null);

            return id;
        }

        public override void New<T>(IEnumerable<T> records, string tableName)
        {
            throw new System.NotImplementedException();
        }

        public override IQueryable<T> Get<T>(string cmd, (string, object)[] @params = null)
        {
            var records = base.Query<MySqlConnection, MySqlCommand, T>(cmd, @params);

            return records;
        }

        public override int Edit<T>(string cmd, T record = default(T), (string, object)[] @params = null)
        {
            var id = base.NonQuery<MySqlConnection, MySqlCommand, T>(cmd, record, @params);

            return id;
        }

        public override int Remove<T>(string cmd, T record = default(T), (string, object)[] @params = null)
        {
            var id = base.NonQuery<MySqlConnection, MySqlCommand, T>(cmd, record, @params);

            return id;
        }
    }
}

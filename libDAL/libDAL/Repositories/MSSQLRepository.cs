using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using DataRepositories.Extensions;

namespace DataRepositories
{
    public class MSSQLRepository : BaseSQLRepository
    {
        public MSSQLRepository(string connectionString, string idCommand = "; SELECT SCOPE_IDENTITY()") : base(connectionString, idCommand)
        {
        }

        #region External Interface

        public override int New<T>(string cmd, T record)
        {
            var id = base.NonQuery<SqlConnection, SqlCommand, T>(cmd, record, null);

            return id;
        }

        public override void New<T>(IEnumerable<T> records, string tableName = null)
        {
            BulkInsert(records, tableName);
        }

        public override IEnumerable<T> Get<T>(string cmd, (string, object)[] @params = null)
        {
            var records = base.Query<SqlConnection, SqlCommand, T>(cmd, @params);

            return records;
        }

        public override int Edit<T>(string cmd, T record = default(T), (string, object)[] @params = null)
        {
            var id = base.NonQuery<SqlConnection, SqlCommand, T>(cmd, record, @params);

            return id;
        }

        public override int Remove<T>(string cmd, T record = default(T), (string, object)[] @params = null)
        {
            var id = base.NonQuery<SqlConnection, SqlCommand, T>(cmd, record, @params);

            return id;
        }

        #endregion

        #region Internal Methods

        private void BulkInsert<T>(IEnumerable<T> records, string tableName = null)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                tableName = base.Pluralize(ref tableName);

            try
            {
                using (var bulkCopy = new SqlBulkCopy(base.ConnectionString) {DestinationTableName = tableName})
                {
                    bulkCopy.WriteToServer(records.CopyToDataTable());
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
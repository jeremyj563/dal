﻿using Npgsql;
using System.Collections.Generic;

namespace DataRepositories
{
    public class PGSQLRepository : BaseSQLRepository
    {
        public PGSQLRepository(string connectionString, string idCommand) : base(connectionString, idCommand)
        {
        }

        public override int New<T>(string cmd, T record)
        {
            int id = base.NonQuery<NpgsqlConnection, NpgsqlCommand, T>(cmd, record, null);

            return id;
        }

        public override IEnumerable<T> Get<T>(string cmd, (string, object)[] @params = null)
        {
            IEnumerable<T> records = base.Query<NpgsqlConnection, NpgsqlCommand, T>(cmd, @params);

            return records;
        }

        public override int Edit<T>(string cmd, T record = default(T), (string, object)[] @params = null)
        {
            var id = base.NonQuery<NpgsqlConnection, NpgsqlCommand, T>(cmd, record, @params);

            return id;
        }

        public override int Remove<T>(string cmd, T record = default(T), (string, object)[] @params = null)
        {
            var id = base.NonQuery<NpgsqlConnection, NpgsqlCommand, T>(cmd, record, @params);

            return id;
        }
    }
}
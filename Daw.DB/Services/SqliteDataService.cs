using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using Daw.DB.Interfaces;

namespace Daw.DB.Services
{
    public class SqliteDataService : IDataService
    {
        private string? _connectionString;

        public void CreateDatabase(string databaseName)
        {
            _connectionString = $"Data Source={databaseName}.db";
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
        }

        public string GetConnectionString()
        {
            return _connectionString ?? throw new InvalidOperationException("Connection string is not set");
        }

        public static bool ColumnExists(SqliteConnection connection, string tableName, string columnName)
        {
            var command = connection.CreateCommand();
            command.CommandText = $"PRAGMA table_info([{tableName}])";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (reader.GetString(1).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}

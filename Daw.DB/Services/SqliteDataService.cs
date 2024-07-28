using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using Daw.DB.Interfaces;
using System.Data.Common;

namespace Daw.DB.Services
{
    public class SqliteDataService : IDataService
    {
        public string? _connectionString;
        private readonly ILogger<EntityService> _logger;

        public SqliteDataService(ILogger<EntityService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Create a new database with the given name
        /// </summary>
        /// <param name="databaseName"></param>
        public void CreateDatabase(string databaseName)
        {
            _logger.LogInformation("Attempting to create database: {DatabaseName}", databaseName);
            _connectionString = $"Data Source={databaseName}.db";
            using var connection = this.GetConnection();
            connection.Open();
            _logger.LogInformation("Database created successfully: {DatabaseName}", databaseName);
        }

        /// <summary>
        /// Check if the column exists in the table
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public bool ColumnExists(string tableName, string columnName)
        {
            _logger.LogInformation("Checking if column exists in table: {TableName}", tableName);
            if (_connectionString is null)
            {
                throw new InvalidOperationException("Connection string is not set");
            }
            using var connection = this.GetConnection();
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = $"PRAGMA table_info([{tableName}])";
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (reader.GetString(1).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }


        public SqliteConnection GetConnection()
        {
            if (_connectionString is null)
            {
                throw new InvalidOperationException("Connection string is not set");
            }
            return new SqliteConnection(_connectionString);
        }

    }
}

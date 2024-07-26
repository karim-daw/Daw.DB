using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using DynamicEntitiesApp.Interfaces;

namespace DynamicEntitiesApp.Services
{
    public class SqliteDataService : IDataService
    {
        private string _connectionString;

        public void CreateDatabase(string databaseName)
        {
            _connectionString = $"Data Source={databaseName}.db";
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
        }

        /// <summary>
        /// Creates a new entity in the database if it does not already exist
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="entityFields"></param>
        public void CreateEntity(string entityName, Dictionary<string, string> entityFields)
        {
            if (!EntityExists(entityName))
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();
                var command = connection.CreateCommand();
                var fields = string.Join(", ", entityFields.Select(f => $"{f.Key} {f.Value}"));
                command.CommandText = $"CREATE TABLE IF NOT EXISTS [{entityName}] (id INTEGER PRIMARY KEY, {fields})";
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Adds a new record to the specified entity
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="record"></param>
        public void AddRecord(string entityName, Dictionary<string, object> record)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var command = connection.CreateCommand();
            var fields = string.Join(", ", record.Keys);
            var placeholders = string.Join(", ", record.Keys.Select(k => $"@{k}"));
            command.CommandText = $"INSERT INTO [{entityName}] ({fields}) VALUES ({placeholders})";

            foreach (var kvp in record)
            {
                command.Parameters.AddWithValue($"@{kvp.Key}", kvp.Value);
            }
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Retrieves all records from the specified entity
        /// </summary>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetRecords(string entityName)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = $"SELECT * FROM [{entityName}]";

            var records = new List<Dictionary<string, object>>();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var record = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        record[reader.GetName(i)] = reader.GetValue(i);
                    }
                    records.Add(record);
                }
            }
            return records;
        }


        /// <summary>
        /// Updates the schema of the specified entity
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="newFields"></param>
        public void UpdateSchema(string entityName, Dictionary<string, string> newFields)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            foreach (var field in newFields)
            {
                if (!ColumnExists(connection, entityName, field.Key))
                {
                    var command = connection.CreateCommand();
                    command.CommandText = $"ALTER TABLE [{entityName}] ADD COLUMN {field.Key} {field.Value}";
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Checks if a column exists in the specified table
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        private bool ColumnExists(SqliteConnection connection, string tableName, string columnName)
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


        /// <summary>
        /// Checks if the specified entity exists in the database
        /// </summary>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public bool EntityExists(string entityName)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = $"SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name=@entityName";
            command.Parameters.AddWithValue("@entityName", entityName);
            var count = Convert.ToInt32(command.ExecuteScalar());
            return count > 0;
        }


        /// <summary>
        /// Creates a new entity with the specified fields
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="fields"></param>
        public void CreateDynamicEntity(string entityName, Dictionary<string, string> fields)
        {
            CreateEntity(entityName, fields);
        }

        /// <summary>
        /// Adds a new record to the specified entity
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="record"></param>
        public void AddDynamicRecord(string entityName, Dictionary<string, object> record)
        {
            AddRecord(entityName, record);
        }


        /// <summary>
        /// Retrieves all records from the specified entity
        /// </summary>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetDynamicRecords(string entityName)
        {
            return GetRecords(entityName);
        }

        /// <summary>
        /// Updates the schema of the specified entity
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="newFields"></param>
        public void UpdateDynamicSchema(string entityName, Dictionary<string, string> newFields)
        {
            UpdateSchema(entityName, newFields);
        }
    }
}

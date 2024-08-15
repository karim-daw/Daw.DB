using Daw.DB.Data.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Daw.DB.Data.Services
{
    public class DictionaryHandler : IDictionaryHandler
    {
        private readonly ISqlService _sqlService;

        // Assuming "Id" is the primary key column name
        private const string DefaultIdColumn = "Id";

        public DictionaryHandler(ISqlService sqlService)
        {
            _sqlService = sqlService;
        }

        public void CreateTable(string tableName, Dictionary<string, string> columns, string connectionString)
        {
            // Check if table already exists
            var tableExistsQuery = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{tableName}';";
            var tableExists = _sqlService.ExecuteQuery(tableExistsQuery, connectionString).Any();

            if (tableExists)
            {
                throw new System.Exception($"Table {tableName} already exists.");
            }

            // Ensure the "Id" column is included and set as PRIMARY KEY
            if (!columns.ContainsKey(DefaultIdColumn))
            {
                columns.Add(DefaultIdColumn, "INTEGER PRIMARY KEY AUTOINCREMENT");
            }

            var columnsDefinition = string.Join(", ", columns.Select(kv => $"{kv.Key} {kv.Value}"));
            var createTableQuery = $"CREATE TABLE {tableName} ({columnsDefinition})";

            try
            {
                _sqlService.ExecuteCommand(createTableQuery, connectionString);
            }
            catch (System.Exception ex)
            {
                throw new System.Exception($"Error creating table {tableName}: {ex.Message}");
            }
        }

        public void AddRecord(string tableName, Dictionary<string, object> record, string connectionString)
        {
            var columns = string.Join(", ", record.Keys);
            var values = string.Join(", ", record.Keys.Select(k => $"@{k}"));
            var insertQuery = $"INSERT INTO {tableName} ({columns}) VALUES ({values})";

            try
            {
                _sqlService.ExecuteCommand(insertQuery, connectionString, record);
            }
            catch (System.Exception ex)
            {
                throw new System.Exception($"Error adding record to table {tableName}: {ex.Message}");
            }
        }

        public IEnumerable<dynamic> GetAllRecords(string tableName, string connectionString)
        {
            var selectQuery = $"SELECT * FROM {tableName}";

            try
            {
                return _sqlService.ExecuteQuery(selectQuery, connectionString);
            }
            catch (System.Exception ex)
            {
                throw new System.Exception($"Error retrieving records from table {tableName}: {ex.Message}");
            }
        }

        public dynamic GetRecordById(string tableName, object id, string connectionString)
        {
            var selectQuery = $"SELECT * FROM {tableName} WHERE {DefaultIdColumn} = @Id";

            try
            {
                return _sqlService.ExecuteQuery(selectQuery, connectionString, new { Id = id }).FirstOrDefault();
            }
            catch (System.Exception ex)
            {
                throw new System.Exception($"Error retrieving record from table {tableName}: {ex.Message}");
            }
        }

        public void UpdateRecord(string tableName, object id, Dictionary<string, object> updatedValues, string connectionString)
        {
            var setClause = string.Join(", ", updatedValues.Keys.Select(k => $"{k} = @{k}"));
            var updateQuery = $"UPDATE {tableName} SET {setClause} WHERE {DefaultIdColumn} = @Id";

            updatedValues[DefaultIdColumn] = id;

            try
            {
                _sqlService.ExecuteCommand(updateQuery, connectionString, updatedValues);
            }
            catch (System.Exception ex)
            {
                throw new System.Exception($"Error updating record in table {tableName}: {ex.Message}");
            }
        }

        public void DeleteRecord(string tableName, object id, string connectionString)
        {
            var deleteQuery = $"DELETE FROM {tableName} WHERE {DefaultIdColumn} = @Id";

            try
            {
                _sqlService.ExecuteCommand(deleteQuery, connectionString, new { Id = id });
            }
            catch (System.Exception ex)
            {
                throw new System.Exception($"Error deleting record from table {tableName}: {ex.Message}");
            }
        }
    }
}

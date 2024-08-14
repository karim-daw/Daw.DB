using Dapper;
using Daw.DB.Data.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Daw.DB.Data.Services
{
    public class DictionaryHandler : IDictionaryHandler
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;

        // Assuming "Id" is the primary key column name
        private const string DefaultIdColumn = "Id";

        public DictionaryHandler(IDatabaseConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public void CreateTable(string tableName, Dictionary<string, string> columns, string connectionString)
        {
            using (var db = _connectionFactory.CreateConnection(connectionString))
            {
                db.Open();

                // Try to query the table to see if it exists
                var tableExistsQuery = $"SELECT 1 FROM {tableName} LIMIT 1";
                try
                {
                    db.Execute(tableExistsQuery);
                    // If we reach this point, the table exists
                    throw new System.Exception($"Table {tableName} already exists.");
                }
                catch (System.Exception ex)
                {
                    // For other databases, if the exception is not about the table missing, rethrow it
                    if (!ex.Message.Contains("does not exist") && !ex.Message.Contains("no such table"))
                    {
                        throw;
                    }
                    // Otherwise, it means the table does not exist, so we continue
                }

                var columnsDefinition = string.Join(", ", columns.Select(kv => $"{kv.Key} {kv.Value}"));
                var createTableQuery = $"CREATE TABLE {tableName} ({columnsDefinition})";

                try
                {
                    db.Execute(createTableQuery);
                }
                catch (System.Exception ex)
                {
                    throw new System.Exception($"Error creating table {tableName}: {ex.Message}");
                }
            }
        }


        public void AddRecord(string tableName, Dictionary<string, object> record, string connectionString)
        {
            using (var db = _connectionFactory.CreateConnection(connectionString))
            {
                db.Open();
                var columns = string.Join(", ", record.Keys);
                var values = string.Join(", ", record.Keys.Select(k => $"@{k}"));
                var insertQuery = $"INSERT INTO {tableName} ({columns}) VALUES ({values})";
                try
                {
                    db.Execute(insertQuery, record);
                }
                catch (System.Exception ex)
                {
                    throw new System.Exception($"Error adding record to table {tableName}: {ex.Message}");
                }
            }
        }

        public IEnumerable<dynamic> GetAllRecords(string tableName, string connectionString)
        {
            using (var db = _connectionFactory.CreateConnection(connectionString))
            {
                db.Open();
                var selectQuery = $"SELECT * FROM {tableName}";

                IEnumerable<dynamic> records = null;

                try
                {
                    records = db.Query(selectQuery);
                }
                catch (System.Exception ex)
                {
                    throw new System.Exception($"Error retrieving records from table {tableName}: {ex.Message}");
                }

                return records;
            }
        }

        public dynamic GetRecordById(string tableName, object id, string connectionString)
        {
            using (var db = _connectionFactory.CreateConnection(connectionString))
            {
                db.Open();
                var selectQuery = $"SELECT * FROM {tableName} WHERE {DefaultIdColumn} = @Id";

                dynamic record = null;

                try
                {
                    record = db.QuerySingleOrDefault(selectQuery, new { Id = id });
                }
                catch (System.Exception ex)
                {
                    throw new System.Exception($"Error retrieving record from table {tableName}: {ex.Message}");
                }

                return record;
            }
        }

        public void UpdateRecord(string tableName, object id, Dictionary<string, object> updatedValues, string connectionString)
        {
            using (var db = _connectionFactory.CreateConnection(connectionString))
            {
                db.Open();
                var setClause = string.Join(", ", updatedValues.Keys.Select(k => $"{k} = @{k}"));
                var updateQuery = $"UPDATE {tableName} SET {setClause} WHERE {DefaultIdColumn} = @Id";

                // Add the id to the dictionary so it can be used in the query
                updatedValues[DefaultIdColumn] = id;


                try
                {
                    db.Execute(updateQuery, updatedValues);
                }
                catch (System.Exception ex)
                {
                    throw new System.Exception($"Error updating record in table {tableName}: {ex.Message}");
                }
            }
        }

        public void DeleteRecord(string tableName, object id, string connectionString)
        {
            using (var db = _connectionFactory.CreateConnection(connectionString))
            {
                db.Open();
                var deleteQuery = $"DELETE FROM {tableName} WHERE {DefaultIdColumn} = @Id";
                try
                {
                    db.Execute(deleteQuery, new { Id = id });
                }
                catch (System.Exception ex)
                {
                    throw new System.Exception($"Error deleting record from table {tableName}: {ex.Message}");
                }
            }
        }
    }
}

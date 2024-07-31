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

        public void CreateTable(string tableName, Dictionary<string, string> columns)
        {
            using (var db = _connectionFactory.CreateConnection())
            {
                db.Open();
                var columnsDefinition = string.Join(", ", columns.Select(kv => $"{kv.Key} {kv.Value}"));
                var createTableQuery = $"CREATE TABLE IF NOT EXISTS {tableName} ({columnsDefinition})";
                db.Execute(createTableQuery);
            }
        }

        public void AddRecord(string tableName, Dictionary<string, object> record)
        {
            using (var db = _connectionFactory.CreateConnection())
            {
                db.Open();
                var columns = string.Join(", ", record.Keys);
                var values = string.Join(", ", record.Keys.Select(k => $"@{k}"));
                var insertQuery = $"INSERT INTO {tableName} ({columns}) VALUES ({values})";
                db.Execute(insertQuery, record);
            }
        }

        public IEnumerable<dynamic> GetAllRecords(string tableName)
        {
            using (var db = _connectionFactory.CreateConnection())
            {
                db.Open();
                var selectQuery = $"SELECT * FROM {tableName}";
                return db.Query(selectQuery);
            }
        }

        public dynamic GetRecordById(string tableName, object id)
        {
            using (var db = _connectionFactory.CreateConnection())
            {
                db.Open();
                var selectQuery = $"SELECT * FROM {tableName} WHERE {DefaultIdColumn} = @Id";
                return db.QuerySingleOrDefault(selectQuery, new { Id = id });
            }
        }

        public void UpdateRecord(string tableName, object id, Dictionary<string, object> updatedValues)
        {
            using (var db = _connectionFactory.CreateConnection())
            {
                db.Open();
                var setClause = string.Join(", ", updatedValues.Keys.Select(k => $"{k} = @{k}"));
                var updateQuery = $"UPDATE {tableName} SET {setClause} WHERE {DefaultIdColumn} = @Id";
                updatedValues[DefaultIdColumn] = id;
                db.Execute(updateQuery, updatedValues);
            }
        }

        public void DeleteRecord(string tableName, object id)
        {
            using (var db = _connectionFactory.CreateConnection())
            {
                db.Open();
                var deleteQuery = $"DELETE FROM {tableName} WHERE {DefaultIdColumn} = @Id";
                db.Execute(deleteQuery, new { Id = id });
            }
        }
    }
}

using System.Collections.Generic;
using System.Linq;

namespace Daw.DB.Data.Services
{
    public interface IQueryBuilderService
    {
        string BuildCreateTableQuery(string tableName, Dictionary<string, string> columns);
        string BuildCheckTableExistsQuery(string tableName);
        string BuildInsertQuery(string tableName, Dictionary<string, object> record);
        string BuildUpdateQuery(string tableName, Dictionary<string, object> updatedValues);
        string BuildSelectQuery(string tableName, string whereClause = null);
        string BuildDeleteQuery(string tableName);
    }

    public class QueryBuilderService : IQueryBuilderService
    {
        private const string DefaultIdColumn = "Id";

        public string BuildCreateTableQuery(string tableName, Dictionary<string, string> columns)
        {
            EnsureIdColumn(columns);
            var columnsDefinition = string.Join(", ", columns.Select(kv => $"{kv.Key} {kv.Value}"));
            return $"CREATE TABLE {tableName} ({columnsDefinition})";
        }

        public string BuildCheckTableExistsQuery(string tableName)
        {
            return $"SELECT name FROM sqlite_master WHERE type='table' AND name='{tableName}'";
        }

        public string BuildInsertQuery(string tableName, Dictionary<string, object> record)
        {
            var columns = string.Join(", ", record.Keys);
            var values = string.Join(", ", record.Keys.Select(k => $"@{k}"));
            return $"INSERT INTO {tableName} ({columns}) VALUES ({values})";
        }

        public string BuildUpdateQuery(string tableName, Dictionary<string, object> updatedValues)
        {
            var setClause = string.Join(", ", updatedValues.Keys.Select(k => $"{k} = @{k}"));
            return $"UPDATE {tableName} SET {setClause} WHERE {DefaultIdColumn} = @{DefaultIdColumn}";
        }

        public string BuildSelectQuery(string tableName, string whereClause = null)
        {
            return whereClause == null
                ? $"SELECT * FROM {tableName}"
                : $"SELECT * FROM {tableName} WHERE {whereClause}";
        }

        public string BuildDeleteQuery(string tableName)
        {
            return $"DELETE FROM {tableName} WHERE {DefaultIdColumn} = @{DefaultIdColumn}";
        }

        private void EnsureIdColumn(Dictionary<string, string> columns)
        {
            if (!columns.ContainsKey(DefaultIdColumn))
            {
                columns.Add(DefaultIdColumn, "INTEGER PRIMARY KEY AUTOINCREMENT");
            }
        }
    }
}

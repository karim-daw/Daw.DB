using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        (string query, Dictionary<string, object> parameters) BuildBatchInsertQuery(string tableName, IEnumerable<Dictionary<string, object>> records, int batchSize = 1000);
        string BuildDeleteTableQuery(string tableName);
        string BuildGetAllTableNamesQuery();
    }

    public class QueryBuilderService : IQueryBuilderService
    {
        private const string DefaultIdColumn = "Id";

        public string BuildCreateTableQuery(string tableName, Dictionary<string, string> columns)
        {
            // Inject Id column if it doesn't exist
            columns = InjectIdColumn(columns);
            var columnsDefinition = string.Join(", ", columns.Select(kv => $"{kv.Key} {kv.Value}"));
            return $"CREATE TABLE {tableName} ({columnsDefinition})";
        }

        public string BuildCheckTableExistsQuery(string tableName)
        {
            return $"SELECT name FROM sqlite_master WHERE type='table' AND name='{tableName}'";
        }

        public string BuildInsertQuery(string tableName, Dictionary<string, object> record)
        {
            // Get column names and values from record
            var columns = string.Join(", ", record.Keys);
            var values = string.Join(", ", record.Keys.Select(k => $"@{k}"));
            return $"INSERT INTO {tableName} ({columns}) VALUES ({values})";
        }

        public (string query, Dictionary<string, object> parameters) BuildBatchInsertQuery(string tableName, IEnumerable<Dictionary<string, object>> records, int batchSize = 1000)
        {
            if (records == null || !records.Any())
                throw new ArgumentException("Records collection cannot be null or empty.");

            // Extract column names from the first record
            var columns = string.Join(", ", records.First().Keys);

            // Use StringBuilder to efficiently construct the final query
            var queryBuilder = new StringBuilder();
            var parameters = new Dictionary<string, object>();

            // Use a Queue for efficient batch processing
            var currentBatch = new Queue<string>();
            int batchCounter = 0;
            int recordIndex = 0;

            foreach (var record in records)
            {
                // Create value placeholders for the current record
                var values = string.Join(", ", record.Keys.Select(k => $"@{k}{recordIndex}"));
                currentBatch.Enqueue($"({values})");

                // Add the actual values to the parameters dictionary
                foreach (var kvp in record)
                {
                    parameters[$"{kvp.Key}{recordIndex}"] = kvp.Value;
                }

                recordIndex++;
                batchCounter++;

                // If batch size is reached, append the batch insert statement
                if (batchCounter == batchSize)
                {
                    queryBuilder.Append($"INSERT INTO {tableName} ({columns}) VALUES {string.Join(", ", currentBatch)};");
                    currentBatch.Clear();
                    batchCounter = 0;
                }
            }

            // Append any remaining records in the last batch
            if (currentBatch.Any())
            {
                queryBuilder.Append($"INSERT INTO {tableName} ({columns}) VALUES {string.Join(", ", currentBatch)};");
            }

            return (queryBuilder.ToString(), parameters);
        }

        public string BuildUpdateQuery(string tableName, Dictionary<string, object> updatedValues)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));


            if (updatedValues == null || !updatedValues.Any())
                throw new ArgumentException("Updated values collection cannot be empty.");


            var setClause = string.Join(", ", updatedValues.Keys.Select(k => $"{k} = @{k}"));
            return $"UPDATE {tableName} SET {setClause} WHERE {DefaultIdColumn} = @{DefaultIdColumn}";
        }

        public string BuildSelectQuery(string tableName, string whereClause = null)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return whereClause == null
                ? $"SELECT * FROM {tableName}"
                : $"SELECT * FROM {tableName} WHERE {whereClause}";
        }

        public string BuildDeleteQuery(string tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            if (tableName.Equals(string.Empty))
                throw new ArgumentException(nameof(tableName));

            return $"DELETE FROM {tableName} WHERE {DefaultIdColumn} = @{DefaultIdColumn}";
        }

        private Dictionary<string, string> InjectIdColumn(Dictionary<string, string> columns)
        {
            // Create a copy of columns dictionary quickly
            var columnsCopy = new Dictionary<string, string>(columns);

            if (!columnsCopy.ContainsKey(DefaultIdColumn))
            {
                columnsCopy.Add(DefaultIdColumn, "INTEGER PRIMARY KEY AUTOINCREMENT");
            }

            return columnsCopy;
        }

        public string BuildDeleteTableQuery(string tableName)
        {
            return $"DROP TABLE IF EXISTS {tableName}";
        }

        public string BuildGetAllTableNamesQuery()
        {
            return "SELECT name FROM sqlite_master WHERE type='table'";
        }
    }
}

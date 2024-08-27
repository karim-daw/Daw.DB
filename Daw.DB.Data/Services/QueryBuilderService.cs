using System;
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
        (string query, Dictionary<string, object> parameters) BuildBatchInsertQuery(string tableName, IEnumerable<Dictionary<string, object>> records, int batchSize = 1000);
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
            // get column names and values from record
            var columns = string.Join(BuildGetColumnNamesQuery(tableName), record.Keys);


            var values = string.Join(", ", record.Keys.Select(k => $"@{k}"));
            return $"INSERT INTO {tableName} ({columns}) VALUES ({values})";
        }

        // get table column names from table name
        private string BuildGetColumnNamesQuery(string tableName)
        {
            // create column names query
            return $"PRAGMA table_info({tableName})";
        }

        // Example record structure:
        // [
        //     { "Name": "Building 1", "Height": 100, "Floors": 10, "Location": "London" },
        //     { "Name": "Building 2", "Height": 200, "Floors": 20, "Location": "Paris" },
        //     { "Name": "Building 3", "Height": 300, "Floors": 30, "Location": "Berlin" }
        // ]

        public (string query, Dictionary<string, object> parameters) BuildBatchInsertQuery(string tableName, IEnumerable<Dictionary<string, object>> records, int batchSize = 1000)
        {
            if (records == null || !records.Any())
                throw new ArgumentException("Records collection cannot be null or empty.");

            // Extract column names from the first record
            var columns = string.Join(", ", records.First().Keys);

            // Initialize a list to hold the batch insert queries
            var queries = new List<string>();

            // Initialize a dictionary to hold all the parameters
            var parameters = new Dictionary<string, object>();

            // Group records into batches based on the specified batch size
            foreach (var batch in records.Select((record, index) => new { record, index })
                                         .GroupBy(x => x.index / batchSize))
            {
                // For each batch, create a group of values for the insert statement
                var valueGroups = batch.Select(x =>
                {
                    // Create a list of placeholders for the values in the current record
                    var values = string.Join(", ", x.record.Keys.Select(k => $"@{k}{x.index}"));

                    // Add the actual values to the parameters dictionary
                    foreach (var kvp in x.record)
                    {
                        parameters.Add($"{kvp.Key}{x.index}", kvp.Value);
                    }

                    return $"({values})";
                });

                // Combine the value groups into a single values clause
                var valuesClause = string.Join(", ", valueGroups);

                // Construct the full insert query for this batch
                queries.Add($"INSERT INTO {tableName} ({columns}) VALUES {valuesClause}");
            }

            // Combine all batch queries into one string, separated by semicolons
            var finalQuery = string.Join("; ", queries);

            return (finalQuery, parameters);
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

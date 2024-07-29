using System;
using System.Collections.Generic;
using System.Linq;
using Daw.DB.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace Daw.DB.Services
{
    public class RecordService : IRecordService
    {
        private readonly IDataService _dataService;
        private readonly ILogger<RecordService> _logger;

        public RecordService(IDataService dataService, ILogger<RecordService> logger)
        {
            _dataService = dataService;
            _logger = logger;
        }

        public void AddRecord(string entityName, Dictionary<string, object> record)
        {
            _logger.LogInformation("Attempting to add record to entity: {EntityName}", entityName);
            var connection = _dataService.GetConnection();
            try
            {
                connection.Open();
                var command = connection.CreateCommand();

                // Create the SQL command where the fields are the keys of the record
                var fields = string.Join(", ", record.Keys);

                // Create the placeholders for the values in the record, '@' followed by the key means it's a parameter
                var placeholders = string.Join(", ", record.Keys.Select(k => $"@{k}"));

                // Create the SQL command with the fields and placeholders
                command.CommandText = $"INSERT INTO [{entityName}] ({fields}) VALUES ({placeholders})";

                foreach (var kvp in record)
                {
                    command.Parameters.AddWithValue($"@{kvp.Key}", kvp.Value);
                }
                command.ExecuteNonQuery();
                _logger.LogInformation("Record added to entity: {EntityName}", entityName);
            }
            finally
            {
                connection.Dispose();
            }
        }

        public void DeleteRecord(string entityName, Dictionary<string, object> record)
        {
            _logger.LogInformation("Attempting to delete record from entity: {EntityName}", entityName);
            var connection = _dataService.GetConnection();
            try
            {
                connection.Open();

                var command = connection.CreateCommand();
                var id = record["id"];
                command.CommandText = $"DELETE FROM [{entityName}] WHERE id = @id";
                command.Parameters.AddWithValue("@id", id);
                command.ExecuteNonQuery();
                _logger.LogInformation("Record deleted from entity: {EntityName}", entityName);
            }
            finally
            {
                connection.Dispose();
            }
        }

        public List<Dictionary<string, object>> GetRecords(string entityName)
        {
            _logger.LogInformation("Attempting to get records from entity: {EntityName}", entityName);
            var connection = _dataService.GetConnection();
            try
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = $"SELECT * FROM [{entityName}]";

                var records = new List<Dictionary<string, object>>();
                var reader = command.ExecuteReader();
                try
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
                finally
                {
                    reader.Dispose();
                }
                _logger.LogInformation("Retrieved {RecordCount} records from entity: {EntityName}", records.Count, entityName);
                return records;
            }
            finally
            {
                connection.Dispose();
            }
        }

        public Dictionary<string, object> GetRecordById(string entityName, int id)
        {
            _logger.LogInformation("Attempting to get record from entity: {EntityName} with id: {Id}", entityName, id);
            var connection = _dataService.GetConnection();
            try
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = $"SELECT * FROM [{entityName}] WHERE id = @id";
                command.Parameters.AddWithValue("@id", id);

                var reader = command.ExecuteReader();
                try
                {
                    if (reader.Read())
                    {
                        var record = new Dictionary<string, object>(reader.FieldCount);
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            record[reader.GetName(i)] = reader.GetValue(i);
                        }
                        _logger.LogInformation("Record found in entity: {EntityName} with id: {Id}", entityName, id);
                        return record;
                    }
                }
                finally
                {
                    reader.Dispose();
                }
                _logger.LogInformation("Record not found in entity: {EntityName} with id: {Id}", entityName, id);
                return null;
            }
            finally
            {
                connection.Dispose();
            }
        }

        public void UpdateRecord(string entityName, Dictionary<string, object> record)
        {
            _logger.LogInformation("Attempting to update record in entity: {EntityName}", entityName);
            if (!record.ContainsKey("id"))
                throw new ArgumentException("Record must contain an 'id' field for update operation");

            var connection = _dataService.GetConnection();
            try
            {
                connection.Open();
                var command = connection.CreateCommand();
                var fields = string.Join(", ", record.Keys.Where(k => k != "id").Select(k => $"{k} = @{k}"));
                command.CommandText = $"UPDATE [{entityName}] SET {fields} WHERE id = @id";

                foreach (var kvp in record)
                {
                    command.Parameters.AddWithValue($"@{kvp.Key}", kvp.Value);
                }
                command.ExecuteNonQuery();
                _logger.LogInformation("Record updated in entity: {EntityName}", entityName);
            }
            finally
            {
                connection.Dispose();
            }
        }
    }
}

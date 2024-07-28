using Daw.DB.Interfaces;
using Microsoft.Data.Sqlite;

namespace Daw.DB.Services;

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
        using var connection = _dataService.GetConnection();

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
        _logger.LogInformation("Record added to entity: {EntityName}", entityName);
    }


    public void DeleteRecord(string entityName, Dictionary<string, object> record)
    {
        _logger.LogInformation("Attempting to delete record from entity: {EntityName}", entityName);
        using var connection = _dataService.GetConnection();
        connection.Open();
        var command = connection.CreateCommand();
        var id = record["id"];
        command.CommandText = $"DELETE FROM [{entityName}] WHERE id = @id";
        command.Parameters.AddWithValue("@id", id);
        command.ExecuteNonQuery();
        _logger.LogInformation("Record deleted from entity: {EntityName}", entityName);
    }


    public List<Dictionary<string, object>> GetRecords(string entityName)
    {
        _logger.LogInformation("Attempting to get records from entity: {EntityName}", entityName);
        using var connection = _dataService.GetConnection();
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
        _logger.LogInformation("Retrieved {RecordCount} records from entity: {EntityName}", records.Count, entityName);
        return records;
    }


    public Dictionary<string, object>? GetRecordById(string entityName, int id)
    {
        _logger.LogInformation("Attempting to get record from entity: {EntityName} with id: {Id}", entityName, id);
        using var connection = _dataService.GetConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT * FROM [{entityName}] WHERE id = @id";
        command.Parameters.AddWithValue("@id", id);

        using var reader = command.ExecuteReader();
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
        _logger.LogInformation("Record not found in entity: {EntityName} with id: {Id}", entityName, id);
        return null;
    }


    public void UpdateRecord(string entityName, Dictionary<string, object> record)
    {
        _logger.LogInformation("Attempting to update record in entity: {EntityName}", entityName);
        if (!record.ContainsKey("id"))
            throw new ArgumentException("Record must contain an 'id' field for update operation");

        using var connection = _dataService.GetConnection();
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
}

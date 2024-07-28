using Daw.DB.Interfaces;
using Microsoft.Data.Sqlite;

namespace Daw.DB.Services;

public class RecordService : IRecordService
{

    // inject IEntityService
    private readonly IDataService _dataService;

    private readonly string _connectionString;

    public RecordService(IDataService dataService)
    {
        _dataService = dataService;
        _connectionString = _dataService.GetConnectionString();
    }

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

    public void DeleteRecord(string entityName, Dictionary<string, object> record)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var command = connection.CreateCommand();
        var id = record["id"];
        command.CommandText = $"DELETE FROM [{entityName}] WHERE id = @id";
        command.Parameters.AddWithValue("@id", id);
        command.ExecuteNonQuery();
    }

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

    // get record by id
    public Dictionary<string, object>? GetRecordById(string entityName, int id)
    {
        using var connection = new SqliteConnection(_connectionString);
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
            return record;
        }
        return null;
    }


    public void UpdateRecord(string entityName, Dictionary<string, object> record)
    {
        if (!record.ContainsKey("id"))
            throw new ArgumentException("Record must contain an 'id' field for update operation");

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var command = connection.CreateCommand();
        var fields = string.Join(", ", record.Keys.Where(k => k != "id").Select(k => $"{k} = @{k}"));
        command.CommandText = $"UPDATE [{entityName}] SET {fields} WHERE id = @id";

        foreach (var kvp in record)
        {
            command.Parameters.AddWithValue($"@{kvp.Key}", kvp.Value);
        }
        command.ExecuteNonQuery();
    }
}

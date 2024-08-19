using System.Collections.Generic;
using System.Text.Json;


namespace Daw.DB.Data.Services
{

    public interface IJsonHandler
    {
        void AddRecordFromJson(string tableName, string jsonRecord, string connectionString);
        void AddTableFromJson(string tableName, string jsonSchema, string connectionString);
        void UpdateRecordFromJson(string tableName, object id, string jsonRecord, string connectionString);
    }



    public class JsonHandler : IJsonHandler
    {
        private readonly IDictionaryHandler _dictionaryHandler;

        public JsonHandler(IDictionaryHandler dictionaryHandler)
        {
            _dictionaryHandler = dictionaryHandler;
        }

        public void AddRecordFromJson(string tableName, string jsonRecord, string connectionString)
        {
            // using system.text.json
            var record = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonRecord);
            _dictionaryHandler.AddRecord(tableName, record, connectionString);
        }

        public void AddTableFromJson(string tableName, string jsonSchema, string connectionString)
        {
            var columns = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonSchema);
            _dictionaryHandler.CreateTable(tableName, columns, connectionString);
        }

        // update record from json
        public void UpdateRecordFromJson(string tableName, object id, string jsonRecord, string connectionString)
        {
            var record = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonRecord);
            _dictionaryHandler.UpdateRecord(tableName, id, record, connectionString);
        }
    }
}



using Daw.DB.Data.Interfaces;
using System.Collections.Generic;
using System.Text.Json;

namespace Daw.DB.Data.Services
{
    namespace Daw.DB.Data.Services
    {
        public class JsonHandler : IJsonHandler
        {
            private readonly IDictionaryHandler _dictionaryHandler;

            public JsonHandler(IDictionaryHandler dictionaryHandler)
            {
                _dictionaryHandler = dictionaryHandler;
            }

            public void AddRecordFromJson(string tableName, string jsonRecord)
            {
                // using system.text.json
                var record = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonRecord);
                _dictionaryHandler.AddRecord(tableName, record);
            }

            public void AddTableFromJson(string tableName, string jsonSchema)
            {
                var columns = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonSchema);
                _dictionaryHandler.CreateTable(tableName, columns);
            }

            // update record from json
            public void UpdateRecordFromJson(string tableName, object id, string jsonRecord)
            {
                var record = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonRecord);
                _dictionaryHandler.UpdateRecord(tableName, id, record);
            }
        }
    }

}

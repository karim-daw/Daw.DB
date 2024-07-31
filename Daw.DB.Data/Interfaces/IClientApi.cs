using System.Collections.Generic;

namespace Daw.DB.Data.Interfaces
{
    public interface IClientApi
    {
        void InitializeDatabase(string databaseName); // Add this method

        void CreateTable(string tableName, Dictionary<string, string> columns);
        void AddEntityRecord<T>(string tableName, T record) where T : class;
        void AddDictionaryRecord(string tableName, Dictionary<string, object> record);
        void AddRecordFromJson(string tableName, string jsonRecord);
        void AddTableFromJson(string tableName, string jsonSchema);
        IEnumerable<T> GetAllEntityRecords<T>(string tableName) where T : class;
        IEnumerable<dynamic> GetAllDictionaryRecords(string tableName);
        T GetEntityRecordById<T>(string tableName, object id) where T : class;
        dynamic GetDictionaryRecordById(string tableName, object id);
        void UpdateEntityRecord<T>(string tableName, T record) where T : class;
        void UpdateDictionaryRecord(string tableName, object id, Dictionary<string, object> record);
        void UpdateRecordFromJson(string tableName, object id, string jsonRecord);
        void DeleteRecord(string tableName, object id);
        IEnumerable<dynamic> ExecuteQuery(string sql, object parameters = null);
        void ExecuteCommand(string sql, object parameters = null);
    }
}

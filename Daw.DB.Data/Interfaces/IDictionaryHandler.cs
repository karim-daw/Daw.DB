using System.Collections.Generic;

namespace Daw.DB.Data.Interfaces
{
    public interface IDictionaryHandler
    {
        void CreateTable(string tableName, Dictionary<string, string> columns, string connectionString);
        void AddRecord(string tableName, Dictionary<string, object> record, string connectionString);
        IEnumerable<dynamic> GetAllRecords(string tableName, string connectionString);
        dynamic GetRecordById(string tableName, object id, string connectionString);
        void UpdateRecord(string tableName, object id, Dictionary<string, object> updatedValues, string connectionString);
        void DeleteRecord(string tableName, object id, string connectionString);
    }
}

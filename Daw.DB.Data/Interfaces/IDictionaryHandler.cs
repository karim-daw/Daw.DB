using System.Collections.Generic;

namespace Daw.DB.Data.Interfaces
{
    public interface IDictionaryHandler
    {
        void CreateTable(string tableName, Dictionary<string, string> columns);
        void AddRecord(string tableName, Dictionary<string, object> record);
        IEnumerable<dynamic> GetAllRecords(string tableName);
        dynamic GetRecordById(string tableName, object id);
        void UpdateRecord(string tableName, object id, Dictionary<string, object> updatedValues);
        void DeleteRecord(string tableName, object id);
    }
}

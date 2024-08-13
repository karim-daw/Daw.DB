using System.Collections.Generic;

namespace Daw.DB.Data.Interfaces
{
    public interface IEntityHandler<T> where T : class
    {
        void CreateTable(Dictionary<string, string> columns, string connectionString);
        void AddRecord(T record, string connectionString);
        IEnumerable<T> GetAllRecords(string connectionString);
        T GetRecordById(object id, string connectionString);
        void UpdateRecord(T record, string connectionString);
        void DeleteRecord(object id, string connectionString);
    }
}

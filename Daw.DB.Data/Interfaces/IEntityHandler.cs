using System.Collections.Generic;

namespace Daw.DB.Data.Interfaces
{
    public interface IEntityHandler<T> where T : class
    {
        void CreateTable(Dictionary<string, string> columns);
        void AddRecord(T record);
        IEnumerable<T> GetAllRecords();
        T GetRecordById(object id);
        void UpdateRecord(T record);
        void DeleteRecord(object id);
    }
}

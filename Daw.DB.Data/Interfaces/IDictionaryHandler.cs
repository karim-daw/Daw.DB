using System.Collections.Generic;

namespace Daw.DB.Data.Interfaces
{
    public interface IDictionaryHandler
    {
        void CreateTable(string tableName, Dictionary<string, string> columns, string connectionString);
        void AddRecord(string tableName, Dictionary<string, object> record, string connectionString);


        /// <summary>
        /// Try to retrieve all records from a table
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        IEnumerable<dynamic> GetAllRecords(string tableName, string connectionString);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="id"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        dynamic GetRecordById(string tableName, object id, string connectionString);

        void UpdateRecord(string tableName, object id, Dictionary<string, object> updatedValues, string connectionString);
        void DeleteRecord(string tableName, object id, string connectionString);
    }
}

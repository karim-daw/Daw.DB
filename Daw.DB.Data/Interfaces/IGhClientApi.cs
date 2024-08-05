
using System.Collections.Generic;

namespace Daw.DB.Data.Interfaces
{
    public interface IGhClientApi
    {
        #region Database Operations

        /// <summary>
        /// Initializes a new database with the given name.
        /// </summary>
        /// <param name="databaseName"></param>
        /// <returns>Message output relating to success or failure of db operatations</returns>
        string InitializeDatabase(string databaseName); // Add this method

        #endregion

        #region Table Operations

        /// <summary>
        /// Creates a new table with the given name and columns. 
        /// This expects a dictionary of column names and their data types.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columns"></param>
        /// <returns>Message output relating to success or failure of table operations</returns>
        string CreateTable(string tableName, Dictionary<string, string> columns);


        #endregion


        #region Add Record

        /// <summary>
        /// Adds a new record to the table with the given name.
        /// This expects a dictionary of column names and their values.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="record"></param>
        /// <returns>Message output relating to success or failure of record operations</returns>
        string AddDictionaryRecord(string tableName, Dictionary<string, object> record);


        #endregion

        #region Get Records

        /// <summary>
        /// Gets all records from the table with the given name.
        /// This expects a dictionary of column names and their values.
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        IEnumerable<dynamic> GetAllDictionaryRecords(string tableName);


        /// <summary>
        /// Gets a record from the table with the given name.
        /// THis expectts an id of the record.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        dynamic GetDictionaryRecordById(string tableName, object id);

        #endregion


        #region Update Record

        /// <summary>
        /// Updates a record in the table with the given name.
        /// This expects a dictionary of column names and their values.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="id"></param>
        /// <param name="record"></param>
        void UpdateDictionaryRecord(string tableName, object id, Dictionary<string, object> record);

        #endregion

        #region Delete Record

        /// <summary>
        /// Deletes a record from the table with the given name.
        /// You will need to provide the id of the record to delete.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="id"></param>
        void DeleteRecord(string tableName, object id);

        #endregion

    }

}

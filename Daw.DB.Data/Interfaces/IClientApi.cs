using System.Collections.Generic;

namespace Daw.DB.Data.Interfaces
{
    public interface IClientApi
    {

        #region Database Operations

        /// <summary>
        /// Initializes a new database with the given name.
        /// </summary>
        /// <param name="databaseName"></param>
        void InitializeDatabase(string connectionString); // Add this method

        #endregion

        #region Table Operations

        /// <summary>
        /// Creates a new table with the given name and columns. 
        /// This expects a dictionary of column names and their data types.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columns"></param>
        void CreateTable(string tableName, Dictionary<string, string> columns, string connectionString);

        /// <summary>
        /// Creates a new table with the given name and columns.
        /// This expects a JSON schema string.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="jsonSchema"></param>
        void AddTableFromJson(string tableName, string jsonSchema, string connectionString);

        #endregion


        #region Add Record

        /// <summary>
        /// Adds a new record to the table with the given name.
        /// This expects an entity object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName"></param>
        /// <param name="record"></param>
        void AddEntityRecord<T>(string tableName, T record, string connectionString) where T : class;

        /// <summary>
        /// Adds a new record to the table with the given name.
        /// This expects a dictionary of column names and their values.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="record"></param>
        void AddDictionaryRecord(string tableName, Dictionary<string, object> record, string connectionString);

        /// <summary>
        /// Adds a new record to the table with the given name.
        /// This expects a JSON string.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="jsonRecord"></param>
        void AddRecordFromJson(string tableName, string jsonRecord, string connectionString);


        #endregion

        #region Get Records

        /// <summary>
        /// Gets all records from the table with the given name.
        /// This expects an entity object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName"></param>
        /// <returns></returns>
        IEnumerable<T> GetAllEntityRecords<T>(string tableName, string connectionString) where T : class;

        /// <summary>
        /// Gets all records from the table with the given name.
        /// This expects a dictionary of column names and their values.
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        IEnumerable<dynamic> GetAllDictionaryRecords(string tableName, string connectionString);

        /// <summary>
        /// Gets all records from the table with the given name.
        /// This expects a JSON string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        T GetEntityRecordById<T>(string tableName, object id, string connectionString) where T : class;

        /// <summary>
        /// Gets a record from the table with the given name.
        /// This expects a dictionary of column names and their values.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        dynamic GetDictionaryRecordById(string tableName, object id, string connectionString);

        #endregion


        #region Update Record

        /// <summary>
        /// Updates a record in the table with the given name.
        /// This expects an entity object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName"></param>
        /// <param name="record"></param>
        void UpdateEntityRecord<T>(string tableName, T record, string connectionString) where T : class;

        /// <summary>
        /// Updates a record in the table with the given name.
        /// This expects a dictionary of column names and their values.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="id"></param>
        /// <param name="record"></param>
        void UpdateDictionaryRecord(string tableName, object id, Dictionary<string, object> record, string connectionString);

        /// <summary>
        /// Updates a record in the table with the given name.
        /// This expects a JSON string representing the record.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="id"></param>
        /// <param name="jsonRecord"></param>
        void UpdateRecordFromJson(string tableName, object id, string jsonRecord, string connectionString);

        #endregion

        #region Delete Record

        /// <summary>
        /// Deletes a record from the table with the given name.
        /// You will need to provide the id of the record to delete.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="id"></param>
        void DeleteRecord(string tableName, object id, string connectionString);

        #endregion

        #region Query

        /// <summary>
        /// Executes a query and returns the result.
        /// This expects a SQL query string.
        /// You can also provide parameters for the query. 
        /// Example:
        /// var parameters = new { Id = 1 };
        /// var result = ExecuteQuery("SELECT * FROM Table WHERE Id = @Id", parameters);
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        IEnumerable<dynamic> ExecuteQuery(string sql, string connectionString, object parameters = null);

        /// <summary>
        /// Executes a command.
        /// This expects a SQL command string.
        /// You can also provide parameters for the command.
        /// Example:
        /// var parameters = new { Id = 1 };
        /// ExecuteCommand("DELETE FROM Table WHERE Id = @Id", parameters);
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        void ExecuteCommand(string sql, string connectionString, object parameters = null);

        #endregion

    }
}

using Daw.DB.Data.Interfaces;
using System;
using System.Collections.Generic;


namespace Daw.DB.Data.APIs
{
    public class GH_ClientApi : IGH_ClientApi
    {
        private IDatabaseConnectionFactory _connectionFactory;
        private readonly ISqlService _sqlService;
        private readonly IJsonHandler _jsonHandler;
        private readonly IDictionaryHandler _dictionaryHandler;

        public GH_ClientApi(
            IDatabaseConnectionFactory connectionFactory,
            ISqlService sqlService,
            IJsonHandler jsonHandler,
            IDictionaryHandler dictionaryHandler)
        {
            _connectionFactory = connectionFactory;
            _sqlService = sqlService;
            _jsonHandler = jsonHandler;
            _dictionaryHandler = dictionaryHandler;
        }

        /// <summary>
        /// Initializes a new database with the given name.
        /// </summary>
        /// <param name="databasePath"></param>
        public void InitializeDatabase(string databasePath)
        {
            _connectionFactory.SetConnectionString(databasePath);
            // Create the database connection to ensure the database file is created
            using (var db = _connectionFactory.CreateConnection())
            {
                db.Open();
            }
        }

        /// <summary>
        /// Creates a new table with the given name and columns.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columns"></param>
        public void CreateTable(string tableName, Dictionary<string, string> columns)
        {
            _dictionaryHandler.CreateTable(tableName, columns);
        }

        /// <summary>
        /// Adds a new record to the table with the given name.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="record"></param>
        public void AddDictionaryRecord(string tableName, Dictionary<string, object> record)
        {
            _dictionaryHandler.AddRecord(tableName, record);
        }


        /// <summary>
        /// Gets all records from the table with the given name.
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public IEnumerable<dynamic> GetAllDictionaryRecords(string tableName)
        {
            return _dictionaryHandler.GetAllRecords(tableName);
        }

        /// <summary>
        /// Gets a record from the table with the given name.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public dynamic GetDictionaryRecordById(string tableName, object id)
        {
            return _dictionaryHandler.GetRecordById(tableName, id);
        }

        /// <summary>
        /// Adds a new record to the table with the given name using the id and record as a dictionary.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="id"></param>
        /// <param name="record"></param>
        public void UpdateDictionaryRecord(string tableName, object id, Dictionary<string, object> record)
        {
            _dictionaryHandler.UpdateRecord(tableName, id, record);
        }


        /// <summary>
        /// Deletes a record from the table with the given name using the given id.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="id"></param>
        public void DeleteRecord(string tableName, object id)
        {
            _dictionaryHandler.DeleteRecord(tableName, id);
        }

        /// <summary>
        /// Executes a query on the database.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public IEnumerable<dynamic> ExecuteQuery(string sql, object parameters = null)
        {
            return _sqlService.ExecuteQuery(sql, parameters);
        }

        /// <summary>
        /// Executes a command on the database.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        public void ExecuteCommand(string sql, object parameters = null)
        {
            _sqlService.ExecuteCommand(sql, parameters);
        }
    }
}

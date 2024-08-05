using Daw.DB.Data.Interfaces;
using System.Collections.Generic;


namespace Daw.DB.Data.APIs
{
    public class GhClientApi : IGhClientApi
    {
        private IDatabaseConnectionFactory _connectionFactory;
        private readonly ISqlService _sqlService;
        private readonly IJsonHandler _jsonHandler;
        private readonly IDictionaryHandler _dictionaryHandler;

        public GhClientApi(
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
        /// <returns>Message output relating to success or failure of db operatations</returns>
        public string InitializeDatabase(string databasePath)
        {
            try
            {
                _connectionFactory.SetConnectionString(databasePath);
                // Create the database connection to ensure the database file is created
                using (var db = _connectionFactory.CreateConnection())
                {
                    db.Open();
                }
                return $"Database created at '{databasePath}'  successfully.";
            }
            catch (System.Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        /// <summary>
        /// Creates a new table with the given name and columns.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columns"></param>
        /// <returns>Message output relating to success or failure of table operations</returns>
        public string CreateTable(string tableName, Dictionary<string, string> columns)
        {
            try
            {
                _dictionaryHandler.CreateTable(tableName, columns);
                return $"Table '{tableName}' created successfully.";
            }
            catch (System.Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        /// <summary>
        /// Adds a new record to the table with the given name.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="record"></param>
        /// <returns>Message output relating to success or failure of table operations</returns>
        public string AddDictionaryRecord(string tableName, Dictionary<string, object> record)
        {
            try
            {
                // add the record to the table
                _dictionaryHandler.AddRecord(tableName, record);

                // return the record added message wth the record details as a string
                var stringRecord = record.ToString();
                return $"Record added to table '{tableName}' successfully. Record: {stringRecord}";
            }
            catch (System.Exception ex)
            {
                return $"Error: {ex.Message}";
            }
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

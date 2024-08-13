using Daw.DB.Data.Interfaces;
using System.Collections.Generic;


namespace Daw.DB.Data.APIs
{
    public class GhClientApi : IGhClientApi
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;
        private readonly ISqlService _sqlService;
        private readonly IDictionaryHandler _dictionaryHandler;

        public GhClientApi(
            IDatabaseConnectionFactory connectionFactory,
            ISqlService sqlService,
            IDictionaryHandler dictionaryHandler)
        {
            _connectionFactory = connectionFactory;
            _sqlService = sqlService;
            _dictionaryHandler = dictionaryHandler;
        }

        /// <summary>
        /// Creates a connection to a new database with the given name.
        /// </summary>
        /// <param name="databasePath"></param>
        /// <returns>Message output relating to success or failure of db operatations</returns>
        public string CreateConnection(string connectionString)
        {
            try
            {
                // Create the database connection to ensure the database file is created
                using (var db = _connectionFactory.CreateConnection(connectionString))
                {
                    db.Open();
                }
                return $"Database connection created at '{connectionString}'  successfully.";
            }
            catch (System.Exception _)
            {
                throw;
            }
        }

        /// <summary>
        /// Creates a new table with the given name and columns.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columns"></param>
        /// <returns>Message output relating to success or failure of table operations</returns>
        public string CreateTable(string tableName, Dictionary<string, string> columns, string connectionString)
        {
            try
            {
                _dictionaryHandler.CreateTable(tableName, columns, connectionString);
                return $"Table '{tableName}' created successfully.";
            }
            catch (System.Exception ex)
            {
                throw new System.Exception($"Error creating table '{tableName}': {ex.Message}");
                //return $"Error: {ex.Message}";
            }
        }

        /// <summary>
        /// Adds a new record to the table with the given name.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="record"></param>
        /// <returns>Message output relating to success or failure of table operations</returns>
        public string AddDictionaryRecord(string tableName, Dictionary<string, object> record, string connectionString)
        {
            try
            {
                // add the record to the table
                _dictionaryHandler.AddRecord(tableName, record, connectionString);

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
        public IEnumerable<dynamic> GetAllDictionaryRecords(string tableName, string connectionString)
        {
            try
            {
                var records = _dictionaryHandler.GetAllRecords(tableName, connectionString);
                return records;
            }
            catch (System.Exception ex)
            {
                throw new System.Exception($"Error retrieving records from table {tableName}: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets a record from the table with the given name.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public dynamic GetDictionaryRecordById(string tableName, object id, string connectionString)
        {
            // TODO: add error handling
            return _dictionaryHandler.GetRecordById(tableName, id, connectionString);
        }

        /// <summary>
        /// Adds a new record to the table with the given name using the id and record as a dictionary.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="id"></param>
        /// <param name="record"></param>
        public void UpdateDictionaryRecord(string tableName, object id, Dictionary<string, object> record, string connectionString)
        {
            _dictionaryHandler.UpdateRecord(tableName, id, record, connectionString);
        }


        /// <summary>
        /// Deletes a record from the table with the given name using the given id.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="id"></param>
        public void DeleteRecord(string tableName, object id, string connectionString)
        {
            _dictionaryHandler.DeleteRecord(tableName, id, connectionString);
        }

        /// <summary>
        /// Executes a query on the database.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public IEnumerable<dynamic> ExecuteQuery(string sql, string connectionString, object parameters = null)
        {
            return _sqlService.ExecuteQuery(sql, connectionString, parameters);
        }

        /// <summary>
        /// Executes a command on the database.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        public void ExecuteCommand(string sql, string connectionString, object parameters = null)
        {
            _sqlService.ExecuteCommand(sql, connectionString, parameters);
        }
    }
}

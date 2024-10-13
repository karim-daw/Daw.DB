using Daw.DB.Data.Services;
using System;
using System.Collections.Generic;
using System.Text.Json;


namespace Daw.DB.Data.APIs
{
    public interface IGhClientApi
    {
        string CreateConnection();
        string CreateTable(string tableName, Dictionary<string, string> columns);
        string DeleteTable(string tableName);

        // TODO: Create many tables
        string CreateTables(IEnumerable<string> tableNames, IEnumerable<Dictionary<string, string>> columns);


        IEnumerable<dynamic> GetTables();

        string AddDictionaryRecord(string tableName, Dictionary<string, object> record);
        IEnumerable<dynamic> GetAllDictionaryRecords(string tableName);
        dynamic GetDictionaryRecordById(string tableName, object id);
        void UpdateDictionaryRecord(string tableName, object id, Dictionary<string, object> record);
        void DeleteRecord(string tableName, object id);
        string AddDictionaryRecordInTransaction(string tableName, IEnumerable<Dictionary<string, object>> record);
        string AddDictionaryRecordBatch(string tableName, IEnumerable<Dictionary<string, object>> records);
        string AddDictionaryRecordBatchInTransaction(string tableName, IEnumerable<Dictionary<string, object>> records);
        bool Ping(string connectionString);
    }
    public class GhClientApi : IGhClientApi
    {
        private readonly IDictionaryHandler _dictionaryHandler;
        private readonly IDatabaseContext _databaseContext;
        private readonly IDatabaseConnectionFactory _databaseConnectionFactory;

        public GhClientApi(
            IDictionaryHandler dictionaryHandler,
            IDatabaseContext databaseContext,
            IDatabaseConnectionFactory databaseConnectionFactory)
        {
            _dictionaryHandler = dictionaryHandler;
            _databaseContext = databaseContext;
            _databaseConnectionFactory = databaseConnectionFactory;

        }

        // checks if the connection string is pointing to a valid database
        public bool Ping(string connectionString)
        {
            return IsValidDatabase(connectionString);
        }

        private bool IsValidDatabase(string connectionString)
        {
            // Check if the factory supports pinging
            if (_databaseConnectionFactory is IPingable pingableFactory)
            {
                return pingableFactory.Ping(connectionString);
            }
            else
            {
                throw new NotSupportedException("Ping operation is not supported by this connection factory.");
            }
        }

        public string CreateConnection()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_databaseContext.ConnectionString))
                {
                    return "Connection string is not set.";
                }

                // just use ping to check if the connection is valid
                Ping(_databaseContext.ConnectionString);


                // No need to create a connection here, as the connection is established when needed
                return $"Database connection is configured.";
            }
            catch (Exception ex)
            {
                return $"Error configuring database connection: {ex.Message}";
            }
        }

        public string CreateTable(string tableName, Dictionary<string, string> columns)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_databaseContext.ConnectionString))
                {
                    return "Connection string is not set.";
                }

                _dictionaryHandler.CreateTable(tableName, columns);
                return $"Table '{tableName}' created successfully.";
            }
            catch (Exception ex)
            {
                return $"Error creating table '{tableName}': {ex.Message}";
            }
        }

        public string DeleteTable(string tableName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_databaseContext.ConnectionString))
                {
                    return "Connection string is not set.";
                }

                _dictionaryHandler.DeleteTable(tableName);
                return $"Table '{tableName}' deleted successfully.";
            }
            catch (Exception ex)
            {
                return $"Error deleting table '{tableName}': {ex.Message}";
            }
        }

        public IEnumerable<dynamic> GetTables()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_databaseContext.ConnectionString))
                {
                    throw new InvalidOperationException("Connection string is not set.");
                }

                return _dictionaryHandler.GetTables();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving tables: {ex.Message}");
            }
        }

        public string AddDictionaryRecord(string tableName, Dictionary<string, object> record)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_databaseContext.ConnectionString))
                {
                    return "Connection string is not set.";
                }

                _dictionaryHandler.AddRecord(tableName, record);
                return $"Record added to table '{tableName}' successfully.";
            }
            catch (Exception ex)
            {
                return $"Error adding record to table '{tableName}': {ex.Message}";
            }
        }

        public string AddDictionaryRecordInTransaction(string tableName, IEnumerable<Dictionary<string, object>> records)
        {
            try
            {
                // add the record to the table
                _dictionaryHandler.AddRecordsInTransaction(tableName, records);

                // return the record added message wth the record details as a string
                var stringRecord = records.ToString();
                return $"Record added to table '{tableName}' successfully. Record: {stringRecord}";
            }
            catch (System.Exception ex)
            {
                return $"Error adding record to table '{tableName}': {ex.Message}";
            }
        }


        // add dicitonary bath 
        public string AddDictionaryRecordBatch(string tableName, IEnumerable<Dictionary<string, object>> records)
        {
            try
            {
                // add the record to the table
                _dictionaryHandler.AddRecordsBatch(tableName, records);

                // return the record added message wth the record details as a string
                // var stringRecord = records.ToString();

                // return record as json indented
                var stringRecord = JsonSerializer.Serialize(records, new JsonSerializerOptions { WriteIndented = true });

                return $"Record added to table '{tableName}' successfully. Record: {stringRecord}";
            }
            catch (System.Exception ex)
            {
                return $"Error adding record to table '{tableName}': {ex.Message}";
            }
        }

        // add dicitonary bath in transaction
        public string AddDictionaryRecordBatchInTransaction(string tableName, IEnumerable<Dictionary<string, object>> records)
        {
            try
            {
                // add the record to the table
                _dictionaryHandler.AddRecordsBatchInTransaction(tableName, records);

                // return the record added message wth the record details as a string
                var stringRecord = records.ToString();
                return $"Record added to table '{tableName}' successfully. Record: {stringRecord}";
            }
            catch (System.Exception ex)
            {
                return $"Error adding record to table '{tableName}': {ex.Message}";
            }
        }


        /// <summary>
        /// Gets all records from the table with the given name.
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public IEnumerable<dynamic> GetAllDictionaryRecords(string tableName)
        {
            try
            {
                var records = _dictionaryHandler.GetAllRecords(tableName);
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
        public dynamic GetDictionaryRecordById(string tableName, object id)
        {
            try
            {
                var record = _dictionaryHandler.GetRecordById(tableName, id);
                return record;
            }
            catch (System.Exception ex)
            {
                throw new System.Exception($"Error retrieving record from table {tableName}: {ex.Message}");
            }

        }

        /// <summary>
        /// Adds a new record to the table with the given name using the id and record as a dictionary.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="id"></param>
        /// <param name="record"></param>
        public void UpdateDictionaryRecord(string tableName, object id, Dictionary<string, object> record)
        {
            try
            {
                _dictionaryHandler.UpdateRecord(tableName, id, record);
            }
            catch (System.Exception ex)
            {
                throw new System.Exception($"Error updating record in table {tableName}: {ex.Message}");
            }
        }


        /// <summary>
        /// Deletes a record from the table with the given name using the given id.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="id"></param>
        public void DeleteRecord(string tableName, object id)
        {
            try
            {
                _dictionaryHandler.DeleteRecord(tableName, id);
            }
            catch (System.Exception ex)
            {
                throw new System.Exception($"Error deleting record from table {tableName}: {ex.Message}");
            }
        }


        public string CreateTables(IEnumerable<string> tableNames, IEnumerable<Dictionary<string, string>> columns)
        {
            throw new System.NotImplementedException();
        }

    }
}

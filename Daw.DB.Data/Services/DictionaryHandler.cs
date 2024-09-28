using System.Collections.Generic;
using System.Linq;

namespace Daw.DB.Data.Services
{
    public interface IDictionaryHandler
    {
        void CreateTable(string tableName, Dictionary<string, string> columns, string connectionString);
        void DeleteTable(string tableName, string connectionString);
        void AddRecord(string tableName, Dictionary<string, object> record, string connectionString);
        void AddRecordsInTransaction(string tableName, IEnumerable<Dictionary<string, object>> records, string connectionString);
        IEnumerable<dynamic> GetAllRecords(string tableName, string connectionString);
        dynamic GetRecordById(string tableName, object id, string connectionString);
        void UpdateRecord(string tableName, object id, Dictionary<string, object> updatedValues, string connectionString);
        void UpdateRecordsInTransaction(string tableName, IEnumerable<KeyValuePair<object, Dictionary<string, object>>> records, string connectionString);
        void DeleteRecord(string tableName, object id, string connectionString);
        void DeleteRecordsInTransaction(string tableName, IEnumerable<object> ids, string connectionString);
        void AddRecordsBatch(string tableName, IEnumerable<Dictionary<string, object>> records, string connectionString);
        void AddRecordsBatchInTransaction(string tableName, IEnumerable<Dictionary<string, object>> records, string connectionString);
    }

    public class DictionaryHandler : IDictionaryHandler
    {
        private readonly IValidationService _validationService;
        private readonly IQueryBuilderService _queryBuilderService;
        private readonly ISqlService _sqlService;

        // Assuming "Id" is the primary key column name
        private const string DefaultIdColumn = "Id";

        public DictionaryHandler(
            IValidationService validationService,
            IQueryBuilderService queryBuilderService,
            ISqlService sqlService
        )
        {
            _validationService = validationService;
            _queryBuilderService = queryBuilderService;
            _sqlService = sqlService;
        }

        public void CreateTable(string tableName, Dictionary<string, string> columns, string connectionString)
        {
            _validationService.ValidateTableName(tableName);

            // Check if the table already exists
            var checkTableExistsQuery = _queryBuilderService.BuildCheckTableExistsQuery(tableName);
            var tableExists = _sqlService.ExecuteQuery(checkTableExistsQuery, connectionString).Any();
            if (tableExists)
            {
                throw new System.InvalidOperationException("Table already exists.");
            }

            _validationService.ValidateColumns(columns);

            var createTableQuery = _queryBuilderService.BuildCreateTableQuery(tableName, columns);
            _sqlService.ExecuteCommand(createTableQuery, connectionString);
        }
        public void DeleteTable(string tableName, string connectionString)
        {
            var checkTableExistsQuery = _queryBuilderService.BuildCheckTableExistsQuery(tableName);
            var tableExists = _sqlService.ExecuteQuery(checkTableExistsQuery, connectionString).Any();

            if (!tableExists)
            {
                throw new System.InvalidOperationException("Table does not exist.");
            }


            var deleteTableQuery = _queryBuilderService.BuildDeleteTableQuery(tableName);
            _sqlService.ExecuteCommand(deleteTableQuery, connectionString);
        }

        public void AddRecord(string tableName, Dictionary<string, object> record, string connectionString)
        {
            _validationService.ValidateTableName(tableName);
            _validationService.ValidateRecord(record);

            var insertQuery = _queryBuilderService.BuildInsertQuery(tableName, record);
            _sqlService.ExecuteCommand(insertQuery, connectionString, record);
        }

        public void AddRecordsInTransaction(string tableName, IEnumerable<Dictionary<string, object>> records, string connectionString)
        {
            var sqlCommands = new List<(string sql, object parameters)>();

            // Create a list of SQL commands to execute
            foreach (var record in records)
            {
                _validationService.ValidateRecord(record);
                var insertQuery = _queryBuilderService.BuildInsertQuery(tableName, record);
                sqlCommands.Add((insertQuery, record));
            }

            // Execute all commands within a transaction
            _sqlService.ExecuteInTransaction(sqlCommands, connectionString);
        }

        // TODO: still need to test this method
        /// <summary>
        /// Add records in batch.
        /// Use this for the fastest way to insert a large number of records.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="records"></param>
        /// <param name="connectionString"></param>
        public void AddRecordsBatch(string tableName, IEnumerable<Dictionary<string, object>> records, string connectionString)
        {
            // Validate the table name
            _validationService.ValidateTableName(tableName);

            // Validate each record in the collection
            foreach (var record in records)
            {
                _validationService.ValidateRecord(record);
            }

            // Generate the batch insert query and corresponding parameters
            var (batchInsertQuery, parameters) = _queryBuilderService.BuildBatchInsertQuery(tableName, records);

            // Execute the batch insert command with the generated query and parameters
            _sqlService.ExecuteCommand(batchInsertQuery, connectionString, parameters);
        }


        // TODO: still need to test this method
        /// <summary>
        /// Add records in batch within a transaction.
        /// Use this for the fastest way to insert a large number of records while ensuring atomicity.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="records"></param>
        /// <param name="connectionString"></param>
        public void AddRecordsBatchInTransaction(string tableName, IEnumerable<Dictionary<string, object>> records, string connectionString)
        {
            // Validate the table name
            _validationService.ValidateTableName(tableName);

            // Validate each record in the collection
            foreach (var record in records)
            {
                _validationService.ValidateRecord(record);
            }

            // Generate the batch insert query and corresponding parameters
            var (batchInsertQuery, parameters) = _queryBuilderService.BuildBatchInsertQuery(tableName, records);

            // wrap that batch insert query and parameters in an IEnumeration type
            List<(string, object)> batchInsertQueries = new List<(string, object)> { (batchInsertQuery, parameters) };


            // Wrap the batch insert in a transaction and pass the parameters
            _sqlService.ExecuteInTransaction(batchInsertQueries, connectionString);
        }




        /// <summary>
        /// Get all records from the table with the given name.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public IEnumerable<dynamic> GetAllRecords(string tableName, string connectionString)
        {
            _validationService.ValidateTableName(tableName);

            var selectQuery = _queryBuilderService.BuildSelectQuery(tableName);
            return _sqlService.ExecuteQuery(selectQuery, connectionString);
        }



        /// <summary>
        /// Get a record by its Id.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="id"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public dynamic GetRecordById(string tableName, object id, string connectionString)
        {
            _validationService.ValidateTableName(tableName);
            _validationService.ValidateId(id);

            var whereClause = $"{DefaultIdColumn} = @Id";
            var selectQuery = _queryBuilderService.BuildSelectQuery(tableName, whereClause);
            return _sqlService.ExecuteQuery(selectQuery, connectionString, new { Id = id }).FirstOrDefault();
        }



        /// <summary>
        /// Update a record in the table with the given name.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="id"></param>
        /// <param name="updatedValues"></param>
        /// <param name="connectionString"></param>
        public void UpdateRecord(string tableName, object id, Dictionary<string, object> updatedValues, string connectionString)
        {
            _validationService.ValidateTableName(tableName);
            _validationService.ValidateRecord(updatedValues);
            _validationService.ValidateId(id);

            var updateQuery = _queryBuilderService.BuildUpdateQuery(tableName, updatedValues);
            updatedValues[DefaultIdColumn] = id;
            _sqlService.ExecuteCommand(updateQuery, connectionString, updatedValues);
        }


        // TODO: still need to test this method
        /// <summary>
        /// Update records in batch within a transaction.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="records"></param>
        /// <param name="connectionString"></param>
        public void UpdateRecordsInTransaction(string tableName, IEnumerable<KeyValuePair<object, Dictionary<string, object>>> records, string connectionString)
        {
            IEnumerable<(string sql, object parameters)> sqlCommands = null;

            // create a list of sql commands to execute
            foreach (var record in records)
            {
                var id = record.Key;
                var updatedValues = record.Value;

                _validationService.ValidateId(id);
                _validationService.ValidateRecord(updatedValues);

                var updateQuery = _queryBuilderService.BuildUpdateQuery(tableName, updatedValues);
                updatedValues[DefaultIdColumn] = id;

                sqlCommands.Append((updateQuery, updatedValues));
            }

            _sqlService.ExecuteInTransaction(sqlCommands, connectionString);
        }

        public void DeleteRecord(string tableName, object id, string connectionString)
        {
            _validationService.ValidateTableName(tableName);
            _validationService.ValidateId(id);

            var deleteQuery = _queryBuilderService.BuildDeleteQuery(tableName);
            _sqlService.ExecuteCommand(deleteQuery, connectionString, new { Id = id });
        }

        public void DeleteRecordsInTransaction(string tableName, IEnumerable<object> ids, string connectionString)
        {


            IEnumerable<(string sql, object parameters)> sqlCommands = null;

            // create a list of sql commands to execute
            foreach (var id in ids)
            {
                _validationService.ValidateId(id);

                var deleteQuery = _queryBuilderService.BuildDeleteQuery(tableName);
                sqlCommands.Append((deleteQuery, new { Id = id }));
            }


            _sqlService.ExecuteInTransaction(sqlCommands, connectionString);
        }


    }
}

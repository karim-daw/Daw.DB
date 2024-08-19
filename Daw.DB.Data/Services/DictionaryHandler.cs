using System.Collections.Generic;
using System.Linq;

namespace Daw.DB.Data.Services
{
    public interface IDictionaryHandler
    {
        void CreateTable(string tableName, Dictionary<string, string> columns, string connectionString);
        void AddRecord(string tableName, Dictionary<string, object> record, string connectionString);
        void AddRecordsInTransaction(string tableName, IEnumerable<Dictionary<string, object>> records, string connectionString);
        IEnumerable<dynamic> GetAllRecords(string tableName, string connectionString);
        dynamic GetRecordById(string tableName, object id, string connectionString);
        void UpdateRecord(string tableName, object id, Dictionary<string, object> updatedValues, string connectionString);
        void UpdateRecordsInTransaction(string tableName, IEnumerable<KeyValuePair<object, Dictionary<string, object>>> records, string connectionString);
        void DeleteRecord(string tableName, object id, string connectionString);
        void DeleteRecordsInTransaction(string tableName, IEnumerable<object> ids, string connectionString);
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

        public void AddRecord(string tableName, Dictionary<string, object> record, string connectionString)
        {
            _validationService.ValidateTableName(tableName);
            _validationService.ValidateRecord(record);

            var insertQuery = _queryBuilderService.BuildInsertQuery(tableName, record);
            _sqlService.ExecuteCommand(insertQuery, connectionString, record);
        }

        public void AddRecordsInTransaction(string tableName, IEnumerable<Dictionary<string, object>> records, string connectionString)
        {
            var sqlCommands = records.Select(record =>
            {
                _validationService.ValidateRecord(record);
                return _queryBuilderService.BuildInsertQuery(tableName, record);
            }).ToList();

            _sqlService.ExecuteInTransaction(sqlCommands, connectionString);
        }

        public IEnumerable<dynamic> GetAllRecords(string tableName, string connectionString)
        {
            _validationService.ValidateTableName(tableName);

            var selectQuery = _queryBuilderService.BuildSelectQuery(tableName);
            return _sqlService.ExecuteQuery(selectQuery, connectionString);
        }

        public dynamic GetRecordById(string tableName, object id, string connectionString)
        {
            _validationService.ValidateTableName(tableName);
            _validationService.ValidateId(id);

            var whereClause = $"{DefaultIdColumn} = @Id";
            var selectQuery = _queryBuilderService.BuildSelectQuery(tableName, whereClause);
            return _sqlService.ExecuteQuery(selectQuery, connectionString, new { Id = id }).FirstOrDefault();
        }

        public void UpdateRecord(string tableName, object id, Dictionary<string, object> updatedValues, string connectionString)
        {
            _validationService.ValidateTableName(tableName);
            _validationService.ValidateRecord(updatedValues);
            _validationService.ValidateId(id);

            var updateQuery = _queryBuilderService.BuildUpdateQuery(tableName, updatedValues);
            updatedValues[DefaultIdColumn] = id;
            _sqlService.ExecuteCommand(updateQuery, connectionString, updatedValues);
        }

        public void UpdateRecordsInTransaction(string tableName, IEnumerable<KeyValuePair<object, Dictionary<string, object>>> records, string connectionString)
        {
            var sqlCommands = records.Select(record =>
            {
                var id = record.Key;
                var updatedValues = record.Value;

                _validationService.ValidateId(id);
                _validationService.ValidateRecord(updatedValues);

                var updateQuery = _queryBuilderService.BuildUpdateQuery(tableName, updatedValues);
                updatedValues[DefaultIdColumn] = id;

                return updateQuery;
            }).ToList();

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
            var sqlCommands = ids.Select(id =>
            {
                _validationService.ValidateId(id);
                return _queryBuilderService.BuildDeleteQuery(tableName);
            }).ToList();

            _sqlService.ExecuteInTransaction(sqlCommands, connectionString);
        }
    }
}

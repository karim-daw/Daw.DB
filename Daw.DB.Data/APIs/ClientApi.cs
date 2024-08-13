using Daw.DB.Data.Interfaces;
using System;
using System.Collections.Generic;

namespace Daw.DB.Data.APIs
{
    public class ClientApi : IClientApi
    {
        private IDatabaseConnectionFactory _connectionFactory;
        private readonly ISqlService _sqlService;
        private readonly IJsonHandler _jsonHandler;
        private readonly IDictionaryHandler _dictionaryHandler;
        private readonly IServiceProvider _serviceProvider;

        public ClientApi(
            IDatabaseConnectionFactory connectionFactory,
            ISqlService sqlService,
            IJsonHandler jsonHandler,
            IDictionaryHandler dictionaryHandler,
            IServiceProvider serviceProvider)
        {
            _connectionFactory = connectionFactory;
            _sqlService = sqlService;
            _jsonHandler = jsonHandler;
            _dictionaryHandler = dictionaryHandler;
            _serviceProvider = serviceProvider;
        }

        public void InitializeDatabase(string connectionString)
        {
            // Create the database connection to ensure the database file is created
            using (var db = _connectionFactory.CreateConnection(connectionString))
            {
                db.Open();
            }
        }


        public void CreateTable(string tableName, Dictionary<string, string> columns, string connectionString)
        {
            _dictionaryHandler.CreateTable(tableName, columns, connectionString);
        }

        public void AddEntityRecord<T>(string tableName, T record, string connectionString) where T : class
        {
            var entityHandler = GetEntityHandler<T>();
            entityHandler.AddRecord(record, connectionString);
        }

        public void AddDictionaryRecord(string tableName, Dictionary<string, object> record, string connectionString)
        {
            _dictionaryHandler.AddRecord(tableName, record, connectionString);
        }

        public void AddRecordFromJson(string tableName, string jsonRecord, string connectionString)
        {
            _jsonHandler.AddRecordFromJson(tableName, jsonRecord, connectionString);
        }

        public void AddTableFromJson(string tableName, string jsonSchema, string connectionString)
        {
            _jsonHandler.AddTableFromJson(tableName, jsonSchema, connectionString);
        }

        public IEnumerable<T> GetAllEntityRecords<T>(string tableName, string connectionString) where T : class
        {
            var entityHandler = GetEntityHandler<T>();
            return entityHandler.GetAllRecords(connectionString);
        }

        public IEnumerable<dynamic> GetAllDictionaryRecords(string tableName, string connectionString)
        {
            return _dictionaryHandler.GetAllRecords(tableName, connectionString);
        }

        public T GetEntityRecordById<T>(string tableName, object id, string connectionString) where T : class
        {
            var entityHandler = GetEntityHandler<T>();
            return entityHandler.GetRecordById(id, connectionString);
        }

        public dynamic GetDictionaryRecordById(string tableName, object id, string connectionString)
        {
            return _dictionaryHandler.GetRecordById(tableName, id, connectionString);
        }

        public void UpdateEntityRecord<T>(string tableName, T record, string connectionString) where T : class
        {
            var entityHandler = GetEntityHandler<T>();
            entityHandler.UpdateRecord(record, connectionString);
        }

        public void UpdateDictionaryRecord(string tableName, object id, Dictionary<string, object> record, string connectionString)
        {
            _dictionaryHandler.UpdateRecord(tableName, id, record, connectionString);
        }

        public void UpdateRecordFromJson(string tableName, object id, string jsonRecord, string connectionString)
        {
            _jsonHandler.UpdateRecordFromJson(tableName, id, jsonRecord, connectionString);
        }

        public void DeleteRecord(string tableName, object id, string connectionString)
        {
            _dictionaryHandler.DeleteRecord(tableName, id, connectionString);
        }

        public IEnumerable<dynamic> ExecuteQuery(string sql, string connectionString, object parameters = null)
        {
            return _sqlService.ExecuteQuery(sql, connectionString, parameters);
        }

        public void ExecuteCommand(string sql, string connectionString, object parameters = null)
        {
            _sqlService.ExecuteCommand(sql, connectionString, parameters);
        }

        private IEntityHandler<T> GetEntityHandler<T>() where T : class
        {
            return (IEntityHandler<T>)_serviceProvider.GetService(typeof(IEntityHandler<T>));
        }
    }
}

using Daw.DB.Data.Interfaces;
using Daw.DB.Data.Services;
using System;
using System.Collections.Generic;

namespace Daw.DB.Data.APIs
{
    public interface IGenericClientApi
    {
        void InitializeDatabase(string connectionString);
        void CreateTable(string tableName, Dictionary<string, string> columns, string connectionString);
        void AddEntityRecord<T>(string tableName, T record, string connectionString) where T : class, IEntity;
        void AddDictionaryRecord(string tableName, Dictionary<string, object> record, string connectionString);
        void AddRecordFromJson(string tableName, string jsonRecord, string connectionString);
        void AddTableFromJson(string tableName, string jsonSchema, string connectionString);
        IEnumerable<T> GetAllEntityRecords<T>(string tableName, string connectionString) where T : class, IEntity;
        IEnumerable<dynamic> GetAllDictionaryRecords(string tableName, string connectionString);
        T GetEntityRecordById<T>(string tableName, object id, string connectionString) where T : class, IEntity;
        dynamic GetDictionaryRecordById(string tableName, object id, string connectionString);
        void UpdateEntityRecord<T>(string tableName, object id, T record, string connectionString) where T : class, IEntity;
        void UpdateDictionaryRecord(string tableName, object id, Dictionary<string, object> record, string connectionString);
        void UpdateRecordFromJson(string tableName, object id, string jsonRecord, string connectionString);
        void DeleteRecord<T>(string tableName, object id, string connectionString) where T : class, IEntity;
        IEnumerable<dynamic> ExecuteQuery(string sql, string connectionString, object parameters = null);
        void ExecuteCommand(string sql, string connectionString, object parameters = null);
    }
    public class GenericClientApi : IGenericClientApi
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;
        private readonly ISqlService _sqlService;
        private readonly IJsonHandler _jsonHandler;
        private readonly IDictionaryHandler _dictionaryHandler;
        private readonly IServiceProvider _serviceProvider;

        public GenericClientApi(
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

        public void AddEntityRecord<T>(string tableName, T record, string connectionString) where T : class, IEntity
        {
            // Get the entity handler for the given entity type
            var entityHandler = GetEntityHandler<T>();
            entityHandler.AddRecord(tableName, record, connectionString);
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

        public IEnumerable<T> GetAllEntityRecords<T>(string tableName, string connectionString) where T : class, IEntity
        {
            var entityHandler = GetEntityHandler<T>();
            return entityHandler.GetAllRecords(tableName, connectionString);
        }

        public IEnumerable<dynamic> GetAllDictionaryRecords(string tableName, string connectionString)
        {
            return _dictionaryHandler.GetAllRecords(tableName, connectionString);
        }

        public T GetEntityRecordById<T>(string tableName, object id, string connectionString) where T : class, IEntity
        {
            var entityHandler = GetEntityHandler<T>();
            return entityHandler.GetRecordById(tableName, id, connectionString);
        }

        public dynamic GetDictionaryRecordById(string tableName, object id, string connectionString)
        {
            return _dictionaryHandler.GetRecordById(tableName, id, connectionString);
        }

        public void UpdateEntityRecord<T>(string tableName, object id, T record, string connectionString) where T : class, IEntity
        {
            var entityHandler = GetEntityHandler<T>();
            entityHandler.UpdateRecord(tableName, id, record, connectionString);
        }

        public void UpdateDictionaryRecord(string tableName, object id, Dictionary<string, object> record, string connectionString)
        {
            _dictionaryHandler.UpdateRecord(tableName, id, record, connectionString);
        }

        public void UpdateRecordFromJson(string tableName, object id, string jsonRecord, string connectionString)
        {
            _jsonHandler.UpdateRecordFromJson(tableName, id, jsonRecord, connectionString);
        }

        public void DeleteRecord<T>(string tableName, object id, string connectionString) where T : class, IEntity
        {
            var entityHandler = GetEntityHandler<T>();
            entityHandler.DeleteRecord(tableName, id, connectionString);
        }

        public IEnumerable<dynamic> ExecuteQuery(string sql, string connectionString, object parameters = null)
        {
            return _sqlService.ExecuteQuery(sql, connectionString, parameters);
        }

        public void ExecuteCommand(string sql, string connectionString, object parameters = null)
        {
            _sqlService.ExecuteCommand(sql, connectionString, parameters);
        }

        private IEntityHandler<T> GetEntityHandler<T>() where T : class, IEntity
        {
            return (IEntityHandler<T>)_serviceProvider.GetService(typeof(IEntityHandler<T>));
        }

    }
}

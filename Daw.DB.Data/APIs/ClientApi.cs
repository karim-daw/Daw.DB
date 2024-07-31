using Daw.DB.Data.Interfaces;
using Daw.DB.Data.Services;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

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

        public void SetDatabasePath(string databasePath)
        {
            _connectionFactory = new SQLiteConnectionFactory(databasePath);
        }

        // Method to explicitly create and initialize the database
        public void InitializeDatabase(string databaseName)
        {
            string databasePath = $"{databaseName}.db";
            if (!File.Exists(databasePath))
            {
                SQLiteConnection.CreateFile(databasePath);
            }
        }

        public void CreateTable(string tableName, Dictionary<string, string> columns)
        {
            _dictionaryHandler.CreateTable(tableName, columns);
        }

        public void AddEntityRecord<T>(string tableName, T record) where T : class
        {
            var entityHandler = GetEntityHandler<T>();
            entityHandler.AddRecord(record);
        }

        public void AddDictionaryRecord(string tableName, Dictionary<string, object> record)
        {
            _dictionaryHandler.AddRecord(tableName, record);
        }

        public void AddRecordFromJson(string tableName, string jsonRecord)
        {
            _jsonHandler.AddRecordFromJson(tableName, jsonRecord);
        }

        public void AddTableFromJson(string tableName, string jsonSchema)
        {
            _jsonHandler.AddTableFromJson(tableName, jsonSchema);
        }

        public IEnumerable<T> GetAllEntityRecords<T>(string tableName) where T : class
        {
            var entityHandler = GetEntityHandler<T>();
            return entityHandler.GetAllRecords();
        }

        public IEnumerable<dynamic> GetAllDictionaryRecords(string tableName)
        {
            return _dictionaryHandler.GetAllRecords(tableName);
        }

        public T GetEntityRecordById<T>(string tableName, object id) where T : class
        {
            var entityHandler = GetEntityHandler<T>();
            return entityHandler.GetRecordById(id);
        }

        public dynamic GetDictionaryRecordById(string tableName, object id)
        {
            return _dictionaryHandler.GetRecordById(tableName, id);
        }

        public void UpdateEntityRecord<T>(string tableName, T record) where T : class
        {
            var entityHandler = GetEntityHandler<T>();
            entityHandler.UpdateRecord(record);
        }

        public void UpdateDictionaryRecord(string tableName, object id, Dictionary<string, object> record)
        {
            _dictionaryHandler.UpdateRecord(tableName, id, record);
        }

        public void UpdateRecordFromJson(string tableName, object id, string jsonRecord)
        {
            _jsonHandler.UpdateRecordFromJson(tableName, id, jsonRecord);
        }

        public void DeleteRecord(string tableName, object id)
        {
            _dictionaryHandler.DeleteRecord(tableName, id);
        }

        public IEnumerable<dynamic> ExecuteQuery(string sql, object parameters = null)
        {
            return _sqlService.ExecuteQuery(sql, parameters);
        }

        public void ExecuteCommand(string sql, object parameters = null)
        {
            _sqlService.ExecuteCommand(sql, parameters);
        }

        private IEntityHandler<T> GetEntityHandler<T>() where T : class
        {
            return (IEntityHandler<T>)_serviceProvider.GetService(typeof(IEntityHandler<T>));
        }
    }
}

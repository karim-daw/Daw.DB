using Dapper;
using Daw.DB.Data.Interfaces;
using System.Collections.Generic;

namespace Daw.DB.Data.Services
{
    public class EntityHandler<T> : IEntityHandler<T> where T : class, IEntity
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;
        private readonly string _tableName;

        public EntityHandler(IDatabaseConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
            _tableName = typeof(T).Name;
        }

        public void CreateTable(Dictionary<string, string> columns, string connectionString)
        {
            using (var db = _connectionFactory.CreateConnection(connectionString))
            {
                db.Open();
                var columnsDefinition = string.Join(", ", columns);
                var createTableQuery = $"CREATE TABLE IF NOT EXISTS {_tableName} ({columnsDefinition})";
                db.Execute(createTableQuery);
            }
        }

        public void AddRecord(T record, string connectionString)
        {
            using (var db = _connectionFactory.CreateConnection(connectionString))
            {
                db.Open();
                var insertQuery = $"INSERT INTO {_tableName} (Name) VALUES (@Name)";
                db.Execute(insertQuery, record);
            }
        }

        public IEnumerable<T> GetAllRecords(string connectionString)
        {
            using (var db = _connectionFactory.CreateConnection(connectionString))
            {
                db.Open();
                var selectQuery = $"SELECT * FROM {_tableName}";
                return db.Query<T>(selectQuery);
            }
        }

        public T GetRecordById(object id, string connectionString)
        {
            using (var db = _connectionFactory.CreateConnection(connectionString))
            {
                db.Open();
                var selectQuery = $"SELECT * FROM {_tableName} WHERE Id = @Id";
                return db.QuerySingleOrDefault<T>(selectQuery, new { Id = id });
            }
        }

        public void UpdateRecord(T record, string connectionString)
        {
            using (var db = _connectionFactory.CreateConnection(connectionString))
            {
                db.Open();
                var updateQuery = $"UPDATE {_tableName} SET Name = @Name WHERE Id = @Id";
                db.Execute(updateQuery, record);
            }
        }

        public void DeleteRecord(object id, string connectionString)
        {
            using (var db = _connectionFactory.CreateConnection(connectionString))
            {
                db.Open();
                var deleteQuery = $"DELETE FROM {_tableName} WHERE Id = @Id";
                db.Execute(deleteQuery, new
                {
                    Id = id
                });
            }
        }
    }
}

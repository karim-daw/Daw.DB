using Daw.DB.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Daw.DB.Services
{
    public class EntityService : IEntityService
    {
        private readonly IDataService _dataService;
        private readonly string _connectionString;
        private readonly ILogger<EntityService> _logger;

        public EntityService(IDataService dataService, ILogger<EntityService> logger)
        {
            _dataService = dataService;
            _connectionString = _dataService.GetConnectionString();
            _logger = logger;
        }

        public void CreateEntity(string entityName, Dictionary<string, string> entityFields)
        {
            _logger.LogInformation("Attempting to create entity: {EntityName}", entityName);

            if (!EntityExists(entityName))
            {
                _logger.LogInformation("Entity does not exist. Creating new entity: {EntityName}", entityName);

                using var connection = new SqliteConnection(_connectionString);
                connection.Open();
                var command = connection.CreateCommand();
                var fields = string.Join(", ", entityFields.Select(f => $"{f.Key} {f.Value}"));
                command.CommandText = $"CREATE TABLE IF NOT EXISTS [{entityName}] (id INTEGER PRIMARY KEY, {fields})";

                try
                {
                    command.ExecuteNonQuery();
                    _logger.LogInformation("Entity created successfully: {EntityName}", entityName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create entity: {EntityName}", entityName);
                }
            }
            else
            {
                _logger.LogInformation("Entity already exists: {EntityName}", entityName);
            }
        }

        public void CreateManyToManyRelation(string firstEntity, string secondEntity, string relationTable, Dictionary<string, string> relationTableFields)
        {
            _logger.LogInformation("Attempting to create many-to-many relation: {RelationTable}", relationTable);

            var fields = new Dictionary<string, string>(relationTableFields)
            {
                { $"{firstEntity}_id", "INTEGER" },
                { $"{secondEntity}_id", "INTEGER" },
                { $"FOREIGN KEY ({firstEntity}_id)", $"REFERENCES [{firstEntity}](id)" },
                { $"FOREIGN KEY ({secondEntity}_id)", $"REFERENCES [{secondEntity}](id)" }
            };
            CreateEntity(relationTable, fields);
        }

        public void CreateOneToManyRelation(string parentEntity, string childEntity, string foreignKey)
        {
            _logger.LogInformation("Attempting to create one-to-many relation from {ParentEntity} to {ChildEntity}", parentEntity, childEntity);

            var childFields = new Dictionary<string, string>
            {
                { foreignKey, $"INTEGER, FOREIGN KEY({foreignKey}) REFERENCES {parentEntity}(id)" }
            };
            UpdateSchema(childEntity, childFields);
        }

        public void DeleteEntity(string entityName)
        {
            _logger.LogInformation("Attempting to delete entity: {EntityName}", entityName);

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = $"DROP TABLE IF EXISTS [{entityName}]";

            try
            {
                command.ExecuteNonQuery();
                _logger.LogInformation("Entity deleted successfully: {EntityName}", entityName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete entity: {EntityName}", entityName);
            }
        }

        public bool EntityExists(string entityName)
        {
            _logger.LogInformation("Checking if entity exists: {EntityName}", entityName);

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = $"SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name=@entityName";
            command.Parameters.AddWithValue("@entityName", entityName);

            try
            {
                var count = Convert.ToInt32(command.ExecuteScalar());
                _logger.LogInformation("Entity existence check completed. Entity exists: {EntityExists}", count > 0);
                return count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check if entity exists: {EntityName}", entityName);
                throw;
            }
        }

        public void UpdateSchema(string entityName, Dictionary<string, string> newFields)
        {
            _logger.LogInformation("Attempting to update schema for entity: {EntityName}", entityName);

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            foreach (var field in newFields)
            {
                if (!SqliteDataService.ColumnExists(connection, entityName, field.Key))
                {
                    var command = connection.CreateCommand();
                    command.CommandText = $"ALTER TABLE [{entityName}] ADD COLUMN {field.Key} {field.Value}";

                    try
                    {
                        command.ExecuteNonQuery();
                        _logger.LogInformation("Added column {ColumnName} to entity {EntityName}", field.Key, entityName);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to add column {ColumnName} to entity {EntityName}", field.Key, entityName);
                    }
                }
            }
        }
    }
}

using System.Collections.Generic;
using System.Data.Common;
using Microsoft.Data.Sqlite;

namespace Daw.DB.Interfaces
{
    public interface IDataService
    {
        /// <summary>
        /// Create a new database with the given connection string
        /// </summary>
        /// <param name="connectionString"></param>
        void CreateDatabase(string connectionString);

        /// <summary>
        /// Get a connection to the database
        /// </summary>
        /// <returns></returns>
        SqliteConnection GetConnection();

        /// <summary>
        /// Check if the column exists in the table
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        bool ColumnExists(string tableName, string columnName);
    }

    /// <summary>
    /// Service for working with entities, implement CRUD operations
    /// and relations between entities
    /// </summary>
    public interface IEntityService
    {
        /// <summary>
        /// Create a new entity with the given fields
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="entityFields"></param>
        void CreateEntity(string entityName, Dictionary<string, string> entityFields);

        /// <summary>
        /// Update the schema of the entity with the given name
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="newFields"></param>
        void UpdateSchema(string entityName, Dictionary<string, string> newFields);

        /// <summary>
        /// Check if the entity with the given name exists
        /// </summary>
        /// <param name="entityName"></param>
        /// <returns></returns>
        bool EntityExists(string entityName);

        /// <summary>
        /// Delete the entity with the given name
        /// </summary>
        /// <param name="entityName"></param>
        void DeleteEntity(string entityName);

        /// <summary>
        /// Create a one-to-one relation between the first and second entities
        /// </summary>
        /// <param name="parentEntity"></param>
        /// <param name="childEntity"></param>
        /// <param name="foreignKey"></param>
        void CreateOneToManyRelation(string parentEntity, string childEntity, string foreignKey);

        /// <summary>
        /// Create a many-to-many relation between the first and second entities
        /// </summary>
        /// <param name="firstEntity"></param>
        /// <param name="secondEntity"></param>
        /// <param name="relationTable"></param>
        /// <param name="relationTableFields"></param>
        void CreateManyToManyRelation(string firstEntity, string secondEntity, string relationTable, Dictionary<string, string> relationTableFields);
    }

    /// <summary>
    /// Service for working with records, implement CRUD operations
    /// </summary>
    public interface IRecordService
    {
        /// <summary>
        /// Add a record to the entity with the given name
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="record"></param>
        void AddRecord(string entityName, Dictionary<string, object> record);

        /// <summary>
        /// Get all records from the entity with the given name
        /// </summary>
        /// <param name="entityName"></param>
        /// <returns></returns>
        List<Dictionary<string, object>> GetRecords(string entityName);

        /// <summary>
        /// Update a record in the entity with the given name
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="record"></param>
        void UpdateRecord(string entityName, Dictionary<string, object> record);

        /// <summary>
        /// Delete a record from the entity with the given name
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="record"></param>
        void DeleteRecord(string entityName, Dictionary<string, object> record);
    }

    /// <summary>
    /// Service for working with queries, implement query execution
    /// </summary>
    public interface IQueryService
    {
        /// <summary>
        /// Execute the given query and return the result
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        List<Dictionary<string, object>> ExecuteQuery(string query);
    }
}

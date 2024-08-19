using Daw.DB.Data.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Daw.DB.Data.Services
{

    public interface IEntityHandler<T> where T : class, IEntity
    {
        void CreateTable(T entity, string connectionString);
        void AddRecord(string tableName, T record, string connectionString);
        IEnumerable<T> GetAllRecords(string tableName, string connectionString);
        T GetRecordById(string tableName, object id, string connectionString);
        void UpdateRecord(string tableName, object id, T updatedValues, string connectionString);
        void DeleteRecord(string tableName, object id, string connectionString);
    }

    public class EntityHandler<T> : IEntityHandler<T> where T : class, IEntity
    {
        private readonly ISqlService _sqlService;
        private readonly ILogger<EntityHandler<T>> _logger;

        // Assuming "Id" is the primary key column name
        private const string DefaultIdColumn = "Id";

        public EntityHandler(ISqlService sqlService, ILogger<EntityHandler<T>> logger)
        {
            _sqlService = sqlService;
            _logger = logger;
        }

        public void CreateTable(T entity, string connectionString)
        {
            string tableName;
            string columnsDefinition;
            string createTableQuery;

            if (entity == null)
            {
                throw new System.Exception("Entity is null.");
            }

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new System.Exception("Connection string is empty.");
            }

            // Get the table name from the entity type
            try
            {
                tableName = GetTableName(entity);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error getting table name.");
                throw new System.Exception($"Error getting table name: {ex.Message}");
            }

            // Check if table already exists
            if (TableExists(tableName, connectionString))
            {
                throw new System.Exception($"Table {tableName} already exists.");
            }

            // Get the column definitions from the entity type
            try
            {
                columnsDefinition = ColumnDefinitionsFromEntity(entity);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error getting column definitions for table {TableName}.", tableName);
                throw new System.Exception($"Error getting column definitions: {ex.Message}");
            }

            // Create the create table query
            try
            {
                createTableQuery = CreateTableQuery(tableName, columnsDefinition);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error creating table query for table {TableName}.", tableName);
                throw new System.Exception("Error creating table query.");
            }

            // Finally, execute the create table query
            try
            {
                _sqlService.ExecuteCommand(createTableQuery, connectionString);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error creating table {TableName}.", tableName);
                throw new System.Exception($"Error creating table {tableName}: {ex.Message}");
            }
        }

        public void AddRecord(string tableName, T record, string connectionString)
        {
            tableName = ValidateAndFormatTableName(tableName);

            var columns = string.Join(", ", record.GetType().GetProperties().Select(p => ValidateAndFormatColumnName(p.Name)));
            var values = string.Join(", ", record.GetType().GetProperties().Select(p => $"@{p.Name}"));
            var insertQuery = $"INSERT INTO {tableName} ({columns}) VALUES ({values})";

            try
            {
                _sqlService.ExecuteCommand(insertQuery, connectionString, record);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error adding record to table {TableName}.", tableName);
                throw new System.Exception($"Error adding record to table {tableName}: {ex.Message}");
            }
        }

        public IEnumerable<T> GetAllRecords(string tableName, string connectionString)
        {
            tableName = ValidateAndFormatTableName(tableName);
            var selectQuery = $"SELECT * FROM {tableName}";

            try
            {
                return _sqlService.ExecuteQuery<T>(selectQuery, connectionString);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error retrieving records from table {TableName}.", tableName);
                throw new System.Exception($"Error retrieving records from table {tableName}: {ex.Message}");
            }
        }

        public T GetRecordById(string tableName, object id, string connectionString)
        {
            tableName = ValidateAndFormatTableName(tableName);
            var selectQuery = $"SELECT * FROM {tableName} WHERE {DefaultIdColumn} = @Id";

            try
            {
                return _sqlService.ExecuteQuery<T>(selectQuery, connectionString, new { Id = id }).FirstOrDefault();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error retrieving record by ID from table {TableName}.", tableName);
                throw new System.Exception($"Error retrieving record from table {tableName}: {ex.Message}");
            }
        }

        public void UpdateRecord(string tableName, object id, T updatedValues, string connectionString)
        {
            tableName = ValidateAndFormatTableName(tableName);

            // Get the properties of the updatedValues object
            var properties = updatedValues.GetType().GetProperties();

            // Create the set clause
            var setClause = string.Join(", ", properties.Select(p => $"{ValidateAndFormatColumnName(p.Name)} = @{p.Name}"));

            // Create the update query
            var updateQuery = $"UPDATE {tableName} SET {setClause} WHERE {DefaultIdColumn} = @Id";

            try
            {
                _sqlService.ExecuteCommand(updateQuery, connectionString, updatedValues);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error updating record in table {TableName}.", tableName);
                throw new System.Exception($"Error updating record in table {tableName}: {ex.Message}");
            }
        }

        public void DeleteRecord(string tableName, object id, string connectionString)
        {
            tableName = ValidateAndFormatTableName(tableName);
            var deleteQuery = $"DELETE FROM {tableName} WHERE {DefaultIdColumn} = @Id";

            try
            {
                _sqlService.ExecuteCommand(deleteQuery, connectionString, new { Id = id });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error deleting record from table {TableName}.", tableName);
                throw new System.Exception($"Error deleting record from table {tableName}: {ex.Message}");
            }
        }

        /// <summary>
        /// Create the create table query
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnsDefinition"></param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        private static string CreateTableQuery(string tableName, string columnsDefinition)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                throw new System.Exception("Table name is empty.");
            }

            if (string.IsNullOrEmpty(columnsDefinition))
            {
                throw new System.Exception("Columns definition is empty.");
            }

            if (columnsDefinition.Contains(DefaultIdColumn))
            {
                throw new System.Exception($"Columns definition contains {DefaultIdColumn}. This is a reserved keyword.");
            }

            return $"CREATE TABLE {tableName} ({columnsDefinition})";
        }

        /// <summary>
        /// Get the column definitions from the entity type
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private static string ColumnDefinitionsFromEntity(T entity)
        {
            if (entity == null)
            {
                throw new System.Exception("Entity is null.");
            }

            if (entity.GetType().GetProperties().Length == 0)
            {
                throw new System.Exception("Entity has no properties.");
            }

            if (entity.GetType().GetProperties().Any(p => p.Name == DefaultIdColumn))
            {
                throw new System.Exception($"Entity has a property named {DefaultIdColumn}. This is a reserved keyword.");
            }

            // Get SQL type from property type using GetSqliteType method and add id column which auto increments
            var columns = entity.GetType().GetProperties()
                .ToDictionary(p => ValidateAndFormatColumnName(p.Name), p => GetSqliteType(p.PropertyType));

            // Add the Id column
            columns.Add(DefaultIdColumn, "INTEGER PRIMARY KEY AUTOINCREMENT");

            // Join the columns definitions 
            var columnsDefinition = string.Join(", ", columns.Select(kv => $"{kv.Key} {kv.Value}"));

            return columnsDefinition;
        }

        /// <summary>
        /// Check if a table exists in the database
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private bool TableExists(string tableName, string connectionString)
        {
            tableName = ValidateAndFormatTableName(tableName);
            string tableExistsQuery = "SELECT name FROM sqlite_master WHERE type='table' AND name = @TableName;";
            bool tableExists = _sqlService.ExecuteQuery(tableExistsQuery, connectionString, new { TableName = tableName }).Any();
            return tableExists;
        }

        /// <summary>
        /// Get the table name from the entity type
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        private static string GetTableName(T entity)
        {
            var tableName = entity.GetType().Name;
            if (string.IsNullOrEmpty(tableName))
            {
                throw new System.Exception("Entity type name is empty.");
            }

            return ValidateAndFormatTableName(tableName);
        }

        /// <summary>
        /// Get the SQLite type from a .NET type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        private static string GetSqliteType(Type type)
        {
            if (type == typeof(int) || type == typeof(long))
                return "INTEGER";
            if (type == typeof(string))
                return "TEXT";
            if (type == typeof(bool))
                return "BOOLEAN";
            if (type == typeof(double) || type == typeof(float))
                return "REAL";
            if (type == typeof(byte[]))
                return "BLOB";

            throw new NotSupportedException($"Type {type.Name} is not supported by SQLite.");
        }

        /// <summary>
        /// Validate and format a table name to prevent SQL injection
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private static string ValidateAndFormatTableName(string tableName)
        {
            if (!Regex.IsMatch(tableName, @"^[a-zA-Z0-9_]+$"))
            {
                throw new ArgumentException("Invalid table name.");
            }
            return tableName;
        }

        /// <summary>
        /// Validate and format a column name to prevent SQL injection
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        private static string ValidateAndFormatColumnName(string columnName)
        {
            if (!Regex.IsMatch(columnName, @"^[a-zA-Z0-9_]+$"))
            {
                throw new ArgumentException("Invalid column name.");
            }
            return columnName;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Daw.DB.Data.Services
{
    public interface IValidationService
    {
        /// <summary>
        /// Validates the table name by checking if it contains only alphanumeric characters and underscores.
        /// </summary>
        /// <param name="tableName"></param>
        void ValidateTableName(string tableName);

        /// <summary>
        /// Validates the column names and types. Checks if the column names contain only alphanumeric characters and underscores.
        /// </summary>
        /// <param name="columns"></param>
        void ValidateColumns(Dictionary<string, string> columns);

        /// <summary>
        /// Validates the record by checking if the column names contain only alphanumeric characters and underscores.
        /// </summary>
        /// <param name="record"></param>
        void ValidateRecord(Dictionary<string, object> record);

        /// <summary>
        /// Validates the id by checking if it is not null. 
        /// </summary>
        /// <param name="id"></param>
        void ValidateId(object id);

        /// <summary>
        /// Validates the record against the columns and their types. This uses the column metadata to validate the record.
        /// </summary>
        /// <param name="record"></param>
        /// <param name="columnMetadata"></param>
        void ValidateRecordAgainstColumns(Dictionary<string, object> record, Dictionary<string, string> columnMetadata); // New method for validating record against columns and their types
    }

    public class ValidationService : IValidationService
    {
        private static readonly HashSet<string> ValidSqlTypes = new HashSet<string> { "INTEGER", "TEXT", "REAL", "BLOB", "NUMERIC" };

        public void ValidateTableName(string tableName)
        {
            if (!Regex.IsMatch(tableName, @"^[a-zA-Z0-9_]+$"))
            {
                throw new ArgumentException("Invalid table name.");
            }
        }

        /// <summary>
        /// Validates the column names and types.
        /// </summary>
        /// <param name="columns"></param>
        /// <exception cref="ArgumentException"></exception>
        public void ValidateColumns(Dictionary<string, string> columns)
        {
            foreach (var column in columns)
            {
                if (!Regex.IsMatch(column.Key, @"^[a-zA-Z0-9_]+$"))
                {
                    throw new ArgumentException($"Invalid column name: {column.Key}");
                }
                if (!IsValidSqlType(column.Value))
                {
                    throw new ArgumentException($"Invalid column type: {column.Value}");
                }
            }
        }

        public void ValidateRecord(Dictionary<string, object> record)
        {
            foreach (var key in record.Keys)
            {
                if (!Regex.IsMatch(key, @"^[a-zA-Z0-9_]+$"))
                {
                    throw new ArgumentException($"Invalid column name in record: {key}");
                }
            }
        }

        public void ValidateId(object id)
        {
            if (id == null)
            {
                throw new ArgumentException("Id cannot be null.");
            }
        }

        public void ValidateRecordAgainstColumns(Dictionary<string, object> record, Dictionary<string, string> columnMetadata)
        {
            foreach (var key in record.Keys)
            {
                if (!columnMetadata.ContainsKey(key))
                {
                    throw new ArgumentException($"Invalid column name {key} in record.");
                }

                var expectedType = columnMetadata[key];
                var value = record[key];

                if (!ValidateType(expectedType, value))
                {
                    throw new ArgumentException($"Invalid data type for column {key}: Expected {expectedType} but got {value?.GetType().Name}");
                }
            }
        }

        private bool ValidateType(string expectedType, object value)
        {
            if (value == null)
            {
                return true; // Assuming NULL is allowed in the column
            }

            switch (expectedType.ToUpperInvariant())
            {
                case "INTEGER":
                    return value is int || value is long;
                case "REAL":
                    return value is float || value is double || value is decimal;
                case "TEXT":
                    return value is string;
                case "BLOB":
                    return value is byte[] || value is System.IO.Stream;
                case "NUMERIC":
                    return value is int || value is long || value is float || value is double || value is decimal;
                default:
                    return false;
            }
        }

        private bool IsValidSqlType(string type)
        {
            return ValidSqlTypes.Contains(type.ToUpper());
        }
    }
}

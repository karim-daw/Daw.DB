using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Daw.DB.Data.Services
{
    public interface IValidationService
    {
        void ValidateTableName(string tableName);
        void ValidateColumns(Dictionary<string, string> columns);
        void ValidateRecord(Dictionary<string, object> record);
        void ValidateId(object id);
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

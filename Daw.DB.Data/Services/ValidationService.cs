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

        private bool IsValidSqlType(string type)
        {
            return ValidSqlTypes.Contains(type.ToUpper());
        }
    }
}

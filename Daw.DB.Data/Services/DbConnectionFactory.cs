using Npgsql;
using System;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace Daw.DB.Data.Services
{

    public interface IDatabaseConnectionFactory
    {
        IDbConnection CreateConnection(string connectionString);
    }

    public interface ISQLiteConnectionFactory : IDatabaseConnectionFactory
    {
        IDbConnection CreateConnectionFromFilePath(string dbPath);
    }


    public class SQLiteConnectionFactory : ISQLiteConnectionFactory
    {

        static public string ConnectionString { get; set; }


        // Method to create a connection to an existing database using a connection string
        public IDbConnection CreateConnection(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("Connection string must be provided.", nameof(connectionString));
            }
            return new SQLiteConnection(connectionString);
        }

        // Optional method to create a connection to a database using a file path
        // This method is useful when you want to create a connection to a database file
        // that is local to the application
        public IDbConnection CreateConnectionFromFilePath(string dbPath)
        {
            if (string.IsNullOrWhiteSpace(dbPath))
            {
                throw new ArgumentException("Database path must be provided.", nameof(dbPath));
            }

            if (!File.Exists(dbPath))
            {
                throw new FileNotFoundException($"Database file not found at {dbPath}");
            }

            string connectionString = $"Data Source={dbPath};Version=3;";
            return CreateConnection(connectionString);
        }


        // extract db path from connection string
        public static string ExtractDbPath(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("Connection string must be provided.", nameof(connectionString));
            }

            var parts = connectionString.Split(';');
            foreach (var part in parts)
            {
                if (part.Contains("Data Source"))
                {
                    return part.Split('=')[1];
                }
            }

            throw new ArgumentException("Invalid connection string. Data Source not found.", nameof(connectionString));
        }


    }

    // posgres connection factory
    public class PostgresConnectionFactory : IDatabaseConnectionFactory
    {
        public IDbConnection CreateConnection(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("Connection string must be provided.", nameof(connectionString));
            }
            return new NpgsqlConnection(connectionString);
        }
    }

}

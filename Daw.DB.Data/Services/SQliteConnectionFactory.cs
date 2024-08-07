using Daw.DB.Data.Interfaces;
using System;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace Daw.DB.Data.Services
{
    public class SQLiteConnectionFactory : IDatabaseConnectionFactory, IDatabaseCreationFactory
    {
        // Constructor that takes a default connection string (optional)

        // Method to create a new SQLite database file
        public void CreateDatabase(string dbPath)
        {
            if (string.IsNullOrWhiteSpace(dbPath))
            {
                throw new ArgumentException("Database path must be provided.", nameof(dbPath));
            }

            // If the database file does not exist, create it
            if (!File.Exists(dbPath))
            {
                SQLiteConnection.CreateFile(dbPath);
            }
            else
            {
                throw new IOException($"Database file already exists at {dbPath}");
            }
        }

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
    }
}

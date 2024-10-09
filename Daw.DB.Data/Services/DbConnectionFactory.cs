using Npgsql;
using System;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace Daw.DB.Data.Services
{
    public interface IPingable
    {
        // Method to check if the database is reachable
        bool Ping(string connectionString);
    }

    public interface IDatabaseConnectionFactory
    {
        IDbConnection CreateConnection();
    }

    public interface ISQLiteConnectionFactory : IDatabaseConnectionFactory, IPingable
    {
        IDbConnection CreateConnectionFromFilePath(string dbPath);
    }


    public class SQLiteConnectionFactory : ISQLiteConnectionFactory
    {

        private readonly IDatabaseContext _databaseContext;

        // static public string ConnectionString { get; set; }

        public SQLiteConnectionFactory(IDatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public bool Ping(string connectionString)
        {
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    return true;
                }

            }
            catch (Exception)
            {
                return false;
            }

        }


        // Method to create a connection to an existing database using a connection string
        public IDbConnection CreateConnection()
        {
            string connectionString = _databaseContext.ConnectionString;

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

            return CreateConnection();
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
        private readonly IDatabaseContext _databaseContext;

        public PostgresConnectionFactory(IDatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public IDbConnection CreateConnection()
        {
            string connectionString = _databaseContext.ConnectionString;
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("Connection string must be provided.", nameof(connectionString));
            }
            return new NpgsqlConnection(connectionString);
        }
    }

}

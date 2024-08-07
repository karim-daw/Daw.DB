﻿using Daw.DB.Data.Interfaces;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace Daw.DB.Data.Services
{
    public class SQLiteConnectionFactory : IDatabaseConnectionFactory
    {
        private string _connectionString;


        // Default path should be to desktop
        private const string DefaultPath = @"C:\Users\Public\Desktop";

        public SQLiteConnectionFactory()
        {
            // Initialize with a default connection string if needed
            _connectionString = "Data Source=:memory:;Version=3;"; // Default to an in-memory database
        }

        public void SetConnectionString(string dbPath)
        {
            if (!File.Exists(dbPath))
            {
                SQLiteConnection.CreateFile(dbPath);
            }

            _connectionString = $"Data Source={dbPath};Version=3;";
        }

        // todo this should take a path as an argument
        public IDbConnection CreateConnection()
        {
            return new SQLiteConnection(_connectionString);
        }
    }

}

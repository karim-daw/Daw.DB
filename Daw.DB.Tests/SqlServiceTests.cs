using Daw.DB.Data.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;

namespace Daw.DB.Tests {
    [TestClass]
    public class SqlServiceTests {
        private ISqlService _sqlService;
        private IDatabaseContext _databaseContext;

        private string _databaseFilePath;

        [TestInitialize]
        public void Setup() {
            // Load configuration from appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                // Adjust the file name and path as needed
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            // Configure services based on the loaded configuration
            var serviceProvider = ServiceConfiguration.ConfigureServices(configuration);
            _sqlService = serviceProvider.GetRequiredService<ISqlService>();
            _databaseContext = serviceProvider.GetRequiredService<IDatabaseContext>();

            // Set up a temporary database file path
            _databaseFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.db");

            // Set the connection string in the database context
            _databaseContext.ConnectionString = $"Data Source={_databaseFilePath};Version=3;";
        }

        [TestCleanup]
        public void Cleanup() {
            // Ensure all connections are closed before cleanup
            GC.Collect();
            GC.WaitForPendingFinalizers();

            if (File.Exists(_databaseFilePath)) {
                try {
                    File.Delete(_databaseFilePath);
                }
                catch (IOException ex) {
                    Console.WriteLine($"Failed to delete the file {_databaseFilePath}: {ex.Message}");
                }
            }
        }

        [TestMethod]
        public void CreateTable_Success() {
            // Arrange
            string sqlCreateTable = "CREATE TABLE TestTable (Name TEXT)";
            _sqlService.ExecuteCommand(sqlCreateTable);

            // Act
            var result = _sqlService.ExecuteQuery("SELECT name FROM sqlite_master WHERE type='table' AND name='TestTable'");

            // Assert
            Assert.AreEqual(1, result.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(SQLiteException))]
        public void WrongSql_Failure() {
            // Arrange: Incorrect SQL syntax to create a table
            string sql = "CREATE TABLES TestTable (Name TEXT)";

            // Act
            _sqlService.ExecuteCommand(sql);

            // Assert: Exception is expected
        }

        [TestMethod]
        public void InsertData_Success() {
            // Arrange
            string sqlCreateTable = "CREATE TABLE TestTable (Name TEXT)";
            _sqlService.ExecuteCommand(sqlCreateTable);

            // Act
            _sqlService.ExecuteCommand("INSERT INTO TestTable (Name) VALUES ('Test')");
            var result = _sqlService.ExecuteQuery("SELECT * FROM TestTable");

            // Assert
            Assert.AreEqual(1, result.Count());
        }

        [TestMethod]
        public void ExecuteInTransaction_Success() {
            // Arrange
            string sqlCreateTable = "CREATE TABLE TestTable (Name TEXT)";
            _sqlService.ExecuteCommand(sqlCreateTable);

            var sqlCommands = new List<(string sql, object parameters)>
            {
                ("INSERT INTO TestTable (Name) VALUES ('Test1')", null),
                ("INSERT INTO TestTable (Name) VALUES ('Test2')", null)
            };

            // Act
            _sqlService.ExecuteInTransaction(sqlCommands);
            var result = _sqlService.ExecuteQuery("SELECT * FROM TestTable");

            // Assert
            Assert.AreEqual(2, result.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void ExecuteInTransaction_Failure() {
            // Arrange
            string sqlCreateTable = "CREATE TABLE TestTable (Name TEXT)";
            _sqlService.ExecuteCommand(sqlCreateTable);

            var sqlCommands = new List<(string sql, object parameters)>
            {
                ("INSERT INTO TestTable (Name) VALUES ('Test1')", null),
                ("INVALID SQL COMMAND", null), // This will cause the transaction to fail
                ("INSERT INTO TestTable (Name) VALUES ('Test3')", null)
            };

            // Act
            _sqlService.ExecuteInTransaction(sqlCommands);

            // Assert: Exception is expected
        }
    }
}

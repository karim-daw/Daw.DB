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
    public class DictionaryHandlerTests {
        private IDictionaryHandler _dictionaryHandler;
        private IDatabaseContext _databaseContext;
        private string _databaseFilePath;

        [TestInitialize]
        public void Setup() {
            // Load configuration from appsettings.json
            var configuration = new ConfigurationBuilder()
                // Adjust the base path as needed
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            // Configure services based on the loaded configuration
            var serviceProvider = ServiceConfiguration.ConfigureServices(configuration);
            _dictionaryHandler = serviceProvider.GetRequiredService<IDictionaryHandler>();
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
        public void CreateTable_NewTable_CreatesTableSuccessfully() {
            // Arrange
            string tableName = "TestTable";
            var columns = new Dictionary<string, string>
            {
                { "Name", "TEXT" }
            };

            // Act
            _dictionaryHandler.CreateTable(tableName, columns);

            // Assert
            using (var connection = new SQLiteConnection(_databaseContext.ConnectionString)) {
                connection.Open();
                var query = $"PRAGMA table_info({tableName})";
                var command = connection.CreateCommand();
                command.CommandText = query;
                using (var reader = command.ExecuteReader()) {
                    Assert.IsTrue(reader.HasRows);
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreateTable_ExistingTable_ThrowsException() {
            // Arrange
            string tableName = "TestTable";
            var columns = new Dictionary<string, string>
            {
                { "Name", "TEXT" }
            };

            _dictionaryHandler.CreateTable(tableName, columns);

            // Act
            _dictionaryHandler.CreateTable(tableName, columns);

            // Assert: Expecting exception
        }

        [TestMethod]
        public void AddRecord_ValidRecord_AddsRecordSuccessfully() {
            // Arrange
            string tableName = "TestTable";
            var columns = new Dictionary<string, string>
            {
                { "Name", "TEXT" }
            };

            _dictionaryHandler.CreateTable(tableName, columns);

            var record = new Dictionary<string, object>
            {
                { "Name", "TestName" }
            };

            // Act
            _dictionaryHandler.AddRecord(tableName, record);

            // Assert
            var records = _dictionaryHandler.GetAllRecords(tableName).ToList();
            Assert.AreEqual(1, records.Count);
            Assert.AreEqual("TestName", records.First().Name);
        }

        [TestMethod]
        public void GetRecordById_ValidId_ReturnsRecord() {
            // Arrange
            string tableName = "TestTable";
            var columns = new Dictionary<string, string>
            {
                { "Name", "TEXT" }
            };

            _dictionaryHandler.CreateTable(tableName, columns);

            var record = new Dictionary<string, object>
            {
                { "Name", "TestName" }
            };

            _dictionaryHandler.AddRecord(tableName, record);

            // Act
            var retrievedRecord = _dictionaryHandler.GetRecordById(tableName, 1);

            // Assert
            Assert.IsNotNull(retrievedRecord);
            Assert.AreEqual("TestName", retrievedRecord.Name);
        }

        [TestMethod]
        public void UpdateRecord_ValidUpdate_UpdatesRecordSuccessfully() {
            // Arrange
            string tableName = "TestTable";
            var columns = new Dictionary<string, string>
            {
                { "Name", "TEXT" }
            };

            _dictionaryHandler.CreateTable(tableName, columns);

            var record = new Dictionary<string, object>
            {
                { "Name", "TestName" }
            };

            _dictionaryHandler.AddRecord(tableName, record);

            var updatedValues = new Dictionary<string, object>
            {
                { "Name", "UpdatedName" }
            };

            // Act
            _dictionaryHandler.UpdateRecord(tableName, 1, updatedValues);

            // Assert
            var retrievedRecord = _dictionaryHandler.GetRecordById(tableName, 1);
            Assert.IsNotNull(retrievedRecord);
            Assert.AreEqual("UpdatedName", retrievedRecord.Name);
        }

        [TestMethod]
        public void DeleteRecord_ValidId_DeletesRecordSuccessfully() {
            // Arrange
            string tableName = "TestTable";
            var columns = new Dictionary<string, string>
            {
                { "Name", "TEXT" }
            };

            _dictionaryHandler.CreateTable(tableName, columns);

            var record = new Dictionary<string, object>
            {
                { "Name", "TestName" }
            };

            _dictionaryHandler.AddRecord(tableName, record);

            // Act
            _dictionaryHandler.DeleteRecord(tableName, 1);

            // Assert
            var retrievedRecord = _dictionaryHandler.GetRecordById(tableName, 1);
            Assert.IsNull(retrievedRecord);
        }
    }
}

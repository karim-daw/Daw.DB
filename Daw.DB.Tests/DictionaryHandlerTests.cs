using Daw.DB.Data;
using Daw.DB.Data.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;

namespace Daw.DB.Tests
{
    [TestClass]
    public class DictionaryHandlerTests
    {
        private IDictionaryHandler _dictionaryHandler;
        private string _databaseFilePath;
        private string _connectionString;

        [TestInitialize]
        public void Setup()
        {
            // Configure the DI container and resolve the DictionaryHandler
            var serviceProvider = ServiceConfiguration.ConfigureServices();
            _dictionaryHandler = serviceProvider.GetRequiredService<IDictionaryHandler>();

            _databaseFilePath = Path.GetTempFileName();
            _connectionString = $"Data Source={_databaseFilePath};Version=3;";
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Ensure all connections are closed before cleanup
            GC.Collect();
            GC.WaitForPendingFinalizers();

            if (File.Exists(_databaseFilePath))
            {
                try
                {
                    File.Delete(_databaseFilePath);
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"Failed to delete the file {_databaseFilePath}: {ex.Message}");
                }
            }
        }

        [TestMethod]
        public void CreateTable_NewTable_CreatesTableSuccessfully()
        {
            // Arrange
            string tableName = "TestTable";
            var columns = new Dictionary<string, string>
            {
                { "Id", "INTEGER PRIMARY KEY" },
                { "Name", "TEXT" }
            };

            // Act
            _dictionaryHandler.CreateTable(tableName, columns, _connectionString);

            // Assert
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var query = $"PRAGMA table_info({tableName})";
                var command = connection.CreateCommand();
                command.CommandText = query;
                using (var reader = command.ExecuteReader())
                {
                    Assert.IsTrue(reader.HasRows);
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void CreateTable_ExistingTable_ThrowsException()
        {
            // Arrange
            string tableName = "TestTable";
            var columns = new Dictionary<string, string>
            {
                { "Id", "INTEGER PRIMARY KEY" },
                { "Name", "TEXT" }
            };

            _dictionaryHandler.CreateTable(tableName, columns, _connectionString);

            // Act
            _dictionaryHandler.CreateTable(tableName, columns, _connectionString);

            // Assert: Expecting exception
        }

        [TestMethod]
        public void AddRecord_ValidRecord_AddsRecordSuccessfully()
        {
            // Arrange
            string tableName = "TestTable";
            var columns = new Dictionary<string, string>
            {
                { "Id", "INTEGER PRIMARY KEY" },
                { "Name", "TEXT" }
            };

            _dictionaryHandler.CreateTable(tableName, columns, _connectionString);

            var record = new Dictionary<string, object>
            {
                { "Id", 1 },
                { "Name", "TestName" }
            };

            // Act
            _dictionaryHandler.AddRecord(tableName, record, _connectionString);

            // Assert
            var records = _dictionaryHandler.GetAllRecords(tableName, _connectionString).ToList();
            Assert.AreEqual(1, records.Count);
            Assert.AreEqual("TestName", records.First().Name);
        }

        [TestMethod]
        public void GetRecordById_ValidId_ReturnsRecord()
        {
            // Arrange
            string tableName = "TestTable";
            var columns = new Dictionary<string, string>
            {
                { "Id", "INTEGER PRIMARY KEY" },
                { "Name", "TEXT" }
            };

            _dictionaryHandler.CreateTable(tableName, columns, _connectionString);

            var record = new Dictionary<string, object>
            {
                { "Id", 1 },
                { "Name", "TestName" }
            };

            _dictionaryHandler.AddRecord(tableName, record, _connectionString);

            // Act
            var retrievedRecord = _dictionaryHandler.GetRecordById(tableName, 1, _connectionString);

            // Assert
            Assert.IsNotNull(retrievedRecord);
            Assert.AreEqual("TestName", retrievedRecord.Name);
        }

        [TestMethod]
        public void UpdateRecord_ValidUpdate_UpdatesRecordSuccessfully()
        {
            // Arrange
            string tableName = "TestTable";
            var columns = new Dictionary<string, string>
            {
                { "Id", "INTEGER PRIMARY KEY" },
                { "Name", "TEXT" }
            };

            _dictionaryHandler.CreateTable(tableName, columns, _connectionString);

            var record = new Dictionary<string, object>
            {
                { "Id", 1 },
                { "Name", "TestName" }
            };

            _dictionaryHandler.AddRecord(tableName, record, _connectionString);

            var updatedValues = new Dictionary<string, object>
            {
                { "Name", "UpdatedName" }
            };

            // Act
            _dictionaryHandler.UpdateRecord(tableName, 1, updatedValues, _connectionString);

            // Assert
            var retrievedRecord = _dictionaryHandler.GetRecordById(tableName, 1, _connectionString);
            Assert.IsNotNull(retrievedRecord);
            Assert.AreEqual("UpdatedName", retrievedRecord.Name);
        }

        [TestMethod]
        public void DeleteRecord_ValidId_DeletesRecordSuccessfully()
        {
            // Arrange
            string tableName = "TestTable";
            var columns = new Dictionary<string, string>
            {
                { "Id", "INTEGER PRIMARY KEY" },
                { "Name", "TEXT" }
            };

            _dictionaryHandler.CreateTable(tableName, columns, _connectionString);

            var record = new Dictionary<string, object>
            {
                { "Id", 1 },
                { "Name", "TestName" }
            };

            _dictionaryHandler.AddRecord(tableName, record, _connectionString);

            // Act
            _dictionaryHandler.DeleteRecord(tableName, 1, _connectionString);

            // Assert
            var retrievedRecord = _dictionaryHandler.GetRecordById(tableName, 1, _connectionString);
            Assert.IsNull(retrievedRecord);
        }
    }

}

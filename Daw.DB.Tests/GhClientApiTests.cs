using Daw.DB.Data.APIs;
using Daw.DB.Data.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Daw.DB.Tests {
    [TestClass]
    public class GhClientApiTests {
        private IGhClientApi _ghClientApi;
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

            _ghClientApi = serviceProvider.GetRequiredService<IGhClientApi>();
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
        public void DeleteTable_ShouldDeleteTableSuccessfully() {
            // Arrange
            string tableName = "TestTable";
            var columns = new Dictionary<string, string>
            {
                { "Name", "TEXT" }
            };

            _ghClientApi.CreateTable(tableName, columns);

            // Act
            _ghClientApi.DeleteTable(tableName);

            // Assert
            var allTableNames = _ghClientApi.GetTables();
            var tableExists = allTableNames.Contains(tableName);
            Assert.IsFalse(tableExists, "Table was not deleted successfully.");
        }

        [TestMethod]
        public void DeleteTable_ShouldThrowExceptionWhenTableDoesNotExist() {
            // Arrange
            string tableName = "NonExistingTable";

            // Act
            var result = _ghClientApi.DeleteTable(tableName);

            // Assert: Exception is expected
            // Error deleting table 'NonExistingTable': Table does not exist.
            Assert.IsTrue(result.Contains("Error deleting table"), "Exception message is incorrect.");

        }

        [TestMethod]
        public void DeleteTable_ShouldDeleteWithManyTables() {
            // Arrange 
            string tableName1 = "TestTable1";
            string tableName2 = "TestTable2";

            var columns = new Dictionary<string, string>
            {
                { "Name", "TEXT" }
            };

            _ghClientApi.CreateTable(tableName1, columns);
            _ghClientApi.CreateTable(tableName2, columns);

            // Act
            _ghClientApi.DeleteTable(tableName1);

            // Assert
            var allTableNames = _ghClientApi.GetTables();
            var tableExists = allTableNames.Contains(tableName1);
            Assert.IsFalse(tableExists, "Table was not deleted successfully.");

            var otherTableExists = allTableNames.Contains(tableName2);
            Assert.IsTrue(otherTableExists, "Other table was deleted.");
        }

        [TestMethod]
        public void AddDictionaryRecord_ShouldAddRecordSuccessfully() {
            // Arrange
            string tableName = "TestTable";
            var columns = new Dictionary<string, string>
            {
                { "Name", "TEXT" }
            };

            _ghClientApi.CreateTable(tableName, columns);

            var record = new Dictionary<string, object>
            {
                { "Name", "TestName" }
            };

            // Act
            _ghClientApi.AddDictionaryRecord(tableName, record);

            // Assert
            var allRecords = _ghClientApi.GetAllDictionaryRecords(tableName);
            Assert.AreEqual(1, allRecords.Count(), "Record was not added successfully.");

            Assert.AreEqual("TestName", allRecords.Select(x => x.Name).First(), "Record was not added with the correct value.");
        }

        [TestMethod]
        public void AddDictionaryRecordSingle_ShouldAddRecord() {
            // Arrange
            string tableName = "TestTable";
            var columns = new Dictionary<string, string>
            {
                { "Name", "TEXT" }
            };

            _ghClientApi.CreateTable(tableName, columns);

            var record = new Dictionary<string, object> { { "Name", "Record1" } };

            // Act
            _ghClientApi.AddDictionaryRecord(tableName, record);

            // Assert
            var allRecords = _ghClientApi.GetAllDictionaryRecords(tableName);
            Assert.AreEqual(1, allRecords.Count(), "Record was not added successfully.");

            Assert.AreEqual("Record1", allRecords.Select(x => x.Name).First(), "Record was not added with the correct value.");
        }

        [TestMethod]
        public void AddDictionaryRecordBatch_ShouldAddRecordsSuccessfully() {
            // Arrange
            string tableName = "TestTable";
            var columns = new Dictionary<string, string>
            {
                { "Name", "TEXT" }
            };

            _ghClientApi.CreateTable(tableName, columns);

            var records = new[]
            {
                new Dictionary<string, object> { { "Name", "Record1" } },
                new Dictionary<string, object> { { "Name", "Record2" } }
            };

            // Act
            _ghClientApi.AddDictionaryRecordBatch(tableName, records);

            // Assert
            var allRecords = _ghClientApi.GetAllDictionaryRecords(tableName);
            Assert.AreEqual(2, allRecords.Count(), "Records were not added successfully.");

            Assert.AreEqual("Record1", allRecords.Select(x => x.Name).First(), "First record was not added with the correct value.");
            Assert.AreEqual("Record2", allRecords.Select(x => x.Name).Last(), "Second record was not added with the correct value.");
        }

        [TestMethod]
        public void UpdateDictionaryRecord_ShouldUpdateRecordSuccessfully() {
            // Arrange
            string tableName = "TestTable";
            var columns = new Dictionary<string, string>
            {
                { "Name", "TEXT" }
            };

            _ghClientApi.CreateTable(tableName, columns);

            var record = new Dictionary<string, object>
            {
                { "Name", "TestName" }
            };

            _ghClientApi.AddDictionaryRecord(tableName, record);

            var updatedValues = new Dictionary<string, object>
            {
                { "Name", "UpdatedName" }
            };

            // Act
            _ghClientApi.UpdateDictionaryRecord(tableName, 1, updatedValues);

            // Assert
            var updatedRecord = _ghClientApi.GetDictionaryRecordById(tableName, 1);
            Assert.AreEqual("UpdatedName", updatedRecord.Name, "Record was not updated successfully.");
        }

        [TestMethod]
        public void DeleteRecord_ShouldDeleteRecordSuccessfully() {
            // Arrange
            string tableName = "TestTable";
            var columns = new Dictionary<string, string>
            {
                { "Name", "TEXT" }
            };

            _ghClientApi.CreateTable(tableName, columns);

            var record = new Dictionary<string, object>
            {
                { "Name", "TestName" }
            };

            _ghClientApi.AddDictionaryRecord(tableName, record);

            // Act
            _ghClientApi.DeleteRecord(tableName, 1);

            // Assert
            var deletedRecord = _ghClientApi.GetDictionaryRecordById(tableName, 1);
            Assert.IsNull(deletedRecord, "Record was not deleted successfully.");
        }

        [TestMethod]
        public void MultipleOperations_ShouldHandleCorrectly() {
            // Arrange
            string tableName = "TestTable";
            var columns = new Dictionary<string, string>
            {
                { "Name", "TEXT" }
            };

            _ghClientApi.CreateTable(tableName, columns);

            var record = new Dictionary<string, object>
            {
                { "Name", "TestName" }
            };

            var updatedValues = new Dictionary<string, object>
            {
                { "Name", "UpdatedName" }
            };

            // Act
            _ghClientApi.AddDictionaryRecord(tableName, record);
            _ghClientApi.UpdateDictionaryRecord(tableName, 1, updatedValues);
            _ghClientApi.DeleteRecord(tableName, 1);

            // Assert
            var allRecords = _ghClientApi.GetAllDictionaryRecords(tableName);
            Assert.AreEqual(0, allRecords.Count(), "Multiple operations were not handled correctly.");
        }

        [TestMethod]
        public void AddDictionaryRecordInTransaction_ShouldAddRecordsSuccessfully() {
            // Arrange
            string tableName = "TestTable";
            var columns = new Dictionary<string, string>
            {
                { "Name", "TEXT" }
            };

            _ghClientApi.CreateTable(tableName, columns);

            var records = new[]
            {
                new Dictionary<string, object> { { "Name", "Record1" } },
                new Dictionary<string, object> { { "Name", "Record2" } }
            };

            // Act
            _ghClientApi.AddDictionaryRecordInTransaction(tableName, records);

            // Assert
            var allRecords = _ghClientApi.GetAllDictionaryRecords(tableName);
            Assert.AreEqual(2, allRecords.Count(), "Records were not added successfully in transaction.");

            Assert.AreEqual("Record1", allRecords.Select(x => x.Name).First(), "First record was not added with the correct value.");
            Assert.AreEqual("Record2", allRecords.Select(x => x.Name).Last(), "Second record was not added with the correct value.");
        }

        [TestMethod]
        public void ReadOperations_ShouldRetrieveRecordsSuccessfully() {
            // Arrange
            string tableName = "TestTable";
            var columns = new Dictionary<string, string>
            {
                { "Name", "TEXT" }
            };

            _ghClientApi.CreateTable(tableName, columns);

            var record = new Dictionary<string, object>
            {
                { "Name", "TestName" }
            };

            _ghClientApi.AddDictionaryRecord(tableName, record);

            // Act
            var allRecords = _ghClientApi.GetAllDictionaryRecords(tableName);
            var singleRecord = _ghClientApi.GetDictionaryRecordById(tableName, 1);

            // Assert
            Assert.AreEqual(1, allRecords.Count(), "Failed to retrieve all records.");
            Assert.IsNotNull(singleRecord, "Failed to retrieve the record by ID.");
            Assert.AreEqual("TestName", singleRecord.Name, "The record was not retrieved with the correct value.");
        }
    }
}

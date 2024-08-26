using Daw.DB.Data.APIs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Daw.DB.Tests
{
    [TestClass]
    public class GhClientApiTests
    {
        private IGhClientApi _ghClientApi;
        private string _databaseFilePath;
        private string _connectionString;

        [TestInitialize]
        public void Setup()
        {
            // Configure the DI container and resolve the GhClientApi
            var serviceProvider = ServiceConfiguration.ConfigureServices();
            _ghClientApi = serviceProvider.GetRequiredService<IGhClientApi>();

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
        public void AddDictionaryRecord_ShouldAddRecordSuccessfully()
        {
            // Arrange
            string tableName = "TestTable";
            var columns = new Dictionary<string, string>
            {
                { "Name", "TEXT" }
            };

            _ghClientApi.CreateTable(tableName, columns, _connectionString);

            var record = new Dictionary<string, object>
            {
                { "Name", "TestName" }
            };

            // Act
            _ghClientApi.AddDictionaryRecord(tableName, record, _connectionString);

            // Assert
            var allRecords = _ghClientApi.GetAllDictionaryRecords(tableName, _connectionString);
            Assert.AreEqual(1, allRecords.Count(), "Record was not added successfully.");

            // use select in assert to get the record
            Assert.AreEqual("TestName", allRecords.Select(x => x.Name).First(), "Record was not added with the correct value.");

        }

        [TestMethod]
        public void UpdateDictionaryRecord_ShouldUpdateRecordSuccessfully()
        {
            // Arrange
            string tableName = "TestTable";
            var columns = new Dictionary<string, string>
            {
                { "Name", "TEXT" }
            };

            _ghClientApi.CreateTable(tableName, columns, _connectionString);

            var record = new Dictionary<string, object>
            {
                { "Name", "TestName" }
            };

            _ghClientApi.AddDictionaryRecord(tableName, record, _connectionString);

            var updatedValues = new Dictionary<string, object>
            {
                { "Name", "UpdatedName" }
            };

            // Act
            _ghClientApi.UpdateDictionaryRecord(tableName, 1, updatedValues, _connectionString);

            // Assert
            var updatedRecord = _ghClientApi.GetDictionaryRecordById(tableName, 1, _connectionString);
            Assert.AreEqual("UpdatedName", updatedRecord.Name, "Record was not updated successfully.");
        }

        [TestMethod]
        public void DeleteRecord_ShouldDeleteRecordSuccessfully()
        {
            // Arrange
            string tableName = "TestTable";
            var columns = new Dictionary<string, string>
            {
                { "Name", "TEXT" }
            };

            _ghClientApi.CreateTable(tableName, columns, _connectionString);

            var record = new Dictionary<string, object>
            {
                { "Name", "TestName" }
            };

            _ghClientApi.AddDictionaryRecord(tableName, record, _connectionString);

            // Act
            _ghClientApi.DeleteRecord(tableName, 1, _connectionString);

            // Assert
            var deletedRecord = _ghClientApi.GetDictionaryRecordById(tableName, 1, _connectionString);
            Assert.IsNull(deletedRecord, "Record was not deleted successfully.");
        }

        [TestMethod]
        public void MultipleOperations_ShouldHandleCorrectly()
        {
            // Arrange
            string tableName = "TestTable";
            var columns = new Dictionary<string, string>
            {
                { "Name", "TEXT" }
            };

            _ghClientApi.CreateTable(tableName, columns, _connectionString);

            var record = new Dictionary<string, object>
            {
                { "Name", "TestName" }
            };

            var updatedValues = new Dictionary<string, object>
            {
                { "Name", "UpdatedName" }
            };

            // Act
            _ghClientApi.AddDictionaryRecord(tableName, record, _connectionString);
            _ghClientApi.UpdateDictionaryRecord(tableName, 1, updatedValues, _connectionString);
            _ghClientApi.DeleteRecord(tableName, 1, _connectionString);

            // Assert
            var allRecords = _ghClientApi.GetAllDictionaryRecords(tableName, _connectionString);
            Assert.AreEqual(0, allRecords.Count(), "Multiple operations were not handled correctly.");
        }

        [TestMethod]
        public void AddDictionaryRecordInTransaction_ShouldAddRecordsSuccessfully()
        {
            // Arrange
            string tableName = "TestTable";
            var columns = new Dictionary<string, string>
            {
                { "Name", "TEXT" }
            };

            _ghClientApi.CreateTable(tableName, columns, _connectionString);

            var records = new[]
            {
                new Dictionary<string, object> { { "Name", "Record1" } },
                new Dictionary<string, object> { { "Name", "Record2" } }
            };

            // Act
            _ghClientApi.AddDictionaryRecordInTransaction(tableName, records, _connectionString);

            // Assert
            var allRecords = _ghClientApi.GetAllDictionaryRecords(tableName, _connectionString);
            Assert.AreEqual(2, allRecords.Count(), "Records were not added successfully in transaction.");

            // use select in assert to get the record
            Assert.AreEqual("Record1", allRecords.Select(x => x.Name).First(), "First record was not added with the correct value.");
            Assert.AreEqual("Record2", allRecords.Select(x => x.Name).Last(), "Second record was not added with the correct value.");

            // Assert.AreEqual("Record1", allRecords[0].Name, "First record was not added with the correct value.");
            // Assert.AreEqual("Record2", allRecords[1].Name, "Second record was not added with the correct value.");
        }

        [TestMethod]
        public void ReadOperations_ShouldRetrieveRecordsSuccessfully()
        {
            // Arrange
            string tableName = "TestTable";
            var columns = new Dictionary<string, string>
            {
                { "Name", "TEXT" }
            };

            _ghClientApi.CreateTable(tableName, columns, _connectionString);

            var record = new Dictionary<string, object>
            {
                { "Name", "TestName" }
            };

            _ghClientApi.AddDictionaryRecord(tableName, record, _connectionString);

            // Act
            var allRecords = _ghClientApi.GetAllDictionaryRecords(tableName, _connectionString);
            var singleRecord = _ghClientApi.GetDictionaryRecordById(tableName, 1, _connectionString);

            // Assert
            Assert.AreEqual(1, allRecords.Count(), "Failed to retrieve all records.");
            Assert.IsNotNull(singleRecord, "Failed to retrieve the record by ID.");
            Assert.AreEqual("TestName", singleRecord.Name, "The record was not retrieved with the correct value.");
        }
    }
}

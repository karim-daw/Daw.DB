﻿using Daw.DB.Data.APIs;
using Microsoft.Extensions.Configuration;
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
            // Load configuration from appsettings.json
            var configuration = new ConfigurationBuilder()
                // make sure to geT appsettings.json from the correct path which is in root folder
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Configure services based on the loaded configuration
            var serviceProvider = ServiceConfiguration.ConfigureServices(configuration);

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

        //Test delete table
        [TestMethod]
        public void DeleteTable_ShouldDeleteTableSuccessfully()
        {
            // Arrange
            string tableName = "TestTable";
            var columns = new Dictionary<string, string>
            {
                { "Name", "TEXT" }
            };

            _ghClientApi.CreateTable(tableName, columns, _connectionString);

            // Act
            _ghClientApi.DeleteTable(tableName, _connectionString);

            // Assert
            // check all table names and see if the table is deleted
            var allTableNames = _ghClientApi.GetTables(_connectionString);
            var tableExists = allTableNames.Contains(tableName);
            Assert.IsFalse(tableExists, "Table was not deleted successfully.");
        }

        [ExpectedException(typeof(Exception))]
        [TestMethod]
        public void DeleteTable_ShouldThrowExceptionWhenTableDoesNotExist()
        {
            // Arrange
            string tableName = "NonExistingTable";

            // Act
            _ghClientApi.DeleteTable(tableName, _connectionString);
        }

        // more tests for delete table
        [TestMethod]
        public void DeleteTable_ShouldDeleteWithManyTables()
        {
            // Arrange 
            string tableName1 = "TestTable1";
            string tableName2 = "TestTable2";

            var columns = new Dictionary<string, string>
            {
                { "Name", "TEXT" }
            };

            _ghClientApi.CreateTable(tableName1, columns, _connectionString);
            _ghClientApi.CreateTable(tableName2, columns, _connectionString);

            // Act
            _ghClientApi.DeleteTable(tableName1, _connectionString);

            // Assert
            var allTableNames = _ghClientApi.GetTables(_connectionString);
            var tableExists = allTableNames.Contains(tableName1);
            Assert.IsFalse(tableExists, "Table was not deleted successfully.");

            var otherTableExists = allTableNames.Contains(tableName2);
            Assert.IsTrue(otherTableExists, "Other table was deleted.");

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


        // add test for adding record batch with only one record
        [TestMethod]
        public void AddDictionaryRecordSingle_ShouldAddRecord()
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
                new Dictionary<string, object> { { "Name", "Record1" } }
            };

            // Act
            _ghClientApi.AddDictionaryRecord(tableName, records[0], _connectionString);

            // Assert
            var allRecords = _ghClientApi.GetAllDictionaryRecords(tableName, _connectionString);
            Assert.AreEqual(1, allRecords.Count(), "Record was not added successfully.");

            // use select in assert to get the record
            Assert.AreEqual("Record1", allRecords.Select(x => x.Name).First(), "Record was not added with the correct value.");

        }


        // test adding batch records
        [TestMethod]
        public void AddDictionaryRecordBatch_ShouldAddRecordsSuccessfully()
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
            _ghClientApi.AddDictionaryRecordBatch(tableName, records, _connectionString);

            // Assert
            var allRecords = _ghClientApi.GetAllDictionaryRecords(tableName, _connectionString);
            Assert.AreEqual(2, allRecords.Count(), "Records were not added successfully.");

            // use select in assert to get the record
            Assert.AreEqual("Record1", allRecords.Select(x => x.Name).First(), "First record was not added with the correct value.");
            Assert.AreEqual("Record2", allRecords.Select(x => x.Name).Last(), "Second record was not added with the correct value.");
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

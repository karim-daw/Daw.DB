using Daw.DB.Data.APIs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;

namespace Daw.DB.Tests
{
    [TestClass]
    public class EventfulGhClientApiTests
    {
        private IEventfulGhClientApi _eventfulGhClientApi;
        private string _databaseFilePath;
        private string _connectionString;
        private bool _eventRaised;

        [TestInitialize]
        public void Setup()
        {
            // Load configuration from appsettings.json
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Configure services based on the loaded configuration
            var serviceProvider = ServiceConfiguration.ConfigureServices(configuration);
            _eventfulGhClientApi = serviceProvider.GetRequiredService<IEventfulGhClientApi>();

            _databaseFilePath = Path.GetTempFileName();
            _connectionString = $"Data Source={_databaseFilePath};Version=3;";
            _eventRaised = false;
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
        public void AddDictionaryRecord_ShouldRaiseTableChangedEvent()
        {
            // Arrange
            string tableName = "TestTable";
            var columns = new Dictionary<string, string>
            {
                { "Name", "TEXT" }
            };

            _eventfulGhClientApi.CreateTable(tableName, columns, _connectionString);

            var record = new Dictionary<string, object>
            {
                { "Name", "TestName" }
            };

            _eventfulGhClientApi.SubscribeToTableChanges((sender, args) =>
            {
                if (args.TableName == tableName && args.Operation == "AddRecord")
                {
                    _eventRaised = true;
                }
            });

            // Act
            _eventfulGhClientApi.AddDictionaryRecord(tableName, record, _connectionString);

            // Assert
            Assert.IsTrue(_eventRaised, "The TableChanged event was not raised as expected.");
        }


        [TestMethod]
        public void UpdateDictionaryRecord_ShouldRaiseTableChangedEvent()
        {
            // Arrange
            string tableName = "TestTable";
            var columns = new Dictionary<string, string>
            {
                { "Name", "TEXT" }
            };

            _eventfulGhClientApi.CreateTable(tableName, columns, _connectionString);

            var record = new Dictionary<string, object>
            {
                { "Name", "TestName" }
            };

            _eventfulGhClientApi.AddDictionaryRecord(tableName, record, _connectionString);

            var updatedValues = new Dictionary<string, object>
            {
                { "Name", "UpdatedName" }
            };

            _eventfulGhClientApi.SubscribeToTableChanges((sender, args) =>
            {
                if (args.TableName == tableName && args.Operation == "UpdateRecord")
                {
                    _eventRaised = true;
                }
            });

            // Act
            _eventfulGhClientApi.UpdateDictionaryRecord(tableName, 1, updatedValues, _connectionString);

            // Assert
            Assert.IsTrue(_eventRaised, "The TableChanged event was not raised as expected.");
        }

        [TestMethod]
        public void DeleteRecord_ShouldRaiseTableChangedEvent()
        {
            // Arrange
            string tableName = "TestTable";
            var columns = new Dictionary<string, string>
            {
                { "Name", "TEXT" }
            };

            _eventfulGhClientApi.CreateTable(tableName, columns, _connectionString);

            var record = new Dictionary<string, object>
            {
                { "Name", "TestName" }
            };

            _eventfulGhClientApi.AddDictionaryRecord(tableName, record, _connectionString);

            _eventfulGhClientApi.SubscribeToTableChanges((sender, args) =>
            {
                if (args.TableName == tableName && args.Operation == "DeleteRecord")
                {
                    _eventRaised = true;
                }
            });

            // Act
            _eventfulGhClientApi.DeleteRecord(tableName, 1, _connectionString);

            // Assert
            Assert.IsTrue(_eventRaised, "The TableChanged event was not raised as expected.");
        }

        [TestMethod]
        public void MultipleOperations_ShouldRaiseCorrectEvents()
        {
            // Arrange
            string tableName = "TestTable";
            var columns = new Dictionary<string, string>
            {
                { "Name", "TEXT" }
            };

            _eventfulGhClientApi.CreateTable(tableName, columns, _connectionString);

            var record = new Dictionary<string, object>
            {
                { "Name", "TestName" }
            };

            var updatedValues = new Dictionary<string, object>
            {
                { "Name", "UpdatedName" }
            };

            bool addEventRaised = false;
            bool updateEventRaised = false;
            bool deleteEventRaised = false;

            _eventfulGhClientApi.SubscribeToTableChanges((sender, args) =>
            {
                if (args.TableName == tableName && args.Operation == "AddRecord")
                {
                    addEventRaised = true;
                }
                if (args.TableName == tableName && args.Operation == "UpdateRecord")
                {
                    updateEventRaised = true;
                }
                if (args.TableName == tableName && args.Operation == "DeleteRecord")
                {
                    deleteEventRaised = true;
                }
            });

            // Act
            _eventfulGhClientApi.AddDictionaryRecord(tableName, record, _connectionString);
            _eventfulGhClientApi.UpdateDictionaryRecord(tableName, 1, updatedValues, _connectionString);
            _eventfulGhClientApi.DeleteRecord(tableName, 1, _connectionString);

            // Assert
            Assert.IsTrue(addEventRaised, "The AddRecord event was not raised as expected.");
            Assert.IsTrue(updateEventRaised, "The UpdateRecord event was not raised as expected.");
            Assert.IsTrue(deleteEventRaised, "The DeleteRecord event was not raised as expected.");
        }

        [TestMethod]
        public void UnsubscribeFromTableChanges_ShouldNotRaiseEvents()
        {
            // Arrange
            string tableName = "TestTable";
            var columns = new Dictionary<string, string>
            {
                { "Name", "TEXT" }
            };

            _eventfulGhClientApi.CreateTable(tableName, columns, _connectionString);

            var record = new Dictionary<string, object>
            {
                { "Name", "TestName" }
            };

            void handler(object sender, TableChangedEventArgs args)
            {
                _eventRaised = true;
            }

            _eventfulGhClientApi.SubscribeToTableChanges(handler);

            // Act
            _eventfulGhClientApi.UnsubscribeFromTableChanges(handler);
            _eventfulGhClientApi.AddDictionaryRecord(tableName, record, _connectionString);

            // Assert
            Assert.IsFalse(_eventRaised, "The event was raised even after unsubscribing.");
        }

        [TestMethod]
        public void AddDictionaryRecordInTransaction_ShouldRaiseSingleEvent()
        {
            // Arrange
            string tableName = "TestTable";
            var columns = new Dictionary<string, string>
            {
                { "Name", "TEXT" }
            };

            _eventfulGhClientApi.CreateTable(tableName, columns, _connectionString);

            var records = new[]
            {
                new Dictionary<string, object> { { "Name", "Record1" } },
                new Dictionary<string, object> { { "Name", "Record2" } }
            };

            _eventfulGhClientApi.SubscribeToTableChanges((sender, args) =>
            {
                if (args.TableName == tableName && args.Operation == "AddRecordsInTransaction")
                {
                    _eventRaised = true;
                }
            });

            // Act
            _eventfulGhClientApi.AddDictionaryRecordInTransaction(tableName, records, _connectionString);

            // Assert
            Assert.IsTrue(_eventRaised, "The AddRecordsInTransaction event was not raised as expected.");
        }

        [TestMethod]
        public void ReadOperations_ShouldNotRaiseEvents()
        {
            // Arrange
            string tableName = "TestTable";
            var columns = new Dictionary<string, string>
            {
                { "Name", "TEXT" }
            };

            _eventfulGhClientApi.CreateTable(tableName, columns, _connectionString);

            var record = new Dictionary<string, object>
            {
                { "Name", "TestName" }
            };

            _eventfulGhClientApi.AddDictionaryRecord(tableName, record, _connectionString);

            _eventfulGhClientApi.SubscribeToTableChanges((sender, args) =>
            {
                _eventRaised = true;
            });

            // Act
            var allRecords = _eventfulGhClientApi.GetAllDictionaryRecords(tableName, _connectionString);
            var singleRecord = _eventfulGhClientApi.GetDictionaryRecordById(tableName, 1, _connectionString);

            // Assert
            Assert.IsFalse(_eventRaised, "An event was raised during a read operation.");
            Assert.IsNotNull(allRecords, "Failed to retrieve all records.");
            Assert.IsNotNull(singleRecord, "Failed to retrieve the record by ID.");
        }
    }
}

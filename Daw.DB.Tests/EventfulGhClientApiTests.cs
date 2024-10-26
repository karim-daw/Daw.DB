using Daw.DB.Data.APIs;
using Daw.DB.Data.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;

namespace Daw.DB.Tests {
    [TestClass]
    public class EventfulGhClientApiTests {
        private IEventfulGhClientApi _eventfulGhClientApi;
        private IDatabaseContext _databaseContext;
        private string _databaseFilePath;
        private bool _eventRaised;

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
            _eventfulGhClientApi = serviceProvider.GetRequiredService<IEventfulGhClientApi>();
            _databaseContext = serviceProvider.GetRequiredService<IDatabaseContext>();

            // Set up a temporary database file path
            _databaseFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.db");

            // Set the connection string in the database context
            _databaseContext.ConnectionString = $"Data Source={_databaseFilePath};Version=3;";
            _eventRaised = false;
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
        public void AddDictionaryRecord_ShouldRaiseTableChangedEvent() {
            // Arrange
            string tableName = "TestTable";
            var columns = new Dictionary<string, string>
            {
                { "Name", "TEXT" }
            };

            _eventfulGhClientApi.CreateTable(tableName, columns);

            var record = new Dictionary<string, object>
            {
                { "Name", "TestName" }
            };

            _eventfulGhClientApi.SubscribeToTableChanges(OnTableChanged);

            // Act
            _eventfulGhClientApi.AddDictionaryRecord(tableName, record);

            // Assert
            Assert.IsTrue(_eventRaised, "The TableChanged event was not raised as expected.");

            // Cleanup
            _eventfulGhClientApi.UnsubscribeFromTableChanges(OnTableChanged);
        }

        private void OnTableChanged(object sender, TableChangedEventArgs args) {
            if (args.TableName == "TestTable" && args.Operation == "AddRecord") {
                _eventRaised = true;
            }
        }

        [TestMethod]
        public void UpdateDictionaryRecord_ShouldRaiseTableChangedEvent() {
            // Arrange
            _eventRaised = false;
            string tableName = "TestTable";
            var columns = new Dictionary<string, string>
            {
                { "Name", "TEXT" }
            };

            _eventfulGhClientApi.CreateTable(tableName, columns);

            var record = new Dictionary<string, object>
            {
                { "Name", "TestName" }
            };

            _eventfulGhClientApi.AddDictionaryRecord(tableName, record);

            var updatedValues = new Dictionary<string, object>
            {
                { "Name", "UpdatedName" }
            };

            _eventfulGhClientApi.SubscribeToTableChanges(OnTableChangedUpdate);

            // Act
            _eventfulGhClientApi.UpdateDictionaryRecord(tableName, 1, updatedValues);

            // Assert
            Assert.IsTrue(_eventRaised, "The TableChanged event was not raised as expected.");

            // Cleanup
            _eventfulGhClientApi.UnsubscribeFromTableChanges(OnTableChangedUpdate);
        }

        private void OnTableChangedUpdate(object sender, TableChangedEventArgs args) {
            if (args.TableName == "TestTable" && args.Operation == "UpdateRecord") {
                _eventRaised = true;
            }
        }

        [TestMethod]
        public void DeleteRecord_ShouldRaiseTableChangedEvent() {
            // Arrange
            _eventRaised = false;
            string tableName = "TestTable";
            var columns = new Dictionary<string, string>
            {
                { "Name", "TEXT" }
            };

            _eventfulGhClientApi.CreateTable(tableName, columns);

            var record = new Dictionary<string, object>
            {
                { "Name", "TestName" }
            };

            _eventfulGhClientApi.AddDictionaryRecord(tableName, record);

            _eventfulGhClientApi.SubscribeToTableChanges(OnTableChangedDelete);

            // Act
            _eventfulGhClientApi.DeleteRecord(tableName, 1);

            // Assert
            Assert.IsTrue(_eventRaised, "The TableChanged event was not raised as expected.");

            // Cleanup
            _eventfulGhClientApi.UnsubscribeFromTableChanges(OnTableChangedDelete);
        }

        private void OnTableChangedDelete(object sender, TableChangedEventArgs args) {
            if (args.TableName == "TestTable" && args.Operation == "DeleteRecord") {
                _eventRaised = true;
            }
        }

        [TestMethod]
        public void MultipleOperations_ShouldRaiseCorrectEvents() {
            // Arrange
            string tableName = "TestTable";
            var columns = new Dictionary<string, string>
            {
                { "Name", "TEXT" }
            };

            bool addEventRaised = false;
            bool updateEventRaised = false;
            bool deleteEventRaised = false;

            _eventfulGhClientApi.CreateTable(tableName, columns);

            var record = new Dictionary<string, object>
            {
                { "Name", "TestName" }
            };

            var updatedValues = new Dictionary<string, object>
            {
                { "Name", "UpdatedName" }
            };

            void OnTableChangedMultiple(object sender, TableChangedEventArgs args) {
                if (args.TableName == tableName && args.Operation == "AddRecord") {
                    addEventRaised = true;
                }
                if (args.TableName == tableName && args.Operation == "UpdateRecord") {
                    updateEventRaised = true;
                }
                if (args.TableName == tableName && args.Operation == "DeleteRecord") {
                    deleteEventRaised = true;
                }
            }

            _eventfulGhClientApi.SubscribeToTableChanges(OnTableChangedMultiple);

            // Act
            _eventfulGhClientApi.AddDictionaryRecord(tableName, record);
            _eventfulGhClientApi.UpdateDictionaryRecord(tableName, 1, updatedValues);
            _eventfulGhClientApi.DeleteRecord(tableName, 1);

            // Assert
            Assert.IsTrue(addEventRaised, "The AddRecord event was not raised as expected.");
            Assert.IsTrue(updateEventRaised, "The UpdateRecord event was not raised as expected.");
            Assert.IsTrue(deleteEventRaised, "The DeleteRecord event was not raised as expected.");

            // Cleanup
            _eventfulGhClientApi.UnsubscribeFromTableChanges(OnTableChangedMultiple);
        }

        [TestMethod]
        public void UnsubscribeFromTableChanges_ShouldNotRaiseEvents() {
            // Arrange
            _eventRaised = false;
            string tableName = "TestTable";
            var columns = new Dictionary<string, string>
            {
                { "Name", "TEXT" }
            };

            _eventfulGhClientApi.CreateTable(tableName, columns);

            var record = new Dictionary<string, object>
            {
                { "Name", "TestName" }
            };

            void handler(object sender, TableChangedEventArgs args) {
                _eventRaised = true;
            }

            _eventfulGhClientApi.SubscribeToTableChanges(handler);

            // Act
            _eventfulGhClientApi.UnsubscribeFromTableChanges(handler);
            _eventfulGhClientApi.AddDictionaryRecord(tableName, record);

            // Assert
            Assert.IsFalse(_eventRaised, "The event was raised even after unsubscribing.");
        }

        [TestMethod]
        public void AddDictionaryRecordInTransaction_ShouldRaiseSingleEvent() {
            // Arrange
            _eventRaised = false;
            string tableName = "TestTable";
            var columns = new Dictionary<string, string>
            {
                { "Name", "TEXT" }
            };

            _eventfulGhClientApi.CreateTable(tableName, columns);

            var records = new[]
            {
                new Dictionary<string, object> { { "Name", "Record1" } },
                new Dictionary<string, object> { { "Name", "Record2" } }
            };

            _eventfulGhClientApi.SubscribeToTableChanges(OnTableChangedTransaction);

            // Act
            _eventfulGhClientApi.AddDictionaryRecordInTransaction(tableName, records);

            // Assert
            Assert.IsTrue(_eventRaised, "The AddRecordsInTransaction event was not raised as expected.");

            // Cleanup
            _eventfulGhClientApi.UnsubscribeFromTableChanges(OnTableChangedTransaction);
        }

        private void OnTableChangedTransaction(object sender, TableChangedEventArgs args) {
            if (args.TableName == "TestTable" && args.Operation == "AddRecordsInTransaction") {
                _eventRaised = true;
            }
        }

        [TestMethod]
        public void ReadOperations_ShouldNotRaiseEvents() {
            // Arrange
            _eventRaised = false;
            string tableName = "TestTable";
            var columns = new Dictionary<string, string>
            {
                { "Name", "TEXT" }
            };

            _eventfulGhClientApi.CreateTable(tableName, columns);

            var record = new Dictionary<string, object>
            {
                { "Name", "TestName" }
            };

            _eventfulGhClientApi.AddDictionaryRecord(tableName, record);

            _eventfulGhClientApi.SubscribeToTableChanges((sender, args) =>
            {
                _eventRaised = true;
            });

            // Act
            var allRecords = _eventfulGhClientApi.GetAllDictionaryRecords(tableName);
            var singleRecord = _eventfulGhClientApi.GetDictionaryRecordById(tableName, 1);

            // Assert
            Assert.IsFalse(_eventRaised, "An event was raised during a read operation.");
            Assert.IsNotNull(allRecords, "Failed to retrieve all records.");
            Assert.IsNotNull(singleRecord, "Failed to retrieve the record by ID.");

            // Cleanup
            _eventfulGhClientApi.UnsubscribeFromTableChanges((sender, args) =>
            {
                _eventRaised = true;
            });
        }
    }
}

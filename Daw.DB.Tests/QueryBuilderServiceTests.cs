using Daw.DB.Data.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Daw.DB.Tests {
    [TestClass]
    public class QueryBuilderServiceTests {
        private IQueryBuilderService _queryBuilderService;

        [TestInitialize]
        public void Setup() {
            _queryBuilderService = new QueryBuilderService();
        }

        [TestMethod]
        public void BuildCreateTableQuery_ShouldCreateCorrectQuery_WhenColumnsProvided() {
            // Arrange
            var tableName = "TestTable";
            var columns = new Dictionary<string, string>
            {
                { "Name", "TEXT" },
                { "Age", "INTEGER" }
            };

            // Act
            var result = _queryBuilderService.BuildCreateTableQuery(tableName, columns);

            // Assert
            var expectedQuery = "CREATE TABLE TestTable (Name TEXT, Age INTEGER, Id INTEGER PRIMARY KEY AUTOINCREMENT)";
            Assert.AreEqual(expectedQuery, result);
        }

        [TestMethod]
        public void BuildCreateTableQuery_ShouldAddIdColumn_WhenNotProvided() {
            // Arrange
            var tableName = "TestTable";
            var columns = new Dictionary<string, string>
            {
                { "Name", "TEXT" }
            };

            // Act
            var result = _queryBuilderService.BuildCreateTableQuery(tableName, columns);

            // Assert
            var expectedQuery = "CREATE TABLE TestTable (Name TEXT, Id INTEGER PRIMARY KEY AUTOINCREMENT)";
            Assert.AreEqual(expectedQuery, result);
        }

        [TestMethod]
        public void BuildCheckTableExistsQuery_ShouldReturnCorrectQuery() {
            // Arrange
            var tableName = "TestTable";

            // Act
            var result = _queryBuilderService.BuildCheckTableExistsQuery(tableName);

            // Assert
            var expectedQuery = "SELECT name FROM sqlite_master WHERE type='table' AND name='TestTable'";
            Assert.AreEqual(expectedQuery, result);
        }

        [TestMethod]
        public void BuildInsertQuery_ShouldCreateCorrectInsertQuery_WhenRecordProvided() {
            // Arrange
            var tableName = "TestTable";
            var record = new Dictionary<string, object>
            {
                { "Name", "John" },
                { "Age", 30 }
            };

            // Act
            var result = _queryBuilderService.BuildInsertQuery(tableName, record);

            // Assert
            var expectedQuery = "INSERT INTO TestTable (Name, Age) VALUES (@Name, @Age)";
            Assert.AreEqual(expectedQuery, result);
        }

        [TestMethod]
        public void BuildBatchInsertQuery_ShouldCreateCorrectBatchInsertQuery_WhenRecordsProvided() {
            // Arrange
            var tableName = "TestTable";
            var records = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object> { { "Name", "John" }, { "Age", 30 } },
                new Dictionary<string, object> { { "Name", "Jane" }, { "Age", 25 } }
            };

            // Act
            var (query, parameters) = _queryBuilderService.BuildBatchInsertQuery(tableName, records);

            // Assert
            var expectedQuery = "INSERT INTO TestTable (Name, Age) VALUES (@Name0, @Age0), (@Name1, @Age1);";
            Assert.AreEqual(expectedQuery, query);
            Assert.AreEqual(4, parameters.Count);
            Assert.AreEqual("John", parameters["Name0"]);
            Assert.AreEqual(30, parameters["Age0"]);
            Assert.AreEqual("Jane", parameters["Name1"]);
            Assert.AreEqual(25, parameters["Age1"]);
        }

        [TestMethod]
        public void BuildBatchInsertQuery_ShouldThrowException_WhenNoRecordsProvided() {
            // Arrange
            var tableName = "TestTable";
            var records = new List<Dictionary<string, object>>();

            // Act & Assert
            Assert.ThrowsException<ArgumentException>(() => _queryBuilderService.BuildBatchInsertQuery(tableName, records));
        }

        [TestMethod]
        public void BuildUpdateQuery_ShouldCreateCorrectUpdateQuery_WhenUpdatedValuesProvided() {
            // Arrange
            var tableName = "TestTable";
            var updatedValues = new Dictionary<string, object>
            {
                { "Name", "John" },
                { "Age", 35 }
            };

            // Act
            var result = _queryBuilderService.BuildUpdateQuery(tableName, updatedValues);

            // Assert
            var expectedQuery = "UPDATE TestTable SET Name = @Name, Age = @Age WHERE Id = @Id";
            Assert.AreEqual(expectedQuery, result);
        }

        [TestMethod]
        public void BuildSelectQuery_ShouldCreateCorrectSelectQuery_WhenNoWhereClauseProvided() {
            // Arrange
            var tableName = "TestTable";

            // Act
            var result = _queryBuilderService.BuildSelectQuery(tableName);

            // Assert
            var expectedQuery = "SELECT * FROM TestTable";
            Assert.AreEqual(expectedQuery, result);
        }

        [TestMethod]
        public void BuildSelectQuery_ShouldCreateCorrectSelectQuery_WhenWhereClauseProvided() {
            // Arrange
            var tableName = "TestTable";
            var whereClause = "Age > 30";

            // Act
            var result = _queryBuilderService.BuildSelectQuery(tableName, whereClause);

            // Assert
            var expectedQuery = "SELECT * FROM TestTable WHERE Age > 30";
            Assert.AreEqual(expectedQuery, result);
        }

        [TestMethod]
        public void BuildDeleteQuery_ShouldCreateCorrectDeleteQuery() {
            // Arrange
            var tableName = "TestTable";

            // Act
            var result = _queryBuilderService.BuildDeleteQuery(tableName);

            // Assert
            var expectedQuery = "DELETE FROM TestTable WHERE Id = @Id";
            Assert.AreEqual(expectedQuery, result);
        }

        [TestMethod]
        public void BuildDeleteTableQuery_ShouldCreateCorrectDeleteTableQuery() {
            // Arrange
            var tableName = "TestTable";

            // Act
            var result = _queryBuilderService.BuildDeleteTableQuery(tableName);

            // Assert
            var expectedQuery = "DROP TABLE IF EXISTS TestTable";
            Assert.AreEqual(expectedQuery, result);
        }

        [TestMethod]
        public void BuildGetAllTableNamesQuery_ShouldReturnCorrectQuery() {
            // Act
            var result = _queryBuilderService.BuildGetAllTableNamesQuery();

            // Assert
            var expectedQuery = "SELECT name FROM sqlite_master WHERE type='table'";
            Assert.AreEqual(expectedQuery, result);
        }

        [TestMethod]
        public void BuildUpdateQuery_ShouldThrowException_WhenNoUpdatedValuesProvided() {
            // Arrange
            var tableName = "TestTable";
            var updatedValues = new Dictionary<string, object>();

            // Assert
            Assert.ThrowsException<ArgumentException>(() => _queryBuilderService.BuildUpdateQuery(tableName, updatedValues));
        }

        [TestMethod]
        public void BuildDeleteQuery_ShouldThrowException_WhenTableNameIsNullOrEmpty() {
            // Arrange
            var tableName = "";

            // Act & Assert
            Assert.ThrowsException<ArgumentException>(() => _queryBuilderService.BuildDeleteQuery(tableName));
        }
    }
}
